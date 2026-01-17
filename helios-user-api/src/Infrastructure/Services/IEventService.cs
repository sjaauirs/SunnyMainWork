using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.User.Core.Domain.Dtos;

namespace SunnyRewards.Helios.User.Infrastructure.Services
{
    public interface IEventService
    {
        Task<BaseResponseDto> PostEvent(PostEventRequestDto postEventRequestDto);

        /// <summary>
        /// Creates the pick a purse event.
        /// </summary>
        /// <param name="consumerAccountDto">The consumer account dto.</param>
        /// <returns></returns>
        Task<BaseResponseDto> CreateConsumerHistoryEvent(List<ConsumerDto> consumers, string source = "consumerService");
        /// <summary>
        /// Creates cohort event 
        /// </summary>
        /// <param name="consumerCode"></param>
        /// <param name="tenantCode"></param>
        /// <returns></returns>
        Task PublishCohortEventToSNSTopic(string consumerCode, string tenantCode);
    }
}
