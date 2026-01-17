using Amazon.Batch;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SunnyRewards.Helios.Common.Core.Extensions.AWSSecretsManager;
using SunnyRewards.Helios.Common.Core.Helpers.Interfaces;
using SunnyRewards.Helios.Common.Core.ServiceRegistry;
using SunnyRewards.Helios.Etl.Core.Domain.Dtos;
using SunnyRewards.Helios.Etl.Infrastructure.Services;
using SunnyRewards.Helios.Etl.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.ETL.Common.Constants;
using SunnyRewards.Helios.ETL.Common.Extensions;
using SunnyRewards.Helios.ETL.Common.Helpers;
using SunnyRewards.Helios.ETL.Common.Helpers.Interfaces;
using SunnyRewards.Helios.ETL.Common.Repositories;
using SunnyRewards.Helios.ETL.Common.Repositories.Interfaces;
using SunnyRewards.Helios.ETL.Core.Domain.Dtos;
using SunnyRewards.Helios.ETL.Core.Domain.Dtos.Enums;
using SunnyRewards.Helios.ETL.Core.Domain.Models;
using SunnyRewards.Helios.ETL.Core.Domain.Models.DynamoDb;
using SunnyRewards.Helios.ETL.Infrastructure.Helpers;
using SunnyRewards.Helios.ETL.Infrastructure.Helpers.Interfaces;
using SunnyRewards.Helios.ETL.Infrastructure.HttpClients;
using SunnyRewards.Helios.ETL.Infrastructure.HttpClients.Interfaces;
using SunnyRewards.Helios.ETL.Infrastructure.Logs;
using SunnyRewards.Helios.ETL.Infrastructure.Logs.Interface;
using SunnyRewards.Helios.ETL.Infrastructure.Mappings;
using SunnyRewards.Helios.ETL.Infrastructure.Profiles;
using SunnyRewards.Helios.ETL.Infrastructure.Repositories;
using SunnyRewards.Helios.ETL.Infrastructure.Repositories.HeliosRepo;
using SunnyRewards.Helios.ETL.Infrastructure.Repositories.HeliosRepo.Interfaces;
using SunnyRewards.Helios.ETL.Infrastructure.RuleEngine;
using SunnyRewards.Helios.ETL.Infrastructure.RuleEngine.Interfaces;
using SunnyRewards.Helios.ETL.Infrastructure.S3FileProcessor;
using SunnyRewards.Helios.ETL.Infrastructure.S3FileProcessor.Interface;
using SunnyRewards.Helios.ETL.Infrastructure.Services;
using SunnyRewards.Helios.ETL.Infrastructure.Services.DynamoDb;
using SunnyRewards.Helios.ETL.Infrastructure.Services.FIS;
using SunnyRewards.Helios.ETL.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.ETL.Infrastructure.Services.Interfaces.DynamoDb;
using SunnyRewards.Helios.ETL.Infrastructure.Services.Interfaces.FIS;
using ISecretHelper = SunnyRewards.Helios.ETL.Infrastructure.Helpers.Interfaces.ISecretHelper;

namespace SunnyRewards.Helios.ETL.App
{
    public class LocalLogger
    {
        public ILogger<LocalLogger> Logger { get; private set; }


        public LocalLogger()
        {
            var logfactory = LoggerFactory.Create(builder =>
            {
                builder.AddConsole();
                builder.AddDebug();
            });
            Logger = new Logger<LocalLogger>(logfactory);
        }
    }

    public static class Program
    {
        private readonly static LocalLogger _ll = new();
        private const string className = nameof(Program);

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            string env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "";
            return Host.CreateDefaultBuilder(args)
                .ConfigureServices(async (hostContext, services) =>
                {
                    try
                    {
                        var configuration = new ConfigurationBuilder()
                        .SetBasePath(Directory.GetCurrentDirectory())
                        .AddJsonFile("appsettings.json")
                        .AddJsonFile($"appsettings.{env}.json", optional: true)
                        .AddAWSSecretsManager(env)
                        .Build();
                        services.AddSingleton<IConfiguration>(configuration);

                        services.AddLogging(x => x.AddAWSProvider());

                        var mapperConfig = new MapperConfiguration(mc =>
                        {
                            mc.AddProfile(new MonetaryTransactionMappingProfile());
                            mc.AddProfile(new ConsumerAccountHistoryMappingProfile());
                            mc.AddProfile(new TaskRewardMappingProfile());
                            mc.AddProfile(new ConsumerTaskMappingProfile());
                            mc.AddProfile(new CohortConsumerMappingProfile());
                            mc.AddProfile(new ConsumerMappingProfile());
                        });

                        IMapper mapper = mapperConfig.CreateMapper();

                        var vault = RegisterServices.GetVault(configuration);

                        // var envName = Environment.GetEnvironmentVariable("env");
                        // var secEnv = Environment.GetEnvironmentVariable(envName);
                        // _ll.Logger.LogInformation("secEnv: {secEnv}", secEnv);

                        // DB ConnectionString
                        string srConnectionStr = await vault.GetSecret("SRConnectionString");

                        // string customerdbConnectionStr = await vault.GetSecret("CustomerDbConnectionString");

                        if (string.IsNullOrEmpty(srConnectionStr) || srConnectionStr == vault.InvalidSecret /* || string.IsNullOrEmpty(customerdbConnectionStr) || customerdbConnectionStr == vault.InvalidSecret */)
                            throw new ArgumentNullException(nameof(srConnectionStr), "SRConnectionString cannot be null or empty");

                        //_ll.Logger.LogInformation("SRconn: {srConn}, custConn: {custConn}", srConnectionStr, customerdbConnectionStr);

                        services.AddNhibernate<ETLConsumerModelMap>(srConnectionStr, null, "etl");

                        services.InitDependencies(x =>
                        {
                            try
                            {
                                x.InitVault();
                                x.AddSingleton(mapper);

                                // JSON Convert Wrapper
                                x.AddSingleton<IJsonConvertWrapper, JsonConvertWrapper>();

                                x.AddScoped<IRuleExecutor, RuleExecutor>();

                                // DB Repos
                                x.AddScoped<ITenantRepo, TenantRepo>();
                                x.AddScoped<IPersonRepo, PersonRepo>();
                                x.AddScoped<IConsumerRepo, ConsumerRepo>();
                                x.AddScoped<ITaskRepo, TaskRepo>();
                                x.AddScoped<ITaskDetailRepo, TaskDetailRepo>();
                                x.AddScoped<ITaskTypeRepo, TaskTypeRepo>();
                                x.AddScoped<IRewardTypeRepo, RewardTypeRepo>();
                                x.AddScoped<ITaskRewardRepo, TaskRewardRepo>();
                                x.AddScoped<ICohortRepo, CohortRepo>();
                                x.AddScoped<ICohortTenantTaskRewardRepo, CohortTenantTaskRewardRepo>();
                                x.AddScoped<ITriviaRepo, TriviaRepo>();
                                x.AddScoped<ITriviaQuestionRepo, TriviaQuestionRepo>();
                                x.AddScoped<IQuestionnaireRepo, QuestionnaireRepo>();
                                x.AddScoped<IQuestionnaireQuestionRepo, QuestionnaireQuestionRepo>();
                                x.AddScoped<IWalletRepo, WalletRepo>();
                                x.AddScoped<IWalletTypeRepo, WalletTypeRepo>();
                                x.AddScoped<ITenantAccountRepo, TenantAccountRepo>();
                                x.AddScoped<IMonetaryTransactionRepo, MonetaryTransactionRepo>();
                                x.AddScoped<IRedemptionRepo, RedemptionRepo>();

                                // Services
                                x.AddSingleton<IEnrollmentService, EnrollmentService>();
                                x.AddSingleton<ICohortService, CohortService>();
                                x.AddSingleton<IRedemptionService, RedemptionService>();

                                // PLD file handling services
                                x.AddSingleton<IPldFieldInfoProvider, PldFieldInfoProvider>();
                                x.AddScoped<IPldParser, PldParser>();

                                // 
                                x.AddScoped<IAwsQueueService, AwsQueueService>();
                                x.AddScoped<IAwsS3Service, AwsS3Service>();
                                x.AddScoped<IS3FileProcessorCore, S3FileProcessorCore>();

                                x.AddScoped<ITaskUpdateService, TaskUpdateService>();
                                x.AddScoped<IWalletService, WalletService>();

                                // 
                                x.AddScoped<IIngestConsumerAttrService, IngestConsumerAttrService>();

                                // Task import
                                x.AddScoped<ITaskImportService, TaskImportService>();

                                // Trivia import
                                x.AddScoped<ITriviaImportService, TriviaImportService>();

                                // Questionnaire import
                                x.AddScoped<IQuestionnaireImportService, QuestionnaireImportService>();

                                //Logs
                                x.AddSingleton<IS3FileLogger, S3FileLogger>();
                                x.AddScoped<IS3FISFileLogger, S3FISFileLogger>();

                                //Member import
                                x.AddScoped<IMemberImportService, MemberImportService>();

                                x.AddSingleton<IConsumerQuery, ConsumerQuery>();
                                x.AddScoped<IConsumerTaskRepo, ConsumerTaskRepo>();
                                x.AddScoped<IAwsNotificationService, AwsNotificationService>();

                                x.AddScoped<IDataFeedClient, DataFeedClient>();
                                x.AddScoped<IAdminClient, AdminClient>();
                                x.AddScoped<IHealthClient, HealthClient>();
                                x.AddScoped<IRetailClient, RetailClient>();
                                x.AddScoped<IBenefitBFFClient, BenefitBFFClient>();

                                x.AddScoped<IRetailProductSyncService, RetailProductSyncService>();
                                x.AddScoped<IS3FileEncryptionHelper, S3FileEncryptionHelper>();

                                x.AddTransient<IFlatFileGenerator, FlatFileGenerator>();
                                x.AddTransient<IFlatFileReader, FlatFileReader>();
                                x.AddTransient<IFISFlatFileRecordDtoFactory, FISFlatFileRecordDtoFactory>();

                                // FIS Batch file creation services
                                x.AddScoped<ICardBatchFileRecordCreateService, CardBatchFileRecordCreateService>();
                                x.AddScoped<ICardBatchFileCreateService, CardBatchFileCreateService>();
                                x.AddScoped<ICardDisbursementFileCreateService, CardDisbursementFileCreateService>();
                                x.AddScoped<ICardDisbursementFileRecordCreateService, CardDisbursementFileRecordCreateService>();
                                x.AddScoped<ICardBatchFileReadService, CardBatchFileReadService>();
                                x.AddScoped<IMonetaryTransactionsFileReadService, MonetaryTransactionsFileReadService>();
                                x.AddScoped<IPerformExternalTxnSyncService, PerformExternalTxnSyncService>();
                                x.AddScoped<IRecordType30ProcessService, RecordType30ProcessService>();
                                x.AddScoped<IRecordType60ProcessService, RecordType60ProcessService>();
                                x.AddScoped<IEncryptAndUploadFileToOutboundService, EncryptAndUploadFileToOutboundService>();

                                // Health metric services
                                x.AddScoped<IHealthMetricTypeRepo, HealthMetricTypeRepo>();
                                x.AddScoped<IHealthMetricRepo, HealthMetricRepo>();
                                x.AddScoped<IHealtMetricsSyncService, HealthMetricsSyncService>();

                                x.AddScoped<ITransactionRepo, TransactionRepo>();
                                x.AddScoped<IConsumerWalletRepo, ConsumerWalletRepo>();
                                x.AddScoped<IFundTransferService, FundTransferService>();
                                x.AddScoped<IFundingRuleExecService, PeriodMonthFundingRuleService>();
                                x.AddScoped<IFundingRuleExecService, PeriodQuarterFundingRuleService>();
                                x.AddScoped<IBenefitsFundingService, BenefitsFundingService>();
                                x.AddScoped<IConsumerAccountRepo, ConsumerAccountRepo>();
                                x.AddScoped<IConsumerAccountHistoryRepo, ConsumerAccountHistoryRepo>();
                                x.AddScoped<IConsumerAccountService, ConsumerAccountService>();
                                x.AddScoped<IPgpS3FileEncryptionHelper, PgpS3FileEncryptionHelper>();
                                x.AddScoped<IS3Helper, S3Helper>();
                                x.AddScoped<IConsumerNonMonetaryTransactionsFileReadService, ConsumerNonMonetaryTransactionsFileReadService>();
                                x.AddScoped<ITaskExternalMappingRepo, TaskExternalMappingRepo>();
                                x.AddScoped<ITokenService, TokenService>();

                                // Health tasks services
                                x.AddScoped<IHealthMetricRollupRepo, HealthMetricRollupRepo>();
                                x.AddScoped<IHealthTaskSyncService, HealthTaskSyncService>();

                                // Helpers
                                x.AddScoped<ISecretHelper, SecretHelper>();
                                x.AddScoped<ICsvWrapper, CsvWrapper>();
                                x.AddScoped<IDateTimeHelper, DateTimeHelper>();
                                x.AddScoped<IDynamoDbHelper, DynamoDbHelper>();

                                // Sweepstakes services
                                x.AddScoped<ITenantSweepstakesRepo, TenantSweepstakesRepo>();
                                x.AddScoped<ISweepstakesInstanceRepo, SweepstakesInstanceRepo>();
                                x.AddScoped<ISweepstakesResultRepo, SweepstakesResultRepo>();
                                x.AddScoped<ISweepstakesConsumerService, SweepstakesConsumerService>();
                                x.AddScoped<ISweepstakesImportService, SweepstakesImportService>();
                                x.AddScoped<ICreateDuplicateConsumerWithNewEmail, CreateDuplicateConsumerWithNewEmail>();

                                // Consumer Services
                                x.AddScoped<IConsumerService, ConsumerService>();

                                x.AddScoped<IAuditTrailRepo, AuditTrailRepo>();


                                x.AddScoped<IJobReportService, JobReportService>();

                                x.AddScoped<ISponsorRepo, SponsorRepo>();
                                x.AddScoped<ICustomerRepo, CustomerRepo>();

                                //Batch Operation
                                x.AddScoped<IBatchOperationService, BatchOperationService>();
                                x.AddScoped<IBatchOperationRepo, BatchOperationRepo>();
                                x.AddScoped<IProcessRecurringTasksService, ProcessRecurringTasksService>();
                                x.AddScoped<ICohortConsumerRepo, CohortConsumerRepo>();
                                x.AddScoped<ICohortConsumerService, CohortConsumerService>();
                                x.AddScoped<IMemberImportFileDataService, MemberImportFileDataService>();
                                x.AddScoped<IETLMemberImportFileDataRepo, ETLMemberImportFileDataRepo>();
                                x.AddScoped<IETLMemberImportFileRepo, ETLMemberImportFileRepo>();
                                x.AddScoped<IBatchFileRepo, BatchFileRepo>();
                                x.AddScoped<ICSATransactionRepo, CSATransactionRepo>();
                                x.AddScoped<IBatchFileService, BatchFileService>();
                                x.AddScoped<IJobHistoryService, JobHistoryService>();

                                x.AddScoped<ITenantConfigSyncService, TenantConfigSyncService>();
                                x.AddScoped<IConsumerAccountSyncService, ConsumerAccountSyncService>();
                                x.AddScoped<IWalletSyncService, WalletSyncService>();
                                x.AddScoped<ITaskCompletionCheckerService, TaskCompletionCheckerService>();

                                x.AddScoped<INotificationRulesService, NotificationRulesService>();
                                x.AddScoped<INotificationRuleRepository, NotificationRuleRepository>();
                                x.AddScoped<IConsumerNotificationRepo, ConsumerNotificationRepo>();
                                x.AddScoped<INotificationClient, NotificationClient>();
                                x.AddScoped<IMemoryCache, MemoryCache>();
                                x.AddScoped<IProcessCompletedConsumerTask, ProcessCompletedConsumerTask>();
                                x.AddScoped<IPersonAddressRepo, PersonAddressRepo>();
                                x.AddScoped<IPhoneNumberRepo, PhoneNumberRepo>();
                                x.AddScoped<IFileCryptoProcessor, FileCryptoProcessor>();
                                x.AddSingleton<IRedshiftSyncStatusRepo, RedshiftSyncStatusRepo>();
                                x.AddSingleton<ISyncMembersFromRedshiftToPostgresService, SyncMembersFromRedshiftToPostgresService>();
                                x.AddSingleton<ISyncRedshiftToPostgresService, SyncRedshiftToPostgresService>();
                                x.AddScoped<IRedshiftDataReader, RedshiftDataReader>();
                                x.AddScoped<IPostgresBulkInserter, PostgresBulkInserter>();
                                x.AddScoped<IRedshiftToSftpExportService, RedshiftToSftpExportService>();
                                x.AddScoped<ISftpUploader, SftpUploader>();
                                x.AddScoped<IRedshiftDataService, RedshiftDataService>();
                                x.AddScoped<IFileExportService, FileExportService>();
                                x.AddScoped<IEventService, EventService>();


                                x.AddScoped<IRedshiftToSftpExportService, RedshiftToSftpExportService>();
                                x.AddScoped<ISftpUploader, SftpUploader>();
                                x.AddScoped<IRedshiftDataService, RedshiftDataService>();
                                x.AddScoped<IFileExportService, FileExportService>();
                                x.AddScoped<ITenantProgramConfigRepo, TenantProgramConfigRepo>();

                                // Inject AWS Batch Job
                                x.AddScoped<IAwsBatchService, AwsBatchService>();
                                x.AddScoped<IDepositInstructionService, DepositInstructionService>();
                                x.AddScoped<ITaskCategoryRepo, TaskCategoryRepo>();
                                x.AddScoped<ITermOfServiceRepo, TermOfServiceRepo>();
                                x.AddScoped<IMemberImportEventingService, MemberImportEventingService>();
                                x.AddScoped<ICohortingEventingService, CohortingEventingService>();
                                x.AddScoped<IEventingWrapperService, EventingWrapperService>();
                            }
                            catch (Exception ex)
                            {
                                _ll.Logger.LogError(ex, "CreateHostBuilder: Exception during InitDependencies: {msg}", ex.Message);
                            }
                        });
                    }
                    catch (Exception ex)
                    {
                        _ll.Logger.LogError(ex, "CreateHostBuilder: Exception during ConfigureServices: {msg}", ex.Message);
                    }
                });
        }

