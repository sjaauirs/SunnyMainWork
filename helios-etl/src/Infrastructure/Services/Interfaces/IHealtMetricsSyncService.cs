using SunnyRewards.Helios.ETL.Core.Domain.Dtos;

namespace SunnyRewards.Helios.ETL.Infrastructure.Services.Interfaces
{
    public interface IHealtMetricsSyncService
    {
        /// <summary>
        /// Processes messages from the Retail product AWS SQS queue and generate FIS APL file
        /// </summary>
        /// <param name="etlExecutionContext"></param>
        /// <returns></returns>
        Task ProcessQueueMessages(EtlExecutionContext etlExecutionContext);
    }
}
