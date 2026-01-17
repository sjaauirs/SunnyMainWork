using Microsoft.Extensions.Logging;
using SunnyRewards.Helios.Common.Core.Repositories;
using SunnyRewards.Helios.Tenant.Core.Domain.Models;
using SunnyRewards.Helios.Tenant.Infrastructure.Repositories.Interfaces;

namespace SunnyRewards.Helios.Tenant.Infrastructure.Repositories
{
    public class ComponentTypeRepo : BaseRepo<ComponentTypeModel>, IComponentTypeRepo
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="baseLogger"></param>
        /// <param name="session"></param>
        public ComponentTypeRepo(ILogger<BaseRepo<ComponentTypeModel>> baseLogger, NHibernate.ISession session) : base(baseLogger, session)
        {
            
        }
    }
}
