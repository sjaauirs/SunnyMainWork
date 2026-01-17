using SunnyRewards.Helios.Common.Core.Mappings;
using SunnyRewards.Helios.User.Core.Domain.Models;

namespace SunnyRewards.Helios.User.Infrastructure.Mappings
{
    public class RoleMap : BaseMapping<RoleModel>
    {
        public RoleMap()
        {
            Schema("huser");
            Table("role");

            Id(x => x.RoleId).Column("role_id").GeneratedBy.Identity();
            Map(x => x.RoleCode).Column("role_code");
            Map(x => x.RoleName).Column("role_name");
            Map(x => x.RoleDescription).Column("role_description");
            Map(x => x.CreateTs).Column("create_ts");
            Map(x => x.UpdateTs).Column("update_ts");
            Map(x => x.CreateUser).Column("create_user");
            Map(x => x.UpdateUser).Column("update_user");
            Map(x => x.DeleteNbr).Column("delete_nbr");
        }
    }
}