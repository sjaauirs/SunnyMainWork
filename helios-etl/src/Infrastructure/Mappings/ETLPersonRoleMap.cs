using SunnyRewards.Helios.ETL.Common.Mappings;
using SunnyRewards.Helios.ETL.Core.Domain.Models;

namespace SunnyRewards.Helios.ETL.Infrastructure.Mappings
{
    public class ETLPersonRoleMap : BaseMapping<ETLPersonRoleModel>
    {
        public ETLPersonRoleMap()
        {
            Schema("huser");
            Table("person_role");

            Id(x => x.Id).Column("person_role_id").GeneratedBy.Identity();
            Map(x => x.PersonId).Column("person_id");
            Map(x => x.RoleId).Column("role_id");
            Map(x => x.UpdateTs).Column("update_ts");
            Map(x => x.CreateTs).Column("create_ts");
            Map(x => x.UpdateUser).Column("update_user");
            Map(x => x.CreateUser).Column("create_user");
            Map(x => x.DeleteNbr).Column("delete_nbr");
        }
    }
}