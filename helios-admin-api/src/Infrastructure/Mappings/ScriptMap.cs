using SunnyRewards.Helios.Common.Core.Mappings;
using SunnyRewards.Helios.Admin.Core.Domain.Models;

namespace SunnyRewards.Helios.Admin.Infrastructure.Mappings
{
    public class ScriptMap : BaseMapping<ScriptModel>
    {
        public ScriptMap()
        {
            Schema("admin");
            Table("script");
            Id(x => x.ScriptId).Column("script_id").GeneratedBy.Identity();
            Map(x => x.ScriptCode).Column("script_code");
            Map(x => x.ScriptName).Column("script_name");
            Map(x => x.ScriptDescription).Column("script_description");
            Map(x => x.ScriptJson).Column("script_json").CustomSqlType("jsonb").CustomType<StringAsJsonb>(); ;
            Map(x => x.ScriptSource).Column("script_source");
            Map(x => x.CreateTs).Column("create_ts");
            Map(x => x.UpdateTs).Column("update_ts");
            Map(x => x.DeleteNbr).Column("delete_nbr");
            Map(x => x.UpdateUser).Column("update_user");
            Map(x => x.CreateUser).Column("create_user");
        }
    }
}
