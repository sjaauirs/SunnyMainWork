using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NHibernate;
using SunnyRewards.Helios.Common.Core.Extensions;
using SunnyRewards.Helios.Etl.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.ETL.Common.Constants;
using SunnyRewards.Helios.ETL.Core.Domain.Constants;
using SunnyRewards.Helios.ETL.Core.Domain.Constants.FIS;
using SunnyRewards.Helios.ETL.Core.Domain.Dtos;
using SunnyRewards.Helios.ETL.Core.Domain.Models;
using SunnyRewards.Helios.ETL.Infrastructure.Extensions;
using SunnyRewards.Helios.ETL.Infrastructure.Logs.Interface;
using SunnyRewards.Helios.ETL.Infrastructure.Repositories.HeliosRepo.Interfaces;
using SunnyRewards.Helios.ETL.Infrastructure.S3FileProcessor.Interface;
using SunnyRewards.Helios.ETL.Infrastructure.Services;
using SunnyRewards.Helios.ETL.Infrastructure.Services.DynamoDb;
using SunnyRewards.Helios.ETL.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.ETL.Infrastructure.Services.Interfaces.DynamoDb;
using SunnyRewards.Helios.ETL.Infrastructure.Services.Interfaces.FIS;
using System;
using System.Globalization;
using System.Reflection;
using System.Text;

