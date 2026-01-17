using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using System.ComponentModel.DataAnnotations;

namespace Sunny.Benefits.Bff.Core.Domain.Dtos
{
    public class LoginRequestDto
    {
        [Required]
        public required string ConsumerCode { get; set; }
    }
}
