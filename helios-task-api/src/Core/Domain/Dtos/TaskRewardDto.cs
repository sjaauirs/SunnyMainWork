using Newtonsoft.Json;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Task.Core.Domain.Dtos
{
    public class TaskRewardDto
    {
        public long TaskId { get; set; }
        public long TaskRewardId { get; set; }
        public long RewardTypeId { get; set; }
        public string? TenantCode { get; set; }
        public string? TaskRewardCode { get; set; }
        public string? TaskActionUrl { get; set; }
        public string? Reward { get; set; }
        public int Priority { get; set; }
        public DateTime? Expiry { get; set; }
        public int MinTaskDuration { get; set; }
        public int MaxTaskDuration { get; set; }
        public string? TaskExternalCode { get; set; }
        public DateTime? ValidStartTs { get; set; }
        public bool IsRecurring { get; set; }
        public string? RecurrenceDefinitionJson { get; set; }
        public bool SelfReport { get; set; }
        public string? TaskCompletionCriteriaJson { get; set; }
        public string? CreateUser { get; set; }
        public DateTime? MinAllowedTaskCompletionTs { get; set; }
        public DateTime? MaxAllowedTaskCompletionTs { get; set; }
        public RewardDto RewardDetails { get; set; } = new RewardDto();

        public string? TaskRewardConfigJson { get; set; }
        public bool IsCollection { get; set; }


        public static RewardDto GetRewardDetails( string reward)
        {
            if (string.IsNullOrEmpty(reward))
                return new RewardDto();

            try
            {
                return JsonConvert.DeserializeObject<RewardDto>(reward) ?? new RewardDto();
            }
            catch (JsonException)
            {
                // Handle deserialization error
                return new RewardDto(); 
            }
        }
        public class ExportTaskRewardDto
        {
            public string? TaskRewardTypeCode { get; set; }
            public TaskRewardDto? TaskReward { get; set; }
        }
    }
}
