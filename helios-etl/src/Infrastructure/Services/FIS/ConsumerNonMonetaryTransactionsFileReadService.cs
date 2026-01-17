using AutoMapper;
using Azure.Core;
using Azure;
using FirebaseAdmin.Auth.Multitenancy;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SunnyRewards.Helios.Common.Core.Helpers.Interfaces;
using SunnyRewards.Helios.ETL.Common.Constants;
using SunnyRewards.Helios.ETL.Common.CustomException;
using SunnyRewards.Helios.ETL.Common.Helpers.Interfaces;
using SunnyRewards.Helios.ETL.Core.Domain.Constants;
using SunnyRewards.Helios.ETL.Core.Domain.Constants.FIS;
using SunnyRewards.Helios.ETL.Core.Domain.Dtos;
using SunnyRewards.Helios.ETL.Core.Domain.Dtos.FIS;
using SunnyRewards.Helios.ETL.Core.Domain.Dtos.Json;
using SunnyRewards.Helios.ETL.Core.Domain.Models;
using SunnyRewards.Helios.ETL.Infrastructure.AwsConfig;
using SunnyRewards.Helios.ETL.Infrastructure.Helpers.Interfaces;
using SunnyRewards.Helios.ETL.Infrastructure.Repositories.HeliosRepo.Interfaces;
using SunnyRewards.Helios.ETL.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.ETL.Infrastructure.Services.Interfaces.FIS;
using SunnyRewards.Helios.Tenant.Core.Domain.Dtos.Json;
using SunnyRewards.Helios.Wallet.Core.Domain.Constants;
using SunnyRewards.Helios.Wallet.Core.Domain.Dtos;
using System.Diagnostics.Eventing.Reader;
using SunnyRewards.Helios.ETL.Infrastructure.HttpClients.Interfaces;
using System.Threading.Tasks;
using SunnyRewards.Helios.ETL.Common.Domain.Dtos;
using static Org.BouncyCastle.Math.EC.ECCurve;
using Microsoft.ClearScript;
using NHibernate.Loader.Custom;

namespace SunnyRewards.Helios.ETL.Infrastructure.Services.FIS
{
    public class ConsumerNonMonetaryTransactionsFileReadService : AwsConfiguration, IConsumerNonMonetaryTransactionsFileReadService
    {
        private readonly IFlatFileReader _flatFileReader;
        private readonly ILogger<ConsumerNonMonetaryTransactionsFileReadService> _logger;
        private readonly IPgpS3FileEncryptionHelper _s3FileEncryptionHelper;
        private readonly ITenantRepo _tenantRepo;
        private readonly ITenantAccountRepo _tenantAccountRepo;
        private readonly IConsumerAccountRepo _consumerAccountRepo;
        private readonly IS3Helper _s3Helper;
        private readonly IJobReportService _jobReportService;
        private readonly IAdminClient _adminClient;
        private readonly IConfiguration _configuration;
        const string className = nameof(ConsumerNonMonetaryTransactionsFileReadService);

        public ConsumerNonMonetaryTransactionsFileReadService(IFlatFileReader flatFileReader, ILogger<ConsumerNonMonetaryTransactionsFileReadService> logger, IS3Helper s3Helper,
        ITenantRepo tenantRepo, IVault vault, IConfiguration configuration, IPgpS3FileEncryptionHelper s3FileEncryptionHelper, IConsumerAccountRepo consumerAccountRepo, ITenantAccountRepo tenantAccountRepo
            , IJobReportService jobReportService , IAdminClient adminClient)
            : base(vault, configuration)
        {
            _flatFileReader = flatFileReader;
            _logger = logger;
            _s3FileEncryptionHelper = s3FileEncryptionHelper;
            _tenantRepo = tenantRepo;
            _consumerAccountRepo = consumerAccountRepo;
            _tenantAccountRepo = tenantAccountRepo;
            _s3Helper = s3Helper;
            _jobReportService = jobReportService;
            _adminClient = adminClient;
            _configuration = configuration;
        }

