using SunnyRewards.Helios.ETL.Core.Domain.Dtos;

namespace SunnyRewards.Helios.ETL.Infrastructure.Services.Interfaces
{
    /// <summary>
    /// Processes health tasks.
    /// </summary>
    /// <param name="etlExecutionContext"></param>
    /// <returns></returns>
    public interface IHealthTaskSyncService
    {
        /// <summary>
        /// Processes health tasks.
        /// </summary>
        /// <param name="etlExecutionContext"></param>
        /// <returns></returns>
        Task ProcessHealthTaskAsync(EtlExecutionContext etlExecutionContext);
    }

}
