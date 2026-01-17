using System.ComponentModel.DataAnnotations;

namespace SunnyRewards.Helios.User.Core.Domain.Dtos
{
    public class UpdatePhoneNumberRequestDto
    {
        [Required]
        public long PhoneNumberId { get; set; }
        [Required]
        public string ConsumerCode { get; set; }
        [Required]
        public string TenantCode { get; set; }
        [Required]
        public long PhoneTypeId { get; set; }
        public string? PhoneNumber { get; set; }
        public bool IsVerified { get; set; }
        public DateTime? VerifiedDate { get; set; }
        public string? Source { get; set; }
        [Required]
        public string? UpdateUser { get; set; }
    }
}