        public async Task ImportConsumerNonMonetaryTransactions(EtlExecutionContext etlExecutionContext)
        {
            const string methodName = nameof(ImportConsumerNonMonetaryTransactions);
            try
            {

                _jobReportService.BatchJobRecords.JobType = methodName;
                _jobReportService.JobResultDetails.Files.Add(etlExecutionContext.ConsumerNonMonetaryTransactionsFileName);
                _logger.LogInformation("{className}.{methodName}: Starting to process read non monetary transaction file.", className, methodName);

                var invalidTenant = new List<string>();
                var validTenant = new List<ETLTenantModel>();
                var validTenantConfig = new Dictionary<string, FISTenantConfigDto>();
                var providedTenantCodes = etlExecutionContext.TenantCode.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                               .Select(t => t.Trim())
                                               .ToList();

                foreach (var tenantCode in providedTenantCodes)
                {
                    var tenant = await _tenantRepo.FindOneAsync(x => x.TenantCode == tenantCode && x.DeleteNbr == 0);

                    if (tenant == null)
                    {
                        _logger.LogWarning("{ClassName}.{MethodName}: Invalid tenant code: {TenantCode}, Error Code: {ErrorCode}",
                            className, methodName, tenantCode, StatusCodes.Status404NotFound);
                        invalidTenant.Add(tenantCode);
                        continue;
                    }

                    var tenantAccount = await _tenantAccountRepo.FindOneAsync(x => x.TenantCode == tenantCode && x.DeleteNbr == 0);

                    if (tenantAccount == null)
                    {
                        _logger.LogWarning("{ClassName}.{MethodName}: No tenant account for tenant code: {TenantCode}, Error Code: {ErrorCode}",
                            className, methodName, tenantCode, StatusCodes.Status404NotFound);
                        invalidTenant.Add(tenantCode);
                        continue;
                    }
                    validTenant.Add(tenant);

                    if (!string.IsNullOrEmpty(tenantAccount.TenantConfigJson))
                    {
                        var tenantConfig = JsonConvert.DeserializeObject<FISTenantConfigDto>(tenantAccount.TenantConfigJson);
                        if (tenantConfig != null)
                        {
                            validTenantConfig.Add(tenantCode, tenantConfig);
                        }
                    }
                }
                var secureFileTransferRequestDto = new SecureFileTransferRequestDto
                {
                    TenantCode = validTenant[0].TenantCode,
                    SourceBucketName = GetAwsFisSftpS3BucketName(),
                    SourceFileName = etlExecutionContext.ConsumerNonMonetaryTransactionsFileName,
                    SourceFolderName = FISBatchConstants.FIS_DATA_EXTRACT_INBOUND_FOLDER,
                    FisAplPublicKeyName = GetFisAplPublicKeyName(),
                    SunnyAplPrivateKeyName = GetSunnyAplPrivateKeyName(),
                    SunnyAplPublicKeyName = GetSunnyAplPublicKeyName(),
                    TargetBucketName = GetAwsFisSftpS3BucketName(),
                    TargetFileName = "",
                    DeleteFileAfterCopy = false,
                    PassPhraseKeyName = GetSunnyPrivateKeyPassPhraseKeyName(),
                    ArchiveBucketName = GetAwsSunnyArchiveFileBucketName(),
                    InboundArchiveFolderName = FISBatchConstants.FIS_DATA_EXTRACT_INBOUND_ARCHIVE_FOLDER,

                };
                // Download and decrypt the file
                var response = await _s3FileEncryptionHelper.DownloadAndDecryptFile(secureFileTransferRequestDto);
             //var response = await File.ReadAllBytesAsync(@"C:\tmp\Con-non-mon.txt");

                var dataStream = new MemoryStream(response);
                using (StreamReader reader = new StreamReader(dataStream))
                {
                    string? line;
                    int recordNbr = -1;
                    while ((line = await reader.ReadLineAsync()) != null)
                    {
                        try
                        {
                            var recordType = line.Substring(0, 1);
                            if (recordType == FISBatchConstants.MONETORY_RECORD_TYPE)
                            {
                                _jobReportService.JobResultDetails.RecordsReceived++;
                                recordNbr++;

                                var fisMonetoryDetailData = _flatFileReader.ReadFlatFileRecord<ConsumerNonMonetoryDetailDto>(line, FISBatchConstants.CONSUMER_NON_MONETORY_RECORD_DELIMITER);
                                var consumerTenant = (ValidateFISProgramDetails(fisMonetoryDetailData, validTenantConfig));
                                if (!string.IsNullOrEmpty(consumerTenant))
                                {
                                    var consumerAccount = await GetConsumerAccountByProxyOrClientIdAsync(fisMonetoryDetailData, consumerTenant);
                                    if (consumerAccount == null)
                                    {
                                        var message = $"{className}.{methodName} Consumer account not found with CardNumberProxy or CardholderClientUniqueID of non monetary file, CardNumberProxy: {fisMonetoryDetailData.CardNumberProxy}, CardholderClientUniqueID: {fisMonetoryDetailData.CardholderClientUniqueID}";
                                        _logger.LogInformation(message);
                                        throw new ETLException(ETLExceptionCodes.NotFoundInDb, message);
                                    }
                                    if (consumerAccount?.ProxyNumber != fisMonetoryDetailData.CardNumberProxy)
                                    {
                                        consumerAccount!.ProxyNumber = fisMonetoryDetailData.CardNumberProxy;
                                    }
                                    if (fisMonetoryDetailData.CardNumber == null || fisMonetoryDetailData.CardNumber.Length < 4)
                                    {
                                        throw new ETLException(ETLExceptionCodes.NullValue, "CardNumber is null or too short to extract the last 4 digits.");
                                    }
                                    
                                    var cardLast4 = fisMonetoryDetailData.CardNumber[^4..];
                                    consumerAccount.CardLast4 = cardLast4;

                                    if (consumerAccount.CardIssueStatus == BenefitsConstants.Card30BatchSentStatus)
                                    {
                                        consumerAccount.CardIssueStatus = BenefitsConstants.EligibleForActivationCardIssueStatus;
                                    }

                                    consumerAccount.UpdateTs = DateTime.UtcNow;
                                    consumerAccount.UpdateUser = Constants.UpdateUser;
                                    //Update consumerAccount along with ConsumerAccountHistory table
                                    await _consumerAccountRepo.UpdateConsumerAccount(consumerAccount);

                                    await LiveFundTransferToPurse(validTenant, consumerAccount);

                                    _logger.LogInformation("{className}.{methodName}: Updating card Last 4 digits for proxy Number {ProxyNbr}", className, methodName, fisMonetoryDetailData.CardNumberProxy);
                                    _jobReportService.JobResultDetails.RecordsSuccessCount++;
                                    _jobReportService.JobResultDetails.RecordsProcessed++;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "{className}.{MethodName}: An error occurred while processing a line in the non monetary transaction file., Error: {Message}", className, methodName, ex.Message);
                            _jobReportService.JobResultDetails.RecordsErrorCount++;
                            _jobReportService.CollectError(recordNbr, 400, null, ex);
                            _jobReportService.JobResultDetails.RecordsProcessed++;

                        }
                    }
                }
                //set job history status
                etlExecutionContext = _jobReportService.SetJobHistoryStatus(etlExecutionContext);
                //set tenantCodes
                etlExecutionContext.TenantCode = string.Join(',', validTenant);

                //Save Errors
                if (!string.IsNullOrWhiteSpace(etlExecutionContext.ConsumerNonMonetaryTransactionsFileName))
                    await _jobReportService.SaveEtlErrors();


                var sourceFileFullPath = secureFileTransferRequestDto.SourceFolderName != null
                       ? $"{secureFileTransferRequestDto.SourceFolderName.TrimEnd('/')}/{secureFileTransferRequestDto.SourceFileName}"
                       : secureFileTransferRequestDto.SourceFileName;

                var archiveFileFullPath = secureFileTransferRequestDto.InboundArchiveFolderName != null
                            ? $"{secureFileTransferRequestDto.InboundArchiveFolderName.TrimEnd('/')}/{secureFileTransferRequestDto.SourceFileName}"
                            : $"{FISBatchConstants.ARCHIVE_FOLDER_NAME}/{secureFileTransferRequestDto.SourceFileName}";

                await _s3Helper.MoveFileToFolder(secureFileTransferRequestDto.SourceBucketName, sourceFileFullPath,
                     secureFileTransferRequestDto.ArchiveBucketName, archiveFileFullPath);
                _logger.LogInformation("{className}.{MethodName}:non Monetary transaction file processing completed.", className, methodName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{className}.{methodName}: ReadNonMonetaryTransactionFile An error occurred while processing non monetary transaction file. Error: {ErrorMessage}", className, methodName, ex.Message);
                _jobReportService.JobResultDetails.RecordsErrorCount++;
                _jobReportService.CollectError(0, 400, null, ex);
                await _jobReportService.SaveEtlErrors();
                throw;
            }

        }

        string ValidateFISProgramDetails(ConsumerNonMonetoryDetailDto fisMonetoryDetailData, Dictionary<string, FISTenantConfigDto> validTenantConfigs)
        {
            const string methodName = nameof(ValidateFISProgramDetails);

            // Find the first matching TenantConfig based on IssuerClientId
            var matchingConfigEntry = validTenantConfigs.FirstOrDefault(config =>
                config.Value.FISProgramDetail != null &&
                Convert.ToInt64(config.Value.FISProgramDetail.ClientId) == fisMonetoryDetailData.IssuerClientId);

            if (matchingConfigEntry.Equals(default(KeyValuePair<string, FISTenantConfigDto>)))
            {
                var msg = $"No valid TenantConfig found matching IssuerClientId: {fisMonetoryDetailData.IssuerClientId}";
                _logger.LogInformation("{className}.{methodName}: {message}", className, methodName, msg);
                throw new ETLException(ETLExceptionCodes.InValidValue, msg);
            }

            string matchingConfigKey = matchingConfigEntry.Key;
            var fisProgramDetails = matchingConfigEntry.Value.FISProgramDetail!;

            // Validate additional fields
            if (Convert.ToInt64(fisProgramDetails.CompanyId) != fisMonetoryDetailData.TopClientId ||
                Convert.ToInt64(fisProgramDetails.SubprogramId) != fisMonetoryDetailData.SubProgramId)
            {
                var message = @$"ValidateFISProgramDetails - Record not validated with 
            Company Id : {fisMonetoryDetailData.TopClientId}, 
            Client Id : {fisMonetoryDetailData.IssuerClientId},  
            SubClient Id : {fisMonetoryDetailData.SubProgramId}, 
            Package Id : {fisMonetoryDetailData.PackageId}";

                _logger.LogInformation("{className}.{methodName}: {message}", className, methodName, message);
                throw new ETLException(ETLExceptionCodes.InValidValue, message);
            }

            return matchingConfigKey; // Return the key of the matched TenantConfig
        }

        private async Task<ETLConsumerAccountModel?> GetConsumerAccountByProxyOrClientIdAsync(ConsumerNonMonetoryDetailDto fisMonetoryDetailData, string consumerTenant)
        {
            ETLConsumerAccountModel? consumerAccount = null;
            if (!string.IsNullOrEmpty(fisMonetoryDetailData.CardNumberProxy))
            {
                consumerAccount = await _consumerAccountRepo.FindOneAsync(x => x.ProxyNumber == fisMonetoryDetailData.CardNumberProxy && x.TenantCode == consumerTenant);
            }
            if (consumerAccount == null && !string.IsNullOrEmpty(fisMonetoryDetailData.CardholderClientUniqueID))
            {
                consumerAccount = await _consumerAccountRepo.FindOneAsync(x => x.ClientUniqueId == fisMonetoryDetailData.CardholderClientUniqueID && x.TenantCode == consumerTenant);
            }

            return consumerAccount;
        }
        private async Task<bool> LiveFundTransferToPurse(List<ETLTenantModel> validTenants, ETLConsumerAccountModel consumerAccountModel)
        {
            try
            {
                if (!CheckSupportLiveTransferToRewardsPurseflag(validTenants, consumerAccountModel))
                {
                    _logger.LogInformation("Live transfer skipped: Flag not enabled for ConsumerCode={ConsumerCode}, TenantCode={TenantCode}",
                        consumerAccountModel.ConsumerCode, consumerAccountModel.TenantCode);
                    return false;
                }

                var consumerWallets = await GetConsumerWallets(consumerAccountModel);
                var rewardWallet = consumerWallets.ConsumerWalletDetails
                    .FirstOrDefault(x => x.WalletType?.WalletTypeCode == GetRewardWalletTypeCode());

                if (rewardWallet?.Wallet?.Balance > 0)
                {
                    var fundTransferRequest = new FundTransferToPurseRequestDto
                    {
                        ConsumerCode = consumerAccountModel.ConsumerCode,
                        TenantCode = consumerAccountModel.TenantCode,
                        ConsumerWalletTypeCode = GetRewardWalletTypeCode(),
                        RedemptionVendorCode = Constants.HealthyLivingRedumtionVendorCode,
                        RedemptionAmount = rewardWallet.Wallet.Balance,
                        PurseWalletType = GetHealthyLivingPurseWalletTypeCode()
                    };

                    var response = await _adminClient.Post<BaseResponseDto>("fund-transfer", fundTransferRequest);

                    _logger.LogInformation("Fund transfer completed for ConsumerCode={ConsumerCode} with amount={Amount}",
                        fundTransferRequest.ConsumerCode, fundTransferRequest.RedemptionAmount);

                    return true;
                }
                else
                {
                    _logger.LogInformation("No balance to transfer for ConsumerCode={ConsumerCode}", consumerAccountModel.ConsumerCode);
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in LiveFundTransferToPurse for ConsumerCode={ConsumerCode}, TenantCode={TenantCode}",
                    consumerAccountModel.ConsumerCode, consumerAccountModel.TenantCode);
                return false;
            }
        }
        private bool CheckSupportLiveTransferToRewardsPurseflag(List<ETLTenantModel> validTenant , ETLConsumerAccountModel consumerAccountModel)
        {
            var consumer = consumerAccountModel.ConsumerCode;
            var consumerTenant = validTenant.Where(s => s.TenantCode == consumerAccountModel.TenantCode).FirstOrDefault();

            if (consumerTenant != null)
            {
                var tenantOption = !string.IsNullOrEmpty(consumerTenant.TenantOption)
                    ? JsonConvert.DeserializeObject<TenantOption>(consumerTenant.TenantOption)
                    : new TenantOption();

                if (tenantOption?.Apps?.Any(x => string.Equals(x, Constants.Benefits, StringComparison.OrdinalIgnoreCase)) == true)
                {
                    var tenantAttributes = !string.IsNullOrEmpty(consumerTenant.TenantAttribute)
                        ? JsonConvert.DeserializeObject<TenantAttribute>(consumerTenant.TenantAttribute)
                        : new TenantAttribute();
                    return tenantAttributes?.SupportLiveTransferToRewardsPurse == true &&
                            tenantAttributes?.SupportLiveTransferWhileProcessingNonMonetary == true;
                }
            }

            return false;
        }

        private async Task<ConsumerWalletResponseDto> GetConsumerWallets(ETLConsumerAccountModel consumerAccountModel)
        {
            var getConsumerWalletRequestDto = new GetConsumerWalletRequestDto()
            {
                TenantCode = consumerAccountModel.TenantCode,
                ConsumerCode = consumerAccountModel.ConsumerCode
            };
            return await _adminClient.Post<ConsumerWalletResponseDto>(AdminConstants.GetAllConsumerWallets, getConsumerWalletRequestDto);
        }

        private string? GetRewardWalletTypeCode()
        {
            return _configuration.GetSection("Reward_Wallet_Type_Code").Value;
        }

        private string? GetHealthyLivingPurseWalletTypeCode()
        {
            return _configuration.GetSection("Healthy_Living_Wallet_Type_Code").Value;
        }
    }
}
