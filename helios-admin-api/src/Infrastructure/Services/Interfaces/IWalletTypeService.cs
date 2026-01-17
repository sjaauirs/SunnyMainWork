using SunnyRewards.Helios.Admin.Core.Domain.Dtos;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.Task.Core.Domain.Dtos;
using SunnyRewards.Helios.Wallet.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Admin.Infrastructure.Services.Interfaces
{
    public interface IWalletTypeService
    {
        /// <summary>
        /// Get all the available WalletTypes
        /// </summary>
        /// <returns></returns>
        Task<GetWalletTypeResponseDto> GetAllWalletTypes();
        /// <summary>
        /// Creates new WalletType based on the request data
        /// </summary>
        /// <param name="walletTypeDto">request contains data to create new WalletType in database</param>
        /// <returns>base response</returns>
        Task<BaseResponseDto> CreateWalletType(WalletTypeDto walletTypeDto);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="walletTypeDto"></param>
        /// <returns></returns>
        Task<WalletTypeDto> GetWalletTypeCode(WalletTypeDto walletTypeDto);
        /// <summary>
        /// Imports a list of wallet types asynchronously by sending them to the wallet client.
        /// </summary>
        /// <param name="walletTypes">The list of wallet types to be imported.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result contains an 
        /// <see cref="ImportWalletTypeResponseDto"/> indicating the result of the import operation,
        /// including error details if the operation fails.
        /// </returns>
        Task<ImportWalletTypeResponseDto> ImportWalletTypesAsync(List<WalletTypeDto> walletTypes);

        Task<TaskRewardWalletSplitConfigDto> GetTaskRewardMonetaryDollarWalletSplit(TaskRewardDto taskRewardDto, string consumerCode, string tenantCode, bool isLiveTransferToRewardsPurseEnabled);
        Task<List<WalletSplitConfig>> CreateMissingWalletsAsync(string consumerCode, string tenantCode, bool isLiveTransferToRewardsPurseEnabled,
            List<WalletSplitConfig> walletSplitConfig);
    }
}
