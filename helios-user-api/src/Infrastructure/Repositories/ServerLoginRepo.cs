using Microsoft.Extensions.Logging;
using SunnyRewards.Helios.Common.Core.Repositories;
using SunnyRewards.Helios.User.Core.Domain.Models;
using SunnyRewards.Helios.User.Infrastructure.Repositories.Interfaces;

namespace SunnyRewards.Helios.User.Infrastructure.Repositories
{
    public class ServerLoginRepo : BaseRepo<ServerLoginModel>, IServerLoginRepo
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="baseLogger"></param>
        /// <param name="session"></param>
        public ServerLoginRepo(ILogger<BaseRepo<ServerLoginModel>> baseLogger, NHibernate.ISession session) : base(baseLogger, session)
        {
        }
    }
}
