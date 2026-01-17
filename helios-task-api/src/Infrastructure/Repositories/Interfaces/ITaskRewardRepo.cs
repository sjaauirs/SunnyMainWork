using SunnyRewards.Helios.Common.Core.Repositories.Interfaces;
using SunnyRewards.Helios.Task.Core.Domain.Dtos;
using SunnyRewards.Helios.Task.Core.Domain.Models;

namespace SunnyRewards.Helios.Task.Infrastructure.Repositories.Interfaces
{
    public interface ITaskRewardRepo : IBaseRepo<TaskRewardModel>
    {
        Task<List<TaskAndTaskRewardModel>> GetTasksAndTaskRewards(GetTasksAndTaskRewardsRequestDto requestDto);

        /// <summary>
        ///  Retrieves the list of task reward details for a given tenant code.
        /// </summary>
        /// <param name="tenantCode"></param>
        /// <returns></returns>
        Task<List<TaskRewardDetailModel>> GetTaskRewardDetails(string tenantCode, string? taskExternalCode, string languageCode);
        List<TaskRewardDetailDto> GetTaskRewardDetailsList(string tenantCode, string languageCode, List<long> taskRewardIds);
        List<TaskRewardDetailDto> GetTaskRewardDetailsList(string tenantCode, string languageCode);
        Task<List<TaskRewardModel>> GetSelfReportTaskRewards(GetSelfReportTaskReward requestDto);
    }
}
