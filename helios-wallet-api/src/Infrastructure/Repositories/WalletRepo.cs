using Microsoft.Extensions.Logging;
using NHibernate.Linq;
using NHibernate.Linq.Visitors;
using NHibernate.Mapping;
using SunnyRewards.Helios.Common.Core.Repositories;
using SunnyRewards.Helios.Wallet.Core.Domain.Models;
using SunnyRewards.Helios.Wallet.Infrastructure.Repositories.Interfaces;
using System.Diagnostics.CodeAnalysis;

namespace SunnyRewards.Helios.Wallet.Infrastructure.Repositories
{
    [ExcludeFromCodeCoverage]
    public class WalletRepo : BaseRepo<WalletModel>, IWalletRepo
    {
        private readonly NHibernate.ISession _session;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="baseLogger"></param>
        /// <param name="session"></param>
        public WalletRepo(ILogger<BaseRepo<WalletModel>> baseLogger, NHibernate.ISession session) : base(baseLogger, session)
        {
            _session = session;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="walletTypeId"></param>
        /// <param name="tenantCode"></param>
        /// <returns></returns>
        public async Task<WalletModel> GetMasterWallet(long walletTypeId, string tenantCode)
        {
            var wallets = (from wallet in _session.Query<WalletModel>()
                           .Where(x => x.WalletTypeId == walletTypeId && x.TenantCode == tenantCode)
                           select wallet).ToList();
            var masterWallet = wallets.FirstOrDefault(x => x.MasterWallet && x.DeleteNbr == 0);

            return await Task.FromResult(masterWallet ?? new WalletModel());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="walletTypeId"></param>
        /// <param name="consumerCode"></param>
        /// <returns></returns>
        public async Task<WalletModel?> GetConsumerWallet(long walletTypeId, string consumerCode)
        {
            var walletModel = await _session.Query<WalletModel>()
                .Join(_session.Query<ConsumerWalletModel>(),
                      wallet => wallet.WalletId,
                      consumerWallet => consumerWallet.WalletId,
                      (wallet, consumerWallet) => new { wallet, consumerWallet })
                .Where(x => x.wallet.WalletTypeId == walletTypeId &&
                            x.wallet.DeleteNbr == 0 &&
                            x.wallet.ActiveStartTs <= DateTime.UtcNow &&            // while retrieving consumer wallet, ensure it's active
                            x.wallet.ActiveEndTs >= DateTime.UtcNow &&
                            x.consumerWallet.ConsumerCode == consumerCode &&
                            x.consumerWallet.DeleteNbr == 0)
                .Select(x => x.wallet)
                .FirstOrDefaultAsync();

            return walletModel ?? new WalletModel();
        }

        public async Task<WalletModel?> GetConsumerWalletById(long walletId, string consumerCode)
        {
            var walletModel = await _session.Query<WalletModel>()
                .Join(_session.Query<ConsumerWalletModel>(),
                      wallet => wallet.WalletId,
                      consumerWallet => consumerWallet.WalletId,
                      (wallet, consumerWallet) => new { wallet, consumerWallet })
                .Where(x => x.wallet.WalletId == walletId &&
                            x.wallet.DeleteNbr == 0 &&
                            x.wallet.ActiveStartTs <= DateTime.UtcNow &&            // while retrieving consumer wallet, ensure it's active
                            x.wallet.RedeemEndTs >= DateTime.UtcNow &&
                            x.consumerWallet.ConsumerCode == consumerCode &&
                            x.consumerWallet.DeleteNbr == 0)
                .Select(x => x.wallet)
                .FirstOrDefaultAsync();

            return walletModel ?? new WalletModel();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="transactionTs"></param>
        /// <param name="masterWalletBalance"></param>
        /// <param name="walletId"></param>
        /// <param name="xmin"></param>
        /// <returns></returns>

        public int UpdateMasterWalletBalance(DateTime transactionTs, double? masterWalletBalance, long walletId, int xmin)
        {
            int rec = _session.Query<WalletModel>()
                       .Where(x => x.WalletId == walletId && x.Xmin == xmin)
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
        public int UpdateConsumerWalletBalance(DateTime transactionTs, double? consumerWalletBalance, double? totalEarned, long walletId, int xmin)
        {
            int rec = _session.Query<WalletModel>()
                        .Where(x => x.WalletId == walletId && x.Xmin == xmin)
                        .Update(x => new { balance = consumerWalletBalance, total_earned = totalEarned, update_ts = transactionTs });
            return rec;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="transactionTs"></param>
        /// <param name="redemptionWalletBalance"></param>
        /// <param name="walletId"></param>
        /// <param name="xmin"></param>
        /// <returns></returns>
        public int UpdateRedemptionWalletBalance(DateTime transactionTs, double? redemptionWalletBalance, long walletId, int xmin)
        {
            int rec = _session.Query<WalletModel>()
                         .Where(x => x.WalletId == walletId && x.Xmin == xmin)
                         .Update(x => new { balance = redemptionWalletBalance, update_ts = transactionTs });
            return rec;
        }

        /// <summary>
        /// Set entries (secondary) wallet balance=0.0 for all consumers of a given tenant
        /// </summary>
        /// <param name="tenantCode"></param>
        /// <param name="walletTypeId"></param>
        /// <returns></returns>
        public async Task ClearEntriesWalletBalance(string? tenantCode, long walletTypeId)
        {
            using var transaction = _session.BeginTransaction();
            try
            {
                var walletsToUpdate = GetWalletsToUpdate(tenantCode, walletTypeId);

                UpdateWalletsBalance(walletsToUpdate);

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        /// <summary>
        /// Get wallet based on given TenantCode, ConsumerCode and WalletType
        /// </summary>
        /// <param name="tenantCode"></param>
        /// <param name="consumerCode"></param>
        /// <param name="walletTypeId"></param>
        /// <returns></returns>
        public async Task<WalletModel> GetWalletByConsumerAndWalletType(string? tenantCode, string? consumerCode, long walletTypeId)
        {
            var query = from wt in _session.Query<WalletModel>()
                        join cwt in _session.Query<ConsumerWalletModel>() on wt.WalletId equals cwt.WalletId
                        where cwt.ConsumerCode == consumerCode &&
                              wt.WalletTypeId == walletTypeId &&
                              wt.TenantCode == tenantCode &&
                              wt.DeleteNbr == 0 &&
                              wt.ActiveStartTs <= DateTime.UtcNow &&            // while retrieving consumer wallet, ensure it's active
                              wt.ActiveEndTs >= DateTime.UtcNow &&
                              cwt.TenantCode == tenantCode &&
                              cwt.DeleteNbr == 0
                        select wt;

            return await query.FirstOrDefaultAsync();

        }

        private List<WalletModel> GetWalletsToUpdate(string? tenantCode, long walletTypeId)
        {
            var walletsToUpdate = _session.Query<WalletModel>()
                .Join(_session.Query<ConsumerWalletModel>(),
                    wm => new { wm.WalletId, wm.TenantCode },
                    cwm => new { cwm.WalletId, cwm.TenantCode },
                    (wm, cwm) => new { Wallet = wm, ConsumerWallet = cwm })
                .Where(joined => joined.Wallet.TenantCode == tenantCode &&
                    joined.Wallet.WalletTypeId == walletTypeId &&
                    joined.Wallet.DeleteNbr == 0 &&
                    joined.Wallet.ActiveStartTs <= DateTime.UtcNow &&            // while retrieving consumer wallet, ensure it's active
                    joined.Wallet.ActiveEndTs >= DateTime.UtcNow &&
                    joined.ConsumerWallet.DeleteNbr == 0)
                .Select(x => x.Wallet)
                .ToList();

            return walletsToUpdate;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="walletsToUpdate"></param>
        private void UpdateWalletsBalance(List<WalletModel> walletsToUpdate)
        {
            if (walletsToUpdate.Count > 0)
            {
                var walletIdsToUpdate = walletsToUpdate.Select(wt => wt.WalletId);

                _session.Query<WalletModel>()
                   .Where(wm => walletIdsToUpdate.Contains(wm.WalletId))
                   .Update(x => new { balance = 0 });
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="walletModel"></param>
        /// <returns></returns>
        public int UpdateWalletBalance(WalletModel walletModel)
        {

            int rec = _session.Query<WalletModel>()
                   .Where(x => x.WalletId == walletModel.WalletId)
                   .Update(x => new { balance = walletModel.Balance });
            return rec;

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="walletTypeId"></param>
        /// <param name="tenantCode"></param>
        /// <param name="walletName"></param>
        /// <returns></returns>
        public Task<WalletModel> GetSuspenseWallet(long walletTypeId, string tenantCode, string walletName)
        {
            var suspenseWallet = (from wallet in _session.Query<WalletModel>()
                            .Where(x => x.WalletTypeId == walletTypeId && x.TenantCode == tenantCode && x.MasterWallet && x.WalletName == walletName && x.DeleteNbr == 0)
                                  select wallet).FirstOrDefaultAsync();

            return suspenseWallet;
        }
        /// <summary>
        ///  Updates the master wallet balance
        /// </summary>
        /// <param name="walletId"></param>
        /// <param name="masterWalletBalance"></param>
        /// <param name="transactionTs"></param>
        /// <exception cref="InvalidOperationException"></exception>
        public void UpdateMasterWalletBalance(long walletId, double masterWalletBalance, DateTime transactionTs)
        {
            int result = _session.Query<WalletModel>()
                       .Where(x => x.WalletId == walletId && x.DeleteNbr == 0)
                       .Update(x => new { balance = masterWalletBalance, update_ts = transactionTs });

            if (result == 0)
                throw new InvalidOperationException($"Failed to update master wallet balance for WalletId: {walletId}");
        }
        /// <summary>
        ///  Updates the consumer wallet balance
        /// </summary>
        /// <param name="walletId"></param>
        /// <param name="consumerWalletBalance"></param>
        /// <param name="totalEarned"></param>
        /// <param name="transactionTs"></param>
        /// <exception cref="InvalidOperationException"></exception>
        public void UpdateConsumerWalletBalance(long walletId, double consumerWalletBalance, double totalEarned, DateTime transactionTs)
        {
            int result = _session.Query<WalletModel>()
                        .Where(x => x.WalletId == walletId && x.DeleteNbr == 0)
                        .Update(x => new { balance = consumerWalletBalance, total_earned = totalEarned, update_ts = transactionTs });

            if (result == 0)
                throw new InvalidOperationException($"Failed to update  consumer wallet balance for WalletId: {walletId}");
        }
    }
}