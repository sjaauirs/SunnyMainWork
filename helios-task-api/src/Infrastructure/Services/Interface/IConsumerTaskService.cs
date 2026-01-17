using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.Task.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Task.Infrastructure.Services.Interface
{
    public interface IConsumerTaskService
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="findConsumerTasksByIdRequestDto"></param>
        /// <returns></returns>
        Task<FindConsumerTasksByIdResponseDto> GetConsumerTask(FindConsumerTasksByIdRequestDto findConsumerTasksByIdRequestDto);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="consumerTaskRequestDto"></param>
        /// <returns></returns>
        Task<FindConsumerTaskResponseDto> GetConsumerTasks(FindConsumerTaskRequestDto consumerTaskRequestDto);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="consumerTaskDto"></param>
        /// <returns></returns>
        Task<ConsumerTaskResponseUpdateDto> CreateConsumerTasks(ConsumerTaskDto consumerTaskDto);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="consumerTaskDto"></param>
        /// <returns></returns>
        Task<ConsumerTaskDto> UpdateConsumerTask(UpdateConsumerTaskDto consumerTaskDto);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="consumerTaskDto"></param>
        /// <returns></returns>
        Task<BaseResponseDto> UpdateConsumerTaskDetails(ConsumerTaskDto consumerTaskDto);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="consumerTaskRequestDto"></param>
        /// <returns></returns>
        Task<ConsumerTaskResponseDto> GetAllConsumerTask(ConsumerTaskRequestDto consumerTaskRequestDto);

        /// <summary>
        /// Revert all consumer tasks for given consumer
        /// </summary>
        /// <param name="revertAllConsumerTasksRequestDto"></param>
        /// <returns></returns>
        Task<BaseResponseDto> RevertAllConsumerTasks(RevertAllConsumerTasksRequestDto revertAllConsumerTasksRequestDto);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rewardTypeConsumerTaskRequestDto"></param>
        /// <returns></returns>
        Task<ConsumerTaskResponseDto> GetAvailableTaskRewardType(GetRewardTypeConsumerTaskRequestDto rewardTypeConsumerTaskRequestDto);
        /// <summary>
        /// Removes a specific consumer task based on the provided request details.
        /// </summary>
        /// <param name="deleteConsumerTaskRequestDto">The request DTO containing consumer and task details for the task to be removed.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a <see cref="BaseResponseDto"/> 
        /// indicating the success or failure of the removal operation.</returns>
        Task<BaseResponseDto> RemoveConsumerTask(DeleteConsumerTaskRequestDto deleteConsumerTaskRequestDto);

        /// <summary>
        ///  get all consumers who completed a task by TaskId within a date range
        /// </summary>
        /// <param name="getConsumerTaskByTaskId"></param>
        /// <returns></returns>
        Task<PageinatedCompletedConsumerTaskResponseDto> GetConsumersByTaskId(GetConsumerTaskByTaskId getConsumerTaskByTaskId);

        Task<ConsumerHealthTaskResponseUpdateDto> UpdateHealthTaskProgress(UpdateHealthTaskProgressRequestDto request);
    }
}

