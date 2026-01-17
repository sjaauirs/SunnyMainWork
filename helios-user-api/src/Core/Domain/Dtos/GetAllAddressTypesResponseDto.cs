using SunnyRewards.Helios.Common.Core.Domain.Dtos;

namespace SunnyRewards.Helios.User.Core.Domain.Dtos
{
    public class GetAllAddressTypesResponseDto : BaseResponseDto
    {
        public IList<AddressTypeDto>? AddressTypesList { get; set; }
    }
}
