using Microsoft.Extensions.Logging;
using SunnyRewards.Helios.ETL.Common.Constants;
using SunnyRewards.Helios.ETL.Core.Domain.Dtos;
using SunnyRewards.Helios.ETL.Core.Domain.Dtos.FIS;
using SunnyRewards.Helios.ETL.Core.Domain.Models;
using SunnyRewards.Helios.ETL.Infrastructure.Repositories.HeliosRepo.Interfaces;
using SunnyRewards.Helios.ETL.Infrastructure.Services.Interfaces.FIS;

namespace SunnyRewards.Helios.ETL.Infrastructure.Services.FIS
{
    public class FundTransferService : IFundTransferService
    {
        private readonly ILogger<FundTransferService> _logger;
        private readonly ITransactionRepo _transactionRepo;
        private readonly IWalletRepo _walletRepo;
        private readonly NHibernate.ISession _session;
        private readonly IWalletTypeRepo _walletTypeRepo;
        private readonly IConsumerWalletRepo _consumerWalletRepo;
        const string className = nameof(FundTransferService);

        public FundTransferService(ILogger<FundTransferService> logger, NHibernate.ISession session,
            ITransactionRepo transactionRepo, IWalletRepo walletRepo, IWalletTypeRepo walletTypeRepo, IConsumerWalletRepo consumerWalletRepo)
        {
            _logger = logger;
            _session = session;
            _transactionRepo = transactionRepo;
            _walletRepo = walletRepo;
            _walletTypeRepo = walletTypeRepo;
            _consumerWalletRepo = consumerWalletRepo;
        }
        public async Task ExecuteFundTransferAsync(FISFundTransferRequestDto fundTransferRequest, bool? isFundingRuleExecution = true)
        {
            const string methodName = nameof(ExecuteFundTransferAsync);
            var masterWallet = await _walletRepo.FindOneAsync(x => x.WalletId == fundTransferRequest.MasterWallet.WalletId);

            using (var transaction = _session.BeginTransaction())
            {
                var transactionTs = DateTime.UtcNow;

                try
                {

                    var maxMasterWalletTransactionId = await _transactionRepo.GetMaxTransactionIdByWallet(masterWallet?.WalletId ?? 0);
                    var lastMasterWalletTransaction = await _transactionRepo.FindOneAsync(t => t.TransactionId == maxMasterWalletTransactionId);

                    var maxConsumerWalletTransactionId = await _transactionRepo.GetMaxTransactionIdByWallet(fundTransferRequest.ConsumerWallet?.WalletId ?? 0);

                    var lastConsumerWalletTransaction = await _transactionRepo.FindOneAsync(t => t.TransactionId == maxConsumerWalletTransactionId);

                    var transactionDetail = new ETLTransactionDetailModel()
                    {
                        TransactionDetailType = fundTransferRequest.TransactionDetailType,
                        ConsumerCode = fundTransferRequest.ConsumerCode,
                        RewardDescription = fundTransferRequest.RewardDescription,
                        Notes = null,
                        RedemptionRef = fundTransferRequest.RedemptionRef,
                        RedemptionItemDescription = null,
                        CreateTs = transactionTs,
                        CreateUser = Constants.CreateUser,
                        UpdateTs = DateTime.UtcNow,
                        UpdateUser = Constants.CreateUser,
                        DeleteNbr = 0
                    };

                    var transactionDetailResponse = await _session.SaveAsync(transactionDetail);
                    transactionDetail.TransactionDetailId = Convert.ToInt64(transactionDetailResponse);

                    string newTxnCode = "txn-" + Guid.NewGuid().ToString("N");

                    var fundAmount = Math.Max(fundTransferRequest.Amount, 0);

                    var masterWalletBalance = masterWallet?.Balance - fundAmount;
                    var subTransaction = new ETLTransactionModel()
                    {
                        WalletId = masterWallet?.WalletId ?? 0,
                        TransactionCode = newTxnCode,
                        TransactionType = Constants.Subtract,
                        PreviousBalance = masterWallet?.Balance,
                        TransactionAmount = fundAmount,
                        Balance = masterWalletBalance,
                        PrevWalletTxnCode = masterWallet?.WalletId + ":" + (lastMasterWalletTransaction != null ?
                        lastMasterWalletTransaction.TransactionCode : "init"),
                        CreateTs = transactionTs,
                        CreateUser = Constants.CreateUser,
                        UpdateTs = DateTime.UtcNow,
                        UpdateUser = Constants.CreateUser,
                        DeleteNbr = 0,
                        TransactionDetailId = transactionDetail.TransactionDetailId
                    };

                    await _session.SaveAsync(subTransaction);

                    var consumerWalletBalance = fundTransferRequest.ConsumerWallet?.Balance + fundAmount;
                    var totalEarned = fundTransferRequest.ConsumerWallet?.TotalEarned + fundAmount;

                    var addTransaction = new ETLTransactionModel()
                    {
                        WalletId = fundTransferRequest.ConsumerWallet?.WalletId ?? 0,
                        TransactionCode = newTxnCode,
                        TransactionType = Constants.Addition,
                        PreviousBalance = fundTransferRequest.ConsumerWallet?.Balance,
                        TransactionAmount = fundAmount,
                        Balance = consumerWalletBalance,
                        PrevWalletTxnCode = fundTransferRequest.ConsumerWallet?.WalletId + ":" + (lastConsumerWalletTransaction != null ?
                        lastConsumerWalletTransaction.TransactionCode : "init"),
                        CreateTs = transactionTs,
                        CreateUser = Constants.CreateUser,
                        UpdateTs = DateTime.UtcNow,
                        UpdateUser = Constants.CreateUser,
                        DeleteNbr = 0,
                        TransactionDetailId = transactionDetail.TransactionDetailId
                    };
                    if (fundAmount > 0)
                    {
                        await _session.SaveAsync(addTransaction);
                    }

                    int rec = _walletRepo.UpdateMasterWalletBalance(transactionTs, masterWalletBalance, masterWallet?.WalletId ?? 0);

                    if (rec == 0)
                    {
                        _logger.LogError("{className}.{methodName}: Could not update master wallet: WalletId: {walletId}", className, methodName,
                            masterWallet?.WalletId);
                        await transaction.RollbackAsync();
                        _session.Clear();
                        return;
                    }
                    else
                    {
                        _logger.LogInformation("{className}.{methodName}: Updated master wallet balance, WalletId: {walletId}, Balance: {balance}, Tenant: {tenant}", className, methodName,
                            masterWallet.WalletId, masterWalletBalance, fundTransferRequest.TenantCode);

                    }

                    int recc = _walletRepo.UpdateConsumerWalletBalance(transactionTs, consumerWalletBalance, totalEarned, fundTransferRequest.ConsumerWallet?.WalletId ?? 0);
                    if (recc == 0)

                    {
                        _logger.LogError("{className}.{methodName}: Could not update consumer wallet: WalletId: {walletId}", className, methodName,
                            fundTransferRequest.ConsumerWallet?.WalletId);

                        await transaction.RollbackAsync();
                        _session.Clear();
                        return;
                    }
                    else
                    {
                        _logger.LogInformation("{className}.{methodName}: Updated consumer wallet balance, WalletId: {walletId}, Balance: {balance}, ConsumerCode: {consumerCode}", className, methodName,
                            fundTransferRequest.ConsumerWallet?.WalletId, consumerWalletBalance, fundTransferRequest.ConsumerCode);
                    }

                    if (isFundingRuleExecution == true)
                    {
                        var fundingHistoryModel = new ETLFundingHistoryModel
                        {
                            TenantCode = fundTransferRequest.TenantCode,
                            ConsumerCode = fundTransferRequest.ConsumerCode,
                            FundRuleNumber = fundTransferRequest.RuleNumber,
                            FundTs = DateTime.UtcNow,
                            CreateTs = DateTime.UtcNow,
                            CreateUser = Constants.CreateUser,
                            DeleteNbr = 0
                        };

                        await _session.SaveAsync(fundingHistoryModel);
                    }
                    _logger.LogInformation($"{className}.{methodName}: Created funding history record, TenantCode:{fundTransferRequest.TenantCode}, ConsumerCode: {fundTransferRequest.ConsumerCode}");
                    await transaction.CommitAsync();
                    _session.Clear();
                    _logger.LogInformation($"{className}.{methodName}: Fund transfer completed successfully. TenantCode: {fundTransferRequest.TenantCode}, ConsumerCode: {fundTransferRequest.ConsumerCode}, RuleNumber: {fundTransferRequest.RuleNumber}, Amount: {fundTransferRequest.Amount}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "{className}.{methodName}: Error occurred during fund transfer. TenantCode: {tenantCode}, ConsumerCode: {consumerCode}, RuleNumber: {ruleNumber}", className, methodName, fundTransferRequest.TenantCode, fundTransferRequest.ConsumerCode, fundTransferRequest.RuleNumber);
                    await transaction.RollbackAsync();
                }
            }

        }

        public async Task<bool> ExecuteRedemptionTransactionAsync(RedemptionRequestDto redemptionRequest)
        {
            const string methodName = nameof(ExecuteRedemptionTransactionAsync);
            _logger.LogInformation("{className}.{methodName}: Redeem Started with ConsumerCode : {ConsumerCode}", className, methodName, redemptionRequest.ConsumerCode);

            var redemptionWalletType = await _walletTypeRepo.FindOneAsync(w => w.WalletTypeCode == redemptionRequest.RedemptionWalletTypeCode);
            var redemptionWallet = await _walletRepo.FindOneAsync(x => x.WalletTypeId == redemptionWalletType.WalletTypeId &&
            x.TenantCode == redemptionRequest.TenantCode && x.WalletId == redemptionRequest.MasterRedemptionWalletId);

            var maxRedemptionWalletTransactionId = await _transactionRepo.GetMaxTransactionIdByWallet(redemptionWallet.WalletId);
            var lastRedemptionWalletTransaction = await _transactionRepo.FindOneAsync(t => t.TransactionId == maxRedemptionWalletTransactionId);

            var maxConsumerWalletTransactionId = await _transactionRepo.GetMaxTransactionIdByWallet(redemptionRequest.ConsumerWallet?.WalletId ?? 0);

            var lastConsumerWalletTransaction = await _transactionRepo.FindOneAsync(t => t.TransactionId == maxConsumerWalletTransactionId);


            using (var transaction = _session.BeginTransaction())
            {
                var transactionTs = DateTime.UtcNow;

                try
                {
                    var transactionDetail = new ETLTransactionDetailModel
                    {
                        TransactionDetailType = redemptionRequest.TransactionDetailType,
                        ConsumerCode = redemptionRequest.ConsumerCode,
                        TaskRewardCode = null,
                        Notes = redemptionRequest.Notes,
                        RedemptionRef = redemptionRequest.RedemptionRef,
                        RedemptionItemDescription = redemptionRequest.RedemptionItemDescription,
                        CreateTs = transactionTs,
                        CreateUser = Constants.CreateUser,
                        DeleteNbr = 0
                    };
                    var transactionDetailResponse = await _session.SaveAsync(transactionDetail);
                    transactionDetail.TransactionDetailId = Convert.ToInt64(transactionDetailResponse);

                    var consumerWalletBalance = redemptionRequest.ConsumerWallet?.Balance - redemptionRequest.RedemptionAmount;

                    var subTransaction = new ETLTransactionModel
                    {
                        WalletId = redemptionRequest.ConsumerWallet?.WalletId ?? 0,
                        TransactionCode = redemptionRequest.NewTransactionCode,
                        TransactionType = Constants.Subtract,
                        PreviousBalance = redemptionRequest.ConsumerWallet?.Balance ?? 0,
                        TransactionAmount = redemptionRequest.RedemptionAmount,
                        Balance = consumerWalletBalance,
                        PrevWalletTxnCode = redemptionRequest.ConsumerWallet?.WalletId + ":" + (lastConsumerWalletTransaction != null ?
                            lastConsumerWalletTransaction.TransactionCode : "init"),
                        CreateTs = transactionTs,
                        CreateUser = Constants.CreateUser,
                        UpdateTs = DateTime.UtcNow,
                        UpdateUser = Constants.CreateUser,
                        DeleteNbr = 0,
                        TransactionDetailId = transactionDetail.TransactionDetailId
                    };
                    var subTransactionResponse = await _session.SaveAsync(subTransaction);
                    subTransaction.TransactionId = Convert.ToInt64(subTransactionResponse);

                    var redemptionWalletBalance = redemptionWallet.Balance + redemptionRequest.RedemptionAmount;
                    var addTransaction = new ETLTransactionModel
                    {
                        WalletId = redemptionWallet.WalletId,
                        TransactionCode = redemptionRequest.NewTransactionCode,
                        TransactionType = Constants.Addition,
                        PreviousBalance = redemptionWallet.Balance,
                        TransactionAmount = redemptionRequest.RedemptionAmount,
                        Balance = redemptionWalletBalance,
                        PrevWalletTxnCode = redemptionWallet.WalletId + ":" + (lastRedemptionWalletTransaction != null ?
                            lastRedemptionWalletTransaction.TransactionCode : "init"),
                        CreateTs = transactionTs,
                        CreateUser = Constants.CreateUser,
                        UpdateTs = DateTime.UtcNow,
                        UpdateUser = Constants.CreateUser,
                        DeleteNbr = 0,
                        TransactionDetailId = transactionDetail.TransactionDetailId
                    };
                    var addTransactionResponse = await _session.SaveAsync(addTransaction);
                    addTransaction.TransactionId = Convert.ToInt64(addTransactionResponse);

                    var redemption = new ETLRedemptionModel
                    {
                        SubTransactionId = subTransaction.TransactionId,
                        AddTransactionId = addTransaction.TransactionId,
                        RedemptionStatus = Constants.InProgress,
                        RedemptionRef = redemptionRequest.RedemptionRef,
                        RedemptionStartTs = transactionTs,
                        CreateTs = transactionTs,
                        CreateUser = Constants.CreateUser,
                        UpdateTs = DateTime.UtcNow,
                        UpdateUser = Constants.CreateUser,
                        DeleteNbr = 0,
                        RedemptionItemDescription = redemptionRequest.RedemptionItemDescription,
                        RedemptionItemData = redemptionRequest.RedemptionItemData
                    };
                    await _session.SaveAsync(redemption);

                    int rec = _walletRepo.UpdateMasterWalletBalance(transactionTs, redemptionWalletBalance, redemptionWallet.WalletId);
                    if (rec == 0)
                    {
                        _logger.LogError("{className}.{methodName}: Could not update master wallet: WalletId: {walletId}", className, methodName,
                            redemptionWallet.WalletId);
                        await transaction.RollbackAsync();
                        _session.Clear();
                        return false;
                    }
                    else
                    {
                        _logger.LogInformation("{className}.{methodName}: Updated master wallet balance, WalletId: {walletId}, Balance: {balance}, Tenant: {tenant}", className, methodName,
                            redemptionWallet?.WalletId, redemptionWalletBalance, redemptionRequest.TenantCode);
                    }

                    int recc = _walletRepo.UpdateConsumerWalletBalance(transactionTs, consumerWalletBalance, redemptionRequest.ConsumerWallet?.TotalEarned, redemptionRequest.ConsumerWallet?.WalletId ?? 0);
                    if (recc == 0)
                    {
                        _logger.LogError("{className}.{methodName}: Could not update consumer wallet, WalletId: {walletId}", className, methodName,
                            redemptionRequest.ConsumerWallet?.WalletId);

                        await transaction.RollbackAsync();
                        _session.Clear();
                        return false;
                    }
                    else
                    {
                        _logger.LogInformation("{className}.{methodName}: Updated consumer wallet balance, WalletId: {walletId}, Balance: {balance}, ConsumerCode: {consumerCode}", className, methodName,
                            redemptionRequest.ConsumerWallet?.WalletId, consumerWalletBalance, redemptionRequest.ConsumerCode);
                    }
                    await transaction.CommitAsync();
                    _session.Clear();
                    _logger.LogInformation($"{className}.{methodName}: Redemption transaction completed successfully. TenantCode: {redemptionRequest.TenantCode}, ConsumerCode: {redemptionRequest.ConsumerCode},  RedemptionAmount: {redemptionRequest.RedemptionAmount}");
                    return true;

                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "{className}.{methodName}: Error occurred during redemption transaction. TenantCode: {tenantCode}, ConsumerCode: {consumerCode}, Error Msg:{msg}", className, methodName, redemptionRequest.TenantCode, redemptionRequest.ConsumerCode, ex.Message);
                    transaction.Rollback();
                    _session.Clear();
                    return false;
                }
            }

        }

