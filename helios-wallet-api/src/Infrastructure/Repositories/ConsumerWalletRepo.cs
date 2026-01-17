using Microsoft.Extensions.Logging;
using NHibernate.Linq;
using SunnyRewards.Helios.Common.Core.Repositories;
using SunnyRewards.Helios.Wallet.Core.Domain.Dtos;
using SunnyRewards.Helios.Wallet.Core.Domain.Models;
using SunnyRewards.Helios.Wallet.Infrastructure.Repositories.Interfaces;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace SunnyRewards.Helios.Wallet.Infrastructure.Repositories
{
    [ExcludeFromCodeCoverage]
    public class ConsumerWalletRepo : BaseRepo<ConsumerWalletModel>, IConsumerWalletRepo
    {

        private readonly NHibernate.ISession _session;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="baseLogger"></param>
        /// <param name="session"></param>
        public ConsumerWalletRepo(ILogger<BaseRepo<ConsumerWalletModel>> baseLogger, NHibernate.ISession session) : base(baseLogger, session)
        {
            _session = session;

        }
        /// <summary>
        /// Get Consumer wallet by wallet type
        /// </summary>
        /// <param name="consumerCode"></param>
        /// <param name="walletTypeId"></param>
        /// <returns></returns>
        public async Task<IList<ConsumerWalletModel>> GetConsumerWalletsByWalletType(string? consumerCode, long walletTypeId)
        {
            var query = (from cwt in _session.Query<ConsumerWalletModel>()
                         join wt in _session.Query<WalletModel>() on cwt.WalletId equals wt.WalletId
                         where cwt.ConsumerCode == consumerCode && wt.WalletTypeId == walletTypeId && cwt.DeleteNbr == 0
                         && wt.ActiveStartTs <= DateTime.UtcNow &&  wt.ActiveEndTs >= DateTime.UtcNow
                         select cwt);

            return await query.ToListAsync();
        }

        /// <summary>
        /// Retrieves consumer wallets excluding those with a specific wallet type ID.
        /// </summary>
        /// <param name="consumerCode">The consumer code identifying the consumer.</param>
        /// <param name="walletTypeId">The wallet type ID to exclude.</param>
        /// <returns>A list of consumer wallets excluding the specified wallet type ID.</returns>
        public async Task<IList<ConsumerWalletModel>> GetConsumerWalletsExcludingWalletType(string? consumerCode, long walletTypeId)
        {
            var query = (from cwt in _session.Query<ConsumerWalletModel>()
                         join wt in _session.Query<WalletModel>() on cwt.WalletId equals wt.WalletId
                         where cwt.ConsumerCode == consumerCode && wt.WalletTypeId != walletTypeId && cwt.DeleteNbr == 0
                          && wt.ActiveStartTs <= DateTime.UtcNow && wt.ActiveEndTs >= DateTime.UtcNow
                         select cwt);

            return await query.ToListAsync();
        }


        /// <summary>
        /// Gets the consumer all wallets.
        /// </summary>
        /// <param name="tenantCode">The tenant code.</param>
        /// <param name="consumerCode">The consumer code.</param>
        /// <returns></returns>
        public async Task<List<ConsumerWalletDetailsModel>> GetConsumerAllWallets(string? tenantCode, string? consumerCode)
        {
            var query = from cwt in _session.Query<ConsumerWalletModel>()
                        join wt in _session.Query<WalletModel>() on cwt.WalletId equals wt.WalletId
                        join wtt in _session.Query<WalletTypeModel>() on wt.WalletTypeId equals wtt.WalletTypeId
                        where cwt.ConsumerCode == consumerCode &&
                              wt.TenantCode == tenantCode &&
                              wt.DeleteNbr == 0 &&
                              wt.ActiveStartTs <= DateTime.UtcNow && wt.ActiveEndTs >= DateTime.UtcNow &&
                              cwt.TenantCode == tenantCode &&
                              cwt.DeleteNbr == 0 &&
                              wtt.DeleteNbr == 0
                        select new ConsumerWalletDetailsModel
                        {
                            ConsumerWallet = cwt,
                            Wallet = wt,
                            WalletType = wtt
                        };

            return await query.ToListAsync();
        }

        public async Task<IList<ConsumerWalletDetailsModel>> GetConsumerWalletsWithDetails(string consumerCode, bool IncludeRedeemOnlyWallets = false, long? excludeWalletTypeId = null)
        {
            var now = DateTime.UtcNow;
            var query = from cwt in _session.Query<ConsumerWalletModel>()
                        join wt in _session.Query<WalletModel>() on cwt.WalletId equals wt.WalletId
                        join wtt in _session.Query<WalletTypeModel>() on wt.WalletTypeId equals wtt.WalletTypeId
                        where cwt.ConsumerCode == consumerCode
                              && cwt.DeleteNbr == 0
                              && wt.ActiveStartTs <= DateTime.UtcNow && wt.ActiveEndTs >= DateTime.UtcNow
                              && wt.DeleteNbr == 0
                              && (excludeWalletTypeId == null || wt.WalletTypeId != excludeWalletTypeId)
                        select new ConsumerWalletDetailsModel
                        {
                            ConsumerWallet = cwt,
                            Wallet = wt,
                            WalletType = wtt
                        };

            var result = await query.ToListAsync();

            if (IncludeRedeemOnlyWallets)
            {
                var redeemQuery =
                    from cwt in _session.Query<ConsumerWalletModel>()
                    join wt in _session.Query<WalletModel>() on cwt.WalletId equals wt.WalletId
                    join wtt in _session.Query<WalletTypeModel>() on wt.WalletTypeId equals wtt.WalletTypeId
                    where cwt.ConsumerCode == consumerCode
                          && cwt.DeleteNbr == 0
                          && wt.DeleteNbr == 0
                          && wt.ActiveEndTs < now
                          && wt.RedeemEndTs >= now
                          && (excludeWalletTypeId == null || wt.WalletTypeId != excludeWalletTypeId)
                    select new ConsumerWalletDetailsModel
                    {
                        ConsumerWallet = cwt,
                        Wallet = wt,
                        WalletType = wtt
                    };

                var redeemWallets = await redeemQuery.ToListAsync();

                // Combine both
                result.AddRange(redeemWallets);
            }

            return result;
        }

        public Task<List<WalletModel>> GetConsumerWallets(List<string> walletTypeCodes, string consumerCode)
        {
            var wallets = (from w in _session.Query<WalletModel>()
                          join cw in _session.Query<ConsumerWalletModel>() on w.WalletId equals cw.WalletId
                          join wt in _session.Query<WalletTypeModel>() on w.WalletTypeId equals wt.WalletTypeId
                          where cw.ConsumerCode == consumerCode && cw.DeleteNbr == 0 && w.DeleteNbr == 0
                          && w.ActiveStartTs <= DateTime.UtcNow && w.ActiveEndTs >= DateTime.UtcNow
                          && wt.DeleteNbr == 0 && walletTypeCodes.Contains(wt.WalletTypeCode!)
                          select w).ToListAsync();

            return wallets;
        }
    }
}

