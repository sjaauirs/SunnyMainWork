using System.ComponentModel.DataAnnotations;

namespace SunnyRewards.Helios.Task.Core.Domain.Dtos
{
    public class DeleteConsumerTaskRequestDto
    {
        [Required]
        public string TenantCode { get; set; } = null!;
        [Required]
        public string ConsumerCode { get; set; } = null!;
        [Required]
        public string TaskExternalCode { get; set; } = null!;
    }
}
