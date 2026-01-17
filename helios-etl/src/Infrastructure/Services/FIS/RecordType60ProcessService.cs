using Microsoft.Extensions.Logging;
using SunnyRewards.Helios.ETL.Common.Constants;
using SunnyRewards.Helios.ETL.Core.Domain.Constants;
using SunnyRewards.Helios.ETL.Core.Domain.Constants.FIS;
using SunnyRewards.Helios.ETL.Core.Domain.Dtos;
using SunnyRewards.Helios.ETL.Core.Domain.Dtos.FIS;
using SunnyRewards.Helios.ETL.Infrastructure.Logs;
using SunnyRewards.Helios.ETL.Infrastructure.Logs.Interface;
using SunnyRewards.Helios.ETL.Infrastructure.Repositories.HeliosRepo.Interfaces;
using SunnyRewards.Helios.ETL.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.ETL.Infrastructure.Services.Interfaces.FIS;
using SunnyRewards.Helios.Common.Core.Extensions;
using SunnyRewards.Helios.ETL.Common.Helpers.Interfaces;
using SunnyRewards.Helios.ETL.Infrastructure.Helpers;
using log4net.Core;
using Microsoft.AspNetCore.Http;

namespace SunnyRewards.Helios.ETL.Infrastructure.Services.FIS
{
    public class RecordType60ProcessService : IRecordType60ProcessService
    {
        private readonly IFlatFileReader _flatFileReader;
        private static int CONSUMER_ACCOUNT_CREATE_CHUNK_SIZE = 100;
        private readonly IS3FISFileLogger _s3FISFileLogger;
        private readonly IRedemptionService _redemptionService;
        private readonly ILogger<RecordType60ProcessService> _logger;
        private readonly IFundTransferService _fundTransferService;
        private readonly ITransactionRepo _transactionRepo;
        private readonly IConsumerWalletRepo _consumerWalletRepo;
        private readonly IConsumerAccountRepo _consumerAccountRepo;
        const string className = nameof(RecordType60ProcessService);

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="flatFileReader"></param>
        /// <param name="logger"></param>
        /// <param name="s3FISFileLogger"></param>
        public RecordType60ProcessService(IFlatFileReader flatFileReader, IS3FISFileLogger s3FISFileLogger,
            IRedemptionService redemptionService, ILogger<RecordType60ProcessService> logger,
            IFundTransferService fundTransferService, ITransactionRepo transactionRepo, IConsumerWalletRepo consumerWalletRepo, IConsumerAccountRepo consumerAccountRepo)
        {
            _flatFileReader = flatFileReader;
            _s3FISFileLogger = s3FISFileLogger;
            _redemptionService = redemptionService;
            _logger = logger;
            _fundTransferService = fundTransferService;
            _transactionRepo = transactionRepo;
            _consumerWalletRepo = consumerWalletRepo;
            _consumerAccountRepo = consumerAccountRepo;
        }

        /// <summary>
        /// Process60RecordFile
        /// </summary>
        /// <param name="line"></param>
        /// <param name="modelObject"></param>
        /// <param name="etlExecutionContext"></param>
        /// <returns></returns>
        public async Task Process60RecordFile(string line, FISCardAdditionalDisbursementRecordDto modelObject, EtlExecutionContext etlExecutionContext)
        {
            var fisCardDisbursementData = _flatFileReader.ReadFlatFileRecord((FISCardAdditionalDisbursementRecordDto)modelObject,
                                        line, FISCardAdditionalDisbursementRecordDto.FieldConfigurationMap);
            if (string.IsNullOrEmpty(etlExecutionContext.TenantCode))
            {
                if (string.IsNullOrWhiteSpace(fisCardDisbursementData.PANProxyClientUniqueID))
                {
                    _logger.LogWarning("Skipping record as ProxyClientUniqueID is null or empty.");
                    return;
                }

                // Find consumer accounts based on proxy
                var consumerAccountRecords = await _consumerAccountRepo.FindAsync(x => x.ProxyNumber == fisCardDisbursementData.PANProxyClientUniqueID && x.DeleteNbr == 0);

                if (consumerAccountRecords == null || !consumerAccountRecords.Any())
                {
                    _logger.LogWarning($"No consumer account found for ProxyNumber: {fisCardDisbursementData.PANProxyClientUniqueID}");
                    return;
                }

                // Extract distinct tenant codes
                etlExecutionContext.TenantCode = consumerAccountRecords.FirstOrDefault()!.TenantCode!;
            }
            

            if (fisCardDisbursementData != null && fisCardDisbursementData.CardRecordStatusCode >= FISBatchConstants.CARD_RECORD_MIN_ERROR_STATUS_CODE
                && fisCardDisbursementData.CardRecordStatusCode <= FISBatchConstants.CARD_RECORD_MAX_ERROR_STATUS_CODE)
            {
                var errorMessage = $"Error processing 60 record type file: lastName = {fisCardDisbursementData.LastName}, person_id = {fisCardDisbursementData.SSN}, " +
                    $"clientReferenceNumber = {fisCardDisbursementData.ClientReferenceNumber}, purse:{fisCardDisbursementData.Purse}, " +
                    $"PANProxyClientUniqueID: {fisCardDisbursementData.PANProxyClientUniqueID}" +
                    $"status = {fisCardDisbursementData.CardRecordStatusCode}, error message = {fisCardDisbursementData.ProcessingMessage}";

                await _s3FISFileLogger.AddErrorLogs(new S3FISLogContext
                {
                    LogFileName = etlExecutionContext.FISRecordFileName,
                    Message = errorMessage,
                    TenantCode = etlExecutionContext?.TenantCode,
                    throwEtlError = false
                });
                var redumptionError =  await RevertRedemption(fisCardDisbursementData, etlExecutionContext?.TenantCode);
                var mergedMessage = String.IsNullOrEmpty(redumptionError)? errorMessage : errorMessage + " RedumtionError : "+ redumptionError;
                throw new EtlJobException(mergedMessage);
            }
            else
            {
                _redemptionService.UpdateRedemptionStatus(fisCardDisbursementData.ClientReferenceNumber, Constants.RedemptionStatusCompleted);
            }
        }

