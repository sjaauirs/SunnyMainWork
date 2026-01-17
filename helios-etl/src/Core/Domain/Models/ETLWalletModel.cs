using SunnyRewards.Helios.ETL.Common.Domain.Models;

namespace SunnyRewards.Helios.ETL.Core.Domain.Models
{
    public class ETLWalletModel : BaseModel
    {
        public virtual long WalletId { get; set; }

        public virtual long WalletTypeId { get; set; }

        public virtual string? CustomerCode { get; set; }

        public virtual string? SponsorCode { get; set; }

        public virtual string? TenantCode { get; set; }

        public virtual string? WalletCode { get; set; }

        public virtual bool MasterWallet { get; set; }

        public virtual string? WalletName { get; set; }

        public virtual bool Active { get; set; }

        public virtual DateTime ActiveStartTs { get; set; }

        public virtual DateTime ActiveEndTs { get; set; }

        public virtual double Balance { get; set; }

        public virtual double EarnMaximum { get; set; }

        public virtual double TotalEarned { get; set; }

        public virtual double LeftToEarn { get; set; }
    }
}
