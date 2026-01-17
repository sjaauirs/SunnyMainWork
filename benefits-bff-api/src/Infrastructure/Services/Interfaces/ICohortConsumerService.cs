using SunnyRewards.Helios.Cohort.Core.Domain.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sunny.Benefits.Bff.Infrastructure.Services.Interfaces
{
    public interface ICohortConsumerService
    {
        Task<CohortConsumerResponseDto> GetConsumerCohorts(GetConsumerByCohortsNameRequestDto requestDto, string? requestId = null);
        Task<CohortsResponseDto> GetConsumerAllCohorts(string tenantCode, string consumerCode);
    }
}
