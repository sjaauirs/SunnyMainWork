using SunnyRewards.Helios.Common.Core.Mappings;
using SunnyRewards.Helios.Wallet.Core.Domain.Models;

namespace SunnyRewards.Helios.Wallet.Infrastructure.Mappings
{
    public class WalletMap : BaseMapping<WalletModel>
    {
        public WalletMap()
        {
            Table("wallet");
            Schema("wallet");
            Id(x => x.WalletId).Column("wallet_id").GeneratedBy.Identity();
            Map(x => x.WalletTypeId).Column("wallet_type_id");
            Map(x => x.CustomerCode).Column("customer_code");
            Map(x => x.SponsorCode).Column("sponsor_code");
            Map(x => x.TenantCode).Column("tenant_code");
            Map(x => x.WalletCode).Column("wallet_code");
            Map(x => x.MasterWallet).Column("master_wallet");
            Map(x => x.WalletName).Column("wallet_name");
            Map(x => x.Active).Column("active");
            Map(x => x.ActiveStartTs).Column("active_start_ts");
            Map(x => x.ActiveEndTs).Column("active_end_ts");
            Map(x => x.Balance).Column("balance");
            Map(x => x.EarnMaximum).Column("earn_maximum");
            Map(x => x.CreateTs).Column("create_ts");
            Map(x => x.UpdateTs).Column("update_ts");
            Map(x => x.DeleteNbr).Column("delete_nbr");
            Map(x => x.UpdateUser).Column("update_user");
            Map(x => x.CreateUser).Column("create_user");
            Map(x => x.TotalEarned).Column("total_earned");
            Map(x => x.Xmin).Column("xmin").ReadOnly();
            Map(x => x.Index).Column("index");
            Map(x => x.RedeemEndTs).Column("redeem_end_ts");
        }
    }
}
