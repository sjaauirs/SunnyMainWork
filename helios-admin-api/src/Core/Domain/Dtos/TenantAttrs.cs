using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunnyRewards.Helios.Admin.Core.Domain.Dtos
{
    public class TenantAttrs
    {
        [JsonProperty("spinwheelTaskEnabled")]
        public bool SpinWheelTaskEnabled { get; set; }

        [JsonProperty("disableMembershipDollars")]
        public bool DisableMembershipDollars { get; set; }

        [JsonProperty("costcoMembershipSupport")]
        public bool CostcoMembershipSupport { get; set; }

        [JsonProperty("consumerWallet")]

        public ConsumerWallet ConsumerWallet { get; set; }

        [JsonProperty("supportLiveTransferToRewardsPurse")]
        public bool SupportLiveTransferToRewardsPurse { get; set; }

        [JsonProperty("membershipWallet")]
        public MembershipWallet? MembershipWallet { get; set; }


    }

    public class ConsumerWallet
    {
        [JsonProperty("ownerMaximum")]
        public double OwnerMaximum { get; set; }

        [JsonProperty("walletMaximum")]
        public double WalletMaximum { get; set; }

        [JsonProperty("contributorMaximum")]
        public double ContributorMaximum { get; set; }

        [JsonProperty("splitRewardOverflow")]
        public bool? SplitRewardOverflow { get; set; }


        [JsonProperty("individualWallet")]
        public bool IndividualWallet { get; set; }

    }

    public class MembershipWallet
    {
        [JsonProperty("earnMaximum")]
        public double EarnMaximum { get; set; }
    }
}
