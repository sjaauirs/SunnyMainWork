using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Sunny.Benefits.Bff.Api.Controllers;
using Sunny.Benefits.Bff.Core.Domain.Dtos;
using Sunny.Benefits.Bff.Infrastructure.Services.Interfaces;
using Xunit;

namespace Sunny.Benefits.Bff.UnitTest.Controllers
{
    public class FisNotificationsEnrollmentControllerTests
    {
        private readonly Mock<ILogger<FisNotificationsEnrollmentController>> _loggerMock = new();
        private readonly Mock<IFisNotificationEnrollmentService> _serviceMock = new();
        private readonly FisNotificationsEnrollmentController _controller;

        public FisNotificationsEnrollmentControllerTests()
        {
            _controller = new FisNotificationsEnrollmentController(_loggerMock.Object, _serviceMock.Object);
        }

        [Fact]
        public async Task SetNotificationsEnrollment_ReturnsOk_WhenNoError()
        {
            // Arrange
            var request = new FisSetEnrollNotificationsRequestDto { TenantCode = "T1", ConsumerCode = "C1", EnrolledNotifications = "EN" };
            var response = new FisEnrollNotificationsResponseDto { Success = true };
            _serviceMock.Setup(s => s.SetNotificationsEnrollmentAsync(request)).ReturnsAsync(response);

            // Act
            var result = await _controller.SetNotificationsEnrollment(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(response, okResult.Value);
        }

        [Fact]
        public async Task SetNotificationsEnrollment_ReturnsStatusCode_WhenError()
        {
            // Arrange
            var request = new FisSetEnrollNotificationsRequestDto { TenantCode = "T1", ConsumerCode = "C1", EnrolledNotifications = "EN" };
            var response = new FisEnrollNotificationsResponseDto { ErrorCode = StatusCodes.Status400BadRequest, ErrorMessage = "Bad" };
            _serviceMock.Setup(s => s.SetNotificationsEnrollmentAsync(request)).ReturnsAsync(response);

            // Act
            var result = await _controller.SetNotificationsEnrollment(request);

            // Assert
            var statusResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(StatusCodes.Status400BadRequest, statusResult.StatusCode);
            Assert.Equal(response, statusResult.Value);
        }

        [Fact]
        public async Task SetNotificationsEnrollment_Returns500_OnException()
        {
            // Arrange
            var request = new FisSetEnrollNotificationsRequestDto { TenantCode = "T1", ConsumerCode = "C1", EnrolledNotifications = "EN" };
            _serviceMock.Setup(s => s.SetNotificationsEnrollmentAsync(request)).ThrowsAsync(new Exception("fail"));

            // Act
            var result = await _controller.SetNotificationsEnrollment(request);

            // Assert
            var statusResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(StatusCodes.Status500InternalServerError, statusResult.StatusCode);
            var errorDto = Assert.IsType<FisGetNotificationsEnrollmentResponseDto>(statusResult.Value);
            Assert.Equal(StatusCodes.Status500InternalServerError, errorDto.ErrorCode);
        }

        [Fact]
        public async Task GetNotificationsEnrollment_ReturnsOk_WhenNoError()
        {
            // Arrange
            var request = new FisGetNotificationsEnrollmentRequestDto { TenantCode = "T1", ConsumerCode = "C1" };
            var response = new FisGetNotificationsEnrollmentResponseDto { EnrollmentUid = "UID", EnrolledNotifications = new List<FisNotificationsEnrollment>() };
            _serviceMock.Setup(s => s.GetNotificationsEnrollmentAsync(request)).ReturnsAsync(response);

            // Act
            var result = await _controller.GetNotificationsEnrollment(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(response, okResult.Value);
        }

        [Fact]
        public async Task GetNotificationsEnrollment_ReturnsStatusCode_WhenError()
        {
            // Arrange
            var request = new FisGetNotificationsEnrollmentRequestDto { TenantCode = "T1", ConsumerCode = "C1" };
            var response = new FisGetNotificationsEnrollmentResponseDto { ErrorCode = StatusCodes.Status404NotFound, EnrolledNotifications = new List<FisNotificationsEnrollment>() };
            _serviceMock.Setup(s => s.GetNotificationsEnrollmentAsync(request)).ReturnsAsync(response);

            // Act
            var result = await _controller.GetNotificationsEnrollment(request);

            // Assert
            var statusResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(StatusCodes.Status404NotFound, statusResult.StatusCode);
            Assert.Equal(response, statusResult.Value);
        }

        [Fact]
        public async Task GetNotificationsEnrollment_Returns500_OnException()
        {
            // Arrange
            var request = new FisGetNotificationsEnrollmentRequestDto { TenantCode = "T1", ConsumerCode = "C1" };
            _serviceMock.Setup(s => s.GetNotificationsEnrollmentAsync(request)).ThrowsAsync(new Exception("fail"));

            // Act
            var result = await _controller.GetNotificationsEnrollment(request);

            // Assert
            var statusResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(StatusCodes.Status500InternalServerError, statusResult.StatusCode);
            var errorDto = Assert.IsType<FisGetNotificationsEnrollmentResponseDto>(statusResult.Value);
            Assert.Equal(StatusCodes.Status500InternalServerError, errorDto.ErrorCode);
        }

        [Fact]
        public async Task GetClientConfig_ReturnsOk_WhenNoError()
        {
            // Arrange
            var request = new FisGetNotificationsEnrollmentRequestDto { TenantCode = "T1", ConsumerCode = "C1" };
            var response = new FisGetClientConfigResponseDto();
            _serviceMock.Setup(s => s.GetClientConfigAsync(request)).ReturnsAsync(response);

            // Act
            var result = await _controller.GetClientConfig(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(response, okResult.Value);
        }

        [Fact]
        public async Task GetClientConfig_ReturnsStatusCode_WhenError()
        {
            // Arrange
            var request = new FisGetNotificationsEnrollmentRequestDto { TenantCode = "T1", ConsumerCode = "C1" };
            var response = new FisGetClientConfigResponseDto { ErrorCode = StatusCodes.Status403Forbidden };
            _serviceMock.Setup(s => s.GetClientConfigAsync(request)).ReturnsAsync(response);

            // Act
            var result = await _controller.GetClientConfig(request);

            // Assert
            var statusResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status403Forbidden, statusResult.StatusCode);
            Assert.Equal(response, statusResult.Value);
        }

        [Fact]
        public async Task GetClientConfig_Returns500_OnException()
        {
            // Arrange
            var request = new FisGetNotificationsEnrollmentRequestDto { TenantCode = "T1", ConsumerCode = "C1" };
            _serviceMock.Setup(s => s.GetClientConfigAsync(request)).ThrowsAsync(new Exception("fail"));

            // Act
            var result = await _controller.GetClientConfig(request);

            // Assert
            var statusResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, statusResult.StatusCode);
            var errorDto = Assert.IsType<FisGetNotificationsEnrollmentResponseDto>(statusResult.Value);
            Assert.Equal(StatusCodes.Status500InternalServerError, errorDto.ErrorCode);
        }
    }
}
