using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.Wallet.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Admin.Infrastructure.Services.Interfaces
{
    public interface IWalletService
    {
        /// <summary>
        /// Set entries (secondary) wallet balance=0.0 for all consumers of a given tenant
        /// </summary>
        /// <param name="clearEntriesWalletRequestDto"></param>
        /// <returns></returns>
        Task<BaseResponseDto> ClearEntriesWallet(ClearEntriesWalletRequestDto clearEntriesWalletRequestDto);

        /// <summary>
        /// Initiates the redemption process for a consumer's balance.
        /// </summary>
        /// <param name="postRedeemStartRequestDto"></param>
        /// <returns></returns>
        Task<PostRedeemCompleteResponseDto> RedeemConsumerBalance(PostRedeemStartRequestDto postRedeemStartRequestDto);

        /// <summary>
        /// Revert all transactions and tasks for given consumer
        /// </summary>
        /// <param name="revertTransactionsRequestDto"></param>
        /// <returns></returns>
        Task<BaseResponseDto> RevertAllTransactionsAndTasksForConsumer(RevertTransactionsRequestDto revertTransactionsRequestDto);
        /// <summary>
        /// Get all Tenant master wallets
        /// </summary>
        /// <param name="tenantCode">tenantCode</param>
        /// <returns>List of master wallets</returns>
        Task<GetAllMasterWalletsResponseDto> GetMasterWallets(string tenantCode);
        Task<BaseResponseDto> CreateWallet(WalletRequestDto walletRequestDto);

        /// <summary>
        /// Creates the tenant master wallets.
        /// </summary>
        /// <param name="createTenantMasterWalletsRequest">The request DTO containing tenant and app information.</param>
        /// <returns>A <see cref="BaseResponseDto"/> indicating the result of the operation.</returns>
        Task<BaseResponseDto> CreateTenantMasterWallets(CreateTenantMasterWalletsRequestDto createTenantMasterWalletsRequest);
    }
}
