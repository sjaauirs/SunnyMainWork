using SunnyRewards.Helios.ETL.Common.Repositories;
using SunnyRewards.Helios.ETL.Core.Domain.Models;
using SunnyRewards.Helios.ETL.Infrastructure.Repositories.HeliosRepo.Interfaces;
using Microsoft.Extensions.Logging;

namespace SunnyRewards.Helios.ETL.Infrastructure.Repositories.HeliosRepo
{
    public class TenantProgramConfigRepo : BaseRepo<ETLTenantProgramConfigModel>, ITenantProgramConfigRepo
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="baseLogger"></param>
        /// <param name="session"></param>
        public TenantProgramConfigRepo(ILogger<BaseRepo<ETLTenantProgramConfigModel>> baseLogger, NHibernate.ISession session) :
            base(baseLogger, session)
        {
        }
    }
}
