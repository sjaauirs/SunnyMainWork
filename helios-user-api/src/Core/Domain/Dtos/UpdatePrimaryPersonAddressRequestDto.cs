using System.ComponentModel.DataAnnotations;

namespace SunnyRewards.Helios.User.Core.Domain.Dtos
{
    public class UpdatePrimaryPersonAddressRequestDto
    {
        [Required]
        public long PersonAddressId { get; set; }
        [Required]
        public string UpdateUser { get; set; }
    }
}
