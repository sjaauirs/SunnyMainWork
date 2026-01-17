using Newtonsoft.Json;

namespace SunnyRewards.Helios.Etl.Core.Domain.Dtos
{
    public class PldRecordDto
    {
        [JsonProperty("pld")]
        public Dictionary<string, string> PldFieldData { get; set; } =
            new Dictionary<string, string>();
    }
}
