using System.ComponentModel.DataAnnotations;

namespace SunnyRewards.Helios.User.Core.Domain.Dtos
{
    public class ConsumerActivityRequestDto
    {
        [Required]
        public string ConsumerCode { get; set; } = null!;
        [Required]
        public string TenantCode { get; set; } = null!;
        [Required]
        public string ActivitySource { get; set; } = null!;
        [Required]
        public string ActivityType { get; set; } = null!;
        [Required]
        public string ActivityJson { get; set; } = null!;
    }
}
