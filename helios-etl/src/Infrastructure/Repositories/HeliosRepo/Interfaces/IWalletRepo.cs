using SunnyRewards.Helios.ETL.Common.Repositories.Interfaces;
using SunnyRewards.Helios.ETL.Core.Domain.Dtos;
using SunnyRewards.Helios.ETL.Core.Domain.Models;

namespace SunnyRewards.Helios.ETL.Infrastructure.Repositories.HeliosRepo.Interfaces
{
    public interface IWalletRepo : IBaseRepo<ETLWalletModel>
    {

        /// <summary>
        /// Get wallet based on given TenantCode, ConsumerCode and WalletType
        /// </summary>
        /// <param name="tenantCode"></param>
        /// <param name="consumerCode"></param>
        /// <param name="walletTypeId"></param>
        /// <returns></returns>
        Task<ETLWalletModel> GetWalletByConsumerAndWalletType(string? tenantCode, string? consumerCode, long walletTypeId);
        Task<ETLWalletModel> GetWalletByConsumerAndWalletTypeForTransactionSync(string? tenantCode, string? consumerCode, long walletTypeId);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="transactionTs"></param>
        /// <param name="masterWalletBalance"></param>
        /// <param name="walletId"></param>
        /// <param name="xmin"></param>
        /// <returns></returns>
        int UpdateConsumerWalletBalance(DateTime transactionTs, double? consumerWalletBalance, double? totalEarned, long walletId);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="transactionTs"></param>
        /// <param name="masterWalletBalance"></param>
        /// <param name="walletId"></param>
        /// <param name="xmin"></param>
        /// <returns></returns>
        int UpdateMasterWalletBalance(DateTime transactionTs, double? masterWalletBalance, long walletId);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="tenantCode"></param>
        /// <param name="walletTypeId"></param>
        /// <returns></returns>
        List<WalletDetailsDto> GetAllConsumerWalletsAndWallets(string tenantCode, long walletTypeId, List<string>? consumerCodes = null);

        Task<(ETLWalletModel, ETLWalletTypeModel)> GetMasterWalletTypeByTenantAndWalletType(string? tenantCode, long walletTypeId);
    }
}
