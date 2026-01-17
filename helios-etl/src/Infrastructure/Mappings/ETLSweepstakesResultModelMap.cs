using SunnyRewards.Helios.ETL.Common.Mappings;
using SunnyRewards.Helios.ETL.Core.Domain.Models;

namespace Infrastructure.Mappings
{
    public class ETLSweepstakesResultModelMap : BaseMapping<ETLSweepstakesResultModel>
    {
            public ETLSweepstakesResultModelMap()
            {
                Schema("sweepstakes");
                Table("sweepstakes_result");
                Id(x => x.SweepstakesResultId).Column("sweepstakes_result_id").GeneratedBy.Identity();
                Map(x => x.SweepstakesInstanceId).Column("sweepstakes_instance_id");
                Map(x => x.TenantCode).Column("tenant_code");
                Map(x => x.ConsumerCode).Column("consumer_code");
                Map(x => x.PrizeIdentifier).Column("prize_identifier");
                Map(x => x.ResultTs).Column("result_ts");
                Map(x => x.CreateTs).Column("create_ts");
                Map(x => x.UpdateTs).Column("update_ts");
                Map(x => x.CreateUser).Column("create_user");
                Map(x => x.UpdateUser).Column("update_user");
                Map(x => x.DeleteNbr).Column("delete_nbr");
                Map(x => x.PrizeDescribeJson).Column("prize_describe_json").CustomSqlType("jsonb").CustomType<StringAsJsonb>().Nullable();
                Map(x => x.IsRewarded).Column("is_rewarded");
        }
        }
    }
