using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Sunny.Benefits.Bff.Infrastructure.HttpClients.Interfaces;
using Sunny.Benefits.Bff.Infrastructure.Services;
using SunnyRewards.Helios.Admin.Core.Domain.Dtos;
using SunnyRewards.Helios.Bff.Api.Controllers;
using Microsoft.AspNetCore.Http;
using Sunny.Benefits.Bff.UnitTest.HttpClients;
using SunnyBenefits.Fis.Core.Domain.Dtos;
using Sunny.Benefits.Bff.Infrastructure.HttpClients;
using Sunny.Benefits.Bff.Core.Domain.Constants;

namespace Sunny.Benefits.Bff.UnitTest.Controllers
{
    public class EventControllerTests
    {
        private readonly Mock<IAdminClient> _adminClientMock;
        private readonly Mock<IFisClient> _fisClientMock;
        private readonly Mock<ILogger<EventService>> _eventServiceLoggerMock;
        private readonly Mock<ILogger<EventController>> _eventControllerLoggerMock;
        private readonly EventService _eventService;
        private readonly EventController _eventController;

        public EventControllerTests()
        {
            _adminClientMock = new Mock<IAdminClient>();
            _eventServiceLoggerMock = new Mock<ILogger<EventService>>();
            _eventControllerLoggerMock = new Mock<ILogger<EventController>>();
            _fisClientMock = new FisClientMock();
            _eventService = new EventService(_eventServiceLoggerMock.Object, _adminClientMock.Object, _fisClientMock.Object);
            _eventController = new EventController(_eventControllerLoggerMock.Object, _eventService);
        }

        [Fact]
        public async Task PostEvents_ShouldReturnOk_WhenEventIsPostedSuccessfully()
        {
            // Arrange
            var request = new PostEventRequestDto
            {
                EventType = "TEST",
                EventSubtype = "SUBTYPE",
                EventSource = "SOURCE",
                TenantCode = "TENANT",
                ConsumerCode = "CONSUMER",
                EventData = null
            };

            var response = new PostEventResponseDto
            {
                ErrorCode = null,
                ErrorMessage = null
            };

            _adminClientMock
                .Setup(client => client.Post<PostEventResponseDto>("post-event", It.IsAny<PostEventRequestDto>()))
                .ReturnsAsync(response);

            // Act
            var result = await _eventController.PostEvents(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(response, okResult.Value);
        }

        [Fact]
        public async Task PostEvents_ShouldReturnErrorResponse_WhenAdminClientReturnsErrorCode()
        {
            // Arrange
            var request = new PostEventRequestDto
            {
                EventType = "TEST",
                EventSubtype = "SUBTYPE",
                EventSource = "SOURCE",
                TenantCode = "TENANT",
                ConsumerCode = "CONSUMER",
                EventData = null
            };

            var response = new PostEventResponseDto
            {
                ErrorCode = StatusCodes.Status500InternalServerError,
                ErrorMessage = null
            };

            _adminClientMock
                .Setup(client => client.Post<PostEventResponseDto>("post-event", It.IsAny<PostEventRequestDto>()))
                .ReturnsAsync(response);

            // Act
            var result = await _eventController.PostEvents(request);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, objectResult.StatusCode);
            Assert.Equal(response, objectResult.Value);

        }

        [Fact]
        public async Task PostEvents_ShouldReturnErrorResponse_WhenExceptionOccurs()
        {
            // Arrange
            var request = new PostEventRequestDto
            {
                EventType = "TEST",
                EventSubtype = "SUBTYPE",
                EventSource = "SOURCE",
                TenantCode = "TENANT",
                ConsumerCode = "CONSUMER",
                EventData = null
            };

            var exception = new Exception("Unexpected Error");
            _adminClientMock
                .Setup(client => client.Post<PostEventResponseDto>("post-event", It.IsAny<PostEventRequestDto>()))
                .ThrowsAsync(exception);

            // Act
            var result = await _eventController.PostEvents(request);

            // Assert
            var response = Assert.IsType<PostEventResponseDto>(result.Value);
            Assert.Equal("Unexpected Error", response.ErrorMessage);
            Assert.Null(response.ErrorCode);
        }

        [Fact]
        public async Task CreatePickAPurseEvent_ShouldReturnNotFound_WhenWalletTypesResponseIsNull()
        {
            // Arrange
            var consumerAccountDto = new ConsumerAccountDto { ConsumerCode = "test", TenantCode = "test" };
            _fisClientMock.Setup(client => client.Post<ConsumerBenefitsWalletTypesResponseDto>(WalletConstants.GetConsumerBenefitWalletTypesAPIUrl, It.IsAny<ConsumerBenefitsWalletTypesRequestDto>()))
                 .ReturnsAsync(new ConsumerBenefitsWalletTypesResponseDto()
                 {
                     ErrorCode = StatusCodes.Status404NotFound

                 });
            // Act
            var result = await _eventService.CreatePickAPurseEvent(consumerAccountDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(StatusCodes.Status404NotFound, result.ErrorCode);
        }

        [Fact]
        public async Task CreatePickAPurseEvent_ShouldReturnInternalServerError_WhenExceptionIsThrown()
        {
            // Arrange
            var consumerAccountDto = new ConsumerAccountDto { ConsumerCode = "test", TenantCode = "test" };
            _fisClientMock.Setup(client => client.Post<ConsumerBenefitsWalletTypesResponseDto>(It.IsAny<string>(), It.IsAny<object>()))
                          .ThrowsAsync(new Exception("Test exception"));

            // Act
            var result = await _eventService.CreatePickAPurseEvent(consumerAccountDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, result.ErrorCode);
        }

        [Fact]
        public async Task CreatePickAPurseEvent_ShouldReturnSuccess_WhenWalletTypesResponseIsValid()
        {
            // Arrange
            var consumerAccountDto = new ConsumerAccountDto { ConsumerCode = "test", TenantCode = "test" };
            var walletTypesResponse = new ConsumerBenefitsWalletTypesResponseDto
            {
                BenefitsWalletTypes = new List<ConsumerBenefitWalletTypeDto>
            {
                new ConsumerBenefitWalletTypeDto { PurseLabel = "Label1" },
                new ConsumerBenefitWalletTypeDto { PurseLabel = "Label2" }
            }
            };
            _fisClientMock.Setup(client => client.Post<ConsumerBenefitsWalletTypesResponseDto>(It.IsAny<string>(), It.IsAny<object>()))
                          .ReturnsAsync(walletTypesResponse);
            _adminClientMock.Setup(client => client.Post<PostEventResponseDto>(It.IsAny<string>(), It.IsAny<object>()))
                          .ReturnsAsync(new PostEventResponseDto());

            // Act
            var result = await _eventService.CreatePickAPurseEvent(consumerAccountDto);

            // Assert
            Assert.NotNull(result);
            Assert.Null(result.ErrorCode);
            Assert.Null(result.ErrorMessage);
        }
    }
}
