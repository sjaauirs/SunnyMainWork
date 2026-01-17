using Newtonsoft.Json;

namespace SunnyRewards.Helios.ETL.Core.Domain.Dtos.FIS
{
    public class FISPeriodConfigDto
    {
        [JsonProperty("fundDate")]
        public int FundDate { get; set; }

        [JsonProperty("interval")]
        public string? Interval { get; set; }
    }
}
