using System.ComponentModel.DataAnnotations;

namespace SunnyRewards.Helios.Tenant.Core.Domain.Dtos
{
    public class CustomerSponsorTenantsRequestDto
    {
        [Required]
        public IList<CustomerSponsorTenantRequestDto> CustomerSponsorTenants { get; set; } = [];
    }
}
