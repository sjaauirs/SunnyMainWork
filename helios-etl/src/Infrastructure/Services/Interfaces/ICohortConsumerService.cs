using Microsoft.AspNetCore.Mvc;
using SunnyRewards.Helios.Cohort.Core.Domain.Dtos;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.ETL.Core.Domain.Dtos;
using SunnyRewards.Helios.ETL.Core.Domain.Models;

namespace SunnyRewards.Helios.ETL.Infrastructure.Services.Interfaces
{
    public interface ICohortConsumerService
    {
        /// <summary>
        /// Imports the specified etl execution context.
        /// </summary>
        /// <param name="etlExecutionContext">The etl execution context.</param>
        /// <returns></returns>
        Task Import(EtlExecutionContext etlExecutionContext);

        Task<ETLCohortConsumerModel> AddConsumerToCohort(CohortConsumerRequestDto cohortConsumerRequestDto);
        Task<ETLCohortConsumerModel> RemoveConsumerToCohort(CohortConsumerRequestDto cohortConsumerRequestDto);

        Task<List<CohortConsumerTaskDto>> GetCohortConsumerTask(string tenantCode,
            string consumerCode, string cohortName);
    }
}
