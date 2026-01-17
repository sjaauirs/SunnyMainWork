using Microsoft.Extensions.Logging;
using SunnyRewards.Helios.User.Core.Domain.Dtos;
using SunnyRewards.Helios.Wallet.Infrastructure.HttpClients.Interfaces;
using SunnyRewards.Helios.Wallet.Infrastructure.Services.Interfaces;

namespace SunnyRewards.Helios.Wallet.Infrastructure.Services
{
    public class ConsumerService : IConsumerService
    {
        private readonly IUserClient _userClient;
        private readonly ILogger<ConsumerService> _logger;
        private readonly string className = nameof(ConsumerService);

        public ConsumerService(IUserClient userClient, ILogger<ConsumerService> logger)
        {
            _userClient = userClient;
            _logger = logger;
        }

        /// <summary>
        /// Fetches consumer details to determine if the consumer is a spouse or dependent.
        /// </summary>
        /// <param name="getConsumerRequestDto">DTO containing ConsumerCode</param>
        /// <returns>Consumer details wrapped in GetConsumerResponseDto</returns>
        public async Task<GetConsumerResponseDto> GetConsumer(GetConsumerRequestDto getConsumerRequestDto)
        {
            const string methodName = nameof(GetConsumer);
            var consumer = await _userClient.Post<GetConsumerResponseDto>("consumer/get-consumer", getConsumerRequestDto);

            if (consumer.Consumer == null)
            {
                _logger.LogError("{className}.{methodName}: Consumer Details Not Found For Consumer Code:{consumerCode}",
                    className, methodName, getConsumerRequestDto.ConsumerCode);
                return new GetConsumerResponseDto();
            }

            _logger.LogInformation("{className}.{methodName}: Retrieved Consumer Details Successfully for ConsumerCode : {ConsumerCode}",
                className, methodName, getConsumerRequestDto.ConsumerCode);

            return consumer;
        }
    }
}
