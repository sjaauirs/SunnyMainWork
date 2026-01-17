using FluentNHibernate.Testing.Values;
using SunnyRewards.Helios.Common.Core.Repositories.Interfaces;
using SunnyRewards.Helios.Wallet.Core.Domain.Dtos;
using SunnyRewards.Helios.Wallet.Core.Domain.Models;

namespace SunnyRewards.Helios.Wallet.Infrastructure.Repositories.Interfaces
{
    public interface ITransactionRepo : IBaseRepo<TransactionModel>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="walletId"></param>
        /// <returns></returns>
        Task<long> GetMaxTransactionIdByWallet(long walletId);

        Task<double> GetTotalAmountForConsumerByTransactionDetailType(string consumerCode, long walletId, string transactionDetailType);

        Task<List<TransactionModel>> GetConsumerWalletTransactions(string consumerCode);
        IQueryable<TransactionEntryDto> GetWalletTransactionsQueryable(List<long> walletIds);
        /// <summary>
        /// This will return the transactions for the list of walletId's and count is optional here if count 0 returning all the  transactions
        /// </summary>
        /// <param name="walletId">List of walletIds</param>
        /// <param name="count">optional transaction count</param>
        /// <returns></returns>
        Task<IList<TransactionModel>> GetConsumerWalletTopTransactions(List<long> walletId, int? count = 0, List<string>? skipTransactionType = null);
    }
}


