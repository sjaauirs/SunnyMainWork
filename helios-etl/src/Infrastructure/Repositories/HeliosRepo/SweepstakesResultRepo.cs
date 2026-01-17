using SunnyRewards.Helios.ETL.Common.Repositories;
using SunnyRewards.Helios.ETL.Core.Domain.Models;
using Microsoft.Extensions.Logging;
using SunnyRewards.Helios.ETL.Infrastructure.Repositories.HeliosRepo.Interfaces;

namespace SunnyRewards.Helios.ETL.Infrastructure.Repositories.HeliosRepo
{
    public class SweepstakesResultRepo : BaseRepo<ETLSweepstakesResultModel>, ISweepstakesResultRepo
    {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="baseLogger"></param>
        /// <param name="session"></param>
        public SweepstakesResultRepo(ILogger<BaseRepo<ETLSweepstakesResultModel>> baseLogger, NHibernate.ISession session) : base(baseLogger, session)
        {
        }
    }
}