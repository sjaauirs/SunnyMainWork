using SunnyRewards.Helios.Admin.Core.Domain.Constants;
using SunnyRewards.Helios.Admin.Infrastructure.HttpClients.Interfaces;
using SunnyRewards.Helios.Admin.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.User.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Admin.Infrastructure.Services
{
    public class ConsumerService : IConsumerService
    {
        private readonly IUserClient _userClient;

        public ConsumerService(IUserClient userClient)
        {
            _userClient = userClient;
        }

        /// <summary>
        /// Retrieves a consumer that matches the given TenantCode and MemNbr.
        /// </summary>
        /// <param name="consumerRequestDto">The request data containing the TenantCode and MemNbr.</param>
        /// <returns>A response containing the consumer details or an error message.</returns>
        public async Task<GetConsumerByMemIdResponseDto> GetConsumerByMemId(GetConsumerByMemIdRequestDto consumerRequestDto)
        {
            return await _userClient.Post<GetConsumerByMemIdResponseDto>(Constant.GetConsumerByMemId, consumerRequestDto);
        }
        /// <summary>
        /// Retrieves consumer details based on the provided request data.
        /// </summary>
        /// <param name="consumerRequestDto">The request data containing the consumer code.</param>
        /// <returns>A response containing the consumer details or an error message.</returns>
        public async Task<GetConsumerResponseDto> GetConsumerData(GetConsumerRequestDto consumerRequestDto)
        {
            return await _userClient.Post<GetConsumerResponseDto>(Constant.GetConsumer, consumerRequestDto);
        }
    }
}
