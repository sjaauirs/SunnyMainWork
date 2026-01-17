using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.Common.Core.Helpers.Interfaces;
using SunnyRewards.Helios.User.Core.Domain.Constant;
using SunnyRewards.Helios.User.Core.Domain.Dtos;
using SunnyRewards.Helios.User.Infrastructure.HttpClients.Interfaces;
using System;
using System.Security.Cryptography;

namespace SunnyRewards.Helios.User.Infrastructure.Services
{

    public class EventService : IEventService
    {
        private readonly IAdminClient _adminClient;
        private readonly ILogger<EventService> _logger;
        private readonly IHeliosEventPublisher<CohortEventDto> _heliosEventPublisher;
        private readonly IHttpContextAccessor _httpContextAccessor;
        const string className = nameof(EventService);


        public EventService(ILogger<EventService> eventServiceLogger, IAdminClient adminClient,
            IHeliosEventPublisher<CohortEventDto> heliosEventPublisher,IHttpContextAccessor httpContextAccessor)
        {
            _logger = eventServiceLogger;
            _adminClient = adminClient;
            _heliosEventPublisher = heliosEventPublisher;
            _httpContextAccessor = httpContextAccessor;
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
                EventType = Constant.ConsumerHistoryEvent,
                EventSubtype = Constant.ConsumerHistoryEventSubType,
                EventSource = eventSource,
                EventData = consumers
            };
        }


