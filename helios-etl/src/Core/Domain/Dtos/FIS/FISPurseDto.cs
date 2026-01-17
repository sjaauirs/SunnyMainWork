using Newtonsoft.Json;

namespace SunnyRewards.Helios.ETL.Core.Domain.Dtos.FIS
{
    public class FISPurseDto
    {
        [JsonProperty("purseLabel")]
        public string? PurseLabel { get; set; }

        [JsonProperty("walletType")]
        public string? WalletType { get; set; }

        [JsonProperty("purseNumber")]
        public int PurseNumber { get; set; }

        [JsonProperty("purseWalletType")]
        public string? PurseWalletType { get; set; }

        [JsonProperty("masterWalletType")]
        public string? MasterWalletType { get; set; }

        [JsonProperty("masterRedemptionWalletType")]
        public string? MasterRedemptionWalletType { get; set; }
        [JsonProperty("periodConfig")]
        public PeriodConfigDto? PeriodConfig { get; set; }
    }

    public class PeriodConfigDto
    {
        [JsonProperty("fundDate")]
        public int FundDate { get; set; }

        [JsonProperty("interval")]
        public string? Interval { get; set; }

        [JsonProperty("applyDateConfig")]
        public ApplyDateConfig? ApplyDateConfig { get; set; }
    }

    public class ApplyDateConfig
    {
        [JsonProperty("applyDate")]
        public int ApplyDate { get; set; }

        [JsonProperty("applyDateType")]
        public string? ApplyDateType { get; set; }
    }
}
