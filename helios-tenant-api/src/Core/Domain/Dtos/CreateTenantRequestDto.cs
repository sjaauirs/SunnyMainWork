using System.ComponentModel.DataAnnotations;

namespace SunnyRewards.Helios.Tenant.Core.Domain.Dtos
{
    public class CreateTenantRequestDto
    {
        [Required]
        public string CustomerCode { get; set; } = null!;

        [Required]
        public string SponsorCode { get; set; } = null!;

        [Required]
        public PostTenantDto Tenant { get; set; } = null!;
    }
}
