using System.ComponentModel.DataAnnotations;

namespace Sunny.Benefits.Bff.Core.Domain.Dtos
{
    public class VerificationEmailRequestDto
    {
        [Required]
        public string UserId { get; set; } = string.Empty;
        [Required]
        public string Email { get; set; } = string.Empty;
    }
}