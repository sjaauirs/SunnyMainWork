using Amazon;
using Amazon.SecretsManager;
using Amazon.SimpleSystemsManagement;
using AutoMapper;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Sunny.Benefits.Bff.Api.Filters;
using Sunny.Benefits.Bff.Api.Middlewares;
using Sunny.Benefits.Bff.Core.Constants;
using Sunny.Benefits.Bff.Infrastructure.Helpers;
using Sunny.Benefits.Bff.Infrastructure.Helpers.Interface;
using Sunny.Benefits.Bff.Infrastructure.HttpClients;
using Sunny.Benefits.Bff.Infrastructure.HttpClients.Interfaces;
using Sunny.Benefits.Bff.Infrastructure.Mappings.MappingProfile;
using Sunny.Benefits.Bff.Infrastructure.Repositories;
using Sunny.Benefits.Bff.Infrastructure.Repositories.Interfaces;
using Sunny.Benefits.Bff.Infrastructure.Services;
using Sunny.Benefits.Bff.Infrastructure.Services.Interfaces;
using IJwtValidationCacheService = Sunny.Benefits.Bff.Infrastructure.Services.IJwtValidationCacheService;
using JwtValidationCacheService = Sunny.Benefits.Bff.Infrastructure.Services.JwtValidationCacheService;
using SunnyRewards.Helios.Common.Core.Domain;
using SunnyRewards.Helios.Common.Core.Extensions;
using SunnyRewards.Helios.Common.Core.Extensions.AWSSecretsManager;
using SunnyRewards.Helios.Common.Core.Helpers;
using SunnyRewards.Helios.Common.Core.Middlewere;
using SunnyRewards.Helios.Common.Core.ServiceRegistry;
using SunnyRewards.Helios.Tenant.Core.Domain.Dtos.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using StackExchange.Redis;
using Sunny.Benefits.Bff.Infrastructure.Configuration;
using Microsoft.Extensions.Caching.Memory;
using System.Net.Http;


var builder = WebApplication.CreateBuilder(args);
var configuration = new ConfigurationBuilder()
           .SetBasePath(Directory.GetCurrentDirectory())
           .AddJsonFile("appsettings.json")
           .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
           .AddAWSSecretsManager(builder.Environment.EnvironmentName)
           .Build();

builder.Services.AddSingleton<IConfiguration>(configuration);

// Register AWS SSM client for Parameter Store access
var region = configuration.GetValue<string>("AWS:Region") ?? "us-east-2";
var ssmClient = new AmazonSimpleSystemsManagementClient(RegionEndpoint.GetBySystemName(region));
builder.Services.AddSingleton<IAmazonSimpleSystemsManagement>(ssmClient);

// Register SSM Parameter Store Helper
builder.Services.AddSingleton<IParameterStoreHelper, ParameterStoreHelper>();

// Redis Configuration
builder.Services.Configure<RedisConfiguration>(configuration.GetSection("Redis"));

// Read cache flags from SSM Parameter Store (always, regardless of environment)
var rawEnvironment = builder.Environment.EnvironmentName;

// Map environment name for SSM parameter path
var environment = rawEnvironment.ToLowerInvariant() switch
{
    "development" => "dev",
    "production" => "prod",
    "integration" or "integ" => "integ",
    _ => rawEnvironment.ToLowerInvariant() // Use as-is for qa, uat, newdev, sbx, etc.
};

Console.WriteLine($"Reading cache flags from SSM Parameter Store (environment: {environment})");

var loggerFactory = LoggerFactory.Create(b => b.AddConsole());
var ssmLogger = loggerFactory.CreateLogger<ParameterStoreHelper>();
var parameterStoreHelper = new ParameterStoreHelper(ssmClient, ssmLogger);

