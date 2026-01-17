using SunnyRewards.Helios.User.Core.Domain.Dtos;

namespace SunnyRewards.Helios.User.Infrastructure.Services.Interfaces
{
    public interface IConsumerActivityService
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="consumerActivityRequestDto"></param>
        /// <returns></returns>
        Task<ConsumerActivityResponseDto> CreateConsumerActivityAsync(ConsumerActivityRequestDto consumerActivityRequestDto);
    }
}
