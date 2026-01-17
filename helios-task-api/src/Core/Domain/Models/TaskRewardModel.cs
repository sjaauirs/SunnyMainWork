using Newtonsoft.Json;
using SunnyRewards.Helios.Common.Core.Domain.Models;
using SunnyRewards.Helios.Task.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Task.Core.Domain.Models
{
    public class TaskRewardModel : BaseModel
    {
        public virtual long TaskId { get; set; }
        public virtual long TaskRewardId { get; set; }
        public virtual long RewardTypeId { get; set; }
        public virtual string? TaskActionUrl { get; set; }
        public virtual string? TenantCode { get; set; }
        public virtual string? TaskRewardCode { get; set; }
        public virtual string? Reward { get; set; }
        public virtual int Priority { get; set; }
        public virtual DateTime? Expiry { get; set; }
        public virtual int MinTaskDuration { get; set; }
        public virtual int MaxTaskDuration { get; set; }
        public virtual string? TaskExternalCode { get; set; }
        public virtual DateTime? ValidStartTs { get; set; }
        public virtual bool IsRecurring { get; set; }
        public virtual string? RecurrenceDefinitionJson { get; set; }
        public virtual bool SelfReport { get; set; }
        public virtual string? TaskCompletionCriteriaJson { get; set; }
        public virtual bool ConfirmReport { get; set; }
        public virtual string? TaskRewardConfigJson { get; set; }
        public virtual bool IsCollection { get; set; }
        
        /// <summary>
        /// Deserialize the TaskCompletionCriteriaJson to TaskCompletionCriteria object
        /// </summary>
        [JsonIgnore]
        public virtual TaskCompletionCriteriaJson? TaskCompletionCriteria
        {
            get => !string.IsNullOrEmpty(TaskCompletionCriteriaJson)
                ? JsonConvert.DeserializeObject<TaskCompletionCriteriaJson>(TaskCompletionCriteriaJson)
                : null;
        }
    }
}
