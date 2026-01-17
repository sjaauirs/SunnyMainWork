using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SunnyRewards.Helios.ETL.Common.Constants;
using SunnyRewards.Helios.ETL.Common.CustomException;
using SunnyRewards.Helios.ETL.Common.Extensions;
using SunnyRewards.Helios.ETL.Core.Domain.Constants;
using SunnyRewards.Helios.ETL.Core.Domain.Dtos;
using SunnyRewards.Helios.ETL.Core.Domain.Dtos.Enums;
using SunnyRewards.Helios.ETL.Core.Domain.Dtos.FIS;
using SunnyRewards.Helios.ETL.Core.Domain.Dtos.Json;
using SunnyRewards.Helios.ETL.Infrastructure.Repositories.HeliosRepo;
using SunnyRewards.Helios.ETL.Infrastructure.Repositories.HeliosRepo.Interfaces;
using SunnyRewards.Helios.ETL.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.ETL.Infrastructure.Services.Interfaces.DynamoDb;
using SunnyRewards.Helios.ETL.Infrastructure.Services.Interfaces.FIS;
using System.Globalization;

namespace SunnyRewards.Helios.ETL.Infrastructure.Services
{
    public class DepositInstructionService : IDepositInstructionService
    {
        private readonly ILogger<DepositInstructionService> _logger;
        private readonly ITenantRepo _tenantRepo;
        private readonly ITenantAccountRepo _tenantAcountRepo;
        private readonly IWalletRepo _walletRepo;
        private readonly IWalletTypeRepo _walletTypeRepo;
        private readonly IFundTransferService _fundTransferService;
        private readonly IAwsS3Service _awsS3Service;
        private readonly IAwsBatchService _awsBatchService;
        private readonly IJobHistoryService _jobHistoryService;
        private readonly IConsumerRepo _consumerRepo;
        private readonly IPersonRepo _personRepo;
        const string className = nameof(DepositInstructionService);

        public DepositInstructionService(ILogger<DepositInstructionService> logger,
            ITenantRepo tenantRepo, ITenantAccountRepo tenantAcountRepo, IWalletRepo walletRepo,
            IWalletTypeRepo walletTypeRepo, IFundTransferService fundTransferService,
            IAwsS3Service awsS3Service, IAwsBatchService awsBatchService, IJobHistoryService jobHistoryService, IConsumerRepo consumerRepo, IPersonRepo personRepo)
        {
            _logger = logger;
            _tenantRepo = tenantRepo;
            _tenantAcountRepo = tenantAcountRepo;
            _walletRepo = walletRepo;
            _walletTypeRepo = walletTypeRepo;
            _fundTransferService = fundTransferService;
            _awsS3Service = awsS3Service;
            _awsBatchService = awsBatchService;
            _jobHistoryService = jobHistoryService;
            _consumerRepo = consumerRepo;
            _personRepo = personRepo;
        }

