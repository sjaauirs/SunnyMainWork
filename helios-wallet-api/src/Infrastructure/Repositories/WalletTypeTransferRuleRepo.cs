using Microsoft.Extensions.Logging;
using NHibernate.Linq;
using SunnyRewards.Helios.Common.Core.Repositories;
using SunnyRewards.Helios.Wallet.Core.Domain.Dtos;
using SunnyRewards.Helios.Wallet.Core.Domain.Models;
using SunnyRewards.Helios.Wallet.Infrastructure.Repositories.Interfaces;

namespace SunnyRewards.Helios.Wallet.Infrastructure.Repositories
{
    public class WalletTypeTransferRuleRepo : BaseRepo<WalletTypeTransferRuleModel>, IWalletTypeTransferRuleRepo
    {

        private readonly NHibernate.ISession _session;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="baseLogger"></param>
        /// <param name="session"></param>
        public WalletTypeTransferRuleRepo(ILogger<BaseRepo<WalletTypeTransferRuleModel>> baseLogger, NHibernate.ISession session) : base(baseLogger, session)
        {
            _session = session;
        }

        /// <summary>
        /// Get walletType transfer rule by tenant code
        /// </summary>
        /// <param name="tenantCode"></param>
        /// <returns></returns>
        public async Task<List<ExportWalletTypeTransferRuleDto>> GetWalletTypeTransferRules(string tenantCode)
        {
            var walletTypeTransferRules = await (from rule in _session.Query<WalletTypeTransferRuleModel>()
                                                 join source in _session.Query<WalletTypeModel>() on rule.SourceWalletTypeId equals source.WalletTypeId
                                                 join target in _session.Query<WalletTypeModel>() on rule.TargetWalletTypeId equals target.WalletTypeId
                                                 where rule.TenantCode == tenantCode && rule.DeleteNbr == 0
                                                 && source.DeleteNbr == 0 && target.DeleteNbr == 0
                                                 select new ExportWalletTypeTransferRuleDto
                                                 {
                                                     WalletTypeTransferRuleId = rule.WalletTypeTransferRuleId,
                                                     WalletTypeTransferRuleCode = rule.WalletTypeTransferRuleCode,
                                                     TenantCode = rule.TenantCode,
                                                     SourceWalletTypeId = rule.SourceWalletTypeId,
                                                     TargetWalletTypeId = rule.TargetWalletTypeId,
                                                     SourceWalletTypeCode = source.WalletTypeCode!,
                                                     TargetWalletTypeCode = target.WalletTypeCode!,
                                                     TransferRule = rule.TransferRule
                                                 }).ToListAsync();
            return walletTypeTransferRules;
        }
    }
}
