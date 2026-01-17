using System.ComponentModel.DataAnnotations;

namespace SunnyRewards.Helios.Task.Core.Domain.Dtos
{
    public class TaskRewardTypeRequestDto
    {
        [Required]
        public string? RewardTypeName { get; set; }
        [Required]
        public string? RewardTypeDescription { get; set; }
        [Required]
        public string? RewardTypeCode { get; set; }
    }
}
