using SunnyRewards.Helios.Common.Core.Domain.Dtos;

namespace SunnyRewards.Helios.User.Core.Domain.Dtos
{
    public class GetAllPersonAddressesResponseDto : BaseResponseDto
    {
        public IList<PersonAddressDto>? PersonAddressesList { get; set; }
    }
}