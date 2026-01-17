using System.ComponentModel.DataAnnotations;

namespace SunnyRewards.Helios.Task.Core.Domain.Dtos
{
    public class CreateTaskDetailsRequestDto
    {
        [Required]
        public string TaskCode { get; set; } = null!;
        [Required]
        public PostTaskDetailsDto TaskDetail { get; set; } = null!;
    }
}
