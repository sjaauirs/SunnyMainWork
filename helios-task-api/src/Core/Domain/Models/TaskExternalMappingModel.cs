using SunnyRewards.Helios.Common.Core.Domain.Models;

namespace SunnyRewards.Helios.Task.Core.Domain.Models
{
    public class TaskExternalMappingModel : BaseModel
    {
        public virtual long TaskExternalMappingId { get; set; }
        public virtual string? TenantCode { get; set; }
        public virtual string? TaskThirdPartyCode { get; set; }
        public virtual string? TaskExternalCode { get; set; }
    }
}
