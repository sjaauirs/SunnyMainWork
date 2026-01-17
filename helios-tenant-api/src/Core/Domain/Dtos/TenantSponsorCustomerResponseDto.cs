using SunnyRewards.Helios.Common.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Tenant.Core.Domain.Dtos
{
    public class TenantSponsorCustomerResponseDto : BaseResponseDto
    {
        public TenantDto? Tenant { get; set; }
        public SponsorDto? Sponsor { get; set; }
        public CustomerDto? Customer { get; set; }
    }
}
