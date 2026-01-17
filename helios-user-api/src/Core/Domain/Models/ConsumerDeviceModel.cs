using SunnyRewards.Helios.Common.Core.Domain.Models;

namespace SunnyRewards.Helios.User.Core.Domain.Models
{
    public class ConsumerDeviceModel : BaseModel
    {
        public virtual long ConsumerDeviceId { get; set; }
        public virtual string? ConsumerDeviceCode { get; set; }
        public virtual string? TenantCode { get; set; }
        public virtual string? ConsumerCode { get; set; }
        public virtual string? DeviceIdHash { get; set; }
        public virtual string? DeviceIdEnc { get; set; }
        public virtual string? DeviceType { get; set; }
        public virtual string? DeviceAttrJson { get; set; }
    }

}
