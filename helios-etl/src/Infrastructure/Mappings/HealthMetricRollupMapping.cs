using SunnyRewards.Helios.ETL.Common.Mappings;
using SunnyRewards.Helios.ETL.Core.Domain.Models;

namespace SunnyRewards.Helios.ETL.Infrastructure.Mappings
{
    public class HealthMetricRollupMapping : BaseMapping<ETLHealthMetricRollupModel>
    {
        public HealthMetricRollupMapping()
        {
            Schema("health"); //TODO: verify
            Table("health_metric_rollup"); //TODO: verify
            Id(x => x.HealthMetricRollupId).Column("health_metric_rollup_id").GeneratedBy.Identity();
            Map(x => x.RollupPeriodTypeId).Column("rollup_period_type_id");
            Map(x => x.TenantCode).Column("tenant_code");
            Map(x => x.ConsumerCode).Column("consumer_code");
            Map(x => x.RollupPeriodStartTs).Column("rollup_period_start_ts");
            Map(x => x.RollupPeriodEndTs).Column("rollup_period_end_ts");
            Map(x => x.RollupDataJson).Column("rollup_data_json").CustomSqlType("jsonb").CustomType<StringAsJsonb>();
            Map(x => x.CreateTs).Column("create_ts");
            Map(x => x.UpdateTs).Column("update_ts");
            Map(x => x.CreateUser).Column("create_user");
            Map(x => x.UpdateUser).Column("update_user");
            Map(x => x.DeleteNbr).Column("delete_nbr");
        }
    }
}
