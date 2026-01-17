using SunnyRewards.Helios.Common.Core.Domain.Models;

namespace SunnyRewards.Helios.ETL.Core.Domain.Models
{
    public class ETLConsumerNotificationModel : BaseModel
    {
        public virtual long ConsumerNotificationId { get; set; }

        public virtual string? ConsumerNotificationCode { get; set; }

        public virtual string? ConsumerCode { get; set; }

        public virtual string? TenantCode { get; set; }

        public virtual long? NotificationChannelId { get; set; }

        public virtual string? ExternalNotificationId { get; set; }

        public virtual string? Status { get; set; }

        public virtual int RetryCount { get; set; }

        public virtual string? NotificationPayload { get; set; }

        public virtual long NotificationRuleId { get; set; }
    }
}
