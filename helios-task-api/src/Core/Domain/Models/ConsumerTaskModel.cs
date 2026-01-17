using SunnyRewards.Helios.Common.Core.Domain.Models;

namespace SunnyRewards.Helios.Task.Core.Domain.Models
{
    public class ConsumerTaskModel : BaseModel
    {
        public virtual string? ConsumerCode { get; set; }
        public virtual string? TenantCode { get; set; }
        public virtual long TaskId { get; set; }
        public virtual long ConsumerTaskId { get; set; }
        public virtual string TaskStatus { get; set; }
        public virtual int Progress { get; set; }
        public virtual string? Notes { get; set; }
        public virtual DateTime TaskStartTs { get; set; }
        public virtual DateTime TaskCompleteTs { get; set; }
        public virtual bool AutoEnrolled { get; set; }
        public virtual string? ProgressDetail { get; set; }
        public virtual long? ParentConsumerTaskId { get; set; } // only available if this task is a subtask
        public virtual string? WalletTransactionCode { get; set; }
        public virtual string? RewardInfoJson { get; set; }
    }
}
