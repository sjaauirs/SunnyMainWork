using Newtonsoft.Json;

namespace SunnyRewards.Helios.Admin.Core.Domain.Dtos
{
    public class TaskRewardWalletSplitConfigDto
    {
        [JsonProperty("wallet_split_config")]
        public List<WalletSplitConfig> WalletSplitConfig { get; set; } = new();
    }
    public class WalletSplitConfig
    {
        [JsonProperty("wallet_type_code")]
        public string? WalletTypeCode { get; set; }

        [JsonProperty("redemption_wallet_type_code")]
        public string? RedemptionWalletTypeCode { get; set; }

        [JsonProperty("purse_wallet_type_code")]
        public string? PurseWalletTypeCode { get; set; }

        [JsonProperty("percentage")]
        public double Percentage { get; set; }

        public string? MasterWalletTypeCode { get; set; }
        public string? RedemptionVendorCode { get; set; }
        public string? WalletName { get; set; }
        public string? TaskRewardCurrency { get; set; }
    }
}
