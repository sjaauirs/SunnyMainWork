using Microsoft.Extensions.Logging;
using NHibernate.Linq;
using SunnyRewards.Helios.ETL.Common.Repositories;
using SunnyRewards.Helios.ETL.Core.Domain.Dtos;
using SunnyRewards.Helios.ETL.Core.Domain.Models;
using SunnyRewards.Helios.ETL.Infrastructure.Repositories.HeliosRepo.Interfaces;
using SunnyRewards.Helios.User.Core.Domain.Models;

namespace SunnyRewards.Helios.ETL.Infrastructure.Repositories.HeliosRepo
{
    public class WalletRepo : BaseRepo<ETLWalletModel>, IWalletRepo
    {


        private readonly NHibernate.ISession _session;
        public WalletRepo(ILogger<BaseRepo<ETLWalletModel>> baseLogger, NHibernate.ISession session) : base(baseLogger, session)
        {
            _session = session;
        }
        /// <summary>
        /// Get wallet based on given TenantCode, ConsumerCode and WalletType
        /// </summary>
        /// <param name="tenantCode"></param>
        /// <param name="consumerCode"></param>
        /// <param name="walletTypeId"></param>
        /// <returns></returns>
        public async Task<ETLWalletModel> GetWalletByConsumerAndWalletType(string? tenantCode, string? consumerCode, long walletTypeId)
        {
            var query = from wt in _session.Query<ETLWalletModel>()
                        join cwt in _session.Query<ETLConsumerWalletModel>() on wt.WalletId equals cwt.WalletId
                        where cwt.ConsumerCode == consumerCode &&
                              wt.WalletTypeId == walletTypeId &&
                              wt.TenantCode == tenantCode &&
                              wt.ActiveStartTs<= DateTime.UtcNow &&
                              wt.ActiveEndTs >= DateTime.UtcNow &&
                              wt.DeleteNbr == 0 &&
                              cwt.TenantCode == tenantCode &&
                              cwt.DeleteNbr == 0
                        select wt;

            return await query.FirstOrDefaultAsync();
        }

        public async Task<ETLWalletModel> GetWalletByConsumerAndWalletTypeForTransactionSync(string? tenantCode, string? consumerCode, long walletTypeId)
        {
            var query = from wt in _session.Query<ETLWalletModel>()
                        join cwt in _session.Query<ETLConsumerWalletModel>() on wt.WalletId equals cwt.WalletId
                        where cwt.ConsumerCode == consumerCode &&
                              wt.WalletTypeId == walletTypeId &&
                              wt.TenantCode == tenantCode &&
                              wt.DeleteNbr == 0 &&
                              cwt.TenantCode == tenantCode &&
                              cwt.DeleteNbr == 0
                        select wt;

            return await query.FirstOrDefaultAsync();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="transactionTs"></param>
        /// <param name="masterWalletBalance"></param>
        /// <param name="walletId"></param>
        /// <param name="xmin"></param>
        /// <returns></returns>

        public int UpdateMasterWalletBalance(DateTime transactionTs, double? masterWalletBalance, long walletId)
        {
            int rec = _session.Query<ETLWalletModel>()
                       .Where(x => x.WalletId == walletId && x.DeleteNbr == 0)
                       .Update(x => new { balance = masterWalletBalance, update_ts = transactionTs });
            return rec;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="transactionTs"></param>
        /// <param name="consumerWalletBalance"></param>
        /// <param name="walletId"></param>
        /// <param name="xmin"></param>
        /// <returns></returns>
        public int UpdateConsumerWalletBalance(DateTime transactionTs, double? consumerWalletBalance, double? totalEarned, long walletId)
        {
            int rec = _session.Query<ETLWalletModel>()
                        .Where(x => x.WalletId == walletId && x.DeleteNbr == 0)
                        .Update(x => new { balance = consumerWalletBalance, total_earned = totalEarned, update_ts = transactionTs });
            return rec;
        }

        /// <summary>
        /// Retrieves a list of wallet details for a specific tenant and wallet type.
        /// This method performs a join between wallet and consumer wallet models, 
        /// filtering by tenant code, wallet type, and excluding deleted and master wallets.
        /// </summary>
        /// <param name="tenantCode">The tenant code used to filter the wallets.</param>
        /// <param name="walletTypeId">The ID of the wallet type to filter by.</param>
        /// <returns>A list of <see cref="WalletDetailsDto"/> containing wallet and consumer wallet details.</returns>
        public List<WalletDetailsDto> GetAllConsumerWalletsAndWallets(string tenantCode, long walletTypeId, List<string>? consumerCodes = null)
        {
            var query = from w in _session.Query<ETLWalletModel>()
                        join cm in _session.Query<ETLConsumerWalletModel>() on w.WalletId equals cm.WalletId
                        join c in _session.Query<ETLConsumerModel>() on cm.ConsumerCode equals c.ConsumerCode
                        join p in _session.Query<ETLPersonModel>() on c.PersonId equals p.PersonId
                        where w.TenantCode == tenantCode
                              && w.WalletTypeId == walletTypeId
                              && w.DeleteNbr == 0
                              && w.MasterWallet == false
                              && w.ActiveStartTs <= DateTime.UtcNow
                              && w.ActiveEndTs >= DateTime.UtcNow
                              && cm.DeleteNbr == 0
                              && p.SyntheticUser == false
                              && c.DeleteNbr == 0
                              && p.DeleteNbr == 0
                        select new WalletDetailsDto()
                        {
                            WalletModel = w,
                            ConsumerWalletModel = cm,
                        };
            if (consumerCodes != null && consumerCodes.Any())
            {
                query = query.Where(x => consumerCodes.Contains(x.ConsumerWalletModel.ConsumerCode!));
            }

            return query.ToList();
        }

        public async Task<(ETLWalletModel, ETLWalletTypeModel)> GetMasterWalletTypeByTenantAndWalletType(string? tenantCode, long walletTypeId)
        {
            var query = from wt in _session.Query<ETLWalletModel>()
                        join wtp in _session.Query<ETLWalletTypeModel>() on wt.WalletTypeId equals wtp.WalletTypeId
                        where wt.MasterWallet == true &&
                              wt.WalletTypeId == walletTypeId &&
                              wt.TenantCode == tenantCode &&
                              wt.DeleteNbr == 0 &&
                              wtp.DeleteNbr == 0
                        select new { wt, wtp };

            var result = await query.FirstOrDefaultAsync();

            return (result.wt, result.wtp);
        }
    }
}
