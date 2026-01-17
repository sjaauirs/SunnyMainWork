using Newtonsoft.Json;

namespace SunnyRewards.Helios.ETL.Core.Domain.Models
{
    public class CompletionEligibilityJson
    {
        [JsonProperty("earlyCompletionDays")]
        public int? EarlyCompletionDays { get; set; }
        [JsonProperty("lateCompletionDays")]
        public int LateCompletionDays { get; set; }
    }

}