using Newtonsoft.Json;

namespace SunnyRewards.Helios.Admin.Core.Domain.Dtos
{
    public class Reward
    {
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("rewardAmount")]
        public double RewardAmount { get; set; }
    }
}