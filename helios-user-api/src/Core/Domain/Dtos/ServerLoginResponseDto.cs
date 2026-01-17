using SunnyRewards.Helios.Common.Core.Domain.Dtos;

namespace SunnyRewards.Helios.User.Core.Domain.Dtos
{
    public class ServerLoginResponseDto : BaseResponseDto
    {
        public string? ApiToken { get; set; }
    }
}
