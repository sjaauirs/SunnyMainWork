using Microsoft.Extensions.Logging;
using NHibernate;
using NHibernate.Linq;
using SunnyRewards.Helios.ETL.Common.Repositories;
using SunnyRewards.Helios.ETL.Core.Domain.Models;
using SunnyRewards.Helios.ETL.Infrastructure.Repositories.HeliosRepo.Interfaces;
using SunnyRewards.Helios.User.Core.Domain.Models;

namespace SunnyRewards.Helios.ETL.Infrastructure.Repositories.HeliosRepo
{
    public class ConsumerTaskRepo : BaseRepo<ETLConsumerTaskModel>, IConsumerTaskRepo
    {
        private readonly ILogger<BaseRepo<ETLConsumerTaskModel>> _baseLogger;
        private readonly NHibernate.ISession _session;
        private const string _className = nameof(ConsumerTaskRepo);

        public ConsumerTaskRepo(ILogger<BaseRepo<ETLConsumerTaskModel>> baseLogger, ISession session) : base(baseLogger, session)
        {
            _baseLogger = baseLogger;
            _session = session;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="consumerCode"></param>
        /// <param name="taskExternalCode"></param>
        /// <returns></returns>
        public async Task<bool> HasCompletedTask(string consumerCode, string taskExternalCode)
        {
            var query = await (from ct in _session.Query<ETLConsumerTaskModel>()
                               join trm in _session.Query<ETLTaskRewardModel>() on ct.TaskId equals trm.TaskId
                               where trm.TaskExternalCode == taskExternalCode && trm.DeleteNbr == 0 && ct.ConsumerCode == consumerCode
                               && ct.TaskStatus == "COMPLETED"
                               select new
                               {
                                   ConsumerTask = ct,
                                   TaskReward = trm
                               }).ToListAsync();

            return query.Any();
        }

        /// <summary>
        /// Get the count of consumer tasks based on tenant code, task status
        /// </summary>
        /// <param name="tenantCode"></param>
        /// <param name="taskStatus"></param>
        /// <returns></returns>
        public async Task<int> GetConsumerTasksCount(string tenantCode, string taskStatus)
        {
            var query = _session.Query<ETLConsumerTaskModel>()
                .Where(x => x.TenantCode == tenantCode && x.TaskStatus == taskStatus && x.DeleteNbr == 0);

            return await query.CountAsync();
        }

        /// <summary>
        /// Get list of consumer tasks along with their corresponding rewards based on on tenant code, task status
        /// </summary>
        /// <param name="tenantCode"></param>
        /// <param name="taskStatus"></param>
        /// <param name="limit"></param>
        /// <returns></returns>
        public async Task<IList<ETLConsumerTaskRewardModel>> GetConsumerTasksWithRewards(string tenantCode, string taskStatus, int limit)
        {
            var query = (from ct in _session.Query<ETLConsumerTaskModel>()
                         join tr in _session.Query<ETLTaskRewardModel>() on ct.TaskId equals tr.TaskId
                         where ct.TenantCode == tenantCode && ct.TaskStatus == taskStatus && ct.DeleteNbr == 0 && tr.TenantCode == tenantCode
                         orderby ct.TaskId
                         select new ETLConsumerTaskRewardModel()
                         {
                             ConsumerTask = ct,
                             TaskReward = tr
                         });

            return await query.Take(limit).ToListAsync();
        }

        /// <summary>
        /// Get consumer task along with their corresponding rewards based on on tenant code, consumer Task Id, task status
        /// </summary>
        /// <param name="tenantCode"></param>
        /// <param name="consumerTaskId"></param>
        /// <param name="taskStatus"></param>
        /// <returns></returns>
        public async Task<ETLConsumerTaskRewardModel> GetConsumerTaskWithReward(string tenantCode, long consumerTaskId, string taskStatus)
        {
            var query = (from ct in _session.Query<ETLConsumerTaskModel>()
                         join tr in _session.Query<ETLTaskRewardModel>() on ct.TaskId equals tr.TaskId
                         where ct.TenantCode == tenantCode && ct.ConsumerTaskId == consumerTaskId && ct.TaskStatus == taskStatus && ct.DeleteNbr == 0 && tr.DeleteNbr == 0
                         select new ETLConsumerTaskRewardModel()
                         {
                             ConsumerTask = ct,
                             TaskReward = tr
                         });

            return await query.FirstOrDefaultAsync();
        }

        /// <summary>
        /// Get list of child consumer tasks based on tenant code, parentConsumerTaskId, task status
        /// </summary>
        /// <param name="tenantCode"></param>
        /// <param name="parentConsumerTaskId"></param>
        /// <param name="taskStatus"></param>
        /// <returns></returns>
        public async Task<IList<ETLConsumerTaskModel>> GetChildConsumerTasks(string tenantCode, int parentConsumerTaskId, string taskStatus)
        {
            return await _session.Query<ETLConsumerTaskModel>()
                    .Where(x => x.TenantCode == tenantCode && x.ParentConsumerTaskId == parentConsumerTaskId && x.TaskStatus == taskStatus && x.DeleteNbr == 0).ToListAsync();
            
        }

        /// <summary>
        /// Get list of child consumer tasks based on tenant code, parentConsumerTaskId, task status
        /// </summary>
        /// <param name="tenantCode"></param>
        /// <param name="taskStatus"></param>
        /// <returns></returns>
        public async Task<IList<ETLConsumerTaskModel>> GetAllChildConsumerTasks(string tenantCode, string taskStatus)
        {
            return await _session.Query<ETLConsumerTaskModel>()
                    .Where(x => x.TenantCode == tenantCode && x.ParentConsumerTaskId != null && x.TaskStatus == taskStatus && x.DeleteNbr == 0).ToListAsync();

        }
        /// <returns></returns>
        public async Task<ETLConsumerTaskModel?> GetConsumerTask(string consumerCode, string tenantCode, string taskExternalCode)
        {
            var query = await (from ct in _session.Query<ETLConsumerTaskModel>()
                               join trm in _session.Query<ETLTaskRewardModel>() on ct.TaskId equals trm.TaskId
                               where trm.TaskExternalCode == taskExternalCode && trm.DeleteNbr == 0
                               && ct.ConsumerCode == consumerCode && ct.DeleteNbr == 0 && ct.TenantCode == tenantCode
                               select ct).FirstOrDefaultAsync();
            return query;

        }
    }
}
