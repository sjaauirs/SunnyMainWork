using System.ComponentModel.DataAnnotations;

namespace SunnyRewards.Helios.Wallet.Core.Domain.Dtos
{
    public class CsaWalletTransactionsRequestDto
    {
        [Required]
        public long WalletId { get; set; }
        [Required]
        public string TenantCode { get; set; } = null!;
        [Required]
        public string ConsumerCode { get; set; } = null!;
        [Required]
        public double Amount { get; set; }
        [Required]
        public string TenantConfig { get; set; } = null!;
        public string Description { get; set; } = null!;
    }
}
