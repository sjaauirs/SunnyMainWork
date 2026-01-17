using SunnyRewards.Helios.Common.Core.Domain.Models;

namespace SunnyRewards.Helios.Tenant.Core.Domain.Models
{
    public class CustomerModel : BaseModel
    {
        public virtual long CustomerId { get; set; }
        public virtual string CustomerCode { get; set; } = string.Empty;
        public virtual string CustomerName { get; set; } = string.Empty;
        public virtual string CustomerDescription { get; set; } = string.Empty;
    }
}
