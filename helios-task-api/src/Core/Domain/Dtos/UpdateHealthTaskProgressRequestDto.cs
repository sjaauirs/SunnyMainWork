using System.ComponentModel.DataAnnotations;

namespace SunnyRewards.Helios.Task.Core.Domain.Dtos
{
    public class UpdateHealthTaskProgressRequestDto
    {
        [Required]
        public string ConsumerCode { get; set; }
        [Required]
        public string TenantCode { get; set; }
        [Required]
        public long TaskId { get; set; }
        [Required]
        public string HealthTaskType { get; set; } 
        public int? Steps { get; set; }
        public int? NumberOfDays { get; set; }
        public int? NumberOfUnits { get; set; }
        public List<HealthTrackingDto>? HealthReport { get; set; }
        public DateTime? TaskCompletionTs { get; set; }
        public DateTime? DateTimeAddedFor { get; set; }

    }
}

