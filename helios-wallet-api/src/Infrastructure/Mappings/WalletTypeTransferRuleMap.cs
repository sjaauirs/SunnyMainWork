using SunnyRewards.Helios.Common.Core.Mappings;
using SunnyRewards.Helios.Wallet.Core.Domain.Models;
using System.Diagnostics.CodeAnalysis;

namespace SunnyRewards.Helios.Wallet.Infrastructure.Mappings
{

    public class WalletTypeTransferRuleMap : BaseMapping<WalletTypeTransferRuleModel>
    {
        public WalletTypeTransferRuleMap() {
            Table("wallet_type_transfer_rule");
            Schema("wallet"); 
            Id(x => x.WalletTypeTransferRuleId).Column("wallet_type_transfer_rule_id").GeneratedBy.Identity();
            Map(x => x.WalletTypeTransferRuleCode).Column("wallet_type_transfer_rule_code");
            Map(x => x.TenantCode).Column("tenant_code");
            Map(x => x.SourceWalletTypeId).Column("source_wallet_type_id");
            Map(x => x.TargetWalletTypeId).Column("target_wallet_type_id");
            Map(x => x.TransferRule).Column("transfer_rule_json").CustomSqlType("jsonb").CustomType<StringAsJsonb>().Not.Nullable();
            Map(x => x.CreateTs).Column("create_ts");
            Map(x => x.UpdateTs).Column("update_ts");
            Map(x => x.DeleteNbr).Column("delete_nbr");
            Map(x => x.UpdateUser).Column("update_user");
            Map(x => x.CreateUser).Column("create_user");

        }
    }
}
