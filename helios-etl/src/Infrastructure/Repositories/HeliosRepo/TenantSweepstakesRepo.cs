using Microsoft.Extensions.Logging;
using SunnyRewards.Helios.ETL.Common.Repositories;
using SunnyRewards.Helios.ETL.Core.Domain.Models;
using SunnyRewards.Helios.ETL.Infrastructure.Repositories.HeliosRepo.Interfaces;

namespace SunnyRewards.Helios.ETL.Infrastructure.Repositories.HeliosRepo
{
    public class TenantSweepstakesRepo : BaseRepo<ETLTenantSweepstakesModel>, ITenantSweepstakesRepo
    {
        private readonly ILogger<BaseRepo<ETLTenantSweepstakesModel>> _logger;
        private readonly NHibernate.ISession _session;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="baseLogger"></param>
        /// <param name="session"></param>
        public TenantSweepstakesRepo(ILogger<BaseRepo<ETLTenantSweepstakesModel>> logger, NHibernate.ISession session) : base(logger, session)
        {
            _logger = logger;
            _session = session;
        }
    }
}