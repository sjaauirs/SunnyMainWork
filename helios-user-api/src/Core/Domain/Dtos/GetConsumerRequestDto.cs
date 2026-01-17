using System.ComponentModel.DataAnnotations;

namespace SunnyRewards.Helios.User.Core.Domain.Dtos
{
    public class GetConsumerRequestDto
    {
        [Required]
        public string? ConsumerCode { get; set; }
    }
}
