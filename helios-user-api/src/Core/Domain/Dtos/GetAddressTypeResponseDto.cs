using SunnyRewards.Helios.Common.Core.Domain.Dtos;

namespace SunnyRewards.Helios.User.Core.Domain.Dtos
{
    public class GetAddressTypeResponseDto : BaseResponseDto
    {
        public AddressTypeDto? AddressType { get; set; }
    }
}
