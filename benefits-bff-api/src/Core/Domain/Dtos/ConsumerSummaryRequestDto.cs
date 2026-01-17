using System.ComponentModel.DataAnnotations;

namespace Sunny.Benefits.Bff.Core.Domain.Dtos
{
    public class ConsumerSummaryRequestDto : BaseRequestDto
    {
        [Required]
        public string AuthUserId { get; set; } = null!;
        public string? LanguageCode { get; set; }
    }
}
