using SunnyRewards.Helios.Common.Core.Mappings;
using SunnyRewards.Helios.ETL.Core.Domain.Models;

namespace SunnyRewards.Helios.ETL.Infrastructure.Mappings
{
    public class ETLConsumerNotificationMap : BaseMapping<ETLConsumerNotificationModel>
    {
        public ETLConsumerNotificationMap() 
        {
            Table("consumer_notification");
            Schema("notification");
            Id(x => x.ConsumerNotificationId).Column("consumer_notification_id").GeneratedBy.Identity();
            Map(x => x.ConsumerNotificationCode).Column("consumer_notification_code");
            Map(x => x.ConsumerCode).Column("consumer_code");
            Map(x => x.TenantCode).Column("tenant_code");
            Map(x => x.NotificationPayload).Column("notification_payload").CustomSqlType("jsonb").CustomType<StringAsJsonb>();
            Map(x => x.NotificationChannelId).Column("notification_channel_id");
            Map(x => x.ExternalNotificationId).Column("external_notification_id");
            Map(x => x.Status).Column("status");
            Map(x => x.RetryCount).Column("retry_count");
            Map(x => x.CreateTs).Column("create_ts");
            Map(x => x.UpdateTs).Column("update_ts");
            Map(x => x.DeleteNbr).Column("delete_nbr");
            Map(x => x.UpdateUser).Column("update_user");
            Map(x => x.CreateUser).Column("create_user");
            Map(x => x.NotificationRuleId).Column("notification_rule_id");
        }
    }
}
