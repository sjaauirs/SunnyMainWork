using System.ComponentModel.DataAnnotations;

namespace SunnyRewards.Helios.User.Core.Domain.Dtos
{
    public class GetConsumerByTenantCodeAndDOBRequestDto
    {
        [Required]
        public string? TenantCode { get; set; }
        [Required]
        public DateTime? DOB { get; set; }
    }
}
