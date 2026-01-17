using FluentNHibernate.Mapping;
using SunnyRewards.Helios.Tenant.Core.Domain.Models;

namespace SunnyRewards.Helios.Tenant.Infrastructure.Mappings
{
    public class WalletCategoryMap : ClassMap<WalletCategoryModel>
    {
        public WalletCategoryMap()
        {
            Schema("tenant");
            Table("wallet_category");

            Id(x => x.Id).Column("id").GeneratedBy.Identity();

            Map(x => x.TenantCode).Column("tenant_code").Not.Nullable();
            Map(x => x.WalletTypeId).Column("wallet_type_id").Not.Nullable();
            Map(x => x.WalletTypeCode).Column("wallet_type_code").Not.Nullable();
            Map(x => x.CategoryFk).Column("category_fk").Not.Nullable();

            Map(x => x.ConfigJson).Column("config_json").CustomSqlType("jsonb");

            Map(x => x.CreateTs).Column("create_ts").Default("CURRENT_TIMESTAMP");
            Map(x => x.UpdateTs).Column("update_ts").Default("CURRENT_TIMESTAMP");
            Map(x => x.CreateUser).Column("create_user");
            Map(x => x.UpdateUser).Column("update_user");
            Map(x => x.DeleteNbr).Column("delete_nbr").Default("0");
        }
    }
}
