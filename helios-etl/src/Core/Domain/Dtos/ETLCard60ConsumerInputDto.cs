using CsvHelper.Configuration.Attributes;

namespace SunnyRewards.Helios.ETL.Core.Domain.Dtos
{
    public class ETLCard60ConsumerInputDto
    {
        [Name("consumer_code")]
        public string? ConsumerCode { get; set; }

        [Name("consumer_wallet_type_code")]
        public string? ConsumerWalletTypeCode { get; set; }

        [Name("redemption_amount")]
        public double RedemptionAmount { get; set; }

        [Name("custom_transaction_description")]
        public string? CustomTransactionDescription { get; set; }
    }
}
