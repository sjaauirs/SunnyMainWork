using Newtonsoft.Json;

namespace SunnyRewards.Helios.ETL.Core.Domain.Dtos.Json
{
    public class SyncInfoJson
    {
        [JsonProperty("syncOptions")]
        public List<string> SyncOptions { get; set; }
    }
}
