using Newtonsoft.Json;

namespace SunnyRewards.Helios.ETL.Core.Domain.Dtos
{
    public class CohortRule
    {
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("ruleExpr")]
        public string RuleExpr { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("successExpr")]
        public string SuccessExpr { get; set; }
    }
}
