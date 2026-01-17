using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.Task.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Admin.Infrastructure.Services.Interfaces
{
    public interface ITaskService
    {
        /// <summary>
        /// Retrieves a list of tasks from the tasks API and returns them in a standardized response format.
        /// </summary>
        /// <returns></returns>
        Task<TasksResponseDto> GetTasksAsync();

        Task<BaseResponseDto> CreateTask(CreateTaskRequestDto createTaskRequestDto);

        /// <summary>
        /// UpdateTaskAsync
        /// </summary>
        /// <param name="taskId"></param>
        /// <param name="taskRequestDto"></param>
        /// <returns></returns>
        Task<TaskResponseDto> UpdateTaskAsync(long taskId, TaskRequestDto taskRequestDto);

        /// <summary>
        /// Method to call Task API remove-consumer-task for soft delete task 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        BaseResponseDto SoftDeleteTask(dynamic request);

        Task<GetTaskByTaskNameResponseDto> GetTaskByTaskName(GetTaskByTaskNameRequestDto getTaskByTaskNameRequestDto);

        Task<ImportTaskTypeResponseDto> ImportTaskTypes(List<TaskTypeDto> taskTypes);
        Task<ImportTaskCategoryResponseDto> ImportTaskCategories(List<TaskCategoryDto> taskCategories);
        Task<ImportRewardTypeResponseDto> ImportTaskRewardTypes(List<TaskRewardTypeDto> taskRewardTypes);


    }
}
