using SunnyRewards.Helios.ETL.Common.Domain.Models;

namespace SunnyRewards.Helios.ETL.Core.Domain.Models
{
    public class ETLTransactionModel : BaseModel
    {
        public virtual long TransactionId { get; set; }
        public virtual long WalletId { get; set; }
        public virtual string? TransactionCode { get; set; }
        public virtual string? TransactionType { get; set; }
        public virtual double? PreviousBalance { get; set; }
        public virtual double? TransactionAmount { get; set; }
        public virtual double? Balance { get; set; }
        public virtual string? PrevWalletTxnCode { get; set; }
        public virtual long TransactionDetailId { get; set; }
    }
}
