using Microsoft.AspNetCore.Http;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using System.ComponentModel;
using System.Text.Json.Serialization;

namespace SunnyRewards.Helios.Task.Core.Domain.Dtos
{
    public class ConsumerTaskDto
    {
        public long ConsumerTaskId { get; set; }
        public long TaskId { get; set; }
        public string? TaskStatus { get; set; }
        public int Progress { get; set; }
        public string? Notes { get; set; } = string.Empty;
        public string? ConsumerCode { get; set; }
        public string? TenantCode { get; set; }
        public DateTime TaskStartTs { get; set; }
        public DateTime? TaskCompleteTs { get; set; }
        public bool AutoEnrolled { get; set; }
        public string? ProgressDetail { get; set; }
        public long? ParentConsumerTaskId { get; set; } // only available if this task is a subtask
        public DateTime CreateTs { get; set; }
        [JsonIgnore]
        public string? CreateUser { get; set; } = string.Empty;
        public string? WalletTransactionCode { get; set; } = string.Empty;
        public string? RewardInfoJson { get; set; }

        [DefaultValue(false)]
        public bool SkipValidation { get; set; } = false;
    }

    public class UpdateConsumerTaskDto : ConsumerTaskDto
    {
        public string? TaskCode { get; set; } = string.Empty;
        public string? TaskExternalCode { get; set; } = string.Empty;
        public bool SpinWheelTaskEnabled { get; set; } = false;
        public IFormFile? TaskCompletionEvidenceDocument { get; set; }
    }
}