        /// <summary>
        /// Process Deposit Instruction File and perform
        /// </summary>
        /// <param name="etlExecutionContext"></param>
        /// <returns></returns>
        public async Task ProcessDepositInstructionFile(EtlExecutionContext etlExecutionContext)
        {
            const string methodName = nameof(ProcessDepositInstructionFile);
            var tenantCode = etlExecutionContext.TenantCode;

            // List to hold Card 60 file records
            var card60Rows = new List<ETLCard60ConsumerInputDto>();
            bool uploadSuccess = false;
            var fileName = string.Empty;

            if (string.IsNullOrEmpty(tenantCode))
            {
                _logger.LogWarning("{className}.{methodName}: No tenant code provided for processing Process Deposit Instruction File. ErrorCode:{error}", className, methodName, StatusCodes.Status400BadRequest);
                throw new ETLException(ETLExceptionCodes.NullValue, "No tenant code provided for Process Deposit Instruction File");
            }

            try
            {
                _logger.LogInformation("{className}.{methodName}: Starting to process Process Deposit Instruction File for TenantCode: {TenantCode}", className, methodName, tenantCode);

                // Retrieve tenant information
                var tenant = await _tenantRepo.FindOneAsync(x => x.TenantCode == tenantCode && x.DeleteNbr == 0);
                if (tenant == null)
                {
                    _logger.LogWarning("{className}.{methodName}: Invalid tenant code: {TenantCode}, Error Code:{errorCode}", className, methodName, tenantCode, StatusCodes.Status404NotFound);
                    throw new ETLException(ETLExceptionCodes.NotFoundInDb, $"Invalid tenant code : {tenantCode}");
                }
               
                // Retrieve tenant account information and funding configuration
                var tenantAccount = await _tenantAcountRepo.FindOneAsync(x => x.TenantCode == tenantCode && x.DeleteNbr == 0);
                if (tenantAccount == null || tenantAccount.FundingConfigJson == null)
                {
                    _logger.LogWarning("{className}.{methodName}: Funding configuration not available for Tenant: {TenantCode}, Error Code:{errorCode}", className, methodName, tenantCode, StatusCodes.Status404NotFound);
                    throw new ETLException(ETLExceptionCodes.NotFoundInDb, $" Funding configuration not available in tenant account for Tenant code : {tenantCode}");
                }

                // Parse deposit instruction file
                List<ConsumerDepositInstructionRecord> consumerDepositInstructionRecords = await DepositInstructionFileParsing(etlExecutionContext);

                if (consumerDepositInstructionRecords == null || consumerDepositInstructionRecords.Count == 0)
                {
                    _logger.LogWarning("{className}.{methodName}: No consumers found in the consumer list file for Tenant: {TenantCode}", className, methodName, tenantCode);
                    return;
                }
                // Resolve all ConsumerCodes upfront
                var personToConsumerMap = await GetConsumerCodes(consumerDepositInstructionRecords, tenantCode);

                // Get distinct master wallet types from the consumer list
                var distinctMasterWalletTypes = consumerDepositInstructionRecords
                                                .Select(x => x.MasterWalletTypeCode)
                                                .Where(x => !string.IsNullOrWhiteSpace(x))
                                                .Distinct()
                                                .ToList();

                // Fetch all required wallet types in a single call
                var masterWalletTypeList = await _walletTypeRepo.FindAsync(x => distinctMasterWalletTypes.Contains(x.WalletTypeCode) && x.DeleteNbr == 0);

                foreach (var consumerRecord in consumerDepositInstructionRecords)
                {
                    try
                    {
                        _logger.LogInformation("{className}.{methodName}: ConsumerCode from the list: {ConsumerCode}", className, methodName, consumerRecord.PersonUniqueIdentifier);

                        // validate amount
                        if (consumerRecord.RewardAmount <= 0)
                        {
                            _logger.LogWarning("{className}.{methodName}: Skipping invalid record: {Record}", className, methodName, consumerRecord.ToJson());
                            continue;
                        }
                        if (!personToConsumerMap.TryGetValue(consumerRecord.PersonUniqueIdentifier!, out var consumerCode))
                        {
                            _logger.LogWarning("{className}.{methodName}: No consumer found for PersonUniqueIdentifier:{pid}",
                                className, methodName, consumerRecord.PersonUniqueIdentifier);
                            continue;
                        }

                        // master wallet type from in-memory dictionary Retrieve master wallet type and validating isExternalSync
                        var masterWalletType = masterWalletTypeList.FirstOrDefault(x => x.WalletTypeCode == consumerRecord.MasterWalletTypeCode && x.DeleteNbr == 0 && !x.IsExternalSync);
                        if (masterWalletType == null)
                        {
                            _logger.LogError("{className}.{methodName}: Master wallet type not found. For master Wallet Type Code:{type}", className, methodName, consumerRecord.MasterWalletTypeCode);
                            continue;
                        }
                        // Retrieve master wallet
                        var masterWallet = await _walletRepo.FindOneAsync(w => w.TenantCode == tenantCode && w.WalletTypeId == masterWalletType.WalletTypeId && w.MasterWallet == true);
                        if (masterWallet == null)
                        {
                            _logger.LogError($"{className}.{methodName}: Master wallet not found for tenant, TenantCode: {tenantCode} and ConsumerCode:{consumerCode}");
                            continue;
                        }

                        // Retrieve consumer wallet type
                        var consumerWalletType = await _walletTypeRepo.FindOneAsync(x => x.WalletTypeCode == consumerRecord.ConsumerWalletTypeCode && x.DeleteNbr == 0);
                        if (consumerWalletType == null)
                        {
                            _logger.LogWarning($"{className}.{methodName}: Consumer wallet type not found. TenantCode:{tenantCode} consumerWalletTypeCode:{consumerRecord.ConsumerWalletTypeCode}");
                            continue;
                        }
                        // Retrieve consumer wallets and wallets
                        var consumerWalletsAndWalletModels = _walletRepo.GetAllConsumerWalletsAndWallets(tenantCode, consumerWalletType.WalletTypeId, new List<string> { consumerCode! });

                        // pick consumer's wallet model by consumer code
                        var consumerWalletModelEntry = consumerWalletsAndWalletModels
                            .FirstOrDefault(e => e.ConsumerWalletModel != null && e.ConsumerWalletModel.ConsumerCode == consumerCode);

                        if (consumerWalletModelEntry == null)
                        {
                            _logger.LogWarning("{className}.{methodName}: No consumer wallet found for ConsumerCode:{consumer} and ConsumerWalletTypeCode:{type}", className, methodName, consumerCode, consumerRecord.ConsumerWalletTypeCode);
                            continue;
                        }

                        var consumerWallet = consumerWalletModelEntry.ConsumerWalletModel;
                        var wallet = consumerWalletModelEntry.WalletModel;

                        // Create custom transaction description: consumer_code:job_history_id:client_task_code
                        var customTxnDescription = $"{consumerCode}:{etlExecutionContext.JobHistoryId}:{consumerRecord.ClientTaskCode ?? string.Empty}";
                        // Build FISFundTransferRequestDto
                        var fundTransferRequest = new FISFundTransferRequestDto
                        {
                            TenantCode = tenantCode,
                            ConsumerCode = consumerCode,
                            Amount = consumerRecord.RewardAmount,
                            TransactionDetailType = BenefitsConstants.BenefitTransactionDetailType,
                            RuleNumber = 0, // Assuming 0 for deposit instructions
                            MasterWallet = masterWallet,
                            ConsumerWallet = wallet,
                            RedemptionRef = consumerRecord.RedemptionRef ?? customTxnDescription
                        };


                        fundTransferRequest.RewardDescription = consumerRecord.CustomTransactionDescription;

                        await _fundTransferService.ExecuteFundTransferAsync(fundTransferRequest, isFundingRuleExecution: false);

                        if (string.Equals(consumerRecord.RewardType, BenefitsConstants.MonetaryDollarRewardType,
                            StringComparison.OrdinalIgnoreCase))
                        {
                            card60Rows.Add(new ETLCard60ConsumerInputDto
                            {
                                ConsumerCode = consumerCode,
                                RedemptionAmount = consumerRecord.RewardAmount,
                                ConsumerWalletTypeCode = consumerRecord.ConsumerWalletTypeCode,
                                CustomTransactionDescription = consumerRecord.CustomTransactionDescription
                            });
                        }

                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "{className}.{methodName}: Error processing Record: {record}. Skipping record. Error: {msg}", className, methodName, consumerRecord.ToJson(), ex.Message);
                        continue;
                    }
                }

                // Generate Card 60 file if there are any rows
                if (card60Rows.Any())
                {
                    (uploadSuccess, fileName) = await CreateTxtFileAndUploadToS3(etlExecutionContext, tenantCode, card60Rows, uploadSuccess);
                }
                else
                {
                    _logger.LogInformation($"{className}.{methodName}: No Card 60 records found. Skipping CSV generation.");
                }

                // If Card 60 job submission is required and upload was successful, submit the job
                if (etlExecutionContext.IsSubmitCard60Job && uploadSuccess)
                {
                    try
                    {
                        var batchParams = await BatchJobParameters(etlExecutionContext, fileName);
                        _logger.LogInformation($"{className}.{methodName}: Card 60 job submission is enabled. Proceeding to submit the job.");

                        string jobName = $"{BenefitsConstants.Card60JobNamePrefix}-{etlExecutionContext.TenantCode}-{DateTime.UtcNow.ToString(Constants.DateFormat)}";

                        // Trigger AWS Batch job
                        var jobId = await _awsBatchService.TriggerBatchJob(jobName, batchParams);

                        _logger.LogInformation("{className}.{methodName}: Successfully submitted Card 60 job. AWS Batch JobId: {jobId}", className, methodName, jobId);

                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "{className}.{methodName}: Failed to trigger AWS Batch job. TenantCode: {TenantCode}", className, methodName, tenantCode);
                        throw;
                    }


                }

