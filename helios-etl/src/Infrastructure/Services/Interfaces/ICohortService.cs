using SunnyRewards.Helios.ETL.Core.Domain.Dtos;
using SunnyRewards.Helios.ETL.Core.Domain.Models;

namespace SunnyRewards.Helios.ETL.Infrastructure.Services.Interfaces
{
    /// <summary>
    /// 
    /// </summary>
    public interface ICohortService
    {
        /// <summary>
        /// 
        /// </summary>
        Task<CohortRuleExecutionDto> TestRuleExecutor();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="etlConsumers"></param>
        /// <returns></returns>
        Task ProcessCohorts(List<ETLConsumerModel> etlConsumers, List<ETLPersonModel> etlPersons, List<string>? cohortCodesList = null);

        /// <summary>
        /// ExecuteCohortingAsync
        /// </summary>
        /// <param name="etlExecutionContext"></param>
        /// <returns></returns>
        Task ExecuteCohortingAsync(EtlExecutionContext etlExecutionContext);
    }
}
