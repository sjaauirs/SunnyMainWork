using System.Text.Json.Serialization;

namespace SunnyRewards.Helios.Admin.Core.Domain.Dtos
{
    public class CohortRuleJson
    {
        [JsonPropertyName("ruleExpr")]
        public string RuleExpr { get; set; } = string.Empty;

        [JsonPropertyName("successExpr")]
        public required string SuccessExpr { get; set; }
    }


    public class CohortRuleArrayJson
    {
        [JsonPropertyName("ruleExpr")]
        public List<string> RuleExpr { get; set; } = new();

        [JsonPropertyName("successExpr")]
        public required string SuccessExpr { get; set; }
    }
}
