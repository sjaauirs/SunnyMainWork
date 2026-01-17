using SunnyRewards.Helios.Common.Core.Domain.Dtos;

namespace SunnyRewards.Helios.User.Core.Domain.Dtos
{
    public class AddressTypeDto : BaseDto
    {
        public long AddressTypeId { get; set; }
        public string? AddressTypeCode { get; set; }
        public string? AddressTypeName { get; set; }
        public string? Description { get; set; }

    }
}
