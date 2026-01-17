using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SunnyRewards.Helios.Admin.Core.Domain.Constants;
using SunnyRewards.Helios.Admin.Infrastructure.HttpClients.Interfaces;
using SunnyRewards.Helios.Admin.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.Task.Core.Domain.Dtos;
using SunnyRewards.Helios.User.Core.Domain.Dtos;
using SunnyRewards.Helios.Wallet.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Admin.Infrastructure.Services
{
    public class WalletService : IWalletService
    {
        private readonly ILogger<WalletService> _logger;
        private readonly IWalletClient _walletClient;
        private readonly IUserClient _userClient;
        private readonly ITaskClient _taskClient;

        public WalletService(
            ILogger<WalletService> logger,
            IWalletClient walletClient,
            IUserClient userClient,
            ITaskClient taskClient)
        {
            _logger = logger;
            _walletClient = walletClient;
            _userClient = userClient;
            _taskClient = taskClient;
        }

        /// <summary>
        /// Set entries (secondary) wallet balance=0.0 for all consumers of a given tenant
        /// </summary>
        /// <param name="clearEntriesWalletRequestDto"></param>
        /// <returns></returns>
        public async Task<BaseResponseDto> ClearEntriesWallet(ClearEntriesWalletRequestDto clearEntriesWalletRequestDto)
        {
            try
            {
                if (string.IsNullOrEmpty(clearEntriesWalletRequestDto.TenantCode))
                    return new BaseResponseDto
                    {
                        ErrorCode = StatusCodes.Status400BadRequest,
                        ErrorMessage = "No tenant code supplied"
                    };

                var walletResponse = await _walletClient.Post<BaseResponseDto>("wallet/clear-entries-wallet", clearEntriesWalletRequestDto);

                return walletResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ERROR - msg : {msg}", ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Revert all transactions and tasks for given consumer
        /// </summary>
        /// <param name="revertTransactionsRequestDto"></param>
        /// <returns></returns>
        public async Task<BaseResponseDto> RevertAllTransactionsAndTasksForConsumer(RevertTransactionsRequestDto revertTransactionsRequestDto)
        {
            try
            {
                // Validate input parameters
                if (!ValidateInput(revertTransactionsRequestDto, out string? errorMessage))
                {
                    return new BaseResponseDto { ErrorCode = StatusCodes.Status400BadRequest, ErrorMessage = errorMessage };
                }

                // Get consumer details
                var consumerCode = revertTransactionsRequestDto.ConsumerCode;
                var consumerResponse = await GetConsumerAsync(revertTransactionsRequestDto.ConsumerCode);
                if (consumerResponse == null || consumerResponse.Consumer == null || consumerResponse.Consumer.ConsumerCode == null)
                {
                    _logger.LogError("RevertAllTransactionsAndTasksForConsumer: Consumer not found for consumerCode: {consumerCode}", consumerCode);
                    return new BaseResponseDto { ErrorCode = StatusCodes.Status404NotFound, ErrorMessage = "Consumer Not Found" };
                }

                // Revert transactions
                var transactionsResponse = await _walletClient.Post<BaseResponseDto>("transaction/revert-all-transactions", revertTransactionsRequestDto);
                if (transactionsResponse.ErrorCode != null)
                {
                    _logger.LogError("RevertAllTransactionsAndTasksForConsumer: Error occurred while reverting wallet transactions for consumer: {ConsumerCode}. ErrorMessage: {ErrorMessage}",
                        consumerCode, transactionsResponse.ErrorMessage);
                    return transactionsResponse;
                }

                // Revert consumer tasks
                var revertAllConsumerTasksRequestDto = new RevertAllConsumerTasksRequestDto()
                {
                    ConsumerCode = consumerCode,
                    TenantCode = revertTransactionsRequestDto.TenantCode
                };
                var consumerTasksResponse = await _taskClient.Post<BaseResponseDto>("revert-all-consumer-tasks", revertAllConsumerTasksRequestDto);
                if (consumerTasksResponse.ErrorCode != null)
                {
                    _logger.LogError("RevertAllTransactionsAndTasksForConsumer: Error occurred while reverting consumer tasks for consumer: {ConsumerCode}. ErrorMessage: {ErrorMessage}",
                        consumerCode, consumerTasksResponse.ErrorMessage);
                    return consumerTasksResponse;
                }

                _logger.LogInformation("RevertAllTransactionsAndTasksForConsumer: Successfully reverted all transactions and consumer tasks for ConsumerCode: {ConsumerCode}", consumerCode);
                return new BaseResponseDto();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "RevertAllTransactionsAndTasksForConsumer: Unexpected exception occurred. TenantCode: {TenantCode}, ConsumerCode: {ConsumerCode}",
                    revertTransactionsRequestDto.TenantCode, revertTransactionsRequestDto.ConsumerCode);
                throw;
            }

        }

        public async Task<PostRedeemCompleteResponseDto> RedeemConsumerBalance(PostRedeemStartRequestDto postRedeemStartRequestDto)
        {
            try
            {
                var redeemStartResponse = await _walletClient.Post<PostRedeemStartResponseDto>("wallet/redeem-start", postRedeemStartRequestDto);
                if (redeemStartResponse.ErrorCode != null)
                {
                    _logger.LogInformation("RedeemConsumerBalance: An error occurred while redeeming wallet balance of Consumer: {ConsumerCode}, Error: {Message}",
                        postRedeemStartRequestDto.ConsumerCode, redeemStartResponse.ErrorMessage);
                    return new PostRedeemCompleteResponseDto()
                    {
                        ErrorCode = redeemStartResponse.ErrorCode,
                        ErrorMessage = redeemStartResponse.ErrorMessage
                    };
                }
                var postRedeemCompleteRequestDto = new PostRedeemCompleteRequestDto()
                {
                    ConsumerCode = postRedeemStartRequestDto.ConsumerCode,
                    RedemptionVendorCode = postRedeemStartRequestDto.RedemptionVendorCode,
                    RedemptionRef = postRedeemStartRequestDto.RedemptionRef
                };

                var redeemSuccessResponse = await _walletClient.Post<PostRedeemCompleteResponseDto>("wallet/redeem-complete", postRedeemCompleteRequestDto);
                _logger.LogInformation("RedeemConsumerBalance: Successfully redeem completed for Consumer: {ConsumerCode}", postRedeemStartRequestDto.ConsumerCode);
                return redeemSuccessResponse;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "RedeemConsumerBalance: Error:{Message}", ex.Message);
                throw;
            }
        }

        private static bool ValidateInput(RevertTransactionsRequestDto request, out string? errorMessage)
        {
            if (string.IsNullOrEmpty(request.TenantCode) || string.IsNullOrEmpty(request.ConsumerCode))
            {
                errorMessage = "Invalid input: TenantCode and ConsumerCode are required.";
                return false;
            }

            errorMessage = null;
            return true;
        }

        private async Task<GetConsumerResponseDto> GetConsumerAsync(string? consumerCode)
        {
            return await _userClient.Post<GetConsumerResponseDto>("consumer/get-consumer", new GetConsumerRequestDto { ConsumerCode = consumerCode });
        }

        public async Task<GetAllMasterWalletsResponseDto> GetMasterWallets(string tenantCode)
        {
            var parameters = new Dictionary<string, long>();
            return await _walletClient.Get<GetAllMasterWalletsResponseDto>($"{Constant.MasterWallet}/{tenantCode}", parameters);
        }

        public async Task<BaseResponseDto> CreateWallet(WalletRequestDto walletRequestDto)
        {
            return await _walletClient.Post<BaseResponseDto>(Constant.Wallet, walletRequestDto);
        }

        /// <summary>
        /// Creates the tenant master wallets.
        /// </summary>
        /// <param name="createTenantMasterWalletsRequest">The request DTO containing tenant and app information.</param>
        /// <returns>A <see cref="BaseResponseDto"/> indicating the result of the operation.</returns>
        public async Task<BaseResponseDto> CreateTenantMasterWallets(CreateTenantMasterWalletsRequestDto createTenantMasterWalletsRequest)
        {
            return await _walletClient.Post<BaseResponseDto>(Constant.CreateMasterWallet, createTenantMasterWalletsRequest);
        }
    }
}