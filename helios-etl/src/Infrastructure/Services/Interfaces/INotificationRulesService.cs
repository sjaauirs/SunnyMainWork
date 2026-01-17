using SunnyRewards.Helios.ETL.Core.Domain.Dtos;

namespace SunnyRewards.Helios.ETL.Infrastructure.Services.Interfaces
{
    public interface INotificationRulesService
    {
        /// <summary>
        /// Processes notification rules by fetching active rules, executing queries, 
        /// and sending notifications based on the defined business logic.
        /// </summary>
        Task ProcessNotificationRulesAsync(EtlExecutionContext etlExecutionContext);
    }
}
