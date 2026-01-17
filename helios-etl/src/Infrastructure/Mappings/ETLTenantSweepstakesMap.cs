using SunnyRewards.Helios.ETL.Common.Mappings;
using SunnyRewards.Helios.ETL.Core.Domain.Models;

namespace SunnyRewards.Helios.ETL.Infrastructure.Mappings
{
    public class ETLTenantSweepstakesMap : BaseMapping<ETLTenantSweepstakesModel>
    {
        public ETLTenantSweepstakesMap()
        {
            Schema("sweepstakes");
            Table("tenant_sweepstakes");

            Id(x => x.TenantSweepstakesId).Column("tenant_sweepstakes_id").GeneratedBy.Identity();
            Map(x => x.SweepstakesId).Column("sweepstakes_id");
            Map(x => x.TenantCode).Column("tenant_code");
            Map(x => x.CreateTs).Column("create_ts");
            Map(x => x.UpdateTs).Column("update_ts");
            Map(x => x.CreateUser).Column("create_user");
            Map(x => x.UpdateUser).Column("update_user");
            Map(x => x.DeleteNbr).Column("delete_nbr");
        }
    }
}