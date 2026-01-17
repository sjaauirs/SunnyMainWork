using System.ComponentModel.DataAnnotations;

namespace Sunny.Benefits.Bff.Core.Domain.Dtos
{
    public class DeactivateConsumerRequestDto
    {
        [Required]
        public string ConsumerCode { get; set; } = string.Empty;
        [Required]
        public string TenantCode { get; set; } = string.Empty;
    }
}
