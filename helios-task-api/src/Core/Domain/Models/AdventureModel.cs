using SunnyRewards.Helios.Common.Core.Domain.Models;

namespace SunnyRewards.Helios.Task.Core.Domain.Models
{
    public class AdventureModel : BaseModel
    {
        public virtual long AdventureId { get; set; }
        public virtual string AdventureCode { get; set; } = string.Empty;
        public virtual string AdventureConfigJson { get; set; } = string.Empty;
        public virtual string? CmsComponentCode { get; set; }
    }
}
