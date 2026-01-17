using SunnyRewards.Helios.Common.Core.Domain.Models;

namespace SunnyRewards.Helios.Wallet.Core.Domain.Models
{
    public class WalletTypeModel : BaseModel
    {
        public virtual long WalletTypeId { get; set; }
        public virtual string? WalletTypeCode { get; set; }
        public virtual string? WalletTypeName { get; set; }
        public virtual string? WalletTypeLabel { get; set; }
        public virtual string? ShortLabel { get; set; }
        public virtual bool IsExternalSync { get; set; }
        public virtual string ConfigJson { get; set; } = string.Empty;
    }
}
