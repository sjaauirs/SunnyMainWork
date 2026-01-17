using SunnyRewards.Helios.ETL.Core.Domain.Dtos;

namespace SunnyRewards.Helios.ETL.Infrastructure.Services.Interfaces
{
    public interface ISyncMembersFromRedshiftToPostgresService
    {
        Task SyncAsync(EtlExecutionContext etlExecutionContext);
    }
}
