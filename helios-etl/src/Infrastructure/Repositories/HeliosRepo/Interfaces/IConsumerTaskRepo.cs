using SunnyRewards.Helios.ETL.Common.Repositories.Interfaces;
using SunnyRewards.Helios.ETL.Core.Domain.Models;

namespace SunnyRewards.Helios.ETL.Infrastructure.Repositories.HeliosRepo.Interfaces
{
    public interface IConsumerTaskRepo : IBaseRepo<ETLConsumerTaskModel>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="consumerCode"></param>
        /// <param name="taskExternalCode"></param>
        /// <returns></returns>
        Task<bool> HasCompletedTask(string consumerCode, string taskExternalCode);

        /// <summary>
        /// Get the count of consumer tasks based on tenant code, task status
        /// </summary>
        /// <param name="tenantCode"></param>
        /// <param name="taskStatus"></param>
        /// <returns></returns>
        Task<int> GetConsumerTasksCount(string tenantCode, string taskStatus);

        /// <summary>
        /// Get list of consumer tasks along with their corresponding rewards based on on tenant code, task status
        /// </summary>
        /// <param name="tenantCode"></param>
        /// <param name="taskStatus"></param>
        /// <param name="limit"></param>
        /// <returns></returns>
        Task<IList<ETLConsumerTaskRewardModel>> GetConsumerTasksWithRewards(string tenantCode, string taskStatus, int limit);

        /// <summary>
        /// Get consumer task along with their corresponding rewards based on on tenant code, consumer Task Id, task status
        /// </summary>
        /// <param name="tenantCode"></param>
        /// <param name="consumerTaskId"></param>
        /// <param name="taskStatus"></param>
        /// <returns></returns>
        Task<ETLConsumerTaskRewardModel> GetConsumerTaskWithReward(string tenantCode, long consumerTaskId, string taskStatus);

        /// <summary>
        /// Get list of child consumer tasks based on tenant code, parentConsumerTaskId, task status
        /// </summary>
        /// <param name="tenantCode"></param>
        /// <param name="parentConsumerTaskId"></param>
        /// <param name="taskStatus"></param>
        /// <returns></returns>
        Task<IList<ETLConsumerTaskModel>> GetChildConsumerTasks(string tenantCode, int parentConsumerTaskId, string taskStatus);

        /// <summary>
        /// Get list of child consumer tasks based on tenant code, parentConsumerTaskId, task status
        /// </summary>
        /// <param name="tenantCode"></param>
        /// <param name="taskStatus"></param>
        /// <returns></returns>
        Task<IList<ETLConsumerTaskModel>> GetAllChildConsumerTasks(string tenantCode, string taskStatus);
        Task<ETLConsumerTaskModel?> GetConsumerTask(string consumerCode , string tenantCode, string taskExternalCode);    }
}
