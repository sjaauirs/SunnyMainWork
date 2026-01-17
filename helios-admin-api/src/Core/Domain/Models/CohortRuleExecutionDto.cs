namespace SunnyRewards.Helios.Admin.Core.Domain.Models
{
    public class CohortRuleExecutionDto
    {
        public bool RulesExecutionResult { get; set; }
        public string CohortName { get; set; } = string.Empty;
    }
}