        public async Task<BaseResponseDto> CreateConsumerHistoryEvent(List<ConsumerDto> consumers , string source = "consumerService")
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
            catch(Exception ex)
            {
                _logger.LogError(ex, "PostEvent - Error :{msg}", ex.Message);
                throw;
            }

        }
        /// <summary>
        /// Publishes a cohort event to the configured SNS topic.
        /// </summary>
        /// <param name="consumerCode">The consumer identifier.</param>
        /// <param name="tenantCode">The tenant identifier.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public async Task PublishCohortEventToSNSTopic(string tenantCode,string consumerCode)
        {
            string methodName = nameof(PublishCohortEventToSNSTopic);
            _logger.LogInformation(
                "{ClassName}.{MethodName}: Starting SNS publish for TenantCode={TenantCode}, ConsumerCode={ConsumerCode}",
                className, methodName, tenantCode, consumerCode);

            try
            {
                if (!ValidateInput(tenantCode, consumerCode))
                {
                    return;
                }

                var eventHeaderDto = BuildCohortEventHeaderDto(tenantCode, consumerCode);
                var eventDataDto = BuildCohortEventDataDto(tenantCode, consumerCode);

                await PublishWithRetry(eventHeaderDto, eventDataDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "{ClassName}.{MethodName}: Exception occurred while sending message to SNS topic. TenantCode={TenantCode}, ConsumerCode={ConsumerCode}",
                    className, methodName, tenantCode, consumerCode);
            }
        }
        /// <summary>
        /// Validates that tenant code and consumer code are not null or empty.
        /// </summary>
        /// <param name="tenantCode">The tenant code.</param>
        /// <param name="consumerCode">The consumer code.</param>
        /// <returns>True if valid; otherwise, false.</returns>
        private bool ValidateInput(string tenantCode, string consumerCode)
        {
            if (string.IsNullOrWhiteSpace(tenantCode) || string.IsNullOrWhiteSpace(consumerCode))
            {
                _logger.LogWarning("{ClassName}.{MethodName}: TenantCode or ConsumerCode is null or empty",
                    className, nameof(ValidateInput));
                return false;
            }
            return true;
        }

        /// <summary>
        /// Builds the event header DTO for SNS publishing.
        /// </summary>
        /// <param name="tenantCode">The tenant code.</param>
        /// <param name="consumerCode">The consumer code.</param>
        /// <returns>An instance of <see cref="EventHeaderDto"/>.</returns>
        private EventHeaderDto BuildCohortEventHeaderDto(string tenantCode, string consumerCode)
        {
            var dto = new EventHeaderDto
            {
                EventId = Guid.NewGuid().ToString("N"),
                EventType = Constant.CohortEventType,
                EventSubtype = Constant.CohortEventSubType,
                PublishTs = DateTime.UtcNow,
                TenantCode = tenantCode,
                ConsumerCode = consumerCode,
                SourceModule = Constant.UserService
            };

            _logger.LogInformation("{ClassName}.{MethodName}: EventHeaderDto created: {@EventHeaderDto}",
                className, nameof(BuildCohortEventHeaderDto), dto);

            return dto;
        }

        /// <summary>
        /// Builds the event data DTO for SNS publishing.
        /// </summary>
        /// <param name="tenantCode">The tenant code.</param>
        /// <param name="consumerCode">The consumer code.</param>
        private CohortEventDto BuildCohortEventDataDto(string tenantCode, string consumerCode)
        {
            var eventId = ExtractRequestIdFromHeader();

            var dto = new CohortEventDto()
            {
                EventId = eventId,
                ConsumerCode = consumerCode,
                TenantCode = tenantCode,
                TriggeredBy = Constant.Import
            };

            _logger.LogInformation("{ClassName}.{MethodName}: EventDataDto created: {@EventDataDto}",
                className, nameof(BuildCohortEventDataDto), dto);

            return dto;
        }

        /// <summary>
        /// Extracts the request ID from the HTTP request header.
        /// </summary>
        /// <returns>The request ID if found; otherwise, an empty string.</returns>
        private string ExtractRequestIdFromHeader()
        {
            if (_httpContextAccessor.HttpContext?.Request.Headers.TryGetValue("X-HELIOS-REQUEST-ID", out var requestId) == true)
            {
                _logger.LogInformation("{ClassName}.{MethodName}: Found request ID in header: {RequestId}",
                    className, nameof(ExtractRequestIdFromHeader), requestId);
                return requestId;
            }

            _logger.LogWarning("{ClassName}.{MethodName}: No request ID found in header",
                className, nameof(ExtractRequestIdFromHeader));

            return string.Empty;
        }

        /// <summary>
        /// Publishes the SNS message with retry logic.
        /// </summary>
        /// <param name="eventHeaderDto">The event header DTO.</param>
        /// <param name="eventDataDto">The event data DTO.</param>
        private async Task PublishWithRetry(EventHeaderDto eventHeaderDto, CohortEventDto eventData)
        {
            const string methodName = nameof(PublishWithRetry);
            int retryCount = Constant.RetryCount;
            PublishResultDto publishResult = new PublishResultDto();

            while (retryCount > 0)
            {
                try
                {
                    _logger.LogInformation("{ClassName}.{MethodName}: Attempting to publish SNS message. Retries left: {RetryCount}",
                        className, methodName, retryCount);

                    publishResult = await _heliosEventPublisher.PublishMessage(eventHeaderDto, eventData, eventData.ConsumerCode,
                        eventHeaderDto.EventId, true);

                    if (publishResult.Published)
                    {
                        _logger.LogInformation("{ClassName}.{MethodName}: SNS message published successfully. EventId: {EventId}",
                            className, methodName, eventHeaderDto.EventId);
                        return;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "{ClassName}.{MethodName}: Exception occurred while publishing SNS message. Retries left: {RetryCount}",
                        className, methodName, retryCount - 1);
                }

                retryCount--;
                if (retryCount > 0)
                {
                    int delay = GetSecureRandomNumber(Constant.RetryMinWaitMS, Constant.RetryMaxWaitMS);
                    _logger.LogWarning("{ClassName}.{MethodName}: Publish failed. Retrying after {Delay} ms. Retries left: {RetryCount},Error:{Error}",
                        className, methodName, delay, retryCount,publishResult?.ErrorMessage);
                    await Task.Delay(delay);
                }
                else
                {
                    _logger.LogError("{ClassName}.{MethodName}: Failed to publish SNS message after all retries. EventId: {EventId}",
                        className, methodName, eventHeaderDto.EventId);
                }
            }
        }

        /// <summary>
        /// Generates a random integer between the specified minimum and maximum values.
        /// </summary>
        /// <param name="minValue">The minimum value (inclusive).</param>
        /// <param name="maxValue">The maximum value (exclusive).</param>
        /// <returns>A random integer.</returns>
        private static int GetSecureRandomNumber(int minValue, int maxValue)
        {
            var buffer = new byte[4];
            RandomNumberGenerator.Fill(buffer);
            int result = BitConverter.ToInt32(buffer, 0) & int.MaxValue;
            return minValue + (result % (maxValue - minValue));
        }
    }
}
