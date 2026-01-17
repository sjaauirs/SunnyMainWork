using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.Sweepstakes.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Admin.Infrastructure.Services.Interfaces
{
    public interface ITenantSweepstakesService
    {
        Task<BaseResponseDto> CreateTenantSweepStakes(TenantSweepstakesRequestDto tenantSweepstakesRequestDto);
    }
}
