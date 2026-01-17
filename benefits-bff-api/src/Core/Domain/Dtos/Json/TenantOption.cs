using Newtonsoft.Json;

namespace Sunny.Benefits.Bff.Core.Domain.Dtos.Json
{
    public class TenantOption
    {
        [JsonProperty("apps")]
        public List<string>? Apps { get; set; }

        [JsonProperty("benefitsOptions")]
        public BenefitsOptions? BenefitsOptions { get; set; }
    }
    public class BenefitsOptions
    {
        [JsonProperty("disableOnboardingFlow")]
        public bool DisableOnboardingFlow { get; set; }

        [JsonProperty("manualCardRequestRequired")]
        public bool ManualCardRequestRequired { get; set; }

        [JsonProperty("cardIssueFlowType")]
        public List<CardIssueFlowType>? CardIssueFlowType { get; set; }

        [JsonProperty("taskCompletionCheckCode")]
        public List<string>? TaskCompletionCheckCode { get; set; }

        [JsonProperty("autoCompleteTaskOnLogin")]
        public bool AutoCompleteTaskOnLogin { get; set; }
    }
    public class CardIssueFlowType
    {
        public string FlowType { get; set; } = string.Empty;
        public List<string> CohortCode { get; set; } = new List<string>();
    }
}
