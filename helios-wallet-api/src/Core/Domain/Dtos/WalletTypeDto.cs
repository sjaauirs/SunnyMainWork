using SunnyRewards.Helios.Common.Core.Domain.Dtos;
namespace SunnyRewards.Helios.Wallet.Core.Domain.Dtos
{
    public class WalletTypeDto
    {

        public long WalletTypeId { get; set; }
        public string? WalletTypeCode { get; set; }
        public string? WalletTypeName { get; set; }
        public string? WalletTypeLabel { get; set; }
        public string? ShortLabel { get; set; }
        public  bool IsExternalSync { get; set; }
        public virtual string ConfigJson { get; set; } = string.Empty;
    }
}

