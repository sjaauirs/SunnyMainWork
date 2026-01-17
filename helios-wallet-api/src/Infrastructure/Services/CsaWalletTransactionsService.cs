using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SunnyRewards.Helios.Common.Core.Domain;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.Wallet.Core.Domain.Constants;
using SunnyRewards.Helios.Wallet.Core.Domain.Dtos;
using SunnyRewards.Helios.Wallet.Core.Domain.Dtos.Json;
using SunnyRewards.Helios.Wallet.Core.Domain.Models;
using SunnyRewards.Helios.Wallet.Infrastructure.Repositories.Interfaces;
using SunnyRewards.Helios.Wallet.Infrastructure.Services.Interfaces;
using ISession = NHibernate.ISession;

namespace SunnyRewards.Helios.Wallet.Infrastructure.Services
{
    public class CsaWalletTransactionsService : ICsaWalletTransactionsService
    {
        private readonly ILogger<CsaWalletTransactionsService> _logger;
        private readonly ISession _session;
        private readonly IWalletRepo _walletRepo;
        private readonly IWalletTypeRepo _walletTypeRepo;
        private readonly IConsumerWalletRepo _consumerWalletRepo;
        private readonly ITransactionRepo _transactionRepo;
        private readonly IConfiguration _configuration;
        private const string className = nameof(CsaWalletTransactionsService);
        public CsaWalletTransactionsService(ILogger<CsaWalletTransactionsService> logger, ISession session, IWalletRepo walletRepo,
            IConsumerWalletRepo consumerWalletRepo, ITransactionRepo transactionRepo, IWalletTypeRepo walletTypeRepo, IConfiguration configuration)
        {
            _logger = logger;
            _session = session;
            _walletRepo = walletRepo;
            _consumerWalletRepo = consumerWalletRepo;
            _transactionRepo = transactionRepo;
            _walletTypeRepo = walletTypeRepo;
            _configuration = configuration;
        }
        /// <summary>
        /// Handles CSA wallet transactions by performing validation, updating wallet balances,
        /// and recording transaction details. The process involves multiple wallets (master, consumer, and suspense)
        /// and ensures transactional integrity.
        /// </summary>
        /// <param name="csaWalletRequestDto">
        /// A DTO containing details of the CSA wallet transaction request, 
        /// including tenant code, consumer code, wallet ID, amount, and tenant configuration.
        /// </param>
        /// <returns>
        /// A <see cref="BaseResponseDto"/> object indicating the outcome of the operation.
        /// </returns>
        /// <remarks>
        /// The method ensures the following steps are performed in a transactional context:
        /// 1. Validates the inputs such as master, consumer, and suspense wallets.
        /// 2. Updates the balances of involved wallets (master, consumer, and suspense) accordingly.
        /// 3. Creates transaction details for auditing.
        /// 4. Logs all relevant information during the process.
        /// If an error occurs, the transaction is rolled back to maintain data integrity.
        /// </remarks>
        /// <exception cref="Exception">
        /// Thrown if an error occurs during the validation or transaction processing steps.
        /// </exception>

