using AutoMapper;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using SunnyRewards.Helios.Common.Core.Extensions;
using SunnyRewards.Helios.Common.Core.Extensions.AWSSecretsManager;
using SunnyRewards.Helios.Common.Core.Helpers;
using SunnyRewards.Helios.Common.Core.ServiceRegistry;
using SunnyRewards.Helios.Task.Infrastructure.Helpers;
using SunnyRewards.Helios.Task.Infrastructure.Helpers.Interface;
using SunnyRewards.Helios.Task.Infrastructure.Mappings;
using SunnyRewards.Helios.Task.Infrastructure.Mappings.MappingProfile;
using SunnyRewards.Helios.Task.Infrastructure.Repositories;
using SunnyRewards.Helios.Task.Infrastructure.Repositories.Interfaces;
using SunnyRewards.Helios.Task.Infrastructure.Services;
using SunnyRewards.Helios.Task.Infrastructure.Services.Helpers;
using SunnyRewards.Helios.Task.Infrastructure.Services.Interface;


var builder = WebApplication.CreateBuilder(args);
    var configuration = new ConfigurationBuilder()
               .SetBasePath(Directory.GetCurrentDirectory())
               .AddJsonFile("appsettings.json")
               .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
               .AddAWSSecretsManager(builder.Environment.EnvironmentName)
               .Build();

// DB ConnectionStrings
builder.Services.AddSingleton<IConfiguration>(configuration);

builder.Host.AddSerilogLogging();

// vault setup for secrets access
builder.Services.InitVault();

    // get instance of vault (unable to use IoC to get instance before builder.Build() is invoked)
    var vault = RegisterServices.GetVault(configuration);

// DB ConnectionString
string srConnectionStr = await vault.GetSecret("SRConnectionString");

