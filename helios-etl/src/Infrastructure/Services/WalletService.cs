using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SunnyRewards.Helios.ETL.Common.Constants;
using SunnyRewards.Helios.ETL.Common.Domain.Dtos;
using SunnyRewards.Helios.Common.Core.Helpers.Interfaces;
using SunnyRewards.Helios.ETL.Core.Domain.Constants;
using SunnyRewards.Helios.ETL.Core.Domain.Dtos;
using SunnyRewards.Helios.ETL.Core.Domain.Models;
using SunnyRewards.Helios.ETL.Infrastructure.AwsConfig;
using SunnyRewards.Helios.ETL.Infrastructure.Helpers.Interfaces;
using SunnyRewards.Helios.ETL.Infrastructure.HttpClients.Interfaces;
using SunnyRewards.Helios.ETL.Infrastructure.Repositories.HeliosRepo.Interfaces;
using SunnyRewards.Helios.ETL.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.Wallet.Core.Domain.Dtos;
using System.Globalization;
using SunnyRewards.Helios.ETL.Common.Helpers.Interfaces;
using Microsoft.AspNetCore.Http;
using SunnyRewards.Helios.User.Core.Domain.Models;
using SunnyRewards.Helios.ETL.Common.CustomException;
using SunnyRewards.Helios.ETL.Core.Domain.Dtos.Json;

namespace SunnyRewards.Helios.ETL.Infrastructure.Services
{
    public class WalletService : AwsConfiguration, IWalletService
    {
        private readonly ILogger<WalletService> _logger;
        private readonly IAdminClient _adminClient;
        private readonly ITenantRepo _tenantRepo;
        private readonly IConsumerRepo _consumersRepo;
        private readonly IWalletTypeRepo _walletTypeRepo;
        private readonly IWalletRepo _walletRepo;
        private readonly IConfiguration _configuration;
        private readonly IPersonRepo _personRepo;
        private readonly IConsumerWalletRepo _consumerWalletRepo;
        private readonly ICsvWrapper _csvHelper;
        private readonly IJsonConvertWrapper _jsonWrapper;
        private readonly IS3Helper _s3Helper;
        private readonly IAwsS3Service _awsS3Service;
        private const string className = nameof(WalletService);
        private List<HsaResponseDto> _hsaCsvFileList;

