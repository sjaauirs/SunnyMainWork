using SunnyRewards.Helios.Common.Core.Repositories.Interfaces;
using SunnyRewards.Helios.Wallet.Core.Domain.Models;

namespace SunnyRewards.Helios.Wallet.Infrastructure.Repositories.Interfaces
{
    public interface IWalletRepo : IBaseRepo<WalletModel>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="walletTypeId"></param>
        /// <param name="consumerCode"></param>
        /// <returns></returns>
        Task<WalletModel?> GetConsumerWallet(long walletTypeId, string consumerCode);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="walletTypeId"></param>
        /// <param name="consumerCode"></param>
        /// <returns></returns>
        Task<WalletModel> GetMasterWallet(long walletTypeId, string tenantCode);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="transactionTs"></param>
        /// <param name="masterWalletBalance"></param>
        /// <param name="walletId"></param>
        /// <param name="xmin"></param>
        /// <returns></returns>
        int UpdateConsumerWalletBalance(DateTime transactionTs, double? consumerWalletBalance, double? totalEarned, long walletId, int xmin);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="transactionTs"></param>
        /// <param name="masterWalletBalance"></param>
        /// <param name="walletId"></param>
        /// <param name="xmin"></param>
        /// <returns></returns>
        int UpdateMasterWalletBalance(DateTime transactionTs, double? masterWalletBalance, long walletId, int xmin);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="transactionTs"></param>
        /// <param name="redemptionWalletBalance"></param>
        /// <param name="walletId"></param>
        /// <param name="xmin"></param>
        /// <returns></returns>
        int UpdateRedemptionWalletBalance(DateTime transactionTs, double? redemptionWalletBalance, long walletId, int xmin);

        /// <summary>
        /// Set entries (secondary) wallet balance=0.0 for all consumers of a given tenant
        /// </summary>
        /// <param name="tenantCode"></param>
        /// <param name="walletTypeId"></param>
        /// <returns></returns>
        Task ClearEntriesWalletBalance(string? tenantCode, long walletTypeId);

        /// <summary>
        /// Get wallet based on given TenantCode, ConsumerCode and WalletType
        /// </summary>
        /// <param name="tenantCode"></param>
        /// <param name="consumerCode"></param>
        /// <param name="walletTypeId"></param>
        /// <returns></returns>
        Task<WalletModel> GetWalletByConsumerAndWalletType(string? tenantCode, string? consumerCode, long walletTypeId);
        int UpdateWalletBalance(WalletModel walletModel);

        Task<WalletModel> GetSuspenseWallet(long walletTypeId, string tenantCode, string walletName);

        void UpdateMasterWalletBalance(long walletId, double masterWalletBalance, DateTime transactionTs);

        void UpdateConsumerWalletBalance(long walletId, double consumerWalletBalance, double totalEarned, DateTime transactionTs);
        Task<WalletModel?> GetConsumerWalletById(long walletId, string consumerCode);
    }
}
