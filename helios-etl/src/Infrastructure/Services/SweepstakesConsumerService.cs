using CsvHelper.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SunnyRewards.Helios.Cohort.Core.Domain.Dtos;
using SunnyRewards.Helios.Common.Core.Extensions;
using SunnyRewards.Helios.Common.Core.Helpers.Interfaces;
using SunnyRewards.Helios.ETL.Common.Constants;
using SunnyRewards.Helios.ETL.Common.CustomException;
using SunnyRewards.Helios.ETL.Core.Domain.Constants;
using SunnyRewards.Helios.ETL.Core.Domain.Dtos;
using SunnyRewards.Helios.ETL.Core.Domain.Dtos.Json;
using SunnyRewards.Helios.ETL.Core.Domain.Models;
using SunnyRewards.Helios.ETL.Infrastructure.AwsConfig;
using SunnyRewards.Helios.ETL.Infrastructure.Helpers.Interfaces;
using SunnyRewards.Helios.ETL.Infrastructure.Repositories.HeliosRepo.Interfaces;
using SunnyRewards.Helios.ETL.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.ETL.Infrastructure.Services.Interfaces.DynamoDb;
using SunnyRewards.Helios.Tenant.Core.Domain.Dtos.Json;
using System.Globalization;

namespace SunnyRewards.Helios.ETL.Infrastructure.Services.FIS
{
    public class SweepstakesConsumerService : AwsConfiguration, ISweepstakesConsumerService
    {
        private readonly ISweepstakesInstanceRepo _sweepstakesInstanceRepo;
        private readonly IPgpS3FileEncryptionHelper _s3FileEncryptionHelper;
        private readonly ILogger<SweepstakesConsumerService> _logger;
        private readonly IConsumerRepo _consumerRepo;
        private readonly ISweepstakesResultRepo _sweepstakesResultRepo;
        private readonly IS3Helper _s3Helper;
        private readonly ICohortConsumerService _cohortConsumerService;
        private readonly ITenantRepo _tenantRepo;
        private readonly ITaskUpdateService _taskUpdateService;
        private readonly IAwsS3Service _awsS3Service;
        private readonly IJobHistoryService _jobHistoryService;
        private readonly IAwsBatchService _awsBatchService;
        private readonly IPersonRepo _personRepo;
        private readonly IWalletRepo _walletRepo;
        private readonly IWalletTypeRepo _walletTypeRepo;
        private readonly IConfiguration _configuration;
        private const string className = nameof(SweepstakesConsumerService);

