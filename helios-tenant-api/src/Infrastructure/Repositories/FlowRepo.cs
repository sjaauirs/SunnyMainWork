using Microsoft.Extensions.Logging;
using SunnyRewards.Helios.Common.Core.Repositories;
using SunnyRewards.Helios.Tenant.Core.Domain.Models;
using SunnyRewards.Helios.Tenant.Infrastructure.Repositories.Interfaces;

namespace SunnyRewards.Helios.Tenant.Infrastructure.Repositories
{
    public class FlowRepo : BaseRepo<FlowModel>, IFlowRepo
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="baseLogger"></param>
        /// <param name="session"></param>
        public FlowRepo(ILogger<BaseRepo<FlowModel>> baseLogger, NHibernate.ISession session) : base(baseLogger, session)
        {

        }
    }
}
