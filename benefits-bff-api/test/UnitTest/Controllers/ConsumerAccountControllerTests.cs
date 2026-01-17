using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Sunny.Benefits.Bff.Api.Controllers;
using Sunny.Benefits.Bff.Infrastructure.Services.Interfaces;
using SunnyBenefits.Fis.Core.Domain.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SunnyBenefits.Fis.Core.Domain.Dtos.Json;
using Xunit;
using Sunny.Benefits.Bff.Infrastructure.HttpClients.Interfaces;
using Sunny.Benefits.Bff.Infrastructure.Services;
using Sunny.Benefits.Bff.Infrastructure.Helpers.Interface;
using Sunny.Benefits.Bff.Infrastructure.Helpers;

namespace Sunny.Benefits.Bff.UnitTest.Controllers
{
    public class ConsumerAccountControllerTests
    {
        private readonly Mock<IConsumerAccountService> _mockConsumerAccountService;
        private readonly Mock<ILogger<ConsumerAccountController>> _mockLogger;
        private readonly ConsumerAccountController _controller;
        private readonly Mock<IEventService> _eventServiceMock;
        private readonly Mock<IFisClient> _mockFisClient;
        private readonly Mock<ILogger<ConsumerAccountService>> _mockServiceLogger;
        private readonly Mock<INotificationHelper> _mockNotificationHelper;
        private readonly ConsumerAccountService _consumerAccountService;

