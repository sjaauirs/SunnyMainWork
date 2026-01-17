using SunnyBenefits.Fis.Core.Domain.Dtos;
using SunnyRewards.Helios.Admin.Core.Domain.Dtos;
using SunnyRewards.Helios.Task.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Admin.Infrastructure.Services.Interfaces
{
    public interface IConsumerTaskService
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="taskUpdateRequestDto"></param>
        /// <returns></returns>
        Task<ConsumerTaskUpdateResponseDto> UpdateConsumerTask(TaskUpdateRequestDto taskUpdateRequestDto);
        Task<ConsumerTaskResponseUpdateDto> PostConsumerTasks(CreateConsumerTaskDto requestDto);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="subtaskUpdateRequestDto"></param>
        /// <returns></returns>
        Task<UpdateSubtaskResponseDto> UpdateCompleteSubtask(SubtaskUpdateRequestDto subtaskUpdateRequestDto);
        Task<bool?> GetTenantByTenantCode(string tenantCode);


        /// <summary>
        /// Api to get Recurring task of a consumer on a timestamp
        /// </summary>
        /// <param name="availableRecurringTasksRequestDto"></param>
        /// <returns></returns>
        Task<AvailableRecurringTaskResponseDto> GetAvailableRecurringTask(AvailableRecurringTasksRequestDto availableRecurringTasksRequestDto);

        /// <summary>
        /// get Consumers and person detail by consumer codes
        /// </summary>
        /// <param name="requestDto"></param>
        /// <returns></returns>
        Task<ConsumersByTaskIdResponseDto> GetConsumersByCompletedTask(GetConsumerTaskByTaskId requestDto);

        /// <summary>
        /// Verifies whether the consumer account exists and has a valid proxy number for the specified request.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<bool> IsValidConsumerAccount(GetConsumerAccountRequestDto request);
    }
}
