using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.Task.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Admin.Infrastructure.Services.Interfaces
{
    public interface ITaskRewardService
    {
        Task<BaseResponseDto> CreateTaskReward(CreateTaskRewardRequestDto createTaskRewardRequestDto);

        /// <summary>
        /// UpdateTaskRewardAsync
        /// </summary>
        /// <param name="taskRewardId"></param>
        /// <param name="taskRewardRequestDto"></param>
        /// <returns></returns>
        Task<TaskRewardResponseDto> UpdateTaskRewardAsync(long taskRewardId, TaskRewardRequestDto taskRewardRequestDto);

        Task<GetTasksAndTaskRewardsResponseDto> GetTasksAndTaskRewards(GetTasksAndTaskRewardsRequestDto getTaskRewardsRequestDto);
        /// <summary>
        /// Retrieves tasks and task rewards associated with a given tenant code.
        /// </summary>
        /// <param name="tenantCode">The tenant code for which tasks and rewards are to be retrieved.</param>
        /// <returns>An <see cref="IActionResult"/> containing the response data or an error message if the operation fails.</returns>
        Task<TaskRewardDetailsResponseDto> GetTaskRewardDetails(string tenantCode, string? languageCode);

        /// <summary>
        /// Retrieves tasks and health task rewards associated with a given tenant code.
        /// </summary>
        /// <param name="tenantCode">The tenant code for which tasks and rewards are to be retrieved.</param>
        /// <returns>An <see cref="IActionResult"/> containing the response data or an error message if the operation fails.</returns>
        Task<TaskRewardsResponseDto> GetHealthTaskRewards(string tenantCode);
    }
}
