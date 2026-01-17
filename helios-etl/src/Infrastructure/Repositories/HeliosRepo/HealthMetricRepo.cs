using SunnyRewards.Helios.ETL.Common.Repositories;
using SunnyRewards.Helios.ETL.Core.Domain.Models;
using SunnyRewards.Helios.ETL.Infrastructure.Repositories.HeliosRepo.Interfaces;
using Microsoft.Extensions.Logging;

namespace SunnyRewards.Helios.ETL.Infrastructure.Repositories.HeliosRepo
{
    public class HealthMetricRepo : BaseRepo<HealthMetricModel>, IHealthMetricRepo
    {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="baseLogger"></param>
        /// <param name="session"></param>
        public HealthMetricRepo(ILogger<BaseRepo<HealthMetricModel>> baseLogger, NHibernate.ISession session) : base(baseLogger, session)
        {
        }
    }
}