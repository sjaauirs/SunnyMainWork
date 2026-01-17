using SunnyRewards.Helios.Common.Core.Domain.Dtos;

namespace SunnyRewards.Helios.User.Core.Domain.Dtos
{
    public class ConsumerDto : BaseDto
    {
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
        public bool IsSsoAuthenticated { get; set; }
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
        public string? AgreementFileName { get; set; }
        public string? Auth0UserName { get; set; }
        public string? MemberId { get; set; }
        public string? MemberType { get; set; }
        public string? SubscriptionStatusJson { get; set; }

    }
}
