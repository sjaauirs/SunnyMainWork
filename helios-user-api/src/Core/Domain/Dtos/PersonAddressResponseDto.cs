using SunnyRewards.Helios.Common.Core.Domain.Dtos;

namespace SunnyRewards.Helios.User.Core.Domain.Dtos
{
    public class PersonAddressResponseDto : BaseResponseDto
    {
        public PersonAddressDto? PersonAddress { get; set; }
    }
}
