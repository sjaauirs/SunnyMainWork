using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.Task.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Task.Infrastructure.Services.Interface
{
    public interface ITaskService
    {
        /// <summary>
        /// Retrieves a list of tasks from the repository and returns them in a standardized response format.
        /// </summary>
        /// <returns></returns>
        Task<TasksResponseDto> GetTasksAsync();

        /// <summary>
        /// UpdateTaskAsync
        /// </summary>
        /// <param name="taskId"></param>
        /// <param name="taskRequestDto"></param>
        /// <returns></returns>
        Task<TaskResponseDto> UpdateTaskAsync(long taskId, TaskRequestDto taskRequestDto);
        Task<TaskResponseDto> UpdateImportTaskAsync(long taskId, TaskRequestDto taskRequestDto);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="taskId"></param>
        /// <returns></returns>
        Task<TaskDto> GetTaskData(long taskId);

        // <summary>
        /// 
        /// </summary>
        /// <param name="taskRewardRequestDto"></param>
        /// <returns></returns>
        Task<GetTaskRewardResponseDto> GetTasksByTaskRewardCode(GetTaskRewardRequestDto taskRewardRequestDto);

        /// <summary>
        /// Gets the task export.
        /// </summary>
        /// <param name="exportTaskRequestDto">The export task request dto.</param>
        /// <returns></returns>
        Task<ExportTaskResponseDto> GetTaskExport(ExportTaskRequestDto exportTaskRequestDto);

        /// <summary>
        /// Creates the Task.
        /// </summary>
        /// <param name="createTaskRequest">The create Task request.</param>
        /// <returns></returns>
        Task<BaseResponseDto> CreateTask(CreateTaskRequestDto taskRequestDto);

        Task<BaseResponseDto> CreateTaskExternalMapping(TaskExternalMappingRequestDto requestDto);

        /// <summary>
        /// Retrieves a task by its task name from the repository.
        /// </summary>
        /// <param name="getTaskByTaskNameRequestDto">The DTO containing the task name used to fetch the task.</param>
        /// <returns>
        /// A <see cref="GetTaskByTaskNameResponseDto"/> containing the <see cref="TaskDto"/> if the task is found; 
        /// otherwise, returns a <see cref="GetTaskByTaskNameResponseDto"/> with an error code and message indicating "Not Found."
        /// </returns>
        Task<GetTaskByTaskNameResponseDto> GetTaskByTaskName(GetTaskByTaskNameRequestDto getTaskByTaskNameRequestDto);
    }
}
