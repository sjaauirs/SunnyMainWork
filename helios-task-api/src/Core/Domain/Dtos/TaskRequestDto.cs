using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using System.ComponentModel.DataAnnotations;

namespace SunnyRewards.Helios.Task.Core.Domain.Dtos
{
    public class TaskRequestDto
    {
        [Required]
        public long TaskTypeId { get; set; }
        [Required]
        public string? TaskCode { get; set; }
        [Required]
        public string? TaskName { get; set; }
        [Required]
        public bool SelfReport { get; set; }
        [Required]
        public bool ConfirmReport { get; set; }
        public long? TaskCategoryId { get; set; }
        [Required]
        public bool IsSubtask { get; set; }
        public string? UpdateUser { get; set; } = string.Empty;
    }
}
