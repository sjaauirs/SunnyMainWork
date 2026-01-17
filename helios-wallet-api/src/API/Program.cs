using AutoMapper;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using SunnyRewards.Helios.Common.Core.Extensions;
using SunnyRewards.Helios.Common.Core.Extensions.AWSSecretsManager;
using SunnyRewards.Helios.Common.Core.Helpers;
using SunnyRewards.Helios.Common.Core.Mappings.MappingProfile;
using SunnyRewards.Helios.Common.Core.ServiceRegistry;
using SunnyRewards.Helios.Wallet.Infrastructure.Helpers;
using SunnyRewards.Helios.Wallet.Infrastructure.Helpers.Interfaces;
using SunnyRewards.Helios.Wallet.Infrastructure.HttpClients;
using SunnyRewards.Helios.Wallet.Infrastructure.HttpClients.Interfaces;
using SunnyRewards.Helios.Wallet.Infrastructure.Mappings;
using SunnyRewards.Helios.Wallet.Infrastructure.Mappings.MappingProfile;
using SunnyRewards.Helios.Wallet.Infrastructure.Repositories;
using SunnyRewards.Helios.Wallet.Infrastructure.Repositories.Interfaces;
using SunnyRewards.Helios.Wallet.Infrastructure.Services;
using SunnyRewards.Helios.Wallet.Infrastructure.Services.Helpers;
using SunnyRewards.Helios.Wallet.Infrastructure.Services.Interfaces;

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
    builder.Services.AddNhibernate<ConsumerWalletMap>(srConnectionStr, "wallet");
    builder.Services.AddHttpContextAccessor();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();
    builder.Services.AddControllers();
    Console.WriteLine("wallet-api: AddControllers"); //Logs

    var mapperConfig = new MapperConfiguration(mc =>
    {
        mc.AddProfile(new ConsumerWalletMapping());
        mc.AddProfile(new WalletMapping());
        mc.AddProfile(new WalletTypeMapping());
        mc.AddProfile(new TransactionMapping());
        mc.AddProfile(new TransactionDetailMapping());
        mc.AddProfile(new RedemptionMapping());
        mc.AddProfile(new AuditTrailMapping());
        mc.AddProfile(new ConsumerWalletDetailsMapping());
    });
    IMapper mapper = mapperConfig.CreateMapper();

    // Register Dependencies to the DI Container
    builder.Services.InitDependencies(x =>
    {
        x.AddSingleton(mapper);
        x.AddSingleton<LivelinessService>();
        x.AddScoped<IConsumerWalletRepo, ConsumerWalletRepo>();
        x.AddScoped<IConsumerWalletService, ConsumerWalletService>();
        x.AddScoped<IWalletRepo, WalletRepo>();
        x.AddScoped<IWalletTypeRepo, WalletTypeRepo>();
        x.AddScoped<IWalletService, WalletService>();
        x.AddScoped<ITransactionRepo, TransactionRepo>();
        x.AddScoped<ITransactionService, TransactionService>();
        x.AddScoped<ITransactionDetailRepo, TransactionDetailRepo>();
        x.AddScoped<IRedemptionRepo, RedemptionRepo>();
        x.AddScoped<IUserClient, UserClient>();
        x.AddScoped<ISecretHelper, SecretHelper>();
        x.AddScoped<ICsaWalletTransactionsService, CsaWalletTransactionsService>();
        x.AddScoped<IPurseFundingService, PurseFundingService>();
        x.AddScoped<IWalletTypeTransferRuleRepo, WalletTypeTransferRuleRepo>();
        x.AddScoped<IConsumerService, ConsumerService>();
        x.AddSingleton<LogEnricher>();
        x.AddScoped<IWalletTypeTransferRuleService, WalletTypeTransferRuleService>();
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
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Wallet API");
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

    Console.WriteLine("wallet-api: about to app.Run"); //Logs
    app.Run();
}
catch (Exception ex)
{
    Console.WriteLine($"Exception: {ex.Message}, {ex.StackTrace}");
}