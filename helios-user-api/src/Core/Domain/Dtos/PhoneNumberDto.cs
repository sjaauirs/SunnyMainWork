using SunnyRewards.Helios.Common.Core.Domain.Dtos;

namespace SunnyRewards.Helios.User.Core.Domain.Dtos
{
    public class PhoneNumberDto : BaseDto
    {
        public long PhoneNumberId { get; set; }
        public long PersonId { get; set; }
        public long PhoneTypeId { get; set; }
        public string? PhoneNumberCode { get; set; }
        public string? PhoneNumber { get; set; }
        public bool IsPrimary { get; set; }
        public bool IsVerified { get; set; }
        public DateTime? VerifiedDate { get; set; }
        public string? Source { get; set; }
    }
}
