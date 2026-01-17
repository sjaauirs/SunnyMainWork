using SunnyRewards.Helios.ETL.Core.Domain.Dtos;

namespace SunnyRewards.Helios.ETL.Infrastructure.Services.Interfaces.FIS
{
    public interface IBenefitsFundingService
    {
        /// <summary>
        /// Executes the benefits funding rules for a given tenant.
        /// </summary>
        /// <param name="executionContext"></param>
        Task ExecuteFundingRules(EtlExecutionContext etlExecutionContext);
    }
}
