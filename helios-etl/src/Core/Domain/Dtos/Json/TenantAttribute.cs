using Newtonsoft.Json;

namespace SunnyRewards.Helios.ETL.Core.Domain.Dtos.Json
{
    public class TenantAttribute
    {
        [JsonProperty("supportLiveTransferWhileProcessingNonMonetary")]
        public bool SupportLiveTransferWhileProcessingNonMonetary { get; set; }

        [JsonProperty("supportLiveTransferToRewardsPurse")]
        public bool SupportLiveTransferToRewardsPurse { get; set; }

        [JsonProperty("consumerWallet")]
        public ConsumerWallet? ConsumerWallet { get; set; }

        [JsonProperty("membershipWallet")]
        public MembershipWallet? MembershipWallet { get; set; }

         [JsonProperty("justInTimeFunding")]
        public bool JustInTimeFunding { get; set; }

        [JsonProperty("jitfTimeOffset")]
        public int JITFTimeOffset { get; set; }

        [JsonProperty("autosweepSweepstakesReward")]
        public bool AutosweepSweepstakesReward { get; set; }

        [JsonProperty("enableSweepstakesDirectDeposit")]
        public bool EnableSweepstakesDirectDeposit { get; set; }
    }

    public class ConsumerWallet
    {
        [JsonProperty("ownerMaximum")]
        public double OwnerMaximum { get; set; }

        [JsonProperty("walletMaximum")]
        public double WalletMaximum { get; set; }

        [JsonProperty("contributorMaximum")]
        public double ContributorMaximum { get; set; }

    }

    public class MembershipWallet
    {
        [JsonProperty("earnMaximum")]
        public double EarnMaximum { get; set; }
    }
}
