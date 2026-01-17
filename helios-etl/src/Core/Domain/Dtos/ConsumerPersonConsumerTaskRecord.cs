namespace SunnyRewards.Helios.ETL.Core.Domain.Dtos
{
    public class ConsumerPersonConsumerTaskRecord
    {
        //consumerDto
        public long ConsumerId { get; set; }

        public long PersonId { get; set; }

        public string? TenantCode { get; set; }

        public string? ConsumerCode { get; set; }

        public DateTime RegistrationTs { get; set; }

        public DateTime EligibleStartTs { get; set; }

        public DateTime EligibleEndTs { get; set; }

        public bool Registered { get; set; }

        public bool Eligible { get; set; }

        public string? MemberNbr { get; set; }

        public string? SubscriberMemberNbr { get; set; }

        public string? ConsumerAttribute { get; set; }

        public string? AnonymousCode { get; set; }

        public bool SubscriberOnly { get; set; }

        public bool IsSSOUser { get; set; }

        public string? EnrollmentStatus { get; set; }

        public string? EnrollmentStatusSource { get; set; }

        public string? OnBoardingState { get; set; }

        public string? AgreementStatus { get; set; }
        public string? RegionCode { get; set; }
        public string? SubsciberMemberNbrPrefix { get; set; }
        public string? MemberNbrPrefix { get; set; }
        public string? PlanId { get; set; }
        public string? SubgroupId { get; set; }
        public string? PlanType { get; set; }

        //personDto
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

        public bool SyncRequired { get; set; }
        public List<string>? SyncOptions { get; set; }
        public bool SyntheticUser { get; set; }
        public string? MiddleName { get; set; }

        //consumer task Data
        public long ConsumerTaskId { get; set; }
        public long TaskId { get; set; }
        public string? TaskStatus { get; set; }
        public int Progress { get; set; }
        public string? Notes { get; set; } = string.Empty;
        public DateTime TaskStartTs { get; set; }
        public DateTime? TaskCompleteTs { get; set; }
        public bool AutoEnrolled { get; set; }
        public string? ProgressDetail { get; set; }
        public long? ParentConsumerTaskId { get; set; }
        public DateTime CreateTs { get; set; }
        public string? CreateUser { get; set; } = string.Empty;
        public string? WalletTransactionCode { get; set; } = string.Empty;
        public string? RewardInfoJson { get; set; }

    }

}
