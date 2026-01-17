using SunnyRewards.Helios.Common.Core.Mappings;
using SunnyRewards.Helios.User.Core.Domain.Models;

namespace SunnyRewards.Helios.User.Infrastructure.Mappings
{
    public class PersonRoleMap : BaseMapping<PersonRoleModel>
    {
        public PersonRoleMap()
        {
            Schema("huser");
            Table("person_role");

            Id(x => x.PersonRoleId).Column("person_role_id").GeneratedBy.Identity();
            Map(x => x.PersonId).Column("person_id");
            Map(x => x.RoleId).Column("role_id");
            Map(x => x.CreateTs).Column("create_ts");
            Map(x => x.UpdateTs).Column("update_ts");
            Map(x => x.CreateUser).Column("create_user");
            Map(x => x.UpdateUser).Column("update_user");
            Map(x => x.DeleteNbr).Column("delete_nbr");
            Map(x => x.CustomerCode).Column("customer_code");
            Map(x => x.SponsorCode).Column("sponsor_code");
            Map(x => x.TenantCode).Column("tenant_code");
        }
    }
}