async Task<(bool value, string source)> ReadBoolFlagFromSsmWithFallbackAsync(string parameterSuffix, string appsettingsKey, bool defaultValue)
{
    var fullParameterName = $"/cache-flags/benefits-bff-api/{environment}/{parameterSuffix}";
    
    try
    {
        var rawValue = await parameterStoreHelper.GetRawValueAsync(fullParameterName, withDecryption: false);

        if (string.IsNullOrWhiteSpace(rawValue))
        {
            Console.WriteLine($"⚠️ SSM parameter read FAILED: {fullParameterName} (parameter not found or empty), falling back to appsettings: {appsettingsKey}");
            // Fallback to appsettings
            var fallbackValue = configuration.GetValue<bool>(appsettingsKey, defaultValue);
            return (fallbackValue, "appsettings (SSM not found)");
        }

        if (bool.TryParse(rawValue, out var parsedValue))
        {
            Console.WriteLine($"✅ SSM parameter read SUCCESS: {fullParameterName} = {parsedValue}");
            return (parsedValue, "SSM");
        }

        Console.WriteLine($"⚠️ SSM parameter read FAILED: {fullParameterName} (unable to parse value '{rawValue}'), falling back to appsettings: {appsettingsKey}");
        // Fallback to appsettings
        var fallbackValue2 = configuration.GetValue<bool>(appsettingsKey, defaultValue);
        return (fallbackValue2, "appsettings (SSM parse error)");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ SSM parameter read ERROR: {fullParameterName} (exception: {ex.Message}), falling back to appsettings: {appsettingsKey}");
        // Fallback to appsettings on any exception
        var fallbackValue = configuration.GetValue<bool>(appsettingsKey, defaultValue);
        return (fallbackValue, "appsettings (SSM error)");
    }
}

var (cacheEnabledForAuth0, auth0Source) = await ReadBoolFlagFromSsmWithFallbackAsync("cache-enabled-for-auth0", "Redis:CacheEnabledForAuth0", false);
var (cacheEnabledForApi, apiSource) = await ReadBoolFlagFromSsmWithFallbackAsync("cache-enabled-for-api", "Redis:CacheEnabledForApi", false);

var anyCacheEnabled = cacheEnabledForAuth0 || cacheEnabledForApi;

Console.WriteLine("📊 CACHE CONFIGURATION:");
Console.WriteLine($"   CacheEnabledForAuth0: {cacheEnabledForAuth0} (source: {auth0Source})");
Console.WriteLine($"   CacheEnabledForApi: {cacheEnabledForApi} (source: {apiSource})");

// Only register Redis services if any caching is enabled
if (anyCacheEnabled)
{
    // Redis Services
    builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
    {
        var redisConfig = configuration.GetSection("Redis").Get<RedisConfiguration>();
        return ConnectionMultiplexer.Connect(redisConfig?.ConnectionString ?? "localhost:6379");
    });
    
    builder.Services.AddStackExchangeRedisCache(options =>
    {
        var redisConfig = configuration.GetSection("Redis").Get<RedisConfiguration>();
        options.Configuration = redisConfig?.ConnectionString ?? "localhost:6379";
        options.InstanceName = redisConfig?.InstanceName ?? "BENEFITS_BFF";
    });
    
    // Update RedisConfiguration with SSM values for runtime access
    builder.Services.Configure<RedisConfiguration>(options =>
    {
        var redisConfig = configuration.GetSection("Redis").Get<RedisConfiguration>();
        options.ConnectionString = redisConfig?.ConnectionString ?? "localhost:6379";
        options.InstanceName = redisConfig?.InstanceName ?? "BENEFITS_BFF";
        options.DefaultExpirationMinutes = redisConfig?.DefaultExpirationMinutes ?? 30;
        options.CacheEnabledForAuth0 = cacheEnabledForAuth0;
        options.CacheEnabledForApi = cacheEnabledForApi;
    });
    
    // Register JWT validation cache service (only if Auth0 caching is enabled)
    if (cacheEnabledForAuth0)
    {
        builder.Services.AddSingleton<IJwtValidationCacheService, JwtValidationCacheService>();
        Console.WriteLine("JWT validation caching enabled - Auth0 calls will be cached in Redis");
    }
}

