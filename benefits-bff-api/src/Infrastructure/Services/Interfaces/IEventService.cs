using SunnyBenefits.Fis.Core.Domain.Dtos;
using SunnyRewards.Helios.Admin.Core.Domain.Dtos;

public interface IEventService
{
    /// <summary>
    /// this method send event to Admin
    /// </summary>
    /// <param name="postEventRequestDto"></param>
    /// <returns></returns>
    Task<PostEventResponseDto> PostEvent(PostEventRequestDto postEventRequestDto);

    /// <summary>
    /// Creates the pick a purse event.
    /// </summary>
    /// <param name="consumerAccountDto">The consumer account dto.</param>
    /// <returns></returns>
    Task<PostEventResponseDto> CreatePickAPurseEvent(ConsumerAccountDto consumerAccountDto);
}