                _logger.LogInformation("{className}.{methodName}: Processing Deposit Instruction File completed successfully for TenantCode: {TenantCode}", className, methodName, tenantCode);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{className}.{methodName}: Error occurred while Processing Deposit Instruction Files, Error: {Message}", className, methodName, ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Generate Card 60 .txt file and upload to S3
        /// </summary>
        /// <param name="etlExecutionContext"></param>
        /// <param name="tenantCode"></param>
        /// <param name="card60Rows">card60File Rows</param>
        /// <param name="uploadSuccess"></param>
        /// <returns></returns>
        private async Task<(bool uploadSuccess, string fileName)> CreateTxtFileAndUploadToS3(EtlExecutionContext etlExecutionContext, string tenantCode, List<ETLCard60ConsumerInputDto> card60Rows, bool uploadSuccess)
        {
            string fileName;
            const string methodName = nameof(CreateTxtFileAndUploadToS3);
            _logger.LogInformation($"{className}.{methodName}: Found {card60Rows.Count} Card 60 records. Starting File generation.");

            // Configure Csv and generate tab-delimited text
            var csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                TrimOptions = TrimOptions.Trim,
                IgnoreBlankLines = true,
                Delimiter = "\t"
            };

            // Create a unique file name with timestamp
            var timestamp = DateTime.UtcNow.ToString(Constants.DateFormat);
            fileName = $"{BenefitsConstants.Card60FileNamePrefix}_{tenantCode}_{etlExecutionContext.JobHistoryId}_{timestamp}.{Constants.txtExtention}";
            var pathToUpload = $"{Constants.INCOMING_FOLDER}/{fileName}";

