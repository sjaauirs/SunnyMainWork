using System.ComponentModel.DataAnnotations;

namespace SunnyRewards.Helios.Task.Core.Domain.Dtos
{
    public class TaskDetailRequestDto
    {
        [Required]
        public long TaskId { get; set; }
        [Required]
        public long TermsOfServiceId { get; set; }
        [Required]
        public string? TaskHeader { get; set; }
        [Required]
        public string? TaskDescription { get; set; }
        [Required]
        public string? LanguageCode { get; set; }
        [Required]
        public string? TenantCode { get; set; }
        [Required]
        public string? TaskCtaButtonText { get; set; }
        public string? UpdateUser { get; set; } = string.Empty;


    }
}
