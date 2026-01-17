using SunnyRewards.Helios.Common.Core.Domain.Models;

namespace SunnyRewards.Helios.Task.Core.Domain.Models
{
    public class TaskRewardTypeModel : BaseModel
    {
        public virtual long RewardTypeId { get; set; }
        public virtual string? RewardTypeName { get; set; }
        public virtual string? RewardTypeDescription { get; set; }
        public virtual string RewardTypeCode { get; set; }
    }
}
