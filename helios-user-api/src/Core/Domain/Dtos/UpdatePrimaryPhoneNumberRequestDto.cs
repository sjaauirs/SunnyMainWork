using System.ComponentModel.DataAnnotations;

namespace SunnyRewards.Helios.User.Core.Domain.Dtos
{
    public class UpdatePrimaryPhoneNumberRequestDto
    {
        [Required]
        public long PhoneNumberId { get; set; }
        [Required]
        public string? UpdateUser { get; set; }
    }
}
