using Microsoft.Extensions.Logging;
using Moq;
using Sunny.Benefits.Bff.Infrastructure.Helpers;
using Sunny.Benefits.Bff.Infrastructure.HttpClients.Interfaces;
using Sunny.Benefits.Bff.Infrastructure.Repositories.Interfaces;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.Common.Core.Helpers.Interfaces;
using SunnyRewards.Helios.NotificationService.Core.Domain.Dtos;
using SunnyRewards.Helios.User.Core.Domain.Dtos;
using Xunit;

namespace Sunny.Benefits.Bff.UnitTest.Helpers
{
    public class NotificationHelperUnitTests
    {
        private readonly Mock<IUserClient> _userClient;
        private readonly Mock<ILogger<NotificationHelper>> _notificationHelperLogger;
        private readonly Mock<INotificationClient> _notificationClient;
        private readonly Mock<IHeliosEventPublisher<Dictionary<string, object>>> _eventPublisherMock;
        private readonly NotificationHelper _notificationHelper;

        public NotificationHelperUnitTests()
        {
            _userClient = new Mock<IUserClient>();
            _notificationHelperLogger = new Mock<ILogger<NotificationHelper>>();
            _notificationClient = new Mock<INotificationClient>();
            _eventPublisherMock = new Mock<IHeliosEventPublisher<Dictionary<string, object>>>();
            _notificationHelper = new NotificationHelper(_notificationHelperLogger.Object, _userClient.Object, _notificationClient.Object, _eventPublisherMock.Object);
        }

        [Fact]
        public async Task ProcessNotification_Should_LogError_When_UserDetails_Are_Null()
        {
            _userClient.Setup(x => x.Post<GetPersonAndConsumerResponseDto>(It.IsAny<string>(), It.IsAny<object>()))
                .ReturnsAsync((GetPersonAndConsumerResponseDto)null);

            await _notificationHelper.ProcessNotification("test-consumer", "test-tenant", "CardOrdered");

            _notificationHelperLogger.VerifyLog(LogLevel.Error);
        }

        [Fact]
        public async Task ProcessNotification_Should_LogError_When_UserDetails_Has_ErrorCode()
        {
            _userClient.Setup(x => x.Post<GetPersonAndConsumerResponseDto>(It.IsAny<string>(), It.IsAny<object>()))
                .ReturnsAsync(new GetPersonAndConsumerResponseDto { ErrorCode = 404 });

            await _notificationHelper.ProcessNotification("test-consumer", "test-tenant", "CardOrdered");

            _notificationHelperLogger.VerifyLog(LogLevel.Error);
        }

        [Fact]
        public async Task ProcessNotification_Should_LogError_When_PhoneNumber_Has_ErrorCode()
        {
            _userClient.Setup(x => x.Post<GetPersonAndConsumerResponseDto>(It.IsAny<string>(), It.IsAny<object>()))
                .ReturnsAsync(new GetPersonAndConsumerResponseDto { Person = new PersonDto { PersonId = 123L } });

            _userClient.Setup(x => x.GetId<GetAllPhoneNumbersResponseDto>(It.IsAny<string>(), It.IsAny<Dictionary<string, string>>()))
                .ReturnsAsync(new GetAllPhoneNumbersResponseDto { ErrorCode = 404 });

            await _notificationHelper.ProcessNotification("test-consumer", "test-tenant", "CardOrdered");

            _notificationHelperLogger.VerifyLog(LogLevel.Error);
        }

        [Fact]
        public async Task ProcessNotification_Should_LogError_When_NotificationEventType_Has_ErrorCode()
        {
            SetupValidUserAndPhone();

            _notificationClient.Setup(x => x.GetId<NotificationEventTypeResponseDto>(It.IsAny<string>(), It.IsAny<Dictionary<string, string>>()))
                .ReturnsAsync(new NotificationEventTypeResponseDto { ErrorCode = 404 });

            await _notificationHelper.ProcessNotification("test-consumer", "test-tenant", "CardOrdered");

            _notificationHelperLogger.VerifyLog(LogLevel.Error);
        }

