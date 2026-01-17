using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using SunnyBenefits.Fis.Core.Domain.Dtos.Json;
using SunnyBenefits.Fis.Core.Domain.Dtos;
using SunnyRewards.Helios.Admin.Api.Controllers;
using SunnyRewards.Helios.Admin.Infrastructure.HttpClients.Interfaces;
using SunnyRewards.Helios.Admin.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.Admin.Infrastructure.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using SunnyRewards.Helios.Admin.Infrastructure.HttpClients;

namespace SunnyRewards.Helios.Admin.UnitTest.Controllers
{
    public class ConsumerAccountControllerTests
    {
        private readonly Mock<IConsumerAccountService> _mockConsumerAccountService;
        private readonly Mock<ILogger<ConsumerAccountController>> _mockLogger;
        private readonly ConsumerAccountController _controller;
        private readonly Mock<IEventService> _eventServiceMock;
        private readonly Mock<IFisClient> _mockFisClient;
        private readonly Mock<ILogger<ConsumerAccountService>> _mockServiceLogger;
        private readonly ConsumerAccountService _consumerAccountService;

        public ConsumerAccountControllerTests()
        {
            _mockConsumerAccountService = new Mock<IConsumerAccountService>();
            _mockLogger = new Mock<ILogger<ConsumerAccountController>>();
            _mockFisClient = new Mock<IFisClient>();
            _mockServiceLogger = new Mock<ILogger<ConsumerAccountService>>();
            _eventServiceMock = new Mock<IEventService>();
            _consumerAccountService = new ConsumerAccountService(_mockServiceLogger.Object, _mockFisClient.Object);
            _controller = new ConsumerAccountController(_mockLogger.Object, _mockConsumerAccountService.Object);
        }
        [Fact]
        public async System.Threading.Tasks.Task UpdateConsumerAccountConfig_ReturnsOk_WhenUpdateIsSuccessful()
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
        public async System.Threading.Tasks.Task UpdateConsumerAccountConfig_ReturnsOk_WhenUpdateReturns_NotFound()
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
        public async System.Threading.Tasks.Task UpdateConsumerAccountConfig_ReturnsOk_WhenUpdateReturns_Throws_Exception()
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
        public async System.Threading.Tasks.Task UpdateConsumerAccountConfig_Returns_WhenUpdateIsSuccessful_InService()
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
        public async System.Threading.Tasks.Task UpdateConsumerAccountConfig_Returns_WhenUpdateThrowsException_InService()
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
        public async System.Threading.Tasks.Task UpdateConsumerAccountConfig_Returns_WhenUpdateIsSuccessful_Error_InService()
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
