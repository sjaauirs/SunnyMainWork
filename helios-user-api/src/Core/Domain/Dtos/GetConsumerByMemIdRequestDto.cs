using System.ComponentModel.DataAnnotations;

namespace SunnyRewards.Helios.User.Core.Domain.Dtos
{
    public class GetConsumerByMemIdRequestDto
    {
        [Required]
        public string? TenantCode { get; set; }

       
        [Required]
        public string? MemberId { get; set; }
    }
}
