using SunnyRewards.Helios.ETL.Common.Domain.Models;

namespace SunnyRewards.Helios.ETL.Core.Domain.Models
{
    public class ETLTransactionDetailModel : BaseModel
    {
        public virtual long TransactionDetailId { get; set; }
        public virtual string? TransactionDetailType { get; set; }
        public virtual string? ConsumerCode { get; set; }
        public virtual string? TaskRewardCode { get; set; }
        public virtual string? Notes { get; set; }
        public virtual string? RedemptionRef { get; set; }
        public virtual string? RedemptionItemDescription { get; set; }
        public virtual string? RewardDescription { get; set; }
    }
}
