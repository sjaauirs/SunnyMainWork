using SunnyRewards.Helios.Common.Core.Domain.Models;

namespace SunnyRewards.Helios.Wallet.Core.Domain.Models
{
    public class TransactionDetailModel : BaseModel
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
