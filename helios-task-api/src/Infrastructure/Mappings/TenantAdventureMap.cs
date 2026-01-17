using SunnyRewards.Helios.Common.Core.Mappings;
using SunnyRewards.Helios.Task.Core.Domain.Models;

namespace SunnyRewards.Helios.Task.Infrastructure.Mappings
{
    public class TenantAdventureMap : BaseMapping<TenantAdventureModel>
    {
        public TenantAdventureMap()
        {
            Schema("task");
            Table("tenant_adventure");

            Id(x => x.TenantAdventureId).Column("tenant_adventure_id").GeneratedBy.Identity();
            Map(x => x.TenantAdventureCode).Column("tenant_adventure_code");
            Map(x => x.TenantCode).Column("tenant_code");
            Map(x => x.AdventureId).Column("adventure_id");

            Map(x => x.CreateTs).Column("create_ts");
            Map(x => x.UpdateTs).Column("update_ts");
            Map(x => x.DeleteNbr).Column("delete_nbr");
            Map(x => x.UpdateUser).Column("update_user");
            Map(x => x.CreateUser).Column("create_user");
        }
    }

}

