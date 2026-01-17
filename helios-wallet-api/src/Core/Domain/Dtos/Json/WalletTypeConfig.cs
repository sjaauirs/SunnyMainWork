using Newtonsoft.Json;

namespace SunnyRewards.Helios.Wallet.Core.Domain.Dtos.Json
{
    public class WalletTypeConfig
    {
        [JsonProperty("currency")]
        public string? Currency { get; set; }
    }
}
