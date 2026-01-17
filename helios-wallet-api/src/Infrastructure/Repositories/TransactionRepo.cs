using AutoMapper;
using Microsoft.Extensions.Logging;
using NHibernate.Criterion;
using NHibernate.Linq;
using SunnyRewards.Helios.Common.Core.Repositories;
using SunnyRewards.Helios.Wallet.Core.Domain.Dtos;
using SunnyRewards.Helios.Wallet.Core.Domain.Models;
using SunnyRewards.Helios.Wallet.Infrastructure.Repositories.Interfaces;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace SunnyRewards.Helios.Wallet.Infrastructure.Repositories
{
    [ExcludeFromCodeCoverage]
    public class TransactionRepo : BaseRepo<TransactionModel>, ITransactionRepo
    {
        private readonly NHibernate.ISession _session;
        private readonly IMapper _mapper;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="baseLogger"></param>
        /// <param name="session"></param>
        public TransactionRepo(ILogger<BaseRepo<TransactionModel>> baseLogger, NHibernate.ISession session, IMapper mapper) : base(baseLogger, session)
        {
            _session = session;
            _mapper = mapper;
        }

        public async Task<long> GetMaxTransactionIdByWallet(long walletId)
        {
            var maxTransactionId = _session.QueryOver<TransactionModel>().Where(x => x.WalletId == walletId && x.DeleteNbr == 0)
                       .Select(Projections.Max<TransactionModel>(x => x.TransactionId)).SingleOrDefault<long>();

            return await Task.FromResult(maxTransactionId);

        }

        public async Task<double> GetTotalAmountForConsumerByTransactionDetailType(string consumerCode, long walletId, string transactionDetailType)
        {
            var transactionAmountSum = await (from t in _session.Query<TransactionModel>()
                                              join td in _session.Query<TransactionDetailModel>() on t.TransactionDetailId equals td.TransactionDetailId
                                              where td.ConsumerCode == consumerCode &&
                                                    td.TransactionDetailType == transactionDetailType &&
                                                    td.DeleteNbr == 0 &&
                                                    t.WalletId == walletId &&
                                                    t.DeleteNbr == 0
                                              select t.TransactionAmount)
                                        .SumAsync();

            return transactionAmountSum ?? 0;
        }

        public async Task<List<TransactionModel>> GetConsumerWalletTransactions(string consumerCode)
        {
            var transactions = await (from t in _session.Query<TransactionModel>()
                                      join td in _session.Query<TransactionDetailModel>() on t.TransactionDetailId equals td.TransactionDetailId
                                      where td.ConsumerCode == consumerCode &&
                                            td.DeleteNbr == 0 &&
                                            t.DeleteNbr == 0
                                      select t).ToListAsync();
            return transactions;
        }

        public async Task<IList<TransactionModel>> GetConsumerWalletTopTransactions(
     List<long> walletIds,
     int? count = 0,
     List<string>? skipTransactionType = null)
        {
            // Step 1: Query without skipTransactionType OR count
            var query =
                from t in _session.Query<TransactionModel>()
                join td in _session.Query<TransactionDetailModel>()
                    on t.TransactionDetailId equals td.TransactionDetailId
                where walletIds.Contains(t.WalletId)
                      && t.DeleteNbr == 0
                orderby t.TransactionId descending
                select new { t, td.Notes };

            // Fetch ALL matching rows
            var result = await query.ToListAsync();

            // Step 2: Apply skip logic safely (in-memory)
            if (skipTransactionType != null && skipTransactionType.Any())
            {
                var skipList = skipTransactionType
                    .Select(x => x.Trim().ToLower())
                    .ToList();

                result = result
                    .Where(x =>
                        x.Notes == null ||                       // always include rows with null Notes
                        !skipList.Any(s =>
                            (x.Notes ?? "").ToLower().Contains(s)
                        )
                    )
                    .ToList();
            }

            // Step 3: Apply count at the END
            if (count.HasValue && count.Value > 0)
            {
                result = result.Take(count.Value).ToList();
            }

            // Return only TransactionModel
            return result.Select(x => x.t).ToList();
        }





        public IQueryable<TransactionEntryDto> GetWalletTransactionsQueryable(List<long> walletIds)
        {
            var transactions = (from t in _session.Query<TransactionModel>()
                          join td in _session.Query<TransactionDetailModel>() on t.TransactionDetailId equals td.TransactionDetailId
                          join w in _session.Query<WalletModel>() on t.WalletId equals w.WalletId
                          join wt in _session.Query<WalletTypeModel>() on w.WalletTypeId equals wt.WalletTypeId
                          where walletIds.Contains(t.WalletId) && t.DeleteNbr == 0 && td.DeleteNbr == 0
                          orderby t.TransactionId descending
                          select new TransactionEntryDto()
                          {
                              Transaction = _mapper.Map<TransactionDto>(t),
                              TransactionDetail = _mapper.Map<TransactionDetailDto>(td),
                              TransactionWalletType = _mapper.Map<WalletTypeDto>(wt)
                          });

            return transactions;
        }
    }
}

