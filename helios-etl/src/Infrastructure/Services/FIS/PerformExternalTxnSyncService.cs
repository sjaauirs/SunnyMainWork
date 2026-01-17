using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SunnyRewards.Helios.Common.Core.Extensions;
using SunnyRewards.Helios.ETL.Common.Constants;
using SunnyRewards.Helios.ETL.Common.CustomException;
using SunnyRewards.Helios.ETL.Core.Domain.Dtos;
using SunnyRewards.Helios.ETL.Core.Domain.Dtos.FIS;
using SunnyRewards.Helios.ETL.Core.Domain.Models;
using SunnyRewards.Helios.ETL.Infrastructure.Repositories.HeliosRepo.Interfaces;
using SunnyRewards.Helios.ETL.Infrastructure.Services.Interfaces.FIS;
using ISession = NHibernate.ISession;

namespace SunnyRewards.Helios.ETL.Infrastructure.Services.FIS
{
    public class PerformExternalTxnSyncService : IPerformExternalTxnSyncService
    {

        private readonly ILogger<PerformExternalTxnSyncService> _logger;
        private readonly ITenantAccountRepo _tenantAccountRepo;
        private readonly IConsumerAccountRepo _consumerAccountRepo;
        private readonly IMonetaryTransactionRepo _monetaryTransactionRepo;
        private readonly IWalletTypeRepo _walletTypeRepo;
        private readonly IWalletRepo _walletRepo;
        private readonly ISession _session;
        private readonly ITenantRepo _tenantRepo;
        private readonly ITransactionRepo _transactionRepo;
        const string className = nameof(PerformExternalTxnSyncService);
        public PerformExternalTxnSyncService(ILogger<PerformExternalTxnSyncService> logger,
           ITenantAccountRepo tenantAccountRepo, IMonetaryTransactionRepo monetaryTransactionRepo, ISession session,
           IConsumerAccountRepo consumerAccountRepo, IWalletTypeRepo walletTypeRepo, IWalletRepo walletRepo, ITenantRepo tenantRepo, ITransactionRepo transactionRepo)

        {
            _logger = logger;
            _tenantAccountRepo = tenantAccountRepo;
            _monetaryTransactionRepo = monetaryTransactionRepo;
            _session = session;
            _consumerAccountRepo = consumerAccountRepo;
            _walletTypeRepo = walletTypeRepo;
            _walletRepo = walletRepo;
            _tenantRepo = tenantRepo;
            _transactionRepo = transactionRepo;
        }
        
