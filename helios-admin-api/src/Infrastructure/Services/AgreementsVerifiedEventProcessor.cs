using Microsoft.Extensions.Logging;
using SunnyRewards.Helios.Admin.Core.Domain.Dtos;
using SunnyRewards.Helios.Admin.Core.Domain.Models;
using SunnyRewards.Helios.Admin.Infrastructure.Helpers.Interface;
using SunnyRewards.Helios.Admin.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.User.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Admin.Infrastructure.Services
{
    public class AgreementsVerifiedEventProcessor : IAgreementsVerifiedEventProcessor
    {
        private readonly ILogger<AgreementsVerifiedEventProcessor> _logger;
        private readonly IAgreementsVerifiedEventService _agreementsVerifiedEventService;
        private readonly IConsumerEventProcessorHelper _eventProcessorHelper;
        private const string ClassName = nameof(AgreementsVerifiedEventProcessor);

        public AgreementsVerifiedEventProcessor(
            ILogger<AgreementsVerifiedEventProcessor> logger,
            IAgreementsVerifiedEventService agreementsVerifiedEventService,
            IConsumerEventProcessorHelper eventProcessorHelper)
        {
            _logger = logger;
            _agreementsVerifiedEventService = agreementsVerifiedEventService;
            _eventProcessorHelper = eventProcessorHelper;
        }

        /// <summary>
        /// Process AgreementsVerified event
        /// </summary>
        /// <param name="eventRequest"></param>
        /// <returns></returns>
        public async Task<bool> ProcessEvent(EventDto<AgreementsVerifiedEventDto> eventRequest)
        {
            const string methodName = nameof(ProcessEvent);
            try
            {
                _logger.LogInformation("{ClassName}.{MethodName} : Started processing ConsumerCode:{ConsumerCode}, TenantCode:{TenantCode}",
                    ClassName, methodName, eventRequest.Header.ConsumerCode, eventRequest.Header.TenantCode);

                // Create request DTO for the service
                var requestDto = new AgreementsVerifiedEventRequestDto
                {
                    ConsumerCode = eventRequest.Header.ConsumerCode ?? string.Empty,
                    TenantCode = eventRequest.Header.TenantCode ?? string.Empty
                };

                var postedEventData = new PostedEventData
                {
                    EventSubtype = eventRequest.Header.EventSubtype,
                    EventType = eventRequest.Header.EventType,
                };

                var argInstances = new Dictionary<string, object>
                {
                     { nameof(AgreementsVerifiedEventRequestDto), requestDto },
                     { nameof(AgreementsVerifiedEventService), _agreementsVerifiedEventService },
                     { nameof(PostedEventData), postedEventData }
                };

                // Call the service to process the agreements verified event
                var eventResult =  await _eventProcessorHelper.ProcessEventAsync(eventRequest, argInstances, ClassName);

                _logger.LogInformation("{ClassName}.{MethodName} : Successfully processed ConsumerCode:{ConsumeCode},TenantCode:{TenantCode}",
                        ClassName, methodName, eventRequest.Header.ConsumerCode, eventRequest.Header.TenantCode);

                return eventResult;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName} : Error processing AgreementsVerified event for ConsumerCode:{ConsumerCode}",
                    ClassName, methodName, eventRequest.Header.ConsumerCode);
                throw;
            }
        }
    }
}
