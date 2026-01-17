using SunnyRewards.Helios.Common.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Tenant.Core.Domain.Dtos
{
    public class SponsorsResponseDto : BaseResponseDto
    {
        public List<SponsorDto>? Sponsors { get; set; }
    }
}
