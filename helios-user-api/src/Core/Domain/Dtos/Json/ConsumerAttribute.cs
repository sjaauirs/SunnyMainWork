using Newtonsoft.Json;

namespace SunnyRewards.Helios.User.Core.Domain.Dtos.Json
{
    public class ConsumerAttribute
    {
        [JsonProperty("source_mem_nbr")]
        public string? SourceMemberNbr { get; set; }

        [JsonProperty("source_subscriber_mem_nbr")]
        public string? SourceSubscriberMemberNbr { get; set; }
        [JsonProperty("is_ssbci")]
        public bool IsSSBCI {  get; set; }
    }
}