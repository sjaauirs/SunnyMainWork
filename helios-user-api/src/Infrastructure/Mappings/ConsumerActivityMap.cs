using SunnyRewards.Helios.Common.Core.Mappings;
using SunnyRewards.Helios.User.Core.Domain.Models;

namespace SunnyRewards.Helios.User.Infrastructure.Mappings
{
    public class ConsumerActivityMap : BaseMapping<ConsumerActivityModel>
    {
        public ConsumerActivityMap()
        {
            Schema("huser");
            Table("consumer_activity");
            Id(x => x.ConsumerActivityId).Column("consumer_activity_id").GeneratedBy.Identity();
            Map(x => x.ConsumerActivityCode).Column("consumer_activity_code");
            Map(x => x.ConsumerCode).Column("consumer_code");
            Map(x => x.TenantCode).Column("tenant_code");
            Map(x => x.ActivitySource).Column("activity_source");
            Map(x => x.ActivityType).Column("activity_type");
            Map(x => x.ActivityDetailJson).Column("activity_detail_json").CustomSqlType("jsonb").CustomType<StringAsJsonb>();
            Map(x => x.CreateTs).Column("create_ts");
            Map(x => x.UpdateTs).Column("update_ts");
            Map(x => x.CreateUser).Column("create_user");
            Map(x => x.UpdateUser).Column("update_user");
            Map(x => x.DeleteNbr).Column("delete_nbr");
        }
    }
}
