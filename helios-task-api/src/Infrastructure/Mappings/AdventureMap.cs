using SunnyRewards.Helios.Common.Core.Mappings;
using SunnyRewards.Helios.Task.Core.Domain.Models;

namespace SunnyRewards.Helios.Task.Infrastructure.Mappings
{

    public class AdventureMap : BaseMapping<AdventureModel>
    {
        public AdventureMap()
        {
            Schema("task");
            Table("adventure");


            Id(x => x.AdventureId).Column("adventure_id").GeneratedBy.Identity();
            Map(x => x.AdventureCode).Column("adventure_code");
            Map(x => x.AdventureConfigJson).Column("adventure_config_json").CustomSqlType("jsonb").CustomType<StringAsJsonb>();
            Map(x => x.CmsComponentCode).Column("cms_component_code");

            Map(x => x.CreateTs).Column("create_ts");
            Map(x => x.UpdateTs).Column("update_ts");
            Map(x => x.DeleteNbr).Column("delete_nbr");
            Map(x => x.UpdateUser).Column("update_user");
            Map(x => x.CreateUser).Column("create_user");
        }
    }

}
