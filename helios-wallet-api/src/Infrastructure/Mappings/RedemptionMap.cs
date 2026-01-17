using SunnyRewards.Helios.Common.Core.Mappings;
using SunnyRewards.Helios.Wallet.Core.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunnyRewards.Helios.Wallet.Infrastructure.Mappings
{
    public class RedemptionMap : BaseMapping<RedemptionModel>
    {
        public RedemptionMap()
        {
            Table("redemption");
            Schema("wallet");
            Id(x => x.RedemptionId).Column("redemption_id").GeneratedBy.Identity();
            Map(x => x.SubTransactionId).Column("sub_transaction_id");
            Map(x => x.AddTransactionId).Column("add_transaction_id");
            Map(x => x.RevertSubTransactionId).Column("revert_sub_transaction_id").Nullable();
            Map(x => x.RevertAddTransactionId).Column("revert_add_transaction_id").Nullable();
            Map(x => x.RedemptionStatus).Column("redemption_status");
            Map(x => x.Notes).Column("notes");
            Map(x => x.RedemptionRef).Column("redemption_ref");
            Map(x => x.RedemptionItemDescription).Column("redemption_item_description");
            Map(x => x.RedemptionStartTs).Column("redemption_start_ts");
            Map(x => x.RedemptionCompleteTs).Column("redemption_complete_ts");
            Map(x => x.RedemptionRevertTs).Column("redemption_revert_ts").Nullable();
            Map(x => x.RedemptionItemData).Column("redemption_item_data").CustomSqlType("jsonb").CustomType<StringAsJsonb>().Nullable();
            Map(x => x.CreateTs).Column("create_ts");
            Map(x => x.UpdateTs).Column("update_ts");
            Map(x => x.DeleteNbr).Column("delete_nbr");
            Map(x => x.UpdateUser).Column("update_user");
            Map(x => x.CreateUser).Column("create_user");
            Map(x => x.Xmin).Column("xmin").ReadOnly();
        }
    }
}
