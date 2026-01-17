using Microsoft.Extensions.Logging;
using Sunny.Benefits.Bff.Core.Domain.Constants;
using Sunny.Benefits.Bff.Infrastructure.Helpers.Interface;
using Sunny.Benefits.Bff.Infrastructure.HttpClients.Interfaces;
using Sunny.Benefits.Bff.Infrastructure.Repositories.Interfaces;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.Common.Core.Extensions;
using SunnyRewards.Helios.Common.Core.Helpers.Interfaces;
using SunnyRewards.Helios.NotificationService.Core.Domain.Dtos;
using SunnyRewards.Helios.User.Core.Domain.Dtos;

namespace Sunny.Benefits.Bff.Infrastructure.Helpers
{
    public class NotificationHelper : INotificationHelper
    {
        private readonly ILogger<NotificationHelper> _notificationHelperLogger;
        private readonly IUserClient _userClient;
        private readonly INotificationClient _notificationClient;
        private readonly IHeliosEventPublisher<Dictionary<string, object>> _eventPublisher;
        private const string className = nameof(NotificationHelper);

        public NotificationHelper(ILogger<NotificationHelper> notificationHelperLogger, IUserClient userClient, INotificationClient notificationClient, IHeliosEventPublisher<Dictionary<string, object>> eventPublisher)
        {
            _notificationHelperLogger = notificationHelperLogger;
            _userClient = userClient;
            _notificationClient = notificationClient;
            _eventPublisher = eventPublisher;
        }

        public async Task ProcessNotification(string consumerCode, string tenantCode, string eventTypeName)
        {
            const string methodName = nameof(ProcessNotification);
            try
            {
                _notificationHelperLogger.LogInformation("{ClassName}.{MethodName} - Started processing notification for TenantCode: {TenantCode}, ConsumerCode: {ConsumerCode}", className, methodName, tenantCode, consumerCode);
                
                // Fetch user details
                var getConsumerRequestDto = new GetConsumerRequestDto
                {
                    ConsumerCode = consumerCode
                };
                var userResponse = await _userClient.Post<GetPersonAndConsumerResponseDto>(CardOperationConstants.GetPersonAndConsumerDetails, getConsumerRequestDto);
                if (userResponse == null || userResponse?.ErrorCode != null)
                {
                    _notificationHelperLogger.LogError("{ClassName}.{MethodName} - Failed to fetch user details for TenantCode: {TenantCode}, ConsumerCode: {ConsumerCode}", className, methodName, tenantCode, consumerCode);
                    return;
                }

                // Fetch primary phone number
                var parameters = new Dictionary<string, string>();
                var phoneNumberResponse = await _userClient.GetId<GetAllPhoneNumbersResponseDto>($"phone-number/{userResponse?.Person?.PersonId}?isPrimary=true", parameters);
                if (phoneNumberResponse == null || phoneNumberResponse?.ErrorCode != null)
                {
                    _notificationHelperLogger.LogError("{ClassName}.{MethodName}: Error occurred while getting phone numbers for person with ID:{PersonId}, ErrorCode: {ErrorCode} and Error Message: {ErrorMessage}",
                        className, methodName, userResponse?.Person?.PersonId, phoneNumberResponse?.ErrorCode, phoneNumberResponse?.ErrorMessage);
                }

                // Fetch notification event type by event name
                var notificationEventResponse = await _notificationClient.GetId<NotificationEventTypeResponseDto>(
                    $"notification-event-type?notificationEventName={eventTypeName}",
                    parameters
                );
                if (notificationEventResponse == null || notificationEventResponse?.ErrorCode != null)
                {
                    _notificationHelperLogger.LogError("{ClassName}.{MethodName}: Error occurred while retrieving Notification Event Type. notificationEventTypeName: {NotificationEventTypeName}, Response: {ResponseData}, ErrorCode: {ErrorCode}",
                        className, methodName, eventTypeName, notificationEventResponse?.ToJson(), notificationEventResponse?.ErrorCode);
                    return;
                }

                // Fetch notification category by category id
                var notificationCategoryResponse = await _notificationClient.GetId<NotificationCategoryResponseDto>(
                    $"notification-category?notificationCategoryId={notificationEventResponse?.NotificationEventType?.NotificationCategoryId}",
                    parameters
                );
                if (notificationCategoryResponse == null || notificationCategoryResponse?.ErrorCode != null)
                {
                    _notificationHelperLogger.LogError("{ClassName}.{MethodName}: Error occurred while retrieving Notification Category. notificationCategoryId: {notificationCategoryId}, Response: {ResponseData}, ErrorCode: {ErrorCode}",
                        className, methodName, notificationEventResponse?.NotificationEventType?.NotificationCategoryId, notificationCategoryResponse?.ToJson(), notificationCategoryResponse?.ErrorCode);
                    return;
                }

                //Create notification event object
                var notificationEvent = ConstructNotificationEvent(userResponse, phoneNumberResponse, tenantCode, consumerCode, notificationCategoryResponse.NotificationCategory.CategoryName, eventTypeName);

                // Publish the notification event
                var publishResult = await PublishSNSMessage(notificationEvent);
                if (publishResult.Published)
                {
                    _notificationHelperLogger.LogInformation("{ClassName}.{MethodName} - Successfully processed notification for TenantCode: {TenantCode}, ConsumerCode: {ConsumerCode}", className, methodName, tenantCode, consumerCode);
                    return;
                }
                else
                {
                    _notificationHelperLogger.LogError("{ClassName}.{MethodName} - Error processing notification for TenantCode: {TenantCode}, ConsumerCode: {ConsumerCode}", className, methodName, tenantCode, consumerCode);
                    return;
                }
            }
            catch (Exception ex)
            {
                _notificationHelperLogger.LogError(ex, "{ClassName}.{MethodName} - An error occurred while processing notification. Error Message: {ErrorMessage}", className, methodName, ex.Message);
            }
        }

