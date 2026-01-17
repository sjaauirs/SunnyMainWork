using Newtonsoft.Json;

namespace SunnyRewards.Helios.ETL.Core.Domain.Dtos.FIS
{
    public class FISTenantConfigDto
    {
        [JsonProperty("purseConfig")]
        public FISPurseConfigDto? PurseConfig { get; set; }

        [JsonProperty("fisProgramDetail")]
        public FISProgramDetailDto? FISProgramDetail { get; set; }
    }
}
