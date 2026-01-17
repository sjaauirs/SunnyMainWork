using CsvHelper.Configuration.Attributes;
using System.ComponentModel.DataAnnotations;

namespace SunnyRewards.Helios.ETL.Core.Domain.Dtos
{
    public class ConsumerDepositInstructionRecord
    {
        [Name("client_task_code")]
        public string? ClientTaskCode { get; set; }

        [Name("reward_amount")]
        [Required]
        public double RewardAmount { get; set; }

        [Name("reward_type")]
        public string? RewardType { get; set; }

        [Name("person_unique_identifier")]
        [Required]
        public string? PersonUniqueIdentifier { get; set; }

        [Name("master_wallet_type_code")]
        [Required]
        public string? MasterWalletTypeCode { get; set; }

        [Name("consumer_wallet_type_code")]
        [Required]
        public string? ConsumerWalletTypeCode { get; set; }

        [Name("custom_transaction_description")]
        public string? CustomTransactionDescription { get; set; }

        [Name("redemption_ref")]
        public string? RedemptionRef { get; set; }
    }

}
