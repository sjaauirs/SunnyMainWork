using SunnyRewards.Helios.ETL.Common.Mappings;
using SunnyRewards.Helios.ETL.Core.Domain.Models;

namespace SunnyRewards.Helios.ETL.Infrastructure.Mappings
{
    public class ETLTenantModelMap : BaseMapping<ETLTenantModel>
    {
        public ETLTenantModelMap()
        {
            Schema("tenant");
            Table("tenant");

            Id(x => x.TenantId).Column("tenant_id").GeneratedBy.Identity();
            Map(x => x.SponsorId).Column("sponsor_id");
            Map(x => x.TenantCode).Column("tenant_code");
            Map(x => x.PlanYear).Column("plan_year");
            Map(x => x.PeriodStartTs).Column("period_start_ts");
            Map(x => x.PeriodEndTs).Column("period_end_ts");
            Map(x => x.UpdateTs).Column("update_ts");
            Map(x => x.CreateTs).Column("create_ts");
            Map(x => x.UpdateUser).Column("update_user");
            Map(x => x.CreateUser).Column("create_user");
            Map(x => x.DeleteNbr).Column("delete_nbr");
            Map(x => x.PartnerCode).Column("partner_code");
            Map(x => x.RecommendedTask).Column("recommended_tasks");
            Map(x => x.redemption_vendor_name_0).Column("redemption_vendor_name_0");
            Map(x => x.redemption_vendor_partner_id_0).Column("redemption_vendor_partner_id_0");
            Map(x => x.TenantAttribute).Column("tenant_attr").CustomSqlType("jsonb").CustomType<StringAsJsonb>();
            Map(x => x.ApiKey).Column("api_key");
            Map(x => x.TenantOption).Column("tenant_option_json").CustomSqlType("jsonb").CustomType<StringAsJsonb>();
            Map(x => x.UtcTimeOffset).Column("utc_time_offset");
            Map(x => x.DstEnabled).Column("dst_enabled");
        }
    }
}
