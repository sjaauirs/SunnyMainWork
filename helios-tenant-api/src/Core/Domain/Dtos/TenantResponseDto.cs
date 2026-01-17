using SunnyRewards.Helios.Common.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Tenant.Core.Domain.Dtos
{
    public class TenantResponseDto : BaseResponseDto
    {
        public TenantDto? Tenant { get; set; }
    }
}
