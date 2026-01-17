using SunnyRewards.Helios.ETL.Common.Domain.Models;

namespace SunnyRewards.Helios.ETL.Common.Mappings
{
    public class AuditTrailMap : BaseMapping<AuditTrailModel>
    {
        public static string? SchemaName { get; set; }

        public AuditTrailMap()
        {
            if (SchemaName != null)
            {
                Table("audit_trail");
                Schema(SchemaName);

                Id(x => x.AuditTrailId).Column("audit_trail_id").GeneratedBy.Identity();

                Map(x => x.SourceModule).Column("source_module").Not.Nullable();
                Map(x => x.SourceContext).Column("source_context").Not.Nullable();
                Map(x => x.AuditName).Column("audit_name").Not.Nullable();
                Map(x => x.AuditMessage).Column("audit_message").Not.Nullable();
                Map(x => x.CreateTs).Column("create_ts");
                Map(x => x.DeleteNbr).Column("delete_nbr");
                Map(x => x.CreateUser).Column("create_user");
                Map(x => x.AuditData).Column("audit_data");
                Map(x => x.AuditJsonData).Column("audit_json_data").CustomSqlType("jsonb").CustomType<StringAsJsonb>();
            }
            else
            {
                // this is a fake setup - Audit Trail will not work with this setup and must not be used
                Table("fake");
                Schema("fake");

                Id(x => x.AuditTrailId).Column("fake_id").GeneratedBy.Identity();
            }
        }
    }
}
