extern alias SunnyRewards_Task;
using SunnyRewards.Helios.ETL.Core.Domain.Dtos;
using SunnyRewards_Task::SunnyRewards.Helios.Task.Core.Domain.Dtos;

namespace SunnyRewards.Helios.ETL.Infrastructure.Services.Interfaces
{
    public interface IProcessRecurringTasksService
    {
        Task RecurringTaskCreationProcess(string tenantCode);
        Task<ConsumerTaskResponseUpdateDto> CreateConsumerTask(CreateConsumerTaskDto requestDto);
    }
}