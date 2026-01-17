using Microsoft.Extensions.Logging;
using SunnyRewards.Helios.Common.Core.Repositories;
using SunnyRewards.Helios.User.Core.Domain.Models;
using SunnyRewards.Helios.User.Infrastructure.Repositories.Interfaces;

namespace SunnyRewards.Helios.User.Infrastructure.Repositories
{
    public class ConsumerHistoryRepo : BaseRepo<ConsumerHistoryModel>, IConsumerHistoryRepo
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="baseLogger"></param>
        /// <param name="session"></param>
        public ConsumerHistoryRepo(ILogger<BaseRepo<ConsumerHistoryModel>> baseLogger, NHibernate.ISession session) : base(baseLogger, session)
        {
        }
    }
}