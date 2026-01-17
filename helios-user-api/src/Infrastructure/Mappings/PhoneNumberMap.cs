using SunnyRewards.Helios.Common.Core.Mappings;
using SunnyRewards.Helios.User.Core.Domain.Models;

namespace SunnyRewards.Helios.User.Infrastructure.Mappings
{
    public class PhoneNumberMap : BaseMapping<PhoneNumberModel>
    {
        public PhoneNumberMap()
        {
            Schema("huser");
            Table("phone_number");
            Id(x => x.PhoneNumberId).Column("phone_number_id").GeneratedBy.Identity();
            Map(x => x.PhoneNumberCode).Column("phone_number_code");
            Map(x => x.PhoneTypeId).Column("phone_type_id");
            Map(x => x.PersonId).Column("person_id");
            Map(x => x.PhoneNumber).Column("phone_number");
            Map(x => x.IsPrimary).Column("is_primary");
            Map(x => x.IsVerified).Column("is_verified");
            Map(x => x.VerifiedDate).Column("verified_at");
            Map(x => x.Source).Column("source");
            Map(x => x.CreateTs).Column("create_ts");
            Map(x => x.UpdateTs).Column("update_ts");
            Map(x => x.CreateUser).Column("create_user");
            Map(x => x.UpdateUser).Column("update_user");
            Map(x => x.DeleteNbr).Column("delete_nbr");
        }
    }
}