        private EventDto<Dictionary<string, object>> ConstructNotificationEvent(GetPersonAndConsumerResponseDto userData, GetAllPhoneNumbersResponseDto phoneNumberData, string tenantCode, string consumerCode, string eventType, string eventSubType)
        {
            const string methodName = nameof(PublishSNSMessage);
            try
            {
                var notificationEventData = new Dictionary<string, object>
                {
                    { "FirstName", userData?.Person?.FirstName },
                    { "LastName", userData?.Person?.LastName },
                    { "Email", userData?.Person?.Email },
                    { "PhoneNumber", phoneNumberData?.PhoneNumbersList?[0].PhoneNumber },
                    { "TenantCode", tenantCode },
                    { "ConsumerCode", consumerCode }
                };

                var notificationEventHeader = new EventHeaderDto
                {
                    EventId = Guid.NewGuid().ToString("N"),
                    EventType = eventType,
                    EventSubtype = eventSubType,
                    PublishTs = DateTime.UtcNow,
                    TenantCode = tenantCode,
                    ConsumerCode = consumerCode,
                    SourceModule = CardOperationConstants.NotificationEventSourceModule
                };

                var notificationEvent = new EventDto<Dictionary<string, object>>
                {
                    Header = notificationEventHeader,
                    Data = notificationEventData
                };

                _notificationHelperLogger.LogInformation("{ClassName}.{MethodName}: Notification event object constructed. NotificationEventData: {NotificationEventData}",
                        className, methodName, notificationEvent.ToJson());

                return notificationEvent;
            }
            catch (Exception ex)
            {
                _notificationHelperLogger.LogError(ex, "{ClassName}.{MethodName}: Exception occurred while constructing notification event object. Error: {error}",
                    className, methodName, ex.Message);
                throw;
            }
        }

        private async Task<PublishResultDto> PublishSNSMessage(EventDto<Dictionary<string, object>> notificationEvent)
        {
            const string methodName = nameof(PublishSNSMessage);
            PublishResultDto publishResult = new PublishResultDto
            {
                Published = false,
                ErrorMessage = string.Empty
            };
            try
            {
                publishResult = await _eventPublisher.PublishMessage(notificationEvent.Header, notificationEvent.Data);
                if (publishResult.Published)
                {
                    _notificationHelperLogger.LogInformation("{ClassName}.{MethodName}: Message published successfully. NotificationEventData: {NotificationEventData}",
                        className, methodName, notificationEvent.ToJson());
                }
                else
                {
                    _notificationHelperLogger.LogWarning("{ClassName}.{MethodName}: Message publishing failed. ErrorMessage: {ErrorMessage}, NotificationEventData: {NotificationEventData}",
                        className, methodName, publishResult.ErrorMessage, notificationEvent.ToJson());
                }
                return publishResult;
            }
            catch (Exception ex)
            {
                _notificationHelperLogger.LogError(ex, "{ClassName}.{MethodName}: Exception occurred while publishing message. NotificationEventData: {NotificationEventData}, Error: {error}",
                    className, methodName, notificationEvent.ToJson(), ex.Message);
                throw;
            }
        }
    }
}
