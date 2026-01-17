using SunnyRewards.Helios.Common.Core.Domain.Models;

namespace SunnyRewards.Helios.Task.Core.Domain.Models
{
    public class SubTaskModel : BaseModel
    {
        public virtual long SubTaskId { get; set; }
        public virtual long ParentTaskRewardId { get; set; }
        public virtual long ChildTaskRewardId { get; set; }
        public virtual string? ConfigJson { get; set; }
    }
}