if (string.IsNullOrEmpty(srConnectionStr) || srConnectionStr == vault.InvalidSecret)
        throw new Exception("SRConnectionString is null/empty or invalid");

    // Add services to the container.
    builder.Services.AddHealthChecks().AddCheck("Liveliness", () => HealthCheckResult.Healthy());
    builder.Services.AddNhibernate<TaskMap>(srConnectionStr);
    builder.Services.AddHttpContextAccessor();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();
    builder.Services.AddControllers();
    Console.WriteLine("task-api: AddControllers"); //Logs

    var mapperConfig = new MapperConfiguration(mc =>
    {
        mc.AddProfile(new TaskMapping());
        mc.AddProfile(new TaskDetailMapping());
        mc.AddProfile(new TaskRewardMapping());
        mc.AddProfile(new TaskRewardTypeMapping());
        mc.AddProfile(new TermsOfServiceMapping());
        mc.AddProfile(new ConsumerTaskMapping());
        mc.AddProfile(new TaskCategoryMapping());
        mc.AddProfile(new TenantTaskCategoryMapping());
        mc.AddProfile(new TriviaQuestionMapping());
        mc.AddProfile(new TriviaMapping());
        mc.AddProfile(new TriviaQuestionGroupMapping());
        mc.AddProfile(new TaskTypeMapping());
        mc.AddProfile(new SubTaskMapping());
        mc.AddProfile(new TaskExternalMapping());
        mc.AddProfile(new TriviaRequestMapping());
        mc.AddProfile(new AdventureMapping());
        mc.AddProfile(new TaskRewardCollectionMapping());
        mc.AddProfile(new TenantAdventureMapping());
        mc.AddProfile(new QuestionnaireMapping());
        mc.AddProfile(new QuestionnaireQuestionMapping());
        mc.AddProfile(new QuestionnaireQuestionGroupMapping());
        mc.AddProfile(new QuestionnaireRequestMapping());
    });
    IMapper mapper = mapperConfig.CreateMapper();

    // Register Dependencies to the DI Container
    builder.Services.InitDependencies(x =>
    {
        x.AddSingleton(mapper);
        x.AddSingleton<LivelinessService>();
        x.AddScoped<ITaskRepo, TaskRepo>();
        x.AddScoped<ITaskService, TaskService>();
        x.AddScoped<ITaskRewardRepo, TaskRewardRepo>();
        x.AddScoped<ITaskRewardService, TaskRewardService>();
        x.AddScoped<IConsumerTaskRepo, ConsumerTaskRepo>();
        x.AddScoped<IConsumerTaskService, ConsumerTaskService>();
        x.AddScoped<ITaskDetailRepo, TaskDetailRepo>();
        x.AddScoped<ITermsOfServiceRepo, TermsOfServiceRepo>();
        x.AddScoped<ITaskCategoryRepo, TaskCategoryRepo>();
        x.AddScoped<ITenantTaskCategoryRepo, TenantTaskCategoryRepo>();
        x.AddScoped<ITriviaService, TriviaService>();
        x.AddScoped<ITriviaQuestionRepo, TriviaQuestionRepo>();
        x.AddScoped<ITriviaRepo, TriviaRepo>();
        x.AddScoped<ITriviaQuestionGroupRepo, TriviaQuestionGroupRepo>();
        x.AddScoped<ITaskTypeRepo, TaskTypeRepo>();
        x.AddScoped<ITaskTypeService, TaskTypeService>();
        x.AddScoped<ISubTaskRepo, SubTaskRepo>();
        x.AddScoped<ISubtaskService, SubTaskService>();
        x.AddScoped<ITaskRewardTypeRepo, TaskRewardTypeRepo>();
        x.AddScoped<IFileHelper, FileHelper>();
        x.AddScoped<ITaskExternalMappingRepo, TaskExternalMappingRepo>();
        x.AddScoped<ITenantTaskCategoryService, TenantTaskCategoryService>();
        x.AddScoped<ITaskDetailsService, TaskDetailsService>();
        x.AddScoped<ITermsOfServiceService, TermsOfServiceService>();
        x.AddScoped<ITriviaQuestionService, TriviaQuestionService>();
        x.AddScoped<ITriviaQuestionGroupService, TriviaQuestionGroupService>();
        x.AddScoped<ITaskCategoryService, TaskCategoryService>();
        x.AddScoped<ITaskCategoryRepo, TaskCategoryRepo>();
        x.AddScoped<ITaskRewardTypeService, TaskRewardTypeService>();
        x.AddScoped<ITaskRewardTypeRepo, TaskRewardTypeRepo>();
        x.AddScoped<IImportTaskService, ImportTaskService>();
        x.AddScoped<IHealthMetricsConsumerSummaryService, HealthMetricsConsumerSummaryService>();
        x.AddScoped<IAdventureRepo, AdventureRepo>();
        x.AddScoped<IAdventureService, AdventureService>();
        x.AddScoped<IImportTriviaService, ImportTriviaService>();
        x.AddScoped<ITaskSubtaskMappingImportService, TaskSubtaskMappingImportService>();
        x.AddScoped<ITaskRewardCollectionRepo, TaskRewardCollectionRepo>();
        x.AddScoped<ITaskRewardCollectionService, TaskRewardCollectionService>();
        x.AddScoped<ICommonTaskRewardService, CommonTaskRewardService>();
        x.AddScoped<IImportTaskRewardCollectionService, ImportTaskRewardCollectionService>();
        x.AddScoped<ITenantAdventureRepo, TenantAdventureRepo>();
        x.AddScoped<IQuestionnaireRepo, QuestionnaireRepo>();
        x.AddScoped<IQuestionnaireQuestionRepo, QuestionnaireQuestionRepo>();
        x.AddScoped<IQuestionnaireQuestionGroupRepo, QuestionnaireQuestionGroupRepo>();
        x.AddScoped<IQuestionnaireService, QuestionnaireService>();
        x.AddScoped<IQuestionnaireHelper, QuestionnaireHelper>();
        x.AddScoped<IImportQuestionnaireService, ImportQuestionnaireService>();
        x.AddScoped<IQuestionnaireQuestionGroupService, QuestionnaireQuestionGroupService>();
        x.AddScoped<IQuestionnaireQuestionService, QuestionnaireQuestionService>();
        x.AddScoped<IDataQueryService, DataQueryService>();
        x.AddScoped<IMappingProfileProvider, MappingProfileProvider>();
        x.AddScoped<IRowToDtoMapper, RowToDtoMapper>();
        x.AddScoped<IQueryGeneratorService, QueryGeneratorService>();
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
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Task API");
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

    app.UseHttpsRedirection();

    app.UseAuthorization();

    app.MapControllers();
    var httpContextAccessor = app.Services.GetRequiredService<IHttpContextAccessor>();
    HttpContextHelper.Initialize(httpContextAccessor);

Console.WriteLine("task-api: about to app.Run"); //Logs
    app.Run();
