using System.ComponentModel.DataAnnotations;

namespace SunnyRewards.Helios.Wallet.Core.Domain.Dtos
{
    public class GetWalletTransactionRequestDto
    {
        [Required]
        public string ConsumerCode { get; set; } = null!;
        [Required]
        public int Count { get; set; }
    }
}
