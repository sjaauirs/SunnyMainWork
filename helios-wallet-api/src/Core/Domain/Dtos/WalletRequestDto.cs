using System.ComponentModel.DataAnnotations;

namespace SunnyRewards.Helios.Wallet.Core.Domain.Dtos
{
    public class WalletRequestDto
    {
        public long WalletId { get; set; }
        [Required]
        public long WalletTypeId { get; set; }
        [Required]
        public string? CustomerCode { get; set; }
        [Required]
        public string? SponsorCode { get; set; }
        [Required]
        public string? TenantCode { get; set; }
        [Required]
        public string? WalletCode { get; set; }
        public bool MasterWallet { get; set; }
        public string? WalletName { get; set; }
        public bool Active { get; set; }
        public DateTime ActiveStartTs { get; set; }
        public DateTime ActiveEndTs { get; set; }
        public double Balance { get; set; }
        public double EarnMaximum { get; set; }
        public double TotalEarned { get; set; }
    }
}
