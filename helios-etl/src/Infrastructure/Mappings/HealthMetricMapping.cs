using SunnyRewards.Helios.ETL.Common.Mappings;
using SunnyRewards.Helios.ETL.Core.Domain.Models;

namespace SunnyRewards.Helios.ETL.Infrastructure.Mappings
{
    public class HealthMetricMap : BaseMapping<HealthMetricModel>
    {
        public HealthMetricMap()
        {
            Schema("health"); //TODO: verify
            Table("health_metric"); //TODO: verify
            Id(x => x.HealthMetricId).Column("health_metric_id").GeneratedBy.Identity();
            Map(x => x.HealthMetricTypeId).Column("health_metric_type_id");
            Map(x => x.TenantCode).Column("tenant_code");
            Map(x => x.ConsumerCode).Column("consumer_code");
            Map(x => x.DataJson).Column("data_json").CustomSqlType("jsonb").CustomType<StringAsJsonb>();
            Map(x => x.CaptureTs).Column("capture_ts");
            Map(x => x.CreateTs).Column("create_ts");
            Map(x => x.UpdateTs).Column("update_ts");
            Map(x => x.CreateUser).Column("create_user");
            Map(x => x.UpdateUser).Column("update_user");
            Map(x => x.DeleteNbr).Column("delete_nbr");
            Map(x => x.OsMetricTs).Column("os_metric_ts").Not.Nullable();
        }
    }
}
