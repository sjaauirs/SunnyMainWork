using SunnyRewards.Helios.Task.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Admin.Infrastructure.Services.Interfaces
{
    public interface ITaskCategoryService
    {
        /// <summary>
        /// Retrieves a list of tasks from the TaskCategory API and returns them in a standardized response format.
        /// </summary>
        /// <returns></returns>
        Task<TaskCategoriesResponseDto> GetTaskCategoriesAsync();
    }
}
