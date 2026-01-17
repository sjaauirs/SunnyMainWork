using SunnyRewards.Helios.Task.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Task.Infrastructure.Services.Interface
{
    public interface ITaskCategoryService
    {
        /// <summary>
        /// Retrieves a list of task categories from the repository and returns them in a standardized response format.
        /// </summary>
        /// <returns></returns>
        Task<TaskCategoriesResponseDto> GetTaskCategoriesAsync();
        /// <summary>
        /// Imports a list of task category types from the request DTO. 
        /// </summary>
        /// <param name="taskTypeRequestDto">The import request containing task types.</param>
        /// <returns>Import result with error messages if any failures occur.</returns>
        Task<ImportTaskCategoryResponseDto> ImportTaskCategoriesAsync(ImportTaskCategoryRequestDto taskCategoryRequestDto);
    }
}
