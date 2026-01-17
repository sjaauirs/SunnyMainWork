using SunnyRewards.Helios.ETL.Common.Mappings;
using SunnyRewards.Helios.ETL.Core.Domain.Models;

namespace SunnyRewards.Helios.ETL.Infrastructure.Mappings
{
    public class ETLRoleModelMap : BaseMapping<RoleModel>
    {
        public ETLRoleModelMap()
        {
            Schema("huser");
            Table("role"); // Replace "role_table" with your actual database table name

            Id(x => x.RoleId).Column("role_id").GeneratedBy.Identity();
            Map(x => x.RoleCode).Column("role_code");
            Map(x => x.RoleName).Column("role_name");
            Map(x => x.RoleDescription).Column("role_description");
            Map(x => x.UpdateTs).Column("update_ts");
            Map(x => x.CreateTs).Column("create_ts");
            Map(x => x.UpdateUser).Column("update_user");
            Map(x => x.CreateUser).Column("create_user");
            Map(x => x.DeleteNbr).Column("delete_nbr");

        }
    }
}