        public async Task<BaseResponseDto> HandleCsaWalletTransactions(CsaWalletTransactionsRequestDto csaWalletRequestDto)
        {
            const string methodName = nameof(HandleCsaWalletTransactions);
            try
            {
                _logger.LogInformation("{ClassName}.{MethodName}: Started processing Csa wallet transactions with TenantCode:{Tenant},ConsumerCode:{Code}",
                    className, methodName, csaWalletRequestDto.TenantCode, csaWalletRequestDto.ConsumerCode);

                using var transaction = _session.BeginTransaction();
                try
                {
                    // Validate Wallets
                    var walletsDto = await ValidateInputs(csaWalletRequestDto, methodName);

                    if (!walletsDto.IsValid)
                    {
                        return new BaseResponseDto() { ErrorCode = StatusCodes.Status404NotFound };
                    }

                    var transactionTs = DateTime.UtcNow;
                    double transactionAmount = csaWalletRequestDto.Amount;
                    var mastertransactionDetail = await CreateTransactionDetail(csaWalletRequestDto, transactionTs, WalletConstants.Benifits);
                    // process master wallet
                    var masterWalletBalance = walletsDto.MasterWallet.Balance - transactionAmount;
                    await ProcessWalletTransaction(walletsDto.MasterWallet, mastertransactionDetail, transactionAmount, masterWalletBalance, Constants.Subtract);
                    _walletRepo.UpdateMasterWalletBalance(walletsDto.MasterWallet.WalletId, masterWalletBalance, transactionTs);
                    _logger.LogInformation("{className}.{methodName}: Updated master wallet balance, WalletId: {walletId}, Balance: {balance}, Tenant: {tenant}", className, methodName,
                            walletsDto.MasterWallet?.WalletId, masterWalletBalance, csaWalletRequestDto.TenantCode);

                    // process consumer wallet
                    var addConsumerWalletbalance = walletsDto.ConsumerWallet.Balance + transactionAmount;
                    var totalEarned = walletsDto.ConsumerWallet.TotalEarned + transactionAmount;
                    await ProcessWalletTransaction(walletsDto.ConsumerWallet, mastertransactionDetail, transactionAmount, addConsumerWalletbalance, Constants.Addition);
                    _walletRepo.UpdateConsumerWalletBalance(walletsDto.ConsumerWallet.WalletId, addConsumerWalletbalance, totalEarned, transactionTs);
                    _logger.LogInformation("{ClassName}.{MethodName}: Updated consumer wallet balance, WalletId: {walletId}, Balance: {balance}, Tenant: {tenant}", className, methodName,
                            walletsDto.ConsumerWallet?.WalletId, addConsumerWalletbalance, csaWalletRequestDto.TenantCode);


                    var suspensetransactionDetail = await CreateTransactionDetail(csaWalletRequestDto, transactionTs, WalletConstants.Redemption);

                    // process consumer wallet
                    walletsDto.ConsumerWallet.Balance = addConsumerWalletbalance;
                    walletsDto.ConsumerWallet.TotalEarned = totalEarned;
                    var subConsumerWalletbalance = walletsDto.ConsumerWallet.Balance - transactionAmount;
                    var consumerTotalEarned = walletsDto.ConsumerWallet.TotalEarned - transactionAmount;
                    var subtransactionId = await ProcessWalletTransaction(walletsDto.ConsumerWallet, suspensetransactionDetail, transactionAmount, subConsumerWalletbalance, Constants.Subtract);
                    _walletRepo.UpdateConsumerWalletBalance(walletsDto.ConsumerWallet.WalletId, subConsumerWalletbalance, consumerTotalEarned, transactionTs);
                    _logger.LogInformation("{ClassName}.{MethodName}: Updated consumer wallet balance, WalletId: {WalletId}, Balance: {Balance}, Tenant: {Tenant}", className, methodName,
                            walletsDto.ConsumerWallet?.WalletId, subConsumerWalletbalance, csaWalletRequestDto.TenantCode);


                    // process suspense wallet
                    var redemptionWalletBalance = walletsDto.RedemptionWallet.Balance + transactionAmount;
                    var addTransactionId = await ProcessWalletTransaction(walletsDto?.RedemptionWallet, suspensetransactionDetail, transactionAmount, redemptionWalletBalance, Constants.Addition);
                    _walletRepo.UpdateMasterWalletBalance(walletsDto.RedemptionWallet.WalletId, redemptionWalletBalance, transactionTs);

                    _logger.LogInformation("{ClassName}.{MethodName}: Updated suspense wallet balance, WalletId: {WalletId}, Balance: {Balance}, Tenant: {Tenant}", className, methodName,
                            walletsDto.RedemptionWallet, redemptionWalletBalance, csaWalletRequestDto.TenantCode);

                    await CreateRedemption(addTransactionId, subtransactionId);
                    await transaction.CommitAsync();

                    _logger.LogInformation("{ClassName}.{MethodName}: Sucessfully processed Csa wallet transactions with TenantCode:{Tenant},ConsumerCode:{Code}",
                   className, methodName, csaWalletRequestDto.TenantCode, csaWalletRequestDto.ConsumerCode);

                    return new BaseResponseDto();
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _session.Clear();
                    _logger.LogError(ex, "{ClassName}.{MethodName}: Error occurred during wallet transaction handling with TenantCode:{Tenant},ConsumerCode:{Code}",
                        className, methodName, csaWalletRequestDto.TenantCode, csaWalletRequestDto.ConsumerCode);
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName}: Error occurred during wallet transaction handling with TenantCode:{Tenant},ConsumerCode:{Code}",
                    className, methodName, csaWalletRequestDto.TenantCode, csaWalletRequestDto.ConsumerCode);
                throw;
            }
        }

