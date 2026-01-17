using SunnyRewards.Helios.Common.Core.Domain.Dtos;

namespace SunnyRewards.Helios.User.Core.Domain.Dtos
{
    public class ConsumerLoginResponseDto : BaseResponseDto
    {
        public string? ConsumerCode { get; set; }
        public string? Jwt { get; set; }    // JW token created for this login if successful
    }
}