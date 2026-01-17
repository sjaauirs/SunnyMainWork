using AutoMapper.Configuration.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SunnyRewards.Helios.Admin.Core.Domain.Constants;
using SunnyRewards.Helios.Admin.Core.Domain.Dtos;
using SunnyRewards.Helios.Admin.Infrastructure.HttpClients.Interfaces;
using SunnyRewards.Helios.Admin.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.Common.Core.Services;
using SunnyRewards.Helios.Wallet.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Admin.Infrastructure.Services
{
    public class TransactionService : BaseService, ITransactionService
    {
        private readonly ILogger<TransactionService> _logger;
        private readonly IWalletClient _walletClient;
        private readonly IWalletTypeService _walletTypeService;
        private readonly IConfiguration _config;
        const string className = nameof(TransactionService);

        public TransactionService(ILogger<TransactionService> transactionLogger, IWalletClient walletClient,
            IWalletTypeService walletTypeService, IConfiguration config)
        {
            _logger = transactionLogger;
            _walletClient = walletClient;
            _walletTypeService = walletTypeService;
            _config = config;
        }

        /// <summary>
        /// Get rewards wallets transactions
        /// </summary>
        /// <param name="walletTransactionRequest"></param>
        /// <returns></returns>
        public async Task<RewardsRecentActivityResponseDto> GetWalletTransactions(GetWalletTransactionRequestDto walletTransactionRequest)
        {
            const string methodName = nameof(GetWalletTransactions);
            try
            {
                _logger.LogInformation("{ClassName}.{MethodName}: Get rewards wallet transactions for consumerCode: {consumerCode}",
                    className, methodName, walletTransactionRequest.ConsumerCode);
                var response = new RewardsRecentActivityResponseDto();
                var transactions = await _walletClient.Post<GetWalletTransactionResponseDto>(Constant.RewardsWalletsTransactions, walletTransactionRequest);
                if (transactions.ErrorCode != null)
                {
                    _logger.LogError("{ClassName}.{MethodName}: Error occurred while fetching rewards wallet transactions, consumerCode: {consumerCode}, ErrorCode: {ErrorCode}",
                        className, methodName, walletTransactionRequest.ConsumerCode, response.ErrorCode);
                    return new RewardsRecentActivityResponseDto
                    {
                        ErrorCode = transactions.ErrorCode,
                        ErrorMessage = transactions.ErrorMessage,
                    };
                }
                var healthyActionWalletType = await _walletTypeService.GetWalletTypeCode(
                    new WalletTypeDto
                    {
                        WalletTypeCode = _config.GetSection("Health_Actions_Reward_Wallet_Type_Code").Value
                    }
                );
                var healthyActionWallet = transactions.Wallets.FirstOrDefault(x => x.WalletTypeId == healthyActionWalletType.WalletTypeId);
                if (healthyActionWallet == null)
                {
                    _logger.LogError("{ClassName}.{MethodName}: Error occurred while fetching rewards wallet transactions, consumerCode: {consumerCode}, ErrorCode: {ErrorCode}",
                        className, methodName, walletTransactionRequest.ConsumerCode, StatusCodes.Status404NotFound);
                    return new RewardsRecentActivityResponseDto
                    {
                        ErrorCode = StatusCodes.Status404NotFound,
                        ErrorMessage = "No healthy action wallet found",
                    };
                }
                response.MaxAvailable = healthyActionWallet.EarnMaximum;
                response.AvailableToSpend = healthyActionWallet.Balance;
                response.LeftToEarn = healthyActionWallet.EarnMaximum - healthyActionWallet.TotalEarned;
                response.TotalEarned = healthyActionWallet.TotalEarned;
                response.RecentTransactions = (from t in transactions.Transactions
                                               select new RecentTransaction
                                               {
                                                   TransactionCode = t.Transaction?.TransactionCode,
                                                   TransactionType = t.Transaction?.TransactionType,
                                                   TransactionDate = t.Transaction.CreateTs,
                                                   TransactionAmount = t.Transaction.TransactionAmount,
                                                   Description = t.TransactionDetail?.RewardDescription,
                                                   WalletTypeCode = t.TransactionWalletType?.WalletTypeCode,
                                                   WalletTypeName = t.TransactionWalletType?.WalletTypeName,
                                                   Notes = t.TransactionDetail?.Notes,
                                                   IsPending = false,
                                                   TransactionDetailType = t.TransactionDetail?.TransactionDetailType
                                               }).ToList();


                _logger.LogInformation("{ClassName}.{MethodName}: Successfully fetching rewards wallet transactions, consumerCode: {consumerCode}",
                    className, methodName, walletTransactionRequest.ConsumerCode);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName}: Exception occurred while fetching rewards wallet transactions. ErrorMessage: {ErrorMessage}",
                    className, methodName, ex.Message);
                throw;
            }
        }
    }
}
