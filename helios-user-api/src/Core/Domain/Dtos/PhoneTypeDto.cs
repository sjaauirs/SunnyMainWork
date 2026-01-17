using SunnyRewards.Helios.Common.Core.Domain.Dtos;

namespace SunnyRewards.Helios.User.Core.Domain.Dtos
{
    public class PhoneTypeDto : BaseDto
    {
        public long PhoneTypeId { get; set; }
        public string? PhoneTypeCode { get; set; }
        public string? PhoneTypeName { get; set; }
        public string? Description { get; set; }
    }
}