builder.Host.AddSerilogLogging();

//Add the line to disable the server header
builder.Services.Configure<KestrelServerOptions>(options =>
{
    options.AddServerHeader = false;
});

// vault setup for secrets access
builder.Services.InitVault();

// get instance of vault (unable to use IoC to get instance before builder.Build() is invoked)
var vault = RegisterServices.GetVault(configuration);

// jwt secretkey
string jwtSecretKey = await vault.GetSecret("JWT_SECRET_KEY");

if (string.IsNullOrEmpty(jwtSecretKey) || jwtSecretKey == vault.InvalidSecret)
    throw new ArgumentNullException(nameof(jwtSecretKey), "SecretKey cannot be null or empty");

// Add services to the container
var healthChecksBuilder = builder.Services.AddHealthChecks().AddCheck("Liveliness", () => HealthCheckResult.Healthy());

//Redis health check if any caching is enabled
if (anyCacheEnabled)
{
    var redisConfig = configuration.GetSection("Redis").Get<RedisConfiguration>();
    healthChecksBuilder.AddRedis(redisConfig?.ConnectionString ?? "localhost:6379", name: "redis");
}
builder.Services.AddHttpContextAccessor();

// Add HttpClient factory for proper HttpClient lifecycle management
builder.Services.AddHttpClient();

// Add memory cache for JWKS caching (1-hour TTL)
builder.Services.AddMemoryCache();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();
Console.WriteLine("benefits-api: AddControllers"); //Logs

