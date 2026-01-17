using Newtonsoft.Json;

namespace SunnyRewards.Helios.Wallet.Core.Domain.Dtos
{
    public class ConsumerTaskRewardInfoDto
    {
        [JsonProperty("currency")]
        public string Currency { get; set; } = string.Empty;

        [JsonProperty("rewardAmount")]
        public double RewardAmount { get; set; }

        [JsonProperty("splitCurrency")]
        public bool SplitCurrency { get; set; }

        // Set when reward is partially given in another currency due to reaching max dollar cap
        [JsonProperty("overflowCurrency")]
        public string? OverflowCurrency { get; set; }

        [JsonProperty("overflowAmount")]
        public double? OverflowAmount { get; set; }

        // Only populated if SplitCurrency is true
        [JsonProperty("originalCurrency")]
        public string? OriginalCurrency { get; set; }

        [JsonProperty("originalRewardAmount")]
        public double? OriginalRewardAmount { get; set; }

        [JsonProperty("conversionRatio")]
        public double? ConversionRatio { get; set; }
    }

}
