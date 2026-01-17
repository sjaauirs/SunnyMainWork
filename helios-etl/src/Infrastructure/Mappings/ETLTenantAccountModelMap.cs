using SunnyRewards.Helios.ETL.Common.Mappings;
using SunnyRewards.Helios.ETL.Core.Domain.Models;

namespace SunnyRewards.Helios.ETL.Infrastructure.Mappings
{
    public class ETLTenantAccountMap : BaseMapping<ETLTenantAccountModel>
    {
        public ETLTenantAccountMap()
        {
            Schema("fis");
            Table("tenant_account");

            Id(x => x.TenantAccountId).Column("tenant_account_id").GeneratedBy.Identity();
            Map(x => x.TenantAccountCode).Column("tenant_account_code").Not.Nullable();
            Map(x => x.TenantCode).Column("tenant_code").Not.Nullable();
            Map(x => x.AccLoadConfig).Column("acc_load_config").Nullable();
            Map(x => x.TenantConfigJson).Column("tenant_config_json").CustomSqlType("jsonb").CustomType<StringAsJsonb>().Not.Nullable();
            Map(x => x.FundingConfigJson).Column("funding_config_json").CustomSqlType("jsonb").CustomType<StringAsJsonb>().Nullable();
            Map(x => x.CreateTs).Column("create_ts");
            Map(x => x.UpdateTs).Column("update_ts");
            Map(x => x.CreateUser).Column("create_user");
            Map(x => x.UpdateUser).Column("update_user");
            Map(x => x.DeleteNbr).Column("delete_nbr");
            Map(x => x.LastMonetaryTransactionId).Column("last_monetary_transaction_id").Not.Nullable().Default("0");
        }
    }
}
