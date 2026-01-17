using SunnyRewards.Helios.ETL.Common.Mappings;
using SunnyRewards.Helios.ETL.Core.Domain.Models;

namespace Infrastructure.Mappings
{
    public class ETLSweepstakesInstanceMap : BaseMapping<ETLSweepstakesInstanceModel>
    {
        public ETLSweepstakesInstanceMap()
        {
            Schema("sweepstakes");
            Table("sweepstakes_instance");

            Id(x => x.SweepstakesInstanceId).Column("sweepstakes_instance_id").GeneratedBy.Identity();
            Map(x => x.SweepstakesId).Column("sweepstakes_id");
            Map(x => x.TenantSweepstakesId).Column("tenant_sweepstakes_id");
            Map(x => x.InstanceTs).Column("instance_ts");
            Map(x => x.CreateTs).Column("create_ts");
            Map(x => x.UpdateTs).Column("update_ts");
            Map(x => x.CreateUser).Column("create_user");
            Map(x => x.UpdateUser).Column("update_user");
            Map(x => x.DeleteNbr).Column("delete_nbr");
            Map(x => x.PrizeDescriptionJson).Column("prize_description_json").CustomSqlType("jsonb").CustomType<StringAsJsonb>().Nullable();
            Map(x => x.SweepstakesInstanceCode).Column("sweepstakes_instance_code").Not.Nullable();
            Map(x => x.Status).Column("status").Not.Nullable();
        }
    }
}