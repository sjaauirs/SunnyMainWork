using System.ComponentModel.DataAnnotations;

namespace SunnyRewards.Helios.User.Core.Domain.Dtos
{
    public class UpdatePersonRequestDto
    {
        [Required]
        public string? ConsumerCode { get; set; }
        public string? PhoneNumber { get; set; }
        [Required]
        public string? UpdateUser { get; set; }
    }
}