        private async Task<string> RevertRedemption(FISCardAdditionalDisbursementRecordDto fISCardDisbursementRecord, string? tenantCode)
        {
            string redumptionError = string.Empty;
            const string methodName = nameof(RevertRedemption);
            var redemption = _redemptionService.GetRedemptionWithRedemptionRef(fISCardDisbursementRecord.ClientReferenceNumber);
            if (redemption == null)
            {
                redumptionError = $"{methodName}: Redemption not found with redemption red. Redemption Ref: {fISCardDisbursementRecord.ClientReferenceNumber}";
                _logger.LogWarning($"{className}.{methodName}: Redemption not found with redemption red. Redemption Ref: {fISCardDisbursementRecord.ClientReferenceNumber}, Error Code:{StatusCodes.Status404NotFound}");

                return redumptionError;

            }
            var addTransaction = await _transactionRepo.FindOneAsync(x => x.TransactionId == redemption.AddTransactionId && x.DeleteNbr == 0);
            if (addTransaction == null)
            {
                redumptionError = $"{methodName}: Addition Transaction not found. Transaction ID: {redemption.AddTransactionId}";
                _logger.LogWarning($"{className}.{methodName}: Addition Transaction not found. Transaction ID: {redemption.AddTransactionId}, Error Code:{StatusCodes.Status404NotFound}");
                return redumptionError;

            }
            var subTransaction = await _transactionRepo.FindOneAsync(x => x.TransactionId == redemption.SubTransactionId && x.DeleteNbr == 0);
            if (subTransaction == null)
            {
                redumptionError = $"{methodName}: Subtraction Transaction not found. Transaction ID: {redemption.SubTransactionId}";
                _logger.LogWarning($"{className}.{methodName}: Subtraction Transaction not found. Transaction ID: {redemption.SubTransactionId}, Error Code:{StatusCodes.Status404NotFound}");
                return redumptionError;

            }
            var consumerWallet = await _consumerWalletRepo.FindOneAsync(x => x.WalletId == subTransaction.WalletId && x.DeleteNbr == 0);

            var transactionCode = "txn-" + Guid.NewGuid().ToString("N");
            var revertRequest = new RevertRedemptionRequestDto
            {
                MasterWalletId = addTransaction.WalletId,
                ConsumerWalletId = subTransaction.WalletId,
                RedemptionRef = fISCardDisbursementRecord.ClientReferenceNumber,
                TransactionAmount = addTransaction.TransactionAmount.GetValueOrDefault(),
                TransactionDetailType = BenefitsConstants.RevertRedemptionTransactionDetailType,
                Redemption = redemption,
                ConsumerCode = consumerWallet.ConsumerCode,
                NewTransactionCode = transactionCode,
                TenantCode = tenantCode
            };

            var (isRedemptionExecuted, msg) = await _fundTransferService.ExecuteRevertRedemptionTransactionAsync(revertRequest);

            if (!isRedemptionExecuted)
            {
                redumptionError =$"GenerateDisbursementRecords: Redemption transaction failed,  Redemption Request: {revertRequest.ToJson()}, Detail : {msg} ";
                _logger.LogInformation($"{className}.{methodName}: Redemption transaction failed,  Redemption Request: {revertRequest.ToJson()}");
                return redumptionError ;

            }
            return redumptionError;
        }

    }
}
