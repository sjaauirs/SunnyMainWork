using SunnyRewards.Helios.Task.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Admin.Infrastructure.Services.Interfaces
{
    public interface ITaskTypeService
    {
        /// <summary>
        /// Retrieves a list of task types from the repository and returns them in a standardized response format.
        /// </summary>
        /// <returns></returns>
        Task<TaskTypesResponseDto> GetTaskTypesAsync();
    }
}