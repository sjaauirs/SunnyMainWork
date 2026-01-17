using Newtonsoft.Json;

namespace Sunny.Benefits.Bff.Core.Domain.Dtos
{
    public class TenantAttribute
    {
        [JsonProperty("supportLiveTransferToRewardsPurse")]

        public bool SupportLiveTransferToRewardsPurse { get; set; }

        [JsonProperty("costcoMemberShipSupport")]
        public bool CostcoMemberShipSupport { get; set; }
    }

    
}
