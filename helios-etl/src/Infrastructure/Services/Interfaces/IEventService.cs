using SunnyRewards.Helios.User.Core.Domain.Dtos;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.ETL.Core.Domain.Dtos;

namespace SunnyRewards.Helios.ETL.Infrastructure.Services.Interfaces
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
    }
}
