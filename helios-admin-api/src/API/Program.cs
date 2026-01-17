using Amazon.SQS;
using AutoMapper;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using SunnyRewards.Helios.Admin.Infrastructure.Helpers;
using SunnyRewards.Helios.Admin.Infrastructure.Helpers.Interface;
using SunnyRewards.Helios.Admin.Infrastructure.HttpClients;
using SunnyRewards.Helios.Admin.Infrastructure.HttpClients.Interfaces;
using SunnyRewards.Helios.Admin.Infrastructure.Mappings;
using SunnyRewards.Helios.Admin.Infrastructure.Mappings.MappingProfile;
using SunnyRewards.Helios.Admin.Infrastructure.Repositories;
using SunnyRewards.Helios.Admin.Infrastructure.Repositories.HeliosRepo.Interfaces;
using SunnyRewards.Helios.Admin.Infrastructure.Repositories.Interfaces;
using SunnyRewards.Helios.Admin.Infrastructure.Services;
using SunnyRewards.Helios.Admin.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.Bff.Infrastructure.Services;
using SunnyRewards.Helios.Bff.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.Common.Core.Extensions;
using SunnyRewards.Helios.Common.Core.Extensions.AWSSecretsManager;
using SunnyRewards.Helios.Common.Core.Helpers;
using SunnyRewards.Helios.Common.Core.ServiceRegistry;
using SunnyRewards.Helios.ETL.Infrastructure.Profiles;

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
var mapperConfig = new MapperConfiguration(mc =>
{
    mc.AddProfile(new TenantTaskRewardScriptMapping());
    mc.AddProfile(new ScriptMapping());
    mc.AddProfile(new BatchJobDetailReportMappingProfile());
    mc.AddProfile(new BatchJobReportMappingProfile());
    mc.AddProfile(new PostEventMappingProfile());
    mc.AddProfile(new TaskMappingProfile());
    mc.AddProfile(new PostTenantMapping());
    mc.AddProfile(new TenantAccountMappingProfile());
    mc.AddProfile(new EventHandlerScriptMappingProfile());
});
IMapper mapper = mapperConfig.CreateMapper();

