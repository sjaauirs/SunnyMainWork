using Microsoft.Extensions.Logging;
using NHibernate;
using NHibernate.Linq;
using SunnyRewards.Helios.Common.Core.Repositories;
using SunnyRewards.Helios.Tenant.Core.Domain.Models;
using SunnyRewards.Helios.User.Core.Domain.Models;
using SunnyRewards.Helios.User.Infrastructure.Repositories.Interfaces;

namespace SunnyRewards.Helios.User.Infrastructure.Repositories
{
    public class ConsumerLoginRepo : BaseRepo<ConsumerLoginModel>, IConsumerLoginRepo
    {
        private readonly ISession _session;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="baseLogger"></param>
        /// <param name="session"></param>
        public ConsumerLoginRepo(ILogger<BaseRepo<ConsumerLoginModel>> baseLogger, NHibernate.ISession session) : base(baseLogger, session)
        {
            _session = session;

        }
        public async Task<DateTime?> GetFirstLoginDateAsync(long consumerId)
        {
            var query = from login in _session.Query<ConsumerLoginModel>()
                        where login.ConsumerId == consumerId
                        select login.LoginTs;

            return await query.MinAsync();
        }
    }
}