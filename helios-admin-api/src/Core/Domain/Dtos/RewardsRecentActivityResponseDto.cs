using SunnyRewards.Helios.Common.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Admin.Core.Domain.Dtos
{
    public class RewardsRecentActivityResponseDto : BaseResponseDto
    {
        public double MaxAvailable { get; set; }
        public double LeftToEarn { get; set; }
        public double AvailableToSpend { get; set; }
        public double TotalEarned { get; set; }
        public List<RecentTransaction> RecentTransactions { get; set; } = new List<RecentTransaction>();
    }

    public class RecentTransaction
    {
        public string? TransactionCode { get; set; }
        public string? TransactionType { get; set; }
        public DateTime TransactionDate { get; set; }
        public string? Status { get; set; }
        public string? Description { get; set; }
        public double TransactionAmount { get; set; }
        public string? TaskRewardCode { get; set; }
        public string? WalletTypeCode { get; set; }
        public string? WalletTypeName { get; set; }
        public string? Notes { get; set; }
        public string? TransactionDetailType { get; set; }
        public bool IsPending { get; set; }
    }
}
