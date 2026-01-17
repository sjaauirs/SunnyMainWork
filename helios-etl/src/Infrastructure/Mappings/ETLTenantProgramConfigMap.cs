using SunnyRewards.Helios.ETL.Common.Mappings;
using SunnyRewards.Helios.ETL.Core.Domain.Models;

namespace SunnyRewards.Helios.ETL.Infrastructure.Mappings
{
    public class ETLTenantProgramConfigMap : BaseMapping<ETLTenantProgramConfigModel>
    {
        public ETLTenantProgramConfigMap()
        {
            Schema("fis");
            Table("tenant_program_config");
            Id(x => x.TenantProgramConfigId).Column("tenant_program_config_id").GeneratedBy.Identity();
            Map(x => x.TenantProgramConfigCode).Column("tenant_program_config_code").Not.Nullable();
            Map(x => x.TenantCode).Column("tenant_code").Not.Nullable();
            Map(x => x.ClientId).Column("client_id").Not.Nullable();
            Map(x => x.CompanyId).Column("company_id").Not.Nullable();
            Map(x => x.SubprogramId).Column("subprogram_id").Not.Nullable();
            Map(x => x.PackageIdMapping).Column("package_id_mapping").CustomSqlType("jsonb").CustomType<StringAsJsonb>().Not.Nullable();
            Map(x => x.DiscreteDataConfig).Column("discrete_data_config").CustomSqlType("jsonb").CustomType<StringAsJsonb>().Nullable();
            Map(x => x.CreatedAt).Column("created_at");
            Map(x => x.UpdatedAt).Column("updated_at");
            Map(x => x.CreatedUser).Column("created_user");
            Map(x => x.UpdatedUser).Column("updated_user");
            Map(x => x.DeleteNbr).Column("delete_nbr");
        }
    }
}