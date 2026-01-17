using SunnyRewards.Helios.User.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Admin.Infrastructure.Services.Interfaces
{
    public interface IConsumerService
    {
        /// <summary>
        /// Retrieves Consumer matching the given Tenant+MemNbr
        /// </summary>
        /// <param name="consumerRequestDto"></param>
        /// <returns></returns>
        Task<GetConsumerByMemIdResponseDto> GetConsumerByMemId(GetConsumerByMemIdRequestDto consumerRequestDto);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="consumerRequestDto"></param>
        /// <returns></returns>
        Task<GetConsumerResponseDto> GetConsumerData(GetConsumerRequestDto consumerRequestDto);

    }
}