        public WalletService(ILogger<WalletService> logger, IAdminClient adminClient, ITenantRepo tenantRepo,
            IConsumerRepo consumersRepo, IWalletTypeRepo walletTypeRepo, IWalletRepo walletRepo, IConfiguration configuration, IVault vault, IPersonRepo personRepo, IConsumerWalletRepo consumerWalletRepo, ICsvWrapper csvHelper, IJsonConvertWrapper jsonWrapper, IS3Helper s3Helper, IAwsS3Service awsS3Service) : base(vault, configuration)
        {
            _logger = logger;
            _adminClient = adminClient;
            _tenantRepo = tenantRepo;
            _consumersRepo = consumersRepo;
            _walletTypeRepo = walletTypeRepo;
            _walletRepo = walletRepo;
            _configuration = configuration;
            _personRepo = personRepo;
            _consumerWalletRepo = consumerWalletRepo;
            _csvHelper = csvHelper;
            _jsonWrapper = jsonWrapper;
            _s3Helper = s3Helper;
            _awsS3Service = awsS3Service;
        }
        public async Task ClearEntriesWallet(string? tenantCode)
        {
            const string methodName=nameof(ClearEntriesWallet);
            if (string.IsNullOrEmpty(tenantCode))
            {
                _logger.LogError("{ClassName}.{MethodName} - No tenant code provided for clearing entries type wallet balance.", className, methodName);
                throw new ETLException(ETLExceptionCodes.NullValue, "No tenant code provided for clearing entries type wallet balance");
            }

            try
            {
                _logger.LogInformation("{ClassName}.{MethodName} - Started clearing entries type wallet balance for tenant with code: {TenantCode}", className, methodName, tenantCode);

                var tenant = await _tenantRepo.FindOneAsync(x => x.TenantCode == tenantCode && x.DeleteNbr == 0);
                if (tenant == null)
                {
                    _logger.LogWarning("{ClassName}.{MethodName} - Invalid tenant code: {TenantCode} supplied for entries type wallet balance clearance.", className, methodName, tenantCode);
                    throw new ETLException(ETLExceptionCodes.NotFoundInDb, $"Invalid tenant code: {tenantCode} supplied for entries type wallet balance clearance.");
                }

                var clearEntriesWalletRequestDto = new Core.Domain.Dtos.ClearEntriesWalletRequestDto { TenantCode = tenantCode };
                var response = await _adminClient.Post<BaseResponseDto>("admin/wallet/clear-entries-wallet", clearEntriesWalletRequestDto);

                if (response.ErrorCode != null && response.ErrorMessage != null)
                {
                    _logger.LogError("{ClassName}.{MethodName} - Error occurred while clearing entries type wallet balance for tenant with code: {TenantCode}.ErrorCode:{Code}, ERROR: {ErrorMessage}", className, methodName, tenantCode,response.ErrorCode,response.ErrorMessage);
                    throw new ETLException(ETLExceptionCodes.ErrorFromAPI, $"Error occurred while clearing entries type wallet balance for tenant with code: {tenantCode}.ErrorCode:{response.ErrorCode}, ERROR: {response.ErrorMessage}");
                }
                else
                {
                    _logger.LogInformation("{ClassName}.{MethodName} - Successfully cleared entries type wallet balance for tenant with code: {TenantCode}", className, methodName, tenantCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName} - An error occurred while clearing entries type wallet balance for tenant with code: {TenantCode}.ErrorCode:{Code}, ERROR: {ErrorMessage}", className, methodName, tenantCode,StatusCodes.Status500InternalServerError, ex.Message);
                throw;
            }
        }

        public async Task RedeemHSA(EtlExecutionContext etlExecutionContext)
        {
            const string methodName=nameof(RedeemHSA);
            _logger.LogInformation("{ClassName}.{MethodName} - Started redeeming consumer wallet funds into HSA redemption wallet for Tenant Code: {TenantCode}", className, methodName, etlExecutionContext.TenantCode);

            if (!ValidateInputParameters(etlExecutionContext))
            {
                return;
            }

            var tenant = await GetTenant(etlExecutionContext.TenantCode);
            if (tenant == null)
            {
                throw new ETLException(ETLExceptionCodes.NotFoundInDb, $"Invalid tenant code supplied for HSA redemption, Tenant code:{etlExecutionContext.TenantCode}");
            }

            _hsaCsvFileList = new();
            var errorMessageList = new List<string>();
            int totalRecordsCount = 0;

            // If LocalDownloadFolderPath is null and RedeemConsumerListFilePath is not null then create the file in S3 bucket
            if (string.IsNullOrEmpty(etlExecutionContext.LocalDownloadFolderPath) && !string.IsNullOrEmpty(etlExecutionContext.RedeemConsumerListFilePath))
            {
                var reoprtS3BucketName = GetAwsReoprtS3BucketName();
                byte[] fileContent = await _awsS3Service.GetFileFromAwsS3($"{Constants.HSA_INBOUND_FOLDER}/{etlExecutionContext.RedeemConsumerListFilePath}", GetAwsTmpS3BucketName());
                var streamReader = new StreamReader(new MemoryStream(fileContent));
                var csvConfiguration = new CsvConfiguration(CultureInfo.InvariantCulture) { Delimiter = "\t" };
                using var redeemConsumerListCSV = new CsvReader(streamReader, csvConfiguration);

                while (await redeemConsumerListCSV.ReadAsync())
                {
                    var redeemConsumerCsvDto = redeemConsumerListCSV.GetRecord<RedeemConsumerCSVDto>();
                    if (redeemConsumerCsvDto != null)
                    {
                        var errorMessage = await ProcessRedemptionRecord(redeemConsumerCsvDto.consumer_code, tenant.TenantCode);
                        totalRecordsCount++;
                        if (!string.IsNullOrEmpty(errorMessage))
                            errorMessageList.Add(errorMessage);
                    }
                }
                
                // File name prefixed with S3 folder path.
                var fileName = $"{Constants.HSA_OUTBOUND_FOLDER}/{FilePrefixes.HSA_TRANSFER_OUTPUT_FILE_PREFIX}{DateTime.UtcNow.ToString(Constants.yyyyMMdd_HHmmss)}{Constants.txtFileExtension}";
                
                await _s3Helper.UploadCsvFileToS3<HsaResponseDto>(_hsaCsvFileList, reoprtS3BucketName, fileName);
            }
            // Else then create the file in LocalDownloadFolderPath folder Ex: C:\tmp
            else
            {
                var memberImportFileReader = new StreamReader(etlExecutionContext.RedeemConsumerListFilePath);
                var csvConfiguration = new CsvConfiguration(CultureInfo.InvariantCulture) { Delimiter = "\t" };
                using var redeemConsumerListCSV = new CsvReader(memberImportFileReader, csvConfiguration);
                while (await redeemConsumerListCSV.ReadAsync())
                {
                    var redeemConsumerCsvDto = redeemConsumerListCSV.GetRecord<RedeemConsumerCSVDto>();
                    if (redeemConsumerCsvDto != null)
                    {
                        var errorMessage = await ProcessRedemptionRecord(redeemConsumerCsvDto.consumer_code, tenant.TenantCode);
                        totalRecordsCount++;
                        if (!string.IsNullOrEmpty(errorMessage))
                            errorMessageList.Add(errorMessage);
                    }
                }

                // File name prefixed with LocalDownloadFolderPath Ex: C:\tmp
                var fileName = Path.Combine(etlExecutionContext.LocalDownloadFolderPath, $"{FilePrefixes.HSA_TRANSFER_OUTPUT_FILE_PREFIX}{DateTime.UtcNow.ToString(Constants.yyyyMMdd_HHmmss)}{Constants.txtFileExtension}");
                await _csvHelper.CreateCsvFile<HsaResponseDto>(csvConfiguration, _hsaCsvFileList, fileName);
            }

            LogErrorMessages(errorMessageList);

            //set job history status
            etlExecutionContext.JobHistoryStatus = errorMessageList.Count == 0
                ? Constants.JOB_HISTORY_SUCCESS_STATUS
                : (errorMessageList.Count == totalRecordsCount
                    ? Constants.JOB_HISTORY_FAILURE_STATUS
                    : Constants.JOB_HISTORY_PARTIAL_SUCCESS_STATUS);
            etlExecutionContext.JobHistoryErrorLog = errorMessageList.Count != 0
                ? $"Errored records count: {errorMessageList.Count}"
                : string.Empty;

            _logger.LogInformation("{ClassName}.{MethodName} - HSA redemption process completed for tenant: {TenantCode}", className, methodName, etlExecutionContext.TenantCode);
        }

        /// <summary>
        /// Generate wallet balances report CSV file with tab delimiter
        /// </summary>
        /// <param name="etlExecutionContext"></param>
        /// <returns></returns>
        public async Task GenerateWalletBalancesReport(EtlExecutionContext etlExecutionContext)
        {
            const string methodName = nameof(GenerateWalletBalancesReport);
            try
            {
                _logger.LogInformation("{ClassName}.{MethodName} - Started Processing. Request: {Request}", className, methodName, _jsonWrapper.SerializeObject(etlExecutionContext));

                var tenant = await _tenantRepo.FindOneAsync(x => x.TenantCode == etlExecutionContext.TenantCode);
                if (tenant == null)
                {
                    _logger.LogError("{ClassName}.{MethodName} - Tenant  not found with TenantCode:{Code}", className, methodName,etlExecutionContext.TenantCode);
                    throw new ETLException(ETLExceptionCodes.NotFoundInDb, $"Tenant not found in DB with TenantCode : {etlExecutionContext.TenantCode}");
                }

                var walletType = await _walletTypeRepo.FindOneAsync(x => x.WalletTypeCode == etlExecutionContext.WalletTypeCode);
                if (walletType == null)
                {
                    _logger.LogError("{ClassName}.{MethodName} - Invalid WalletTypeCode:{Code}.", className, methodName,etlExecutionContext.WalletTypeCode);
                    throw new ETLException(ETLExceptionCodes.NotFoundInDb, $"Invalid WalletTypeCode : {etlExecutionContext.WalletTypeCode}");
                }

                int skip = 0;
                int batchSize = etlExecutionContext.BatchSize;
                IQueryable<ETLConsumerAndPersonModel> batch;
                var walletBalancesReportList = new List<WalletBalancesReportDto>();

                do
                {
                    // Fetch the batch of consumer and person records
                    batch = _consumersRepo.GetConsumersAndPersonsByTenantCode(etlExecutionContext.TenantCode, skip, batchSize);
                    var consumersAndPersons = batch.ToList();

                    if (consumersAndPersons.Count > 0)
                    {
                        var walletBalancesReport = await GetWalletBalancesReport(etlExecutionContext.TenantCode, consumersAndPersons, walletType.WalletTypeId);
                        walletBalancesReportList = walletBalancesReportList.Concat(walletBalancesReport.ToList()).ToList();
                    }

                    // Increment the skip value by batch size
                    skip += batchSize;

                } while (batch.Any());

                var csvConfiguration = new CsvConfiguration(CultureInfo.InvariantCulture) { Delimiter = "\t" };

                // If LocalDownloadFolderPath is null or empty, then upload the file to the S3 bucket
                if (string.IsNullOrEmpty(etlExecutionContext.LocalDownloadFolderPath))
                {
                    // 📁 Construct the S3 file path using folder and tenant-specific filename
                    var fileName = $"{Constants.WALLET_BALANCES_FOLDER}/{FilePrefixes.WALLET_BALANCES_FILE_PREFIX}{etlExecutionContext.TenantCode}_{DateTime.UtcNow.ToString("yyyyMMdd_HHmmss")}.txt";

                    // 🪣 Retrieve the name of the AWS S3 bucket designated for report storage
                    var reportsS3BucketName = GetAwsSunnyHeliosReportsS3BucketName();

                    // ☁️ Upload the CSV report file to the specified S3 bucket
                    await _s3Helper.UploadCsvFileToS3<WalletBalancesReportDto>(walletBalancesReportList, reportsS3BucketName, fileName);
                }
                else
                {
                    // 💾 Construct the local file path using the configured download folder and timestamped filename
                    var fileName = Path.Combine(etlExecutionContext.LocalDownloadFolderPath, $"wallet_balances_report_{DateTime.UtcNow.ToString("yyyyMMdd_HHmmss")}.txt");

                    // 📝 Create the CSV report file locally at the specified path
                    await _csvHelper.CreateCsvFile<WalletBalancesReportDto>(csvConfiguration, walletBalancesReportList, fileName);
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex,"{ClassName}.{MethodName} - Failed Processing,ErrorCode:{Code},ERROR: {Msg}", className, methodName,StatusCodes.Status500InternalServerError,ex.Message);
                throw;
            }
            finally
            {
                _logger.LogInformation("{ClassName}.{MethodName} - Ended Processing.", className, methodName);
            }
        }

        private bool ValidateInputParameters(EtlExecutionContext etlExecutionContext)
        {
            if (string.IsNullOrEmpty(etlExecutionContext.TenantCode))
            {
                _logger.LogError("{ClassName}.ValidateInputParameters: No tenant code supplied for HSA redemption",className);
                throw new ETLException(ETLExceptionCodes.NullValue, "No tenant code supplied for HSA redemption");
            }

            if (string.IsNullOrEmpty(etlExecutionContext.RedeemConsumerListFilePath))
            {
                _logger.LogError("{ClassName}.ValidateInputParameters: No file path provided for redeeming consumers",className);
                throw new ETLException(ETLExceptionCodes.NullValue, "No tenant code supplied for HSA redemption");
            }

            return true;
        }

        private async Task<ETLTenantModel?> GetTenant(string tenantCode)
        {
            var tenant = await _tenantRepo.FindOneAsync(x => x.TenantCode == tenantCode && x.DeleteNbr == 0);
            if (tenant == null)
            {
                _logger.LogError("{ClassName}.GetTenant: Invalid tenant code supplied for HSA redemption, Tenant code:{TenantCode}",className, tenantCode);
            }
            return tenant;
        }

        private async Task<string> ProcessRedemptionRecord(string? consumerCode, string? tenantCode)
        {
            const string methodName=nameof(ProcessRedemptionRecord);
            var hsaCsvFileDto = new HsaResponseDto()
            {
                ConsumerCode = consumerCode,
            };

            var consumer = await _consumersRepo.FindOneAsync(x => x.ConsumerCode == consumerCode && x.DeleteNbr == 0);
            if (consumer == null)
            {
                _logger.LogError("{ClassName}.{MethodName} - Consumer not found with ConumserCode:{Code}", className, methodName, consumerCode);
                hsaCsvFileDto.Comments = $"Consumer not found.";
                _hsaCsvFileList.Add(hsaCsvFileDto);
                return $"ProcessRedemptionRecord: Error - Consumer code: {consumerCode} not found";
            }

            var person = await _personRepo.FindOneAsync(x => x.PersonId == consumer.PersonId);
            if (person == null)
            {
                _logger.LogError("{ClassName}.{MethodName} - Person not found with PersonId:{Id}", className, methodName, consumer.PersonId);
                hsaCsvFileDto.Comments = $"Person not found.";
                _hsaCsvFileList.Add(hsaCsvFileDto);
                return $"ProcessRedemptionRecord: Error - PersonId: {consumerCode} not found";
            }
            hsaCsvFileDto.Name = $"{person.FirstName} {person.LastName}";

            var consumerRewardsWallet = await GetConsumerWallet(tenantCode, consumerCode);
            if (consumerRewardsWallet == null || consumerRewardsWallet.WalletId <= 0)
            {
                _logger.LogError("{ClassName}.{MethodName} - ConsumerWallet not found with ConumserCode:{Code}", className, methodName, consumerCode);
                hsaCsvFileDto.Comments = $"Consumer wallet not found.";
                _hsaCsvFileList.Add(hsaCsvFileDto);
                return $"ProcessRedemptionRecord: Error - Consumer wallet not found, Consumer code: {consumerCode}"; ;
            }

            if (consumerRewardsWallet.Balance <= 0)
            {
                hsaCsvFileDto.Comments = $"The wallet balance is insufficient for redemption.";
                _hsaCsvFileList.Add(hsaCsvFileDto);
                return $"ProcessRedemptionRecord: The wallet balance for consumer {consumerCode} is insufficient for redemption. Current Balance: {consumerRewardsWallet.Balance}";
            }

            var redemptionResponse = await ExecuteRedemption(consumerCode, tenantCode, consumerRewardsWallet.Balance);
            hsaCsvFileDto.Amount = consumerRewardsWallet.Balance;
            hsaCsvFileDto.Comments = redemptionResponse;
            _hsaCsvFileList.Add(hsaCsvFileDto);

            return redemptionResponse;

        }

        private async Task<string> ExecuteRedemption(string? consumerCode, string? tenantCode, double balance)
        {
            try
            {
                var redemptionRef = "red-hsa-" + Guid.NewGuid().ToString("N");
                var postRedeemStartRequestDto = new PostRedeemStartRequestDto()
                {
                    ConsumerCode = consumerCode,
                    TenantCode = tenantCode,
                    ConsumerWalletTypeCode = GetRewardWalletTypeCode(),
                    RedemptionWalletTypeCode = GetRedemptionWalletTypeCode(),
                    RedemptionVendorCode = Constants.RedemptionVendorCodeHsa,
                    RedemptionRef = redemptionRef,
                    RedemptionAmount = balance,
                    RedemptionItemDescription = Constants.RedemptionItemDescriptionForHsa
                };

                var redeemResponse = await _adminClient.Post<PostRedeemStartResponseDto>("admin/redeem", postRedeemStartRequestDto);
                if (redeemResponse.ErrorCode != null)
                {
                    _logger.LogError("{ClassName}.ExecuteRedemption: Error occurred while redeeming consumer wallet balance, Consumer code:{ConsumerCode}, ErrorCode: {ErrorCode}, ErrorMessage: {ErrorMessage}",className,consumerCode,redeemResponse.ErrorCode,redeemResponse.ErrorMessage);
                    return $"ExecuteRedemption: Error occurred while redeeming consumer wallet balance, Consumer code:{consumerCode}, ErrorCode: {redeemResponse.ErrorCode}, ErrorMessage: {redeemResponse.ErrorMessage}";
                }
                else
                {
                    _logger.LogInformation("{ClassName}.ExecuteRedemption: Successfully redeemed for Consumer: {ConsumerCode}, Balance: {Balance}",className, consumerCode, balance);
                    return string.Empty;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,"{ClassName}.ExecuteRedemption: Error occurred while redeeming consumer wallet balance, Consumer code: {ConsumerCode},ErrorCode:{Code}, ERROR: {Message}", className, consumerCode, StatusCodes.Status500InternalServerError,ex.Message);
                return $"ExecuteRedemption: Error occurred while redeeming consumer wallet balance, Consumer code: {consumerCode}, ErrorMessage: {ex.Message}";
            }

        }

        private async Task<ETLWalletModel> GetConsumerWallet(string? tenantCode, string? consumerCode)
        {
            var rewardWalletTypeData = await GetRewardWalletType();
            return await _walletRepo.GetWalletByConsumerAndWalletType(tenantCode, consumerCode, rewardWalletTypeData.WalletTypeId);
        }

        private async Task<ETLWalletTypeModel> GetRewardWalletType()
        {
            string? walletTypeCode = GetRewardWalletTypeCode();
            return await _walletTypeRepo.FindOneAsync(x => x.WalletTypeCode == walletTypeCode && x.DeleteNbr == 0);
        }

        private void LogErrorMessages(List<string> errorMessageList)
        {
            foreach (var error in errorMessageList)
            {
                _logger.LogError(error);
            }
        }

        private string? GetRedemptionWalletTypeCode()
        {
            return _configuration.GetSection("Redemption_Wallet_Type_Code").Value;
        }

        private string? GetRewardWalletTypeCode()
        {
            return _configuration.GetSection("Reward_Wallet_Type_Code").Value;
        }

        /// <summary>
        /// Get wallet balances report by tenant code and wallet type
        /// </summary>
        /// <param name="tenantCode"></param>
        /// <param name="walletTypeId"></param>
        /// <returns></returns>
        private async Task<List<WalletBalancesReportDto>> GetWalletBalancesReport(string? tenantCode, IList<ETLConsumerAndPersonModel?> consumersAndPersons, long walletTypeId)
        {
            const string methodName = nameof(GetWalletBalancesReport);

            _logger.LogInformation("{ClassName}.{MethodName} - Started Processing. TenantCode: {TenantCode}, walletTypeId:{WalletTypeId}", className, methodName, tenantCode,walletTypeId);

            var walletBalancesReportList = new List<WalletBalancesReportDto>();
            try
            {
                foreach (var item in consumersAndPersons)
                {
                    if (item?.Consumer?.ConsumerCode == null)
                    {
                        _logger.LogError("{ClassName}.{MethodName} - No consumer found for given ConsumerCode:{Code}.", className, methodName,item?.Consumer?.ConsumerCode);
                        continue;
                    }

                    if (item?.Person?.PersonId == 0)
                    {
                        _logger.LogError("{ClassName}.{MethodName} - Person not found with PersonId:{Id}", className, methodName, item.Consumer.PersonId);
                        continue;
                    }

                    var wallet = await _walletRepo.GetWalletByConsumerAndWalletType(tenantCode, item.Consumer.ConsumerCode, walletTypeId);
                    if (wallet == null)
                    {
                        _logger.LogError("{ClassName}.{MethodName} - ConsumerWallet not found with ConumserCode:{Code}", className, methodName, item?.Consumer.ConsumerCode);
                        continue;
                    }

                    walletBalancesReportList.Add(new WalletBalancesReportDto()
                    {
                        TimeStamp = DateTime.Now,
                        ConsumerCode = item.Consumer.ConsumerCode,
                        FirstName = item.Person.FirstName,
                        LastName = item.Person.LastName,
                        Email = item.Person.Email,
                        WalletCode = wallet.WalletCode,
                        Balance = wallet.Balance
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,"{ClassName}.{MethodName} - Failed processing ErrorCode:{Code},ERROR:{Msg}", className, methodName,StatusCodes.Status500InternalServerError,ex.Message);
                throw;
            }

            finally
            {
                _logger.LogInformation("{ClassName}.{MethodName} - Ended Processing.");
            }
            return walletBalancesReportList;
        }

        private string GetAwsTmpS3BucketName()
        {
            return _configuration.GetSection("AWS:AWS_TMP_BUCKET_NAME").Value?.ToString() ?? "";
        }
    }
}