        private static string GetUniqueCode()
        {
            return Guid.NewGuid().ToString().Replace("-", "");
        }

        /// <summary>
        /// Validates the inputs required for processing CSA wallet transactions, including
        /// wallets, wallet types, tenant configurations, and balances.
        /// </summary>
        /// <param name="csaWalletRequestDto">
        /// A DTO containing details of the CSA wallet transaction request, 
        /// including tenant code, consumer code, wallet ID, amount, and tenant configuration.
        /// </param>
        /// <returns>
        /// A tuple containing: if all are valid returns true,wallet,master wallet,suspense wallet
        /// If any validation step fails, the tuple contains `false` and `null` for the wallet objects.
        /// </returns>
        /// <remarks>
        /// This method performs several validation steps:
        /// <list type="number">
        /// <item>Validates the existence of the consumer wallet.</item>
        /// <item>Validates the wallet type and tenant configuration.</item>
        /// <item>Validates the master wallet and its balance.</item>
        /// <item>Validates the suspense wallet and its associated type.</item>
        /// </list>
        /// Appropriate error or warning logs are generated for each failure case.
        /// </remarks>
        /// <exception cref="JsonException">
        /// Thrown if the TenantConfig JSON cannot be deserialized.
        /// </exception>
        private async Task<WalletsDto> ValidateInputs(CsaWalletTransactionsRequestDto csaWalletRequestDto, string methodName)
        {
            var consumerWalletsAndWallets = await _consumerWalletRepo.GetConsumerAllWallets(csaWalletRequestDto.TenantCode, csaWalletRequestDto.ConsumerCode);

            var consumerWalletDetailsModel = GetWallet(csaWalletRequestDto, consumerWalletsAndWallets);
            if (consumerWalletsAndWallets == null || consumerWalletDetailsModel?.Wallet == null)
            {
                _logger.LogError("{ClassName}.{MethodName}:Consumer Wallet not found with TenantCode:{Tenant},ConsumerCode:{Code},WalletId:{Id}",
                              className, methodName, csaWalletRequestDto.TenantCode, csaWalletRequestDto.ConsumerCode, csaWalletRequestDto.WalletId);
                return new WalletsDto() { IsValid = false };
            }

            var tenantConfig = JsonConvert.DeserializeObject<TenantConfig>(csaWalletRequestDto.TenantConfig);
            var purseConfig = tenantConfig?.PurseConfig;
            if (purseConfig == null || purseConfig.Purses?.Count == 0)
            {
                _logger.LogError("{ClassName}.{MethodName}: Purse config is null with TenantCode:{Tenant},ConsumerCode:{Code}",
                              className, methodName, csaWalletRequestDto.TenantCode, csaWalletRequestDto.ConsumerCode);
                return new WalletsDto() { IsValid = false };
            }

            var purse = purseConfig?.Purses?.Find(x => x.PurseWalletType == consumerWalletDetailsModel?.WalletType?.WalletTypeCode);
            if (purse == null)
            {
                _logger.LogError("{ClassName}.{MethodName}: Purse not found with WalletType:{Type},TenantCode:{Tenant},ConsumerCode:{Code}",
                              className, methodName, consumerWalletDetailsModel?.WalletType?.WalletTypeCode, csaWalletRequestDto.TenantCode, csaWalletRequestDto.ConsumerCode);
                return new WalletsDto() { IsValid = false };
            }

            var consumerWalletAndWalletType = consumerWalletsAndWallets.FirstOrDefault(x => x.WalletType?.WalletTypeCode == purse.WalletType && x.WalletType.DeleteNbr == 0);
            if (consumerWalletAndWalletType == null || consumerWalletAndWalletType.WalletType == null)
            {
                _logger.LogError("{ClassName}.{MethodName}: consumer wallet type not found with WalletTypeCode:{Type},TenantCode:{Tenant},ConsumerCode:{Code}",
                              className, methodName, purse.WalletType, csaWalletRequestDto.TenantCode, csaWalletRequestDto.ConsumerCode);
                return new WalletsDto() { IsValid = false };
            }
            var consumerWallet = await _walletRepo.GetConsumerWallet(consumerWalletAndWalletType.WalletType.WalletTypeId, csaWalletRequestDto.ConsumerCode);
            if (consumerWallet == null)
            {
                _logger.LogError("{ClassName}.{MethodName}: consumer wallet not found with WalletTypeCode:{Type},TenantCode:{Tenant},ConsumerCode:{Code}",
                              className, methodName, purse.WalletType, csaWalletRequestDto.TenantCode, csaWalletRequestDto.ConsumerCode);
                return new WalletsDto() { IsValid = false };
            }

            var masterWalletType = await _walletTypeRepo.FindOneAsync(x => x.WalletTypeCode == purse.MasterWalletType && x.DeleteNbr == 0);
            if (masterWalletType == null)
            {
                _logger.LogError("{ClassName}.{MethodName}: Master wallet type not found with WalletTypeCode:{Type},TenantCode:{Tenant},ConsumerCode:{Code}",
                              className, methodName, purse.MasterWalletType, csaWalletRequestDto.TenantCode, csaWalletRequestDto.ConsumerCode);
                return new WalletsDto() { IsValid = false };
            }

            var masterWallet = await _walletRepo.GetMasterWallet(masterWalletType.WalletTypeId, csaWalletRequestDto.TenantCode);
            if (masterWallet == null || masterWallet.Balance < csaWalletRequestDto.Amount)
            {
                _logger.LogWarning("{ClassName}.{MethodName}: Master wallet does not have sufficient funds. WalletId: {WalletId}", className, methodName, masterWallet?.WalletId);
                return new WalletsDto() { IsValid = false };
            }

            var masterRedemptionWalletType = await _walletTypeRepo.FindOneAsync(x => x.WalletTypeCode == purse.MasterRedemptionWalletType && x.DeleteNbr == 0);
            if (masterRedemptionWalletType == null)
            {
                _logger.LogError("{ClassName}.{MethodName}: Suspense wallet type not found with WalletTypeCode:{Type},TenantCode:{Tenant},ConsumerCode:{Code}",
                              className, methodName, purse.MasterWalletType, csaWalletRequestDto.TenantCode, csaWalletRequestDto.ConsumerCode);
                return new WalletsDto() { IsValid = false };
            }
            var masterRedemptionWallet = new WalletModel();
            var rewardRemdeptionWalletTypeCode = GetRedemptionWalletTypeCode();
            if (purse.MasterRedemptionWalletType == rewardRemdeptionWalletTypeCode)
            {
                masterRedemptionWallet = await _walletRepo.FindOneAsync(x => x.WalletTypeId == masterRedemptionWalletType.WalletTypeId &&
                            x.TenantCode == csaWalletRequestDto.TenantCode && x.WalletName == WalletConstants.RewardSuspenseWalletName && x.MasterWallet && x.DeleteNbr == 0);
            }
            else
            {
                masterRedemptionWallet = await _walletRepo.FindOneAsync(x => x.WalletTypeId == masterRedemptionWalletType.WalletTypeId &&
                    x.TenantCode == csaWalletRequestDto.TenantCode && x.MasterWallet && x.DeleteNbr == 0);
            }
            if (masterRedemptionWallet == null)
            {
                _logger.LogError("{ClassName}.{MethodName}: redemption wallet type not found with TenantCode:{Tenant},ConsumerCode:{Code},WalletTypeCode:{Id}",
                              className, methodName, csaWalletRequestDto.TenantCode, csaWalletRequestDto.ConsumerCode, purse.MasterRedemptionWalletType);
                return new WalletsDto() { IsValid = false };
            }

            return new WalletsDto() { IsValid = true, ConsumerWallet = consumerWallet, MasterWallet = masterWallet, RedemptionWallet = masterRedemptionWallet };
        }

