using System.ComponentModel.DataAnnotations;

namespace Sunny.Benefits.Bff.Core.Domain.Dtos
{
    public class FundTransferRequestDto
    {
        [Required]
        public string TenantCode { get; set; } = null!;

        [Required]
        public string ConsumerCode { get; set; } = null!;

        [Required]
        public string SourceWalletType { get; set; } = null!;

        [Required]
        public string TargetWalletType { get; set; } = null!;

        [Required]
        public string PurseLabel { get; set; } = null!;

        [Required]
        public double Amount { get; set; }
    }

}
