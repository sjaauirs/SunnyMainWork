using SunnyRewards.Helios.ETL.Core.Domain.Dtos;

namespace SunnyRewards.Helios.ETL.Infrastructure.Services.Interfaces
{
    public interface IProcessCompletedConsumerTask
    {
        Task ProcessCompletedConsumerTasksAsync(EtlExecutionContext etlExecutionContext);
    }
}
