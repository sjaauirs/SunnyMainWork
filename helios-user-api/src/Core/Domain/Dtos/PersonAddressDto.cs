using SunnyRewards.Helios.Common.Core.Domain.Dtos;

namespace SunnyRewards.Helios.User.Core.Domain.Dtos
{
    public class PersonAddressDto : BaseDto
    {
        public long PersonAddressId { get; set; }
        public long AddressTypeId { get; set; }
        public long PersonId { get; set; }
        public string? AddressLabel { get; set; }
        public string? Line1 { get; set; }
        public string? Line2 { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? PostalCode { get; set; }
        public string? Region { get; set; }
        public string? CountryCode { get; set; }
        public string? Country { get; set; }
        public string? Source { get; set; }
        public bool IsPrimary { get; set; }
    }
}
