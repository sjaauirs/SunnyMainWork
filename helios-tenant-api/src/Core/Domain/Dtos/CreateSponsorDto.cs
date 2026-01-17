using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using System.ComponentModel.DataAnnotations;

namespace SunnyRewards.Helios.Tenant.Core.Domain.Dtos
{
    public class CreateSponsorDto
    {
        public long? SponsorId { get; set; }
        [Required]
        public long? CustomerId { get; set; }
        [Required]
        public string SponsorCode { get; set; } = string.Empty;
        [Required]
        public string SponsorName { get; set; } = string.Empty;
        public string SponsorDescription { get; set; } = string.Empty;
    }
}
