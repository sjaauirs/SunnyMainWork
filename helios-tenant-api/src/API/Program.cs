using AutoMapper;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using SunnyRewards.Helios.Common.Core.Extensions;
using SunnyRewards.Helios.Common.Core.Extensions.AWSSecretsManager;
using SunnyRewards.Helios.Common.Core.Helpers;
using SunnyRewards.Helios.Common.Core.ServiceRegistry;
using SunnyRewards.Helios.Tenant.Infrastructure.Mappings;
using SunnyRewards.Helios.Tenant.Infrastructure.Mappings.MappingProfile;
using SunnyRewards.Helios.Tenant.Infrastructure.Repositories;
using SunnyRewards.Helios.Tenant.Infrastructure.Repositories.Interfaces;
using SunnyRewards.Helios.Tenant.Infrastructure.Services;
using SunnyRewards.Helios.Tenant.Infrastructure.Services.Helpers;
using SunnyRewards.Helios.Tenant.Infrastructure.Services.Interfaces;

try
{
    var builder = WebApplication.CreateBuilder(args);
    var configuration = new ConfigurationBuilder()
               .SetBasePath(Directory.GetCurrentDirectory())
               .AddJsonFile("appsettings.json")
               .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
               .AddAWSSecretsManager(builder.Environment.EnvironmentName)
               .Build();
    builder.Services.AddSingleton<IConfiguration>(configuration);

    // enable AWS CloudWatch logging
    builder.Host.AddSerilogLogging();

    // vault setup for secrets access
    builder.Services.InitVault();

    // get instance of vault (unable to use IoC to get instance before builder.Build() is invoked)
    var vault = RegisterServices.GetVault(configuration);

    // DB ConnectionString
    string srConnectionStr = await vault.GetSecret("SRConnectionString");
    if (string.IsNullOrEmpty(srConnectionStr) || srConnectionStr == vault.InvalidSecret)
        throw new ArgumentNullException(nameof(srConnectionStr), "SRConnectionString cannot be null or empty");

    // Add services to the container.
    builder.Services.AddHealthChecks().AddCheck("Liveliness", () => HealthCheckResult.Healthy());
    builder.Services.AddNhibernate<TenantMap>(srConnectionStr, "tenant");
    builder.Services.AddHttpContextAccessor();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();
    builder.Services.AddControllers();
    Console.WriteLine("tenant-api: AddControllers"); //Logs

    var mapperConfig = new MapperConfiguration(mc =>
    {
        mc.AddProfile(new TenantMapping());
        mc.AddProfile(new CustomerMapping());
        mc.AddProfile(new SponsorMapping());
        mc.AddProfile(new WalletCategoryMapping());

    });
    IMapper mapper = mapperConfig.CreateMapper();

    // Register Dependencies to the DI Container
    builder.Services.InitDependencies(x =>
    {
        x.AddSingleton(mapper);
        x.AddSingleton<LivelinessService>();
        x.AddScoped<ITenantRepo, TenantRepo>();
        x.AddScoped<ITenantService, TenantService>();
        x.AddScoped<ICustomerRepo, CustomerRepo>();
        x.AddScoped<ICustomerService, CustomerService>();
        x.AddScoped<ISponsorRepo,SponsorRepo>();
        x.AddScoped<IComponentTypeRepo, ComponentTypeRepo>();
        x.AddScoped<IComponentCatalogueRepo, ComponentCatalogueRepo>();
        x.AddScoped<IFlowRepo, FlowRepo>();
        x.AddScoped<IFlowStepRepo, FlowStepRepo>();
        x.AddScoped<IFlowStepService, FlowStepService>();
        x.AddScoped<IWalletCategoryRepo, WalletCategoryRepo>();
        x.AddScoped<IWalletCategoryService, WalletCategoryService>();
        x.AddSingleton<LogEnricher>();
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
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Tenant API");
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

    Console.WriteLine("tenant-api: about to app.Run"); //Logs
    app.Run();
}
catch (Exception ex)
{
    Console.WriteLine($"Exception: {ex.Message}, {ex.StackTrace}");
}