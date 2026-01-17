using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using System.ComponentModel.DataAnnotations;

namespace SunnyRewards.Helios.User.Core.Domain.Dtos
{
    public class PostConsumerDeviceRequestDto : BaseDto
    {
        [Required]
        public string DeviceId { get; set; } = null!;
        [Required]
        public string TenantCode { get; set; } = null!;
        [Required]
        public string ConsumerCode { get; set; } = null!;
        [Required]
        public string DeviceType { get; set; } = null!;
        [Required]
        public string DeviceAttrJson { get; set; } = null!;
    }
}