var mapperConfig = new MapperConfiguration(mc =>
{
    mc.AddProfile(new ConsumerMapping());
});
IMapper mapper = mapperConfig.CreateMapper();
// Register services to the DI container.
builder.Services.InitDependencies(x =>
{
    x.AddSingleton(mapper);
    x.AddScoped<ILoginService, LoginService>();
    x.AddTransient<IUserClient, UserClient>();
    x.AddSingleton<IAuth0TokenCacheService, Auth0TokenCacheService>();
    x.AddScoped<IAuth0Helper, Auth0Helper>();
    x.AddScoped<IWalletService, WalletService>();
    x.AddScoped<IWalletCategoryService, WalletCategoryService>();
    x.AddScoped<IWalletClient, WalletClient>();
    x.AddScoped<ITaskClient, TaskClient>();
    x.AddScoped<ICmsClient, CmsClient>();
    x.AddScoped<IFisClient, FisClient>();
    x.AddScoped<ICmsService, CmsService>();
    x.AddScoped<IAdminClient, AdminClient>();
    x.AddScoped<ICohortClient, CohortClient>();
    x.AddScoped<INotificationClient, NotificationClient>();
    x.AddScoped<IProductService, ProductService>();
    x.AddScoped<IStoreSearchService, StoreSearchService>();
    x.AddScoped<ITenantService, TenantService>();
    x.AddScoped<ITenantClient, TenantClient>();
    x.AddScoped<IValidicClient, ValidicClient>();
    x.AddScoped<ICardOperationService, CardOperationService>();
    x.AddScoped<IReplaceCardService, ReplaceCardService>();
    x.AddScoped<ICardReissueService, CardReissueService>();
    x.AddScoped<ICardOperationsHelper, CardOperationsHelper>();
    x.AddScoped<IPersonHelper, PersonHelper>();
    x.AddScoped<IConsumerSummaryService, ConsumerSummaryService>();
    x.AddScoped<ITaskService, TaskService>();
    x.AddScoped<IFundTransferService, FundTransferService>();
    x.AddScoped<ITenantAccountService, TenantAccountService>();
    x.AddScoped<IConsumerAccountService, ConsumerAccountService>();
    x.AddScoped<ValidatePersonOnboardingStateAttribute>();
    x.AddScoped<IEventService, EventService>();
    x.AddScoped<IConsumerService, ConsumerService>();
    x.AddScoped<IImageSearchService, ImageSearchService>();
    x.AddScoped<IConsumerActivityService, ConsumerActivityService>();
    x.AddScoped<INotificationService, NotificationService>();
    x.AddScoped<IPersonAddressService, PersonAddressService>();
    x.AddScoped<IPersonService, PersonService>();
    x.AddScoped<IValidicService, ValidicService>();
    x.AddScoped<IPhoneNumberService, PhoneNumberService>();
    x.AddSingleton<LogEnricher>();
    x.AddScoped<ICommonHelper, CommonHelper>();
    x.AddScoped<INotificationHelper, NotificationHelper>();
    x.AddScoped<ValidateLanguageCodeAttribute>();
    x.AddScoped<IFisNotificationEnrollmentService, FisNotificationEnrollmentService>();
    x.AddScoped<ICohortConsumerService, CohortConsumerService>();
    x.AddScoped<IConsumerFlowProgressService, ConsumerFlowProgressService>();
    x.AddScoped<IFlowStepService, FlowStepService>();
    x.AddScoped<IDynamicQueryProcessor, DynamicQueryProcessor>();
    x.AddScoped<IFlowStepProcessor, FlowStepProcessor>();
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin",
        policy =>
        {
            policy.WithOrigins("https://benefits-app.dev.sunnyrewards.com",
                "https://benefits-app.dev.sunnyrewards.mobi",
                "https://benefits-app.qa.sunnyrewards.com",
                "https://benefits-app.uat.sunnyrewards.com",
                "https://benefits-app.integ.sunnyrewards.com",
                "https://benefits-app.sunnyrewards.com",
                "http://localhost:3000",
                "https://app.dev.sunnyrewards.com",
                "https://app.dev.sunnyrewards.mobi",
                "https://app.qa.sunnyrewards.com",
                "https://app.uat.sunnyrewards.com",
                "https://app.integ.sunnyrewards.com",
                "https://app.sunnyrewards.com",
                "https://demo.dev.sunnyhealthplan.com",
                "https://demo.dev.sunnyhealthplan.mobi",
                "https://demo.qa.sunnyhealthplan.com",
                "https://demo.uat.sunnyhealthplan.com",
                "https://demo.integ.sunnyhealthplan.com",
                "https://demo.sunnyhealthplan.com",
                "http://localhost:19006",
                "https://app.dev.sunnybenefits.com",
                "https://app.dev.sunnybenefits.mobi",
                "https://app.qa.sunnybenefits.com",
                "https://app.uat.sunnybenefits.com",
                "https://app.integ.sunnybenefits.com",
                "https://app.sunnybenefits.com",
                "https://hap.integ.sunnybenefits.com",
                "https://hap.sunnybenefits.com",
                "https://kp.dev.sunnybenefits.com",
                "https://kp.qa.sunnybenefits.com",
                "https://kp.uat.sunnybenefits.com",
                "https://kp.integ.sunnybenefits.com",
                "https://kp.sunnybenefits.com",
                "https://kp-espanol.sunnybenefits.com",
                "https://kp-espanol.dev.sunnybenefits.com",
                "https://kp-espanol.qa.sunnybenefits.com",
                "https://kp-espanol.integ.sunnybenefits.com",
                "https://kp-espanol.uat.sunnybenefits.com",
                "http://localhost:8081",
                "https://app.sbx.sunnybenefits.com",
                "https://dfkyz377c5cqo.cloudfront.net",
                "https://app.dev01.sunnybenefits.com",
                "https://kp.dev01.sunnybenefits.com",
                "https://kp-espanol.dev01.sunnybenefits.com"
                 );
            policy.AllowAnyHeader()
            .AllowAnyMethod();
        });
});


