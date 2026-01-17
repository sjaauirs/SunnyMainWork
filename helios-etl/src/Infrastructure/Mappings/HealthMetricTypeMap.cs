using SunnyRewards.Helios.ETL.Common.Mappings;
using SunnyRewards.Helios.ETL.Core.Domain.Models;

namespace SunnyRewards.Helios.ETL.Infrastructure.Mappings
{
    public class HealthMetricTypeMap : BaseMapping<HealthMetricTypeModel>
    {
        public HealthMetricTypeMap()
        {
            Schema("health");
            Table("health_metric_type");
            Id(x => x.HealthMetricTypeId).Column("health_metric_type_id").GeneratedBy.Identity();
            Map(x => x.HealthMetricTypeCode).Column("health_metric_type_code");
            Map(x => x.HealthMetricTypeName).Column("health_metric_type_name");
            Map(x => x.SchemaJson).Column("schema_json").CustomSqlType("jsonb").CustomType<StringAsJsonb>();
            Map(x => x.CreateTs).Column("create_ts");
            Map(x => x.UpdateTs).Column("update_ts");
            Map(x => x.CreateUser).Column("create_user");
            Map(x => x.UpdateUser).Column("update_user");
            Map(x => x.DeleteNbr).Column("delete_nbr");
        }
    }
}
