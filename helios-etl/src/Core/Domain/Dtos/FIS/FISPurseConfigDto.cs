using Newtonsoft.Json;

namespace SunnyRewards.Helios.ETL.Core.Domain.Dtos.FIS
{
    public class FISPurseConfigDto
    {
        [JsonProperty("purses")]
        public List<FISPurseDto> Purses { get; set; } = new List<FISPurseDto>();
    }
}
