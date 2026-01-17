using SunnyRewards.Helios.Common.Core.Domain.Models;

namespace SunnyRewards.Helios.Wallet.Core.Domain.Models
{
    public class ConsumerWalletModel : XminBaseModel
    {
        public virtual long ConsumerWalletId { get; set; }
        public virtual long WalletId { get; set; }
        public virtual string? TenantCode { get; set; }
        public virtual string? ConsumerCode { get; set; }
        public virtual string? ConsumerRole { get; set; }
        public virtual decimal? EarnMaximum { get; set; }
        public virtual double TotalEarned { get; set; }
    }
}