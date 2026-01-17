using SunnyRewards.Helios.ETL.Common.Mappings;
using SunnyRewards.Helios.ETL.Core.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunnyRewards.Helios.ETL.Infrastructure.Mappings
{
    public class ETLWalletModelMap :BaseMapping<ETLWalletModel>
    {
        public ETLWalletModelMap()
        {
            Schema("wallet");
            Table("wallet");

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
            Map(x => x.TotalEarned).Column("total_earned");
            Map(x => x.UpdateTs).Column("update_ts");
            Map(x => x.CreateTs).Column("create_ts");
            Map(x => x.UpdateUser).Column("update_user");
            Map(x => x.CreateUser).Column("create_user");
            Map(x => x.DeleteNbr).Column("delete_nbr");
        }
    }
}
