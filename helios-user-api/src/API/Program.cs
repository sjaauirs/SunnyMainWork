using AutoMapper;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using SunnyRewards.Helios.Common.Core.Extensions;
using SunnyRewards.Helios.Common.Core.Extensions.AWSSecretsManager;
using SunnyRewards.Helios.Common.Core.Helpers;
using SunnyRewards.Helios.Common.Core.ServiceRegistry;
using SunnyRewards.Helios.User.Infrastructure.AWSConfig;
using SunnyRewards.Helios.User.Infrastructure.AWSConfig.Interface;
using SunnyRewards.Helios.User.Infrastructure.Helpers;
using SunnyRewards.Helios.User.Infrastructure.Helpers.Interface;
using SunnyRewards.Helios.User.Infrastructure.HttpClients;
using SunnyRewards.Helios.User.Infrastructure.HttpClients.Interfaces;
using SunnyRewards.Helios.User.Infrastructure.Mappings;
using SunnyRewards.Helios.User.Infrastructure.Mappings.MappingProfile;
using SunnyRewards.Helios.User.Infrastructure.Repositories;
using SunnyRewards.Helios.User.Infrastructure.Repositories.Interfaces;
using SunnyRewards.Helios.User.Infrastructure.ReadReplica;
using SunnyRewards.Helios.User.Infrastructure.Services;
using SunnyRewards.Helios.User.Infrastructure.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);
var configuration = new ConfigurationBuilder()
           .SetBasePath(Directory.GetCurrentDirectory())
           .AddJsonFile("appsettings.json")
           .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
           .AddAWSSecretsManager(builder.Environment.EnvironmentName)
           .Build();


builder.Services.AddSingleton<IConfiguration>(configuration);

builder.Host.AddSerilogLogging();
// vault setup for secrets access
builder.Services.InitVault();

// get instance of vault (unable to use IoC to get instance before builder.Build() is invoked)
var vault = RegisterServices.GetVault(configuration);

// DB ConnectionString
string srConnectionStr = await vault.GetSecret("SRConnectionString");

if (string.IsNullOrEmpty(srConnectionStr) || srConnectionStr == vault.InvalidSecret)
    throw new ArgumentNullException("SRConnectionString is null/empty or invalid");

// Add services to the container.
builder.Services.AddHealthChecks().AddCheck("Liveliness", () => HealthCheckResult.Healthy());
builder.Services.AddNhibernate<ConsumerMap>(srConnectionStr);

// Add NHibernate Read Replica for read-only operations (only if configured)
string? readReplicaConnectionStr = await vault.GetSecret("SRReadReplicaConnectionString");
if (string.IsNullOrEmpty(readReplicaConnectionStr) || readReplicaConnectionStr == vault.InvalidSecret)
    readReplicaConnectionStr = null;
builder.Services.AddNhibernateReadReplica<ConsumerMap>(readReplicaConnectionStr);

// Log database endpoints for verification
var primaryHost = ExtractHostFromConnectionString(srConnectionStr);
var replicaHost = readReplicaConnectionStr != null ? ExtractHostFromConnectionString(readReplicaConnectionStr) : "Not configured";
Console.WriteLine($"user-api: Primary DB Host: {primaryHost}");
Console.WriteLine($"user-api: Read Replica Host: {replicaHost}");
Console.WriteLine($"user-api: Read Replica configured: {!string.IsNullOrEmpty(readReplicaConnectionStr)}");

builder.Services.AddHttpContextAccessor();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();
Console.WriteLine("user-api: AddControllers"); //Logs

var mapperConfig = new MapperConfiguration(mc =>
{
    mc.AddProfile(new ConsumerMapping());
    mc.AddProfile(new ConsumerFlowProgressMapping());
    mc.AddProfile(new ConsumerLoginMapping());
    mc.AddProfile(new ConsumerHistoryMapping());
    mc.AddProfile(new PersonMapping());
    mc.AddProfile(new RoleMapping());
    mc.AddProfile(new PersonRoleMapping());
    mc.AddProfile(new ConsumerDeviceMapping());
    mc.AddProfile(new ConsumerActivityMapping());
    mc.AddProfile(new AddressTypeMapping());
    mc.AddProfile(new PersonAddressMapping());
    mc.AddProfile(new PhoneTypeMapping());
    mc.AddProfile(new PhoneNumberMapping());
    mc.AddProfile(new ConsumerETLMapping());
});
IMapper mapper = mapperConfig.CreateMapper();