var isProduction = builder.Environment.IsProduction();

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = Constants.AuthenticationScheme; // Use our policy scheme to route dynamically
})
.AddPolicyScheme(Constants.AuthenticationScheme, Constants.AuthenticationScheme, options =>
{
    options.ForwardDefaultSelector = context =>
    {
        var headers = context.Request.Headers;

        if (headers.ContainsKey(HttpHeaderNames.XAPIKey))
            return Constants.AuthentcateUsingAPIkey;

        if (!isProduction && headers.ContainsKey(HttpHeaderNames.Authint))
            return Constants.AuthenticateUsingInternalToken;

        return JwtBearerDefaults.AuthenticationScheme;
    };
})
// Default Scheme- OAuth
.AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, jwtOptions =>
{
    jwtOptions.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = false,
        SignatureValidator = (token, parameters) =>
        {
            // Just parse the token, skip validation here
            return new JwtSecurityTokenHandler().ReadJwtToken(token);
        }
    };

    jwtOptions.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var token = context.Request.Headers["Authorization"].FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(token))
                context.Token = token.Replace("Bearer ", "", StringComparison.OrdinalIgnoreCase);
            return Task.CompletedTask;
        },

        OnTokenValidated = async context =>
        {
            try
            {
                var httpContext = context.HttpContext;
                var config = httpContext.RequestServices.GetRequiredService<IConfiguration>();

                string issuer = null;
                string[] audience = null;
                string jwksUrl = null;

                if (httpContext.Items.TryGetValue("AuthConfig", out var configObj) &&
                    configObj is AuthConfig authConfig)
                {
                    issuer = authConfig.Auth0?.Issuer;
                    audience = authConfig.Auth0?.Audience;
                    jwksUrl = authConfig.Auth0?.JwksUrl;
                }

                if (string.IsNullOrWhiteSpace(issuer))
                {
                    issuer = config["Auth0:Issuer"] ?? string.Empty;
                }
                if (string.IsNullOrWhiteSpace(jwksUrl))
                {
                    jwksUrl = configuration.GetSection("Auth0:JwksUrl").Value ?? string.Empty;
                }

                if (audience == null || !audience.Any())
                {
                    audience = config.GetSection("Auth0:Audiences").Get<string[]>() ?? Array.Empty<string>();
                }

                if (string.IsNullOrWhiteSpace(issuer) || audience == null || !audience.Any())
                    throw new SecurityTokenValidationException("Missing issuer or audience configuration.");

                // Cache JWKS keys with 1-hour TTL to avoid fetching on every request
                var memoryCache = httpContext.RequestServices.GetRequiredService<IMemoryCache>();
                var cacheKey = $"jwks_{jwksUrl}";
                const int cacheTTLHours = 1;
                
                if (!memoryCache.TryGetValue(cacheKey, out JsonWebKeySet jwks))
                {
                    // Use IHttpClientFactory for proper HttpClient lifecycle management
                    var httpClientFactory = httpContext.RequestServices.GetRequiredService<IHttpClientFactory>();
                    using var httpClient = httpClientFactory.CreateClient();
                    
                    var jwksJson = await httpClient.GetStringAsync(jwksUrl);
                    jwks = new JsonWebKeySet(jwksJson);
                    
                    // Cache JWKS for 1 hour
                    var cacheOptions = new MemoryCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(cacheTTLHours),
                        SlidingExpiration = null // Use absolute expiration only
                    };
                    memoryCache.Set(cacheKey, jwks, cacheOptions);
                }

                if (context.SecurityToken is not JwtSecurityToken jwtToken)
                    throw new SecurityTokenValidationException("Invalid token type");

                var tokenString = jwtToken.RawData;
                var handler = new JwtSecurityTokenHandler();

                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = issuer,
                    ValidateAudience = true,
                    ValidAudiences = audience,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKeys = jwks.Keys
                };

                handler.ValidateToken(tokenString, validationParameters, out _);
            }
            catch (Exception ex)
            {
                context.Fail($"Token validation failed: {ex.Message}");
                throw new SecurityTokenValidationException("Token validation failed.");
            }
        },
        OnAuthenticationFailed = context =>
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.ContentType = "application/json";
            context.Response.WriteAsync("{\"error\": \"Unauthorized\"}");
            return Task.CompletedTask;
        }
    };
})
// 2nd token Scheme - AuthintScheme
.AddJwtBearer(Constants.AuthenticateUsingInternalToken, jwtOptions =>
{
    jwtOptions.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = JwtSettings.Issuer,
        ValidAudience = JwtSettings.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecretKey))
    };

    jwtOptions.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var token = context.Request.Headers[HttpHeaderNames.Authint].FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(token))
                context.Token = token.Replace("Bearer ", "", StringComparison.OrdinalIgnoreCase);
            return Task.CompletedTask;
        }
    };
})
//Add 3rd for XAPI 
.AddScheme<AuthenticationSchemeOptions, ApiKeyAuthenticationHandler>
     (Constants.AuthentcateUsingAPIkey, options => {});

