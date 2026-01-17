using System.ComponentModel.DataAnnotations;

namespace SunnyRewards.Helios.Task.Core.Domain.Dtos
{
    public class PostTaskDetailsDto
    {
        public long TaskId { get; set; }
        public long TaskDetailId { get; set; }
        public long TermsOfServiceId { get; set; }
        public string? TaskHeader { get; set; }
        public string? TaskDescription { get; set; }
        public string? LanguageCode { get; set; }
        [Required]
        public string? TenantCode { get; set; }
        public string? TaskCtaButtonText { get; set; }
        public string? CreateUser { get; set; }
    }
}
