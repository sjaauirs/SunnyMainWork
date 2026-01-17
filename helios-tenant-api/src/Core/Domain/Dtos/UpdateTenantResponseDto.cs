using SunnyRewards.Helios.Common.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Tenant.Core.Domain.Dtos
{
    public class UpdateTenantResponseDto : BaseResponseDto
    {
        public TenantDto? UpdateTenant { get; set; }
    }
}
