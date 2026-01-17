using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.User.Api.Controllers;
using SunnyRewards.Helios.User.Core.Domain.Constant;
using SunnyRewards.Helios.User.Core.Domain.Dtos;
using SunnyRewards.Helios.User.Core.Domain.Models;
using SunnyRewards.Helios.User.Infrastructure.Repositories.Interfaces;
using SunnyRewards.Helios.User.Infrastructure.Services;
using SunnyRewards.Helios.User.Infrastructure.Services.Interfaces;
using System.Linq.Expressions;
using Xunit;

namespace SunnyRewards.Helios.User.UnitTest.Controllers
{
    public class ConsumerFlowProgressControllerUnitTests
    {
        private readonly Mock<ILogger<ConsumerFlowProgressController>> _logger;
        private readonly Mock<ILogger<ConsumerFlowProgressService>> _serviceLogger;
        private readonly Mock<IMapper> _mapper;
        private readonly Mock<IConsumerFlowProgressRepo> _consumerFlowProgressRepo;
        private readonly Mock<IConsumerOnboardingProgressHistoryRepo> _consumerOnboardingProgressHistoryRepo;
        private readonly Mock<NHibernate.ISession> _session;
        private readonly IConsumerFlowProgressService _consumerFlowProgressService;
        private ConsumerFlowProgressController _controller;
        private readonly Mock<IConsumerFlowProgressService> _serviceMock = new Mock<IConsumerFlowProgressService>();

        public ConsumerFlowProgressControllerUnitTests()
        {
            _consumerFlowProgressRepo = new Mock<IConsumerFlowProgressRepo>();
            _consumerOnboardingProgressHistoryRepo = new Mock<IConsumerOnboardingProgressHistoryRepo>();
            _logger = new Mock<ILogger<ConsumerFlowProgressController>>();
            _serviceLogger = new Mock<ILogger<ConsumerFlowProgressService>>();
            _mapper = new Mock<IMapper>();
            _session = new Mock<NHibernate.ISession> { CallBase = true };
            _consumerFlowProgressService = new ConsumerFlowProgressService(_serviceLogger.Object,
                _mapper.Object,_consumerFlowProgressRepo.Object,_consumerOnboardingProgressHistoryRepo.Object,_session.Object);
            _controller = new ConsumerFlowProgressController(_logger.Object, _consumerFlowProgressService);

        }

        [Fact]
        public async Task GetConsumerFlowProgressAsync_ReturnsOk_WhenRecordExists()
        {
            // Arrange
            var request = new ConsumerFlowProgressRequestDto
            {
                ConsumerCode = "cmr-123",
                TenantCode = "ten-123",
                CohortCodes = new List<string> { "coh-123" }
            };
            _consumerFlowProgressRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<ConsumerFlowProgressModel, bool>>>(), false))
                .ReturnsAsync(new ConsumerFlowProgressModel() { Pk = 1, ConsumerCode = "cmr-123" });

            // Act
            var result = await _controller.GetConsumerFlowProgressAsync(request);

            // Assert
            
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
            var response = Assert.IsType<ConsumerFlowProgressResponseDto>(okResult.Value);
            Assert.NotNull(response);
        }

        [Fact]
        public async Task GetConsumerFlowProgressAsync_ReturnsNotStarted_WhenNoRecord()
        {
            // Arrange
            var request = new ConsumerFlowProgressRequestDto
            {
                ConsumerCode = "non-existent",
                TenantCode = "ten-x",
                CohortCodes = new List<string> { "coh-x" }
            };

            // Act
            var result = await _controller.GetConsumerFlowProgressAsync(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
            var response = Assert.IsType<ConsumerFlowProgressResponseDto>(okResult.Value);
            Assert.Equal(Constant.NotStarted, response.ConsumerFlowProgress.Status);
        }

        [Fact]
        public async Task GetConsumerFlowProgressAsync_ReturnsInternalServerError_OnException()
        {
            var request = new ConsumerFlowProgressRequestDto
            {
                ConsumerCode = "cmr-123",
                TenantCode = "ten-123",
                CohortCodes = new List<string> { "coh-123" }
            };
            _consumerFlowProgressRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<ConsumerFlowProgressModel, bool>>>(), false))
                .ThrowsAsync(new Exception("testing"));

            // Act
            var result = await _controller.GetConsumerFlowProgressAsync(request);

            // Assert
            var statusResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, statusResult.StatusCode);
        }

        [Fact]
        public async Task UpdateFlowStatus_ReturnsOk_WhenNoError()
        {
            // Arrange
            var request = new UpdateFlowStatusRequestDto
            {
                ConsumerCode = "C123",
                TenantCode = "T456",
                CohortCode = "CH789",
                FlowId = 1,
                FromFlowStepId = 2,
                Status = "Completed",
                VersionId = 1,
                ToFlowStepId = 3
            };
            var response = new ConsumerFlowProgressResponseDto
            {
                ConsumerFlowProgress = new ConsumerFlowProgressDto { ConsumerCode = "C123" }
            };
            _serviceMock.Setup(s => s.UpdateOnboardingStatusFlow(It.IsAny<UpdateFlowStatusRequestDto>())).ReturnsAsync(response);
            _controller = new ConsumerFlowProgressController(_logger.Object, _serviceMock.Object);

            // Act
            var result = await _controller.UpdateFlowStatus(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(response, okResult.Value);
        }

        [Fact]
        public async Task UpdateFlowStatus_ReturnsError_WhenErrorCodePresent()
        {
            // Arrange
            var request = new UpdateFlowStatusRequestDto
            {
                ConsumerCode = "C123",
                TenantCode = "T456",
                CohortCode = "CH789",
                FlowId = 1,
                FromFlowStepId = 2,
                Status = "Completed",
                VersionId = 1,
                ToFlowStepId = 3
            };
            var response = new ConsumerFlowProgressResponseDto
            {
                ErrorCode = StatusCodes.Status400BadRequest,
                ErrorMessage = "Bad request"
            };
            _serviceMock.Setup(s => s.UpdateOnboardingStatusFlow(request)).ReturnsAsync(response);
            _controller = new ConsumerFlowProgressController(_logger.Object, _serviceMock.Object);

            // Act
            var result = await _controller.UpdateFlowStatus(request);

            // Assert
            var statusResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(StatusCodes.Status400BadRequest, statusResult.StatusCode);
            Assert.Equal(response, statusResult.Value);
        }

        [Fact]
        public async Task UpdateFlowStatus_ReturnsInternalServerError_OnException()
        {
            // Arrange
            var request = new UpdateFlowStatusRequestDto
            {
                ConsumerCode = "C123",
                TenantCode = "T456",
                CohortCode = "CH789",
                FlowId = 1,
                FromFlowStepId = 2,
                Status = "Completed",
                VersionId = 1,
                ToFlowStepId = 3
            };
            _serviceMock.Setup(s => s.UpdateOnboardingStatusFlow(request)).ThrowsAsync(new Exception("Unexpected error"));
            _controller = new ConsumerFlowProgressController(_logger.Object, _serviceMock.Object);

            // Act
            var result = await _controller.UpdateFlowStatus(request);

            // Assert
            var statusResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(StatusCodes.Status500InternalServerError, statusResult.StatusCode);
            var errorResponse = Assert.IsType<BaseResponseDto>(statusResult.Value);
            Assert.Equal(StatusCodes.Status500InternalServerError, errorResponse.ErrorCode);
            Assert.Equal("Unexpected error", errorResponse.ErrorMessage);
        }
    }
}