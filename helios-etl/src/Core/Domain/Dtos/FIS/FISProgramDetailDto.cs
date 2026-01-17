using Newtonsoft.Json;

namespace SunnyRewards.Helios.ETL.Core.Domain.Dtos.FIS
{
    public class FISProgramDetailDto
    {
        [JsonProperty("clientId")]
        public string? ClientId { get; set; }

        [JsonProperty("companyId")]
        public string? CompanyId { get; set; }

        [JsonProperty("packageId")]
        public string? PackageId { get; set; }

        [JsonProperty("subprogramId")]
        public string? SubprogramId { get; set; }
    }
}
