using SunnyRewards.Helios.ETL.Common.Repositories.Interfaces;
using SunnyRewards.Helios.ETL.Core.Domain.Dtos;
using SunnyRewards.Helios.ETL.Core.Domain.Models;

namespace SunnyRewards.Helios.ETL.Infrastructure.Repositories.HeliosRepo.Interfaces
{
    public interface ICohortConsumerRepo : IBaseRepo<ETLCohortConsumerModel>
    {
        /// <summary>
        /// Get consumer task by tenant code, consumer code and cohort name
        /// </summary>
        /// <param name="tenantCode"></param>
        /// <param name="consumerCode"></param>
        /// <param name="cohortName"></param>
        /// <returns></returns>
        IQueryable<CohortConsumerTaskDto> GetCohortConsumerTask(string tenantCode, string consumerCode, string cohortName);
    }
}
