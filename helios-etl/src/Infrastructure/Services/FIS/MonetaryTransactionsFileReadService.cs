using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SunnyRewards.Helios.Common.Core.Helpers.Interfaces;
using SunnyRewards.Helios.ETL.Common.Constants;
using SunnyRewards.Helios.ETL.Common.Helpers.Interfaces;
using SunnyRewards.Helios.ETL.Core.Domain.Constants.FIS;
using SunnyRewards.Helios.ETL.Core.Domain.Dtos;
using SunnyRewards.Helios.ETL.Core.Domain.Dtos.Enums;
using SunnyRewards.Helios.ETL.Core.Domain.Dtos.FIS;
using SunnyRewards.Helios.ETL.Core.Domain.Models;
using SunnyRewards.Helios.ETL.Infrastructure.AwsConfig;
using SunnyRewards.Helios.ETL.Infrastructure.Helpers.Interfaces;
using SunnyRewards.Helios.ETL.Infrastructure.Repositories.HeliosRepo.Interfaces;
using SunnyRewards.Helios.ETL.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.ETL.Infrastructure.Services.Interfaces.FIS;
using ISession = NHibernate.ISession;
using SunnyRewards.Helios.ETL.Common.CustomException;

namespace SunnyRewards.Helios.ETL.Infrastructure.Services.FIS
{
    public class MonetaryTransactionsFileReadService : AwsConfiguration, IMonetaryTransactionsFileReadService
    {
        private readonly IFlatFileReader _flatFileReader;
        private readonly ILogger<MonetaryTransactionsFileReadService> _logger;
        private const string FIS_INBOUND_FOLDER = "FIS/DataExtract/inbound";
        private readonly IPgpS3FileEncryptionHelper _s3FileEncryptionHelper;
        private readonly IMonetaryTransactionRepo _monetaryTransactionRepo;
        private readonly ITenantRepo _tenantRepo;
        private readonly ISession _session;
        private readonly IMapper _mapper;
        private readonly IS3Helper _s3Helper;
        private readonly IJobReportService _jobReportService;
        private readonly ITenantAccountRepo _tenantAccountRepo;
        private readonly IBatchFileService _batchFileService;
        private readonly IConsumerAccountRepo _consumerAccountRepo;
        private readonly IWalletTypeRepo _walletTypeRepo;
        private readonly IWalletRepo _walletRepo;
        private readonly ICSATransactionRepo _csaTransactionRepo;

