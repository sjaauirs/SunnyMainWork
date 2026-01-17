using SunnyRewards.Helios.Common.Core.Mappings;
using SunnyRewards.Helios.Wallet.Core.Domain.Models;

namespace SunnyRewards.Helios.Wallet.Infrastructure.Mappings
{
    public class ConsumerWalletMap : BaseMapping<ConsumerWalletModel>
    {
        public ConsumerWalletMap()
        {
            Table("consumer_wallet");
            Schema("wallet");
            Id(x => x.ConsumerWalletId).Column("consumer_wallet_id").GeneratedBy.Identity();
            Map(x => x.WalletId).Column("wallet_id");
            Map(x => x.UpdateTs).Column("update_ts");
            Map(x => x.CreateTs).Column("create_ts");
            Map(x => x.UpdateUser).Column("update_user");
            Map(x => x.CreateUser).Column("create_user");
            Map(x => x.DeleteNbr).Column("delete_nbr");
            Map(x => x.ConsumerCode).Column("consumer_code");
            Map(x => x.TenantCode).Column("tenant_code");
            Map(x => x.ConsumerRole).Column("consumer_role");
            Map(x => x.EarnMaximum).Column("earn_maximum");
            Map(x => x.Xmin).Column("xmin").ReadOnly();
            Map(x => x.TotalEarned).Column("total_earned").Not.Nullable().Default("0.0");
        }
    }
}