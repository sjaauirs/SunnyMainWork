using Microsoft.Extensions.Logging;
using Moq;
using SunnyRewards.Helios.Admin.Api.Controllers;
using SunnyRewards.Helios.Admin.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.Admin.Infrastructure.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SunnyRewards.Helios.Admin.Infrastructure.HttpClients.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SunnyBenefits.Health.Core.Domains.Dtos;
using SunnyBenefits.Health.Core.Domains.Models;
using Xunit;
using SunnyRewards.Helios.Admin.UnitTest.Fixtures.MockDtos;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Admin.UnitTest.Controllers
{
    public class HealthMetricControllerUnitTest
    {
        private readonly Mock<ILogger<HealthMetricController>> _healthMetricControllerLogger;
        private readonly IHealthMetricService _healthMetricService;
        private readonly Mock<ILogger<HealthMetricService>> _healthMetricServiceLogger;
        private readonly Mock<IHealthClient> _healthClient;
        private readonly HealthMetricController _healthMetricController;
        public HealthMetricControllerUnitTest()
        {
            _healthMetricControllerLogger = new Mock<ILogger<HealthMetricController>>();
            _healthMetricServiceLogger = new Mock<ILogger<HealthMetricService>>();
            _healthClient = new Mock<IHealthClient>();
            _healthMetricService = new HealthMetricService(_healthMetricServiceLogger.Object, _healthClient.Object);
            _healthMetricController = new HealthMetricController(_healthMetricControllerLogger.Object, _healthMetricService);
        }

        [Fact]
        public async System.Threading.Tasks.Task SaveHealthMetrics_ReturnsOkResult_WhenSuccessful()
        {
            // Arrange
            var requestDto = new HealthMetricMessageRequestMockDto();
            _healthClient.Setup(x => x.Post<BaseResponseDto>(It.IsAny<string>(), It.IsAny<HealthMetricMessageRequestMockDto>()))
                .ReturnsAsync(new BaseResponseDto());

            // Act
            var result = await _healthMetricController.SaveHealthMetrics(requestDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
        }

        [Fact]
        public async System.Threading.Tasks.Task SaveHealthMetrics_ReturnsOkResult_WhenSuccessfulSavedMessage()
        {
            // Arrange
            var requestDto = new HealthMetricMessageRequestMockDto();
            _healthClient.Setup(x => x.Post<BaseResponseDto>(It.IsAny<string>(), It.IsAny<HealthMetricMessageRequestMockDto>()))
                .ReturnsAsync(new BaseResponseDto()
                {
                    ErrorCode = StatusCodes.Status404NotFound
                });

            // Act
            var result = await _healthMetricController.SaveHealthMetrics(requestDto);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(StatusCodes.Status404NotFound, objectResult.StatusCode);
        }

        [Fact]
        public async System.Threading.Tasks.Task SaveHealthMetrics_ReturnsErrorResult_WhenExceptionOccurred()
        {
            // Arrange
            var requestDto = new HealthMetricMessageRequestMockDto();
            _healthClient.Setup(x => x.Post<BaseResponseDto>(It.IsAny<string>(), It.IsAny<HealthMetricMessageRequestMockDto>()))
                .ThrowsAsync(new Exception("Testing"));

            // Act
            var result = await _healthMetricController.SaveHealthMetrics(requestDto);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
        }

        [Fact]
        public async System.Threading.Tasks.Task SaveHealthMetrics_ReturnsErrorResult_WhenServiceReturnsError()
        {
            // Arrange
            var requestDto = new HealthMetricMessageRequestMockDto();
            requestDto.HealthMetricMessages = null;

            // Act
            var result = await _healthMetricController.SaveHealthMetrics(requestDto);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(StatusCodes.Status400BadRequest, objectResult.StatusCode);
        }

        [Fact]
        public async System.Threading.Tasks.Task SaveHealthMetrics_ReturnsErrorResult_WhenExceptionThrown()
        {
            // Arrange
            var requestDto = new HealthMetricMessageRequestDto();
            var _mockHealthMetricService = new Mock<IHealthMetricService>();
            var _healthMetricController = new HealthMetricController(_healthMetricControllerLogger.Object, _mockHealthMetricService.Object);
            var exceptionMessage = "Exception occurred";
            _mockHealthMetricService.Setup(service => service.SaveHealthMetrics(requestDto)).ThrowsAsync(new Exception(exceptionMessage));

            // Act
            var result = await _healthMetricController.SaveHealthMetrics(requestDto);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
        }
    }
}
