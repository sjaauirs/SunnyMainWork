using SunnyRewards.Helios.Common.Core.Mappings;
using SunnyRewards.Helios.Tenant.Core.Domain.Models;

namespace SunnyRewards.Helios.Tenant.Infrastructure.Mappings
{
    public class TenantMap : BaseMapping<TenantModel>
    {
        public TenantMap()
        {
            Schema("tenant");
            Table("tenant");

            Id(x => x.TenantId).Column("tenant_id").GeneratedBy.Identity();
            Map(x => x.SponsorId).Column("sponsor_id");
            Map(x => x.TenantCode).Column("tenant_code");
            Map(x => x.PlanYear).Column("plan_year");
            Map(x => x.PeriodStartTs).Column("period_start_ts");
            Map(x => x.PeriodEndTs).Column("period_end_ts");
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
            Map(x => x.Selfreport).Column("self_report");
            Map(x => x.DirectLogin).Column("direct_login");
            Map(x => x.EnableServerLogin).Column("enable_server_login").Not.Nullable();
            Map(x => x.TenantName).Column("tenant_name").Not.Nullable();
            Map(x => x.EncKeyId).Column("enc_key_id").Nullable();
            Map(x => x.TenantOption).Column("tenant_option_json").CustomSqlType("jsonb").CustomType<StringAsJsonb>();
            Map(x => x.AuthConfig).Column("auth_config").CustomSqlType("jsonb").CustomType<StringAsJsonb>();
            Map(x => x.UtcTimeOffset).Column("utc_time_offset");
            Map(x => x.DstEnabled).Column("dst_enabled");
        }
    }
}
