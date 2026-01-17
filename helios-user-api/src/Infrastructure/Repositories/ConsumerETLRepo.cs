using Microsoft.Extensions.Logging;
using SunnyRewards.Helios.Common.Core.Repositories;
using SunnyRewards.Helios.User.Core.Domain.Models;
using SunnyRewards.Helios.User.Infrastructure.Repositories.Interfaces;

namespace SunnyRewards.Helios.User.Infrastructure.Repositories
{
    public class ConsumerETLRepo : BaseRepo<ConsumerETLModel>, IConsumerETLRepo
    {
        public ConsumerETLRepo(ILogger<BaseRepo<ConsumerETLModel>> baseLogger, NHibernate.ISession session) : base(baseLogger, session)
        {
        }
    }
}
