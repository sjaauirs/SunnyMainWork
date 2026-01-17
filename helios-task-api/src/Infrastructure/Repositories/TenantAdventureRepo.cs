using Microsoft.Extensions.Logging;
using NHibernate;
using SunnyRewards.Helios.Common.Core.Repositories;
using SunnyRewards.Helios.Task.Core.Domain.Models;
using SunnyRewards.Helios.Task.Infrastructure.Repositories.Interfaces;

namespace SunnyRewards.Helios.Task.Infrastructure.Repositories
{
    public class TenantAdventureRepo : BaseRepo<TenantAdventureModel>, ITenantAdventureRepo
    {
        public TenantAdventureRepo(ILogger<BaseRepo<TenantAdventureModel>> baseLogger, ISession session)
            : base(baseLogger, session)
        {
        }
    }
}
