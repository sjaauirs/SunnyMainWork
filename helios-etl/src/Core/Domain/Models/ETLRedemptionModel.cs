using SunnyRewards.Helios.ETL.Common.Domain.Models;

namespace SunnyRewards.Helios.ETL.Core.Domain.Models
{
    public class ETLRedemptionModel : BaseModel
    {
        public virtual long RedemptionId { get; set; }
        public virtual long SubTransactionId { get; set; }
        public virtual long AddTransactionId { get; set; }
        public virtual long? RevertSubTransactionId { get; set; }
        public virtual long? RevertAddTransactionId { get; set; }
        public virtual string? RedemptionStatus { get; set; }   // IN_PROGRESS, COMPLETED, REVERTED
        public virtual string? Notes { get; set; }
        public virtual string? RedemptionItemDescription { get; set; }
        public virtual string? RedemptionRef { get; set; }
        public virtual DateTime RedemptionStartTs { get; set; }
        public virtual DateTime? RedemptionCompleteTs { get; set; }
        public virtual DateTime RedemptionRevertTs { get; set; }
        public virtual string? RedemptionItemData { get; set; }
    }
}
