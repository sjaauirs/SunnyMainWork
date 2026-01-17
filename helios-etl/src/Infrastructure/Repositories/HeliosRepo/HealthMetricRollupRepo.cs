using SunnyRewards.Helios.ETL.Common.Repositories;
using SunnyRewards.Helios.ETL.Core.Domain.Models;
using SunnyRewards.Helios.ETL.Infrastructure.Repositories.HeliosRepo.Interfaces;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace SunnyRewards.Helios.ETL.Infrastructure.Repositories.HeliosRepo
{
    public class HealthMetricRollupRepo : BaseRepo<ETLHealthMetricRollupModel>, IHealthMetricRollupRepo
    {
        private readonly ILogger<BaseRepo<ETLHealthMetricRollupModel>> _baseLogger;
        private readonly NHibernate.ISession _session;

        /// <summary>
        /// Constructor 
        /// </summary>
        /// <param name="baseLogger"></param>
        /// <param name="session"></param>
        public HealthMetricRollupRepo(ILogger<BaseRepo<ETLHealthMetricRollupModel>> baseLogger, NHibernate.ISession session) : base(baseLogger, session)
        {
            _baseLogger = baseLogger;
            _session = session;
        }
    }
}