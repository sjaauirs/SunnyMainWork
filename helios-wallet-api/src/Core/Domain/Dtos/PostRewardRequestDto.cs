using SunnyRewards.Helios.Wallet.Core.Domain.Models;

namespace SunnyRewards.Helios.Wallet.Core.Domain.Dtos
{
    public class PostRewardRequestDto
    {
        public string? MasterWalletTypeCode { get; set; }
        public string? ConsumerWalletTypeCode { get; set; }
        public string? TenantCode { get; set; }
        public string? ConsumerCode { get; set; }
        public string? TaskRewardCode { get; set; }
        public double RewardAmount { get; set; }
        public string? RewardDescription { get; set; }
        public bool SplitRewardOverflow { get; set; } = false;

    }

    public class ProcessRewardsRequest 
    {
        public string? TenantCode { get; set; }
        public string? ConsumerCode { get; set; }
        public string? TaskRewardCode { get; set; }
        public double RewardAmount { get; set; }
        public string? RewardDescription { get; set; }
        public bool SplitRewardOverflow { get; set; } = true;
        public WalletModel? ConsumerWalletModel { get; set; }
        public WalletModel? MasterWalletModel { get; set; }
 
    }
}
