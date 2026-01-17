using SunnyRewards.Helios.Common.Core.Repositories.Interfaces;
using SunnyRewards.Helios.ETL.Core.Domain.Models;
using SunnyRewards.Helios.NotificationService.Core.Domain.Models;

namespace SunnyRewards.Helios.ETL.Infrastructure.Repositories.HeliosRepo.Interfaces
{
    public interface IConsumerNotificationRepo : IBaseRepo<ETLConsumerNotificationModel>
    {
        Task<ETLConsumerNotificationModel> GetConsumerNotification(string consumerCode, string tenantCode, long ruleId);
    }
}
