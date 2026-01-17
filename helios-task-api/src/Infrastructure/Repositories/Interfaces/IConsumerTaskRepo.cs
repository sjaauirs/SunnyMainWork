using SunnyRewards.Helios.Common.Core.Repositories.Interfaces;
using SunnyRewards.Helios.Task.Core.Domain.Dtos;
using SunnyRewards.Helios.Task.Core.Domain.Models;

namespace SunnyRewards.Helios.Task.Infrastructure.Repositories.Interfaces
{
    public interface IConsumerTaskRepo : IBaseRepo<ConsumerTaskModel>
    {
        Task<ConsumerTaskRewardModel> GetConsumerTasksWithRewards(string tenantCode, string consumerCode, long taskId);

        /// <summary>
        /// Get consumer task along with their corresponding rewards based on on tenant code, consumer Task Id, task status
        /// </summary>
        /// <param name="tenantCode"></param>
        /// <param name="consumerTaskId"></param>
        /// <param name="taskStatus"></param>
        /// <returns></returns>
        Task<ConsumerTaskRewardModel> GetConsumerTaskWithReward(string tenantCode, long consumerTaskId, string taskStatus);

        Task<PageinatedCompletedConsumerTaskDto> GetPaginatedConsumerTask(GetConsumerTaskByTaskId getConsumerTaskByTaskId);
    }
}
