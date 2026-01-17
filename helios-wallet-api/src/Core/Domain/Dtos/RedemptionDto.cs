using SunnyRewards.Helios.Common.Core.Domain.Dtos;
namespace SunnyRewards.Helios.Wallet.Core.Domain.Dtos
{
    public class RedemptionDto : XminBaseDto
    {
        public long SubTransactionId { get; set; }
        public long AddTransactionId { get; set; }
        public long RevertSubTransactionId { get; set; }
        public long RevertAddTransactionId { get; set; }
        public string? RedemptionStatus { get; set; }   // IN_PROGRESS, COMPLETED, REVERTED
        public string? Notes { get; set; }
        public string? RedemptionRef { get; set; }
        public DateTime RedemptionStartTs { get; set; }
        public DateTime RedemptionCompleteTs { get; set; }
        public DateTime RedemptionRevertTs { get; set; }
        public string? RedemptionItemData { get; set; }
    }
}
