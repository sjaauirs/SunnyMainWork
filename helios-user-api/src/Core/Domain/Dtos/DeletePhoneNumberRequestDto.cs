using System.ComponentModel.DataAnnotations;

namespace SunnyRewards.Helios.User.Core.Domain.Dtos
{
    public class DeletePhoneNumberRequestDto
    {
        [Required]
        public long PhoneNumberId { get; set; }
        [Required]
        public string? UpdateUser { get; set; }
    }
}
