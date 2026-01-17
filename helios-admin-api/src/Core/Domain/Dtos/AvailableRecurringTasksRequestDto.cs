using System.ComponentModel.DataAnnotations;

namespace SunnyRewards.Helios.Admin.Core.Domain.Dtos
{
    public class AvailableRecurringTasksRequestDto
    {
        [Required]
        public string TenantCode { get; set; } = null!;

        [Required]
        public string ConsumerCode { get; set; } = null!;

        public DateTime? TaskAvailabilityTs { get; set; }

        public string? LanguageCode { get; set; }
    }
}
