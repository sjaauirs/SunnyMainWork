using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.Task.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Task.Infrastructure.Services.Interface
{
    public interface ITaskDetailsService
    {
        /// <summary>
        ///  Creates Task Details
        /// </summary>
        /// <param name="createTaskDetailsRequestDto">Request for creating task details</param>
        /// <returns></returns>
        Task<BaseResponseDto> CreateTaskDetails(CreateTaskDetailsRequestDto createTaskDetailsRequestDto);

        /// <summary>
        /// UpdateTaskDetailAsync
        /// </summary>
        /// <param name="taskDetailId"></param>
        /// <param name="taskDetailRequestDto"></param>
        /// <returns></returns>
        Task<TaskDetailResponseDto> UpdateTaskDetailAsync(long taskDetailId, TaskDetailRequestDto taskDetailRequestDto);
    }
}
