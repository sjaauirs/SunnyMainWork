using SunnyRewards.Helios.Common.Core.Domain.Models;

namespace SunnyRewards.Helios.Admin.Core.Domain.Models
{
    public class EventHandlerScriptModel : BaseModel
    {
        public virtual long EventHandlerId { get; set; }
        public virtual string? EventHandlerCode { get; set; }
        public virtual string? TenantCode { get; set; }
        public virtual long ScriptId { get; set; }
        public virtual string? EventType { get; set; }
        public virtual string? EventSubType { get; set; }
    }
}
