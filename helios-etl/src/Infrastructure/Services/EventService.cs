using Microsoft.Extensions.Logging;
using SunnyRewards.Helios.User.Core.Domain.Dtos;
using SunnyRewards.Helios.ETL.Infrastructure.HttpClients.Interfaces;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.ETL.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.ETL.Core.Domain.Dtos;
using SunnyRewards.Helios.ETL.Core.Domain.Constants;

namespace SunnyRewards.Helios.ETL.Infrastructure.Services
{
    public class EventService : IEventService
    {
        private readonly IAdminClient _adminClient;
        private readonly ILogger<EventService> _logger;
        const string className = nameof(EventService);


        public EventService(ILogger<EventService> eventServiceLogger, IAdminClient adminClient)
        {
            _logger = eventServiceLogger;
            _adminClient = adminClient;
        }

        /// <summary>
        /// Post Event to Admin, using Admin client
        /// </summary>
        /// <param name="PostEventRequestDto"></param>
        /// <returns></returns>
        public async Task<BaseResponseDto> PostEvent(PostEventRequestDto postEventRequestDto)
        {
            const string methodName = nameof(PostEvent);
            try
            {
                _logger.LogInformation("Sending Event Request to Admin for consumer code {consumerCode}", postEventRequestDto.ConsumerCode);
                var response = await _adminClient.Post<BaseResponseDto>("post-event", postEventRequestDto);
                _logger.LogInformation("{className}.{methodName}: Posted Event Request to Admin successfully for consumerCode : {consumerCode}", className, methodName, postEventRequestDto.ConsumerCode);
                return response;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "PostEvent - Error :{msg}", ex.Message);
                throw;
            }
        }



        private static PostEventRequestDto BuildPostEventRequest(List<ConsumerDto> consumers, string eventSource)
        {
            return new PostEventRequestDto
            {
                ConsumerCode = consumers[0].ConsumerCode!,
                TenantCode = consumers[0].TenantCode!,
                EventType = AdminConstants.ConsumerHistoryEvent,
                EventSubtype = AdminConstants.ConsumerHistoryEventSubType,
                EventSource = eventSource,
                EventData = consumers
            };
        }


        public async Task<BaseResponseDto> CreateConsumerHistoryEvent(List<ConsumerDto> consumers, string source = "consumerService")
        {
            try
            {
                if (consumers == null || consumers.Count == 0)
                {
                    return new BaseResponseDto();
                }
                var events = EventService.BuildPostEventRequest(consumers, source);
                var result = await PostEvent(events);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "PostEvent - Error :{msg}", ex.Message);
                throw;
            }

        }
    }
}
