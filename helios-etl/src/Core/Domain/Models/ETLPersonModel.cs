using SunnyRewards.Helios.ETL.Common.Domain.Models;

namespace SunnyRewards.Helios.ETL.Core.Domain.Models
{
    public class ETLPersonModel : BaseModel
    {
        public virtual long PersonId { get; set; }
        public virtual string? PersonCode { get; set; }
        public virtual string? FirstName { get; set; }
        public virtual string? LastName { get; set; }
        public virtual string? LanguageCode { get; set; }
        public virtual DateTime? MemberSince { get; set; }
        public virtual string? Email { get; set; }
        public virtual string? City { get; set; }
        public virtual string? Country { get; set; }
        public virtual int YearOfBirth { get; set; }
        public virtual string? PostalCode { get; set; }
        public virtual string? PhoneNumber { get; set; }
        public virtual string? Region { get; set; }

        public virtual string? Gender { get; set; }
        public virtual DateTime? DOB { get; set; }

        public virtual bool IsSpouse { get; set; }
        public virtual bool IsDependent { get; set; }
        public virtual string? SSN { get; set; }
        public virtual string? SSNLast4 { get; set; }
        public virtual string? MailingAddressLine1 { get; set; }
        public virtual string? MailingAddressLine2 { get; set; }
        public virtual string? MailingState { get; set; }
        public virtual string? MailingCountryCode { get; set; }
        public virtual string? HomePhoneNumber { get; set; }
        public virtual bool SyntheticUser { get; set; }
        public virtual string? PersonUniqueIdentifier { get; set; }
        public virtual string? MiddleName { get; set; }

        public virtual int Age
        {
            get
            {
                if (DOB.HasValue)
                    return (DateTime.UtcNow.Year - DOB.Value.Year);

                return 0;
            }
        }
    }
}
