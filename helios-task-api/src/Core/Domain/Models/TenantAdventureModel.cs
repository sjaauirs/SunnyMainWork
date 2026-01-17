using SunnyRewards.Helios.Common.Core.Domain.Models;

namespace SunnyRewards.Helios.Task.Core.Domain.Models
{
    public class TenantAdventureModel : BaseModel
    {
        public virtual long TenantAdventureId { get; set; }
        public virtual string TenantAdventureCode { get; set; } = string.Empty;
        public virtual string TenantCode { get; set; } = string.Empty;
        public virtual long AdventureId { get; set; }
    }

}