        const string className = nameof(MonetaryTransactionsFileReadService);
        public MonetaryTransactionsFileReadService(IFlatFileReader flatFileReader, ILogger<MonetaryTransactionsFileReadService> logger,
             ITenantRepo tenantRepo, IVault vault, IConfiguration configuration, IPgpS3FileEncryptionHelper s3FileEncryptionHelper, ISession session,
           IMonetaryTransactionRepo monetaryTransactionRepo, IMapper mapper, IS3Helper s3Helper, IJobReportService jobReportService, ITenantAccountRepo tenantAccountRepo, IBatchFileService batchFileService
            , IConsumerAccountRepo consumerAccountRepo, IWalletTypeRepo walletTypeRepo, IWalletRepo walletRepo, ICSATransactionRepo csaTransactionRepo
            )
            : base(vault, configuration)
        {
            _flatFileReader = flatFileReader;
            _logger = logger;
            _s3FileEncryptionHelper = s3FileEncryptionHelper;
            _monetaryTransactionRepo = monetaryTransactionRepo;
            _tenantRepo = tenantRepo;
            _session = session;
            _mapper = mapper;
            _s3Helper = s3Helper;
            _jobReportService = jobReportService;
            _tenantAccountRepo = tenantAccountRepo;
            _batchFileService = batchFileService;
            _consumerAccountRepo = consumerAccountRepo;
            _walletRepo = walletRepo;
            _walletTypeRepo = walletTypeRepo;
            _csaTransactionRepo = csaTransactionRepo;
        }
        /// <summary>
        /// Entry Method to import Monetary transactions
        /// </summary>
        /// <param name="etlExecutionContext"></param>
        /// <returns></returns>
        public async Task ImportMonetaryTransactions(EtlExecutionContext etlExecutionContext)
        {
            const string methodName = nameof(ImportMonetaryTransactions);
            try
            {
                _logger.LogInformation($"{className}.{methodName}: Starting to process read monetary transaction file.");
                _jobReportService.BatchJobRecords.JobType = nameof(etlExecutionContext.ProcessMonetaryTransactionsBatchFile);
                _jobReportService.JobResultDetails.Files.Add(etlExecutionContext.FISMonetaryTransactionsFileName);

                var batchFile = await _batchFileService.SaveBatchFileRecord(BatchFileDirection.INBOUND, ScanS3FileType.FIS_MONETARY_TXN, etlExecutionContext.FISMonetaryTransactionsFileName);

                var invalidTenant = new List<string>();
                var validTenant = new List<string>();
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
                    validTenant.Add(tenantCode);

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
                    TenantCode = validTenant[0],
                    SourceBucketName = GetAwsFisSftpS3BucketName(),
                    SourceFileName = etlExecutionContext.FISMonetaryTransactionsFileName,
                    SourceFolderName = FISBatchConstants.FIS_DATA_EXTRACT_INBOUND_FOLDER,
                    FisAplPublicKeyName = GetFisAplPublicKeyName(),
                    SunnyAplPrivateKeyName = GetSunnyAplPrivateKeyName(),
                    SunnyAplPublicKeyName = GetSunnyAplPublicKeyName(),
                    TargetBucketName = GetAwsFisSftpS3BucketName(),
                    ArchiveBucketName = GetAwsSunnyArchiveFileBucketName(),
                    InboundArchiveFolderName = FISBatchConstants.FIS_DATA_EXTRACT_INBOUND_ARCHIVE_FOLDER,
                    TargetFileName = "",
                    DeleteFileAfterCopy = false,
                    PassPhraseKeyName = GetSunnyPrivateKeyPassPhraseKeyName()
                };

                // Download and decrypt the file
                 var response = await _s3FileEncryptionHelper.DownloadAndDecryptFile(secureFileTransferRequestDto);

                var dataStream = new MemoryStream(response);
                using (StreamReader reader = new StreamReader(dataStream))
                {
                    string line;
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
                                var fisMonetoryDetailData = _flatFileReader.ReadFlatFileRecord<FISMonetoryDetailDto>(line, FISBatchConstants.MONETORY_RECORD_DELIMITER);
                                var (tenantCode, tenantConfig) = await ValidateFISProgramDetails(fisMonetoryDetailData, validTenantConfig);
                                if (!string.IsNullOrEmpty(tenantCode))
                                {
                                    ETLMonetaryTransactionModel monetaryTransactionRecords = _mapper.Map<ETLMonetaryTransactionModel>(fisMonetoryDetailData);
                                    monetaryTransactionRecords.BatchFileId = batchFile!.BatchFileId;
                                    monetaryTransactionRecords.RecordNbr = recordNbr;
                                    var montransRecord = await _monetaryTransactionRepo.CreateAsync(monetaryTransactionRecords);
                                    //Process CSA
                                    if (montransRecord.TxnTypeName!.ToUpper() == Constants.Adjustment)
                                    {
                                        var csaRecord = await GetCsaRecord(montransRecord, tenantCode, tenantConfig);
                                        if (csaRecord != null)
                                        {
                                            try
                                            {
                                                await _csaTransactionRepo.CreateAsync(csaRecord);
                                            }
                                            catch (Exception ex)
                                            {
                                                //ignore errors
                                                var csaError = ex?.InnerException?.Message ?? ex?.Message;
                                                _logger.LogInformation($"{className}.{methodName}: Error in saving CSA Transaction, for Consumer:{csaRecord.ConsumerCode}, TransactionRefId: {csaRecord.TransactionRefId}, Error Details : {csaError}");
                                                _jobReportService.CollectError(recordNbr, 400, csaError, null);
                                            }
                                        }
                                    }
                                    _jobReportService.JobResultDetails.RecordsSuccessCount++;
                                    _jobReportService.JobResultDetails.RecordsProcessed++;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, $"{className}.{methodName}: An error occurred while processing a line in the monetary transaction file., Error: {ex.Message} Error Code:{StatusCodes.Status500InternalServerError}");
                            _jobReportService.JobResultDetails.RecordsErrorCount++;
                            _jobReportService.CollectError(recordNbr, 400, null, ex);
                            _jobReportService.JobResultDetails.RecordsProcessed++;

                        }

                    }

                    await _batchFileService.UpdateProcessEndTime(batchFile);
                }

                //set job history status
                etlExecutionContext.TenantCode = string.Join(',', validTenant);
                etlExecutionContext = _jobReportService.SetJobHistoryStatus(etlExecutionContext);
                
                //Save Errors
                await _jobReportService.SaveEtlErrors();

                var sourceFileFullPath = secureFileTransferRequestDto.SourceFolderName != null
                       ? $"{secureFileTransferRequestDto.SourceFolderName.TrimEnd('/')}/{secureFileTransferRequestDto.SourceFileName}"
                       : secureFileTransferRequestDto.SourceFileName;

                var archiveFileFullPath = secureFileTransferRequestDto.InboundArchiveFolderName != null
                            ? $"{secureFileTransferRequestDto.InboundArchiveFolderName.TrimEnd('/')}/{secureFileTransferRequestDto.SourceFileName}"
                            : $"{FISBatchConstants.ARCHIVE_FOLDER_NAME}/{secureFileTransferRequestDto.SourceFileName}";

                await _s3Helper.MoveFileToFolder(secureFileTransferRequestDto.SourceBucketName, sourceFileFullPath,
                    secureFileTransferRequestDto.ArchiveBucketName, archiveFileFullPath);
                _logger.LogInformation($"{className}.{methodName}: Monetary transaction file processing completed.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{className}.{methodName}: An error occurred while processing monetary transaction file. Error: {errorMessage}, Error Code:{errorCode}", className, methodName, ex.Message, StatusCodes.Status500InternalServerError);
                _jobReportService.JobResultDetails.RecordsErrorCount++;
                _jobReportService.CollectError(0, 400, null, ex);

