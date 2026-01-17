using Microsoft.Extensions.Logging;
using NHibernate.Criterion;
using SunnyRewards.Helios.ETL.Common.Repositories;
using SunnyRewards.Helios.ETL.Core.Domain.Models;
using SunnyRewards.Helios.ETL.Infrastructure.Repositories.HeliosRepo.Interfaces;

namespace SunnyRewards.Helios.ETL.Infrastructure.Repositories.HeliosRepo
{
    public class TransactionRepo : BaseRepo<ETLTransactionModel>, ITransactionRepo
    {
        private readonly NHibernate.ISession _session;
        public TransactionRepo(ILogger<BaseRepo<ETLTransactionModel>> baseLogger, NHibernate.ISession session) :
           base(baseLogger, session)
        {
            _session = session;
        }

        public async Task<long> GetMaxTransactionIdByWallet(long walletId)
        {
            var maxTransactionId = _session.QueryOver<ETLTransactionModel>().Where(x => x.WalletId == walletId && x.DeleteNbr == 0)
                       .Select(Projections.Max<ETLTransactionModel>(x => x.TransactionId)).SingleOrDefault<long>();

            return await Task.FromResult(maxTransactionId);

        }
    }


}
