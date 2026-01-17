using SunnyRewards.Helios.ETL.Core.Domain.Dtos;

namespace SunnyRewards.Helios.ETL.Infrastructure.Services.Interfaces
{
    public interface ISyncRedshiftToPostgresService
    {
        Task SyncAsync(EtlExecutionContext etlExecutionContext);
    }
}
