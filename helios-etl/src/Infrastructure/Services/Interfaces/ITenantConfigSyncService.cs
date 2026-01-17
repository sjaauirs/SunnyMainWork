using SunnyRewards.Helios.ETL.Core.Domain.Dtos;

namespace SunnyRewards.Helios.ETL.Infrastructure.Services.Interfaces
{
    public interface ITenantConfigSyncService
    {
        /// <summary>
        /// Sync the tenant config options
        /// </summary>
        /// <param name="etlExecutionContext">The etl execution context.</param>
        /// <returns></returns>
        Task SyncAsync(EtlExecutionContext etlExecutionContext);
    }
}
