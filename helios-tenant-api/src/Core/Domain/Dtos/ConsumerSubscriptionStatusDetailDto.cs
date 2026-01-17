using Newtonsoft.Json;

namespace SunnyRewards.Helios.Tenant.Core.Domain.Dtos
{
    public class ConsumerSubscriptionStatusDetailDto
    {
        [JsonProperty("feature")]
        public string Feature { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }
    }
}
