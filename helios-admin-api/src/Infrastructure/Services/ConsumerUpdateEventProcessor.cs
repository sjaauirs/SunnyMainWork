using Microsoft.Extensions.Logging;
using SunnyRewards.Helios.Admin.Core.Domain.Dtos;
using SunnyRewards.Helios.Admin.Core.Domain.Models;
using SunnyRewards.Helios.Admin.Infrastructure.HttpClients.Interfaces;
using SunnyRewards.Helios.Admin.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.User.Core.Domain.Dtos;
using System.Text.Json;

namespace SunnyRewards.Helios.Admin.Infrastructure.Services
{ 

    public interface IConsumerUpdateEventProcessor : IEventProcessor
    {
    }


    public class ConsumerUpdateEventProcessor : IConsumerUpdateEventProcessor
    {
        private readonly ILogger<ConsumerUpdateEventProcessor> _logger;
        private readonly IUserClient _userClient;
        const string className = nameof(ConsumerUpdateEventProcessor);
        public ConsumerUpdateEventProcessor(
            ILogger<ConsumerUpdateEventProcessor> logger,
            IUserClient userClient)
        {
            _logger = logger;
            _userClient = userClient;
        }

        public async Task<bool> ProcessEvent(PostEventRequestModel eventRequest)
        {
            const string methodName = nameof(ProcessEvent);

            try
            {
                if (string.IsNullOrWhiteSpace(eventRequest?.EventData))
                {
                    _logger.LogWarning("{Class}.{Method}: Received null or empty EventData.", className, methodName);
                    return false;
                }

                var consumers = JsonSerializer.Deserialize<List<ConsumerDto>>(eventRequest.EventData);

                if (consumers == null || consumers.Count == 0)
                {
                    _logger.LogWarning("{Class}.{Method}: No consumers found in event data.", className, methodName);
                    return false;
                }

                _logger.LogInformation("{Class}.{Method}: Processing {Count} consumers.", className, methodName, consumers.Count);

                var response = await _userClient.Post<BaseResponseDto>("consumer-history", consumers);

                if (response?.ErrorCode == null)
                {
                    _logger.LogInformation("{Class}.{Method}: Successfully processed consumer update event.", className, methodName);
                    return true;
                }
                else
                {
                    _logger.LogError("{Class}.{Method}: Failed to process consumer update event. ErrorCode: {ErrorCode}, Message: {ErrorMessage}",
                        className, methodName, response.ErrorCode, response.ErrorMessage);
                    return false;
                }
            }
            catch (JsonException jex)
            {
                _logger.LogError(jex, "{Class}.{Method}: Failed to deserialize EventData. Input: {EventData}", className, methodName, eventRequest?.EventData);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{Class}.{Method}: Unexpected error occurred. Message: {Message}", className, methodName, ex.Message);
                return false;
            }
        }


    }
}
