using SunnyRewards.Helios.Common.Core.Repositories.Interfaces;
using SunnyRewards.Helios.Wallet.Core.Domain.Models;

namespace SunnyRewards.Helios.Wallet.Infrastructure.Repositories.Interfaces
{
    public interface IConsumerWalletRepo : IBaseRepo<ConsumerWalletModel> 
    {
        /// <summary>
        /// Get Consumer wallet by wallet type
        /// </summary>
        /// <param name="consumerCode"></param>
        /// <param name="walletTypeId"></param>
        /// <returns></returns>
        Task<IList<ConsumerWalletModel>> GetConsumerWalletsByWalletType(string? consumerCode,  long walletTypeId);

        /// <summary>
        /// Retrieves consumer wallets excluding those with a specific wallet type ID.
        /// </summary>
        /// <param name="consumerCode"></param>
        /// <param name="walletTypeId"></param>
        /// <returns></returns>
        Task<IList<ConsumerWalletModel>> GetConsumerWalletsExcludingWalletType(string? consumerCode, long walletTypeId);

        /// <summary>
        /// Gets the consumer all wallets.
        /// </summary>
        /// <param name="tenantCode">The tenant code.</param>
        /// <param name="consumerCode">The consumer code.</param>
        /// <returns></returns>
        Task<List<ConsumerWalletDetailsModel>> GetConsumerAllWallets(string? tenantCode, string? consumerCode);
        Task<List<WalletModel>> GetConsumerWallets(List<string> walletTypeCodes,string consumerCode);

        Task<IList<ConsumerWalletDetailsModel>> GetConsumerWalletsWithDetails(string consumerCode, bool IncludeRedeemOnlyWallets = false, long? excludeWalletTypeId = null);

    }
}
