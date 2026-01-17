using SunnyRewards.Helios.Common.Core.Mappings;
using SunnyRewards.Helios.User.Core.Domain.Models;

namespace SunnyRewards.Helios.User.Infrastructure.Mappings
{
    public class PersonAddressMap : BaseMapping<PersonAddressModel>
    {
        public PersonAddressMap()
        {
            Schema("huser");
            Table("person_address");
            Id(x => x.PersonAddressId).Column("person_address_id").GeneratedBy.Identity();
            Map(x => x.AddressTypeId).Column("address_type_id");
            Map(x => x.PersonId).Column("person_id");
            Map(x => x.AddressLabel).Column("address_label");
            Map(x => x.Line1).Column("line1");
            Map(x => x.Line2).Column("line2");
            Map(x => x.City).Column("city");
            Map(x => x.State).Column("state");
            Map(x => x.PostalCode).Column("postal_code");
            Map(x => x.Region).Column("region");
            Map(x => x.CountryCode).Column("country_code");
            Map(x => x.Country).Column("country");
            Map(x => x.Source).Column("source");
            Map(x => x.IsPrimary).Column("is_primary");
            Map(x => x.CreateTs).Column("create_ts");
            Map(x => x.UpdateTs).Column("update_ts");
            Map(x => x.CreateUser).Column("create_user");
            Map(x => x.UpdateUser).Column("update_user");
            Map(x => x.DeleteNbr).Column("delete_nbr");
        }
    }
}
