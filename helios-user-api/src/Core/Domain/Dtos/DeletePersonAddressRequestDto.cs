using System.ComponentModel.DataAnnotations;

namespace SunnyRewards.Helios.User.Core.Domain.Dtos
{
    public class DeletePersonAddressRequestDto
    {
        [Required]
        public long PersonAddressId { get; set; }
        [Required]
        public string UpdateUser { get; set; }
    }
}
