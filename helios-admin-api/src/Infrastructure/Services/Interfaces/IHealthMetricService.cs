using SunnyBenefits.Health.Core.Domains.Dtos;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Admin.Infrastructure.Services.Interfaces
{
    public interface IHealthMetricService
    {
        /// <summary>
        /// Saves the health metrics.
        /// </summary>
        /// <param name="healthMetricMessageRequestDto">The health metric message request dto.</param>
        /// <returns></returns>
        Task<BaseResponseDto> SaveHealthMetrics(HealthMetricMessageRequestDto healthMetricMessageRequestDto);
    }
}