// Register services to the DI container.
builder.Services.InitDependencies(x =>
{
    x.AddSingleton(mapper);
    x.AddSingleton<LivelinessService>();
    x.AddScoped<IUserClient, UserClient>();
    x.AddScoped<IWalletClient, WalletClient>();
    x.AddScoped<ITaskClient, TaskClient>();
    x.AddScoped<ITenantClient, TenantClient>();
    x.AddScoped<IFisClient, FisClient>();
    x.AddScoped<IConsumerTaskService, ConsumerTaskService>();
    x.AddScoped<IWalletService, WalletService>();
    x.AddScoped<IConsumerAccountService, ConsumerAccountService>();
    x.AddScoped<ISweepstakesInstanceService, SweepstakesInstanceService>();
    x.AddScoped<ISweepstakesClient, SweepstakesClient>();
    x.AddScoped<ITenantExportService, TenantExportService>();
    x.AddScoped<ISecretHelper, SunnyRewards.Helios.Admin.Infrastructure.Helpers.SecretHelper>();
    x.AddScoped<IS3Service, S3Service>();
    x.AddScoped<ICohortClient, CohortClient>();
    x.AddScoped<ICmsClient, CmsClient>();
    x.AddScoped<IAmazonS3ClientService, AmazonS3ClientService>();
    x.AddScoped<ITenantService, TenantService>();
    x.AddScoped<ITaskService, TaskService>();
    x.AddScoped<ITriviaService, TriviaService>();
    x.AddScoped<ISubtaskService, SubTaskService>();
    x.AddScoped<ITaskExternalMappingService, TaskExternalMappingService>();
    x.AddScoped<ITriviaQuestionService, TriviaQuestionService>();
    x.AddScoped<ITriviaQuestionGroupService, TriviaQuestionGroupService>();
    x.AddScoped<ITenantTaskCategoryService, TenantTaskCategoryService>();
    x.AddScoped<ICohortService, CohortService>();
    x.AddScoped<ITaskDetailsService, TaskDetailsService>();
    x.AddScoped<ITaskRewardService, TaskRewardService>();
    x.AddScoped<ITaskCategoryService, TaskCategoryService>();
    x.AddScoped<ITaskRewardTypeService, TaskRewardTypeService>();
    x.AddScoped<ITaskTypeService, TaskTypeService>();
    x.AddScoped<ITermsOfServiceService, TermsOfServiceService>();
    x.AddScoped<ICohortTenantTaskRewardService, CohortTenantTaskRewardService>();
    x.AddScoped<ITenantAccountService, TenantAccountService>();
    x.AddScoped<ITenantSweepstakesService, TenantSweepstakesService>();
    x.AddScoped<ISweepstakesService, SweepstakesService>();
    x.AddScoped<IComponentService, ComponentService>();
    x.AddScoped<ITenantTaskRewardScriptRepo, TenantTaskRewardScriptRepo>();
    x.AddScoped<ICohortConsumerService, CohortConsumerService>();
    x.AddScoped<IScriptRepo, ScriptRepo>();
    x.AddScoped<ITaskRewardScriptResultRepo, TaskRewardScriptResultRepo>();
    x.AddScoped<IScriptService, ScriptService>();
    x.AddScoped<ITenantTaskRewardScriptService, TenantTaskRewardScriptService>();
    x.AddScoped<ICohortConsumerTaskService, CohortConsumerTaskService>();
    x.AddScoped<IJobReportService, JobReportService>();
    x.AddScoped<IJobDetailReportService, JobDetailReportService>();
    x.AddScoped<IBatchJobReportRepo, BatchJobReportRepo>();
    x.AddScoped<IBatchJobDetailReportRepo, BatchJobDetailReportRepo>();
    x.AddScoped<IImageSearchService, ImageSearchService>();
    x.AddScoped<IAwsQueueService, AwsQueueService>();
    x.AddScoped<IEventService, EventService>();
    x.AddScoped<IAutoEnrollConsumerTaskService, AutoEnrollConsumerTaskService>();
    x.AddScoped<IEventHandlerResultRepo, EventHandlerResultRepo>();
    x.AddScoped<IEventHandlerScriptRepo, EventHandlerScriptRepo>();
    x.AddScoped<IEventProcessorFactory, EventProcessorFactory>();
    x.AddScoped<IEventProcessorHelper,EventProcessorHelper>();
    x.AddScoped<ITaskTriggerEventProcessor, TaskTriggerEventProcessor>();
    x.AddScoped<ICustomerService, CustomerService>();
    x.AddScoped<ISponsorService, SponsorService>();
    x.AddScoped<IWalletTypeService, WalletTypeService>();
    x.AddScoped<IPersonRoleService, PersonRoleService>();
    x.AddScoped<ICsaTransactionService, CsaTransactionService>();
    x.AddScoped<IOnBoardingInitialFundingService, OnBoardingInitialFundingService>();
    x.AddScoped<IPickAPurseEventProcessor, PickAPurseEventProcessor>();
    x.AddScoped<IHealthClient, HealthClient>();
    x.AddScoped<IHealthMetricService, HealthMetricService>();
    x.AddScoped<IAuth0Service, Auth0Service>();
    x.AddScoped<IConsumerWalletService, ConsumerWalletService>();
    x.AddScoped<IConsumerService, ConsumerService>();
    x.AddScoped<ITenantImportService, TenantImportService>();
    x.AddScoped<IS3Helper, S3Helper>();
    x.AddScoped<IConsumerTaskEventService, ConsumerTaskEventService>();
    x.AddScoped<IConsumerTaskEventProcesser, ConsumerTaskEventProcessor>();
    x.AddScoped<IConsumerEventProcessorHelper,ConsumerEventProcessorHelper>();
    x.AddSingleton<LogEnricher>();
    x.AddScoped<IAdminService, AdminService>();
    x.AddScoped<IWalletTypeTransferRuleService, WalletTypeTransferRuleService>();
    x.AddScoped<ITransactionService, TransactionService>();

    x.AddScoped<IConsumerUpdateEventProcessor, ConsumerUpdateEventProcessor>();
    x.AddScoped<IFundTransferService, FundTransferService>();
    x.AddScoped<ICardOperationService, CardOperationService>();

    x.AddScoped<IConsumerCohortRuleProcessor ,  ConsumerCohortRuleProcessor>();
    x.AddScoped<IConsumerCohortEventProcessor,  ConsumerCohortEventProcessor>();
    x.AddScoped<IConsumerCohortHelper, ConsumerCohortHelper>();
    x.AddScoped<IConsumerPurseCohortAssignmentService, ConsumerPurseCohortAssignmentService>();
    x.AddScoped<IConsumerPurseAssignmentService, ConsumerPurseAssignmentService>();
    x.AddScoped<IConsumerLoginService, ConsumerLoginService>();
    x.AddScoped<IAgreementsVerifiedEventService, AgreementsVerifiedEventService>();
    x.AddScoped<IAgreementsVerifiedEventProcessor, AgreementsVerifiedEventProcessor>();
    x.AddScoped<IWalletHelper, WalletHelper>();
    x.AddScoped<IUserContextService, UserContextService>();

});

//Inject vault
builder.Services.InitVault();

// get instance of vault (unable to use IoC to get instance before builder.Build() is invoked)
var vault = RegisterServices.GetVault(configuration);
// DB ConnectionString
string srConnectionStr = await vault.GetSecret("SRConnectionString");


if (string.IsNullOrEmpty(srConnectionStr) || srConnectionStr == vault.InvalidSecret)
    throw new Exception("SRConnectionString is null/empty or invalid");

// Add services to the container
builder.Services.AddHealthChecks().AddCheck("Liveliness", () => HealthCheckResult.Healthy());
builder.Services.AddNhibernate<BatchJobDetailReportMap>(srConnectionStr);
builder.Services.AddHttpContextAccessor();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();
Console.WriteLine("admin-api: AddControllers"); //Logs

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

builder.Services.AddAWSService<IAmazonSQS>();
builder.Services.AddHostedService<SqsMessageListenerService>();
builder.Services.AddHostedService<FIFOSqsMessageListenerService>();

// Middleware
var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Admin API");
});

#region Publish API/DB metrics to CloudWatch
app.Services.SetServiceProvider();
app.UseCloudWatchMetricsMiddleware();
#endregion

// Configure API Health Check Settings 
app.UseHealthChecks("/health/live", new HealthCheckOptions()
{
    Predicate = check => check.Name == "Liveliness"
});

// Default CORS Policy Applied
app.UseCors(x => x
        .AllowAnyOrigin()
        .AllowAnyMethod()
        .AllowAnyHeader());

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

var httpContextAccessor = app.Services.GetRequiredService<IHttpContextAccessor>();
HttpContextHelper.Initialize(httpContextAccessor);

Console.WriteLine("admin-api: about to app.Run"); //Logs
app.Run();