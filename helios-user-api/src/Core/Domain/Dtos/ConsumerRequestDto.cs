using System.ComponentModel.DataAnnotations;

namespace SunnyRewards.Helios.User.Core.Domain.Dtos
{
    public class ConsumerRequestDto
    {
        [Required]
        public long PersonId { get; set; }
        [Required]
        public string? TenantCode { get; set; }
        [Required]
        public string? ConsumerCode { get; set; }
        public DateTime RegistrationTs { get; set; }
        public DateTime EligibleStartTs { get; set; }
        public DateTime EligibleEndTs { get; set; }
        [Required]
        public bool Registered { get; set; }
        [Required]
        public bool Eligible { get; set; }
        [Required]
        public string? MemberNbr { get; set; }
        public string? SubscriberMemberNbr { get; set; }
        public string? ConsumerAttribute { get; set; }
        [Required]
        public string? AnonymousCode { get; set; }
        public bool SubscriberOnly { get; set; }
        public bool IsSsoAuthenticated { get; set; }
        [Required]
        public string? EnrollmentStatus { get; set; }
        [Required]
        public string? EnrollmentStatusSource { get; set; }
        public string? Auth0UserName { get; set; }
        [Required]
        public string? MemberId { get; set; }
    }
}
