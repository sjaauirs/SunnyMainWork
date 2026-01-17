using Newtonsoft.Json;
using SunnyRewards.Helios.Tenant.Core.Domain.Enum;

namespace SunnyRewards.Helios.Tenant.Core.Domain.Dtos.Json
{
    public class TenantAttributeDto
    {
        [JsonProperty("pickAPurseOnboardingEnabled")]
        public bool PickAPurseOnboardingEnabled { get; set; }

        [JsonProperty("autosweepSweepstakesReward")]
        public bool AutosweepSweepstakesReward { get; set; }
        [JsonProperty("consumerWallet")]

        public ConsumerWallet? ConsumerWallet { get; set; }

        [JsonProperty("membershipWallet")]

        public MembershipWallet? MembershipWallet { get; set; }
        [JsonProperty("sweepstakesEntriesRule")]
        public SweepstakesEntriesRule? EntriesRule { get; set; }
    }

    public class ConsumerWallet
    {
        [JsonProperty("ownerMaximum")]
        public double OwnerMaximum { get; set; }

        [JsonProperty("walletMaximum")]
        public double WalletMaximum { get; set; }

        [JsonProperty("contributorMaximum")]
        public double ContributorMaximum { get; set; }

        [JsonProperty("individualWallet")]
        public bool IndividualWallet { get; set; } = false;

    }

    public class MembershipWallet
    {
        [JsonProperty("earnMaximum")]
        public double EarnMaximum { get; set; }
    }
    public class SweepstakesEntriesRule
    {
        [JsonProperty("entryCap")]
        public int EntryCap { get; set; } = 0;
        [JsonProperty("rolloverEnabled")]
        public bool RolloverEnabled { get; set; } = false;
        [JsonProperty("resetFrequency")]
        public string ResetFrequency { get; set; } = nameof(ResetFrequencyType.Monthly);
    }
}
