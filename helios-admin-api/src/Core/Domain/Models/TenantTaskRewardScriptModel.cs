using SunnyRewards.Helios.Common.Core.Domain.Models;

namespace SunnyRewards.Helios.Admin.Core.Domain.Models
{
    public class TenantTaskRewardScriptModel : BaseModel
    {
        public virtual long TenantTaskRewardScriptId { get; set; }
        public virtual string? TenantTaskRewardScriptCode { get; set; }
        public virtual string? TenantCode { get; set; }
        public virtual string? TaskRewardCode { get; set; }
        public virtual string? ScriptType { get; set; }
        public virtual long ScriptId { get; set; }

    }
}
