using SunnyRewards.Helios.Common.Core.Domain.Dtos;

namespace SunnyRewards.Helios.User.Core.Domain.Dtos
{
    public class ConsumerDeviceDto : BaseDto
    {
        public long ConsumerDeviceId { get; set; }
        public string? ConsumerDeviceCode { get; set; }
        public string? TenantCode { get; set; }
        public string? ConsumerCode { get; set; }
        public string? DeviceIdHash { get; set; }
        public string? DeviceIdEnc { get; set; }
        public string? DeviceType { get; set; }
        public string? DeviceAttrJson { get; set; }
    }
}
