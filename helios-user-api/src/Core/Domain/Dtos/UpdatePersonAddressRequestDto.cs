using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SunnyRewards.Helios.User.Core.Domain.Dtos
{
    public class UpdatePersonAddressRequestDto
    {
        [Required]
        public long PersonAddressId { get; set; }
        [Required]
        public string ConsumerCode { get; set; }
        [Required]
        public string TenantCode { get; set; }
        public long AddressTypeId { get; set; }
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
        [Required]
        public string UpdateUser { get; set; }
    }
}
