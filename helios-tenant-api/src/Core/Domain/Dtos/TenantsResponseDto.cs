using SunnyRewards.Helios.Common.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Tenant.Core.Domain.Dtos
{
    public class TenantsResponseDto : BaseResponseDto
    {
        public List<TenantDto>? Tenants { get; set; }
    }
}
