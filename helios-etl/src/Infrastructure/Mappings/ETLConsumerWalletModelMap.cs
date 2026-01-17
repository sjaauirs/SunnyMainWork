using SunnyRewards.Helios.ETL.Common.Mappings;
using SunnyRewards.Helios.ETL.Core.Domain.Models;

namespace SunnyRewards.Helios.ETL.Infrastructure.Mappings
{
    public class ETLConsumerWalletModelMap : BaseMapping<ETLConsumerWalletModel>
    {
        public ETLConsumerWalletModelMap()
        {
            Schema("wallet");
            Table("consumer_wallet"); // Replace "consumer_wallet_table" with your actual database table name

            Id(x => x.ConsumerWalletId).Column("consumer_wallet_id").GeneratedBy.Identity();
            Map(x => x.WalletId).Column("wallet_id");
            Map(x => x.TenantCode).Column("tenant_code");
            Map(x => x.ConsumerCode).Column("consumer_code");
            Map(x => x.ConsumerRole).Column("consumer_role");
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
