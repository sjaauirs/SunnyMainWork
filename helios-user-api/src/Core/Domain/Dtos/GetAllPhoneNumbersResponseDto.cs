using SunnyRewards.Helios.Common.Core.Domain.Dtos;

namespace SunnyRewards.Helios.User.Core.Domain.Dtos
{
    public class GetAllPhoneNumbersResponseDto : BaseResponseDto
    {
        public IList<PhoneNumberDto>? PhoneNumbersList { get; set; }
    }
}