        public async Task<(bool, string)> ExecuteRevertRedemptionTransactionAsync(RevertRedemptionRequestDto revertRedemptionRequest)
        {
            var msg = String.Empty;
            const string methodName = nameof(ExecuteRevertRedemptionTransactionAsync);
            var redemptionWallet = await _walletRepo.FindOneAsync(x => x.WalletId == revertRedemptionRequest.MasterWalletId && x.DeleteNbr == 0);

            var maxRedemptionWalletTransactionId = await _transactionRepo.GetMaxTransactionIdByWallet(redemptionWallet.WalletId);
            var lastRedemptionWalletTransaction = await _transactionRepo.FindOneAsync(t => t.TransactionId == maxRedemptionWalletTransactionId);

            var maxConsumerWalletTransactionId = await _transactionRepo.GetMaxTransactionIdByWallet(revertRedemptionRequest.ConsumerWalletId);

            var lastConsumerWalletTransaction = await _transactionRepo.FindOneAsync(t => t.TransactionId == maxConsumerWalletTransactionId);
            var consumerWallet = await _walletRepo.FindOneAsync(x => x.WalletId == revertRedemptionRequest.ConsumerWalletId && x.DeleteNbr == 0);
            var consumer = await _consumerWalletRepo.FindOneAsync(x => x.WalletId == revertRedemptionRequest.ConsumerWalletId && x.DeleteNbr == 0);
            revertRedemptionRequest.ConsumerCode = consumer.ConsumerCode;

            using (var transaction = _session.BeginTransaction())
            {
                var transactionTs = DateTime.UtcNow;

                try
                {
                    var transactionDetail = new ETLTransactionDetailModel
                    {
                        TransactionDetailType = revertRedemptionRequest.TransactionDetailType,
                        ConsumerCode = revertRedemptionRequest.ConsumerCode,
                        TaskRewardCode = null,
                        Notes = revertRedemptionRequest.Notes,
                        RedemptionRef = revertRedemptionRequest.RedemptionRef,
                        RedemptionItemDescription = revertRedemptionRequest.RedemptionItemDescription,
                        CreateTs = transactionTs,
                        CreateUser = Constants.CreateUser,
                        DeleteNbr = 0
                    };
                    var transactionDetailResponse = await _session.SaveAsync(transactionDetail);
                    transactionDetail.TransactionDetailId = Convert.ToInt64(transactionDetailResponse);

                    var consumerWalletBalance = consumerWallet?.Balance + revertRedemptionRequest.TransactionAmount;

                    var revertAddTransaction = new ETLTransactionModel
                    {
                        WalletId = revertRedemptionRequest.ConsumerWalletId,
                        TransactionCode = revertRedemptionRequest.NewTransactionCode,
                        TransactionType = Constants.Addition,
                        PreviousBalance = consumerWallet?.Balance ?? 0,
                        TransactionAmount = revertRedemptionRequest.TransactionAmount,
                        Balance = consumerWalletBalance,
                        PrevWalletTxnCode = consumerWallet?.WalletId + ":" + (lastConsumerWalletTransaction != null ?
                            lastConsumerWalletTransaction.TransactionCode : "init"),
                        CreateTs = transactionTs,
                        CreateUser = Constants.CreateUser,
                        UpdateTs = DateTime.UtcNow,
                        UpdateUser = Constants.CreateUser,
                        DeleteNbr = 0,
                        TransactionDetailId = transactionDetail.TransactionDetailId
                    };
                    var revertAddTransactionResponse = await _session.SaveAsync(revertAddTransaction);
                    revertAddTransaction.TransactionId = Convert.ToInt64(revertAddTransactionResponse);

                    var redemptionWalletBalance = redemptionWallet.Balance - revertRedemptionRequest.TransactionAmount;
                    var revertSubTransaction = new ETLTransactionModel
                    {
                        WalletId = redemptionWallet.WalletId,
                        TransactionCode = revertRedemptionRequest.NewTransactionCode,
                        TransactionType = Constants.Subtract,
                        PreviousBalance = redemptionWallet.Balance,
                        TransactionAmount = revertRedemptionRequest.TransactionAmount,
                        Balance = redemptionWalletBalance,
                        PrevWalletTxnCode = redemptionWallet.WalletId + ":" + (lastRedemptionWalletTransaction != null ?
                            lastRedemptionWalletTransaction.TransactionCode : "init"),
                        CreateTs = transactionTs,
                        CreateUser = Constants.CreateUser,
                        UpdateTs = DateTime.UtcNow,
                        UpdateUser = Constants.CreateUser,
                        DeleteNbr = 0,
                        TransactionDetailId = transactionDetail.TransactionDetailId
                    };
                    var revertSubTransactionResponse = await _session.SaveAsync(revertSubTransaction);
                    revertSubTransaction.TransactionId = Convert.ToInt64(revertSubTransactionResponse);

                    var redemption = revertRedemptionRequest.Redemption;
                    redemption.RevertAddTransactionId = revertAddTransaction.TransactionId;
                    redemption.RevertSubTransactionId = revertSubTransaction.TransactionId;
                    redemption.RedemptionStatus = Constants.Reverted;
                    redemption.UpdateUser = Constants.CreateUser;
                    redemption.UpdateTs = DateTime.UtcNow;

                    await _session.UpdateAsync(redemption);

                    int masterWalletResponse = _walletRepo.UpdateMasterWalletBalance(transactionTs, redemptionWalletBalance, redemptionWallet.WalletId);
                    if (masterWalletResponse == 0)
                    {

                        msg = $"{className}.{methodName}: Could not update master wallet: WalletId: {redemptionWallet.WalletId}";
                        _logger.LogError(msg);
                        await transaction.RollbackAsync();
                        _session.Clear();
                        return (false, msg);
                    }
                    else
                    {
                        _logger.LogInformation($"{className}.{methodName}: Updated master wallet balance, WalletId: {redemptionWallet?.WalletId}, " +
                            $"Balance: {redemptionWalletBalance}, Tenant: {revertRedemptionRequest.TenantCode}");
                    }

                    int consumerWalletResponse = _walletRepo.UpdateConsumerWalletBalance(transactionTs, consumerWalletBalance,
                        consumerWallet?.TotalEarned - revertRedemptionRequest.TransactionAmount,
                        consumerWallet?.WalletId ?? 0);
                    if (consumerWalletResponse == 0)
                    {

                        msg = $"{className}.{methodName}: Could not update consumer wallet, WalletId: {consumerWallet?.WalletId}";
                        _logger.LogError(msg);
                        await transaction.RollbackAsync();
                        _session.Clear();
                        return (false, msg);
                    }
                    else
                    {
                        _logger.LogInformation($"{className}.{methodName}: Updated consumer wallet balance, " +
                            $"WalletId: {consumerWallet?.WalletId}, Balance: {consumerWalletBalance}, " +
                            $"ConsumerCode: {revertRedemptionRequest.ConsumerCode}");
                    }
                    await transaction.CommitAsync();
                    _session.Clear();
                    msg = $"{className}.{methodName}: Revert Redemption transaction completed successfully. TenantCode: {revertRedemptionRequest.TenantCode}, " +
                        $"ConsumerCode: {revertRedemptionRequest.ConsumerCode},  RedemptionAmount: {revertRedemptionRequest.TransactionAmount}";
                    _logger.LogInformation(msg);
                    return (true, msg);

                }
                catch (Exception ex)
                {
                    msg = $"{className}.{methodName}: Error occurred during revert redemption transaction. Error Msg:{ex.Message}" +
                        $"TenantCode: {revertRedemptionRequest.TenantCode}, ConsumerCode: {revertRedemptionRequest.ConsumerCode}";

                    _logger.LogError(ex, msg);
                    transaction.Rollback();
                    _session.Clear();
                    return (false, msg);
                }
            }
        }

    }
}
