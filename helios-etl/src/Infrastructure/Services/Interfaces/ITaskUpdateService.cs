using SunnyRewards.Helios.ETL.Core.Domain.Dtos;

namespace SunnyRewards.Helios.ETL.Infrastructure.Services.Interfaces
{
    public interface ITaskUpdateService
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="taskUpdatefilePath"></param>
        Task ProcessTaskUpdates(string taskUpdatefilePath = "", byte[]? taskUpdateFileContent = null, EtlExecutionContext? etlExecutionContext = null);

        /// <summary>
        /// Soft delete expired consumer tasks which are in pending state, cleanup recurring tasks progress as recurrence definition
        /// </summary>
        /// <param name="etlExecutionContext"></param>
        /// <returns></returns>
        Task ProcessRecurringTasks(EtlExecutionContext etlExecutionContext);

        Task<ConsumerTaskUpdateResponseDto> UpdateTaskAsCompleted(TaskUpdateRequestDto taskUpdateRequestDto);
    }
}
