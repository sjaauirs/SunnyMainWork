using SunnyRewards.Helios.ETL.Common.Repositories.Interfaces;
using SunnyRewards.Helios.ETL.Core.Domain.Models;

namespace SunnyRewards.Helios.ETL.Infrastructure.Repositories.HeliosRepo.Interfaces
{
    public interface ICohortRepo : IBaseRepo<ETLCohortModel>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="consumerCode"></param>
        /// <param name="cohortName"></param>
        /// <returns></returns>
        Task<bool> IsInCohort(string consumerCode, string cohortName);
    }
}

