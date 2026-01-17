using System.ComponentModel.DataAnnotations;
namespace SunnyRewards.Helios.Wallet.Core.Domain.Dtos
{
    public class GetWalletTypeTransferRule 
    {
        [Required]
        public required string TenantCode { get; set; } 

        [Required]
        public required string ConsumerCode { get; set; } 

    }
}

