using Newtonsoft.Json;
using SunnyRewards.Helios.ETL.Common.Domain.Models;

namespace SunnyRewards.Helios.ETL.Core.Domain.Models
{
    public class ETLTaskRewardModel : BaseModel
    {
        public virtual long TaskId { get; set; }
        public virtual long TaskRewardId { get; set; }
        public virtual long RewardTypeId { get; set; }
        public virtual string? TaskActionUrl { get; set; }
        public virtual string? TenantCode { get; set; }
        public virtual string? TaskRewardCode { get; set; }
        public virtual string? Reward { get; set; }
        public virtual int Priority { get; set; }
        public virtual DateTime Expiry { get; set; }
        public virtual int MinTaskDuration { get; set; }
        public virtual int MaxTaskDuration { get; set; }
        public virtual string? TaskExternalCode { get; set; }
        public virtual DateTime? ValidStartTs { get; set; }
        public virtual bool IsRecurring { get; set; }
        public virtual string? RecurrenceDefinitionJson { get; set; }
        public virtual bool SelfReport { get; set; }
        public virtual string? TaskCompletionCriteriaJson { get; set; }
        public virtual bool ConfirmReport { get; set; }
        public virtual string? CompletionEligibilityJson { get; set; }

        /// <summary>
        /// Deserialize the TaskCompletionCriteriaJson to TaskCompletionCriteria object
        /// </summary>
        [JsonIgnore]
        public virtual TaskCompletionCriteria? TaskCompletionCriteria
        {
            get => !string.IsNullOrEmpty(TaskCompletionCriteriaJson)
                ? JsonConvert.DeserializeObject<TaskCompletionCriteria>(TaskCompletionCriteriaJson)
                : null;
        }
    }

    public class HealthCriteria
    {
        public virtual string? HealthTaskType { get; set; }
        public virtual int RequiredSteps { get; set; }
        public virtual RequiredSleep? RequiredSleep { get; set; }
    }

    public class TaskCompletionCriteria
    {
        public virtual HealthCriteria? HealthCriteria { get; set; }
        public virtual string? CompletionPeriodType { get; set; }
        public virtual string? CompletionCriteriaType { get; set; }
    }

    public class RequiredSleep
    {
        public virtual int MinSleepDuration { get; set; }
        public virtual int NumDaysAtOrAboveMinDuration { get; set; }
    }

}