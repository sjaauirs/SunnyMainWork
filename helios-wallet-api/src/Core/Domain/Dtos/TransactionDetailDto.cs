using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using System.Text.Json.Serialization;

namespace SunnyRewards.Helios.Wallet.Core.Domain.Dtos
{
    public class TransactionDetailDto
    {
        public long TransactionDetailId { get; set; }
        public string? TransactionDetailType { get; set; }
        public string? ConsumerCode { get; set; }
        public string? TaskRewardCode { get; set; }
        public string? Notes { get; set; }
        public string? RedemptionRef { get; set; }
        public string? RedemptionItemDescription { get; set; }
        public string? RewardDescription { get; set; }
        public bool IsSpouse { get; set; }
        public bool IsDependent { get; set; }
        public DateTime CreateTs { get; set; }
        public bool IsPending { get; set; } = false;

    }
}
