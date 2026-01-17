using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunnyRewards.Helios.ETL.Core.Domain.Dtos
{
    public class ETLMemberTaskUpdateDetailDto
    {

        [JsonProperty(Required = Required.Always)]
        public string MemberId { get; set; } = string.Empty;

        /// <summary>
        /// TaskId registered with the Rewards System - either TaskId or TaskName must be supplied
        /// </summary>
        public long TaskId { get; set; }

        /// <summary>
        /// TaskName registered with the Rewards System - either TaskId or TaskName must be supplied
        /// </summary>
        public string TaskName { get; set; } = string.Empty;

        /// <summary>
        /// True if completed - either Completion or Progress must be supplied
        /// </summary>
        public bool Completion { get; set; }

        /// <summary>
        /// Value ranging from 0 to 100 - either Completion or Progress must be supplied
        /// </summary>
        public int Progress { get; set; }

        public bool? SupportLiveTransferToRewardsPurse { get; set; } = false;

        [DefaultValue(false)]
        public bool IsAutoEnrollEnabled { get; set; } = false;

        public DateTime? TaskCompletedTs { get; set; }

        public string? PartnerCode { get; set; }

        public bool SkipValidation { get; set; }

        public bool ValidTask()
        {
            return (TaskId >= 1 && !string.IsNullOrEmpty(MemberId));
        }
    }
}
