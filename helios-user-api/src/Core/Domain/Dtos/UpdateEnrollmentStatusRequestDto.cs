using System.ComponentModel.DataAnnotations;

namespace SunnyRewards.Helios.User.Core.Domain.Dtos
{
    public class UpdateEnrollmentStatusRequestDto
    {
        [Required]
        public string ConsumerCode { get; set; } = string.Empty;
        [Required]
        public string TenantCode { get; set; } = string.Empty;
        [Required]
        public string EnrollmentStatus { get; set; } = string.Empty;
    }

}
