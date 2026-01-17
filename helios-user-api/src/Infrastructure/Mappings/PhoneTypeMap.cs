using SunnyRewards.Helios.Common.Core.Mappings;
using SunnyRewards.Helios.User.Core.Domain.Models;

namespace SunnyRewards.Helios.User.Infrastructure.Mappings
{
    public class PhoneTypeMap : BaseMapping<PhoneTypeModel>
    {
        public PhoneTypeMap()
        {
            Schema("huser");
            Table("phone_type");
            Id(x => x.PhoneTypeId).Column("phone_type_id").GeneratedBy.Identity();
            Map(x => x.PhoneTypeName).Column("phone_type_name");
            Map(x => x.PhoneTypeCode).Column("phone_type_code");
            Map(x => x.Description).Column("description");
            Map(x => x.CreateTs).Column("create_ts");
            Map(x => x.UpdateTs).Column("update_ts");
            Map(x => x.CreateUser).Column("create_user");
            Map(x => x.UpdateUser).Column("update_user");
            Map(x => x.DeleteNbr).Column("delete_nbr");
        }
    }
}
