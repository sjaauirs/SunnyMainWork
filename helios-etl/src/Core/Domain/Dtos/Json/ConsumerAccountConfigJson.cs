using Newtonsoft.Json;
using SunnyRewards.Helios.ETL.Core.Domain.Dtos.FIS;

namespace SunnyRewards.Helios.ETL.Core.Domain.Dtos.Json
{
    public class ConsumerAccountConfigJson
    {
        [JsonProperty("purseConfig")]
        public ConsumerAccountPurseConfigDto? ConsumerAccountPurseConfigDto { get; set; }
    }
}