                if (etlExecutionContext.ProcessMonetaryTransactionsBatchFile)
                {
                    await _jobReportService.SaveEtlErrors();

                }
                throw;
            }
        }
        /// <summary>
        /// Method to get CSA transaction record based on monetaryDetail
        /// </summary>
        /// <param name="monetaryDetail"></param>
        /// <param name="tenant"></param>
        /// <param name="tenantAccount"></param>
        /// <returns></returns>
        private async Task<ETLCSATransactionModel?> GetCsaRecord(ETLMonetaryTransactionModel monetaryDetail, string tenantCode, FISTenantConfigDto tenantConfig)
        {
            const string methodName = nameof(GetCsaRecord);

            var consumerAccount = await _consumerAccountRepo.FindOneAsync(x => x.ProxyNumber == monetaryDetail.CardNumberProxy.Trim() && x.DeleteNbr == 0 && x.TenantCode == tenantCode);

            if (consumerAccount == null)
            {
                // Log error and continue to next record
                _logger.LogError($"{className}.{methodName}: Consumer does not exists for Card Number Proxy: {monetaryDetail.CardNumberProxy} for tenantCode : {tenantCode}");
                return null;
            }

            // Find consumer wallet using tenant_config_json and Purse No field
            var Purse = tenantConfig.PurseConfig?.Purses.Where(x => x.PurseNumber == monetaryDetail.PurseNo).FirstOrDefault() ?? null;

            if (Purse == null)
            {
                // Log error and continue to next record
                _logger.LogError(($"{className}.{methodName}: Wallet not found for Purse No: {monetaryDetail.PurseNo}"));
                return null;
            }
            var walletType = await _walletTypeRepo.FindOneAsync(x => x.WalletTypeCode == Purse.PurseWalletType && x.IsExternalSync && x.DeleteNbr == 0);
            if (walletType == null)
            {
                _logger.LogError($"{className}.{methodName}: Purse Wallet Type does not exists, PurseWalletType: {Purse.PurseWalletType}");
                return null;
            }
            var consumerWallet = await _walletRepo.GetWalletByConsumerAndWalletType(tenantCode, consumerAccount.ConsumerCode, walletType.WalletTypeId);
            if (consumerWallet == null)
            {
                // Log error and continue to next record
                _logger.LogError($"{className}.{methodName}: Consumer Wallet does not exists for Consumer Code: {consumerAccount.ConsumerCode}");
                return null;
            }

            return new ETLCSATransactionModel()
            {
                CsaTransactionCode = $"cst-{Guid.NewGuid().ToString("N")}",
                TenantCode = tenantCode,
                ConsumerCode = consumerAccount.ConsumerCode!.Trim(),
                WalletId = consumerWallet.WalletId,
                TransactionRefId = monetaryDetail.TxnUid,
                MonetaryTransactionId = monetaryDetail.MonetaryTransactionId,
                Amount = Convert.ToDouble(monetaryDetail.SettleAmount),
                Description = monetaryDetail.DerivedRequestCodeDescription,
                Status = CSATransactionStatus.NEW.ToString(),
                CreateUser = Constants.CreateUser,
                CreateTs = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Validate FIS Program Details
        /// </summary>
        /// <param name="fISMonetoryDetailDto"></param>
        /// <param name="tenantCode"></param>
        /// <returns></returns>
        private async Task<KeyValuePair<string, FISTenantConfigDto>> ValidateFISProgramDetails(FISMonetoryDetailDto fISMonetoryDetailDto, Dictionary<string, FISTenantConfigDto> validTenantConfigs)
        {

            const string methodName = nameof(ValidateFISProgramDetails);

            // Find the first matching TenantConfig based on IssuerClientId
            var matchingConfigEntry = validTenantConfigs.FirstOrDefault(config =>
                config.Value.FISProgramDetail != null &&
                Convert.ToInt64(config.Value.FISProgramDetail.ClientId) == fISMonetoryDetailDto.IssuerClientID);

            if (matchingConfigEntry.Equals(default(KeyValuePair<string, FISTenantConfigDto>)))
            {
                var msg = $"No valid TenantConfig found matching IssuerClientId: {fISMonetoryDetailDto.IssuerClientID}";
                _logger.LogInformation("{className}.{methodName}: {message}", className, methodName, msg);
                throw new ETLException(ETLExceptionCodes.InValidValue, msg);
            }

            var fisProgramDetails = matchingConfigEntry.Value.FISProgramDetail!;

            // Validate additional fields
            if (Convert.ToInt64(fisProgramDetails.SubprogramId) != fISMonetoryDetailDto.SubProgramID)
            {
                var message = @$"ValidateFISProgramDetails - Record not validated with 
                                    SubClient Id : {fISMonetoryDetailDto.SubProgramID}";

                _logger.LogInformation("{className}.{methodName}: {message}", className, methodName, message);
                throw new ETLException(ETLExceptionCodes.InValidValue, message);
            }

            return matchingConfigEntry; 
        }

    }
}
