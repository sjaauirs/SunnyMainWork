using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunnyRewards.Helios.Task.Core.Domain.Dtos
{
    public class RewardDto
    {
        [JsonProperty("rewardAmount")]
        public double RewardAmount { get; set; }

        [JsonProperty("rewardType")]
        public string? RewardType { get; set; }=string.Empty;

        [JsonProperty("membershipType")]
        public string? MembershipType { get; set; } = string.Empty;
    }
}
