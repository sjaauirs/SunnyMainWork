using FluentNHibernate.Mapping;
using SunnyRewards.Helios.ETL.Core.Domain.Models;

namespace SunnyRewards.Helios.ETL.Infrastructure.Mappings
{
    public class ETLFundingHistoryMap : ClassMap<ETLFundingHistoryModel>
    {
        public ETLFundingHistoryMap()
        {
            Schema("fis");
            Table("funding_history");

            Id(x => x.FundingHistoryId).Column("funding_history_id").GeneratedBy.Identity();
            Map(x => x.TenantCode).Column("tenant_code").Not.Nullable();
            Map(x => x.ConsumerCode).Column("consumer_code").Not.Nullable();
            Map(x => x.FundRuleNumber).Column("fund_rule_number").Not.Nullable();
            Map(x => x.FundTs).Column("fund_ts").Not.Nullable();
            Map(x => x.CreateTs).Column("create_ts").Not.Nullable();
            Map(x => x.CreateUser).Column("create_user").Not.Nullable();
            Map(x => x.DeleteNbr).Column("delete_nbr").Not.Nullable();
        }
    }
}
