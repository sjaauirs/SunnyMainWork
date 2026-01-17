using Microsoft.Extensions.Logging;
using SunnyRewards.Helios.ETL.Common.Repositories;
using SunnyRewards.Helios.ETL.Core.Domain.Models;
using SunnyRewards.Helios.ETL.Infrastructure.Repositories.HeliosRepo.Interfaces;

namespace SunnyRewards.Helios.ETL.Infrastructure.Repositories.HeliosRepo
{
    public class MonetaryTransactionRepo : BaseRepo<ETLMonetaryTransactionModel>, IMonetaryTransactionRepo
    {
        

        public MonetaryTransactionRepo(ILogger<BaseRepo<ETLMonetaryTransactionModel>> baseLogger, NHibernate.ISession session) : base(baseLogger, session)
        {
           
        }

        

    }
}

