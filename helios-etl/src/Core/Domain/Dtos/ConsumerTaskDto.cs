using System.Text.Json.Serialization;

namespace SunnyRewards.Helios.ETL.Core.Domain.Dtos
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
        public long? ParentConsumerTaskId { get; set; }
        public DateTime CreateTs { get; set; }
        [JsonIgnore]
        public string? CreateUser { get; set; } = string.Empty;
        public string? WalletTransactionCode { get; set; } = string.Empty;
        public string? RewardInfoJson { get; set; }

    }
}
