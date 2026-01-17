using SunnyRewards.Helios.Common.Core.Domain.Dtos;

namespace Sunny.Benefits.Bff.Core.Domain.Dtos
{
    public class LoginResponseDto : BaseResponseDto
    {
        public string? ConsumerCode { get; set; }
        public string? Jwt { get; set; }
    }
}