        // Helper Methods
        private async Task<long> ProcessWalletTransaction(WalletModel wallet, TransactionDetailModel transactionDetail, double transactionAmount, double updatedBalance,
            string transactionType)
        {
            const string methodName = nameof(ProcessWalletTransaction);
            try
            {
                var newTxnCode = "txn-" + Guid.NewGuid().ToString("N");
                var Transaction = new TransactionModel
                {
                    WalletId = wallet.WalletId,
                    TransactionCode = newTxnCode,
                    TransactionType = transactionType,
                    PreviousBalance = wallet.Balance,
                    TransactionAmount = transactionAmount,
                    Balance = updatedBalance,
                    PrevWalletTxnCode = wallet.WalletId + ":" + await GetPreviousTransactionCode(wallet.WalletId),
                    CreateTs = DateTime.UtcNow,
                    CreateUser = Constants.CreateUser,
                    UpdateTs = DateTime.UtcNow,
                    UpdateUser = Constants.CreateUser,
                    DeleteNbr = 0,
                    TransactionDetailId = transactionDetail.TransactionDetailId
                };

                var transactionresponse = await _session.SaveAsync(Transaction);
                return Convert.ToInt64(transactionresponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName}: Error occurred creating transaction detail with WalletId:{Id},TenantCode:{TCode},ConsumerCode:{Code}",
                        className, methodName, wallet.WalletId, wallet.TenantCode, transactionDetail.ConsumerCode);
                throw;
            }
        }

        private async Task CreateRedemption(long addtransactionId, long subtransactionId)
        {
            const string methodName = nameof(CreateRedemption);
            try
            {
                var redemption = new RedemptionModel
                {
                    SubTransactionId = subtransactionId,
                    AddTransactionId = addtransactionId,
                    RedemptionStatus = Constants.Completed,
                    RedemptionRef = GetUniqueCode(),
                    RedemptionStartTs = DateTime.UtcNow,
                    CreateTs = DateTime.UtcNow,
                    CreateUser = Constants.CreateUser,
                    UpdateTs = DateTime.UtcNow,
                    UpdateUser = Constants.CreateUser,
                    DeleteNbr = 0,
                };

                await _session.SaveAsync(redemption);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName}: Error occurred creating transaction detail with AddTransactionId:{AId},SubTransactionId:{Id}",
                       className, methodName, addtransactionId, subtransactionId);

                throw;
            }
        }
        private async Task<TransactionDetailModel> CreateTransactionDetail(CsaWalletTransactionsRequestDto requestDto, DateTime transactionTs
            , string transactionDetailsType)
        {
            var methodName = nameof(CreateTransactionDetail);
            try
            {
                var transactionDetail = new TransactionDetailModel
                {
                    TransactionDetailType = transactionDetailsType,
                    RewardDescription = requestDto.Description,
                    ConsumerCode = requestDto.ConsumerCode,
                    CreateTs = transactionTs,
                    TaskRewardCode = null,
                    Notes = null,
                    RedemptionRef = GetUniqueCode(),
                    RedemptionItemDescription = requestDto.Description,
                    CreateUser = Constants.CreateUser,
                    UpdateTs = DateTime.UtcNow,
                    UpdateUser = Constants.CreateUser,
                    DeleteNbr = 0
                };
                var transactionDetailResponse = await _session.SaveAsync(transactionDetail);
                transactionDetail.TransactionDetailId = Convert.ToInt64(transactionDetailResponse);

                return transactionDetail;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName}: Error occurred creating transaction detail with TenantCode:{Tenant},ConsumerCode:{Code}",
                       className, methodName, requestDto.TenantCode, requestDto.ConsumerCode);
                throw;
            }
        }
        private async Task<string> GetPreviousTransactionCode(long walletId)
        {
            var maxWalletTransactionId = await _transactionRepo.GetMaxTransactionIdByWallet(walletId);
            var lastWalletTransaction = await _transactionRepo.FindOneAsync(t => t.TransactionId == maxWalletTransactionId);

            return lastWalletTransaction.TransactionCode ?? "Init";
        }
        private static ConsumerWalletDetailsModel? GetWallet(CsaWalletTransactionsRequestDto requestDto, IEnumerable<ConsumerWalletDetailsModel> wallets)
        {
            return wallets
                .FirstOrDefault(x => x.Wallet?.WalletId == requestDto.WalletId && x.Wallet.DeleteNbr == 0);
        }
        private string? GetRedemptionWalletTypeCode()
        {
            return _configuration.GetSection("Redemption_Wallet_Type_Code").Value;
        }
    }
}
