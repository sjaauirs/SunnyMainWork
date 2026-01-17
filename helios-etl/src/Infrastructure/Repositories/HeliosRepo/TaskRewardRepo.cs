using SunnyRewards.Helios.ETL.Common.Repositories;
using SunnyRewards.Helios.ETL.Core.Domain.Models;
using SunnyRewards.Helios.ETL.Infrastructure.Repositories.HeliosRepo.Interfaces;
using Microsoft.Extensions.Logging;
using SunnyRewards.Helios.Tenant.Core.Domain.Models;
using NHibernate.Linq;

namespace SunnyRewards.Helios.ETL.Infrastructure.Repositories.HeliosRepo
{
    public class TaskRewardRepo : BaseRepo<ETLTaskRewardModel>, ITaskRewardRepo
    {
        private readonly ILogger<BaseRepo<ETLTaskRewardModel>> _logger;
        private readonly NHibernate.ISession _session;

        private const string _className = nameof(TaskRewardRepo);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="session"></param>
        public TaskRewardRepo(ILogger<BaseRepo<ETLTaskRewardModel>> logger, NHibernate.ISession session) : base(logger, session)
        {
            _logger = logger;
            _session = session;
        }

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