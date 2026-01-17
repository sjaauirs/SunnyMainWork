using System.ComponentModel;

namespace SunnyRewards.Helios.ETL.Core.Domain.Dtos
{
    public class TaskUpdateRequestDto
    {
        public string? ConsumerCode { get; set; }
        public long TaskId { get; set; }
        public string? TaskStatus { get; set; }
        public DateTime? TaskCompletedTs { get; set; }

        [DefaultValue(false)]
        public bool IsAutoEnrollEnabled { get; set; } = false;
    }
}
