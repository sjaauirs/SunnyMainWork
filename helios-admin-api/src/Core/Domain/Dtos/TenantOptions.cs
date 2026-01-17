using System.ComponentModel.DataAnnotations;

namespace SunnyRewards.Helios.Admin.Core.Domain.Dtos
{
    public class TenantOptions
    {
        public List<string> Apps { get; set; } = new List<string>();
        public BenefitsOptions? BenefitsOptions { get; set; } 
    }

    public class BenefitsOptions
    {
        public List<CardIssueFlowType> CardIssueFlowType { get; set; } = new List<CardIssueFlowType>();
        public bool DisableOnboardingFlow { get; set; }
        public List<string> TaskCompletionCheckCode { get; set; } = new List<string>();
        public bool ManualCardRequestRequired { get; set; }
    }

    public class CardIssueFlowType
    {
        public string FlowType { get; set; } = string.Empty;
        public List<string> CohortCode { get; set; } = new List<string>();
    }
}
