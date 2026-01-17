using System.ComponentModel.DataAnnotations;

namespace SunnyRewards.Helios.Task.Core.Domain.Dtos
{
    public class TaskRewardRequestDto
    {
        [Required]
        public long TaskId { get; set; }
        [Required]
        public long RewardTypeId { get; set; }
        [Required]
        public string? TenantCode { get; set; }
        [Required]
        public string? TaskRewardCode { get; set; }
        public string? TaskActionUrl { get; set; } // Nullable in DB
        [Required]
        public string? Reward { get; set; }
        [Required]
        public int Priority { get; set; }
        public DateTime? Expiry { get; set; } // Nullable in DB
        public int MinTaskDuration { get; set; } // Nullable in DB
        public int MaxTaskDuration { get; set; } // Nullable in DB
        [Required]
        public string? TaskExternalCode { get; set; }
        public DateTime? ValidStartTs { get; set; } // Nullable in DB
        [Required]
        public bool IsRecurring { get; set; }
        public string? RecurrenceDefinitionJson { get; set; } // Nullable in DB
        [Required]
        public bool SelfReport { get; set; }
        public string? TaskCompletionCriteriaJson { get; set; } // Nullable in DB
        [Required]
        public bool ConfirmReport { get; set; }
        public string? UpdateUser { get; set; } = string.Empty;
    }
}
