using Newtonsoft.Json;

namespace SunnyRewards.Helios.ETL.Core.Domain.Dtos.FIS
{
    public class FISFundingRuleDto
    {
        [JsonProperty("amount")]
        public double Amount { get; set; }

        [JsonProperty("enabled")]
        public bool Enabled { get; set; }

        [JsonProperty("ruleNumber")]
        public int RuleNumber { get; set; }

        [JsonProperty("periodConfig")]
        public FISPeriodConfigDto? PeriodConfig { get; set; }

        [JsonProperty("recurrenceType")]
        public string? RecurrenceType { get; set; }

        [JsonProperty("ruleDescription")]
        public string? RuleDescription { get; set; }

        [JsonProperty("masterWalletType")]
        public string? MasterWalletType { get; set; }

        [JsonProperty("consumerWalletType")]
        public string? ConsumerWalletType { get; set; }

        [JsonProperty("effectiveStartDate")]
        public required string EffectiveStartDate { get; set; }


        [JsonProperty("effectiveEndDate")]
        public required string EffectiveEndDate { get; set; }

        [JsonProperty("cohortCodes")]
        public List<string> CohortCodes { get; set; } = new();
    }
}
