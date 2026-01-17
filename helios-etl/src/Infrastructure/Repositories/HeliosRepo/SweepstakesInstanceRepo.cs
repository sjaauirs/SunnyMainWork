using Microsoft.Extensions.Logging;
using SunnyRewards.Helios.ETL.Common.Repositories;
using SunnyRewards.Helios.ETL.Core.Domain.Models;
using SunnyRewards.Helios.ETL.Infrastructure.Repositories.HeliosRepo.Interfaces;
using Microsoft.Extensions.Logging;
using NHibernate.Linq;

namespace SunnyRewards.Helios.ETL.Infrastructure.Repositories.HeliosRepo
{
    public class SweepstakesInstanceRepo : BaseRepo<ETLSweepstakesInstanceModel>, ISweepstakesInstanceRepo
    {
        private readonly ILogger<BaseRepo<ETLSweepstakesInstanceModel>> _logger;
        private readonly NHibernate.ISession _session;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="baseLogger"></param>
        /// <param name="session"></param>
        public SweepstakesInstanceRepo(ILogger<BaseRepo<ETLSweepstakesInstanceModel>> logger, NHibernate.ISession session) : base(logger, session)
        {
            _logger = logger;
            _session = session;
        }

        public async Task<ETLSweepstakesInstanceModel?> GetLatestSweepstakesInstance(string tenantCode, long sweepstakesInstanceId, long tenantSweepstakesId)
        {
            var lastSweepstakeInstance = from si in _session.Query<ETLSweepstakesInstanceModel>()
                                   join ts in _session.Query<ETLTenantSweepstakesModel>() on si.SweepstakesId equals ts.SweepstakesId
                                   where ts.TenantCode == tenantCode && si.TenantSweepstakesId== tenantSweepstakesId
                                   && si.SweepstakesInstanceId!= sweepstakesInstanceId
                                   && ts.DeleteNbr == 0 && si.DeleteNbr == 0
                                   orderby si.SweepstakesInstanceId descending
                                   select si;
            return await lastSweepstakeInstance.FirstOrDefaultAsync();
        }
    }
}