using Microsoft.Extensions.Logging;
using SunnyRewards.Helios.Common.Core.Repositories;
using SunnyRewards.Helios.Wallet.Core.Domain.Models;
using SunnyRewards.Helios.Wallet.Infrastructure.Repositories.Interfaces;

namespace SunnyRewards.Helios.Wallet.Infrastructure.Repositories
{
    public class TransactionDetailRepo : BaseRepo<TransactionDetailModel>, ITransactionDetailRepo
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="baseLogger"></param>
        /// <param name="session"></param>
        public TransactionDetailRepo(ILogger<BaseRepo<TransactionDetailModel>> baseLogger, NHibernate.ISession session) : base(baseLogger, session)
        {
        }
    }
}

