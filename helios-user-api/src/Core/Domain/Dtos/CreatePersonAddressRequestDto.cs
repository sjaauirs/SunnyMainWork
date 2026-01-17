using System.ComponentModel.DataAnnotations;

namespace SunnyRewards.Helios.User.Core.Domain.Dtos
{
    public class CreatePersonAddressRequestDto
    {
        [Required]
        public long AddressTypeId { get; set; }
        [Required]
        public long PersonId { get; set; }
        [Required]
        public string ConsumerCode { get; set; }
        [Required]
        public string TenantCode { get; set; }
        public string? AddressLabel { get; set; }
        [Required]
        public string Line1 { get; set; }
        public string? Line2 { get; set; }
        [Required]
        public string City { get; set; }
        [Required]
        public string State { get; set; }
        [Required]
        public string PostalCode { get; set; }
        [Required]
        public string Region { get; set; }
        [Required]
        public string CountryCode { get; set; }
        [Required]
        public string Country { get; set; }
        [Required]
        public string Source { get; set; }
        [Required]
        public string CreateUser { get; set; }
    }
}
