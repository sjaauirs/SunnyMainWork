using SunnyRewards.Helios.Common.Core.Domain.Models;

namespace SunnyRewards.Helios.Tenant.Core.Domain.Models
{
    public class SponsorModel : BaseModel
    {
        public virtual long SponsorId { get; set; }
        public virtual long? CustomerId { get; set; }
        public virtual string? SponsorCode { get; set; }
        public virtual string? SponsorName { get; set; }
        public virtual string? SponsorDescription { get; set; }
    }
}
