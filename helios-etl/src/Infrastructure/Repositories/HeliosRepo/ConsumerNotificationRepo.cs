using Microsoft.Extensions.Logging;
using NHibernate.Linq;
using SunnyRewards.Helios.Common.Core.Repositories;
using SunnyRewards.Helios.ETL.Core.Domain.Models;
using SunnyRewards.Helios.ETL.Infrastructure.Repositories.HeliosRepo.Interfaces;
using SunnyRewards.Helios.NotificationService.Core.Domain.Models;
using SunnyRewards.Helios.Tenant.Core.Domain.Models;

namespace SunnyRewards.Helios.ETL.Infrastructure.Repositories.HeliosRepo
{
    public class ConsumerNotificationRepo : BaseRepo<ETLConsumerNotificationModel>, IConsumerNotificationRepo
    {
        private readonly NHibernate.ISession _session;
        private readonly ILogger<BaseRepo<ETLConsumerNotificationModel>> _logger;
        public ConsumerNotificationRepo(ILogger<BaseRepo<ETLConsumerNotificationModel>> baseLogger, NHibernate.ISession session) : base(baseLogger, session)
        {
            _session = session;
            _logger = baseLogger;
        }

        public async Task<ETLConsumerNotificationModel> GetConsumerNotification(string consumerCode, string tenantCode, long ruleId) 
        {
            var query = from cn in _session.Query<ETLConsumerNotificationModel>()
                        where cn.ConsumerCode == consumerCode &&
                            cn.TenantCode == tenantCode &&
                            cn.NotificationRuleId == ruleId &&
                            cn.DeleteNbr == 0
                        orderby cn.CreateTs descending
                        select cn;

            return await query.FirstOrDefaultAsync();
        }
    }
}
