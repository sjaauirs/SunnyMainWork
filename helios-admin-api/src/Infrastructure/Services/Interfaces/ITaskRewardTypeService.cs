using SunnyRewards.Helios.Task.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Admin.Infrastructure.Services.Interfaces
{
    public interface ITaskRewardTypeService
    {
        /// <summary>
        /// Retrieves a list of reward types from TaskRewardType API and returns them in a standardized response format.
        /// </summary>
        /// <returns></returns>
        Task<TaskRewardTypesResponseDto> GetTaskRewardTypesAsync();
    }
}
