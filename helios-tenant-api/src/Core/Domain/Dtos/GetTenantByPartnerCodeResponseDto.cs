using SunnyRewards.Helios.Common.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Tenant.Core.Domain.Dtos
{
    public class GetTenantByPartnerCodeResponseDto : BaseResponseDto
    {
        public TenantDto? Tenant { get; set; }
    }
}
