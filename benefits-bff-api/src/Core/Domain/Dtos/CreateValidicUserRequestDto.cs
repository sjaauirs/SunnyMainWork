using System.ComponentModel.DataAnnotations;

namespace Sunny.Benefits.Bff.Core.Domain.Dtos
{
    public class CreateValidicUserRequestDto
    {
        [Required]
        public string? TenantCode { get; set; }
        [Required]
        public string? ConsumerCode { get; set; }
    }
}