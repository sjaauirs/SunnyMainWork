using SunnyRewards.Helios.Common.Core.Domain.Models;

namespace SunnyRewards.Helios.Tenant.Core.Domain.Models
{
    public class ComponentCatalogueModel : BaseModel
    {
        public virtual long Pk { get; set; }
        public virtual long ComponentTypeFk { get; set; }
        public virtual string ComponentName { get; set; } = string.Empty;
        public virtual bool IsActive { get; set; }
    }
}
