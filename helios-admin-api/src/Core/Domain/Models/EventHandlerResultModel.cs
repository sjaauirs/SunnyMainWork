using SunnyRewards.Helios.Common.Core.Domain.Models;

namespace SunnyRewards.Helios.Admin.Core.Domain.Models
{
    public class EventHandlerResultModel : BaseModel
    {
        public virtual long EventHandlerResultId { get; set; }
        public virtual string EventCode { get; set; }
        public virtual long EventHandlerScriptId { get; set; }
        public virtual string EventHandlerName { get; set; }
        public virtual string EventData { get; set; } // JSON stored as string
        public virtual string ResultStatus { get; set; }
        public virtual string? ResultDescriptionJson { get; set; } // Nullable JSON stored as string

    }

}
