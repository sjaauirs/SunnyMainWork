using SunnyRewards.Helios.ETL.Common.Mappings;
using SunnyRewards.Helios.ETL.Core.Domain.Models;

namespace SunnyRewards.Helios.ETL.Infrastructure.Mappings
{

    public class ETLPersonModelMap : BaseMapping<ETLPersonModel>
    {
        public ETLPersonModelMap()
        {
            Schema("huser");
            Table("person");

            Id(x => x.PersonId).Column("person_id").GeneratedBy.Identity();
            Map(x => x.UpdateTs).Column("update_ts");
            Map(x => x.CreateTs).Column("create_ts");
            Map(x => x.UpdateUser).Column("update_user");
            Map(x => x.CreateUser).Column("create_user");
            Map(x => x.DeleteNbr).Column("delete_nbr");
            Map(x => x.PersonCode).Column("person_code");
            Map(x => x.FirstName).Column("first_name");
            Map(x => x.LastName).Column("last_name");
            Map(x => x.LanguageCode).Column("language_code");
            Map(x => x.MemberSince).Column("member_since");
            Map(x => x.Email).Column("email");
            Map(x => x.City).Column("city");
            Map(x => x.Country).Column("country");
            Map(x => x.YearOfBirth).Column("year_of_birth");
            Map(x => x.PostalCode).Column("postal_code");
            Map(x => x.PhoneNumber).Column("phone_number");
            Map(x => x.Region).Column("region");
            Map(x => x.Gender).Column("gender");
            Map(x => x.DOB).Column("dob");
            Map(x => x.IsSpouse).Column("is_spouse").Not.Nullable();
            Map(x => x.IsDependent).Column("is_dependent").Not.Nullable();
            Map(x => x.SSN).Column("ssn");
            Map(x => x.SSNLast4).Column("ssn_last4");
            Map(x => x.MailingAddressLine1).Column("mailing_addr_line_1");
            Map(x => x.MailingAddressLine2).Column("mailing_addr_line_2");
            Map(x => x.MailingState).Column("mailing_state");
            Map(x => x.MailingCountryCode).Column("mailing_country_code");
            Map(x => x.HomePhoneNumber).Column("home_phone_number");
            Map(x => x.SyntheticUser).Column("synthetic_user");
            Map(x => x.PersonUniqueIdentifier).Column("person_unique_identifier");
            Map(x => x.MiddleName).Column("middle_name");
        }
    }
}