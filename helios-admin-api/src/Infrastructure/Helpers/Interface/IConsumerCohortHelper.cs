using SunnyRewards.Helios.Cohort.Core.Domain.Dtos;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.User.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Admin.Infrastructure.Helpers.Interface
{
    public interface IConsumerCohortHelper
    {
        Task<CohortsResponseDto> GetConsumerCohorts(ConsumerCohortsRequestDto requestDto);
        Task<BaseResponseDto> AddConsumerCohort(CohortConsumerRequestDto cohortConsumerRequestDto);
        Task<BaseResponseDto> RemoveConsumerCohort(CohortConsumerRequestDto cohortConsumerRequestDto);
    }
}