        [Fact]
        public async Task ProcessNotification_Should_LogError_When_NotificationCategory_Has_ErrorCode()
        {
            SetupValidUserAndPhone();

            _notificationClient.Setup(x => x.GetId<NotificationEventTypeResponseDto>(It.IsAny<string>(), It.IsAny<Dictionary<string, string>>()))
                .ReturnsAsync(new NotificationEventTypeResponseDto
                {
                    NotificationEventType = new NotificationEventTypeDto { NotificationCategoryId = 1001L }
                });

            _notificationClient.Setup(x => x.GetId<NotificationCategoryResponseDto>(It.IsAny<string>(), It.IsAny<Dictionary<string, string>>()))
                .ReturnsAsync(new NotificationCategoryResponseDto { ErrorCode = 404 });

            await _notificationHelper.ProcessNotification("test-consumer", "test-tenant", "CardOrdered");

            _notificationHelperLogger.VerifyLog(LogLevel.Error);
        }

        [Fact]
        public async Task ProcessNotification_Should_LogInformation_When_Publish_Succeeds()
        {
            SetupValidNotificationChain();
            _eventPublisherMock.Setup(x => x.PublishMessage(It.IsAny<EventHeaderDto>(), It.IsAny<Dictionary<string, object>>(),It.IsAny<string>(),
                It.IsAny<string>(),It.IsAny<bool>())).ReturnsAsync(new PublishResultDto { Published = true });

            await _notificationHelper.ProcessNotification("test-consumer", "test-tenant", "CardOrdered");

            _notificationHelperLogger.VerifyLog(LogLevel.Information);
        }

        [Fact]
        public async Task ProcessNotification_Should_LogError_When_Publish_Fails()
        {
            SetupValidNotificationChain();
            _eventPublisherMock.Setup(x => x.PublishMessage(It.IsAny<EventHeaderDto>(), It.IsAny<Dictionary<string, object>>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(new PublishResultDto { Published = false, ErrorMessage = "Failed to publish" });

            await _notificationHelper.ProcessNotification("test-consumer", "test-tenant", "CardOrdered");

            _notificationHelperLogger.VerifyLog(LogLevel.Error);
        }

        private void SetupValidUserAndPhone()
        {
            _userClient.Setup(x => x.Post<GetPersonAndConsumerResponseDto>(It.IsAny<string>(), It.IsAny<object>()))
                .ReturnsAsync(new GetPersonAndConsumerResponseDto
                {
                    Person = new PersonDto { PersonId = 123L, FirstName = "John", LastName = "Doe", Email = "john@example.com" }
                });

            _userClient.Setup(x => x.GetId<GetAllPhoneNumbersResponseDto>(It.IsAny<string>(), It.IsAny<Dictionary<string, string>>()))
                .ReturnsAsync(new GetAllPhoneNumbersResponseDto
                {
                    PhoneNumbersList = new List<PhoneNumberDto> { new PhoneNumberDto { PhoneNumber = "1234567890" } }
                });
        }

        private void SetupValidNotificationChain()
        {
            SetupValidUserAndPhone();

            _notificationClient.Setup(x => x.GetId<NotificationEventTypeResponseDto>(It.IsAny<string>(), It.IsAny<Dictionary<string, string>>()))
                .ReturnsAsync(new NotificationEventTypeResponseDto
                {
                    NotificationEventType = new NotificationEventTypeDto { NotificationCategoryId = 1001L }
                });

            _notificationClient.Setup(x => x.GetId<NotificationCategoryResponseDto>(It.IsAny<string>(), It.IsAny<Dictionary<string, string>>()))
                .ReturnsAsync(new NotificationCategoryResponseDto
                {
                    NotificationCategory = new NotificationCategoryDto { CategoryName = "Card" }
                });
        }
    }

    public static class LoggerExtensions
    {
        public static void VerifyLog<T>(this Mock<ILogger<T>> logger, LogLevel level)
        {
            logger.Verify(x => x.Log(
                level,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, t) => true),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.AtLeastOnce);
        }
    }
}
