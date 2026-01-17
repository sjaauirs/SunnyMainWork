using SunnyRewards.Helios.Common.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Task.Core.Domain.Dtos
{
    public class GetConsumerTaskResponseDto
    {
        public long ConsumerTaskId { get; set; }
        public long TaskId { get; set; }
        public string? TaskStatus { get; set; }
        public int Progress { get; set; }
        public string? Notes { get; set; } = string.Empty;
        public string? ConsumerCode { get; set; }
        public string? TenantCode { get; set; }
        public DateTime TaskStartTs { get; set; }
        public DateTime TaskCompleteTs { get; set; }
        public bool AutoEnrolled { get; set; }
        public string? ProgressDetail { get; set; }
        public long? ParentConsumerTaskId { get; set; } // only available if this task is a subtask
    }
}