        public async Task PerformExternalTxnSync(EtlExecutionContext etlExecutionContext)
        {
            const string methodName = nameof(PerformExternalTxnSync);
            _logger.LogInformation($"{className}.{methodName}: Starting to Perform External Txn Sync...");

            var tenantAccounts = await _tenantAccountRepo
                .FindAsync(x => x.DeleteNbr == 0 && x.TenantConfigJson != null);

            if (tenantAccounts == null || tenantAccounts.Count() == 0)
            {
                _logger.LogWarning($"{className}.{methodName}: No tenant accounts found, Error Code:{StatusCodes.Status404NotFound}");
                throw new ETLException(ETLExceptionCodes.NotFoundInDb, "No Tenant accounts found in DB");
            }

            var tenantAccountDict = tenantAccounts?
                .GroupBy(x => x.TenantCode)?
                .ToDictionary(g => g.Key, g => g.First());

            await ProcessMonetaryTransaction(tenantAccountDict, etlExecutionContext);
        }
        private async Task ProcessMonetaryTransaction(IDictionary<string, ETLTenantAccountModel> tenantAccountDict, EtlExecutionContext etlExecutionContext)
        {
            const string methodName = nameof(ProcessMonetaryTransaction);
            int batchSize = 100;
            int processedCount = 0;
            var minLastTxnId = tenantAccountDict.Values.Min(x => x.LastMonetaryTransactionId);
            var monetaryDetails = await _monetaryTransactionRepo.FindAsync(x => x.MonetaryTransactionId > minLastTxnId && x.DeleteNbr == 0);
            long? maxProcessedMonetaryTxnId = minLastTxnId;

            var erroredMonetary = new List<ETLMonetaryTransactionModel>();
            foreach (var monetaryDetail in monetaryDetails)
            {
                if(Constants.SkipTxnType.Contains(monetaryDetail.TxnTypeName?.Trim() ?? Constants.Unknown) || monetaryDetail.SettleAmount <= 0
                    || monetaryDetail.TransactionCurrencyCode != Constants.skipTransactionCurrencyCode)
                {
                    _logger.LogInformation($"{className}.{methodName}: Skipped the transaction having Transaction UID: {monetaryDetail.TxnUid}");

                    continue;
                }
                // Begin transaction
                using (var transaction = _session.BeginTransaction())
                {
                    try
                    {
                        // Find consumer using Card Number Proxy
                        var consumerAccount = await _consumerAccountRepo.FindOneAsync(x => x.ProxyNumber == monetaryDetail.CardNumberProxy.Trim() && x.DeleteNbr == 0);

                        if (consumerAccount == null)
                        {
                            // Log error and continue to next record
                            _logger.LogError($"{className}.{methodName}: Consumer does not exists for Card Number Proxy: {monetaryDetail.CardNumberProxy}");
                            erroredMonetary.Add(monetaryDetail);
                            continue;
                        }

                        if (!tenantAccountDict.TryGetValue(consumerAccount.TenantCode, out var tenantAccount))
                        {
                            _logger.LogError($"{className}.{methodName}: Tenant Account does not exists for Tenant Code: {consumerAccount.TenantCode}");
                            erroredMonetary.Add(monetaryDetail);
                            continue;
                        }

                        FISTenantConfigDto tenantConfig = JsonConvert.DeserializeObject<FISTenantConfigDto>(tenantAccount.TenantConfigJson);
                        // Find consumer wallet using tenant_config_json and Purse No field
                        var Purse = tenantConfig.PurseConfig.Purses.Where(x => x.PurseNumber == monetaryDetail.PurseNo).FirstOrDefault();

                        if (Purse == null)
                        {
                            // Log error and continue to next record
                            _logger.LogError(($"{className}.{methodName}: Wallet not found for Purse No: {monetaryDetail.PurseNo}"));
                            erroredMonetary.Add(monetaryDetail);
                            continue;
                        }
                        var walletType = await _walletTypeRepo.FindOneAsync(x => x.WalletTypeCode == Purse.PurseWalletType && x.IsExternalSync && x.DeleteNbr == 0);
                        if (walletType == null)
                        {
                            _logger.LogError($"{className}.{methodName}: Purse Wallet Type does not exists, PurseWalletType: {Purse.PurseWalletType}");
                            erroredMonetary.Add(monetaryDetail);
                            continue;
                        }
                        var consumerWallet = await _walletRepo.GetWalletByConsumerAndWalletTypeForTransactionSync(tenantAccount.TenantCode, consumerAccount.ConsumerCode, walletType.WalletTypeId);
                        if (consumerWallet == null)
                        {
                            // Log error and continue to next record
                            _logger.LogError($"{className}.{methodName}: Consumer Wallet does not exists for Consumer Code: {consumerAccount.ConsumerCode}");
                            erroredMonetary.Add(monetaryDetail);
                            continue;
                        }

                        var maxMasterWalletTransactionId = await _transactionRepo.GetMaxTransactionIdByWallet(consumerWallet.WalletId);
                        var lastMasterWalletTransaction = await _transactionRepo.FindOneAsync(t => t.TransactionId == maxMasterWalletTransactionId && t.DeleteNbr == 0);

                        var transactionDetailModel = new ETLTransactionDetailModel()
                        {
                            TransactionDetailType = GetMappingValue(monetaryDetail.TxnTypeName?.Trim()?? Constants.Unknown),
                            ConsumerCode = consumerAccount.ConsumerCode?.Trim(),
                            TaskRewardCode = null,
                            Notes = null,
                            RedemptionRef = monetaryDetail.ReferenceNumber?.Trim(),
                            RedemptionItemDescription = monetaryDetail.MerchantName?.Trim(),
                            CreateUser = Constants.CreateUserAsETL?.Trim(),
                            CreateTs = monetaryDetail.TxnLocDateTime,
                            DeleteNbr = 0,
                            RewardDescription = null
                        };
                        var transactionDetailModelResponse = await _session.SaveAsync(transactionDetailModel);
                        transactionDetailModel.TransactionDetailId = Convert.ToInt64(transactionDetailModelResponse);

                        var transactionModel = new ETLTransactionModel()
                        {
                            WalletId = consumerWallet.WalletId,
                            TransactionCode = "txn-" + Guid.NewGuid().ToString("N"),
                            TransactionType = monetaryDetail.TxnSign == -1 ? Constants.Subtract : Constants.Addition,
                            PreviousBalance = 0,
                            TransactionAmount = Convert.ToDouble(monetaryDetail.SettleAmount),
                            Balance = 0,
                            PrevWalletTxnCode = consumerWallet.WalletId + ":" + (lastMasterWalletTransaction != null ?
                        lastMasterWalletTransaction.TransactionCode : "init"),
                            CreateUser = Constants.CreateUserAsETL,
                            CreateTs = monetaryDetail.TxnLocDateTime,
                            DeleteNbr = 0,
                            TransactionDetailId = transactionDetailModel.TransactionDetailId
                        };
                        await _session.SaveAsync(transactionModel);

                        transaction.Commit();
                        maxProcessedMonetaryTxnId = monetaryDetail.MonetaryTransactionId;
                        processedCount++;
                        if (processedCount % batchSize == 0)
                        {
                            await UpdateAllTenantAccountsLastTxn(
                                tenantAccountDict,
                                maxProcessedMonetaryTxnId);

                            _logger.LogInformation(
                                "{className}.{methodName}: Batch checkpoint updated at MonetaryTransactionId {txnId}",
                                className, methodName, maxProcessedMonetaryTxnId);
                        }

                        _logger.LogInformation($"{className}.{methodName}: Successfully processed monetary transaction for Consumer:{consumerAccount.ConsumerCode}, Proxy Number: {monetaryDetail.CardNumberProxy}");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"{className}.{methodName}: Error processing transaction: {ex.Message}");
                        _logger.LogError(ex, $"{className}.{methodName}: Error for monetary transaction record: {monetaryDetail.ToJson()}");
                        transaction.Rollback();
                        _session.Clear();
                        erroredMonetary.Add(monetaryDetail);
                    }

                }
            }