        /// <summary>
        /// Entry point of app
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        static async System.Threading.Tasks.Task Main(string[] args)
        {
            const string methodName = nameof(Main);
            IHost? host = null;
            try
            {
                var executionContext = ProcessCommandLine(args);

                host = CreateHostBuilder(args).Build();

                if (executionContext.TenantCode.StartsWith("ENVIRONMENT:"))
                {
                    var parts = executionContext.TenantCode.Split(":");
                    executionContext.TenantCode = Environment.GetEnvironmentVariable(parts[1]);
                }
                _ll.Logger.LogInformation("{ClassName}.{MethodName} - Running for tenantCode: {TenantCode}, ver: 2.1", className, methodName, executionContext.TenantCode);
                var config = new MapperConfiguration(cfg =>
                {
                    cfg.AddProfile<MonetaryTransactionMappingProfile>();
                    cfg.AddProfile<ConsumerAccountHistoryMappingProfile>();
                });

                IMapper mapper = config.CreateMapper();


                #region Get/Create Job history record from/to DynamoDB & update status
                //Read job history record from DynamoDb
                var jobHistoryService = host.Services.GetRequiredService<IJobHistoryService>();
                var jobHistory = new JobHistoryModel();
                if (!string.IsNullOrEmpty(executionContext.JobHistoryId)
                    && !string.Equals(executionContext.JobHistoryId, Constants.DEFAULT_JOB_HISTORY_ID, StringComparison.OrdinalIgnoreCase))
                {
                    jobHistory = await jobHistoryService.GetJobHistoryById(executionContext.JobHistoryId);

                    //Update job History status to STARTED
                    jobHistory.RunStatus = Constants.JOB_HISTORY_STARTED_STATUS;
                    await jobHistoryService.UpdateJobHistory(jobHistory);
                }
                else
                {
                    var createRequest = await jobHistoryService.GetJobHistoryCreateRequest(executionContext);
                    jobHistory = await jobHistoryService.InsertJobHistory(createRequest);
                    executionContext.JobHistoryId = jobHistory.JobHistoryId ?? string.Empty;
                }

                #endregion

                if (executionContext.EnableS3)
                {
                    try
                    {
                        _ll.Logger.LogInformation("{ClassName}.EnableS3: Running for  CustomerCode: {CustomerCode}, CustomerLabel: {CustomerLabel}", className, executionContext.CustomerCode, executionContext.CustomerLabel);
                        //Update job definition in job history table
                        await jobHistoryService.UpdateJobDefinitionInJobHistory(jobHistory,
                            GetJObDefinitionIdForEnableS3(executionContext));

                        var s3processCoreService = host.Services.GetRequiredService<IS3FileProcessorCore>();

                        await s3processCoreService.StartScanAndProcessFiles(executionContext);

                        //Update jobHistory status to success
                        jobHistory.RunStatus = !string.IsNullOrEmpty(executionContext.JobHistoryStatus)
                                                      ? executionContext.JobHistoryStatus
                                                      : (string.IsNullOrEmpty(executionContext.JobHistoryErrorLog)
                                                          ? Constants.JOB_HISTORY_SUCCESS_STATUS
                                                          : Constants.JOB_HISTORY_FAILURE_STATUS);
                        jobHistory.ErrorLog = executionContext.JobHistoryErrorLog;


                        //check if Job History does not have correct tenant Code
                        if (!string.IsNullOrEmpty(jobHistory.TenantCode) && jobHistory.TenantCode.Contains(Constants.DUMMY_TENANT_PREFIX))
                        {
                            if (!string.IsNullOrEmpty(executionContext.TenantCode))
                            {
                                jobHistory.TenantCode = executionContext.TenantCode;
                            }
                        }

                        //check if jobHistory.TenantCode is different then executionContext.TenantCode
                        if (!string.IsNullOrEmpty(executionContext.TenantCode) &&
                                 string.Compare(executionContext.TenantCode, jobHistory.TenantCode, StringComparison.OrdinalIgnoreCase) != 0)
                        {
                            jobHistory.TenantCode = executionContext.TenantCode;
                        }


                        await jobHistoryService.UpdateJobHistory(jobHistory);

                        _ll.Logger.LogInformation("{ClassName}.EnableS3: Running Ended", className);
                    }
                    catch (Exception ex)
                    {
                        _ll.Logger.LogError(ex, "{ClassName}.EnableS3 - Failed processing enable S3 with CustomerCode: {CustomerCode}, ErrorCode:{Code},ERROR:{Message}",
                            className, executionContext.CustomerCode, StatusCodes.Status500InternalServerError, ex.Message);

                        //Update jobHistory status to Failure
                        jobHistory.RunStatus = Constants.JOB_HISTORY_FAILURE_STATUS;
                        jobHistory.ErrorLog = ex.Message;
                        await jobHistoryService.UpdateJobHistory(jobHistory);
                        throw;
                    }
                }

                // If we got task update csv file, then let's process it.
                if (!string.IsNullOrEmpty(executionContext.TaskUpdateFilePath))
                {
                    _ll.Logger.LogInformation("{ClasName}.TaskUpdate - Started processing  TaskUpdate with TenantCode:{TenantCode}", className, executionContext.TenantCode);
                    try
                    {
                        _ll.Logger.LogInformation("{ClasName}.TaskUpdate - Started processing  TaskUpdate with TenantCode:{TenantCode}", className, executionContext.TenantCode);

                        var taskUpdateService = host.Services.GetRequiredService<ITaskUpdateService>();
                        await taskUpdateService.ProcessTaskUpdates(executionContext.TaskUpdateFilePath, etlExecutionContext: executionContext);

                        //Update jobHistory status to success
                        jobHistory.RunStatus = !string.IsNullOrEmpty(executionContext.JobHistoryStatus)
                                                      ? executionContext.JobHistoryStatus
                                                      : (string.IsNullOrEmpty(executionContext.JobHistoryErrorLog)
                                                          ? Constants.JOB_HISTORY_SUCCESS_STATUS
                                                          : Constants.JOB_HISTORY_FAILURE_STATUS);
                        jobHistory.ErrorLog = executionContext.JobHistoryErrorLog;
                        await jobHistoryService.UpdateJobHistory(jobHistory);

                        _ll.Logger.LogInformation("{ClasName}.TaskUpdate - Ended processing  TaskUpdate with TenantCode:{TenantCode}", className, executionContext.TenantCode);
                    }
                    catch (Exception ex)
                    {
                        _ll.Logger.LogError(ex, "{ClassName}.ProcessTaskUpdate - Failed processing TaskUpdate, Csv file:{TaskUpdateFilePath},ErrorCode:{Code},ERROR:{Message}",
                            className, executionContext.TaskUpdateFilePath, StatusCodes.Status500InternalServerError, ex.Message);

                        //Update jobHistory status to Failure
                        jobHistory.RunStatus = Constants.JOB_HISTORY_FAILURE_STATUS;
                        jobHistory.ErrorLog = ex.Message;
                        await jobHistoryService.UpdateJobHistory(jobHistory);
                        throw;
                    }
                }

                // perform initial load of member/enrollment data
                /* if (executionContext.EnableMemberLoad)
                {
                    var memberLoadService = host.Services.GetRequiredService<IMemberLoadService>();
                    await memberLoadService.LoadMemberFiles(executionContext);
                } */

                if (executionContext.EnableHealthMetricProcessing)
                {
                    try
                    {
                        _ll.Logger.LogInformation("{ClassName}.EnableHealthMetricProcessing - Started processing", className);

                        var healthMetricService = host.Services.GetRequiredService<IHealtMetricsSyncService>();
                        await healthMetricService.ProcessQueueMessages(executionContext);

                        //Update jobHistory status to success
                        jobHistory.RunStatus = !string.IsNullOrEmpty(executionContext.JobHistoryStatus)
                                                      ? executionContext.JobHistoryStatus
                                                      : (string.IsNullOrEmpty(executionContext.JobHistoryErrorLog)
                                                          ? Constants.JOB_HISTORY_SUCCESS_STATUS
                                                          : Constants.JOB_HISTORY_FAILURE_STATUS);
                        jobHistory.ErrorLog = executionContext.JobHistoryErrorLog;
                        await jobHistoryService.UpdateJobHistory(jobHistory);

                        _ll.Logger.LogInformation("{ClassName}.EnableHealthMetricProcessing - Ended processing", className);
                    }
                    catch (Exception ex)
                    {

                        _ll.Logger.LogError(ex, "{ClassName}.EnableHealthMetricProcessing - Failed processing health metrics, ErrorCode:{Code},ERROR:{Msg}",
                            className, StatusCodes.Status500InternalServerError, ex.Message);

                        //Update jobHistory status to Failure
                        jobHistory.RunStatus = Constants.JOB_HISTORY_FAILURE_STATUS;
                        jobHistory.ErrorLog = ex.Message;
                        await jobHistoryService.UpdateJobHistory(jobHistory);
                        throw;
                    }
                }

                if (executionContext.EnableEnrollment)
                {
                    try
                    {
                        _ll.Logger.LogInformation("{ClassName}.ProcessTenantEnrollments - Started processing with TenantCode:{Code}", className, executionContext.TenantCode);
                        //Update job definition in job history table
                        await jobHistoryService.UpdateJobDefinitionInJobHistory(jobHistory, JobDefinition.EXECUTE_COHORTING);

                        var enrollmentService = host.Services.GetRequiredService<IEnrollmentService>();
                        // Process enrollments and read loaded consumers and persons
                        (var loadedConsumers, var loadedPersons) = await enrollmentService.ProcessTenantEnrollments(executionContext);

                        await ProcessCohorts(executionContext, host, loadedConsumers, loadedPersons);

                        //Update jobHistory status to success
                        jobHistory.RunStatus = !string.IsNullOrEmpty(executionContext.JobHistoryStatus)
                                                      ? executionContext.JobHistoryStatus
                                                      : (string.IsNullOrEmpty(executionContext.JobHistoryErrorLog)
                                                          ? Constants.JOB_HISTORY_SUCCESS_STATUS
                                                          : Constants.JOB_HISTORY_FAILURE_STATUS);
                        jobHistory.ErrorLog = executionContext.JobHistoryErrorLog;
                        await jobHistoryService.UpdateJobHistory(jobHistory);

                        _ll.Logger.LogInformation("{ClassName}.ProcessTenantEnrollments - Ended processing with TenantCode:{Code}", className, executionContext.TenantCode);
                    }
                    catch (Exception ex)
                    {

                        _ll.Logger.LogError(ex, "{ClassName}.ProcessTenantEnrollments - Failed processing Tenant Enrollments with TenantCode:{TenantCode},ErrorCode:{Code},ERROR:{Msg}",
                            className, executionContext.TenantCode, StatusCodes.Status500InternalServerError, ex.Message);

                        //Update jobHistory status to Failure
                        jobHistory.RunStatus = Constants.JOB_HISTORY_FAILURE_STATUS;
                        jobHistory.ErrorLog = ex.Message;
                        await jobHistoryService.UpdateJobHistory(jobHistory);
                        throw;
                    }
                }

                if (executionContext.EnableMemberImport && !string.IsNullOrEmpty(executionContext.CustomerCode) && !string.IsNullOrEmpty(executionContext.CustomerLabel))
                {
                    try
                    {
                        _ll.Logger.LogInformation("{ClassName}.MemberImport - Started processing member import with TenantCode:{Code}, CustomerCode: {CustomerCode}, CustomerLabel: {CustomerLabel}",
                            className, executionContext.TenantCode, executionContext.CustomerCode, executionContext.CustomerLabel);
                        //Update job definition in job history table
                        await jobHistoryService.UpdateJobDefinitionInJobHistory(jobHistory, JobDefinition.SCAN_S3_MEMBER_IMPORT);

                        var memberImportService = host.Services.GetRequiredService<IMemberImportService>();
                        // Read loaded consumers and persons
                        (var loadedConsumers, var loadedPersons) = await memberImportService.Import(executionContext);

                        await ProcessCohorts(executionContext, host, loadedConsumers, loadedPersons);

                        //Update jobHistory status to success
                        jobHistory.RunStatus = !string.IsNullOrEmpty(executionContext.JobHistoryStatus)
                                                      ? executionContext.JobHistoryStatus
                                                      : (string.IsNullOrEmpty(executionContext.JobHistoryErrorLog)
                                                          ? Constants.JOB_HISTORY_SUCCESS_STATUS
                                                          : Constants.JOB_HISTORY_FAILURE_STATUS);
                        jobHistory.ErrorLog = executionContext.JobHistoryErrorLog;
                        await jobHistoryService.UpdateJobHistory(jobHistory);

                        _ll.Logger.LogInformation("{ClassName}.MemberImport - Ended processing member import with TenantCode:{Code}, CustomerCode: {CustomerCode}, CustomerLabel: {CustomerLabel}", className, executionContext.TenantCode, executionContext.CustomerCode, executionContext.CustomerLabel);

                    }
                    catch (Exception ex)
                    {

                        _ll.Logger.LogError(ex, "{ClassName}.MemberImport - Failed processing Member Import failed with TenantCode:{TenantCode}, CustomerCode: {CustomerCode}, CustomerLabel: {CustomerLabel}, Text file={File},ErrorCode:{Code},ERROR:{Msg}",
                            className, executionContext.TenantCode, executionContext.CustomerCode, executionContext.CustomerLabel, executionContext.MemberImportFilePath, StatusCodes.Status500InternalServerError, ex.Message);

                        //Update jobHistory status to Failure
                        jobHistory.RunStatus = Constants.JOB_HISTORY_FAILURE_STATUS;
                        jobHistory.ErrorLog = ex.Message;
                        await jobHistoryService.UpdateJobHistory(jobHistory);
                        throw;
                    }

                }

                if (!string.IsNullOrEmpty(executionContext.TaskImportFilePath) && !executionContext.EnableS3)
                {
                    try
                    {
                        _ll.Logger.LogInformation("{ClassName}.TaskImport - Started processing Task import with TenantCode:{Code},FilePath:{Path}", className, executionContext.TenantCode, executionContext.TaskImportFilePath);

                        var taskImportService = host.Services.GetRequiredService<ITaskImportService>();
                        await taskImportService.Import(executionContext.TenantCode, executionContext.TaskImportFilePath);

                        //Update jobHistory status to success
                        jobHistory.RunStatus = !string.IsNullOrEmpty(executionContext.JobHistoryStatus)
                                                      ? executionContext.JobHistoryStatus
                                                      : (string.IsNullOrEmpty(executionContext.JobHistoryErrorLog)
                                                          ? Constants.JOB_HISTORY_SUCCESS_STATUS
                                                          : Constants.JOB_HISTORY_FAILURE_STATUS);
                        jobHistory.ErrorLog = executionContext.JobHistoryErrorLog;
                        await jobHistoryService.UpdateJobHistory(jobHistory);

                        _ll.Logger.LogInformation("{ClassName}.TaskImport - Ended processing Task import with TenantCode:{Code},FilePath:{Path}", className, executionContext.TenantCode, executionContext.TaskImportFilePath);
                    }
                    catch (Exception ex)
                    {
                        _ll.Logger.LogError(ex, "{ClassName}.TaskImport - Failed processing Task import with TenantCode:{Code},FilePath:{Path},ErrorCode:{ErrorCode},ERROR:{Msg}",
                            className, executionContext.TenantCode, executionContext.TaskImportFilePath, StatusCodes.Status500InternalServerError, ex.Message);

                        //Update jobHistory status to Failure
                        jobHistory.RunStatus = Constants.JOB_HISTORY_FAILURE_STATUS;
                        jobHistory.ErrorLog = ex.Message;
                        await jobHistoryService.UpdateJobHistory(jobHistory);
                        throw;
                    }
                }

                if (!string.IsNullOrEmpty(executionContext.TriviaImportFilePath) && !executionContext.EnableS3)
                {
                    try
                    {
                        _ll.Logger.LogInformation("{ClassName}.TriviaImport - Started processing Trivia import with TenantCode:{Code},FilePath:{Path}", className, executionContext.TenantCode, executionContext.TriviaImportFilePath);
                        //Update job definition in job history table
                        await jobHistoryService.UpdateJobDefinitionInJobHistory(jobHistory, JobDefinition.IMPORT_TRIVIA);

                        var triviaImportService = host.Services.GetRequiredService<ITriviaImportService>();
                        await triviaImportService.Import(executionContext);

                        //Update jobHistory status to success
                        jobHistory.RunStatus = !string.IsNullOrEmpty(executionContext.JobHistoryStatus)
                                                      ? executionContext.JobHistoryStatus
                                                      : (string.IsNullOrEmpty(executionContext.JobHistoryErrorLog)
                                                          ? Constants.JOB_HISTORY_SUCCESS_STATUS
                                                          : Constants.JOB_HISTORY_FAILURE_STATUS);
                        jobHistory.ErrorLog = executionContext.JobHistoryErrorLog;
                        await jobHistoryService.UpdateJobHistory(jobHistory);
                        jobHistory.RunStatus = Constants.JOB_HISTORY_SUCCESS_STATUS;
                        await jobHistoryService.UpdateJobHistory(jobHistory);

                        _ll.Logger.LogInformation("{ClassName}.TriviaImport - Ended processing Trivia import with TenantCode:{Code},FilePath:{Path}", className, executionContext.TenantCode, executionContext.TriviaImportFilePath);
                    }
                    catch (Exception ex)
                    {
                        _ll.Logger.LogError(ex, "{ClassName}.TriviaImport - Failed processing Trivia import with TenantCode:{Code},FilePath:{Path},ErrorCode:{ErrorCode},ERROR:{Msg}",
                            className, executionContext.TenantCode, executionContext.TriviaImportFilePath, StatusCodes.Status500InternalServerError, ex.Message);

                        //Update jobHistory status to Failure
                        jobHistory.RunStatus = Constants.JOB_HISTORY_FAILURE_STATUS;
                        jobHistory.ErrorLog = ex.Message;
                        await jobHistoryService.UpdateJobHistory(jobHistory);
                        throw;
                    }
                }

                if (!string.IsNullOrEmpty(executionContext.QuestionnaireImportFilePath) && !executionContext.EnableS3)
                {
                    try
                    {
                        _ll.Logger.LogInformation("{ClassName}.QuestionnaireImport - Started processing Questionnaire import with TenantCode:{Code},FilePath:{Path}", 
                            className, executionContext.TenantCode, executionContext.QuestionnaireImportFilePath);
                        //Update job definition in job history table
                        await jobHistoryService.UpdateJobDefinitionInJobHistory(jobHistory, JobDefinition.IMPORT_QUESTIONNAIRE);

                        var questionnaireImportService = host.Services.GetRequiredService<IQuestionnaireImportService>();
                        await questionnaireImportService.Import(executionContext);

                        //Update jobHistory status to success
                        jobHistory.RunStatus = !string.IsNullOrEmpty(executionContext.JobHistoryStatus)
                                                      ? executionContext.JobHistoryStatus
                                                      : (string.IsNullOrEmpty(executionContext.JobHistoryErrorLog)
                                                          ? Constants.JOB_HISTORY_SUCCESS_STATUS
                                                          : Constants.JOB_HISTORY_FAILURE_STATUS);
                        jobHistory.ErrorLog = executionContext.JobHistoryErrorLog;
                        await jobHistoryService.UpdateJobHistory(jobHistory);
                        jobHistory.RunStatus = Constants.JOB_HISTORY_SUCCESS_STATUS;
                        await jobHistoryService.UpdateJobHistory(jobHistory);

                        _ll.Logger.LogInformation("{ClassName}.QuestionnaireImport - Ended processing Questionnaire import with TenantCode:{Code},FilePath:{Path}",
                            className, executionContext.TenantCode, executionContext.QuestionnaireImportFilePath);
                    }
                    catch (Exception ex)
                    {
                        _ll.Logger.LogError(ex, "{ClassName}.QuestionnaireImport - Failed processing Questionnaire import with TenantCode:{Code},FilePath:{Path},ErrorCode:{ErrorCode},ERROR:{Msg}",
                            className, executionContext.TenantCode, executionContext.QuestionnaireImportFilePath, StatusCodes.Status500InternalServerError, ex.Message);

                        //Update jobHistory status to Failure
                        jobHistory.RunStatus = Constants.JOB_HISTORY_FAILURE_STATUS;
                        jobHistory.ErrorLog = ex.Message;
                        await jobHistoryService.UpdateJobHistory(jobHistory);
                        throw;
                    }
                }

                if (executionContext.ProcessRecurringTasks)
                {
                    try
                    {
                        _ll.Logger.LogInformation("{ClassName}.ProcessRecurringTasks - Started processing ProcessRecurringTasks with TenantCode:{Code}", className, executionContext.TenantCode);
                        //Update job definition in job history table
                        await jobHistoryService.UpdateJobDefinitionInJobHistory(jobHistory, JobDefinition.PROCESS_RECURRING_CONSUMER_TASKS);

                        var taskUpdateService = host.Services.GetRequiredService<ITaskUpdateService>();
                        await taskUpdateService.ProcessRecurringTasks(executionContext);

                        //Update jobHistory status to success
                        jobHistory.RunStatus = !string.IsNullOrEmpty(executionContext.JobHistoryStatus)
                                                      ? executionContext.JobHistoryStatus
                                                      : (string.IsNullOrEmpty(executionContext.JobHistoryErrorLog)
                                                          ? Constants.JOB_HISTORY_SUCCESS_STATUS
                                                          : Constants.JOB_HISTORY_FAILURE_STATUS);
                        jobHistory.ErrorLog = executionContext.JobHistoryErrorLog;
                        await jobHistoryService.UpdateJobHistory(jobHistory);

                        _ll.Logger.LogInformation("{ClassName}.ProcessRecurringTasks - Ended processing ProcessRecurringTasks with TenantCode:{Code}", className, executionContext.TenantCode);
                    }
                    catch (Exception ex)
                    {
                        _ll.Logger.LogError(ex, "{ClassName}.ProcessRecurringTasks - Failed processing ProcessRecurringTasks with TenantCode:{Code},ErrorCode:{ErrorCode},ERROR:{Msg}", className, executionContext.TenantCode, StatusCodes.Status500InternalServerError, ex.Message);

                        //Update jobHistory status to Failure
                        jobHistory.RunStatus = Constants.JOB_HISTORY_FAILURE_STATUS;
                        jobHistory.ErrorLog = ex.Message;
                        await jobHistoryService.UpdateJobHistory(jobHistory);
                        throw;
                    }

                }

                if (executionContext.ClearWalletEntries)
                {
                    try
                    {
                        _ll.Logger.LogInformation("{ClassName}.ClearWalletEntries - Started processing ClearWalletEntries with TenantCode:{Code}", className, executionContext.TenantCode);
                        //Update job definition in job history table
                        await jobHistoryService.UpdateJobDefinitionInJobHistory(jobHistory, JobDefinition.CLEAR_WALLET_ENTRIES);

                        var walletService = host.Services.GetRequiredService<IWalletService>();
                        await walletService.ClearEntriesWallet(executionContext?.TenantCode);

                        //Update jobHistory status to success
                        jobHistory.RunStatus = !string.IsNullOrEmpty(executionContext.JobHistoryStatus)
                                                      ? executionContext.JobHistoryStatus
                                                      : (string.IsNullOrEmpty(executionContext.JobHistoryErrorLog)
                                                          ? Constants.JOB_HISTORY_SUCCESS_STATUS
                                                          : Constants.JOB_HISTORY_FAILURE_STATUS);
                        jobHistory.ErrorLog = executionContext.JobHistoryErrorLog;
                        await jobHistoryService.UpdateJobHistory(jobHistory);

                        _ll.Logger.LogInformation("{ClassName}.ClearWalletEntries - Ended processing ClearWalletEntries with TenantCode:{Code}", className, executionContext.TenantCode);
                    }
                    catch (Exception ex)
                    {
                        _ll.Logger.LogError(ex, "{ClassName}.ClearWalletEntries - Failed processing ClearWalletEntries with TenantCode:{Code},ErrorCode:{ErrorCode},ERROR:{Msg}", className, executionContext.TenantCode, StatusCodes.Status500InternalServerError, ex.Message);

                        //Update jobHistory status to Failure
                        jobHistory.RunStatus = Constants.JOB_HISTORY_FAILURE_STATUS;
                        jobHistory.ErrorLog = ex.Message;
                        await jobHistoryService.UpdateJobHistory(jobHistory);
                        throw;
                    }

                }

                if (executionContext != null && executionContext.RedeemHSA && !string.IsNullOrEmpty(executionContext.RedeemConsumerListFilePath) && !string.IsNullOrEmpty(executionContext.LocalDownloadFolderPath))
                {
                    try
                    {
                        _ll.Logger.LogInformation("{ClassName}.RedeemHSA - Started processing RedeemHSA with TenantCode:{Code},FilePath:{Path}", className, executionContext.TenantCode, executionContext.RedeemConsumerListFilePath);
                        //Update job definition in job history table
                        await jobHistoryService.UpdateJobDefinitionInJobHistory(jobHistory, JobDefinition.SCAN_S3_HSA_SWEEPER);

                        var walletService = host.Services.GetRequiredService<IWalletService>();
                        await walletService.RedeemHSA(executionContext);

                        //Update jobHistory status to success
                        jobHistory.RunStatus = !string.IsNullOrEmpty(executionContext.JobHistoryStatus)
                                                      ? executionContext.JobHistoryStatus
                                                      : (string.IsNullOrEmpty(executionContext.JobHistoryErrorLog)
                                                          ? Constants.JOB_HISTORY_SUCCESS_STATUS
                                                          : Constants.JOB_HISTORY_FAILURE_STATUS);
                        jobHistory.ErrorLog = executionContext.JobHistoryErrorLog;
                        await jobHistoryService.UpdateJobHistory(jobHistory);

                        _ll.Logger.LogInformation("{ClassName}.RedeemHSA - Ended processing RedeemHSA with TenantCode:{Code},FilePath:{Path}", className, executionContext.TenantCode, executionContext.RedeemConsumerListFilePath);
                    }
                    catch (Exception ex)
                    {
                        _ll.Logger.LogError(ex, "{ClassName}.RedeemHSA - Failed processing RedeemHSA with TenantCode:{Code},FilePath:{Path},ErrorCode:{ErrorCode},ERROR:{Msg}",
                            className, executionContext.TenantCode, executionContext.RedeemConsumerListFilePath, StatusCodes.Status500InternalServerError, ex.Message);

                        //Update jobHistory status to Failure
                        jobHistory.RunStatus = Constants.JOB_HISTORY_FAILURE_STATUS;
                        jobHistory.ErrorLog = ex.Message;
                        await jobHistoryService.UpdateJobHistory(jobHistory);
                        throw;
                    }
                }

                // Generate wallet balances report CSV file with tab delimiter
                if (executionContext != null && executionContext.GenerateWalletBalancesReport && !string.IsNullOrEmpty(executionContext.TenantCode) && !string.IsNullOrEmpty(executionContext.WalletTypeCode))
                {
                    try
                    {
                        _ll.Logger.LogInformation("{ClassName}.GenerateWalletBalancesReport - Started processing GenerateWalletBalancesReport with TenantCode:{Code},WalletTypeCode:{TypeCode}", className, executionContext.TenantCode, executionContext.WalletTypeCode);

                        //Update job definition in job history table
                        await jobHistoryService.UpdateJobDefinitionInJobHistory(jobHistory, JobDefinition.GENERATE_WALLET_BALANCES_REPORT);

                        var walletService = host.Services.GetRequiredService<IWalletService>();
                        await walletService.GenerateWalletBalancesReport(executionContext);

                        //Update jobHistory status to success
                        jobHistory.RunStatus = !string.IsNullOrEmpty(executionContext.JobHistoryStatus)
                                                      ? executionContext.JobHistoryStatus
                                                      : (string.IsNullOrEmpty(executionContext.JobHistoryErrorLog)
                                                          ? Constants.JOB_HISTORY_SUCCESS_STATUS
                                                          : Constants.JOB_HISTORY_FAILURE_STATUS);
                        jobHistory.ErrorLog = executionContext.JobHistoryErrorLog;
                        await jobHistoryService.UpdateJobHistory(jobHistory);

                        _ll.Logger.LogInformation("{ClassName}.GenerateWalletBalancesReport - Ended processing GenerateWalletBalancesReport with TenantCode:{Code},WalletTypeCode:{TypeCode}", className, executionContext.TenantCode, executionContext.WalletTypeCode);
                    }
                    catch (Exception ex)
                    {
                        _ll.Logger.LogError(ex, "{ClassName}.GenerateWalletBalancesReport - Failed processing GenerateWalletBalancesReport with TenantCode:{Code},WalletTypeCode:{TypeCode},ErrorCode:{ErrorCode},ERROR:{Msg}",
                            className, executionContext.TenantCode, executionContext.WalletTypeCode, StatusCodes.Status500InternalServerError, ex.Message);

                        //Update jobHistory status to Failure
                        jobHistory.RunStatus = Constants.JOB_HISTORY_FAILURE_STATUS;
                        jobHistory.ErrorLog = ex.Message;
                        await jobHistoryService.UpdateJobHistory(jobHistory);
                        throw;
                    }
                }

                // generate card create file
                if (executionContext != null && executionContext.FISCreateCards)
                {
                    try
                    {
                        _ll.Logger.LogInformation("{ClassName}.GenerateCreateCardFile - Started processing GenerateCreateCardFile with TenantCode:{Code}", className, executionContext.TenantCode);
                        //Update job definition in job history table
                        await jobHistoryService.UpdateJobDefinitionInJobHistory(jobHistory, JobDefinition.CREATE_CARD_30);

                        var cardBatchFileCreateService = host.Services.GetRequiredService<ICardBatchFileCreateService>();

                        switch (executionContext.BatchActionType.ToUpper().Trim())
                        {
                            case var action when action == BatchActions.GENERATE.ToString():
                                await cardBatchFileCreateService.GenerateCardCreateFile(executionContext);
                                break;

                            case var action when action == BatchActions.ENCRYPT.ToString():
                                await cardBatchFileCreateService.EncryptFile(executionContext);
                                break;

                            case var action when action == BatchActions.COPY.ToString():
                                await cardBatchFileCreateService.CopyFileToDestination(executionContext);
                                break;

                            case var action when action == BatchActions.ARCHIVE.ToString():
                                await cardBatchFileCreateService.ArchiveFile(executionContext);
                                break;

                            case var action when action == BatchActions.DELETE.ToString():
                                await cardBatchFileCreateService.DeleteFile(executionContext);
                                break;

                            case var action when action == BatchActions.ALL.ToString():
                                await cardBatchFileCreateService.GenerateCardCreateFile(executionContext);
                                await cardBatchFileCreateService.EncryptFile(executionContext);
                                await cardBatchFileCreateService.CopyFileToDestination(executionContext);
                                await cardBatchFileCreateService.ArchiveFile(executionContext);
                                await cardBatchFileCreateService.DeleteFile(executionContext);
                                break;

                            default:
                                _ll.Logger.LogWarning("Unrecognized BatchActionType: {BatchActionType}", executionContext.BatchActionType);
                                break;
                        }

                        //Update jobHistory status to success
                        jobHistory.RunStatus = !string.IsNullOrEmpty(executionContext.JobHistoryStatus)
                                                      ? executionContext.JobHistoryStatus
                                                      : (string.IsNullOrEmpty(executionContext.JobHistoryErrorLog)
                                                          ? Constants.JOB_HISTORY_SUCCESS_STATUS
                                                          : Constants.JOB_HISTORY_FAILURE_STATUS);
                        jobHistory.ErrorLog = executionContext.JobHistoryErrorLog;
                        await jobHistoryService.UpdateJobHistory(jobHistory);

                        _ll.Logger.LogInformation("{ClassName}.GenerateCreateCardFile - Ended processing GenerateCreateCardFile with TenantCode:{Code}", className, executionContext.TenantCode);
                    }
                    catch (Exception ex)
                    {
                        _ll.Logger.LogError(ex, "{ClassName}.GenerateCreateCardFile - Failed processing GenerateCreateCardFile with TenantCode:{Code},ErrorCode:{ErrorCode},ERROR:{Msg}",
                            className, executionContext.TenantCode, StatusCodes.Status500InternalServerError, ex.Message);

                        //Update jobHistory status to Failure
                        jobHistory.RunStatus = Constants.JOB_HISTORY_FAILURE_STATUS;
                        jobHistory.ErrorLog = ex.Message;
                        await jobHistoryService.UpdateJobHistory(jobHistory);
                        throw;
                    }
                }
                // updatefisuserinfo
                if (executionContext != null && executionContext.IsUpdateUserInfoInFIS)
                {
                    try
                    {
                        _ll.Logger.LogInformation("{ClassName}.UpdateFISUserInfo - Started processing UpdateFISUserInfo with TenantCode:{Code}", className, executionContext.TenantCode);
                        //Update job definition in job history table
                        await jobHistoryService.UpdateJobDefinitionInJobHistory(jobHistory, JobDefinition.UPDATE_USER_INFO_FIS);

                        var cardBatchFileCreateService = host.Services.GetRequiredService<ICardBatchFileCreateService>();
                        switch (executionContext.BatchActionType.ToUpper().Trim())
                        {
                            case var action when action == BatchActions.GENERATE.ToString():
                                await cardBatchFileCreateService.GenerateCardCreateFile(executionContext);
                                break;

                            case var action when action == BatchActions.ENCRYPT.ToString():
                                await cardBatchFileCreateService.EncryptFile(executionContext);
                                break;

                            case var action when action == BatchActions.COPY.ToString():
                                await cardBatchFileCreateService.CopyFileToDestination(executionContext);
                                break;

                            case var action when action == BatchActions.ARCHIVE.ToString():
                                await cardBatchFileCreateService.ArchiveFile(executionContext);
                                break;

                            case var action when action == BatchActions.DELETE.ToString():
                                await cardBatchFileCreateService.DeleteFile(executionContext);
                                break;

                            case var action when action == BatchActions.ALL.ToString():
                                await cardBatchFileCreateService.GenerateCardCreateFile(executionContext);
                                await cardBatchFileCreateService.EncryptFile(executionContext);
                                await cardBatchFileCreateService.CopyFileToDestination(executionContext);
                                await cardBatchFileCreateService.ArchiveFile(executionContext);
                                await cardBatchFileCreateService.DeleteFile(executionContext);
                                break;

                            default:
                                _ll.Logger.LogWarning("Unrecognized BatchActionType: {BatchActionType}", executionContext.BatchActionType);
                                break;
                        }

                        //Update jobHistory status to success
                        jobHistory.RunStatus = !string.IsNullOrEmpty(executionContext.JobHistoryStatus)
                                                      ? executionContext.JobHistoryStatus
                                                      : (string.IsNullOrEmpty(executionContext.JobHistoryErrorLog)
                                                          ? Constants.JOB_HISTORY_SUCCESS_STATUS
                                                          : Constants.JOB_HISTORY_FAILURE_STATUS);
                        jobHistory.ErrorLog = executionContext.JobHistoryErrorLog;
                        await jobHistoryService.UpdateJobHistory(jobHistory);

                        _ll.Logger.LogInformation("{ClassName}.UpdateFISUserInfo - Ended processing UpdateFISUserInfo with TenantCode:{Code}", className, executionContext.TenantCode);
                    }
                    catch (Exception ex)
                    {
                        _ll.Logger.LogError(ex, "{ClassName}.UpdateFISUserInfo - Failed processing UpdateFISUserInfo with TenantCode:{Code},ErrorCode:{ErrorCode},ERROR:{Msg}",
                             className, executionContext.TenantCode, StatusCodes.Status500InternalServerError, ex.Message);

                        //Update jobHistory status to Failure
                        jobHistory.RunStatus = Constants.JOB_HISTORY_FAILURE_STATUS;
                        jobHistory.ErrorLog = ex.Message;
                        await jobHistoryService.UpdateJobHistory(jobHistory);
                        throw;
                    }
                }

                if (executionContext != null && executionContext.ProcessRetailProductSyncQueue)
                {
                    try
                    {
                        _ll.Logger.LogInformation("{ClassName}.ProcessRetailProductSync - Started processing ProcessRetailProductSync with TenantCode:{Code}", className, executionContext.TenantCode);
                        //Update job definition in job history table
                        await jobHistoryService.UpdateJobDefinitionInJobHistory(jobHistory, JobDefinition.PROCESS_RETAIL_PRODUCT_SYNC);

                        var retailProductSyncService = host.Services.GetRequiredService<IRetailProductSyncService>();
                        switch (executionContext.BatchActionType.ToUpper().Trim())
                        {
                            case var action when action == BatchActions.GENERATE.ToString():
                                await retailProductSyncService.ProcessQueueMessages(executionContext);
                                break;

                            case var action when action == BatchActions.ENCRYPT.ToString():
                                await retailProductSyncService.EncryptFile(executionContext);
                                break;

                            case var action when action == BatchActions.COPY.ToString():
                                await retailProductSyncService.CopyFileToDestination(executionContext);
                                break;

                            case var action when action == BatchActions.ARCHIVE.ToString():
                                await retailProductSyncService.ArchiveFile(executionContext);
                                break;

                            case var action when action == BatchActions.DELETE.ToString():
                                await retailProductSyncService.DeleteFile(executionContext);
                                break;

                            case var action when action == BatchActions.ALL.ToString():
                                await retailProductSyncService.ProcessQueueMessages(executionContext);
                                await retailProductSyncService.EncryptFile(executionContext);
                                await retailProductSyncService.CopyFileToDestination(executionContext);
                                await retailProductSyncService.ArchiveFile(executionContext);
                                await retailProductSyncService.DeleteFile(executionContext);
                                break;

                            default:
                                _ll.Logger.LogWarning("Unrecognized BatchActionType: {BatchActionType}", executionContext.BatchActionType);
                                break;
                        }

                        //Update jobHistory status to success
                        jobHistory.RunStatus = !string.IsNullOrEmpty(executionContext.JobHistoryStatus)
                                                      ? executionContext.JobHistoryStatus
                                                      : (string.IsNullOrEmpty(executionContext.JobHistoryErrorLog)
                                                          ? Constants.JOB_HISTORY_SUCCESS_STATUS
                                                          : Constants.JOB_HISTORY_FAILURE_STATUS);
                        jobHistory.ErrorLog = executionContext.JobHistoryErrorLog;
                        await jobHistoryService.UpdateJobHistory(jobHistory);

                        _ll.Logger.LogInformation("{ClassName}.ProcessRetailProductSync - Ended processing ProcessRetailProductSync with TenantCode:{Code}", className, executionContext.TenantCode);
                        // For testing 
                        // await retailProductSyncService.DecryptAndSaveToLocalPath(executionContext,"C:/tmp");
                    }
                    catch (Exception ex)
                    {
                        _ll.Logger.LogError(ex, "{ClassName}.ProcessRetailProductSync - Failed processing ProcessRetailProductSync with TenantCode:{Code},ErrorCode:{ErrorCode},ERROR:{Msg}",
                            className, executionContext.TenantCode, StatusCodes.Status500InternalServerError, ex.Message);

                        //Update jobHistory status to Failure
                        jobHistory.RunStatus = Constants.JOB_HISTORY_FAILURE_STATUS;
                        jobHistory.ErrorLog = ex.Message;
                        await jobHistoryService.UpdateJobHistory(jobHistory);
                        throw;
                    }
                }

                if (executionContext != null && executionContext.ExecuteBenefitsFunding)
                {
                    try
                    {
                        _ll.Logger.LogInformation("{ClassName}.ExecuteBenefitsFunding - Started processing ExecuteBenefitsFunding with TenantCode:{Code}", className, executionContext.TenantCode);
                        //Update job definition in job history table
                        var jobDefinitionName = string.IsNullOrEmpty(executionContext.ConsumerListFile)
                            ? JobDefinition.EXECUTE_BENEFITS_FUNDING
                            : JobDefinition.EXECUTE_BENEFITS_FUNDING_CONSUMER_LIST;
                        await jobHistoryService.UpdateJobDefinitionInJobHistory(jobHistory, jobDefinitionName);

                        var benefitsFundingService = host.Services.GetRequiredService<IBenefitsFundingService>();
                        await benefitsFundingService.ExecuteFundingRules(executionContext);

                        //Update jobHistory status to success
                        jobHistory.RunStatus = !string.IsNullOrEmpty(executionContext.JobHistoryStatus)
                                                      ? executionContext.JobHistoryStatus
                                                      : (string.IsNullOrEmpty(executionContext.JobHistoryErrorLog)
                                                          ? Constants.JOB_HISTORY_SUCCESS_STATUS
                                                          : Constants.JOB_HISTORY_FAILURE_STATUS);
                        jobHistory.ErrorLog = executionContext.JobHistoryErrorLog;
                        await jobHistoryService.UpdateJobHistory(jobHistory);

                        _ll.Logger.LogInformation("{ClassName}.ExecuteBenefitsFunding - Ended processing ExecuteBenefitsFunding with TenantCode:{Code}", className, executionContext.TenantCode);
                    }
                    catch (Exception ex)
                    {
                        _ll.Logger.LogError(ex, "{ClassName}.ExecuteBenefitsFunding - Failed processing ExecuteBenefitsFunding with TenantCode:{Code},ErrorCode:{ErrorCode},ERROR:{Msg}",
                            className, executionContext.TenantCode, StatusCodes.Status500InternalServerError, ex.Message);

                        //Update jobHistory status to Failure
                        jobHistory.RunStatus = Constants.JOB_HISTORY_FAILURE_STATUS;
                        jobHistory.ErrorLog = ex.Message;
                        await jobHistoryService.UpdateJobHistory(jobHistory);
                        throw;
                    }
                }
                // generate card load file
                if (executionContext != null && executionContext.GenerateCardLoad)
                {
                    try
                    {
                        _ll.Logger.LogInformation("{ClassName}.GenerateCardLoad - Started processing GenerateCardLoad with TenantCode:{Code}", className, executionContext.TenantCode);
                        //Update job definition in job history table
                        var jobDefinitionName = string.IsNullOrEmpty(executionContext.ConsumerListFile)
                            ? JobDefinition.VALUE_LOAD
                            : JobDefinition.VALUE_LOAD_CONSUMER_LIST;
                        await jobHistoryService.UpdateJobDefinitionInJobHistory(jobHistory, jobDefinitionName);

                        var cardDisbursementService = host.Services.GetRequiredService<ICardDisbursementFileCreateService>();
                        switch (executionContext.BatchActionType.ToUpper().Trim())
                        {
                            case var action when action == BatchActions.GENERATE.ToString():
                                await cardDisbursementService.GenerateCardLoadFile(executionContext);
                                break;

                            case var action when action == BatchActions.ENCRYPT.ToString():
                                await cardDisbursementService.EncryptFile(executionContext);
                                break;

                            case var action when action == BatchActions.COPY.ToString():
                                await cardDisbursementService.CopyFileToDestination(executionContext);
                                break;

                            case var action when action == BatchActions.ARCHIVE.ToString():
                                await cardDisbursementService.ArchiveFile(executionContext);
                                break;

                            case var action when action == BatchActions.DELETE.ToString():
                                await cardDisbursementService.DeleteFile(executionContext);
                                break;

                            case var action when action == BatchActions.ALL.ToString():
                                await cardDisbursementService.GenerateCardLoadFile(executionContext);
                                await cardDisbursementService.EncryptFile(executionContext);
                                await cardDisbursementService.CopyFileToDestination(executionContext);
                                await cardDisbursementService.ArchiveFile(executionContext);
                                await cardDisbursementService.DeleteFile(executionContext);
                                break;

                            default:
                                _ll.Logger.LogWarning("Unrecognized BatchActionType: {BatchActionType}", executionContext.BatchActionType);
                                break;
                        }

                        //Update jobHistory status to success
                        jobHistory.RunStatus = !string.IsNullOrEmpty(executionContext.JobHistoryStatus)
                                                      ? executionContext.JobHistoryStatus
                                                      : (string.IsNullOrEmpty(executionContext.JobHistoryErrorLog)
                                                          ? Constants.JOB_HISTORY_SUCCESS_STATUS
                                                          : Constants.JOB_HISTORY_FAILURE_STATUS);
                        jobHistory.ErrorLog = executionContext.JobHistoryErrorLog;
                        await jobHistoryService.UpdateJobHistory(jobHistory);

                        _ll.Logger.LogInformation("{ClassName}.GenerateCardLoad - Ended processing GenerateCardLoad with TenantCode:{Code}", className, executionContext.TenantCode);
                    }
                    catch (Exception ex)
                    {
                        _ll.Logger.LogError(ex, "{ClassName}.GenerateCardLoad - Failed processing GenerateCardLoad with TenantCode:{Code},ErrorCode:{ErrorCode},ERROR:{Msg}",
                            className, executionContext.TenantCode, StatusCodes.Status500InternalServerError, ex.Message);

                        //Update jobHistory status to Failure
                        jobHistory.RunStatus = Constants.JOB_HISTORY_FAILURE_STATUS;
                        jobHistory.ErrorLog = ex.Message;
                        await jobHistoryService.UpdateJobHistory(jobHistory);
                        throw;
                    }
                }


                if (executionContext != null && executionContext.FIS30RecordFileLoad)
                {
                    try
                    {
                        _ll.Logger.LogInformation("{ClassName}.FIS30RecordFileLoad - Started processing FIS30RecordFileLoad with TenantCode:{Code}", className, executionContext.TenantCode);
                        //Update job definition in job history table
                        await jobHistoryService.UpdateJobDefinitionInJobHistory(jobHistory, JobDefinition.PROCESS_CARD_30_RESPONSE);

                        var cardBatchFileReadService = host.Services.GetRequiredService<ICardBatchFileReadService>();
                        await cardBatchFileReadService.CardBatchFileReadAsync(executionContext);

                        //Update jobHistory status to success
                        jobHistory.RunStatus = !string.IsNullOrEmpty(executionContext.JobHistoryStatus)
                                                      ? executionContext.JobHistoryStatus
                                                      : (string.IsNullOrEmpty(executionContext.JobHistoryErrorLog)
                                                          ? Constants.JOB_HISTORY_SUCCESS_STATUS
                                                          : Constants.JOB_HISTORY_FAILURE_STATUS);
                        jobHistory.ErrorLog = executionContext.JobHistoryErrorLog;
                        await jobHistoryService.UpdateJobHistory(jobHistory);

                        _ll.Logger.LogInformation("{ClassName}.FIS30RecordFileLoad - Ended processing FIS30RecordFileLoad with TenantCode:{Code}", className, executionContext.TenantCode);
                    }
                    catch (Exception ex)
                    {
                        _ll.Logger.LogError(ex, "{ClassName}.FIS30RecordFileLoad - Failed processing FIS30RecordFileLoad with TenantCode:{Code},ErrorCode:{ErrorCode},ERROR:{Msg}",
                            className, executionContext.TenantCode, StatusCodes.Status500InternalServerError, ex.Message);

                        //Update jobHistory status to Failure
                        jobHistory.RunStatus = Constants.JOB_HISTORY_FAILURE_STATUS;
                        jobHistory.ErrorLog = ex.Message;
                        await jobHistoryService.UpdateJobHistory(jobHistory);
                        throw;
                    }
                }

                if (executionContext != null && executionContext.ConsumeSweepstakesWinnerReport && executionContext.SweepstakesInstanceId > 0 && !string.IsNullOrEmpty(executionContext.Format))
                {
                    try
                    {
                        _ll.Logger.LogInformation("{ClassName}.ConsumeSweepstakesWinnerReport - Started processing ConsumeSweepstakesWinnerReport with SweepstakesInstanceId:{Id}", className, executionContext.SweepstakesInstanceId);
                        //Update job definition in job history table
                        await jobHistoryService.UpdateJobDefinitionInJobHistory(jobHistory, JobDefinition.SWEEPSTAKES_WINNER_REPORT);

                        var sweepstakesConsumerService = host.Services.GetRequiredService<ISweepstakesConsumerService>();
                        await sweepstakesConsumerService.ConsumeSweepstakesWinnerReportAsync(executionContext);

                        //Update jobHistory status to success
                        jobHistory.RunStatus = !string.IsNullOrEmpty(executionContext.JobHistoryStatus)
                                                      ? executionContext.JobHistoryStatus
                                                      : (string.IsNullOrEmpty(executionContext.JobHistoryErrorLog)
                                                          ? Constants.JOB_HISTORY_SUCCESS_STATUS
                                                          : Constants.JOB_HISTORY_FAILURE_STATUS);
                        jobHistory.ErrorLog = executionContext.JobHistoryErrorLog;
                        await jobHistoryService.UpdateJobHistory(jobHistory);

                        _ll.Logger.LogInformation("{ClassName}.ConsumeSweepstakesWinnerReport - Ended processing ConsumeSweepstakesWinnerReport with SweepstakesInstanceId:{Id}", className, executionContext.SweepstakesInstanceId);
                    }
                    catch (Exception ex)
                    {
                        _ll.Logger.LogError(ex, "{ClassName}.ConsumeSweepstakesWinnerReport - Failed processing ConsumeSweepstakesWinnerReport with SweepstakesInstanceId:{Id},ErrorCode:{ErrorCode},ERROR:{Msg}",
                             className, executionContext.SweepstakesInstanceId, StatusCodes.Status500InternalServerError, ex.Message);

                        //Update jobHistory status to Failure
                        jobHistory.RunStatus = Constants.JOB_HISTORY_FAILURE_STATUS;
                        jobHistory.ErrorLog = ex.Message;
                        await jobHistoryService.UpdateJobHistory(jobHistory);
                        throw;
                    }
                }

                if (executionContext != null && executionContext.FIS60RecordFileLoad)
                {
                    try
                    {
                        _ll.Logger.LogInformation("{ClassName}.FIS60RecordFileLoad - Started processing FIS60RecordFileLoad with File:{File}", className, executionContext.FISRecordFileName);
                        //Update job definition in job history table
                        await jobHistoryService.UpdateJobDefinitionInJobHistory(jobHistory, JobDefinition.PROCESS_CARD_60_RESPONSE);

                        var cardBatchFileReadService = host.Services.GetRequiredService<ICardBatchFileReadService>();
                        await cardBatchFileReadService.CardBatchFileReadAsync(executionContext);

                        //Update jobHistory status to success
                        jobHistory.RunStatus = !string.IsNullOrEmpty(executionContext.JobHistoryStatus)
                                                      ? executionContext.JobHistoryStatus
                                                      : (string.IsNullOrEmpty(executionContext.JobHistoryErrorLog)
                                                          ? Constants.JOB_HISTORY_SUCCESS_STATUS
                                                          : Constants.JOB_HISTORY_FAILURE_STATUS);
                        jobHistory.ErrorLog = executionContext.JobHistoryErrorLog;
                        await jobHistoryService.UpdateJobHistory(jobHistory);

                        _ll.Logger.LogInformation("{ClassName}.FIS60RecordFileLoad - Ended processing FIS60RecordFileLoad with TenantCode:{Code}", className, executionContext.TenantCode);
                    }
                    catch (Exception ex)
                    {
                        _ll.Logger.LogError(ex, "{ClassName}.FIS60RecordFileLoad - Failed processing FIS60RecordFileLoad with TenantCode:{Code},ErrorCode:{ErrorCode},ERROR:{Msg}",
                            className, executionContext.TenantCode, StatusCodes.Status500InternalServerError, ex.Message);

                        //Update jobHistory status to Failure
                        jobHistory.RunStatus = Constants.JOB_HISTORY_FAILURE_STATUS;
                        jobHistory.ErrorLog = ex.Message;
                        await jobHistoryService.UpdateJobHistory(jobHistory);
                        throw;
                    }
                }

                if (executionContext != null && executionContext.ProcessMonetaryTransactionsBatchFile &&
                    !string.IsNullOrEmpty(executionContext.FISMonetaryTransactionsFileName))
                {
                    try
                    {
                        _ll.Logger.LogInformation("{ClassName}.ProcessMonetaryTransactionsBatchFile - Started processing ProcessMonetaryTransactionsBatchFile with TenantCode:{Code},FileName:{Name}", className, executionContext.TenantCode, executionContext.FISMonetaryTransactionsFileName);
                        //Update job definition in job history table
                        await jobHistoryService.UpdateJobDefinitionInJobHistory(jobHistory, JobDefinition.MONETARY_TXN);

                        var MonetaryTransactionsFileReadService = host.Services.GetRequiredService<IMonetaryTransactionsFileReadService>();
                        await MonetaryTransactionsFileReadService.ImportMonetaryTransactions(executionContext);

                        //Update jobHistory status to success
                        jobHistory.RunStatus = !string.IsNullOrEmpty(executionContext.JobHistoryStatus)
                                                      ? executionContext.JobHistoryStatus
                                                      : (string.IsNullOrEmpty(executionContext.JobHistoryErrorLog)
                                                          ? Constants.JOB_HISTORY_SUCCESS_STATUS
                                                          : Constants.JOB_HISTORY_FAILURE_STATUS);
                        jobHistory.ErrorLog = executionContext.JobHistoryErrorLog;
                        jobHistory.TenantCode = executionContext.TenantCode;
                        await jobHistoryService.UpdateJobHistory(jobHistory);

                        _ll.Logger.LogInformation("{ClassName}.ProcessMonetaryTransactionsBatchFile - Ended processing ProcessMonetaryTransactionsBatchFile with TenantCode:{Code},FileName:{Name}", className, executionContext.TenantCode, executionContext.FISMonetaryTransactionsFileName);
                    }
                    catch (Exception ex)
                    {
                        _ll.Logger.LogError(ex, "{ClassName}.ProcessMonetaryTransactionsBatchFile - Failed processing ProcessMonetaryTransactionsBatchFile with TenantCode:{Code},FileName:{Name},ErrorCode:{ErrorCode},ERROR:{Msg}",
                            className, executionContext.TenantCode, executionContext.FISMonetaryTransactionsFileName, StatusCodes.Status500InternalServerError, ex.Message);

                        //Update jobHistory status to Failure
                        jobHistory.RunStatus = Constants.JOB_HISTORY_FAILURE_STATUS;
                        jobHistory.ErrorLog = ex.Message;
                        await jobHistoryService.UpdateJobHistory(jobHistory);
                        throw;
                    }
                }
                if (executionContext != null && executionContext.ConsumerNonMonetaryTransactionsBatchFile &&
                    !string.IsNullOrEmpty(executionContext.ConsumerNonMonetaryTransactionsFileName))
                {
                    try
                    {
                        _ll.Logger.LogInformation("{ClassName}.ConsumerNonMonetaryTransactionsBatchFile - Started processing ConsumerNonMonetaryTransactionsBatch with TenantCode:{Code},FilePath:{Path}", className, executionContext.TenantCode, executionContext.ConsumerNonMonetaryTransactionsFileName);
                        //Update job definition in job history table
                        await jobHistoryService.UpdateJobDefinitionInJobHistory(jobHistory, JobDefinition.NON_MONETARY_TXN);

                        var NonMonetaryTransactionsFileReadService = host.Services.GetRequiredService<IConsumerNonMonetaryTransactionsFileReadService>();
                        await NonMonetaryTransactionsFileReadService.ImportConsumerNonMonetaryTransactions(executionContext);

                        //Update jobHistory status to success
                        jobHistory.RunStatus = !string.IsNullOrEmpty(executionContext.JobHistoryStatus)
                                                      ? executionContext.JobHistoryStatus
                                                      : (string.IsNullOrEmpty(executionContext.JobHistoryErrorLog)
                                                          ? Constants.JOB_HISTORY_SUCCESS_STATUS
                                                          : Constants.JOB_HISTORY_FAILURE_STATUS);
                        jobHistory.ErrorLog = executionContext.JobHistoryErrorLog;
                        jobHistory.TenantCode = executionContext.TenantCode;
                        await jobHistoryService.UpdateJobHistory(jobHistory);

                        _ll.Logger.LogInformation("{ClassName}.ConsumerNonMonetaryTransactionsBatchFile - Ended processing ConsumerNonMonetaryTransactionsBatch with TenantCode:{Code},FilePath:{Path}", className, executionContext.TenantCode, executionContext.ConsumerNonMonetaryTransactionsFileName);
                    }
                    catch (Exception ex)
                    {
                        _ll.Logger.LogError(ex, "{ClassName}.ConsumerNonMonetaryTransactionsBatchFile - Failed processing ConsumerNonMonetaryTransactionsBatch with TenantCode:{Code},FilePath:{Path},ErrorCode:{ErrorCode},ERROR:{Msg}",
                            className, executionContext.TenantCode, executionContext.ConsumerNonMonetaryTransactionsFileName, StatusCodes.Status500InternalServerError, ex.Message);

                        //Update jobHistory status to Failure
                        jobHistory.RunStatus = Constants.JOB_HISTORY_FAILURE_STATUS;
                        jobHistory.ErrorLog = ex.Message;
                        await jobHistoryService.UpdateJobHistory(jobHistory);
                        throw;
                    }
                }
                if (executionContext != null && executionContext.PerformExternalTxnSync)
                {
                    try
                    {
                        _ll.Logger.LogInformation("{ClassName}.PerformExternalTxnSync - Started processing PerformExternalTxnSync with TenantCode:{Code}", className, executionContext.TenantCode);
                        //Update job definition in job history table
                        await jobHistoryService.UpdateJobDefinitionInJobHistory(jobHistory, JobDefinition.EXTERNAL_TXN_SYNC);

                        var PerformExternalTxnSyncService = host.Services.GetRequiredService<IPerformExternalTxnSyncService>();
                        await PerformExternalTxnSyncService.PerformExternalTxnSync(executionContext);

                        //Update jobHistory status to success
                        jobHistory.RunStatus = !string.IsNullOrEmpty(executionContext.JobHistoryStatus)
                                                      ? executionContext.JobHistoryStatus
                                                      : (string.IsNullOrEmpty(executionContext.JobHistoryErrorLog)
                                                          ? Constants.JOB_HISTORY_SUCCESS_STATUS
                                                          : Constants.JOB_HISTORY_FAILURE_STATUS);
                        jobHistory.ErrorLog = executionContext.JobHistoryErrorLog;
                        await jobHistoryService.UpdateJobHistory(jobHistory);

                        _ll.Logger.LogInformation("{ClassName}.PerformExternalTxnSync - Ended processing PerformExternalTxnSync with TenantCode:{Code}", className, executionContext.TenantCode);
                    }
                    catch (Exception ex)
                    {
                        _ll.Logger.LogError(ex, "{ClassName}.PerformExternalTxnSync - Failed processing PerformExternalTxnSync with TenantCode:{Code},ErrorCode:{ErrorCode},ERROR:{Msg}",
                            className, executionContext.TenantCode, StatusCodes.Status500InternalServerError, ex.Message);

                        //Update jobHistory status to Failure
                        jobHistory.RunStatus = Constants.JOB_HISTORY_FAILURE_STATUS;
                        jobHistory.ErrorLog = ex.Message;
                        await jobHistoryService.UpdateJobHistory(jobHistory);
                        throw;
                    }
                }

                if (executionContext != null && executionContext.ProcessHealthTask && !string.IsNullOrEmpty(executionContext?.TenantCode) && executionContext.RollupPeriodTypeName != null && executionContext.Year > 0 && executionContext.Month > 0)
                {
                    try
                    {
                        _ll.Logger.LogInformation("{ClassName}.ProcessHealthTask - Started processing ProcessHealthTask with TenantCode:{Code},TaskId:{Id}", className, executionContext.TenantCode, executionContext.TaskId);
                        //Update job definition in job history table
                        await jobHistoryService.UpdateJobDefinitionInJobHistory(jobHistory, JobDefinition.PROCESS_HEALTH_TASK);

                        var healthTaskSyncService = host.Services.GetRequiredService<IHealthTaskSyncService>();

                        await healthTaskSyncService.ProcessHealthTaskAsync(executionContext);

                        //Update jobHistory status to success
                        jobHistory.RunStatus = !string.IsNullOrEmpty(executionContext.JobHistoryStatus)
                                                      ? executionContext.JobHistoryStatus
                                                      : (string.IsNullOrEmpty(executionContext.JobHistoryErrorLog)
                                                          ? Constants.JOB_HISTORY_SUCCESS_STATUS
                                                          : Constants.JOB_HISTORY_FAILURE_STATUS);
                        jobHistory.ErrorLog = executionContext.JobHistoryErrorLog;
                        await jobHistoryService.UpdateJobHistory(jobHistory);

                        _ll.Logger.LogInformation("{ClassName}.ProcessHealthTask - Ended processing ProcessHealthTask with TenantCode:{Code},TaskId:{Id}", className, executionContext.TenantCode, executionContext.TaskId);
                    }
                    catch (Exception ex)
                    {
                        _ll.Logger.LogError(ex, "{ClassName}.ProcessHealthTask - Failed processing ProcessHealthTask with TenantCode:{Code},TaskId:{Id},ErrorCode:{ErrorCode},ERROR:{Msg}",
                                className, executionContext.TenantCode, executionContext.TaskId, StatusCodes.Status500InternalServerError, ex.Message);

                        //Update jobHistory status to Failure
                        jobHistory.RunStatus = Constants.JOB_HISTORY_FAILURE_STATUS;
                        jobHistory.ErrorLog = ex.Message;
                        await jobHistoryService.UpdateJobHistory(jobHistory);
                        throw;
                    }
                }

                if (executionContext != null && executionContext.ExecuteCohorting && !string.IsNullOrEmpty(executionContext?.TenantCode) && executionContext.BatchSize > 0)
                {
                    try
                    {
                        _ll.Logger.LogInformation("{ClassName}.ExecuteCohorting - Started processing ExecuteCohorting with TenantCode:{Code}", className, executionContext.TenantCode);
                        //Update job definition in job history table
                        await jobHistoryService.UpdateJobDefinitionInJobHistory(jobHistory, JobDefinition.EXECUTE_COHORTING);

                        var cohortService = host.Services.GetRequiredService<ICohortService>();

                        // Lets process cohorts
                        await cohortService.ExecuteCohortingAsync(executionContext);

                        //Update jobHistory status to success
                        jobHistory.RunStatus = !string.IsNullOrEmpty(executionContext.JobHistoryStatus)
                                                      ? executionContext.JobHistoryStatus
                                                      : (string.IsNullOrEmpty(executionContext.JobHistoryErrorLog)
                                                          ? Constants.JOB_HISTORY_SUCCESS_STATUS
                                                          : Constants.JOB_HISTORY_FAILURE_STATUS);
                        jobHistory.ErrorLog = executionContext.JobHistoryErrorLog;
                        await jobHistoryService.UpdateJobHistory(jobHistory);

                        _ll.Logger.LogInformation("{ClassName}.ExecuteCohorting - Ended processing ExecuteCohorting with TenantCode:{Code}", className, executionContext.TenantCode);
                    }
                    catch (Exception ex)
                    {
                        _ll.Logger.LogError(ex, "{ClassName}.ExecuteCohorting - Failed processing ExecuteCohorting with TenantCode:{Code},ErrorCode:{ErrorCode},ERROR:{Msg}",
                            className, executionContext.TenantCode, StatusCodes.Status500InternalServerError, ex.Message);

                        //Update jobHistory status to Failure
                        jobHistory.RunStatus = Constants.JOB_HISTORY_FAILURE_STATUS;
                        jobHistory.ErrorLog = ex.Message;
                        await jobHistoryService.UpdateJobHistory(jobHistory);
                        throw;
                    }
                }

                if (executionContext != null && executionContext.IsEncryptAndCopy &&
                  !string.IsNullOrEmpty(executionContext.FISEncryptAndCopyFileName))
                {
                    try
                    {
                        //Update job definition in job history table
                        await jobHistoryService.UpdateJobDefinitionInJobHistory(jobHistory, JobDefinition.ENCRYPT_AND_COPY);

                        if (String.IsNullOrWhiteSpace(executionContext.BatchOperationGroupCode))
                        {
                            executionContext.BatchOperationGroupCode = "bgc-" + Guid.NewGuid().ToString("N");
                        }
                        _ll.Logger.LogInformation("{ClassName}.FISEncryptAndCopy - Started processing FISEncryptAndCopyFile with TenantCode:{Code}", className, executionContext.TenantCode);

                        var encryptAndCopyService = host.Services.GetRequiredService<IEncryptAndUploadFileToOutboundService>();
                        await encryptAndCopyService.EncryptAndCopyToOutbound(executionContext);

                        //Update jobHistory status to success
                        jobHistory.RunStatus = !string.IsNullOrEmpty(executionContext.JobHistoryStatus)
                                                      ? executionContext.JobHistoryStatus
                                                      : (string.IsNullOrEmpty(executionContext.JobHistoryErrorLog)
                                                          ? Constants.JOB_HISTORY_SUCCESS_STATUS
                                                          : Constants.JOB_HISTORY_FAILURE_STATUS);
                        jobHistory.ErrorLog = executionContext.JobHistoryErrorLog;
                        await jobHistoryService.UpdateJobHistory(jobHistory);

                        _ll.Logger.LogInformation("{ClassName}.FISEncryptAndCopyFile - Ended processing FISEncryptAndCopyFile with TenantCode:{Code}", className, executionContext.TenantCode);
                    }
                    catch (Exception ex)
                    {
                        _ll.Logger.LogError(ex, "{ClassName}.FISEncryptAndCopyFileName - Failed processing FISEncryptAndCopyFile with TenantCode:{Code},ErrorCode:{ErrorCode},ERROR:{Msg}",
                             className, executionContext.TenantCode, StatusCodes.Status500InternalServerError, ex.Message);

                        //Update jobHistory status to Failure
                        jobHistory.RunStatus = Constants.JOB_HISTORY_FAILURE_STATUS;
                        jobHistory.ErrorLog = ex.Message;
                        await jobHistoryService.UpdateJobHistory(jobHistory);
                        throw;
                    }
                }
                if (executionContext != null && executionContext.DeleteIneligibleConsumers && !string.IsNullOrEmpty(executionContext.TenantCode))
                {
                    try
                    {
                        _ll.Logger.LogInformation("{ClassName}.DeleteIneligibleConsumers - Started processing DeleteIneligibleConsumers with TenantCode:{Code}", className, executionContext.TenantCode);
                        //Update job definition in job history table
                        await jobHistoryService.UpdateJobDefinitionInJobHistory(jobHistory, JobDefinition.DELETE_CONSUMERS);

                        var consumerService = host.Services.GetRequiredService<IConsumerService>();
                        await consumerService.DeleteConsumers(executionContext);

                        //Update jobHistory status to success
                        jobHistory.RunStatus = !string.IsNullOrEmpty(executionContext.JobHistoryStatus)
                                                      ? executionContext.JobHistoryStatus
                                                      : (string.IsNullOrEmpty(executionContext.JobHistoryErrorLog)
                                                          ? Constants.JOB_HISTORY_SUCCESS_STATUS
                                                          : Constants.JOB_HISTORY_FAILURE_STATUS);
                        jobHistory.ErrorLog = executionContext.JobHistoryErrorLog;
                        await jobHistoryService.UpdateJobHistory(jobHistory);

                        _ll.Logger.LogInformation("{ClassName}.DeleteIneligibleConsumers - Ended processing DeleteIneligibleConsumers with TenantCode:{Code}", className, executionContext.TenantCode);
                    }
                    catch (Exception ex)
                    {
                        _ll.Logger.LogError(ex, "{ClassName}.DeleteIneligibleConsumers - Failed processing DeleteIneligibleConsumers with TenantCode:{Code},ErrorCode:{ErrorCode},ERROR:{Msg}",
                            className, executionContext.TenantCode, StatusCodes.Status500InternalServerError, ex.Message);

                        //Update jobHistory status to Failure
                        jobHistory.RunStatus = Constants.JOB_HISTORY_FAILURE_STATUS;
                        jobHistory.ErrorLog = ex.Message;
                        await jobHistoryService.UpdateJobHistory(jobHistory);
                        throw;
                    }
                }

                if (executionContext != null && executionContext.GenerateSweepstakesEntriesReport && executionContext.SweepstakesInstanceId > 0 && !string.IsNullOrEmpty(executionContext.Format) && !string.IsNullOrEmpty(executionContext.WalletTypeCode))
                {
                    try
                    {
                        _ll.Logger.LogInformation("{ClassName}.GenerateSweepstakesEntriesReport - Started processing GenerateSweepstakesEntriesReport with SweepstakesInstanceId:{Id}", className, executionContext.SweepstakesInstanceId);
                        //Update job definition in job history table
                        await jobHistoryService.UpdateJobDefinitionInJobHistory(jobHistory, JobDefinition.SWEEPSTAKES_ENTRIES_REPORT);

                        var sweepstakesImportService = host.Services.GetRequiredService<ISweepstakesImportService>();
                        var response = await sweepstakesImportService.GenerateSweepstakesEntriesReport(executionContext);

                        //Update jobHistory status to success
                        jobHistory.RunStatus = !string.IsNullOrEmpty(executionContext.JobHistoryStatus)
                                                      ? executionContext.JobHistoryStatus
                                                      : (string.IsNullOrEmpty(executionContext.JobHistoryErrorLog)
                                                          ? Constants.JOB_HISTORY_SUCCESS_STATUS
                                                          : Constants.JOB_HISTORY_FAILURE_STATUS);
                        jobHistory.ErrorLog = executionContext.JobHistoryErrorLog;
                        await jobHistoryService.UpdateJobHistory(jobHistory);

                        _ll.Logger.LogInformation("{ClassName}.GenerateSweepstakesEntriesReport - Ended processing GenerateSweepstakesEntriesReport with SweepstakesInstanceId:{Id}", className, executionContext.SweepstakesInstanceId);
                    }
                    catch (Exception ex)
                    {
                        _ll.Logger.LogError(ex, "{ClassName}.GenerateSweepstakesEntriesReport - Failed processing GenerateSweepstakesEntriesReport with SweepstakesInstanceId:{Id},ErrorCode:{ErrorCode},ERROR:{Msg}",
                            className, executionContext.SweepstakesInstanceId, StatusCodes.Status500InternalServerError, ex.Message);

                        //Update jobHistory status to Failure
                        jobHistory.RunStatus = Constants.JOB_HISTORY_FAILURE_STATUS;
                        jobHistory.ErrorLog = ex.Message;
                        await jobHistoryService.UpdateJobHistory(jobHistory);
                        throw;
                    }
                }
                if (executionContext != null && executionContext.IsCreateDuplicateConsumer &&
                    !string.IsNullOrEmpty(executionContext.NewEmail))
                {
                    try
                    {
                        _ll.Logger.LogInformation("{ClassName}.createDuplicateConsumerWithNewEmail - Started processing createDuplicateConsumerWithNewEmail with TenantCode:{Code},newEmail:{newEmail}", className, executionContext.TenantCode, executionContext.NewEmail);
                        //Update job definition in job history table
                        await jobHistoryService.UpdateJobDefinitionInJobHistory(jobHistory, JobDefinition.CREATE_DUPLICATE_CONSUMER);

                        var createDuplicateConsumerWithNewEmail = host.Services.GetRequiredService<ICreateDuplicateConsumerWithNewEmail>();
                        await createDuplicateConsumerWithNewEmail.CreateDuplicateConsumer(executionContext);

                        //Update jobHistory status to success
                        jobHistory.RunStatus = !string.IsNullOrEmpty(executionContext.JobHistoryStatus)
                                                      ? executionContext.JobHistoryStatus
                                                      : (string.IsNullOrEmpty(executionContext.JobHistoryErrorLog)
                                                          ? Constants.JOB_HISTORY_SUCCESS_STATUS
                                                          : Constants.JOB_HISTORY_FAILURE_STATUS);
                        jobHistory.ErrorLog = executionContext.JobHistoryErrorLog;
                        await jobHistoryService.UpdateJobHistory(jobHistory);

                        _ll.Logger.LogInformation("{ClassName}.createDuplicateConsumerWithNewEmail - Ended processing createDuplicateConsumerWithNewEmail with TenantCode:{Code},newEmail:{newEmail}", className, executionContext.TenantCode, executionContext.NewEmail);
                    }
                    catch (Exception ex)
                    {
                        _ll.Logger.LogError(ex, "{ClassName}.createDuplicateConsumerWithNewEmail - Failed processing createDuplicateConsumerWithNewEmail with TenantCode:{Code},newEmail:{newEmail},ErrorCode:{ErrorCode},ERROR:{Msg}",
                            className, executionContext.TenantCode, executionContext.NewEmail, StatusCodes.Status500InternalServerError, ex.Message);

                        //Update jobHistory status to Failure
                        jobHistory.RunStatus = Constants.JOB_HISTORY_FAILURE_STATUS;
                        jobHistory.ErrorLog = ex.Message;
                        await jobHistoryService.UpdateJobHistory(jobHistory);
                        throw;
                    }
                }
                //Restore Costco messages backup from DynamoDB
                if (executionContext != null && executionContext.ExecuteRestoreCostcoBackup)
                {
                    try
                    {
                        _ll.Logger.LogInformation("{ClassName}.ExecuteRestoreCostcoBackup - Started processing ExecuteRestoreCostcoBackup", className);
                        //Update job definition in job history table
                        await jobHistoryService.UpdateJobDefinitionInJobHistory(jobHistory, JobDefinition.RESTORE_COSTCO_BACKUP);

                        var retailProductSyncService = host.Services.GetRequiredService<IRetailProductSyncService>();
                        await retailProductSyncService.RestoreCostcoBackupFromDynamoDB(executionContext);

                        //Update jobHistory status to success
                        jobHistory.RunStatus = !string.IsNullOrEmpty(executionContext.JobHistoryStatus)
                                                      ? executionContext.JobHistoryStatus
                                                      : (string.IsNullOrEmpty(executionContext.JobHistoryErrorLog)
                                                          ? Constants.JOB_HISTORY_SUCCESS_STATUS
                                                          : Constants.JOB_HISTORY_FAILURE_STATUS);
                        jobHistory.ErrorLog = executionContext.JobHistoryErrorLog;
                        await jobHistoryService.UpdateJobHistory(jobHistory);

                        _ll.Logger.LogInformation("{ClassName}.ExecuteRestoreCostcoBackup - Ended processing ExecuteRestoreCostcoBackup", className);
                    }
                    catch (Exception ex)
                    {
                        _ll.Logger.LogError(ex, "{ClassName}.ExecuteRestoreCostcoBackup - Failed processing ExecuteRestoreCostcoBackup, ErrorCode:{ErrorCode},ERROR:{Msg}",
                            className, StatusCodes.Status500InternalServerError, ex.Message);
                        //Update jobHistory status to Failure
                        jobHistory.RunStatus = Constants.JOB_HISTORY_FAILURE_STATUS;
                        jobHistory.ErrorLog = ex.Message;
                        await jobHistoryService.UpdateJobHistory(jobHistory);
                        throw;
                    }
                }

                // Tenant sync job
                if (executionContext != null && !string.IsNullOrWhiteSpace(executionContext.SyncTenantConfigOptions))
                {
                    try
                    {
                        _ll.Logger.LogInformation("{ClassName}.TenantConfigSync - Started processing TenantConfigSync with TenantCode:{Code}", className, executionContext.TenantCode);
                        //Update job definition in job history table

                        await jobHistoryService.UpdateJobDefinitionInJobHistory(jobHistory, JobDefinition.EXECUTE_TENANT_SYNC);

                        var tenantConfigSyncService = host.Services.GetRequiredService<ITenantConfigSyncService>();
                        await tenantConfigSyncService.SyncAsync(executionContext);

                        //Update jobHistory status to success
                        jobHistory.RunStatus = !string.IsNullOrEmpty(executionContext.JobHistoryStatus)
                                                      ? executionContext.JobHistoryStatus
                                                      : (string.IsNullOrEmpty(executionContext.JobHistoryErrorLog)
                                                          ? Constants.JOB_HISTORY_SUCCESS_STATUS
                                                          : Constants.JOB_HISTORY_FAILURE_STATUS);
                        jobHistory.ErrorLog = executionContext.JobHistoryErrorLog;
                        await jobHistoryService.UpdateJobHistory(jobHistory);

                        _ll.Logger.LogInformation("{ClassName}.TenantConfigSync - Ended processing TenantConfigSync with TenantCode:{Code}", className, executionContext.TenantCode);
                    }
                    catch (Exception ex)
                    {
                        _ll.Logger.LogError(ex, "{ClassName}.TenantConfigSync - Failed processing TenantConfigSync with TenantCode:{Code},ErrorCode:{ErrorCode},ERROR:{Msg}",
                            className, executionContext.TenantCode, StatusCodes.Status500InternalServerError, ex.Message);

                        //Update jobHistory status to Failure
                        jobHistory.RunStatus = Constants.JOB_HISTORY_FAILURE_STATUS;
                        jobHistory.ErrorLog = ex.Message;
                        await jobHistoryService.UpdateJobHistory(jobHistory);
                        throw;
                    }
                }

                //Process Notification Rules
                if (executionContext != null && executionContext.ProcessNotificationRules)
                {
                    try
                    {
                        _ll.Logger.LogInformation("{ClassName}.ProcessNotificationRules - Started processing Notification rules", className);
                        //Update job definition in job history table
                        await jobHistoryService.UpdateJobDefinitionInJobHistory(jobHistory, JobDefinition.PROCESS_NOTIFICATION_RULES);

                        var notificationRulesService = host.Services.GetRequiredService<INotificationRulesService>();
                        await notificationRulesService.ProcessNotificationRulesAsync(executionContext);

                        //Update jobHistory status to success
                        jobHistory.RunStatus = !string.IsNullOrEmpty(executionContext.JobHistoryStatus)
                                                      ? executionContext.JobHistoryStatus
                                                      : (string.IsNullOrEmpty(executionContext.JobHistoryErrorLog)
                                                          ? Constants.JOB_HISTORY_SUCCESS_STATUS
                                                          : Constants.JOB_HISTORY_FAILURE_STATUS);
                        jobHistory.ErrorLog = executionContext.JobHistoryErrorLog;
                        await jobHistoryService.UpdateJobHistory(jobHistory);

                        _ll.Logger.LogInformation("{ClassName}.ProcessNotificationRules - Ended processing Notification rules", className);
                    }
                    catch (Exception ex)
                    {
                        _ll.Logger.LogError(ex, "{ClassName}.ProcessNotificationRules - Failed processing Notification rules, ErrorCode:{ErrorCode},ERROR:{Msg}",
                            className, StatusCodes.Status500InternalServerError, ex.Message);
                        //Update jobHistory status to Failure
                        jobHistory.RunStatus = Constants.JOB_HISTORY_FAILURE_STATUS;
                        jobHistory.ErrorLog = ex.Message;
                        await jobHistoryService.UpdateJobHistory(jobHistory);
                        throw;
                    }
                }

                // Soct-331-Process Consumer Task completion data
                if (executionContext != null && executionContext.ExtractCompletedConsumerTask)
                {
                    try
                    {
                        _ll.Logger.LogInformation("{ClassName}.ProcessCompletedConsumerTasks - Started processing Completed Consumer Task", className);
                        //Update job definition in job history table
                        await jobHistoryService.UpdateJobDefinitionInJobHistory(jobHistory, JobDefinition.PROCESS_CONSUMER_TASKS_COMPLETION);

                        var completedConsumerTaskService = host.Services.GetRequiredService<IProcessCompletedConsumerTask>();
                        await completedConsumerTaskService.ProcessCompletedConsumerTasksAsync(executionContext);

                        //Update jobHistory status to success
                        jobHistory.RunStatus = !string.IsNullOrEmpty(executionContext.JobHistoryStatus)
                                                      ? executionContext.JobHistoryStatus
                                                      : (string.IsNullOrEmpty(executionContext.JobHistoryErrorLog)
                                                          ? Constants.JOB_HISTORY_SUCCESS_STATUS
                                                          : Constants.JOB_HISTORY_FAILURE_STATUS);
                        jobHistory.ErrorLog = executionContext.JobHistoryErrorLog;
                        await jobHistoryService.UpdateJobHistory(jobHistory);

                        _ll.Logger.LogInformation("{ClassName}.ProcessCompletedConsumerTasks - Ended processing comspleted ConsumerTask", className);
                    }
                    catch (Exception ex)
                    {
                        _ll.Logger.LogError(ex, "{ClassName}.ProcessCompletedConsumerTasks - Failed processing Comspleted Consumer Task, ErrorCode:{ErrorCode},ERROR:{Msg}",
                            className, StatusCodes.Status500InternalServerError, ex.Message);
                        //Update jobHistory status to Failure
                        jobHistory.RunStatus = Constants.JOB_HISTORY_FAILURE_STATUS;
                        jobHistory.ErrorLog = ex.Message;
                        await jobHistoryService.UpdateJobHistory(jobHistory);
                        throw;
                    }
                }


                if (executionContext != null && executionContext.FileCryptoProcessor)
                {
                    try
                    {
                        _ll.Logger.LogInformation("{ClassName}.ProcessFileCrypto - Starting file crypto processing", className);

                        // Update job definition in job history table
                        await jobHistoryService.UpdateJobDefinitionInJobHistory(jobHistory, JobDefinition.PROCESS_FILE_CRYPTO);

                        var fileCryptoProcessor = host.Services.GetRequiredService<IFileCryptoProcessor>();
                        await fileCryptoProcessor.Process(executionContext);

                        // Update jobHistory status to success or failure
                        jobHistory.RunStatus = !string.IsNullOrEmpty(executionContext.JobHistoryStatus)
                                                  ? executionContext.JobHistoryStatus
                                                  : (string.IsNullOrEmpty(executionContext.JobHistoryErrorLog)
                                                      ? Constants.JOB_HISTORY_SUCCESS_STATUS
                                                      : Constants.JOB_HISTORY_FAILURE_STATUS);

                        jobHistory.ErrorLog = executionContext.JobHistoryErrorLog;
                        await jobHistoryService.UpdateJobHistory(jobHistory);

                        _ll.Logger.LogInformation("{ClassName}.ProcessFileCrypto - Completed file crypto processing", className);
                    }
                    catch (Exception ex)
                    {
                        _ll.Logger.LogError(ex, "{ClassName}.ProcessFileCrypto - Error occurred during file crypto processing. ErrorCode: {ErrorCode}, Message: {Msg}",
                            className, StatusCodes.Status500InternalServerError, ex.Message);

                        // Update jobHistory status to failure
                        jobHistory.RunStatus = Constants.JOB_HISTORY_FAILURE_STATUS;
                        jobHistory.ErrorLog = ex.Message;
                        await jobHistoryService.UpdateJobHistory(jobHistory);
                        throw;
                    }
                }

                if (executionContext != null && executionContext.TransferRedshiftToPostgres)
                {
                    try
                    {
                        _ll.Logger.LogInformation("{ClassName}.TransferRedshiftToPostgres - Starting Redshift to Postgres transfer job", className);

                        // Update job definition in job history table
                        await jobHistoryService.UpdateJobDefinitionInJobHistory(jobHistory, JobDefinition.TRANSFER_REDSHIFT_TO_POSTGRES);

                        var redshiftConnectionService = host.Services.GetRequiredService<ISyncRedshiftToPostgresService>();
                        await redshiftConnectionService.SyncAsync(executionContext);

                        // Update jobHistory status to success or failure
                        jobHistory.RunStatus = !string.IsNullOrEmpty(executionContext.JobHistoryStatus)
                                                  ? executionContext.JobHistoryStatus
                                                  : (string.IsNullOrEmpty(executionContext.JobHistoryErrorLog)
                                                      ? Constants.JOB_HISTORY_SUCCESS_STATUS
                                                      : Constants.JOB_HISTORY_FAILURE_STATUS);

                        jobHistory.ErrorLog = executionContext.JobHistoryErrorLog;
                        await jobHistoryService.UpdateJobHistory(jobHistory);

                        _ll.Logger.LogInformation("{ClassName}.TransferRedshiftToPostgres - Completed Redshift to Postgres transfer job successfully", className);
                    }
                    catch (Exception ex)
                    {
                        _ll.Logger.LogError(ex, "{ClassName}.TransferRedshiftToPostgres - Error during Redshift to Postgres transfer. ErrorCode: {ErrorCode}, Message: {ErrorMessage}", className, StatusCodes.Status500InternalServerError, ex.Message);

                        // Update jobHistory status to failure
                        jobHistory.RunStatus = Constants.JOB_HISTORY_FAILURE_STATUS;
                        jobHistory.ErrorLog = ex.Message;
                        await jobHistoryService.UpdateJobHistory(jobHistory);
                        throw;
                    }
                }

                if (executionContext != null && executionContext.RedshiftToSftp)
                {
                    try
                    {
                        _ll.Logger.LogInformation(
                            "{ClassName}.TransferRedshiftToSftp - Starting transfer of Redshift table data to SFTP. JobId: {JobId}",
                            className,
                            jobHistory.JobId);

                        // Update job definition in job history table
                        await jobHistoryService.UpdateJobDefinitionInJobHistory(jobHistory, JobDefinition.TRANSFER_REDSHIFT_TO_SFTP);

                        var redshiftToSftpExportService = host.Services.GetRequiredService<IRedshiftToSftpExportService>();
                        await redshiftToSftpExportService.ExecuteExportAsync(executionContext);

                        // Set job run status based on execution context
                        jobHistory.RunStatus = !string.IsNullOrEmpty(executionContext.JobHistoryStatus)
                            ? executionContext.JobHistoryStatus
                            : string.IsNullOrEmpty(executionContext.JobHistoryErrorLog)
                                ? Constants.JOB_HISTORY_SUCCESS_STATUS
                                : Constants.JOB_HISTORY_FAILURE_STATUS;

                        jobHistory.ErrorLog = executionContext.JobHistoryErrorLog;
                        await jobHistoryService.UpdateJobHistory(jobHistory);

                        _ll.Logger.LogInformation(
                            "{ClassName}.TransferRedshiftToSftp - Completed transfer of Redshift data to SFTP successfully. JobId: {JobId}, Status: {Status}",
                            className,
                            jobHistory.JobId,
                            jobHistory.RunStatus);
                    }
                    catch (Exception ex)
                    {
                        _ll.Logger.LogError(
                            ex,
                            "{ClassName}.TransferRedshiftToSftp - Error during transfer of Redshift data to SFTP. JobId: {JobId}, ErrorCode: {ErrorCode}, Message: {ErrorMessage}",
                            className,
                            jobHistory.JobId,
                            StatusCodes.Status500InternalServerError,
                            ex.Message);

                        // Update job history status to failure
                        jobHistory.RunStatus = Constants.JOB_HISTORY_FAILURE_STATUS;
                        jobHistory.ErrorLog = ex.Message;
                        await jobHistoryService.UpdateJobHistory(jobHistory);

                        throw;
                    }
                }

                if (executionContext != null && executionContext.ExecuteEventing && executionContext.BatchSize > 0 && !string.IsNullOrEmpty(executionContext.CustomerCode) && !string.IsNullOrEmpty(executionContext.CustomerLabel) && !string.IsNullOrEmpty(executionContext.EventingType))
                {
                    try
                    {
                        _ll.Logger.LogInformation("{ClassName}.ExecuteMemberImportEventing - Started processing ExecuteMemberImportEventing with CustomerCode:{Code}", className, executionContext.CustomerCode);
                        
                        await jobHistoryService.UpdateJobDefinitionInJobHistory(jobHistory, JobDefinition.MEMBER_IMPORT_EVENTING);
                        switch (executionContext.EventingType.ToUpperInvariant())
                        {
                            case "MEMBERIMPORT":
                                var memberImportEventingService = host.Services.GetRequiredService<IMemberImportEventingService>();
                                await memberImportEventingService.MemberImportEventingAsync(executionContext,  jobHistory.JobId);
                                break;

                            case "COHORTING":
                                var cohortingEventingService = host.Services.GetRequiredService<ICohortingEventingService>();
                                await cohortingEventingService.CohortingEventingAsync(executionContext,  jobHistory.JobId);
                                break;

                        }

                        //Update jobHistory status to success
                        jobHistory.RunStatus = !string.IsNullOrEmpty(executionContext.JobHistoryStatus)
                                                      ? executionContext.JobHistoryStatus
                                                      : (string.IsNullOrEmpty(executionContext.JobHistoryErrorLog)
                                                          ? Constants.JOB_HISTORY_SUCCESS_STATUS
                                                          : Constants.JOB_HISTORY_FAILURE_STATUS);
                        jobHistory.ErrorLog = executionContext.JobHistoryErrorLog;
                        await jobHistoryService.UpdateJobHistory(jobHistory);

                        _ll.Logger.LogInformation("{ClassName}.ExecuteMemberImportEventing - Ended processing ExecuteMemberImportEventing with CustomerCode:{Code}", className, executionContext.CustomerCode);
                    }
                    catch (Exception ex)
                    {
                        _ll.Logger.LogError(ex, "{ClassName}.ExecuteMemberImportEventing - Failed processing ExecuteMemberImportEventing with CustomerCode:{Code},ErrorCode:{ErrorCode},ERROR:{Msg}",
                            className, executionContext.TenantCode, StatusCodes.Status500InternalServerError, ex.Message);

                        //Update jobHistory status to Failure
                        jobHistory.RunStatus = Constants.JOB_HISTORY_FAILURE_STATUS;
                        jobHistory.ErrorLog = ex.Message;
                        await jobHistoryService.UpdateJobHistory(jobHistory);
                        throw;
                    }
                }


            }
            catch (Exception ex)
            {
                _ll.Logger.LogError(ex, "ETL-Main - ErrorCode:{Code}, Exception: {Msg}", StatusCodes.Status500InternalServerError, ex.Message);
                throw;
            }
            finally
            {
                // Since its a singleton object that internally set log file name in a variable
                // so if file is there it means we send some logs to s3
                await SendErrorAlertToAwsQueue(host);
            }

        }
        private static async System.Threading.Tasks.Task ProcessCohorts(EtlExecutionContext executionContext, IHost host, List<ETLConsumerModel> loadedConsumers, List<ETLPersonModel> loadedPersons)
        {
            // execute Cohorting pass if enabled
            if (executionContext.EnableCohorting && loadedConsumers != null && loadedConsumers.Count > 0)
            {
                var cohortService = host.Services.GetRequiredService<ICohortService>();

                // Lets process cohorts
                await cohortService.ProcessCohorts(loadedConsumers, loadedPersons);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="host"></param>
        /// <returns></returns>
        private static async System.Threading.Tasks.Task SendErrorAlertToAwsQueue(IHost? host)
        {
            if (host == null)
                return;

            var s3FileLogger = host.Services.GetRequiredService<IS3FileLogger>();
            if (s3FileLogger.Executed())
            {
                var awsNotificationService = host.Services.GetRequiredService<IAwsNotificationService>();
                var vault = host.Services.GetRequiredService<IVault>();
                var environment = await vault.GetSecret("env");
                string alertMessage = $"ETL in environment: {environment} encountered 1 or more errors. The errors are stored in the following file : {s3FileLogger.S3LogFile}";
                await awsNotificationService.PushNotificationToAwsTopic(new AwsSnsMessage(alertMessage), "AWS_SNS_TOPIC_NAME", false, string.Empty, string.Empty);
            }
        }

        /// <summary>
        /// get job definitionId based on scan S3 file type
        /// </summary>
        /// <param name="scanS3FileTypes"></param>
        /// <returns></returns>
        private static string GetJObDefinitionIdForEnableS3(EtlExecutionContext? etlExecutionContext)
        {
            var scanS3FileTypes = etlExecutionContext?.ScanS3FileTypes;
            var jobDefinitionId = string.Empty;
            switch (scanS3FileTypes)
            {
                case nameof(ScanS3FileType.HSA_SWEEP):
                    jobDefinitionId = JobDefinition.SCAN_S3_HSA_SWEEPER;
                    break;
                case nameof(ScanS3FileType.MEMBER_IMPORT):
                    jobDefinitionId = JobDefinition.SCAN_S3_MEMBER_IMPORT;
                    break;
                case nameof(ScanS3FileType.SUBSCRIBER_ONLY_MEMBER_IMPORT):
                    jobDefinitionId = JobDefinition.SUBSCRIBE_ONLY_MEMBER_IMPORT;
                    break;
                case nameof(ScanS3FileType.FIS_MONETARY_TXN):
                    jobDefinitionId = JobDefinition.SCAN_S3_MONETARY_TXN;
                    break;
                case nameof(ScanS3FileType.FIS_NON_MONETARY_TXN):
                    jobDefinitionId = JobDefinition.SCAN_S3_NON_MONETARY_TXN;
                    break;
                case nameof(ScanS3FileType.TRIVIA_IMPORT):
                    jobDefinitionId = JobDefinition.IMPORT_TRIVIA;
                    break;
                case nameof(ScanS3FileType.QUESTIONNAIRE_IMPORT):
                    jobDefinitionId = JobDefinition.IMPORT_QUESTIONNAIRE;
                    break;
                case nameof(ScanS3FileType.FIS_CARD30_RESPONSE):
                    jobDefinitionId = JobDefinition.PROCESS_CARD_30_RESPONSE;
                    break;
                case nameof(ScanS3FileType.FIS_CARD60_RESPONSE):
                    jobDefinitionId = JobDefinition.PROCESS_CARD_60_RESPONSE;
                    break;
                case nameof(ScanS3FileType.COHORTCONSUMER):
                    jobDefinitionId = JobDefinition.SCAN_S3_IMPORT_COHORT_CONSUMER;
                    break;
                case nameof(ScanS3FileType.TASK_IMPORT):
                    jobDefinitionId = JobDefinition.SCAN_S3_TASK_IMPORT;
                    break;
                case nameof(ScanS3FileType.PROCESS_DEPOSIT_INSTRUCTIONS_FILE):
                    jobDefinitionId = JobDefinition.PROCESS_DEPOSIT_INSTRUCTIONS_FILE;
                    break;
                case nameof(ScanS3FileType.TASK_UPDATE):
                    jobDefinitionId = string.IsNullOrEmpty(etlExecutionContext?.CustomFormat)
                                        ? JobDefinition.TASK_UPDATE
                                        : JobDefinition.TASK_UPDATE_CUSTOM_FORMAT;

                    break;
            }
            return jobDefinitionId;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        private static EtlExecutionContext ProcessCommandLine(string[] args)
        {
            CommandLineParser clp = new CommandLineParser(new CommandLineParserOption[]
            {
                new CommandLineParserOptionTyped<string>("--tenantCode"),
                new CommandLineParserOptionTyped<int>("--startIndex", 0),
                new CommandLineParserOptionTyped<int>("--maxEnrollments", 10),
                new CommandLineParserOptionTyped<string>("--pldFilePath"),
                new CommandLineParserOptionTyped<bool>("--enableEnrollment", false, true),
                new CommandLineParserOptionTyped<bool>("--enableMemberImport", false, true),
                new CommandLineParserOptionTyped<bool>("--enablePldProcessing", false, true),
                new CommandLineParserOptionTyped<bool>("--enableCohorting", false, true),
                new CommandLineParserOptionTyped<string>("--taskUpdateFilePath"),
                new CommandLineParserOptionTyped<bool>("--enableS3", false, true),
                new CommandLineParserOptionTyped<string>("--memberFilePath"),
                new CommandLineParserOptionTyped<string>("--enrollmentFilePath"),
                new CommandLineParserOptionTyped<bool>("--enableMemberLoad", false, true),
                new CommandLineParserOptionTyped<string>("--taskImportFilePath"),
                new CommandLineParserOptionTyped<string>("--triviaImportFilePath"),
                new CommandLineParserOptionTyped<string>("--memberImportFilePath"),
                new CommandLineParserOptionTyped<string>("--redeemConsumerListFilePath"),
                new CommandLineParserOptionTyped<bool>("--processRecurringTasks", false, true),
                new CommandLineParserOptionTyped<bool>("--clearWalletEntries", false, true),
                new CommandLineParserOptionTyped<bool>("--redeemHSA", false, true),
                new CommandLineParserOptionTyped<bool>("--fisCreateCards", false, true),
                new CommandLineParserOptionTyped<bool>("--processRetailProductSyncQueue", false, true),
                new CommandLineParserOptionTyped<bool>("--executeBenefitsFunding", false, true),
                new CommandLineParserOptionTyped<bool>("--consumeHealthQueue", false, true),
                new CommandLineParserOptionTyped<bool>("--generateCardLoad", false, true),
                new CommandLineParserOptionTyped<bool>("--fis30RecordFileLoad", false, true),
                new CommandLineParserOptionTyped<bool>("--fis60RecordFileLoad", false, true),
                new CommandLineParserOptionTyped<string>("--fisRecordFileName"),
                new CommandLineParserOptionTyped<bool>("--processMonetaryTransactionsBatchFile", false, true),
                new CommandLineParserOptionTyped<string>("--fisMonetaryTransactionsFileName"),
                new CommandLineParserOptionTyped<int>("--taskId", 0),
                new CommandLineParserOptionTyped<string>("--rollupPeriodTypeName", string.Empty),
                new CommandLineParserOptionTyped<int>("--year", 0),
                new CommandLineParserOptionTyped<int>("--month", 0),
                new CommandLineParserOptionTyped<bool>("--processHealthTask", false, true),
                new CommandLineParserOptionTyped<bool>("--PerformExternalTxnSync", false, true),
                new CommandLineParserOptionTyped<string>("--localDownloadFolderPath", string.Empty),
                new CommandLineParserOptionTyped<int>("--batchSize", Constants.DefaultBatchSize),
                new CommandLineParserOptionTyped<bool>("--executeCohorting", false, true),
                new CommandLineParserOptionTyped<string>("--customerCode", string.Empty),
                new CommandLineParserOptionTyped<string>("--customerLabel", string.Empty),
                new CommandLineParserOptionTyped<bool>("--consumerNonMonetaryTransactions", false, true),
                new CommandLineParserOptionTyped<string>("--consumerNonMonetaryTransactionsFileName"),
                new CommandLineParserOptionTyped<string>("--fileName"),
                new CommandLineParserOptionTyped<bool>("--encryptAndCopy", false, true),
                new CommandLineParserOptionTyped<bool>("--donotEncryptAndCopy", false, true),
                new CommandLineParserOptionTyped<string>("--walletTypeCode", string.Empty),
                new CommandLineParserOptionTyped<bool>("--generateWalletBalancesReport", false, true),
                new CommandLineParserOptionTyped<string>("--scanS3FileTypes", string.Empty),
                new CommandLineParserOptionTyped<string>("--customFormat", string.Empty),
                new CommandLineParserOptionTyped<bool>("--updateUserInfoInFIS",false,true),
                new CommandLineParserOptionTyped<bool>("--consumeSweepstakesWinnerReport", false, true),
                new CommandLineParserOptionTyped<long>("--sweepstakesInstanceId",0),
                new CommandLineParserOptionTyped<string>("--format", string.Empty),
                new CommandLineParserOptionTyped<string>("--cutoffDate", string.Empty),
                new CommandLineParserOptionTyped<string>("--cutoffTz", string.Empty),
                new CommandLineParserOptionTyped<bool>("--generateSweepstakesEntriesReport", false, true),
                new CommandLineParserOptionTyped<bool>("--deleteIneligibleConsumers", false, true),
                new CommandLineParserOptionTyped<bool>("--subscriberOnly", false, true),
                new CommandLineParserOptionTyped<string>("--batchOperationGroupCode", string.Empty),
                new CommandLineParserOptionTyped<string>("--batchActionType", string.Empty),
                new CommandLineParserOptionTyped<string>("--newEmail", string.Empty),
                new CommandLineParserOptionTyped<string>("--consumerCode", string.Empty),
                new CommandLineParserOptionTyped<bool>("--CreateDuplicateConsumer", false, true),
                new CommandLineParserOptionTyped<string>("--consumerListFile", string.Empty),
                new CommandLineParserOptionTyped<string>("--incomingFilePath", string.Empty),
                new CommandLineParserOptionTyped<string>("--incomingBucketName", string.Empty),
                new CommandLineParserOptionTyped<string>("--publicfolderBucketName", string.Empty),
                new CommandLineParserOptionTyped<string>("--archiveFilePath", string.Empty),
                new CommandLineParserOptionTyped<string>("--archiveBucketName", string.Empty),
                new CommandLineParserOptionTyped<string>("--actionName", string.Empty),
                new CommandLineParserOptionTyped<string>("--outboundFilePath", string.Empty),
                new CommandLineParserOptionTyped<string>("--outboundBucketName", string.Empty),
                new CommandLineParserOptionTyped<string>("--outboundFileNamePattern", string.Empty),
                new CommandLineParserOptionTyped<bool>("--fileCryptoProcessor", false, true),
                new CommandLineParserOptionTyped<bool>("--executeRestoreCostcoBackup", false, true),
                new CommandLineParserOptionTyped<long>("--minEpochTs", 0),
                new CommandLineParserOptionTyped<long>("--maxEpochTs", 0),
                new CommandLineParserOptionTyped<string>("--jobHistoryId", string.Empty),
                new CommandLineParserOptionTyped<string>("--syncTenantConfig", string.Empty),
                new CommandLineParserOptionTyped<string>("--consumerCodes", string.Empty),
                new CommandLineParserOptionTyped<bool>("--processNotificationRules", false, true),
                new CommandLineParserOptionTyped<bool>("--extractCompletedConsumerTask", false, true),
                new CommandLineParserOptionTyped<bool>("--transferRedshiftToPostgres", false, true),
                new CommandLineParserOptionTyped<string>("--dataType", string.Empty),
                new CommandLineParserOptionTyped<string>("--startDate", string.Empty),
                new CommandLineParserOptionTyped<string>("--endDate", string.Empty),
                new CommandLineParserOptionTyped<string>("--tableName", string.Empty),
                new CommandLineParserOptionTyped<string>("--columnName", string.Empty),
                new CommandLineParserOptionTyped<string>("--dateRangeStart", string.Empty),
                new CommandLineParserOptionTyped<string>("--dateRangeEnd", string.Empty),
                new CommandLineParserOptionTyped<string>("--delimiter", string.Empty),
                new CommandLineParserOptionTyped<bool>("--shouldEncrypt", false),
                new CommandLineParserOptionTyped<bool>("--redshiftToSftp", false, true),
                new CommandLineParserOptionTyped<string>("--dateFormat", string.Empty),
                new CommandLineParserOptionTyped<bool>("--shouldAddSourceFileName", false),
                new CommandLineParserOptionTyped<string>("--cohortListFile", string.Empty),
                new CommandLineParserOptionTyped<bool>("--enableSftp", false),
                new CommandLineParserOptionTyped<string>("--redshiftDatabaseName", string.Empty),
                new CommandLineParserOptionTyped<string>("--cohortCode", string.Empty),
                new CommandLineParserOptionTyped<bool>("--removeFooter", false),
                new CommandLineParserOptionTyped<bool>("--hasHeader", true ,true),
                new CommandLineParserOptionTyped<bool>("--isSubmitCard60Job", false, true),
                new CommandLineParserOptionTyped<string>("--questionnaireImportFilePath", string.Empty),
                new CommandLineParserOptionTyped<bool>("--executeEventing", false, true),
                new CommandLineParserOptionTyped<bool>("--shouldMarkFileAsCompleted", false),
                new CommandLineParserOptionTyped<bool>("--shouldAppendTotalCount", false),
                new CommandLineParserOptionTyped<string>("--eventingType", string.Empty),
                new CommandLineParserOptionTyped<string>("--partnerCode", string.Empty),
                new CommandLineParserOptionTyped<int>("--messagingGroupCount", 10)
            }, args);
        

      
            var enablePldProcessing = (bool)(clp.NamedArguments["--enablePldProcessing"] ?? false);
            var enableMemberLoad = (bool)(clp.NamedArguments["--enableMemberLoad"] ?? false);
            var enableHealthMetricProcessing = (bool)(clp.NamedArguments["--consumeHealthQueue"] ?? false);

            string taskUpdateFilePath = string.Empty;
            if (clp.NamedArguments.ContainsKey("--taskUpdateFilePath") && clp.NamedArguments["--taskUpdateFilePath"] != null)
            {
                taskUpdateFilePath = (string)(clp.NamedArguments["--taskUpdateFilePath"] ?? string.Empty);
            }
            string taskImportFilePath = string.Empty;
            if (clp.NamedArguments.ContainsKey("--taskImportFilePath") && clp.NamedArguments["--taskImportFilePath"] != null)
            {
                taskImportFilePath = (string)(clp.NamedArguments["--taskImportFilePath"] ?? string.Empty);
            }

            string triviaImportFilePath = string.Empty;
            if (clp.NamedArguments.ContainsKey("--triviaImportFilePath") && clp.NamedArguments["--triviaImportFilePath"] != null)
            {
                triviaImportFilePath = (string)(clp.NamedArguments["--triviaImportFilePath"] ?? string.Empty);
            }



            string fisRecordFileName = string.Empty;
            if (clp.NamedArguments.ContainsKey("--fisRecordFileName") && clp.NamedArguments["--fisRecordFileName"] != null)
            {
                fisRecordFileName = (string)(clp.NamedArguments["--fisRecordFileName"] ?? string.Empty);
            }

            string fisMonetaryTransactionsFileName = string.Empty;
            if (clp.NamedArguments.ContainsKey("--fisMonetaryTransactionsFileName") && clp.NamedArguments["--fisMonetaryTransactionsFileName"] != null)
            {
                fisMonetaryTransactionsFileName = (string)(clp.NamedArguments["--fisMonetaryTransactionsFileName"] ?? string.Empty);
            }


            string consumerNonMonetaryTransactionsFileName = string.Empty;
            if (clp.NamedArguments.ContainsKey("--consumerNonMonetaryTransactionsFileName") && clp.NamedArguments["--consumerNonMonetaryTransactionsFileName"] != null)
            {
                consumerNonMonetaryTransactionsFileName = (string)(clp.NamedArguments["--consumerNonMonetaryTransactionsFileName"] ?? string.Empty);
            }

            string fisEncriptAndCopyFileName = string.Empty;
            if (clp.NamedArguments.ContainsKey("--fileName") && clp.NamedArguments["--fileName"] != null)
            {
                fisEncriptAndCopyFileName = (string)(clp.NamedArguments["--fileName"] ?? string.Empty);
            }

            var executionContext = new EtlExecutionContext()
            {
                TenantCode = GetParamValue<string>(clp, "--tenantCode", string.Empty),
                StartIndex = GetParamValue<int>(clp, "--startIndex", 0),
                MaxEnrollments = GetParamValue<int>(clp, "--maxEnrollments", 10),
                EnablePldProcessing = enablePldProcessing,
                PldFilePath = enablePldProcessing ? GetParamValue<string>(clp, "--pldFilePath", string.Empty) : string.Empty,
                EnableEnrollment = GetParamValue<bool>(clp, "--enableEnrollment", false),
                EnableMemberImport = GetParamValue<bool>(clp, "--enableMemberImport", false),
                EnableCohorting = GetParamValue<bool>(clp, "--enableCohorting", false),
                ProcessRecurringTasks = GetParamValue<bool>(clp, "--processRecurringTasks", false),
                ClearWalletEntries = GetParamValue<bool>(clp, "--clearWalletEntries", false),
                RedeemHSA = GetParamValue<bool>(clp, "--redeemHSA", false),
                FISCreateCards = GetParamValue<bool>(clp, "--fisCreateCards", false),
                ProcessRetailProductSyncQueue = GetParamValue<bool>(clp, "--processRetailProductSyncQueue", false),
                ExecuteBenefitsFunding = GetParamValue<bool>(clp, "--executeBenefitsFunding", false),
                TaskUpdateFilePath = taskUpdateFilePath,
                CustomFormat = GetParamValue<string>(clp, "--customFormat", string.Empty),
                EnableS3 = GetParamValue<bool>(clp, "--enableS3", false),
                EnableMemberLoad = enableMemberLoad,
                MemberFilePath = enableMemberLoad ? GetParamValue<string>(clp, "--memberFilePath", string.Empty) : string.Empty,
                EnrollmentFilePath = enableMemberLoad ? GetParamValue<string>(clp, "--enrollmentFilePath", string.Empty) : string.Empty,
                TaskImportFilePath = taskImportFilePath,
                TriviaImportFilePath = triviaImportFilePath,
                MemberImportFilePath = GetParamValue<string>(clp, "--memberImportFilePath", string.Empty),
                RedeemConsumerListFilePath = GetParamValue<string>(clp, "--redeemConsumerListFilePath", string.Empty),
                EnableHealthMetricProcessing = enableHealthMetricProcessing,
                GenerateCardLoad = GetParamValue<bool>(clp, "--generateCardLoad", false),
                FIS30RecordFileLoad = GetParamValue<bool>(clp, "--fis30RecordFileLoad", false),
                FIS60RecordFileLoad = GetParamValue<bool>(clp, "--fis60RecordFileLoad", false),
                FISRecordFileName = fisRecordFileName,
                ProcessMonetaryTransactionsBatchFile = GetParamValue<bool>(clp, "--processMonetaryTransactionsBatchFile", false),
                FISMonetaryTransactionsFileName = fisMonetaryTransactionsFileName,
                PerformExternalTxnSync = GetParamValue<bool>(clp, "--PerformExternalTxnSync", false),
                TaskId = GetParamValue<int>(clp, "--taskId", 0),
                RollupPeriodTypeName = GetParamValue<string>(clp, "--rollupPeriodTypeName", string.Empty),
                Year = GetParamValue<int>(clp, "--year", 0),
                Month = GetParamValue<int>(clp, "--month", 0),
                ProcessHealthTask = GetParamValue<bool>(clp, "--processHealthTask", false),
                LocalDownloadFolderPath = GetParamValue<string>(clp, "--localDownloadFolderPath", string.Empty),
                BatchSize = GetParamValue<int>(clp, "--batchSize", Constants.DefaultBatchSize),
                ExecuteCohorting = GetParamValue<bool>(clp, "--executeCohorting", false),
                CustomerCode = GetParamValue<string>(clp, "--customerCode", string.Empty),
                CustomerLabel = GetParamValue<string>(clp, "--customerLabel", string.Empty),
                DonotEncryptAndCopy = GetParamValue<bool>(clp, "--donotEncryptAndCopy", false),
                ConsumerNonMonetaryTransactionsBatchFile = GetParamValue<bool>(clp, "--consumerNonMonetaryTransactions", false),
                ConsumerNonMonetaryTransactionsFileName = consumerNonMonetaryTransactionsFileName,
                FISEncryptAndCopyFileName = fisEncriptAndCopyFileName,
                IsEncryptAndCopy = GetParamValue<bool>(clp, "--encryptAndCopy", false),
                WalletTypeCode = GetParamValue<string>(clp, "--walletTypeCode", string.Empty),
                GenerateWalletBalancesReport = GetParamValue<bool>(clp, "--generateWalletBalancesReport", false),
                ScanS3FileTypes = GetParamValue<string>(clp, "--scanS3FileTypes", string.Empty),
                IsUpdateUserInfoInFIS = GetParamValue<bool>(clp, "--updateUserInfoInFIS", false),
                ConsumeSweepstakesWinnerReport = GetParamValue<bool>(clp, "--consumeSweepstakesWinnerReport", false),
                SweepstakesInstanceId = GetParamValue<long>(clp, "--sweepstakesInstanceId", 0),
                Format = GetParamValue<string>(clp, "--format", string.Empty),
                CutoffDate = GetParamValue<string>(clp, "--cutoffDate", string.Empty),
                CutoffTz = GetParamValue<string>(clp, "--cutoffTz", string.Empty),
                GenerateSweepstakesEntriesReport = GetParamValue<bool>(clp, "--generateSweepstakesEntriesReport", false),
                SubscriberOnly = GetParamValue<bool>(clp, "--subscriberOnly", false),
                DeleteIneligibleConsumers = GetParamValue<bool>(clp, "--deleteIneligibleConsumers", false),
                BatchOperationGroupCode = GetParamValue<string>(clp, "--batchOperationGroupCode", string.Empty),
                BatchActionType = GetParamValue<string>(clp, "--batchActionType", string.Empty),
                IsCreateDuplicateConsumer = GetParamValue<bool>(clp, "--CreateDuplicateConsumer", false),
                NewEmail = GetParamValue<string>(clp, "--newEmail", string.Empty),
                ConsumerCode = GetParamValue<string>(clp, "--consumerCode", string.Empty),
                ConsumerListFile = GetParamValue<string>(clp, "--consumerListFile", string.Empty),
                IncomingFilePath = GetParamValue<string>(clp, "--incomingFilePath", string.Empty),
                IncomingBucketName = GetParamValue<string>(clp, "--incomingBucketName", string.Empty),
                PublicfolderBucketName = GetParamValue<string>(clp, "--publicfolderBucketName", string.Empty),
                ArchiveFilePath = GetParamValue<string>(clp, "--archiveFilePath", string.Empty),
                ArchiveBucketName = GetParamValue<string>(clp, "--archiveBucketName", string.Empty),
                ActionName = GetParamValue<string>(clp, "--actionName", string.Empty),
                OutboundFilePath = GetParamValue<string>(clp, "--outboundFilePath", string.Empty),
                OutboundBucketName = GetParamValue<string>(clp, "--outboundBucketName", string.Empty),
                OutboundFileNamePattern = GetParamValue<string>(clp, "--outboundFileNamePattern", string.Empty),
                ExecuteRestoreCostcoBackup = GetParamValue<bool>(clp, "--executeRestoreCostcoBackup", false),
                FileCryptoProcessor = GetParamValue<bool>(clp, "--fileCryptoProcessor", false),
                MinEpochTs = GetParamValue<long>(clp, "--minEpochTs", 0),
                MaxEpochTs = GetParamValue<long>(clp, "--maxEpochTs", 0),
                JobHistoryId = GetParamValue<string>(clp, "--jobHistoryId", string.Empty),
                SyncTenantConfigOptions = GetParamValue<string>(clp, "--syncTenantConfig", string.Empty),
                ConsumerCodes = GetParamValue<string>(clp, "--consumerCodes", string.Empty),
                ProcessNotificationRules = GetParamValue<bool>(clp, "--processNotificationRules", false),
                ExtractCompletedConsumerTask = GetParamValue<bool>(clp, "--extractCompletedConsumerTask", false),
                TransferRedshiftToPostgres = GetParamValue<bool>(clp, "--transferRedshiftToPostgres", false),
                SyncDataType = GetParamValue<string>(clp, "--dataType", string.Empty),
                StartDate = GetParamValue<string>(clp, "--startDate", string.Empty),
                EndDate = GetParamValue<string>(clp, "--endDate", string.Empty),
                TableName = GetParamValue<string>(clp, "--tableName", string.Empty),
                ColumnName = GetParamValue<string>(clp, "--columnName", string.Empty),
                DateRangeStart = GetParamValue<string>(clp, "--dateRangeStart", string.Empty),
                DateRangeEnd = GetParamValue<string>(clp, "--dateRangeEnd", string.Empty),
                Delimiter = GetParamValue<string>(clp, "--delimiter", string.Empty),
                ShouldEncrypt = GetParamValue<bool>(clp, "--shouldEncrypt", false),
                RedshiftToSftp = GetParamValue<bool>(clp, "--redshiftToSftp", false),
                DateFormat = GetParamValue<string>(clp, "--dateFormat", string.Empty),
                ShouldAddSourceFileName = GetParamValue<bool>(clp, "--shouldAddSourceFileName", false),
                RemoveFooter = GetParamValue<bool>(clp, "--removeFooter", false),
                HasHeader = GetParamValue<bool>(clp, "--hasHeader", true),
                CohortListFile = GetParamValue<string>(clp, "--cohortListFile", string.Empty),
                RedshiftDatabaseName = GetParamValue<string>(clp, "--redshiftDatabaseName", string.Empty),
                EnableSftp = GetParamValue<bool>(clp, "--enableSftp", false),
                CohortCode = GetParamValue<string>(clp, "--cohortCode", string.Empty),
                QuestionnaireImportFilePath = GetParamValue<string>(clp, "--questionnaireImportFilePath", string.Empty),
                IsSubmitCard60Job = GetParamValue<bool>(clp, "--isSubmitCard60Job", false),
                ExecuteEventing = GetParamValue<bool>(clp, "--executeEventing", false),
                ShouldMarkFileAsCompleted = GetParamValue<bool>(clp, "--shouldMarkFileAsCompleted", false),
                ShouldAppendTotalRowCount = GetParamValue<bool>(clp, "--shouldAppendTotalCount", false),
                EventingType = GetParamValue<string>(clp, "--eventingType", string.Empty),
                PartnerCode = GetParamValue<string>(clp, "--partnerCode", string.Empty),
                MessagingGroupCount = GetParamValue<int>(clp, "--messagingGroupCount", 10),
            };

            return executionContext;
        }

        private static void PrintFieldIds(IHost? host)
        {
            var pldFieldInfoProvider = host?.Services.GetRequiredService<IPldFieldInfoProvider>();
            var fieldInfo = pldFieldInfoProvider?.GetFieldInfo();

            if (fieldInfo != null)
            {
                int idx = 1;
                foreach (var field in fieldInfo)
                {
                    if (field != null)
                    {
                        Console.WriteLine($"Field{idx++}, {field.FieldIdentifier}");
                    }
                }
            }
        }

        private static void ParsePldFile(IHost? host, string pldFilePath)
        {
            const string methodName = nameof(ParsePldFile);
            // _logger.LogInformation("ParsePldFile: Loading PLD data from file: {pld}", pldFilePath);

            IPldParser? pldParser = host?.Services.GetRequiredService<IPldParser>();

            if (pldParser == null) return;

            List<PldRecordDto> pldData = new();
            try
            {
                using var reader = new StreamReader(pldFilePath);

                int lineNum = 0;
                string? line;
                while ((line = reader.ReadLine()) != null)
                {
                    var pldRec = pldParser.ParsePldLine(line.Trim());
                    if (pldRec != null)
                    {
                        pldData.Add(pldRec);
                    }
                    else
                    {
                        _ll.Logger.LogWarning("{ClassName}.{MethodName} - Ignoring PLD data null for line#: {Line}", className, methodName, lineNum);
                    }
                    lineNum++;
                }
            }
            catch (Exception ex)
            {
                _ll.Logger.LogError(ex, "{ClassName}.{MethodName} - Error processing PLD file: {Pld}, ERROR: {Msg}", className, methodName, pldFilePath, ex.Message);
            }
        }

        private static T GetParamValue<T>(CommandLineParser clp, string paramName, T defaultValue)
        {
            if (clp.NamedArguments.TryGetValue(paramName, out var value) && value != null)
            {
                try
                {
                    return (T)Convert.ChangeType(value, typeof(T));
                }
                catch (InvalidCastException)
                {
                    // Handle the case where the conversion fails
                }
                catch (FormatException)
                {
                    // Handle the case where the conversion fails due to format issues
                }
            }
            return defaultValue;
        }
    }
}