        public ConsumerAccountControllerTests()
        {
            _mockConsumerAccountService = new Mock<IConsumerAccountService>();
            _mockLogger = new Mock<ILogger<ConsumerAccountController>>();
            _mockFisClient = new Mock<IFisClient>();
            _mockServiceLogger = new Mock<ILogger<ConsumerAccountService>>();
            _eventServiceMock = new Mock<IEventService>();
            _mockNotificationHelper = new Mock<INotificationHelper>();
            _consumerAccountService = new ConsumerAccountService(_mockServiceLogger.Object, _mockFisClient.Object, _eventServiceMock.Object, _mockNotificationHelper.Object);
            _controller = new ConsumerAccountController(_mockLogger.Object, _mockConsumerAccountService.Object);
        }
        [Fact]
        public async Task UpdateConsumerAccountConfig_ReturnsOk_WhenUpdateIsSuccessful()
        {
            // Arrange
            ConsumerAccountUpdateRequestDto request = RequestUpdateConsumerAccount();

            var response = new ConsumerAccountUpdateResponseDto();
            _mockConsumerAccountService
                .Setup(service => service.UpdateConsumerAccountConfig(It.IsAny<ConsumerAccountUpdateRequestDto>()))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.UpdateConsumerAccountConfig(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
            Assert.Equal(response, okResult.Value);
        }
        [Fact]
        public async Task UpdateConsumerAccountConfig_ReturnsOk_WhenUpdateReturns_NotFound()
        {
            // Arrange
            ConsumerAccountUpdateRequestDto request = RequestUpdateConsumerAccount();

            _mockConsumerAccountService
                .Setup(service => service.UpdateConsumerAccountConfig(It.IsAny<ConsumerAccountUpdateRequestDto>()))
                .ReturnsAsync(new ConsumerAccountUpdateResponseDto { ErrorCode = StatusCodes.Status404NotFound });

            // Act
            var result = await _controller.UpdateConsumerAccountConfig(request);

            // Assert
            var okResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status404NotFound, okResult.StatusCode);
        }
        [Fact]
        public async Task UpdateConsumerAccountConfig_ReturnsOk_WhenUpdateReturns_Throws_Exception()
        {
            // Arrange
            ConsumerAccountUpdateRequestDto request = RequestUpdateConsumerAccount();

            _mockConsumerAccountService
                .Setup(service => service.UpdateConsumerAccountConfig(It.IsAny<ConsumerAccountUpdateRequestDto>()))
                .ThrowsAsync(new Exception("simulated Exception"));

            // Act
            var result = await _controller.UpdateConsumerAccountConfig(request);

            // Assert
            var okResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, okResult.StatusCode);
        }
        [Fact]
        public async Task UpdateConsumerAccountConfig_Returns_WhenUpdateIsSuccessful_InService()
        {
            // Arrange
            ConsumerAccountUpdateRequestDto request = RequestUpdateConsumerAccount();

            var response = new ConsumerAccountUpdateResponseDto();
            _mockFisClient.Setup(client => client.Patch<ConsumerAccountUpdateResponseDto>("consumer-account", request))
                           .ReturnsAsync(response);

            // Act
            var result = await _consumerAccountService.UpdateConsumerAccountConfig(request);

            // Assert
            Assert.NotNull(result);
        }
        [Fact]
        public async Task UpdateConsumerAccountConfig_Returns_WhenUpdateThrowsException_InService()
        {
            // Arrange
            ConsumerAccountUpdateRequestDto request = RequestUpdateConsumerAccount();

            var response = new ConsumerAccountUpdateResponseDto();
            _mockFisClient.Setup(client => client.Patch<ConsumerAccountUpdateResponseDto>("consumer-account", request))
                           .ThrowsAsync(new Exception("simulated Exception"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _consumerAccountService.UpdateConsumerAccountConfig(request));
        }
        [Fact]
        public async Task UpdateConsumerAccountConfig_Returns_WhenUpdateIsSuccessful_Error_InService()
        {
            // Arrange
            ConsumerAccountUpdateRequestDto request = RequestUpdateConsumerAccount();

            _mockFisClient.Setup(client => client.Patch<ConsumerAccountUpdateResponseDto>("consumer-account", request))
                           .ReturnsAsync(new ConsumerAccountUpdateResponseDto { ErrorCode = StatusCodes.Status404NotFound });

            // Act
            var result = await _consumerAccountService.UpdateConsumerAccountConfig(request);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task UpdateConsumerAccountCardIssue_ReturnsOk_WhenUpdateIsSuccessful()
        {
            // Arrange
            var request = new UpdateCardIssueRequestDto
            {
                TenantCode = "Tenant1",
                ConsumerCode = "Consumer1",
                TargetCardIssueStatus = "test"
            };
            var response = new ConsumerAccountResponseDto();

            _mockConsumerAccountService
                .Setup(service => service.UpdateConsumerAccountCardIssue(It.IsAny<UpdateCardIssueRequestDto>()))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.UpdateConsumerAccountCardIssue(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
            Assert.Equal(response, okResult.Value);
        }

        [Fact]
        public async Task UpdateConsumerAccountCardIssue_ReturnsNotFound_WhenServiceReturnsErrorCode()
        {
            // Arrange
            var request = new UpdateCardIssueRequestDto
            {
                TenantCode = "Tenant1",
                ConsumerCode = "Consumer1",
                TargetCardIssueStatus = "test"
            };

            _mockConsumerAccountService
                .Setup(service => service.UpdateConsumerAccountCardIssue(It.IsAny<UpdateCardIssueRequestDto>()))
                .ReturnsAsync(new ConsumerAccountResponseDto { ErrorCode = StatusCodes.Status404NotFound });

            // Act
            var result = await _controller.UpdateConsumerAccountCardIssue(request);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(StatusCodes.Status404NotFound, objectResult.StatusCode);
        }

        [Fact]
        public async Task UpdateConsumerAccountCardIssue_ReturnsInternalServerError_OnException()
        {
            // Arrange
            var request = new UpdateCardIssueRequestDto
            {
                TenantCode = "Tenant1",
                ConsumerCode = "Consumer1",
                TargetCardIssueStatus = "test"
            };

            _mockConsumerAccountService
                .Setup(service => service.UpdateConsumerAccountCardIssue(It.IsAny<UpdateCardIssueRequestDto>()))
                .ThrowsAsync(new Exception("Unexpected Exception"));

            // Act
            var result = await _controller.UpdateConsumerAccountCardIssue(request);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
        }

        [Fact]
        public async Task UpdateConsumerAccountCardIssue_Returns_WhenUpdateIsSuccessful_InService()
        {
            // Arrange
            var request = new UpdateCardIssueRequestDto
            {
                TenantCode = "Tenant1",
                ConsumerCode = "Consumer1",
                TargetCardIssueStatus = "ELIGIBLE_FOR_FIS_BATCH_PROCESS"
            };
            var response = new ConsumerAccountResponseDto();

            _mockFisClient
                .Setup(client => client.Put<ConsumerAccountResponseDto>("update-card-issue-status", request))
                .ReturnsAsync(response);
            _mockNotificationHelper.Setup(helper => helper.ProcessNotification(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);
            // Act
            var result = await _consumerAccountService.UpdateConsumerAccountCardIssue(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(response, result);
        }

        [Fact]
        public async Task UpdateConsumerAccountCardIssue_ReturnsErrorDto_WhenPutReturnsError_InService()
        {
            // Arrange
            var request = new UpdateCardIssueRequestDto
            {
                TenantCode = "Tenant1",
                ConsumerCode = "Consumer1",
                TargetCardIssueStatus = "test"
            };
            var errorResponse = new ConsumerAccountResponseDto
            {
                ErrorCode = StatusCodes.Status404NotFound,
                ErrorMessage = "Not found"
            };

            _mockFisClient
                .Setup(client => client.Put<ConsumerAccountResponseDto>("update-card-issue-status", request))
                .ReturnsAsync(errorResponse);

            // Act
            var result = await _consumerAccountService.UpdateConsumerAccountCardIssue(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(StatusCodes.Status404NotFound, result.ErrorCode);
            Assert.Equal("Not found", result.ErrorMessage);
        }

        [Fact]
        public async Task UpdateConsumerAccountCardIssue_ThrowsException_WhenPutFails_InService()
        {
            // Arrange
            var request = new UpdateCardIssueRequestDto
            {
                TenantCode = "Tenant1",
                ConsumerCode = "Consumer1",
                TargetCardIssueStatus = "test"
            };

            _mockFisClient
                .Setup(client => client.Put<ConsumerAccountResponseDto>("update-card-issue-status", request))
                .ThrowsAsync(new Exception("Unexpected Exception"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _consumerAccountService.UpdateConsumerAccountCardIssue(request));
        }


        private static ConsumerAccountUpdateRequestDto RequestUpdateConsumerAccount()
        {
            return new ConsumerAccountUpdateRequestDto
            {
                TenantCode = "Tenant1",
                ConsumerCode = "Consumer1",
                ConsumerAccountConfig = new ConsumerAccountConfig
                {
                    PurseConfig = new ConsumerAccountPurseConfig
                    {
                        Purses = new List<ConsumerAccountPurse>
                    {
                        new ConsumerAccountPurse
                        {
                            PurseLabel = "OTC",
                            Enabled = true
                        }
                    }
                    }
                }
            };
        }
    }
}
