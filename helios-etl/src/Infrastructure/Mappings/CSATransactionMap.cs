using SunnyRewards.Helios.ETL.Common.Mappings;
using SunnyRewards.Helios.ETL.Core.Domain.Models;


namespace SunnyRewards.Helios.ETL.Infrastructure.Mappings
{
    public class CSATransactionMap : BaseMapping<ETLCSATransactionModel>
    {
        public CSATransactionMap()
        {
            Schema("fis");
            Table("csa_transaction");
            Id(x => x.CsaTransactionId).Column("csa_transaction_id").GeneratedBy.Identity();
            Map(x => x.CsaTransactionCode).Column("csa_transaction_code");
            Map(x => x.TenantCode).Column("tenant_code");
            Map(x => x.ConsumerCode).Column("consumer_code");
            Map(x => x.WalletId).Column("wallet_id");
            Map(x => x.TransactionRefId).Column("transaction_ref_id");
            Map(x => x.Amount).Column("amount");
            Map(x => x.Description).Column("description");
            Map(x => x.Status).Column("status");
            Map(x => x.MonetaryTransactionId).Column("monetary_transaction_id");
            Map(x => x.CreateTs).Column("create_ts");
            Map(x => x.UpdateTs).Column("update_ts");
            Map(x => x.UpdateUser).Column("update_user");
            Map(x => x.CreateUser).Column("create_user");
            Map(x => x.DeleteNbr).Column("delete_nbr");
        }

    }
}
