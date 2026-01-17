using Microsoft.Extensions.Logging;
using NHibernate;
using NHibernate.Linq;
using SunnyRewards.Helios.Common.Core.Repositories;
using SunnyRewards.Helios.Task.Core.Domain.Constants;
using SunnyRewards.Helios.Task.Core.Domain.Dtos;
using SunnyRewards.Helios.Task.Core.Domain.Models;
using SunnyRewards.Helios.Task.Infrastructure.Repositories.Interfaces;
using System.Linq;
using System.Linq.Expressions;

namespace SunnyRewards.Helios.Task.Infrastructure.Repositories
{
    public class ConsumerTaskRepo : BaseRepo<ConsumerTaskModel>, IConsumerTaskRepo
    {
        private readonly ISession _session;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="baseLogger"></param>
        /// <param name="session"></param>
        public ConsumerTaskRepo(ILogger<BaseRepo<ConsumerTaskModel>> baseLogger, NHibernate.ISession session) : base(baseLogger, session)
        {
            _session = session;
        }

        public void FindOneAsync(Expression<Func<ConsumerTaskRewardModel, bool>> expression, bool v)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tenantCode"></param>
        /// <param name="consumerCode"></param>
        /// <param name="taskId"></param>
        /// <returns></returns>
        public async Task<ConsumerTaskRewardModel> GetConsumerTasksWithRewards(string tenantCode, string consumerCode, long taskId)
        {
            var query = (from ct in _session.Query<ConsumerTaskModel>()

                         join tr in _session.Query<TaskRewardModel>() on ct.TaskId equals tr.TaskId

                         where ct.TenantCode == tenantCode && ct.ConsumerCode == consumerCode && ct.TaskId == taskId
                           && ct.DeleteNbr == 0 && tr.TenantCode == tenantCode

                         orderby ct.ConsumerTaskId descending

                         select new ConsumerTaskRewardModel()
                         {
                             ConsumerTask = ct,

                             TaskReward = tr
                         });

            return await query.FirstOrDefaultAsync();
        }

        /// <summary>
        /// Get consumer task along with their corresponding rewards based on on tenant code, consumer Task Id, task status
        /// </summary>
        /// <param name="tenantCode"></param>
        /// <param name="consumerTaskId"></param>
        /// <param name="taskStatus"></param>
        /// <returns></returns>
        public async Task<ConsumerTaskRewardModel> GetConsumerTaskWithReward(string tenantCode, long consumerTaskId, string taskStatus)
        {
            var query = (from ct in _session.Query<ConsumerTaskModel>()
                         join tr in _session.Query<TaskRewardModel>() on ct.TaskId equals tr.TaskId
                         where ct.TenantCode == tenantCode && ct.ConsumerTaskId == consumerTaskId && ct.TaskStatus == taskStatus && ct.DeleteNbr == 0 && tr.DeleteNbr == 0
                         orderby ct.ConsumerTaskId descending
                         select new ConsumerTaskRewardModel()
                         {
                             ConsumerTask = ct,
                             TaskReward = tr
                         });

            return await query.FirstOrDefaultAsync();
        }


        public async Task<PageinatedCompletedConsumerTaskDto> GetPaginatedConsumerTask (GetConsumerTaskByTaskId getConsumerTaskByTaskId)
        {

            var query = from j in _session.Query<ConsumerTaskModel>()
                        where j.TaskId == getConsumerTaskByTaskId.TaskId
                        && j.TenantCode == getConsumerTaskByTaskId.TenantCode
                        && j.DeleteNbr == 0
                        && j.TaskStatus == Constant.CompletedTaskStatus
                        && j.TaskCompleteTs >= getConsumerTaskByTaskId.StartDate
                        && j.TaskCompleteTs <= getConsumerTaskByTaskId.EndDate
                        select j;

            int totalRecords = await query.CountAsync();

            query = query.OrderByDescending(q => q.ConsumerTaskId)
                             .Skip(getConsumerTaskByTaskId.Skip)
                             .Take(getConsumerTaskByTaskId.PageSize);

            return new PageinatedCompletedConsumerTaskDto
            {
                TotalRecords = totalRecords,
                CompletedTasks = await query.ToListAsync()
            };
        }
    }
}
