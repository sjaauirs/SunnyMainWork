using SunnyRewards.Helios.ETL.Common.Mappings;
using SunnyRewards.Helios.ETL.Core.Domain.Models;

namespace SunnyRewards.Helios.ETL.Infrastructure.Mappings
{
    public class ETLConsumerAccountMap : BaseMapping<ETLConsumerAccountModel>
    {
        public ETLConsumerAccountMap()
        {
            Schema("fis");
            Table("consumer_account");

            Id(x => x.ConsumerAccountId).Column("consumer_account_id").GeneratedBy.Identity();
            Map(x => x.ConsumerAccountCode).Column("consumer_account_code");
            Map(x => x.TenantCode).Column("tenant_code");
            Map(x => x.ConsumerCode).Column("consumer_code");
            Map(x => x.ProxyNumber).Column("proxy_number");
            Map(x => x.ProxyUpdateTs).Column("proxy_update_ts");
            Map(x => x.CreateTs).Column("create_ts");
            Map(x => x.UpdateTs).Column("update_ts");
            Map(x => x.CreateUser).Column("create_user");
            Map(x => x.UpdateUser).Column("update_user");
            Map(x => x.DeleteNbr).Column("delete_nbr");
            Map(x => x.CardLast4).Column("card_last_digits");
            Map(x => x.SyncRequired).Column("sync_required");
            Map(x => x.SyncInfoJson).Column("sync_info_json").CustomSqlType("jsonb").CustomType<StringAsJsonb>().Nullable();
            Map(x => x.ConsumerAccountConfigJson).Column("consumer_account_config_json").CustomSqlType("jsonb").CustomType<StringAsJsonb>().Nullable();
            Map(x => x.ClientUniqueId).Column("client_unique_id");
            Map(x => x.CardIssueStatus).Column("card_issue_status");
            Map(x => x.CardRequestStatus).Column("card_request_status");
        }
    }
}
