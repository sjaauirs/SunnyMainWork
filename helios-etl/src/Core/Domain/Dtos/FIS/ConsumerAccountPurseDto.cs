using Newtonsoft.Json;

namespace SunnyRewards.Helios.ETL.Core.Domain.Dtos.FIS
{
    public class ConsumerAccountPurseDto
    {
        [JsonProperty("purseLabel")]
        public string? PurseLabel { get; set; }

        [JsonProperty("enabled")]
        public bool Enabled { get; set; }

    }
}
