using Microsoft.Extensions.Logging;
using Moq;
using Sunny.Benefits.Bff.Infrastructure.Helpers.Interface;
using Sunny.Benefits.Bff.Infrastructure.HttpClients.Interfaces;
using Sunny.Benefits.Bff.Infrastructure.Services;
using SunnyBenefits.Fis.Core.Domain.Dtos;
using Xunit;

namespace Sunny.Benefits.Bff.UnitTest.Services
{
    public class ConsumerAccountServiceTests
    {
        private readonly Mock<ILogger<ConsumerAccountService>> _loggerMock = new();
        private readonly Mock<IFisClient> _fisClientMock = new();
        private readonly Mock<IEventService> _eventServiceMock = new();
        private readonly Mock<INotificationHelper> _notificationHelperMock = new();
        private readonly ConsumerAccountService _service;

        public ConsumerAccountServiceTests()
        {
            _service = new ConsumerAccountService(
                _loggerMock.Object,
                _fisClientMock.Object,
                _eventServiceMock.Object,
                _notificationHelperMock.Object
            );
        }

        [Fact]
        public async Task UpdateConsumerAccountConfig_ReturnsResponse_WhenNoError()
        {
            // Arrange
            var request = new ConsumerAccountUpdateRequestDto { TenantCode = "T1", ConsumerCode = "C1" };
            var response = new ConsumerAccountUpdateResponseDto();
            _fisClientMock.Setup(f => f.Patch<ConsumerAccountUpdateResponseDto>("consumer-account", request))
                .ReturnsAsync(response);

            // Act
            var result = await _service.UpdateConsumerAccountConfig(request);

            // Assert
            Assert.Equal(response, result);
        }

        [Fact]
        public async Task UpdateConsumerAccountConfig_ReturnsError_WhenErrorCodePresent()
        {
            // Arrange
            var request = new ConsumerAccountUpdateRequestDto { TenantCode = "T1", ConsumerCode = "C1" };
            var response = new ConsumerAccountUpdateResponseDto { ErrorCode = 123, ErrorMessage = "fail" };
            _fisClientMock.Setup(f => f.Patch<ConsumerAccountUpdateResponseDto>("consumer-account", request))
                .ReturnsAsync(response);

            // Act
            var result = await _service.UpdateConsumerAccountConfig(request);

            // Assert
            Assert.Equal(123, result.ErrorCode);
            Assert.Equal("fail", result.ErrorMessage);
        }

        [Fact]
        public async Task UpdateConsumerAccountConfig_Throws_OnException()
        {
            // Arrange
            var request = new ConsumerAccountUpdateRequestDto { TenantCode = "T1", ConsumerCode = "C1" };
            _fisClientMock.Setup(f => f.Patch<ConsumerAccountUpdateResponseDto>("consumer-account", request))
                .ThrowsAsync(new Exception("fail"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _service.UpdateConsumerAccountConfig(request));
        }

        [Fact]
        public async Task UpdateConsumerAccountCardIssue_ReturnsResponse_WhenNoError()
        {
            // Arrange
            var request = new UpdateCardIssueRequestDto { TenantCode = "T1", ConsumerCode = "C1" };
            var response = new ConsumerAccountResponseDto();
            _fisClientMock.Setup(f => f.Put<ConsumerAccountResponseDto>("update-card-issue-status", request))
                .ReturnsAsync(response);

            // Act
            var result = await _service.UpdateConsumerAccountCardIssue(request);

            // Assert
            Assert.Equal(response, result);
        }

        [Fact]
        public async Task UpdateConsumerAccountCardIssue_ReturnsError_WhenErrorCodePresent()
        {
            // Arrange
            var request = new UpdateCardIssueRequestDto { TenantCode = "T1", ConsumerCode = "C1" };
            var response = new ConsumerAccountResponseDto { ErrorCode = 404, ErrorMessage = "not found" };
            _fisClientMock.Setup(f => f.Put<ConsumerAccountResponseDto>("update-card-issue-status", request))
                .ReturnsAsync(response);

            // Act
            var result = await _service.UpdateConsumerAccountCardIssue(request);

            // Assert
            Assert.Equal(404, result.ErrorCode);
            Assert.Equal("not found", result.ErrorMessage);
        }

        [Fact]
        public async Task UpdateConsumerAccountCardIssue_Throws_OnException()
        {
            // Arrange
            var request = new UpdateCardIssueRequestDto { TenantCode = "T1", ConsumerCode = "C1" };
            _fisClientMock.Setup(f => f.Put<ConsumerAccountResponseDto>("update-card-issue-status", request))
                .ThrowsAsync(new Exception("fail"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _service.UpdateConsumerAccountCardIssue(request));
        }

        [Fact]
        public async Task GetConsumerAccount_ReturnsResponse_WhenNoError()
        {
            // Arrange
            var request = new GetConsumerAccountRequestDto { TenantCode = "T1", ConsumerCode = "C1" };
            var response = new GetConsumerAccountResponseDto();
            _fisClientMock.Setup(f => f.Post<GetConsumerAccountResponseDto>("get-consumer-account", request))
                .ReturnsAsync(response);

            // Act
            var result = await _service.GetConsumerAccount(request);

            // Assert
            Assert.Equal(response, result);
        }

        [Fact]
        public async Task GetConsumerAccount_ReturnsError_WhenErrorCodePresent()
        {
            // Arrange
            var request = new GetConsumerAccountRequestDto { TenantCode = "T1", ConsumerCode = "C1" };
            var response = new GetConsumerAccountResponseDto { ErrorCode = 500, ErrorMessage = "error" };
            _fisClientMock.Setup(f => f.Post<GetConsumerAccountResponseDto>("get-consumer-account", request))
                .ReturnsAsync(response);

            // Act
            var result = await _service.GetConsumerAccount(request);

            // Assert
            Assert.Equal(500, result.ErrorCode);
            Assert.Equal("error", result.ErrorMessage);
        }

        [Fact]
        public async Task GetConsumerAccount_Throws_OnException()
        {
            // Arrange
            var request = new GetConsumerAccountRequestDto { TenantCode = "T1", ConsumerCode = "C1" };
            _fisClientMock.Setup(f => f.Post<GetConsumerAccountResponseDto>("get-consumer-account", request))
                .ThrowsAsync(new Exception("fail"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _service.GetConsumerAccount(request));
        }
    }
}
