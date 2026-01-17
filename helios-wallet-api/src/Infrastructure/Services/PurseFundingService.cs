using Amazon.CloudWatch;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SunnyRewards.Helios.Common.Core.Domain;
using SunnyRewards.Helios.Wallet.Core.Domain.Constants;
using SunnyRewards.Helios.Wallet.Core.Domain.Dtos;
using SunnyRewards.Helios.Wallet.Core.Domain.Models;
using SunnyRewards.Helios.Wallet.Infrastructure.Repositories.Interfaces;
using SunnyRewards.Helios.Wallet.Infrastructure.Services.Interfaces;
using System.Security.Cryptography;

namespace SunnyRewards.Helios.Wallet.Infrastructure.Services
{
    public class PurseFundingService : IPurseFundingService
    {
        private readonly ILogger<PurseFundingService> _logger;
        private readonly IWalletTypeRepo _walletTypeRepo;
        private readonly IWalletRepo _walletRepo;
        private const string className = nameof(PurseFundingService);
        private readonly NHibernate.ISession _session;
        private readonly ITransactionRepo _transactionRepo;
        private readonly IConfiguration _configuration;
        private readonly int _maxTries;
        public PurseFundingService(ILogger<PurseFundingService> logger,
            IWalletTypeRepo walletTypeRepo,
            IWalletRepo walletRepo,
            NHibernate.ISession session,
            ITransactionRepo transactionRepo,
            IConfiguration configuration)
        {
            _logger = logger;
            _walletTypeRepo = walletTypeRepo;
            _walletRepo = walletRepo;
            _session = session;
            _transactionRepo = transactionRepo;
            _configuration = configuration;
            _maxTries = WalletConstants.MaxTries;
            string? opMaxTries = _configuration.GetSection("OperationMaxTries").Value;
            if (!string.IsNullOrEmpty(opMaxTries))
            {
                _maxTries = Convert.ToInt32(opMaxTries);
            }
        }

        /// <summary>
        /// Purses the funding asynchronous.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns></returns>
        public async Task<PurseFundingResponseDto> PurseFundingAsync(PurseFundingRequestDto purseFundingRequestDto)
        {
            const string methodName = nameof(PurseFundingAsync);
            int maxTries = _maxTries;
            var response = new PurseFundingResponseDto();
            _logger.LogInformation("{className}.{methodName}:  Has been Invoked for ConsumerCode: {ConsumerCode}", className, methodName, purseFundingRequestDto.ConsumerCode);

            while (maxTries > 0)
            {
                try
                {
                    response = await FundTransfer(purseFundingRequestDto);
                    if (response.ErrorCode == null)
                    {
                        break; // success no need to retry.
                    }

                    _session.Clear();
                    _logger.LogError("{className}.{methodName}: Response errorCode {errCode} in RewardDetailsOuter retrying count left={maxTries}, consumer: {consCode}", className, methodName, response.ErrorCode, maxTries,
                        purseFundingRequestDto.ConsumerCode);
                    maxTries--;
                }
                catch (Exception ex)
                {
                    _session.Clear();
                    _logger.LogError(ex, "{className}.{methodName}: Error in RewardDetailsOuter retrying count left={maxTries} and Error Msg:{errorMsg}", className, methodName, maxTries, ex.Message);
                    maxTries--;
                    Thread.Sleep(GetSecureRandomNumber(WalletConstants.RetryMinWaitMS, WalletConstants.RetryMaxWaitMS));
                }
            }

            return response;
        }

