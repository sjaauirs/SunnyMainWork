using SunnyRewards.Helios.ETL.Common.Domain.Models;

namespace SunnyRewards.Helios.ETL.Core.Domain.Models
{
    public class ETLCSATransactionModel : BaseModel
    {
        public virtual long CsaTransactionId { get; set; } // Primary key
        public virtual string CsaTransactionCode { get; set; } = string.Empty; // cst-<GUID>
        public virtual string TenantCode { get; set; } = string.Empty;
        public virtual string ConsumerCode { get; set; } = string.Empty;
        public virtual long WalletId { get; set; } // Foreign key to wallet table
        public virtual string TransactionRefId { get; set; } = string.Empty; // FIS provided unique txn number
        public virtual double Amount { get; set; } // Can be positive or negative
        public virtual string? Description { get; set; } // Optional description
        public virtual string Status { get; set; } = string.Empty; // NEW, APPROVED, REJECTED
        public virtual long MonetaryTransactionId { get; set; }
    }

}