        public SweepstakesConsumerService(ISweepstakesInstanceRepo sweepstakesInstanceRepo, ILogger<SweepstakesConsumerService> logger, IVault vault, IConfiguration configuration,
            IConsumerRepo consumerRepo, IPgpS3FileEncryptionHelper s3FileEncryptionHelper, IS3Helper s3Helper,
            ISweepstakesResultRepo sweepstakesResultRepo, ICohortConsumerService cohortConsumerService, ITenantRepo tenantRepo,
            ITaskUpdateService taskUpdateService, IAwsS3Service awsS3Service, IJobHistoryService jobHistoryService,
            IAwsBatchService awsBatchService, IPersonRepo personRepo, IWalletRepo walletRepo, IWalletTypeRepo walletTypeRepo
           ) : base(vault, configuration)
        {
            _logger = logger;
            _sweepstakesInstanceRepo = sweepstakesInstanceRepo;
            _consumerRepo = consumerRepo;
            _s3FileEncryptionHelper = s3FileEncryptionHelper;
            _sweepstakesResultRepo = sweepstakesResultRepo;
            _s3Helper = s3Helper;
            _cohortConsumerService = cohortConsumerService;
            _tenantRepo = tenantRepo;
            _taskUpdateService = taskUpdateService;
            _awsS3Service = awsS3Service;
            _jobHistoryService = jobHistoryService;
            _awsBatchService = awsBatchService;
            _personRepo = personRepo;
            _walletRepo = walletRepo;
            _walletTypeRepo = walletTypeRepo;
            _configuration = configuration;
        }
        public async Task ConsumeSweepstakesWinnerReportAsync(EtlExecutionContext etlExecutionContext)
        {
            const string methodName = nameof(ConsumeSweepstakesWinnerReportAsync);
            _logger.LogInformation("{ClassName}.{MethodName} - Starting to process Sweepstakes Winner Report", className, methodName);
            var sweepstakesInstance = new ETLSweepstakesInstanceModel();
            var depositRecords = new List<ConsumerDepositInstructionRecord>();
            string? tenantCode = string.Empty;
            try
            {
                //Check Provided format is valid format
                if (!etlExecutionContext.Format.Equals(Constants.RealtimeMedia, StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogError("{ClassName}.{MethodName} - Invalid media type", className, methodName);
                    throw new ETLException(ETLExceptionCodes.InValidValue, $"Invalid media type: {etlExecutionContext.Format}");
                }

                //validate for valid Sweeptakes Instances
                sweepstakesInstance = await _sweepstakesInstanceRepo.FindOneAsync(x => x.SweepstakesInstanceId == etlExecutionContext.SweepstakesInstanceId && x.DeleteNbr == 0);
                if (sweepstakesInstance == null)
                {
                    _logger.LogError("{ClassName}.{MethodName} - Invalid sweepstakes Instance Id: {Id}", className, methodName, etlExecutionContext.SweepstakesInstanceId);
                    throw new ETLException(ETLExceptionCodes.NotFoundInDb, $"Invalid sweepstakes Instance Id: {etlExecutionContext.SweepstakesInstanceId}");
                }

                if (sweepstakesInstance.Status != Constants.SWEEPSTAKES_ENTRIES_REPORT_SUCCESS_STATUS
                    && sweepstakesInstance.Status != Constants.SWEEPSTAKES_WINNERS_REPORT_ERROR_STATUS)
                {
                    _logger.LogError("{ClassName}.{MethodName} - Invalid sweepstakes Instance status: {status} with sweepstakes instance id: {Id}",
                        className, methodName, sweepstakesInstance.Status, etlExecutionContext.SweepstakesInstanceId);
                    throw new ETLException(ETLExceptionCodes.NotFoundInDb, $"Invalid sweepstakes Instance status: {sweepstakesInstance.Status} with sweepstakes instance id: {etlExecutionContext.SweepstakesInstanceId}");
                }

                // update sweepstakes instance status to inprogress
                _logger.LogInformation("{ClassName}.{MethodName} - Updating sweepstakes instance status to {Status}",
                    className, methodName, Constants.SWEEPSTAKES_WINNERS_REPORT_INPROGRESS_STATUS);
                sweepstakesInstance.Status = Constants.SWEEPSTAKES_WINNERS_REPORT_INPROGRESS_STATUS;
                await _sweepstakesInstanceRepo.UpdateAsync(sweepstakesInstance);

                // get File from s3
                var s3FileDownloadRequestDto = new S3FileDownloadRequestDto
                {
                    SourceBucketName = GetAwsSweepstakesSftpS3BucketName(),
                    SourceFileName = Constants.SWEEPSTAKES_CONSUME_FILENAME,
                    SourceFolderName = Constants.SWEEPSTAKES_INBOUND_FOLDER,
                    InboundArchiveFolderName = Constants.SWEEPSTAKES_INBOUND_ARCHIVE_FOLDER,
                    ArchiveBucketName = GetAwsSweepstakesSftpS3BucketName(),
                    DeleteFileAfterCopy = false
                };

                var response = await _s3FileEncryptionHelper.DownloadLatestFileByName(s3FileDownloadRequestDto);
                if (response == null || response.FileContent == null || string.IsNullOrEmpty(response.FileName))
                {
                    _logger.LogError("{ClassName}.{MethodName} - Failed to download file. No response or empty file content returned from DownloadLatestFileByName. Bucket: {BucketName}, FilePath: {FilePath}", className, methodName,
                        s3FileDownloadRequestDto.SourceBucketName, $"{s3FileDownloadRequestDto.SourceFolderName}/{s3FileDownloadRequestDto.SourceFileName}");

                    throw new ETLException(ETLExceptionCodes.InValidValue,
                        $"No response or empty file content returned from DownloadLatestFileByName. Bucket: {s3FileDownloadRequestDto.SourceBucketName}, FilePath: {s3FileDownloadRequestDto.SourceFolderName}/{s3FileDownloadRequestDto.SourceFileName}");
                }
                #region Deserializing Prize description JSON & validate winners count over file
                // Deserializing Prize description JSON
                var prizeDescription = sweepstakesInstance.PrizeDescriptionJson != null
                    ? JsonConvert.DeserializeObject<PrizeDescription>(sweepstakesInstance.PrizeDescriptionJson)
                    : new PrizeDescription { PrizeDetails = new List<PrizeDetail>() };

                if (prizeDescription?.PrizeDetails == null || !prizeDescription.PrizeDetails.Any())
                {
                    _logger.LogError("{className}.{methodName}: Prize Description json or PrizeDetails is null or empty For sweepstakes instanceId: {instanceId}",
                        className, methodName, etlExecutionContext.SweepstakesInstanceId);
                    throw new ETLException(ETLExceptionCodes.InValidValue, $"Prize Description json or PrizeDetails is null or empty For sweepstakes instanceId: {etlExecutionContext.SweepstakesInstanceId}");
                }
                prizeDescription.PrizeDetails = prizeDescription.PrizeDetails.OrderBy(x => x.WinnerPosition).ToList();
                var totalWinnersCountFromInstanceConfig = prizeDescription?.PrizeDetails?.Sum(x => x.MaxWinners);

                //Read winners count from the file
                int winnersCountFromFile = 0;
                List<string> winnersAnonymusList = new List<string>();
                using (StreamReader reader = new StreamReader(new MemoryStream(response.FileContent)))
                {
                    string line;
                    while ((line = await reader.ReadLineAsync()) != null)
                    {
                        if (winnersCountFromFile > 0)
                        {
                            winnersAnonymusList.Add(line.Trim());
                        }
                        winnersCountFromFile++;
                    }
                    //Removing the header count from winners count
                    winnersCountFromFile--;
                }
                //Winners count from the file and sweepstakes_instance prizeDescription must be equal
                if (winnersCountFromFile != totalWinnersCountFromInstanceConfig)
                {
                    _logger.LogError("{className}.{methodName}: Winners count from the file ({WinnersCountFromFile}) and sweepstakes instance prize description json ({TotalWinnersCountFromInstanceConfig}) didn't match. " +
                        "Sweepstakes instanceId: {instanceId}",
                        className, methodName, winnersCountFromFile, totalWinnersCountFromInstanceConfig, etlExecutionContext.SweepstakesInstanceId);
                    throw new ETLException(ETLExceptionCodes.InValidValue, $"Winners count from the file ({winnersCountFromFile}) " +
                        $"and sweepstakes instance prize description json ({totalWinnersCountFromInstanceConfig}) didn't match. for sweepstakes instanceId: {etlExecutionContext.SweepstakesInstanceId}");
                }

                #endregion
                int winnerNumber = 1;
                int prizeIdentifier = 0;
                TenantAttribute? tenantAttributes = null;
                TenantOption? tenantOptions = null;
                foreach (var anonymousCode in winnersAnonymusList)
                {
                    var consumer = await _consumerRepo.GetNonSyntheticConsumer(anonymousCode);
                    PrizeDetail? winnerPrizeDetail = null;
                    if (consumer != null)
                    {
                        //Identify the winner detail based on the position in the file and maxWinners from instance config
                        var prizeDescribeJson = string.Empty;
                        int? totalPastPrizeWinners = 0;
                        foreach (var prizeDetail in prizeDescription?.PrizeDetails ?? Enumerable.Empty<PrizeDetail>())
                        {
                            totalPastPrizeWinners += prizeDetail?.MaxWinners;
                            if (winnerNumber <= totalPastPrizeWinners)
                            {
                                prizeDescribeJson = prizeDetail == null
                                    ? string.Empty
                                    : prizeDetail.ToJson();
                                winnerPrizeDetail = prizeDetail;
                                break;
                            }
                        }

                        prizeIdentifier++;
                        var sweepstakesResult = new ETLSweepstakesResultModel();
                        tenantCode = consumer.TenantCode!;
                        sweepstakesResult.SweepstakesInstanceId = etlExecutionContext.SweepstakesInstanceId;
                        sweepstakesResult.TenantCode = tenantCode;
                        sweepstakesResult.ConsumerCode = consumer.ConsumerCode;
                        sweepstakesResult.PrizeIdentifier = prizeIdentifier.ToString();
                        sweepstakesResult.ResultTs = DateTime.UtcNow;
                        sweepstakesResult.CreateTs = DateTime.UtcNow;
                        sweepstakesResult.CreateUser = Constants.CreateUser;
                        sweepstakesResult.DeleteNbr = 0;
                        sweepstakesResult.PrizeDescribeJson = prizeDescribeJson;
                        await _sweepstakesResultRepo.CreateAsync(sweepstakesResult);
                        if(tenantAttributes == null || tenantOptions == null)
                        {
                            (tenantAttributes, tenantOptions) = await GetTenantAttrAndTenantOptions(tenantCode);
                        }
                        //If prize type is TASK_REWARD, Sweepstakes winnings automatically deposited to the purse 
                        if (string.Equals(winnerPrizeDetail?.PrizeType, Constants.SWEEPSTAKES_WINNER_TASK_PRIZE_TYPE,
                            StringComparison.OrdinalIgnoreCase))
                        {
                            if (tenantAttributes != null && tenantAttributes.EnableSweepstakesDirectDeposit && tenantAttributes.AutosweepSweepstakesReward)
                            {
                                var record = await BuildSweepstakesDepositRecord(consumer, sweepstakesResult, winnerPrizeDetail, tenantOptions!);
                                if (record != null)
                                    depositRecords.Add(record);
                            }
                            else
                            {
                                await AutoDepositSweepstakesReward(sweepstakesResult, winnerPrizeDetail?.WinnerCohort);
                            }

                        }
                    }
                    winnerNumber++;

                }
                #region Move file from inbound to archive folder after processing
                // generate process deposit instructions file with the records and submit the Deposit instructions Job
                if (depositRecords.Count > 0)
                {
                    await CreateTxtFileAndSubmitProcessDepositJob(etlExecutionContext, depositRecords, tenantCode);
                }

                var archiveFileFullPath = $"{s3FileDownloadRequestDto.InboundArchiveFolderName.TrimEnd('/')}/{Path.GetFileName(response.FileName)}";

                await _s3Helper.MoveFileToFolder(s3FileDownloadRequestDto.SourceBucketName, response.FileName,
                    s3FileDownloadRequestDto.ArchiveBucketName, archiveFileFullPath);

                #endregion

                //update sweepstakes instance status to completed
                _logger.LogInformation("{ClassName}.{MethodName} - Updating sweepstakes instance status to {Status}",
                    className, methodName, Constants.SWEEPSTAKES_WINNERS_REPORT_SUCCESS_STATUS);
                sweepstakesInstance.Status = Constants.SWEEPSTAKES_WINNERS_REPORT_SUCCESS_STATUS;
                await _sweepstakesInstanceRepo.UpdateAsync(sweepstakesInstance);
            }
            catch (Exception ex)
            {
                // Log any exception that occurs during the process and rethrow the exception
                _logger.LogError(ex, "{ClassName}.{MethodName} - Failed processing Sweepstakes winners report, ErrorCode:{Code}, ERROR: {Msg}",
                    className, methodName, StatusCodes.Status500InternalServerError, ex.Message);
                if (sweepstakesInstance != null && sweepstakesInstance.SweepstakesInstanceId != 0)
                {
                    _logger.LogInformation("{ClassName}.{MethodName} - Updating sweepstakes instance status to {Status}",
                        className, methodName, Constants.SWEEPSTAKES_WINNERS_REPORT_ERROR_STATUS);
                    //update sweepstakes instance status to error
                    sweepstakesInstance.Status = Constants.SWEEPSTAKES_WINNERS_REPORT_ERROR_STATUS;
                    await _sweepstakesInstanceRepo.UpdateAsync(sweepstakesInstance);
                }
                throw;
            }
        }

        private async Task<(TenantAttribute? tenantAttributes, TenantOption? tenantOptions)> GetTenantAttrAndTenantOptions(string tenantCode)
        {
            const string methodName = nameof(GetTenantAttrAndTenantOptions);
            // Retrieve tenant info(Deserialize tenant_attr and tenant_options_json)
            var tenant = await _tenantRepo.FindOneAsync(x => x.TenantCode == tenantCode && x.DeleteNbr == 0);
            if (tenant == null)
            {
                _logger.LogError("{className}.{methodName}: Invalid tenant code: {TenantCode}, Error Code:{errorCode}",
                    className, methodName, tenantCode, StatusCodes.Status400BadRequest);
                throw new ETLException(ETLExceptionCodes.NotFoundInDb, $"Invalid tenant code: {tenantCode}");
            }
            var tenantAttributes = !string.IsNullOrEmpty(tenant.TenantAttribute)
                    ? JsonConvert.DeserializeObject<TenantAttribute>(tenant.TenantAttribute)
                    : new TenantAttribute();
            var tenantOptions = !string.IsNullOrEmpty(tenant.TenantOption)
                    ? JsonConvert.DeserializeObject<TenantOption>(tenant.TenantOption)
                    : new TenantOption();
            return (tenantAttributes, tenantOptions);
        }

        /// <summary>
        /// Create tab delemited txt file and upload to s3 then kick off Process Deposit instruction Job
        /// </summary>
        /// <param name="etlExecutionContext"></param>
        /// <param name="depositRecords"></param>
        /// <param name="tenantCode"></param>
        /// <returns></returns>
        private async Task CreateTxtFileAndSubmitProcessDepositJob(EtlExecutionContext etlExecutionContext, List<ConsumerDepositInstructionRecord> depositRecords, string tenantCode)
        {
            bool uploadSuccess = false;
            string fileName = string.Empty;
            const string methodName = nameof(CreateTxtFileAndSubmitProcessDepositJob);

            if (depositRecords.Any())
            {
                // pass tenant code 
                (uploadSuccess, fileName) = await CreateTxtFileAndUploadToS3(etlExecutionContext, tenantCode, depositRecords, uploadSuccess);
            }
            // submit Deposit instrcutions
            if (uploadSuccess)
            {
                try
                {
                    var batchParams = await DepositInstrcutionBatchJobParameters(etlExecutionContext, fileName, tenantCode);
                    _logger.LogInformation($"{className}.{methodName}: Deposit job submission is enabled. Proceeding to submit the job.");

                    string jobName = $"{BenefitsConstants.ProcessDepositInstructionsFile}-{tenantCode}-{DateTime.UtcNow.ToString(Constants.DateFormat)}";

                    // Trigger AWS Batch job(Create for Deposit instruction job parameters
                    var jobId = await _awsBatchService.TriggerProcessDepositInstrcutionsBatchJob(jobName, batchParams);

                    _logger.LogInformation("{className}.{methodName}: Successfully submitted Process Deposit Instrcutions job. AWS Batch JobId: {jobId}", className, methodName, jobId);

                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "{className}.{methodName}: Failed to trigger AWS Batch job", className, methodName);
                    throw;
                }
            }
        }

