using SunnyRewards.Helios.Common.Core.Domain.Models;

namespace SunnyRewards.Helios.Task.Core.Domain.Models
{
    public class TaskRewardCollectionModel:BaseModel
    {
        public virtual long TaskRewardCollectionId { get; set; }
        public virtual long ParentTaskRewardId { get; set; }
        public virtual long ChildTaskRewardId { get; set; }
        public virtual string UniqueChildCode { get; set; } = null!;
        public virtual string ConfigJson { get; set; } = null!;
    }
}
