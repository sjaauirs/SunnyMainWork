using SunnyRewards.Helios.ETL.Common.Mappings;
using SunnyRewards.Helios.ETL.Core.Domain.Models;

namespace SunnyRewards.Helios.ETL.Infrastructure.Mappings
{
    public class TransactionMap : BaseMapping<ETLTransactionModel>
    {
        public TransactionMap()
        {
            Table("transaction");
            Schema("wallet");
            Id(x => x.TransactionId).Column("transaction_id").GeneratedBy.Identity();
            Map(x => x.WalletId).Column("wallet_id");
            Map(x => x.TransactionCode).Column("transaction_code");
            Map(x => x.TransactionType).Column("transaction_type");
            Map(x => x.PreviousBalance).Column("previous_balance");
            Map(x => x.TransactionAmount).Column("transaction_amount");
            Map(x => x.Balance).Column("balance");
            Map(x => x.PrevWalletTxnCode).Column("prev_wallet_txn_code");
            Map(x => x.CreateTs).Column("create_ts");
            Map(x => x.DeleteNbr).Column("delete_nbr");
            Map(x => x.CreateUser).Column("create_user");
            Map(x => x.TransactionDetailId).Column("transaction_detail_id");
        }
    }
}
