using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunnyRewards.Helios.Tenant.Core.Domain.Dtos.Json
{
    public class TenantOption
    {
        public List<string> Apps { get; set; } = new List<string>();
        public List<string>? SkipTransactionTypes { get; set; } = new List<string>();
        public BenefitsOptions BenefitsOptions { get; set; } = new BenefitsOptions();
        public EtlAutomationConfig EtlAutomationConfig { get; set; } = new EtlAutomationConfig();
        public List<ConsumerSubscriptionStatusDetailDto>? SubscriptionStatus { get; set; } = new List<ConsumerSubscriptionStatusDetailDto>();
        public string? SweepstakesWinnerRewardWalletTypeCode { get; set; } = string.Empty;
    }

    public class BenefitsOptions
    {
        public List<CardIssueFlowType> CardIssueFlowType { get; set; } = new List<CardIssueFlowType>();
        public List<string> TaskCompletionCheckCode { get; set; } = new List<string>();
        public bool ManualCardRequestRequired { get; set; }
        public bool DisableOnboardingFlow { get; set; }
        public bool ReactivateDeletedConsumer { get; set; }
        public bool IncludeDiscretionaryCardData {  get; set; }
        public bool ShouldFreezeCardOnTermination { get; set; }
        public int ValidCardActiveDays { get; set; }
    }

    public class EtlAutomationConfig
    {
        public bool IsTenantConfigSyncEnabled { get; set; }
        public bool IsCard30CreationEnabled { get; set; }
        public bool IsCard30ResponseProcessEnabled { get; set; }
        public bool IsFundingRulesEnabled { get; set; }
        public bool IsCard60CreationEnabled { get; set; }
        public bool IsCard60ResponseProcessEnabled { get; set; }
    }

    public class CardIssueFlowType
    {
        public string FlowType { get; set; } = string.Empty;
        public List<string> CohortCode { get; set; } = new List<string>();
    }

}
