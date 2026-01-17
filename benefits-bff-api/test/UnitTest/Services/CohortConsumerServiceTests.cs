using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Sunny.Benefits.Bff.Infrastructure.HttpClients.Interfaces;
using Sunny.Benefits.Bff.Infrastructure.Services;
using SunnyRewards.Helios.Cohort.Core.Domain.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Sunny.Benefits.Bff.UnitTest.Services
{
    public class CohortConsumerServiceTests
    {
        private readonly Mock<ILogger<CohortConsumerService>> _mockLogger;
        private readonly Mock<ICohortClient> _mockCohortClient;
        private readonly CohortConsumerService _service;

        public CohortConsumerServiceTests()
        {
            _mockLogger = new Mock<ILogger<CohortConsumerService>>();
            _mockCohortClient = new Mock<ICohortClient>();
            _service = new CohortConsumerService(_mockLogger.Object, _mockCohortClient.Object);
        }

        [Fact]
        public async Task GetConsumerCohorts_ShouldReturnCohortConsumerResponseDto_WhenClientReturnsSuccess()
        {
            // Arrange
            var requestDto = new GetConsumerByCohortsNameRequestDto
            {
                TenantCode = "test-tenant",
                ConsumerCode = "consumer-123"
            };

            var expectedResponse = new CohortConsumerResponseDto
            {
                ConsumerCohorts = new List<CohortConsumersDto>()
            };

            _mockCohortClient
                .Setup(client => client.Post<CohortConsumerResponseDto>(It.IsAny<string>(), requestDto))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _service.GetConsumerCohorts(requestDto);

            // Assert
            Assert.NotNull(result);
           

            _mockLogger.VerifyLog(LogLevel.Information, Times.AtLeastOnce());
        }

        [Fact]
        public async Task GetConsumerCohorts_ShouldReturnErrorResponse_WhenExceptionIsThrown()
        {
            // Arrange
            var requestDto = new GetConsumerByCohortsNameRequestDto
            {
                TenantCode = "test-tenant",
                ConsumerCode = "consumer-123"
            };

            _mockCohortClient
                .Setup(client => client.Post<CohortConsumerResponseDto>(It.IsAny<string>(), requestDto))
                .ThrowsAsync(new Exception("Something went wrong"));

            // Act
            var result = await _service.GetConsumerCohorts(requestDto);

            // Assert
            Assert.NotNull(result);


            _mockLogger.VerifyLog(LogLevel.Error, Times.Once());
        }
    }

    // Logger Extension for verifying log levels
    public static class LoggerExtensions
    {
        public static void VerifyLog<T>(this Mock<ILogger<T>> logger, LogLevel level, Times times)
        {
            logger.Verify(
                x => x.Log(
                    level,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                times);
        }
    }
}
