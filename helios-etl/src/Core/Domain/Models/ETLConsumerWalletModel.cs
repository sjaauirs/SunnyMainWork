using SunnyRewards.Helios.ETL.Common.Domain.Models;

namespace SunnyRewards.Helios.ETL.Core.Domain.Models
{
    public class ETLConsumerWalletModel : BaseModel
    {
        public virtual long ConsumerWalletId { get; set; }

        public virtual long WalletId { get; set; }

        public virtual string? TenantCode { get; set; }

        public virtual string? ConsumerCode { get; set; }

        public virtual string? ConsumerRole { get; set; }

        public virtual decimal? EarnMaximum { get; set; }
        public virtual double? TotalEarned { get; set; }
    }
}
