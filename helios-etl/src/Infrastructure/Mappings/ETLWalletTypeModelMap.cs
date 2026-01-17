using SunnyRewards.Helios.ETL.Common.Mappings;
using SunnyRewards.Helios.ETL.Core.Domain.Models;

namespace SunnyRewards.Helios.ETL.Infrastructure.Mappings
{
    public class ETLWalletTypeModelMap : BaseMapping<ETLWalletTypeModel>
    {
        public ETLWalletTypeModelMap()
        {
            Schema("wallet");
            Table("wallet_type");

            Id(x => x.WalletTypeId).Column("wallet_type_id").GeneratedBy.Identity();
            Map(x => x.WalletTypeCode).Column("wallet_type_code");
            Map(x => x.WalletTypeName).Column("wallet_type_name");
            Map(x => x.WalletTypeLabel).Column("wallet_type_label");
            Map(x => x.CreateTs).Column("create_ts");
            Map(x => x.UpdateTs).Column("update_ts");
            Map(x => x.DeleteNbr).Column("delete_nbr");
            Map(x => x.UpdateUser).Column("update_user");
            Map(x => x.CreateUser).Column("create_user");
            Map(x => x.ShortLabel).Column("short_label");
            Map(x => x.IsExternalSync).Column("is_external_sync").Not.Nullable();
        }
    }
}