            await UpdateAllTenantAccountsLastTxn(tenantAccountDict, maxProcessedMonetaryTxnId);

            etlExecutionContext.JobHistoryStatus = erroredMonetary.Count == 0
                ? Constants.JOB_HISTORY_SUCCESS_STATUS
                : (erroredMonetary.Count == monetaryDetails.Count()
                    ? Constants.JOB_HISTORY_FAILURE_STATUS
                    : Constants.JOB_HISTORY_PARTIAL_SUCCESS_STATUS);
            etlExecutionContext.JobHistoryErrorLog = erroredMonetary?.Count != 0
                ? $"Errored monetary details list: {erroredMonetary.ToString()}"
                : string.Empty;
            _logger.LogInformation($"{className}.{methodName}: Completed processing monetary transactions. Success: {monetaryDetails.Count() - erroredMonetary.Count}, Failed: {erroredMonetary.Count}");

        }
        private async Task UpdateTenantaccount(string tenantCode, int lastTransactionId)
        {
            const string methodName = nameof(UpdateTenantaccount);
            try
            {
                var tenantAccountModel = _session.Query<ETLTenantAccountModel>().Where(x => x.TenantCode == tenantCode && x.DeleteNbr == 0).FirstOrDefault();
                if (tenantAccountModel != null)
                {
                    tenantAccountModel.LastMonetaryTransactionId = lastTransactionId;
                    await _session.UpdateAsync(tenantAccountModel);
                    _logger.LogInformation("{className}.{methodName}: Last Monetary Transaction Id updated", className, methodName);
                }
                else
                {
                    _logger.LogInformation("{className}.{methodName}: TenantAccount not found, Error Code:{errorCode}", className, methodName, StatusCodes.Status404NotFound);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{className}.{methodName}: ERROR occurred while updating tenant account, Error- {message}", className, methodName, ex.Message);

            }
        }

        private async Task UpdateAllTenantAccountsLastTxn(IDictionary<string, ETLTenantAccountModel> tenantAccountDict, long? lastProcessedTxnId)
        {
            const string methodName = nameof(UpdateAllTenantAccountsLastTxn);

            using (var tx = _session.BeginTransaction())
            {
                try
                {
                    foreach (var tenantAccount in tenantAccountDict.Values)
                    {
                        var tenantIds = tenantAccountDict.Values
                                .Select(t => t.TenantAccountId)
                                .ToList();

                        await _session.CreateQuery(@"
                                update ETLTenantAccountModel
                                set LastMonetaryTransactionId = :lastTxnId
                                where TenantAccountId in (:ids)")
                            .SetParameter("lastTxnId", (int?)lastProcessedTxnId)
                            .SetParameterList("ids", tenantIds)
                            .ExecuteUpdateAsync();
                    }

                    await tx.CommitAsync();

                    _logger.LogInformation(
                        "{className}.{methodName}: Updated LastMonetaryTransactionId={lastId} for {count} tenants",
                        className, methodName, lastProcessedTxnId, tenantAccountDict.Count);
                }
                catch (Exception ex)
                {
                    await tx.RollbackAsync();
                    _logger.LogError(ex, "{className}.{methodName}: Failed to update tenant accounts");
                    throw;
                }
            }
        }


        public string GetMappingValue(string key)
        {
            key = key.Contains(Constants.Purchase, StringComparison.OrdinalIgnoreCase) ? Constants.Purchase : key;

            if (Constants.TxnTypeMapping.ContainsKey(key.Trim()))
            {
                return Constants.TxnTypeMapping[key.Trim()];
            }
            else
            {
                _logger.LogInformation("{className}.GetMappingValue: TxnTypeNameMapping not found {txntype}", className, key);
                return Constants.Unknown;
            }
        }
    }
}
