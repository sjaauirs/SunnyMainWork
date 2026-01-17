using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.User.Core.Domain.enums;

namespace SunnyRewards.Helios.User.Core.Domain.Dtos
{
    public class PersonDto : BaseDto
    {
        public long PersonId { get; set; }
        public string? PersonCode { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? LanguageCode { get; set; }
        public DateTime MemberSince { get; set; }
        public string? Email { get; set; }
        public string? City { get; set; }
        public string? Country { get; set; }
        public int YearOfBirth { get; set; }
        public string? PostalCode { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Region { get; set; }
        public DateTime DOB { get; set; }
        public string? Gender { get; set; }
        public bool IsSpouse { get; set; }
        public bool IsDependent { get; set; }
        public string? SSNLast4 { get; set; }
        public string? MailingAddressLine1 { get; set; }
        public string? MailingAddressLine2 { get; set; }
        public string? MailingState { get; set; }
        public string? MailingCountryCode { get; set; }
        public string? HomePhoneNumber { get; set; }
        public string? OnBoardingState { get; set; } = OnboardingState.NOT_STARTED.ToString();
        public bool SyncRequired { get; set; }
        public List<string>? SyncOptions { get; set; }
        public bool SyntheticUser { get; set; }
        public string? MiddleName { get; set; }
        public string? PersonUniqueIdentifier { get; set; }
        public int Age { get; set; }

    }
}
