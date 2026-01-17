using SunnyRewards.Helios.Common.Core.Domain.Models;

namespace SunnyRewards.Helios.Admin.Core.Domain.Models
{
    public class TaskRewardScriptResultModel : BaseModel
    {
        public virtual long TaskRewardScriptId { get; set; }
        public virtual long TenantTaskRewardScriptId { get; set; }
        public virtual string? ConsumerCode { get; set; }
        public virtual string? ExecutionContextJson { get; set; }
        public virtual string? ExecutionResultJson { get; set; }

    }
}
