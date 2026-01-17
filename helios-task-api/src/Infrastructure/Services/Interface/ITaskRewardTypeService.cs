using SunnyRewards.Helios.Task.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Task.Infrastructure.Services.Interface
{
    public interface ITaskRewardTypeService
    {
        /// <summary>
        /// Retrieves a list of reward types from the repository and returns them in a standardized response format.
        /// </summary>
        /// <returns></returns>
        Task<TaskRewardTypesResponseDto> GetTaskRewardTypesAsync();

        /// <summary>
        /// UpdateTaskRewardTypeAsync
        /// </summary>
        /// <param name="rewardTypeId"></param>
        /// <param name="taskRewardTypeRequestDto"></param>
        /// <returns></returns>
        Task<TaskRewardTypeResponseDto> UpdateTaskRewardTypeAsync(long rewardTypeId, TaskRewardTypeRequestDto taskRewardTypeRequestDto);
        /// <summary>
        /// Imports a list of reward types from the request DTO. 
        /// </summary>
        /// <param name="taskTypeRequestDto">The import request containing task types.</param>
        /// <returns>Import result with error messages if any failures occur.</returns>
        Task<ImportRewardTypeResponseDto> ImportRewardTypesAsync(ImportRewardTypeRequestDto rewardTypeRequestDto);
    }
}
