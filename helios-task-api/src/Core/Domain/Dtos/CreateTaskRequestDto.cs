using System.ComponentModel.DataAnnotations;

namespace SunnyRewards.Helios.Task.Core.Domain.Dtos
{
    public class CreateTaskRequestDto
    {
        public long TaskId { get; set; }
        [Required]
        public long TaskTypeId { get; set; }
        [Required]
        public string? TaskCode { get; set; }
        [Required]
        public string? TaskName { get; set; }
        public bool SelfReport { get; set; }
        public bool ConfirmReport { get; set; }
        public long? TaskCategoryId { get; set; }
        public bool IsSubtask { get; set; }
        [Required]
        public string? CreateUser { get; set; }
    }
}
