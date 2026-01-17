using Microsoft.Extensions.Logging;
using SunnyRewards.Helios.Common.Core.Repositories;
using SunnyRewards.Helios.User.Core.Domain.Models;
using SunnyRewards.Helios.User.Infrastructure.Repositories.Interfaces;

namespace SunnyRewards.Helios.User.Infrastructure.Repositories
{
    public class ConsumerDeviceRepo : BaseRepo<ConsumerDeviceModel>, IConsumerDeviceRepo
    {
        public ConsumerDeviceRepo(ILogger<BaseRepo<ConsumerDeviceModel>> baseLogger, NHibernate.ISession session) : base(baseLogger, session)
        {

        }
    }
}
