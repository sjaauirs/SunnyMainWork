using SunnyRewards.Helios.ETL.Common.Repositories;
using SunnyRewards.Helios.ETL.Core.Domain.Models;
using SunnyRewards.Helios.ETL.Infrastructure.Repositories.HeliosRepo.Interfaces;
using Microsoft.Extensions.Logging;

namespace SunnyRewards.Helios.ETL.Infrastructure.Repositories.HeliosRepo
{
    public class CohortTenantTaskRewardRepo : BaseRepo<ETLCohortTenantTaskRewardModel>, ICohortTenantTaskRewardRepo
    {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="baseLogger"></param>
        /// <param name="session"></param>
        public CohortTenantTaskRewardRepo(ILogger<BaseRepo<ETLCohortTenantTaskRewardModel>> baseLogger, NHibernate.ISession session) : base(baseLogger, session)
        {
        }
    }
}