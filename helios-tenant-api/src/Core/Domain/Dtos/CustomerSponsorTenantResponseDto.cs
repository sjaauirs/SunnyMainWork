using SunnyRewards.Helios.Common.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Tenant.Core.Domain.Dtos
{
    public class CustomerSponsorTenantResponseDto : BaseResponseDto
    {
        public CustomerDto? Customer { get; set; }

        public SponsorDto? Sponsor { get; set; }

        public TenantDto? Tenant { get; set; }
    }
}
