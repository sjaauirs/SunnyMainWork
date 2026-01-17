using SunnyRewards.Helios.Common.Core.Repositories.Interfaces;
using SunnyRewards.Helios.NotificationService.Core.Domain.Models;

namespace SunnyRewards.Helios.ETL.Infrastructure.Repositories.HeliosRepo.Interfaces
{
    public interface INotificationRuleRepository : IBaseRepo<NotificationRuleModel>
    {
        public IList<Dictionary<string, object>> ExecuteQuery(string sqlQuery);
    }
}
