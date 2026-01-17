using Microsoft.Extensions.Logging;
using SunnyRewards.Helios.Common.Core.Repositories;
using SunnyRewards.Helios.Tenant.Core.Domain.Models;
using SunnyRewards.Helios.Tenant.Infrastructure.Repositories.Interfaces;

namespace SunnyRewards.Helios.Tenant.Infrastructure.Repositories
{
    public class SponsorRepo : BaseRepo<SponsorModel>, ISponsorRepo
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="baseLogger"></param>
        /// <param name="session"></param>
        public SponsorRepo(ILogger<BaseRepo<SponsorModel>> baseLogger, NHibernate.ISession session) : base(baseLogger, session)
        {

        }
    }
}