builder.Services.AddSwaggerGen(opt =>
{
    opt.SwaggerDoc("v1", new OpenApiInfo { Title = "Sunny.Benefits.Bff.Api", Version = "v1" });

    // Security Definition for Authorization Header (Auth0)
    opt.AddSecurityDefinition("Auth0Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Enter Auth0 or Authint token in the format 'Bearer {token}'",
        Name = HttpHeaderNames.Authorization, // Main header
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "bearer"
    });

    if (!isProduction)
    {
        opt.AddSecurityDefinition("CustomBearer", new OpenApiSecurityScheme
        {
            In = ParameterLocation.Header,
            Description = "Enter Authint token in the format 'Bearer {token}'",
            Name = HttpHeaderNames.Authint, // Custom header for non-prod
            Type = SecuritySchemeType.ApiKey,
            BearerFormat = "JWT",
            Scheme = "bearer"
        });
    }

    opt.AddSecurityDefinition("XApiKey", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Enter your API key in 'X-API-Key' header",
        Name = HttpHeaderNames.XAPIKey,
        Type = SecuritySchemeType.ApiKey
    });

    // Security Requirement
    opt.AddSecurityRequirement(new OpenApiSecurityRequirement
{
    {
        new OpenApiSecurityScheme
        {
            Reference = new OpenApiReference
            {
                Type = ReferenceType.SecurityScheme,
                Id = "Auth0Bearer"
            }
        },
        Array.Empty<string>()
    }
});

    if (!isProduction)
    {
        opt.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "CustomBearer"
                    }
                },
                Array.Empty<string>()
            }
        });
    }

    opt.AddSecurityRequirement(new OpenApiSecurityRequirement
{
    {
        new OpenApiSecurityScheme
        {
            Reference = new OpenApiReference
            {
                Type = ReferenceType.SecurityScheme,
                Id = "XApiKey"
            }
        },
        Array.Empty<string>()
    }
});

});

// Middleware
var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "BENEFITS API");
});

#region Publish API/DB metrics to CloudWatch
app.Services.SetServiceProvider();
app.UseCloudWatchMetricsMiddleware();
#endregion

var httpContextAccessor = app.Services.GetRequiredService<IHttpContextAccessor>();
HttpContextHelper.Initialize(httpContextAccessor);

// Configure API Health Check Settings 
app.UseHealthChecks("/health/live", new HealthCheckOptions()
{
    Predicate = check => check.Name == "Liveliness"
});

// Default CORS Policy Applied
app.UseCors("AllowSpecificOrigin");

app.UseHttpsRedirection();
if (!isProduction)
{
    app.UseMiddleware<AuthIntMiddleware>();
}
app.UseMiddleware<Auth0Middleware>();
app.UseMiddleware<HeaderEnrichmentMiddleware>();
app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

Console.WriteLine("benefits-api: about to app.Run"); //Logs
app.Run();
