using System.ComponentModel.DataAnnotations;

namespace SunnyRewards.Helios.Wallet.Core.Domain.Dtos
{
    public class ImportWalletTypeTransferRuleRequestDto
    {
        [Required]
        public required string TenantCode { get; set; }

        public List<ExportWalletTypeTransferRuleDto> WalletTypeTransferRules { get; set; } = new List<ExportWalletTypeTransferRuleDto>();
    }
}
