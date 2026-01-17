using SunnyRewards.Helios.Admin.Core.Domain.Dtos;
using SunnyRewards.Helios.Admin.Core.Domain.Models;

namespace SunnyRewards.Helios.Admin.Infrastructure.Services.Interfaces
{
    public interface IEventService
        {
        /// <summary>
        /// post event to queue
        /// </summary>
        /// <param name="postEventRequestDto"></param>
        /// <returns>PostEventResponseDto</returns>
        Task<PostEventResponseDto> PostEvent(PostEventRequestDto postEventRequestDto);

        Task<PostEventResponseDto> PostErrorEvent(ConsumerErrorEventDto consumerErrorEventDto);
        }
}
