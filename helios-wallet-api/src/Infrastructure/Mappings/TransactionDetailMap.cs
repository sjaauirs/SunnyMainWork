using SunnyRewards.Helios.Common.Core.Mappings;
using SunnyRewards.Helios.Wallet.Core.Domain.Models;

namespace SunnyRewards.Helios.Wallet.Infrastructure.Mappings
{
    public class TransactionDetailMap : BaseMapping<TransactionDetailModel>
    {
        public TransactionDetailMap()
        {
            Table("transaction_detail");
            Schema("wallet");
            Id(x => x.TransactionDetailId).Column("transaction_detail_id").GeneratedBy.Identity();
            Map(x => x.TransactionDetailType).Column("transaction_detail_type");
            Map(x => x.ConsumerCode).Column("consumer_code");
            Map(x => x.TaskRewardCode).Column("task_reward_code");
            Map(x => x.Notes).Column("notes");
            Map(x => x.RedemptionRef).Column("redemption_ref");
            Map(x => x.RedemptionItemDescription).Column("redemption_item_description");
            Map(x => x.CreateTs).Column("create_ts");
            Map(x => x.DeleteNbr).Column("delete_nbr");
            Map(x => x.CreateUser).Column("create_user");
            Map(x => x.RewardDescription).Column("reward_description").Nullable();
        }
    }
}