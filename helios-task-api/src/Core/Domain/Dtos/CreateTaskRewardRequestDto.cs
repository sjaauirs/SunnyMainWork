using System.ComponentModel.DataAnnotations;

namespace SunnyRewards.Helios.Task.Core.Domain.Dtos
{
    public class CreateTaskRewardRequestDto
    {
        [Required]
        public string TaskCode { get; set; } = null!;
        [Required]
        public TaskRewardDto TaskReward { get; set; } = null!;


    }
}
