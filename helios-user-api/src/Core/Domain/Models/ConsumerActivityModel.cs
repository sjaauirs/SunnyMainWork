using SunnyRewards.Helios.Common.Core.Domain.Models;

namespace SunnyRewards.Helios.User.Core.Domain.Models
{
    public class ConsumerActivityModel : BaseModel
    {
        public virtual long ConsumerActivityId { get; set; }
        public virtual string? ConsumerActivityCode { get; set; }
        public virtual string? TenantCode { get; set; }
        public virtual string? ConsumerCode { get; set; }
        public virtual string? ActivitySource { get; set; }
        public virtual string? ActivityType { get; set; }
        public virtual string? ActivityDetailJson { get; set; }
    }
}
