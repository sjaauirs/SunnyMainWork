using SunnyRewards.Helios.User.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Wallet.Infrastructure.Services.Interfaces
{
    public interface IConsumerService
    {
        /// <summary>
        /// This method fetches the consumer details based on the provided request.
        /// </summary>
        /// <param name="getConsumerRequestDto">request contains consumer code fetch Consumer</param>
        /// <returns>Response contains the Consumer information</returns>
        Task<GetConsumerResponseDto> GetConsumer(GetConsumerRequestDto getConsumerRequestDto);
    }
}
