using System.ComponentModel.DataAnnotations;

namespace SunnyRewards.Helios.Wallet.Core.Domain.Dtos
{
    public class PurseFundingRequestDto
    {
        [Required]
        public string TenantCode { get; set; } = null!;
        [Required]
        public string ConsumerCode { get; set; } = null!;
        [Required]
        public string MasterWalletType { get; set; } = null!;
        [Required]
        public string ConsumerWalletType { get; set; } = null!;
        [Required]
        public double Amount { get; set; }
        [Required]
        public string TransactionDetailType { get; set; } = null!;
        public string? RewardDescription { get; set; }
        [Required]
        public int RuleNumber { get; set; }

    }
}
