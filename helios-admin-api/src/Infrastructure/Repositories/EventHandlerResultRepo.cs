using Microsoft.Extensions.Logging;
using SunnyRewards.Helios.Admin.Core.Domain.Models;
using SunnyRewards.Helios.Admin.Infrastructure.Repositories.Interfaces;
using SunnyRewards.Helios.Common.Core.Repositories;

namespace SunnyRewards.Helios.Admin.Infrastructure.Repositories
{
    public class EventHandlerResultRepo : BaseRepo<EventHandlerResultModel>, IEventHandlerResultRepo
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="baseLogger"></param>
        /// <param name="session"></param>
        public EventHandlerResultRepo(ILogger<BaseRepo<EventHandlerResultModel>> baseLogger, NHibernate.ISession session) : base(baseLogger, session)
        {
        }
    }
}
