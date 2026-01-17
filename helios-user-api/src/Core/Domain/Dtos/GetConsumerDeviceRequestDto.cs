using System.ComponentModel.DataAnnotations;

namespace SunnyRewards.Helios.User.Core.Domain.Dtos
{
    public class GetConsumerDeviceRequestDto
    {
        [Required]
        public string TenantCode { get; set; } = null!;
        [Required]
        public string ConsumerCode { get; set; } = null!;
    }
}
