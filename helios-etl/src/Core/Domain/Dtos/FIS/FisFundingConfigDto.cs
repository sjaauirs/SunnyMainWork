using Newtonsoft.Json;

namespace SunnyRewards.Helios.ETL.Core.Domain.Dtos.FIS
{
    public class FISFundingConfigDto
    {
        [JsonProperty("fundingRules")]
        public List<FISFundingRuleDto>? FundingRules { get; set; }
    }
}
