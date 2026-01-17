using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.Wallet.Core.Domain.Dtos;
using SunnyRewards.Helios.Wallet.Core.Domain.Models;

namespace SunnyRewards.Helios.Wallet.Infrastructure.Services.Interfaces
{
    public interface IWalletService
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="walletId"></param>
        /// <returns></returns>
        Task<WalletDto> GetWalletData(long walletId);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="walletTypeId"></param>
        /// <returns></returns>
        Task<WalletTypeDto> GetWalletType(long walletTypeId);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="walletTypeDto"></param>
        /// <returns></returns>
        Task<WalletTypeDto> GetWalletTypeCode(WalletTypeDto walletTypeDto);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="postRewardRequestDto"></param>
        /// <returns></returns>
        Task<PostResponseMultiTransactionDto> RewardDetails(PostRewardRequestDto postRewardRequestDto);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="redeemStartRequestDto"></param>
        /// <returns></returns>
        Task<PostRedeemStartResponseDto> RedeemStart(PostRedeemStartRequestDto redeemStartRequestDto);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="redeemCompleteRequestDto"></param>
        /// <returns></returns>
        Task<PostRedeemCompleteResponseDto> RedeemComplete(PostRedeemCompleteRequestDto redeemCompleteRequestDto);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="postRedeemFailRequestDto"></param>
        /// <returns></returns>
        Task<PostRedeemFailResponseDto> RedeemFail(PostRedeemFailRequestDto postRedeemFailRequestDto);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="postRewardRequestDto"></param>
        /// <returns></returns>
        Task<PostRewardResponseDto> RewardDetailsOuter(PostRewardRequestDto postRewardRequestDto);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="redeemStartRequestDto"></param>
        /// <returns></returns>
        Task<PostRedeemStartResponseDto> RedeemStartOuter(PostRedeemStartRequestDto redeemStartRequestDto);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="redeemCompleteRequestDto"></param>
        /// <returns></returns>
        Task<PostRedeemCompleteResponseDto> RedeemCompleteOuter(PostRedeemCompleteRequestDto redeemCompleteRequestDto);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="postRedeemFailRequestDto"></param>
        /// <returns></returns>
        Task<PostRedeemFailResponseDto> RedeemFailOuter(PostRedeemFailRequestDto postRedeemFailRequestDto);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="findConsumerWalletRequestDto"></param>
        /// <returns></returns>
        Task<WalletResponseDto> GetWallets(FindConsumerWalletRequestDto findConsumerWalletRequestDto);

        /// <summary>
        /// Set entries (secondary) wallet balance=0.0 for all consumers of a given tenant
        /// </summary>
        /// <param name="clearEntriesWalletRequestDto"></param>
        /// <returns></returns>
        Task<BaseResponseDto> ClearEntriesWallet(ClearEntriesWalletRequestDto clearEntriesWalletRequestDto);
        Task<BaseResponseDto> UpdateWalletBalance(IList<WalletModel> walletModel);

        /// <summary>
        /// Creates the tenant master wallets.
        /// </summary>
        /// <param name="createTenantMasterWalletsRequest">The request DTO containing tenant and app information.</param>
        /// <returns>A <see cref="BaseResponseDto"/> indicating the result of the operation.</returns>
        Task<BaseResponseDto> CreateTenantMasterWallets(CreateTenantMasterWalletsRequestDto createTenantMasterWalletsRequest);
        /// <summary>
        /// Get all the wallet types in database
        /// </summary>
        /// <returns>List of wallet types</returns>
        Task<GetWalletTypeResponseDto> GetAllWalletTypes();
        /// <summary>
        /// Creates new WalletType based on the request data
        /// </summary>
        /// <param name="walletTypeDto">request contains data to create new WalletType in database</param>
        /// <returns>base response</returns>
        Task<BaseResponseDto> CreateWalletType(WalletTypeDto walletTypeDto);
        /// <summary>
        /// Get all Tenant master wallets
        /// </summary>
        /// <param name="tenantCode">tenantCode</param>
        /// <returns>List of master wallets</returns>
        Task<GetAllMasterWalletsResponseDto> GetMasterWallets(string tenantCode);
        Task<BaseResponseDto> CreateWallet(WalletRequestDto walletRequestDto);


        Task<MaxWalletTransferRuleResponseDto> GetWalletTypeTransferRule(GetWalletTypeTransferRule getWalletTypeTransferRule);
        Task<ImportWalletTypeResponseDto> ImportWalletTypesAsync(ImportWalletTypeRequestDto walletTypeRequestDto);
    }
}
