using Newtonsoft.Json;

namespace SunnyRewards.Helios.ETL.Core.Domain.Dtos.FIS
{
    public class ConsumerAccountPurseConfigDto
    {
        [JsonProperty("purses")]
        public List<ConsumerAccountPurseDto> Purses { get; set; } = new List<ConsumerAccountPurseDto>();
    }
}
