using System.ComponentModel.DataAnnotations;

namespace SunnyRewards.Helios.Task.Core.Domain.Dtos
{
    public class GetTaskByTaskNameRequestDto
    {
        [Required]
        public string TaskName { get; set; } = null!;
    }
}