namespace SunnyRewards.Helios.ETL.Infrastructure.S3FileProcessor
{
    public class S3FileProcessorCore : BasePldProcessor, IS3FileProcessorCore
    {
        private readonly IAwsS3Service _awsS3Service;
        private readonly ITaskUpdateService _taskUpdateService;
        private readonly ILogger<S3FileProcessorCore> _logger;
        private readonly ICohortService _cohortService;
        private readonly IPersonRepo _personRepo;
        private readonly IIngestConsumerAttrService _ingestConsumerAttrService;
        private readonly IEnrollmentService _enrollmentService;
        private readonly IMemberImportService _memberImportService;
        private readonly ITriviaImportService _triviaImportService;
        private readonly IQuestionnaireImportService _questionnaireImportService;
        private readonly ITenantRepo _tenantRepo;
        private readonly IConfiguration _configuration;
        private readonly IMonetaryTransactionsFileReadService _monetaryTransactionsFileReadService;
        private readonly IConsumerNonMonetaryTransactionsFileReadService _consumerNonMonetaryTransactionsFileReadService;
        private readonly IWalletService _walletService;
        private readonly IJobReportService _jobReportService;
        private readonly IMemberImportFileDataService _memberImportFileDataService;
        private readonly ICohortConsumerService _cohortConsumerService;
        private readonly IJobHistoryService _jobHistoryService;
        private readonly ITaskImportService _taskImportService;
        private readonly IDepositInstructionService _depositInstructionService;
        private const string className = nameof(S3FileProcessorCore);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="awsS3Service"></param>
        public S3FileProcessorCore(IAwsS3Service awsS3Service,
            IPldParser pldParser, ISession session, ILogger<S3FileProcessorCore> logger,
            ITaskUpdateService taskUpdateService, ICohortService cohortService,
            IPersonRepo personRepo, ITenantRepo tenantRepo, IIngestConsumerAttrService ingestConsumerAttrService,
            IEnrollmentService enrollmentService, IS3FileLogger s3FileLogger, IMemberImportService memberImportService,
            IConfiguration configuration, IMonetaryTransactionsFileReadService monetaryTransactionsFileReadService,
            IConsumerNonMonetaryTransactionsFileReadService consumerNonMonetaryTransactionsFileReadService,
            IWalletService walletService, IJobReportService jobReportService, ITriviaImportService triviaImportService, ITaskImportService taskImportService,
            ICohortConsumerService cohortConsumerService, IMemberImportFileDataService memberImportFileDataService, IJobHistoryService jobHistoryService,
            IQuestionnaireImportService questionnaireImportService, IDepositInstructionService depositInstructionService)
            : base(logger, session, pldParser, s3FileLogger)
        {
            _awsS3Service = awsS3Service;
            _taskUpdateService = taskUpdateService;
            _logger = logger;
            _cohortService = cohortService;
            _personRepo = personRepo;
            _ingestConsumerAttrService = ingestConsumerAttrService;
            _enrollmentService = enrollmentService;
            _memberImportService = memberImportService;
            _tenantRepo = tenantRepo;
            _configuration = configuration;
            _monetaryTransactionsFileReadService = monetaryTransactionsFileReadService;
            _consumerNonMonetaryTransactionsFileReadService = consumerNonMonetaryTransactionsFileReadService;
            _walletService = walletService;
            _jobReportService = jobReportService;
            _triviaImportService = triviaImportService;
            _taskImportService = taskImportService;
            _cohortConsumerService = cohortConsumerService;
            _memberImportFileDataService = memberImportFileDataService;
            _jobHistoryService = jobHistoryService;
            _questionnaireImportService = questionnaireImportService;
            _depositInstructionService = depositInstructionService;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="etlExecutionContext"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public async Task StartScanAndProcessFiles(EtlExecutionContext etlExecutionContext)
        {
            const string _methodName = nameof(StartScanAndProcessFiles);
            _logger.LogInformation($"{_methodName}: Stared processing for TenantCode: {etlExecutionContext.TenantCode}, CustomerCode: {etlExecutionContext.CustomerCode} CustomerLabel: {etlExecutionContext.CustomerLabel}");

            try
            {
                var scanS3FileTypes = etlExecutionContext.ScanS3FileTypes;



                var fileNames = await _awsS3Service.GetAllFileNames(Constants.INCOMING_FOLDER);
                if (scanS3FileTypes.Contains(ScanS3FileType.FIS_MONETARY_TXN.ToString()) || scanS3FileTypes.Contains(ScanS3FileType.FIS_NON_MONETARY_TXN.ToString()))
                {
                    // Get Monetary, NonMonetary files from FIS bucket
                    fileNames.AddRange(await _awsS3Service.GetAllFileNames(FISBatchConstants.FIS_DATA_EXTRACT_INBOUND_FOLDER, GetAwsFisSftpS3BucketName()));
                }

                // Get the HSA files from sunny-tmp-dev/hsa/inbound folder
                if (!string.IsNullOrEmpty(etlExecutionContext.ScanS3FileTypes) && etlExecutionContext.ScanS3FileTypes.Contains(nameof(ScanS3FileType.HSA_SWEEP)))
                {
                    fileNames.AddRange(await _awsS3Service.GetAllFileNames(Constants.HSA_INBOUND_FOLDER, GetAwsTmpS3BucketName()));
                }

                if (!string.IsNullOrEmpty(etlExecutionContext.ScanS3FileTypes))
                {
                    var fileTypeList = scanS3FileTypes.Split(',').Select(f => (ScanS3FileType)Enum.Parse(typeof(ScanS3FileType), f.Trim())).ToList();
                    fileNames = FilterFilesByType(fileNames, fileTypeList);
                }

                if (!fileNames.Any())
                {
                    _logger.LogInformation($"StartScanAndProcessFiles : Nothing to process");
                    if (etlExecutionContext.ScanS3FileTypes.Contains(ScanS3FileType.MEMBER_IMPORT.ToString()))
                    {
                        await ProcessSelfLoadMemberImport(etlExecutionContext);
                    }
                    return;
                }

                //Filter task update custom format files
                if (fileNames.Any() && scanS3FileTypes.Contains(ScanS3FileType.TASK_UPDATE.ToString()))
                {
                    fileNames = GetTaskUpdateMatchingFiles(fileNames, etlExecutionContext.CustomFormat);
                }
                _logger.LogInformation($"StartScanAndProcessFiles : total files count to process = {fileNames.Count}, file names={string.Join(",", fileNames)}");

                await MoveFilesToProcessing(fileNames);

                // this contains the master list of all consumers processed so far (either due to enrollment
                // or due to PLD file processing) - this list is then used as the set of consumers for cohorting
                List<ETLConsumerModel> allConsumers = new();

                await ProcessFiles(etlExecutionContext, fileNames, allConsumers);

                if (allConsumers.Any())
                {
                    // need to run cohorting only once per consumer, so select distinct consumers from allConsumers
                    var uniqueConsumers = allConsumers.DistinctBy(x => x.ConsumerCode);
                    await ProcessCohorts(uniqueConsumers);
                }

                await MoveFilesToArchive(fileNames);
                _logger.LogInformation($"{_methodName}: Completed processing.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"{_methodName}: Failed processing. Error:{ex.Message}.\n {ex}");
                throw;
            }
        }

        private async Task ProcessFiles(EtlExecutionContext etlExecutionContext, List<string> fileNames, List<ETLConsumerModel> allConsumers)
        {
            List<string> memberFiles = fileNames.GetMatchingFiles(FilePrefixes.MEMBER_FILE_PREFIX);
            List<string> pldFiles = fileNames.GetMatchingFiles(FilePrefixes.PLD_FILE_PREFIX);
            List<string> consumerAttrFiles = fileNames.GetMatchingFiles(FilePrefixes.CONSUMER_ATTR_FILE_PREFIX);
            List<string> taskImportFiles = fileNames.GetMatchingFiles(FilePrefixes.TASK_IMPORT_FILE_PREFIX);
            List<string> taskUpdateFiles = fileNames.GetMatchingFiles(FilePrefixes.TASK_UPDATE_FILE_PREFIX);
            List<string> memberImportFiles = fileNames.GetMatchingFiles(FilePrefixes.MEMBER_IMPORT_FILE_PREFIX);
            List<string> subscriberOnlyMemberImportFiles = fileNames.GetMatchingFiles(FilePrefixes.SUBSCRIBER_ONLY_MEMBER_IMPORT);
            List<string> fisMonetaryTxnFiles = fileNames.GetMatchingFiles(FilePrefixes.FIS_MONETARY_TXN_FILE_PREFIX);
            fisMonetaryTxnFiles = SortMonetaryTxnFileNames(fisMonetaryTxnFiles);
            List<string> fisNonMonetaryTxnFiles = fileNames.GetMatchingFiles(FilePrefixes.FIS_NON_MONETARY_TXN_FILE_PREFIX);
            fisNonMonetaryTxnFiles = SortNonMonetaryTxnFileNames(fisNonMonetaryTxnFiles);
            List<string> hsaConsumerCodeListFiles = fileNames.GetMatchingFiles(FilePrefixes.HSA_SWEEP_FILE_PREFIX);
            List<string> triviaImportFiles = fileNames.GetMatchingFiles(FilePrefixes.TRIVIA_IMPORT_FILE_PREFIX);
            List<string> questionnaireImportFiles = fileNames.GetMatchingFiles(FilePrefixes.QUESTIONNAIRE_IMPORT_FILE_PREFIX);
            List<string> cohortConsumerFiles = fileNames.GetMatchingFiles(FilePrefixes.COHORT_CONSUMER_IMPORT_FILE_PREFIX);
            List<string> processDepositInstructionConsumerFiles = fileNames.GetMatchingFiles(FilePrefixes.PROCESS_DEPOSIT_INSTRUCTIONS_FILE_PREFIX);
            // Process tenant enrollment and member files
            await ProcessTenantEnrollmentAndMemberFiles(etlExecutionContext, fileNames, allConsumers, memberFiles);

            // Process member import files
            if (memberImportFiles.Count > 0)
                await ProcessMemberImportFiles(etlExecutionContext, fileNames, allConsumers, memberImportFiles);

            // Process subscribe only member import files
            if (subscriberOnlyMemberImportFiles.Count > 0)
                await ProcessMemberImportFiles(etlExecutionContext, fileNames, allConsumers, subscriberOnlyMemberImportFiles);

            // Process member PLD files
            await ProcessMemberPLDFiles(etlExecutionContext, allConsumers, pldFiles);

            // Process consumer attribute files
            await ProcessConsumerAttributeFiles(etlExecutionContext, allConsumers, consumerAttrFiles);

            // Process task import files
            await ProcessTaskImportFiles(etlExecutionContext, taskImportFiles);

            // Process task update files
            await ProcessTaskUpdateFiles(etlExecutionContext, taskUpdateFiles);

            // Process monetary transaction files
            await ProcessMonetaryTransactionFiles(etlExecutionContext, fisMonetaryTxnFiles);

            // Process non monetary transaction files
            await ProcessNonMonetaryTransactionFiles(etlExecutionContext, fisNonMonetaryTxnFiles);

            // Process Hsa Redemption Consumer List Files
            await ProcessHsaRedemptionConsumerListFiles(etlExecutionContext, hsaConsumerCodeListFiles);

            // Process tenant enrollment and member files
            await ProcessTriviaImportFiles(etlExecutionContext, fileNames, triviaImportFiles);

            // Process tenant questionnaire files
            await ProcessQuestionnaireImportFiles(etlExecutionContext, fileNames, questionnaireImportFiles);

            // Process cohort consumer files
            await ProcessCohortConsumerImportFiles(etlExecutionContext, fileNames, cohortConsumerFiles);

            // Process Deposit Instructions for Deposit Instruction files
            await ProcessDepositInstructionsFiles(etlExecutionContext, fileNames, processDepositInstructionConsumerFiles);
        }

        private async Task ProcessHsaRedemptionConsumerListFiles(EtlExecutionContext etlExecutionContext, List<string> hsaRedemptionConsumerListFiles)
        {
            foreach (var file in hsaRedemptionConsumerListFiles)
            {
                try
                {
                    _logger.LogInformation("ProcessHsaRedemptionConsumerListFiles: Started processing file: {file}", file);
                    etlExecutionContext.RedeemConsumerListFilePath = file;
                    await _walletService.RedeemHSA(etlExecutionContext);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing HSA sweep file: {file}", file);
                }
            }
        }

        private async Task ProcessNonMonetaryTransactionFiles(EtlExecutionContext etlExecutionContext, List<string> fisNonMonetaryTxnFiles)
        {
            foreach (var fisNonMonetaryTxnFile in fisNonMonetaryTxnFiles)
            {
                try
                {
                    _logger.LogInformation("StartScanAndProcessFiles: processing FIS Non Monetary Transaction file: {file}", fisNonMonetaryTxnFile);
                    etlExecutionContext.ConsumerNonMonetaryTransactionsFileName = fisNonMonetaryTxnFile;
                    await _consumerNonMonetaryTransactionsFileReadService.ImportConsumerNonMonetaryTransactions(etlExecutionContext);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing FIS Non Monetary Transaction file: {file}", fisNonMonetaryTxnFile);
                }
            }
        }

        private async Task ProcessMonetaryTransactionFiles(EtlExecutionContext etlExecutionContext, List<string> fisMonetaryTxnFiles)
        {
            foreach (var fisMonetaryTxnFile in fisMonetaryTxnFiles)
            {
                try
                {
                    _logger.LogInformation("StartScanAndProcessFiles: processing FIS Monetary Transaction file: {file}", fisMonetaryTxnFile);
                    etlExecutionContext.FISMonetaryTransactionsFileName = fisMonetaryTxnFile;
                    await _monetaryTransactionsFileReadService.ImportMonetaryTransactions(etlExecutionContext);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing FIS Monetary Transaction file: {file}", fisMonetaryTxnFile);
                }
            }
        }

        private static List<string> GetTaskUpdateMatchingFiles(List<string> fileNames, string customFormat)
        {
            var prefix = FilePrefixes.TASK_UPDATE_CUSTOM_FORMAT_FILE_PREFIX;
            var customFormatFiles = fileNames
                .Where(x => x.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (customFormat == Constants.TASK_UPDATE_CUSTOM_FORMAT)
            {
                return customFormatFiles;
            }
            else
            {
                return fileNames.Except(customFormatFiles).ToList();
            }
        }
        
        private async Task ProcessTaskImportFiles(EtlExecutionContext etlExecutionContext, List<string> taskImportFiles)
        {
            const string methodName = nameof(ProcessTaskImportFiles);
            
            _jobReportService.BatchJobRecords.JobType = nameof(TaskImportService);

            foreach (var taskImportFile in taskImportFiles)
            {
                _logger.LogInformation("{ClassName}.{MethodName} Started processing for file: {file}", className,  methodName, taskImportFile);
                
                if (string.IsNullOrEmpty(taskImportFile))
                {
                    _logger.LogError("{ClassName}.{MethodName} No file name found for: {file}", className, methodName, taskImportFile);
                    continue;
                }

                byte[] s3TaskImportFileContent = await _awsS3Service.GetFileFromAwsS3($"{Constants.PROCESSING_FOLDER}/{taskImportFile}" ?? "");

                if (s3TaskImportFileContent.Length == 0)
                {
                    _logger.LogError("{ClassName}.{MethodName} No file content for the given file name: {TaskImportFile}", className, methodName, taskImportFile);
                    return;
                }
                
                etlExecutionContext.TaskImportFilePath = taskImportFile;
                etlExecutionContext.TaskImportFileContents = s3TaskImportFileContent;

                var taskImportFileResponse = await _taskImportService.ImportTaskAsync(etlExecutionContext);

                _jobReportService.JobResultDetails.Files.Add(taskImportFile);
                _jobReportService.JobResultDetails.RecordsReceived = taskImportFileResponse.TotalRecordsReceived;
                _jobReportService.JobResultDetails.RecordsProcessed = taskImportFileResponse.TotalRecordsProcessed;
                _jobReportService.JobResultDetails.RecordsSuccessCount = taskImportFileResponse.TotalSuccessfulRecords;
                _jobReportService.JobResultDetails.RecordsErrorCount = taskImportFileResponse.TotalFailedRecords;
                
                //Save job Details
                await _jobReportService.SaveEtlErrors(_jobReportService.JobResultDetails.ToJson());
                
                _logger.LogInformation("JobResultDetails: {JobResultDetails}", _jobReportService.JobResultDetails.ToJson());
            }
            
            _logger.LogInformation("{ClassName}.{MethodName} Ended processing.", className, methodName);
        }

        private async Task ProcessTaskUpdateFiles(EtlExecutionContext etlExecutionContext, List<string> taskUpdateFiles)
        {
            foreach (var taskUpdateFile in taskUpdateFiles)
            {
                try
                {
                    _logger.LogInformation("StartScanAndProcessFiles: processing Task Update file: {file}", taskUpdateFile);
                    await ProcessTaskUpdateFile(taskUpdateFile, etlExecutionContext: etlExecutionContext);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error: unable to process task update file: {taskUpdateFile}");
                }
            }
        }

        private async Task ProcessConsumerAttributeFiles(EtlExecutionContext etlExecutionContext, List<ETLConsumerModel> allConsumers, List<string> consumerAttrFiles)
        {
            foreach (var consumerAttrFile in consumerAttrFiles)
            {
                _logger.LogInformation("StartScanAndProcessFiles: processing Consumer Attr file: {file}", consumerAttrFile);
                var consumerAttrConsumers = await ProcessConsumerAttrs(etlExecutionContext, consumerAttrFile);
                allConsumers.AddRange(consumerAttrConsumers);
            }
        }

        private async Task ProcessMemberPLDFiles(EtlExecutionContext etlExecutionContext, List<ETLConsumerModel> allConsumers, List<string> pldFiles)
        {
            foreach (var pldFile in pldFiles)
            {
                _logger.LogInformation("StartScanAndProcessFiles: processing PLD file: {file}", pldFile);
                var pldConsumers = await ProcessPldFile(etlExecutionContext, pldFile);
                allConsumers.AddRange(pldConsumers);
            }
        }

        private async Task ProcessMemberImportFiles(EtlExecutionContext etlExecutionContext, List<string> fileNames, List<ETLConsumerModel> allConsumers, List<string> memberImportFiles)
        {
            foreach (var memberImportFile in memberImportFiles)
            {
                try
                {
                    var memberFile = fileNames.GetFile(memberImportFile);

                    if (string.IsNullOrEmpty(memberFile))
                    {
                        _logger.LogError("StartScanAndProcessFiles: Error: unable to process consumer import file: {memFile}", memberFile);
                        continue;
                    }

                    _logger.LogInformation("StartScanAndProcessFiles: processing consumer import files: {memFile}", memberFile);
                    var importedConsumers = await ProcessMemberImport(etlExecutionContext, memberFile);
                    allConsumers.AddRange(importedConsumers);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "StartScanAndProcessFiles: Error: unable to process consumer import file: {memFile}", memberImportFile);
                }
            }

            //Save job Details
            etlExecutionContext = _jobReportService.SetJobHistoryStatus(etlExecutionContext);
            if (!string.IsNullOrEmpty(etlExecutionContext.JobHistoryId))
            {
                var jobHistory = await _jobHistoryService.GetJobHistoryById(etlExecutionContext.JobHistoryId);

                //Update job History status to STARTED
                jobHistory.RunStatus = etlExecutionContext.JobHistoryStatus;
                jobHistory.ErrorLog = etlExecutionContext.JobHistoryErrorLog;
                await _jobHistoryService.UpdateJobHistory(jobHistory);
            }
        }


        /// <summary>
        /// Processes the cohort consumer import files.
        /// </summary>
        /// <param name="etlExecutionContext">The etl execution context.</param>
        /// <param name="fileNames">The file names.</param>
        /// <param name="cohortConsumerImportFiles">The cohort consumer import files.</param>
        private async Task ProcessCohortConsumerImportFiles(EtlExecutionContext etlExecutionContext, List<string> fileNames, List<string> cohortConsumerImportFiles)
        {
            foreach (var cohortConsumerImportFile in cohortConsumerImportFiles)
            {
                var cohortConsumerFile = fileNames.GetFile(cohortConsumerImportFile);

                if (string.IsNullOrEmpty(cohortConsumerFile))
                {
                    _logger.LogError("ProcessCohortConsumerImportFiles: Error: unable to process cohort consumer import file: {memFile}", cohortConsumerFile);
                    continue;
                }

                _logger.LogInformation("ProcessCohortConsumerImportFiles: processing cohort consumer import files: {memFile}", cohortConsumerFile);
                await ProcessCohortConsumerImport(etlExecutionContext, cohortConsumerFile);
            }

            if (cohortConsumerImportFiles.Count > 0)
            {
                await _jobReportService.SaveEtlErrors();
            }
        }

        private async Task ProcessTriviaImportFiles(EtlExecutionContext etlExecutionContext, List<string> fileNames, List<string> triviaImportFiles)
        {
            foreach (var triviaImportFile in triviaImportFiles)
            {
                try
                {
                    var triviaFile = fileNames.GetFile(triviaImportFile);

                    if (string.IsNullOrEmpty(triviaFile))
                    {
                        _logger.LogError("StartScanAndProcessFiles: Error: unable to process trivia import file: {triviaFile}", triviaFile);
                        continue;
                    }

                    _logger.LogInformation("StartScanAndProcessFiles: processing trivia import files: {triviaFile}", triviaFile);
                    await ProcessTriviaImport(etlExecutionContext, triviaFile);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing trivia import file: {triviaImportFile}", triviaImportFile);
                }
            }
        }
        private async Task ProcessQuestionnaireImportFiles(EtlExecutionContext etlExecutionContext, List<string> fileNames, List<string> questionnaireImportFiles)
        {
            foreach (var questionnaireImportFile in questionnaireImportFiles)
            {
                try
                {
                    var questionnaireFile = fileNames.GetFile(questionnaireImportFile);

                    if (string.IsNullOrEmpty(questionnaireFile))
                    {
                        _logger.LogError("StartScanAndProcessFiles: Error: unable to process questionnaire import file: {questionnaireFile}", questionnaireFile);
                        continue;
                    }

                    _logger.LogInformation("StartScanAndProcessFiles: processing questionnaire import files: {questionnaireFile}", questionnaireFile);
                    await ProcessQuestionnaireImport(etlExecutionContext, questionnaireFile);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing questionnaire import file: {questionnaireImportFile}", questionnaireImportFile);
                }
            }
        }

        private async Task ProcessTenantEnrollmentAndMemberFiles(EtlExecutionContext etlExecutionContext, List<string> fileNames, List<ETLConsumerModel> allConsumers, List<string> memberFiles)
        {
            foreach (var memberFile in memberFiles)
            {
                try
                {
                    var enrollFileName = memberFile.ToLower().Replace(FilePrefixes.MEMBER_FILE_PREFIX, FilePrefixes.ENROLLMENT_FILE_PREFIX);
                    var enrollmentFile = fileNames.GetFile(enrollFileName);

                    if (!string.IsNullOrEmpty(memberFile) && !string.IsNullOrEmpty(enrollmentFile))
                    {
                        _logger.LogInformation("StartScanAndProcessFiles: processing Member/Enrollment files: {memFile}, {enrFile}", memberFile, enrollmentFile);
                        var enrollmentConsumers = await ProcessMemberLoad(etlExecutionContext, memberFile, enrollmentFile);
                        allConsumers.AddRange(enrollmentConsumers);
                    }
                    else
                    {
                        _logger.LogError("StartScanAndProcessFiles: Error: unable to process Member file: {memFile} " +
                            "as matching Enrollment file not found: {enrFile}", memberFile, enrollmentFile);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error: unable to process Member file: {memberFile}");
                }
            }
        }

        /// <summary>
        /// Processes the deposit instructions files.
        /// </summary>
        /// <param name="etlExecutionContext"></param>
        /// <param name="fileNames">List of Filenames</param>
        /// <param name="depositInstructionsFiles">depositInstruction files</param>
        /// <returns></returns>
        private async Task ProcessDepositInstructionsFiles(EtlExecutionContext etlExecutionContext, List<string> fileNames, List<string> depositInstructionsFiles)
        {
            const string methodName = nameof(ProcessDepositInstructionsFiles);
            foreach (var depositInstructionFile in depositInstructionsFiles)
            {
                try
                {
                    var processDepositInstructionFile = fileNames.GetFile(depositInstructionFile);

                    if (string.IsNullOrEmpty(processDepositInstructionFile))
                    {
                        _logger.LogError("{MethodName}: Error: As File is not available in incoming folder and File Name: {memFile}", methodName, processDepositInstructionFile);
                        continue;
                    }

                    _logger.LogInformation("{MethodName}: processing DepositInstructionsFiles: {memFile}", methodName, processDepositInstructionFile);
                    await ProcessDepositInstruction(etlExecutionContext, processDepositInstructionFile);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "{MethodName}: Error: unable to process Files: {memFile}", methodName, depositInstructionFile);
                    continue;
                }
                
            }

            if (depositInstructionsFiles.Count > 0)
            {
                await _jobReportService.SaveEtlErrors();
            }
        }

        private List<string> FilterFilesByType(List<string> fileNames, List<ScanS3FileType> fileTypes)
        {
            var fileTypeToPrefixesMap = new Dictionary<ScanS3FileType, List<string>>
            {
                { ScanS3FileType.MEMBER_IMPORT, new List<string> { FilePrefixes.MEMBER_IMPORT_FILE_PREFIX } },
                { ScanS3FileType.TASK_IMPORT, new List<string> { FilePrefixes.TASK_IMPORT_FILE_PREFIX } },
                { ScanS3FileType.TASK_UPDATE, new List<string> { FilePrefixes.TASK_UPDATE_FILE_PREFIX } },
                { ScanS3FileType.TRIVIA_IMPORT, new List<string> { FilePrefixes.TRIVIA_IMPORT_FILE_PREFIX } },
                { ScanS3FileType.FIS_NON_MONETARY_TXN, new List<string> { FilePrefixes.FIS_NON_MONETARY_TXN_FILE_PREFIX } },
                { ScanS3FileType.FIS_MONETARY_TXN, new List<string> { FilePrefixes.FIS_MONETARY_TXN_FILE_PREFIX } },
                { ScanS3FileType.HSA_SWEEP, new List<string> { FilePrefixes.HSA_SWEEP_FILE_PREFIX } },
                { ScanS3FileType.MEMBER_TENANT_ENROLLMENT, new List<string> { FilePrefixes.MEMBER_FILE_PREFIX, FilePrefixes.ENROLLMENT_FILE_PREFIX } },
                { ScanS3FileType.PROCESS_PLD, new List<string> { FilePrefixes.PLD_FILE_PREFIX } },
                { ScanS3FileType.CONSUMER_ATTRIBUTES, new List<string> { FilePrefixes.CONSUMER_ATTR_FILE_PREFIX } },
                { ScanS3FileType.SUBSCRIBER_ONLY_MEMBER_IMPORT, new List<string> { FilePrefixes.SUBSCRIBER_ONLY_MEMBER_IMPORT } },
                { ScanS3FileType.COHORTCONSUMER, new List<string> { FilePrefixes.COHORT_CONSUMER_IMPORT_FILE_PREFIX } },
                { ScanS3FileType.QUESTIONNAIRE_IMPORT, new List<string> { FilePrefixes.QUESTIONNAIRE_IMPORT_FILE_PREFIX } },
                { ScanS3FileType.PROCESS_DEPOSIT_INSTRUCTIONS_FILE, new List<string> { FilePrefixes.PROCESS_DEPOSIT_INSTRUCTIONS_FILE_PREFIX } }
            };

            var filteredFiles = new List<string>();

            foreach (var fileType in fileTypes)
            {
                if (fileTypeToPrefixesMap.TryGetValue(fileType, out var prefixes))
                {
                    foreach (var prefix in prefixes)
                    {
                        filteredFiles.AddRange(fileNames.GetMatchingFiles(prefix));
                    }
                }
                else
                {
                    _logger.LogWarning($"FilterFilesByType: Unknown or unsupported file type: {fileType}");
                }
            }

            return filteredFiles;
        }

        private List<string> ExcludedFiles(List<string> fileNames)
        {
            return fileNames
                .Where(file => !file.StartsWith(FilePrefixes.FIS_MONETARY_TXN_FILE_PREFIX, StringComparison.OrdinalIgnoreCase)
                            && !file.StartsWith(FilePrefixes.FIS_NON_MONETARY_TXN_FILE_PREFIX, StringComparison.OrdinalIgnoreCase)
                            && !file.StartsWith(FilePrefixes.HSA_SWEEP_FILE_PREFIX, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }
        private List<string> SortTxnFileNames(List<string> fileNames, int dateStartIndex, int dateLength = 8, string dateFormat = "MMddyyyy")
        {
            return fileNames
                .Select(fileName =>
                {
                    var dateString = fileName.Substring(dateStartIndex, dateLength);
                    DateTime fileDate = DateTime.ParseExact(dateString, dateFormat, CultureInfo.InvariantCulture);
                    return new { fileName, fileDate };
                })
                .OrderBy(file => file.fileDate)
                .Select(file => file.fileName)
                .ToList();
        }

        private List<string> SortMonetaryTxnFileNames(List<string> fileNames)
        {
            return SortTxnFileNames(fileNames, dateStartIndex: FISBatchConstants.MON_TXN_FILE_DATE_START_INDEX);
        }

        private List<string> SortNonMonetaryTxnFileNames(List<string> fileNames)
        {
            return SortTxnFileNames(fileNames, dateStartIndex: FISBatchConstants.NON_MON_TXN_FILE_DATE_START_INDEX);
        }


        private string GetAwsFisSftpS3BucketName()
        {
            return _configuration.GetSection("AWS:AWS_FIS_SFTP_BUCKET_NAME").Value?.ToString() ?? "";
        }

        private string GetAwsTmpS3BucketName()
        {
            return _configuration.GetSection("AWS:AWS_TMP_BUCKET_NAME").Value?.ToString() ?? "";
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="etlExecutionContext"></param>
        /// <param name="consumerAttrFile"></param>
        /// <returns></returns>
        private async Task<List<ETLConsumerModel>> ProcessConsumerAttrs(EtlExecutionContext etlExecutionContext, string consumerAttrFile)
        {
            _logger.LogDebug($"StartScanAndProcessFiles : Processing of ConsumerAttr file start");

            byte[] s3ConsumerAttrFileContent = await _awsS3Service.GetFileFromAwsS3($"{Constants.PROCESSING_FOLDER}/{consumerAttrFile}" ?? "");
            var consumers = await _ingestConsumerAttrService.Ingest(etlExecutionContext.TenantCode, s3ConsumerAttrFileContent);

            _logger.LogDebug($"StartScanAndProcessFiles : Processing of ConsumerAttr file completed");

            return consumers;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pldConsumers"></param>
        /// <returns></returns>
        private async Task ProcessCohorts(IEnumerable<ETLConsumerModel> pldConsumers)
        {
            if (pldConsumers != null)
            {
                var etlPersons = new List<ETLPersonModel>();
                var etlConsumers = new List<ETLConsumerModel>();
                foreach (var pldConsumer in pldConsumers)
                {

                    var person = _personRepo.FindOneAsync(x => x.PersonId == pldConsumer.PersonId && x.DeleteNbr == 0).Result;
                    if (person != null)
                    {
                        etlPersons.Add(person);
                        etlConsumers.Add(pldConsumer);
                    }
                }
                await _cohortService.ProcessCohorts(etlConsumers, etlPersons);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="etlExecutionContext"></param>
        /// <param name="memberFile"></param>
        /// <param name="enrollmentFile"></param>
        /// <returns></returns>
        private async Task<List<ETLConsumerModel>> ProcessMemberLoad(EtlExecutionContext etlExecutionContext, string? memberFile, string? enrollmentFile)
        {

            if (string.IsNullOrEmpty(enrollmentFile) || string.IsNullOrEmpty(memberFile))
            {
                _logger.LogError("ProcessMemberLoad : Combination of member and enrollment files does not exist. Can't process further.");
                return new List<ETLConsumerModel>();
            }

            byte[] s3MemberFileContent = await _awsS3Service.GetFileFromAwsS3($"{Constants.PROCESSING_FOLDER}/{memberFile}" ?? "");
            byte[] s3enrollmentFileContent = await _awsS3Service.GetFileFromAwsS3($"{Constants.PROCESSING_FOLDER}/{enrollmentFile}" ?? "");

            _logger.LogDebug("ProcessMemberLoad : Processing of LoadMemberFiles start: {memFile}, {enrFile}", memberFile, enrollmentFile);

            etlExecutionContext.MemberFileContents = s3MemberFileContent;
            etlExecutionContext.EnrolmentFileContents = s3enrollmentFileContent;

            (var loadedConsumers, var loadedPersons) = await _enrollmentService.ProcessTenantEnrollments(etlExecutionContext);

            _logger.LogDebug("ProcessMemberLoad : Processing of LoadMemberFiles completed: {memFile}, {enrFile}", memberFile, enrollmentFile);

            return loadedConsumers;
        }


        /// <summary>
        /// Processes the cohort consumer import.
        /// </summary>
        /// <param name="etlExecutionContext">The etl execution context.</param>
        /// <param name="cohortConsumerFile">The cohort consumer file.</param>
        private async Task ProcessCohortConsumerImport(EtlExecutionContext etlExecutionContext, string? cohortConsumerFile)
        {
            const string _methodName = nameof(ProcessCohortConsumerImport);
            _logger.LogInformation($"{_methodName}: Stared processing for memberFile: {cohortConsumerFile}");

            try
            {
                if (string.IsNullOrEmpty(cohortConsumerFile))
                {
                    _logger.LogError("ProcessCohortConsumerImport : cohort consumer import files does not exist. Can't process further.");
                    return;
                }

                byte[] s3CohortConsumerFileContent = await _awsS3Service.GetFileFromAwsS3($"{Constants.PROCESSING_FOLDER}/{cohortConsumerFile}" ?? "");

                etlExecutionContext.CohortConsumerImportFileContents = s3CohortConsumerFileContent;
                etlExecutionContext.CohortConsumerImportFilePath = cohortConsumerFile;
                await _cohortConsumerService.Import(etlExecutionContext);

                _logger.LogInformation($"{_methodName}: Completed processing.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"{_methodName}: Failed processing. Error:{ex.Message}.\n {ex}");
                throw;
            }
        }

        private async Task<List<ETLConsumerModel>> ProcessMemberImport(EtlExecutionContext etlExecutionContext, string? memberFile)
        {
            const string _methodName = nameof(ProcessMemberImport);
            _logger.LogInformation($"{_methodName}: Stared processing for memberFile: {memberFile}");

            try
            {
                if (string.IsNullOrEmpty(memberFile))
                {
                    _logger.LogError("ProcessMemberLoad : Consumers import files does not exist. Can't process further.");
                    return new List<ETLConsumerModel>();
                }

                byte[] s3MemberImportFileContent = await _awsS3Service.GetFileFromAwsS3($"{Constants.PROCESSING_FOLDER}/{memberFile}" ?? "");

                etlExecutionContext.MemberImportFileContents = s3MemberImportFileContent;
                etlExecutionContext.MemberImportFilePath = memberFile;

                (var loadedConsumers, var loadedPersons) = await _memberImportService.Import(etlExecutionContext);

                _logger.LogInformation($"{_methodName}: Completed processing.");

                return loadedConsumers;

            }
            catch (Exception ex)
            {
                _logger.LogError($"{_methodName}: Failed processing. Error:{ex.Message}.\n {ex}");
                throw;
            }
        }

        private async Task<List<ETLConsumerModel>?> ProcessSelfLoadMemberImport(EtlExecutionContext etlExecutionContext)
        {
            const string _methodName = nameof(ProcessSelfLoadMemberImport);
           
            try
            {
                if (etlExecutionContext.ScanS3FileTypes.Contains(ScanS3FileType.MEMBER_IMPORT.ToString()))
                {
                    (var loadedConsumers, var loadedPersons) = await _memberImportService.Import(etlExecutionContext);
                    _logger.LogInformation($"{_methodName}: Completed processing.");

                    //Save job Details
                    etlExecutionContext = _jobReportService.SetJobHistoryStatus(etlExecutionContext);
                    if (!string.IsNullOrEmpty(etlExecutionContext.JobHistoryId))
                    {
                        var jobHistory = await _jobHistoryService.GetJobHistoryById(etlExecutionContext.JobHistoryId);

                        //Update job History status to STARTED
                        jobHistory.RunStatus = etlExecutionContext.JobHistoryStatus;
                        jobHistory.ErrorLog = etlExecutionContext.JobHistoryErrorLog;
                        await _jobHistoryService.UpdateJobHistory(jobHistory);
                    }

                    return loadedConsumers;
                }
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError($"{_methodName}: Failed processing. Error:{ex.Message}.\n {ex}");
                throw;
            }


        }


        private async Task ProcessTriviaImport(EtlExecutionContext etlExecutionContext, string? triviaFile)
        {
            const string _methodName = nameof(ProcessTriviaImport);
            _logger.LogInformation($"{_methodName}: Stared processing for triviaFile: {triviaFile}");

            try
            {
                if (string.IsNullOrEmpty(triviaFile))
                {
                    _logger.LogError("{methodName} : Trivia import files does not exist. Can't process further.", _methodName);
                }

                byte[] s3TriviaImportFileContent = await _awsS3Service.GetFileFromAwsS3($"{Constants.PROCESSING_FOLDER}/{triviaFile}" ?? "");

                etlExecutionContext.TriviaImportFileContents = s3TriviaImportFileContent;
                etlExecutionContext.TriviaImportFilePath = triviaFile!;
                await _triviaImportService.Import(etlExecutionContext);

                _logger.LogInformation($"{_methodName}: Completed processing.");

            }
            catch (Exception ex)
            {
                _logger.LogError($"{_methodName}: Failed processing. Error:{ex.Message}.\n {ex}");
                throw;
            }
        }

        private async Task ProcessQuestionnaireImport(EtlExecutionContext etlExecutionContext, string? questionnaireFile)
        {
            const string _methodName = nameof(ProcessQuestionnaireImport);
            _logger.LogInformation($"{_methodName}: Stared processing for questionnaireFile: {questionnaireFile}");

            try
            {
                if (string.IsNullOrEmpty(questionnaireFile))
                {
                    _logger.LogError("{methodName} : questionnaire import files does not exist. Can't process further.", _methodName);
                }

                byte[] s3QuestionnaireImportFileContent = await _awsS3Service.GetFileFromAwsS3($"{Constants.PROCESSING_FOLDER}/{questionnaireFile}" ?? "");

                etlExecutionContext.QuestionnaireImportFileContents = s3QuestionnaireImportFileContent;
                etlExecutionContext.QuestionnaireImportFilePath = questionnaireFile!;
                await _questionnaireImportService.Import(etlExecutionContext);

                _logger.LogInformation($"{_methodName}: Completed processing.");

            }
            catch (Exception ex)
            {
                _logger.LogError($"{_methodName}: Failed processing. Error:{ex.Message}.\n {ex}");
                throw;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileNames"></param>
        /// <returns></returns>
        private async Task ProcessTaskUpdateFile(string? taskUpdateFile, EtlExecutionContext etlExecutionContext)
        {
            if (!string.IsNullOrEmpty(taskUpdateFile))
            {
                byte[] s3TaskUpdateFileContent = await _awsS3Service.GetFileFromAwsS3($"{Constants.PROCESSING_FOLDER}/{taskUpdateFile}");

                _logger.LogDebug($"StartScanAndProcessFiles : Processing of ProcessTaskUpdates start");

                await _taskUpdateService.ProcessTaskUpdates(taskUpdateFileContent: s3TaskUpdateFileContent, etlExecutionContext: etlExecutionContext);

                _logger.LogDebug($"StartScanAndProcessFiles : Processing of ProcessTaskUpdates completed");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="etlExecutionContext"></param>
        /// <param name="pldFile"></param>
        /// <returns></returns>
        private async Task<List<ETLConsumerModel>> ProcessPldFile(EtlExecutionContext etlExecutionContext, string? pldFile)
        {
            List<ETLConsumerModel> pldConsumers = new List<ETLConsumerModel>();
            if (!string.IsNullOrEmpty(pldFile))
            {
                byte[] s3PldFileContent = await _awsS3Service.GetFileFromAwsS3($"{Constants.PROCESSING_FOLDER}/{pldFile}");

                _logger.LogDebug($"StartScanAndProcessFiles : Processing of ProcessConsumerAttrUsingPldFile start");

                pldConsumers = await ProcessConsumerAttrUsingPldFile(etlExecutionContext.TenantCode, pldFileContent: s3PldFileContent);

                _logger.LogDebug($"StartScanAndProcessFiles : Processing of ProcessConsumerAttrUsingPldFile completed");
            }
            return pldConsumers;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileNames"></param>
        /// <returns></returns>
        private async Task MoveFilesToProcessing(List<string> fileNames)
        {
            var filteredFiles = ExcludedFiles(fileNames);
            foreach (var file in filteredFiles)
            {
                await _awsS3Service.MoveFileInAwsS3($"{Constants.INCOMING_FOLDER}/{file}", $"{Constants.PROCESSING_FOLDER}/{file}");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileNames"></param>
        /// <returns></returns>
        private async Task MoveFilesToArchive(List<string> fileNames)
        {
            var filteredFiles = ExcludedFiles(fileNames);
            foreach (var file in filteredFiles)
            {
                await _awsS3Service.MoveFileInAwsS3($"{Constants.PROCESSING_FOLDER}/{file}", $"{Constants.ARCHIVE_FOLDER}/{file}");
            }
        }

        /// <summary>
        /// Processes the deposit instruction fetch the file from S3 and call the Deposit service to process the file.
        /// </summary>
        /// <param name="etlExecutionContext"></param>
        /// <param name="depositInstructionFile"></param>
        /// <returns></returns>
        private async Task ProcessDepositInstruction(EtlExecutionContext etlExecutionContext, string? depositInstructionFile)
        {
            const string methodName = nameof(ProcessDepositInstruction);
            _logger.LogInformation($"{methodName}: Stared processing for DepositInstructionFile: {depositInstructionFile}");

            try
            {
                if (string.IsNullOrEmpty(depositInstructionFile))
                {
                    _logger.LogError("{methodName}: ProcessDepositInstruction files does not exist. Can't process further.", methodName);
                    throw new Exception($"No depositInstructionFile provided for Process Deposit Instruction File");
                }

                // Fetch the file from S3
                byte[] s3EligibleConsumersFileContent = await _awsS3Service.GetFileFromAwsS3($"{Constants.PROCESSING_FOLDER}/{depositInstructionFile}" ?? "");

                etlExecutionContext.DepositIntructionEligibleConsumersFileContents = s3EligibleConsumersFileContent;
                etlExecutionContext.DepositInstructionFilePath = depositInstructionFile!;
                await _depositInstructionService.ProcessDepositInstructionFile(etlExecutionContext);

                _logger.LogInformation($"{methodName}: Completed processing.");

            }
            catch (Exception ex)
            {
                _logger.LogError($"{methodName}: Failed processing. Error:{ex.Message}.\n {ex}");
                throw;
            }
        }
    }
}
