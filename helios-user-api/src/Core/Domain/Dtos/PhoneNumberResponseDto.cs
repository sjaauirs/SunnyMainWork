using SunnyRewards.Helios.Common.Core.Domain.Dtos;

namespace SunnyRewards.Helios.User.Core.Domain.Dtos
{
    public class PhoneNumberResponseDto : BaseResponseDto
    {
        public PhoneNumberDto? PhoneNumber { get; set; }
    }
}
