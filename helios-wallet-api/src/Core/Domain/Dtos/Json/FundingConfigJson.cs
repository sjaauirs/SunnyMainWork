using Newtonsoft.Json;

namespace SunnyRewards.Helios.Wallet.Core.Domain.Dtos.Json
{
    public class FundingConfigJson
    {
        [JsonProperty("fundingRules")]
        public List<FundingRule>? FundingRules { get; set; }
    }

    public class FundingRule
    {
        [JsonProperty("amount")]
        public decimal Amount { get; set; }

        [JsonProperty("enabled")]
        public bool Enabled { get; set; }

        [JsonProperty("ruleNumber")]
        public int RuleNumber { get; set; }

        [JsonProperty("periodConfig")]
        public FundingPeriodConfig? PeriodConfig { get; set; }

        [JsonProperty("recurrenceType")]
        public string? RecurrenceType { get; set; }

        [JsonProperty("ruleDescription")]
        public string? RuleDescription { get; set; }

        [JsonProperty("masterWalletType")]
        public string? MasterWalletType { get; set; }

        [JsonProperty("consumerWalletType")]
        public string? ConsumerWalletType { get; set; }
    }

    public class FundingPeriodConfig
    {
        [JsonProperty("fundDate")]
        public int FundDate { get; set; }

        [JsonProperty("interval")]
        public string? Interval { get; set; }
    }

}
