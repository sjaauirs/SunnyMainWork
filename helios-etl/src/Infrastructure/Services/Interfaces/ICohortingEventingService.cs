using SunnyRewards.Helios.ETL.Core.Domain.Dtos;

namespace SunnyRewards.Helios.ETL.Infrastructure.Services.Interfaces
{
    public interface ICohortingEventingService
    {
        Task CohortingEventingAsync(EtlExecutionContext etlExecutionContext, string jobId);
    }
}
