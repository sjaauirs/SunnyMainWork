using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using SunnyRewards.Helios.ETL.Core.Domain.Dtos.Validation;
using SunnyRewards.Helios.User.Core.Domain.Dtos;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace SunnyRewards.Helios.ETL.Core.Domain.Dtos
{
    public class MemberEnrollmentDetailDto
    {
        public MemberDetailDto MemberDetail { get; set; } = new MemberDetailDto();
        public EnrollmentDetailDto EnrollmentDetail { get; set; } = new EnrollmentDetailDto();
    }

    public class MemberDetailDto
    {
        [Required]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        public string LastName { get; set; } = string.Empty;

        [Required]
        public string LanguageCode { get; set; } = string.Empty;

        [Required]
        public DateTime MemberSince { get; set; }

        public string? Email { get; set; } = string.Empty;

        [Required]
        public string City { get; set; } = string.Empty;

        [Required]
        public string Country { get; set; } = string.Empty;

        [Required]
        public string PostalCode { get; set; } = string.Empty;

        [Required]
        public string PhoneNumber { get; set; } = string.Empty;

        public string Region { get; set; } = " ";

        [Required]
        public DateTime Dob { get; set; }

        [Required]
        public string Gender { get; set; } = string.Empty;

        public string SSNLast4 { get; set; } = string.Empty;

        [Required]
        public string MailingAddressLine1 { get; set; } = string.Empty;

        [Required]
        public string MailingAddressLine2 { get; set; } = string.Empty;

        [Required]
        public string MailingState { get; set; } = string.Empty;

        [Required]
        public string MailingCountryCode { get; set; } = string.Empty;

        [Required]
        public string HomePhoneNumber { get; set; } = string.Empty;
        public string MiddleName { get; set; } = string.Empty;
        public string HomeAddressLine1 { get; set; } = string.Empty;
        public string HomeAddressLine2 { get; set; } = string.Empty;
        public string HomeCity { get; set; } = string.Empty;
        public string HomeState { get; set; } = string.Empty;
        public string HomePostalCode { get; set; } = string.Empty;
        public string? PersonUniqueIdentifier { get; set; }
        public string Source { get; set; } = string.Empty;
        public int Age { get; set; } 

        public PersonDto ToPersonDto()
        {
            return new PersonDto()
            {
                FirstName = FirstName,
                LastName = LastName,
                LanguageCode = LanguageCode,
                City = City,
                Country = Country,
                PostalCode = PostalCode,
                PhoneNumber = PhoneNumber,
                Region = Region,
                DOB = Dob,
                Gender = Gender,
                MemberSince = MemberSince,
                Email = Email,
                SSNLast4 = SSNLast4,
                MailingAddressLine1 = MailingAddressLine1,
                MailingAddressLine2 = MailingAddressLine2,
                MailingCountryCode = MailingCountryCode,
                MailingState = MailingState,
                HomePhoneNumber = HomePhoneNumber,
                PersonUniqueIdentifier = PersonUniqueIdentifier,
                Age = Age,
            };
        }
    }

    public class EnrollmentDetailDto
    {
        /// <summary>
        /// Partner Code assgined by Rewards System for the customer
        /// </summary>
        [Required]
        public string PartnerCode { get; set; } = string.Empty;

        public string? CustomerCode { get; set; } = string.Empty;
        public string? CustomerLabel { get; set; } = string.Empty;
        public string? TenantCode { get; set; } = string.Empty;
        public string? Action { get; set; } = string.Empty;

        /// <summary>
        /// Member number of the member as assigned by the Healthcare System
        /// </summary>
        [RequiredIf(nameof(SubscriberOnly), false)]
        public string MemberNbr { get; set; } = string.Empty;

        /// <summary>
        /// Member number of the Primary Subscriber in the Healthcare System
        /// if current Member is the Primary Subscriber then this value needs to
        /// be equal to MemberNbr, if not, then the Primary Subscriber must already be
        /// registered in the Rewards System
        /// </summary>
        [Required]
        public string SubscriberMemberNbr { get; set; } = string.Empty;

        [Required]
        public DateTime RegistrationTs { get; set; }

        [Required]
        public DateTime EligibleStartTs { get; set; }

        [Required]
        public DateTime EligibleEndTs { get; set; }

        [DefaultValue(false)]
        public bool SubscriberOnly { get; set; }
        public string RegionCode { get; set; } = string.Empty;
        public string SubsciberMemberNbrPrefix { get; set; } = string.Empty;
        public string MemberNbrPrefix { get; set; } = string.Empty;
        public string PlanId { get; set; } = string.Empty;
        public string SubgroupId { get; set; } = string.Empty;
        public string PlanType { get; set; } = string.Empty;
        public string MemberId { get; set; } = string.Empty;
        public string MemberType { get; set; } = string.Empty;
        public string? ConsumerAttribute { get; set; } 
        public string PublishStatus { get; set; } = string.Empty;

        public bool IsMainSubscriber()
        {
            return (this.MemberNbr == this.SubscriberMemberNbr);
        }
        public bool IsSSOUser { get; set; }

        public ConsumerDto ToConsumerDto(string tenantCode)
        {
            return new ConsumerDto()
            {
                MemberNbr = SubscriberOnly ? SubscriberMemberNbr : MemberNbr,
                RegistrationTs = RegistrationTs,
                EligibleStartTs = EligibleStartTs,
                EligibleEndTs = EligibleEndTs,
                TenantCode = tenantCode,
                SubscriberMemberNbr = SubscriberMemberNbr,
                MemberId = MemberId,
                MemberType = MemberType,
                ConsumerAttribute = ConsumerAttribute
            };
        }
    }
}
