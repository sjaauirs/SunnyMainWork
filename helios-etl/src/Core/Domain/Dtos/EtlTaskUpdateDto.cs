using Newtonsoft.Json;

namespace SunnyRewards.Helios.ETL.Core.Domain.Dtos
{
    public class EtlTaskUpdateDto
    {
        [JsonProperty("partnerCode")]
        public string PartnerCode { get; set; } = string.Empty;

        [JsonProperty("taskCode")]
        public string TaskCode { get; set; } = string.Empty;

        [JsonProperty("memId")]
        public string MemberId { get; set; } = string.Empty;

        [JsonProperty("taskStatus")]
        public string TaskStatus { get; set; } = string.Empty;

        [JsonProperty("environment")]
        public string Environment { get; set; } = string.Empty;

        [JsonProperty("taskName")]
        public string TaskName { get; set; } = string.Empty;

        [JsonProperty("supportLiveTransferToRewardsPurse")]
        public bool? SupportLiveTransferToRewardsPurse { get; set; } = false;

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool ValidTask()
        {
            return (!string.IsNullOrEmpty(PartnerCode) && !string.IsNullOrEmpty(TaskCode)  && !string.IsNullOrEmpty(MemberId));
        }
    }
}
