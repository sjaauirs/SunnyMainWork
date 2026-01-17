using Newtonsoft.Json;
using SunnyRewards.Helios.Tenant.Core.Domain.Dtos;

namespace SunnyRewards.Helios.User.Core.Domain.Dtos.Json
{
    public class SubscriptionStatusDto
    {
        [JsonProperty("subscriptionStatus")]
        public List<ConsumerSubscriptionStatusDetailDto> subscriptionStatus { get; set; }
    }
}
