using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http.Extensions;

namespace SunnyRewards.Helios.User.Core.Domain.Dtos
{
    public class CreatePhoneNumberRequestDto
    {
        [Required]
        public long PersonId { get; set; }
        [Required]
        public string ConsumerCode { get; set; }
        [Required]
        public string TenantCode { get; set; }
        [Required]
        public long PhoneTypeId { get; set; }
        [Required]
        public string? PhoneNumber { get; set; }
        public bool IsVerified { get; set; }
        public DateTime? VerifiedDate { get; set; }
        [Required]
        public string? Source { get; set; }
        [Required]
        public string? CreateUser { get; set; }
    }
}