        /// <summary>
        /// Funds the transfer.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns></returns>
        private async Task<PurseFundingResponseDto> FundTransfer(PurseFundingRequestDto request)
        {
            const string methodName = nameof(FundTransfer);
            using var transaction = _session.BeginTransaction();
            try
            {
                var transactionTs = DateTime.UtcNow;

                var masterWalletType = await GetWalletType(request.MasterWalletType);
                var consumerWalletType = await GetWalletType(request.ConsumerWalletType);

                var masterWallet = await GetWallet(masterWalletType.WalletTypeId, request.TenantCode, isMasterWallet: true);
                var consumerWallet = await GetWallet(consumerWalletType.WalletTypeId, request.TenantCode, isMasterWallet: false, request.ConsumerCode);

                var transactionDetail = await CreateTransactionDetails(request, transactionTs);
                var createTransactionsResult = await CreateTransactions(request, transactionTs, masterWallet, consumerWallet, transactionDetail);

                UpdateMasterWalletBalance(request, methodName, transactionTs, masterWallet, createTransactionsResult.MasterWalletBalance);
                UpdateConsumerWalletBalance(request, methodName, transactionTs, consumerWallet, createTransactionsResult.ConsumerWalletBalance, createTransactionsResult.TotalEarned);

                _logger.LogInformation("{className}.{methodName}: Created funding history record, TenantCode:{TenantCode}, ConsumerCode: {ConsumerCode}", className, methodName, request.TenantCode, request.ConsumerCode);
                await transaction.CommitAsync();
                _session.Clear();
                _logger.LogInformation("{className}.{methodName}: Fund transfer completed successfully. TenantCode: {TenantCode}, ConsumerCode: {ConsumerCode}, RuleNumber: {RuleNumber}, Amount: {Amount}",
                    className, methodName, request.TenantCode, request.ConsumerCode, request.RuleNumber, request.Amount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{className}.{methodName}: Error occurred during fund transfer. TenantCode: {tenantCode}, ConsumerCode: {consumerCode}, RuleNumber: {ruleNumber}", className, methodName, request.TenantCode, request.ConsumerCode, request.RuleNumber);
                await transaction.RollbackAsync();
                return new PurseFundingResponseDto()
                {
                    ErrorCode = StatusCodes.Status500InternalServerError,
                    ErrorMessage = ex.Message
                };
            }

            return new PurseFundingResponseDto();

        }

        /// <summary>
        /// Updates the master wallet balance.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="methodName">Name of the method.</param>
        /// <param name="transactionTs">The transaction ts.</param>
        /// <param name="masterWallet">The master wallet.</param>
        /// <param name="masterWalletBalance">The master wallet balance.</param>
        /// <exception cref="Amazon.CloudWatchLogs.Model.InvalidOperationException">Concurrency error in updating Master Wallet: {masterWallet?.WalletId}</exception>
        private void UpdateMasterWalletBalance(PurseFundingRequestDto request, string methodName, DateTime transactionTs, WalletModel? masterWallet, double? masterWalletBalance)
        {
            int rec = _walletRepo.UpdateMasterWalletBalance(transactionTs, masterWalletBalance, masterWallet?.WalletId ?? 0, masterWallet?.Xmin ?? 0);

            if (rec == 0)
            {
                _logger.LogError("{className}.{methodName}: Could not update master wallet: WalletId: {walletId}", className, methodName,
                    masterWallet?.WalletId);
                throw new InvalidOperationException($"Concurrency error in updating Master Wallet: {masterWallet?.WalletId}");
            }
            else
            {
                _logger.LogInformation("{className}.{methodName}: Updated master wallet balance, WalletId: {walletId}, Balance: {balance}, Tenant: {tenant}", className, methodName,
                    masterWallet?.WalletId, masterWalletBalance, request.TenantCode);

            }
        }

        /// <summary>
        /// Updates the consumer wallet balance.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="methodName">Name of the method.</param>
        /// <param name="transactionTs">The transaction ts.</param>
        /// <param name="consumerWallet">The consumer wallet.</param>
        /// <param name="consumerWalletBalance">The consumer wallet balance.</param>
        /// <param name="totalEarned">The total earned.</param>
        /// <exception cref="Amazon.CloudWatchLogs.Model.InvalidOperationException">Concurrency error in updating Consumer Wallet: {consumerWallet?.WalletId}</exception>
        private void UpdateConsumerWalletBalance(PurseFundingRequestDto request, string methodName, DateTime transactionTs, WalletModel? consumerWallet, double? consumerWalletBalance, double? totalEarned)
        {
            int recc = _walletRepo.UpdateConsumerWalletBalance(transactionTs, consumerWalletBalance, totalEarned, consumerWallet?.WalletId ?? 0, consumerWallet?.Xmin ?? 0);
            if (recc == 0)

            {
                _logger.LogError("{className}.{methodName}: Could not update consumer wallet: WalletId: {walletId}", className, methodName,
                    consumerWallet?.WalletId);
                throw new InvalidOperationException($"Concurrency error in updating Consumer Wallet: {consumerWallet?.WalletId}");
            }
            else
            {
                _logger.LogInformation("{className}.{methodName}: Updated consumer wallet balance, WalletId: {walletId}, Balance: {balance}, ConsumerCode: {consumerCode}", className, methodName,
                    consumerWallet?.WalletId, consumerWalletBalance, request.ConsumerCode);
            }
        }

        /// <summary>
        /// Creates the transactions.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="transactionTs">The transaction ts.</param>
        /// <param name="masterWallet">The master wallet.</param>
        /// <param name="consumerWallet">The consumer wallet.</param>
        /// <param name="transactionDetail">The transaction detail.</param>
        /// <returns></returns>
        private async Task<CreateTransactionsResultDto> CreateTransactions(PurseFundingRequestDto request, DateTime transactionTs, WalletModel? masterWallet, WalletModel? consumerWallet, TransactionDetailModel transactionDetail)
        {
            string newTxnCode = "txn-" + Guid.NewGuid().ToString("N");

            var fundAmount = Math.Max(request.Amount, 0);

            double? masterWalletBalance = masterWallet?.Balance - fundAmount;
            var subTransaction = new TransactionModel()
            {
                WalletId = masterWallet?.WalletId ?? 0,
                TransactionCode = newTxnCode,
                TransactionType = Constants.Subtract,
                PreviousBalance = masterWallet?.Balance,
                TransactionAmount = fundAmount,
                Balance = masterWalletBalance,
                PrevWalletTxnCode = masterWallet?.WalletId + ":" + await GetPreviousTransactionCodeByWalletId(masterWallet!.WalletId),
                CreateTs = transactionTs,
                CreateUser = Constants.CreateUser,
                UpdateTs = DateTime.UtcNow,
                UpdateUser = Constants.CreateUser,
                DeleteNbr = 0,
                TransactionDetailId = transactionDetail.TransactionDetailId
            };

            await _session.SaveAsync(subTransaction);

            double? consumerWalletBalance = consumerWallet?.Balance + fundAmount;
            double? totalEarned = consumerWallet?.TotalEarned + fundAmount;
            var addTransaction = new TransactionModel()
            {
                WalletId = consumerWallet?.WalletId ?? 0,
                TransactionCode = newTxnCode,
                TransactionType = Constants.Addition,
                PreviousBalance = consumerWallet?.Balance,
                TransactionAmount = fundAmount,
                Balance = consumerWalletBalance,
                PrevWalletTxnCode = consumerWallet?.WalletId + ":" + await GetPreviousTransactionCodeByWalletId(consumerWallet!.WalletId),
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
            return new CreateTransactionsResultDto
            {
                MasterWalletBalance = masterWalletBalance,
                ConsumerWalletBalance = consumerWalletBalance,
                TotalEarned = totalEarned
            };
        }

        /// <summary>
        /// Creates the transaction details.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="transactionTs">The transaction ts.</param>
        /// <returns></returns>
        private async Task<TransactionDetailModel> CreateTransactionDetails(PurseFundingRequestDto request, DateTime transactionTs)
        {
            var transactionDetail = new TransactionDetailModel()
            {
                TransactionDetailType = request.TransactionDetailType,
                ConsumerCode = request.ConsumerCode,
                RewardDescription = request.RewardDescription,
                Notes = null,
                RedemptionRef = null,
                RedemptionItemDescription = null,
                CreateTs = transactionTs,
                CreateUser = Constants.CreateUser,
                UpdateTs = DateTime.UtcNow,
                UpdateUser = Constants.CreateUser,
                DeleteNbr = 0
            };
            var transactionDetailResponse = await _session.SaveAsync(transactionDetail);
            transactionDetail.TransactionDetailId = Convert.ToInt64(transactionDetailResponse);
            return transactionDetail;
        }

        /// <summary>
        /// Gets the type of the wallet.
        /// </summary>
        /// <param name="walletTypeCode">The wallet type code.</param>
        /// <returns></returns>
        /// <exception cref="Amazon.CloudWatchLogs.Model.InvalidOperationException">Wallet type not found. WalletTypeCode: {walletTypeCode}</exception>
        private async Task<WalletTypeModel> GetWalletType(string walletTypeCode)
        {
            var walletType = await _walletTypeRepo.FindOneAsync(x => x.WalletTypeCode == walletTypeCode && x.DeleteNbr == 0);
            if (walletType == null)
                throw new InvalidOperationException($"Wallet type not found. WalletTypeCode: {walletTypeCode}");
            return walletType;
        }

        /// <summary>
        /// Gets the wallet.
        /// </summary>
        /// <param name="walletTypeId">The wallet type identifier.</param>
        /// <param name="tenantCode">The tenant code.</param>
        /// <param name="isMasterWallet">if set to <c>true</c> [is master wallet].</param>
        /// <param name="consumerCode">The consumer code.</param>
        /// <returns></returns>
        /// <exception cref="Amazon.CloudWatchLogs.Model.InvalidOperationException"></exception>
        private async Task<WalletModel> GetWallet(long walletTypeId, string tenantCode, bool isMasterWallet, string? consumerCode = null)
        {
            var wallet = isMasterWallet
                ? await _walletRepo.FindOneAsync(w => w.TenantCode == tenantCode && w.WalletTypeId == walletTypeId && w.MasterWallet && w.DeleteNbr == 0)
                : await _walletRepo.GetWalletByConsumerAndWalletType(tenantCode, consumerCode, walletTypeId);

            if (wallet == null)
                throw new InvalidOperationException($"{(isMasterWallet ? "Master" : "Consumer")} wallet not found for WalletTypeId: {walletTypeId}");
            return wallet;
        }

        /// <summary>
        /// Gets the previous transaction code by wallet identifier.
        /// </summary>
        /// <param name="walletId">The wallet identifier.</param>
        /// <returns></returns>
        private async Task<string> GetPreviousTransactionCodeByWalletId(long walletId)
        {
            var maxWalletTransactionId = await _transactionRepo.GetMaxTransactionIdByWallet(walletId);
            var lastWalletTransaction = await _transactionRepo.FindOneAsync(t => t.TransactionId == maxWalletTransactionId);

            return lastWalletTransaction?.TransactionCode ?? "Init";
        }

        private int GetSecureRandomNumber(int minValue, int maxValue)
        {
            var buffer = new byte[4];
            RandomNumberGenerator.Fill(buffer);
            int result = BitConverter.ToInt32(buffer, 0) & int.MaxValue;
            return minValue + (result % (maxValue - minValue));
        }
    }
}