// Register Dependencies to the DI Container
builder.Services.InitDependencies(x =>
{
    x.AddSingleton(mapper);
    x.AddSingleton<LivelinessService>();
    x.AddScoped<IConsumerRepo, ConsumerRepo>();
    x.AddScoped<IConsumerLoginRepo, ConsumerLoginRepo>();
    x.AddScoped<IPersonRepo, PersonRepo>();
    x.AddScoped<IRoleRepo, RoleRepo>();
    x.AddScoped<IPersonRoleRepo, PersonRoleRepo>();
    x.AddScoped<IServerLoginRepo, ServerLoginRepo>();
    x.AddScoped<IConsumerDeviceRepo, ConsumerDeviceRepo>();
    x.AddScoped<IPhoneTypeRepo, PhoneTypeRepo>();
    x.AddScoped<IPhoneNumberRepo, PhoneNumberRepo>();
    x.AddScoped<IConsumerService, ConsumerService>();
    x.AddScoped<IConsumerLoginService, ConsumerLoginService>();
    x.AddScoped<IPersonService, PersonService>();
    x.AddScoped<IZDService, ZDService>();
    x.AddScoped<IServerLoginService, ServerLoginService>();
    x.AddScoped<ITenantClient, TenantClient>();
    x.AddScoped<IConsumerDeviceService, ConsumerDeviceService>();
    x.AddScoped<IPersonRoleService, PersonRoleService>();
    x.AddScoped<IConsumerActivityService, ConsumerActivityService>();
    x.AddScoped<IConsumerActivityRepo, ConsumerActivityRepo>();
    x.AddScoped<IAdminLoginService, AdminLoginService>();
    x.AddScoped<IAddressTypeRepo, AddressTypeRepo>();
    x.AddScoped<IAddressTypeService, AddressTypeService>();
    x.AddScoped<IPersonAddressRepo, PersonAddressRepo>();
    x.AddScoped<IPersonAddressService, PersonAddressService>(); 
    x.AddScoped<IS3Helper, S3Helper>();
    x.AddScoped<IAwsConfiguration,AwsConfiguration>();
    x.AddScoped<IUploadAgreementPDFService, UploadAgreementPDFService>();
    x.AddScoped<IAmazonS3ClientService, AmazonS3ClientService>();
    x.AddScoped<IPhoneTypeService, PhoneTypeService>();
    x.AddScoped<IPhoneNumberService, PhoneNumberService>();
    x.AddScoped<IMemberImportFileDataRepo, MemberImportFileDataRepo>();
    x.AddSingleton<LogEnricher>();
    x.AddScoped<IAdminClient, AdminClient>();
    x.AddScoped<IEventService, EventService>();
    x.AddScoped<IConsumerHistoryRepo, ConsumerHistoryRepo>();
    x.AddScoped<IConsumerHistoryService, ConsumerHistoryService>();
    x.AddScoped<IConsumerFlowProgressRepo, ConsumerFlowProgressRepo>();
    x.AddScoped<IConsumerOnboardingProgressHistoryRepo, ConsumerOnboardingProgressHistoryRepo>();
    x.AddScoped<IConsumerFlowProgressService, ConsumerFlowProgressService>();
    x.AddScoped<IConsumerETLRepo, ConsumerETLRepo>();

});

// Default CORS Policy Added
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(
        policy =>
        {
            policy.WithOrigins("*");
            policy.AllowAnyOrigin();

        });
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "User API");
});

#region Publish API/DB metrics to CloudWatch
app.Services.SetServiceProvider();
app.UseCloudWatchMetricsMiddleware();
#endregion

//Configure API health check settings 
app.UseHealthChecks("/health/live", new HealthCheckOptions()
{
    Predicate = check => check.Name == "Liveliness"
});



// Default CORS Policy Applied
app.UseCors(x => x
        .AllowAnyOrigin()
        .AllowAnyMethod()
        .AllowAnyHeader());

app.UseAuthorization();

app.MapControllers();

var httpContextAccessor = app.Services.GetRequiredService<IHttpContextAccessor>();
HttpContextHelper.Initialize(httpContextAccessor);

// Log database configuration to CloudWatch
var startupLogger = app.Services.GetRequiredService<ILogger<Program>>();
startupLogger.LogInformation("user-api: Primary DB Host: {PrimaryHost}", primaryHost);
startupLogger.LogInformation("user-api: Read Replica Host: {ReplicaHost}", replicaHost);
startupLogger.LogInformation("user-api: Read Replica configured: {IsConfigured}", !string.IsNullOrEmpty(readReplicaConnectionStr));

Console.WriteLine("user-api: about to app.Run"); //Logs
app.Run();

// Helper function to extract host from PostgreSQL connection string
static string ExtractHostFromConnectionString(string connectionString)
{
    try
    {
        var parts = connectionString.Split(';');
        foreach (var part in parts)
        {
            var keyValue = part.Split('=');
            if (keyValue.Length == 2)
            {
                var key = keyValue[0].Trim().ToLower();
                if (key == "host" || key == "server")
                {
                    return keyValue[1].Trim();
                }
            }
        }
        return "Unknown";
    }
    catch
    {
        return "Unable to parse";
    }
}
