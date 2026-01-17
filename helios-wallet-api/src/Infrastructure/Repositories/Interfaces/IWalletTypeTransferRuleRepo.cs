using SunnyRewards.Helios.Common.Core.Repositories.Interfaces;
using SunnyRewards.Helios.Wallet.Core.Domain.Dtos;
using SunnyRewards.Helios.Wallet.Core.Domain.Models;

namespace SunnyRewards.Helios.Wallet.Infrastructure.Repositories.Interfaces
{
    public interface IWalletTypeTransferRuleRepo : IBaseRepo<WalletTypeTransferRuleModel>
    {
        /// <summary>
        /// Get the transfer rule for a given source and target wallet type
        /// </summary>
        /// <param name="sourceWalletTypeId"></param>
        /// <param name="targetWalletTypeId"></param>
        /// <returns></returns>
        Task<List<ExportWalletTypeTransferRuleDto>> GetWalletTypeTransferRules(string tenantCode);
    }
}
