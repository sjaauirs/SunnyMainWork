using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace SunnyRewards.Helios.Task.Core.Domain.Dtos
{
    public class TaskRewardCollectionRequestDto
    {
        [Required]
        public string TenantCode { get; set; } = null!;
        [Required]
        public string TaskRewardCode { get; set; } = null!;

        [Required]
        public string ConsumerCode { get; set; } = null!;

        [DefaultValue(false)]
        public bool IsIncludeCompletedTask { get; set; }

        public string? LanguageCode { get; set; }
    }
}
