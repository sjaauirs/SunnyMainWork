using SunnyRewards.Helios.Common.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Tenant.Core.Domain.Dtos
{
    /// <summary>
    /// Represents the response DTO for retrieving customer, sponsor, and tenant details.
    /// This DTO extends <see cref="BaseResponseDto"/> and contains lists of customers, sponsors, and tenants.
    /// </summary>
    public class CustomerSponsorTenantsResponseDto : BaseResponseDto
    {
        public IList<CustomerSponsorTenantResponseDto>? CustomerSponsorTenants { get; set; }
    }
}