        private async Task<ETLConsumerModel> getConsumerWithAnonymousCode(string userCode)
        {
            return await _consumerRepo.FindOneAsync(x => x.AnonymousCode == userCode && x.DeleteNbr == 0);
        }

        private async Task<bool> AutoDepositSweepstakesReward(ETLSweepstakesResultModel sweepstakesResult, string? cohortName)
        {
            const string methodName = nameof(AutoDepositSweepstakesReward);
            //Add/remove request for consumer to cohort
            var consumerCohortRequest = new CohortConsumerRequestDto
            {
                ConsumerCode = sweepstakesResult.ConsumerCode,
                CohortName = cohortName,
                TenantCode = sweepstakesResult.TenantCode
            };
            try
            {
                if (string.IsNullOrEmpty(cohortName))
                {
                    _logger.LogWarning("{className}.{methodName}: WinnerCohort is not configured to the sweepstakes instance : {sweepstakesInstanceId}," +
                       "hence skipping auto deposit of amout to purse for consumer :{consumerCode}, prizeDetails: {prizeDetails}, Error Code:{errorCode}",
                       className, methodName, sweepstakesResult.SweepstakesInstanceId, sweepstakesResult.ConsumerCode,
                       sweepstakesResult.PrizeDescribeJson, StatusCodes.Status400BadRequest);
                    return false;
                }
                // Retrieve tenant information
                var tenant = await _tenantRepo.FindOneAsync(x => x.TenantCode == sweepstakesResult.TenantCode && x.DeleteNbr == 0);
                if (tenant == null)
                {
                    _logger.LogError("{className}.{methodName}: Invalid tenant code: {TenantCode}, Error Code:{errorCode}",
                        className, methodName, sweepstakesResult.TenantCode, StatusCodes.Status400BadRequest);
                    throw new ETLException(ETLExceptionCodes.NotFoundInDb, $"Invalid tenant code: {sweepstakesResult.TenantCode}");
                }

                var tenantAttributes = !string.IsNullOrEmpty(tenant.TenantAttribute)
                        ? JsonConvert.DeserializeObject<TenantAttribute>(tenant.TenantAttribute)
                        : new TenantAttribute();
                if (tenantAttributes == null || !tenantAttributes.AutosweepSweepstakesReward)
                {
                    _logger.LogWarning("{className}.{methodName}: AutosweepSweepstakesReward is disabled to the tenant with tenant code: {TenantCode}," +
                        "hence skipping auto deposit of amout to purse for consumer :{consumerCode}, prizeDetails: {prizeDetails}, Error Code:{errorCode}",
                        className, methodName, sweepstakesResult.TenantCode, sweepstakesResult.ConsumerCode,
                        sweepstakesResult.PrizeDescribeJson, StatusCodes.Status400BadRequest);
                    return false;
                }

                //Add consumer to cohort before adding amount to purse
                await _cohortConsumerService.AddConsumerToCohort(consumerCohortRequest);

                //Get cohort consumer task
                var cohortConsumerTask = (await _cohortConsumerService.GetCohortConsumerTask(
                    sweepstakesResult.TenantCode, sweepstakesResult.ConsumerCode, cohortName))?.FirstOrDefault();

                if (cohortConsumerTask == null)
                {
                    _logger.LogError("{className}.{methodName}: Cohort is not mapped to the consumer. cohort: {CohortName} for consumer: {ConsumerCode}," +
                        " prizeDetails: {prizeDetails}, Error Code:{errorCode}",
                        className, methodName, cohortName, sweepstakesResult.ConsumerCode, sweepstakesResult.PrizeDescribeJson, StatusCodes.Status400BadRequest);
                    return false;
                }
                //Complete the task mapped to the cohort to credit the reward amount
                var taskUpdateRequestDto = new TaskUpdateRequestDto
                {
                    ConsumerCode = cohortConsumerTask.ConsumerCode,
                    TaskId = cohortConsumerTask.TaskId,
                    TaskCompletedTs = DateTime.UtcNow,
                    TaskStatus = Constants.TaskStatusCompleted,
                    IsAutoEnrollEnabled = true

                };
                //Update the task to complete
                await _taskUpdateService.UpdateTaskAsCompleted(taskUpdateRequestDto);

                //Update sweepstakes result with rewarded status after adding amount to purse
                sweepstakesResult.IsRewarded = true;
                await _sweepstakesResultRepo.UpdateAsync(sweepstakesResult);
                //Remove consumer from cohort after adding amount to purse
                await _cohortConsumerService.RemoveConsumerToCohort(consumerCohortRequest);

                _logger.LogInformation("{className}.{methodName}: Completed the task reward :{taskRewardCode} for consumer: {ConsumerCode} " +
                    "with cohort: {CohortName} and removed the consumer from cohort, Error Code:{errorCode}",
                    className, methodName, cohortConsumerTask.TaskRewardCode, sweepstakesResult.ConsumerCode, cohortName, StatusCodes.Status200OK);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName} - Failed to auto deposit sweepstakes reward for consumer: {ConsumerCode}, prizeDetails: {prizeDetails} Error: {Error}",
                    className, nameof(AutoDepositSweepstakesReward), sweepstakesResult.ConsumerCode, sweepstakesResult.PrizeDescribeJson, ex.Message);
                //Remove consumer from cohort upon error
                await _cohortConsumerService.RemoveConsumerToCohort(consumerCohortRequest);
                _logger.LogInformation("{className}.{methodName}: Removed consumer from cohort: {CohortName} for consumer: {ConsumerCode}, Error Code:{errorCode}",
                    className, methodName, cohortName, sweepstakesResult.ConsumerCode, StatusCodes.Status400BadRequest);
                return false;
            }
        }

        /// <summary>
        /// Create tab delimited file and upload to s3
        /// </summary>
        /// <param name="etlExecutionContext"></param>
        /// <param name="tenantCode"></param>
        /// <param name="records"></param>
        /// <param name="uploadSuccess"></param>
        /// <returns></returns>
        private async Task<(bool uploadSuccess, string fileName)> CreateTxtFileAndUploadToS3(EtlExecutionContext etlExecutionContext, string tenantCode, List<ConsumerDepositInstructionRecord> records, bool uploadSuccess)
        {
            string fileName;
            const string methodName = nameof(CreateTxtFileAndUploadToS3);
            _logger.LogInformation($"{className}.{methodName}: Found {records.Count} Card 60 records. Starting File generation.");

            // Configure Csv and generate tab-delimited text
            var csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                TrimOptions = TrimOptions.Trim,
                IgnoreBlankLines = true,
                Delimiter = "\t"
            };

            // Create a unique file name with Process Deposit Prefix and timestamp
            var timestamp = DateTime.UtcNow.ToString(Constants.DateFormat);
            fileName = $"{FilePrefixes.PROCESS_DEPOSIT_INSTRUCTIONS_FILE_PREFIX}_{tenantCode}_{etlExecutionContext.JobHistoryId}_{timestamp}.{Constants.txtExtention}";
            var pathToUpload = $"{Constants.INCOMING_FOLDER}/{fileName}";

            // Upload to S3
            uploadSuccess = await _awsS3Service.CreateCsvAndUploadToS3(csvConfig, records, pathToUpload, bucketName: null);

            if (uploadSuccess)
            {
                _logger.LogInformation($"{className}.{methodName}: Successfully generated and uploaded Process Deposit Instruction file to S3. File: {fileName}");
            }
            else
            {
                _logger.LogError($"{className}.{methodName}: Failed to upload Process Deposit Instruction file to S3. File: {fileName}");
            }

            return (uploadSuccess, fileName);
        }
        /// <summary>
        /// Build Deposit Instruction record for each Consumer
        /// </summary>
        /// <param name="consumer"></param>
        /// <param name="sweepstakesResult"></param>
        /// <param name="winnerPrizeDetail"></param>
        /// <param name="tenantOption"></param>
        /// <returns></returns>
        private async Task<ConsumerDepositInstructionRecord?> BuildSweepstakesDepositRecord(ETLConsumerModel consumer, ETLSweepstakesResultModel sweepstakesResult, PrizeDetail? winnerPrizeDetail, TenantOption tenantOption)
        {
            const string methodName = nameof(BuildSweepstakesDepositRecord);

            try
            {
                if (winnerPrizeDetail == null)
                {
                    _logger.LogError("{Class}.{Method}: Winner prize detail is null for Consumer: {ConsumerCode}", className, methodName, consumer.ConsumerCode);
                    return null;
                }

                // Load tenantOption.SweepstakesWinnerRewardWalletTypeCode wallet type code and default Reward_Wallet_Type wallet type
                string tenantWalletTypeCode = tenantOption.SweepstakesWinnerRewardWalletTypeCode ?? string.Empty;
                string defaultWalletTypeCode = _configuration.GetValue<string>("Reward_Wallet_Type_Code") ?? string.Empty;

                if (string.IsNullOrWhiteSpace(tenantWalletTypeCode))
                    tenantWalletTypeCode = defaultWalletTypeCode;

                if (string.IsNullOrWhiteSpace(tenantWalletTypeCode))
                {
                    _logger.LogError("{Class}.{Method}: Reward wallet type code is not configured.", className, methodName);
                    return null;
                }

                // Fetch tenant walletTypeConfig and defaultWalletTypeConfig ONCE
                var tenantOptionWalletTypeConfig = await _walletTypeRepo.FindOneAsync(x => x.WalletTypeCode == tenantWalletTypeCode);
                var defaultWalletTypeConfig = await _walletTypeRepo.FindOneAsync(x => x.WalletTypeCode == defaultWalletTypeCode);

                if (tenantOptionWalletTypeConfig == null)
                {
                    _logger.LogWarning("{Class}.{Method}: Tenant walletType {WalletType} not found. Falling back to default.", className, methodName, tenantWalletTypeCode);

                    if (defaultWalletTypeConfig == null)
                    {
                        _logger.LogError("{Class}.{Method}: Default wallet type also missing. Cannot continue.", className, methodName);
                        return null;
                    }

                    tenantOptionWalletTypeConfig = defaultWalletTypeConfig;
                }

                // This is the effective resolved wallet type we will use everywhere
                var effectiveWalletTypeConfig = tenantOptionWalletTypeConfig;

                // Fetch consumer wallet (using effective wallet type)
                var consumerWallet = await _walletRepo.GetWalletByConsumerAndWalletType(consumer.TenantCode, consumer.ConsumerCode, effectiveWalletTypeConfig.WalletTypeId);

                if (consumerWallet == null)
                {
                    _logger.LogWarning("{Class}.{Method}: Consumer wallet not found for {WalletType}. Retrying with default...", className, methodName, tenantWalletTypeCode);

                    if (defaultWalletTypeConfig == null)
                    {
                        _logger.LogError("{Class}.{Method}: Default walletType configuration missing when retrying for consumer wallet.", className, methodName);
                        return null;
                    }

                    consumerWallet = await _walletRepo.GetWalletByConsumerAndWalletType(consumer.TenantCode, consumer.ConsumerCode, defaultWalletTypeConfig.WalletTypeId);

                    if (consumerWallet == null)
                    {
                        _logger.LogError("{Class}.{Method}: Consumer wallet not found even with default wallet type.",
                            className, methodName);
                        return null;
                    }

                    effectiveWalletTypeConfig = defaultWalletTypeConfig;
                }

                // Fetch master wallet (using effective wallet type)
                var (masterWalletModel, masterWalletTypeModel) = await _walletRepo.GetMasterWalletTypeByTenantAndWalletType(consumer.TenantCode, effectiveWalletTypeConfig.WalletTypeId);

                if (masterWalletModel == null || masterWalletTypeModel == null)
                {
                    _logger.LogWarning("{Class}.{Method}: Master wallet not found for {WalletType}. Retrying with default...", className, methodName, effectiveWalletTypeConfig.WalletTypeCode);

                    if (defaultWalletTypeConfig == null)
                    {
                        _logger.LogError("{Class}.{Method}: Default wallet type missing when checking master wallet.", className, methodName);
                        return null;
                    }

                    (masterWalletModel, masterWalletTypeModel) = await _walletRepo.GetMasterWalletTypeByTenantAndWalletType(consumer.TenantCode, defaultWalletTypeConfig.WalletTypeId);

                    if (masterWalletModel == null || masterWalletTypeModel == null)
                    {
                        _logger.LogError("{Class}.{Method}: Master wallet not found even with default wallet type.", className, methodName);
                        return null;
                    }

                    effectiveWalletTypeConfig = defaultWalletTypeConfig;
                }

                // Person lookup
                var person = await _personRepo.FindOneAsync(x => x.PersonId == consumer.PersonId);
                if (person == null)
                {
                    _logger.LogError("{Class}.{Method}: Person record not found for PersonId {PersonId}", className, methodName, consumer.PersonId);
                    return null;
                }

                // Build Deposit Record
                return new ConsumerDepositInstructionRecord
                {
                    ClientTaskCode = string.Empty,
                    RewardAmount = winnerPrizeDetail.Amount,
                    RewardType = "MONETARY_DOLLARS",
                    PersonUniqueIdentifier = person.PersonUniqueIdentifier,
                    MasterWalletTypeCode = masterWalletTypeModel.WalletTypeCode,
                    ConsumerWalletTypeCode = effectiveWalletTypeConfig.WalletTypeCode,
                    CustomTransactionDescription = winnerPrizeDetail.Description,
                    RedemptionRef = sweepstakesResult.PrizeIdentifier
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "{Class}.{Method}: Error building sweepstakes deposit record for Consumer {ConsumerCode}. Error: {Message}",
                    className, methodName, consumer.ConsumerCode, ex.Message);

                throw;
            }
        }

        private async Task<Dictionary<string, string>> DepositInstrcutionBatchJobParameters(EtlExecutionContext etlExecutionContext, string fileName, string tenantCode)
        {
            var jobHistoryRequest = await _jobHistoryService.GetJobHistoryCreateRequest(etlExecutionContext);
            var jobHistory = await _jobHistoryService.InsertJobHistory(jobHistoryRequest);

            // Prepare AWS Batch job parameters for Deposit instructions
            var batchParams = new Dictionary<string, string>
            {
                { "tenantCode", tenantCode },
                { "jobHistoryId", jobHistory.JobHistoryId ?? "DEFAULT" },
                { "enableS3", "true"},
                { "scanS3FileTypes", BenefitsConstants.ProcessDepositInstructionsFile },
                { "isSubmitCard60Job", "true" }
            };
            return batchParams;
        }

    }
}
