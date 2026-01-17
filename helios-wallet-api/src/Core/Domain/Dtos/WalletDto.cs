using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using System.Text.Json.Serialization;

namespace SunnyRewards.Helios.Wallet.Core.Domain.Dtos
{
    public class WalletDto : XminBaseDto
    {
        public long WalletId { get; set; }
        public long WalletTypeId { get; set; }
        public string? CustomerCode { get; set; }
        public string? SponsorCode { get; set; }
        public string? TenantCode { get; set; }
        public string? WalletCode { get; set; }
        public bool MasterWallet { get; set; }
        public string? WalletName { get; set; }
        public bool Active { get; set; }
        public DateTime ActiveStartTs { get; set; }
        public DateTime ActiveEndTs { get; set; }

        public DateTime RedeemEndTs { get; set; }

        public double Balance { get; set; }
        public double EarnMaximum { get; set; }
        public double TotalEarned { get; set; }
        public double PendingTasksTotalRewardAmount { get; set; }

        // transient / computed field (in BFF)
        public double LeftToEarn { get; set; }

        public DateTime CreateTs { get; set; }
        [JsonIgnore]
        public string CreateUser { get; set; } = string.Empty;
        public FundingDescriptionDto? FundingDescription { get; set; }

        public bool IsDeactivated { get; set; } = false;

        public int Index { get; set; } = 0;

    }
}
