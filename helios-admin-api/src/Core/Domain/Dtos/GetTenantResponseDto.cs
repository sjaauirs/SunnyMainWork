using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.Tenant.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Admin.Core.Domain.Dtos
{
    public class GetTenantResponseDto : BaseResponseDto
    {
        public TenantDto? Tenant { get; set; }
    }
}
