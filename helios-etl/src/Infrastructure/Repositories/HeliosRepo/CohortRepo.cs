using SunnyRewards.Helios.ETL.Common.Repositories;
using SunnyRewards.Helios.ETL.Core.Domain.Models;
using SunnyRewards.Helios.ETL.Infrastructure.Repositories.HeliosRepo.Interfaces;
using Microsoft.Extensions.Logging;
using NHibernate;
using NHibernate.Linq;

namespace SunnyRewards.Helios.ETL.Infrastructure.Repositories.HeliosRepo
{
    public class CohortRepo : BaseRepo<ETLCohortModel>, ICohortRepo
    {
        private readonly ISession _session;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="baseLogger"></param>
        /// <param name="session"></param>
        public CohortRepo(ILogger<BaseRepo<ETLCohortModel>> baseLogger, NHibernate.ISession session) : base(baseLogger, session)
        {
            _session = session;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="consumerCode"></param>
        /// <param name="cohortName"></param>
        /// <returns></returns>
        public async Task<bool> IsInCohort(string consumerCode, string cohortName)
        {
            var query = await (from cc in _session.Query<ETLCohortConsumerModel>()
                               join cm in _session.Query<ETLCohortModel>() on cc.CohortId equals cm.CohortId
                               where cc.ConsumerCode == consumerCode && cc.DeleteNbr == 0 && cm.CohortName == cohortName
                               select new
                               {
                                   ConsumerTask = cc,
                                   TaskReward = cm
                               }).ToListAsync();

            return query.Any();
        }
    }
}