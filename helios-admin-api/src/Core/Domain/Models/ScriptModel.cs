using SunnyRewards.Helios.Common.Core.Domain.Models;

namespace SunnyRewards.Helios.Admin.Core.Domain.Models
{
    public class ScriptModel : BaseModel
    {
        public virtual long ScriptId { get; set; }
        public virtual string? ScriptCode { get; set; }
        public virtual string? ScriptName { get; set; }
        public virtual string? ScriptDescription { get; set; }
        public virtual string? ScriptJson { get; set; }
        public virtual string? ScriptSource { get; set; }

    }
}