            // Upload to S3
            uploadSuccess = await _awsS3Service.CreateCsvAndUploadToS3(csvConfig, card60Rows, pathToUpload, bucketName: null);

            if (uploadSuccess)
            {
                _logger.LogInformation($"{className}.{methodName}: Successfully generated and uploaded Card 60 CSV file to S3. File: {fileName}");
            }
            else
            {
                _logger.LogError($"{className}.{methodName}: Failed to upload Card 60 CSV file to S3. File: {fileName}");
            }

            return (uploadSuccess, fileName);
        }

        /// <summary>
        /// Prepare AWS Batch job parameters which are required for Card 60 job
        /// </summary>
        /// <param name="etlExecutionContext"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        private async Task<Dictionary<string, string>> BatchJobParameters(EtlExecutionContext etlExecutionContext, string fileName)
        {
            var jobHistoryRequest = await _jobHistoryService.GetJobHistoryCreateRequest(etlExecutionContext);
            var jobHistory = await _jobHistoryService.InsertJobHistory(jobHistoryRequest);

            // Prepare AWS Batch job parameters
            var batchParams = new Dictionary<string, string>
            {
                { "tenantCode", etlExecutionContext.TenantCode },
                { "batchOperationGroupCode", "bgc-" + Guid.NewGuid().ToString("N") },
                { "batchActionType", BatchActions.ALL.ToString() },
                { "batchActionOptions", BatchActions.ALL.ToString() },
                { "jobHistoryId", jobHistory.JobHistoryId ?? "DEFAULT" },
                { "consumerListFile", fileName},
                { "generateCardLoad", "true" }
            };
            return batchParams;
        }
        private async Task<Dictionary<string, string>> GetConsumerCodes(List<ConsumerDepositInstructionRecord> records, string tenantCode)
        {
            // Get all unique person_unique_identifiers from file
            var uniquePersonIds = records
                .Select(r => r.PersonUniqueIdentifier)
                .Where(pid => !string.IsNullOrWhiteSpace(pid))
                .Distinct()
                .ToList();

            // Fetch all persons
            var persons = await _personRepo.FindAsync(p =>
                uniquePersonIds.Contains(p.PersonUniqueIdentifier) && p.DeleteNbr == 0);

            var personIds = persons.Select(p => p.PersonId).ToList();

            // Fetch all consumers for this tenant
            var consumers = await _consumerRepo.FindAsync(c =>
                personIds.Contains(c.PersonId) &&
                c.TenantCode == tenantCode &&
                c.DeleteNbr == 0);

            // Build mapping: person_unique_identifier → consumer_code
            var mapping = (from p in persons
                           join c in consumers on p.PersonId equals c.PersonId
                           select new { p.PersonUniqueIdentifier, c.ConsumerCode })
                          .ToDictionary(x => x.PersonUniqueIdentifier!, x => x.ConsumerCode!);

            return mapping;
        }


        /// <summary>
        /// Parse deposit instruction file and return list of ConsumerDepositInstructionRecord
        /// </summary>
        /// <param name="etlExecutionContext"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private async Task<List<ConsumerDepositInstructionRecord>> DepositInstructionFileParsing(EtlExecutionContext etlExecutionContext)
        {
            string depositInstructionFilePath = etlExecutionContext.DepositInstructionFilePath;
            var depositInstructionFileContents = etlExecutionContext.DepositIntructionEligibleConsumersFileContents;

            try
            {
                using var reader = depositInstructionFileContents?.Length > 0
                ? new StreamReader(new MemoryStream(depositInstructionFileContents))
                : new StreamReader(depositInstructionFilePath);

                var csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    Delimiter = "\t",
                    HasHeaderRecord = true,
                    MissingFieldFound = null,
                    HeaderValidated = null,
                    TrimOptions = TrimOptions.Trim
                };

                using var csvReader = new CsvReader(reader, csvConfig);

                var records = new List<ConsumerDepositInstructionRecord>();

                await foreach (var record in csvReader.GetRecordsAsync<ConsumerDepositInstructionRecord>())
                {
                    if (string.IsNullOrWhiteSpace(record.PersonUniqueIdentifier) || record.RewardAmount <= 0 ||
                        string.IsNullOrWhiteSpace(record.MasterWalletTypeCode) || string.IsNullOrWhiteSpace(record.ConsumerWalletTypeCode))
                    {
                        _logger.LogWarning("Skipping invalid record: {Record}", record.ToJson());
                        continue; // skip invalid row
                    }
                    records.Add(record);
                }

                return records;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to read file {Path}", depositInstructionFilePath);
                throw new Exception($"Failed to read file: {ex.Message}", ex);
            }

        }

    }
}
