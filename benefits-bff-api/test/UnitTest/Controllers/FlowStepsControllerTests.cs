using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Sunny.Benefits.Bff.Api.Controllers;
using Sunny.Benefits.Bff.Infrastructure.Helpers.Interface;
using Sunny.Benefits.Bff.Infrastructure.HttpClients.Interfaces;
using Sunny.Benefits.Bff.Infrastructure.Services;
using Sunny.Benefits.Bff.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.Cohort.Core.Domain.Dtos;
using SunnyRewards.Helios.Tenant.Core.Domain.Dtos;
using Xunit;

namespace Sunny.Benefits.Bff.UnitTest.Controllers
{
    public class FlowStepsControllerTests
    {
        private readonly Mock<ICohortConsumerService> _cohortConsumerService;
        private readonly FlowStepService _flowStepService;
        private readonly FlowStepController _controller;
        private readonly Mock<ITenantClient> _tenantClient;
        private readonly Mock<ICommonHelper> _commonHelper;
        private readonly Mock<IFlowStepProcessor> _flowStepProcessor;

        public FlowStepsControllerTests()
        {
            _cohortConsumerService = new Mock<ICohortConsumerService>();
            var loggerService = new Mock<ILogger<FlowStepService>>();
            var loggerController = new Mock<ILogger<FlowStepController>>();
            var httpContext = new Mock<IHttpContextAccessor>();
            _flowStepProcessor = new Mock<IFlowStepProcessor>();
            _tenantClient = new Mock<ITenantClient>();
            _commonHelper = new Mock<ICommonHelper>();
            _flowStepService = new FlowStepService(loggerService.Object,_tenantClient.Object ,
                _cohortConsumerService.Object,httpContext.Object,_commonHelper.Object, _flowStepProcessor.Object);
            _controller = new FlowStepController(loggerController.Object, _flowStepService);
        }

        [Fact]
        public async Task GetFlowSteps_ReturnsBadRequest_TenantClient_ThrowsException()
        {
            // Arrange
            var request = new FlowRequestDto
            {
                ConsumerCode = null,
                TenantCode = "ten-001"
            };
            _tenantClient.Setup(x => x.Post<FlowResponseDto>(It.IsAny<string>(), It.IsAny<FlowRequestDto>()))
                .ThrowsAsync(new Exception("testing"));

            // Act
            var result = await _controller.GetFlowSteps(request);

            // Assert
            var actionResult = Assert.IsType<ActionResult<FlowResponseDto>>(result);
            var badRequest = Assert.IsType<ObjectResult>(actionResult.Result);
            var response = Assert.IsType<FlowResponseDto>(badRequest.Value);

            Assert.Equal(StatusCodes.Status500InternalServerError, response.ErrorCode);
        }

        [Fact]
        public async Task GetFlowSteps_ReturnsOk_WhenValidRequest()
        {
            // Arrange
            var request = new FlowRequestDto
            {
                ConsumerCode = "cmr-123",
                TenantCode = "ten-001",
                CohortCodes = new List<string> { "coh-123" }
            };

            _tenantClient.Setup(x => x.Post<FlowResponseDto>(It.IsAny<string>(), It.IsAny<FlowRequestDto>())).ReturnsAsync(new FlowResponseDto());

            // Act
            var result = await _controller.GetFlowSteps(request);

            // Assert
            var actionResult = Assert.IsType<ActionResult<FlowResponseDto>>(result);
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            var response = Assert.IsType<FlowResponseDto>(okResult.Value);

            Assert.NotNull(response);
            Assert.Null(response.ErrorCode); // assuming 0 means success
        }
        [Fact]
        public async Task GetFlowSteps_ReturnsErrorResponse()
        {
            // Arrange
            var request = new FlowRequestDto
            {
                ConsumerCode = "cmr-123",
                TenantCode = "ten-001",
                CohortCodes = new List<string> { "coh-123" }
            };

            _tenantClient.Setup(x => x.Post<FlowResponseDto>(It.IsAny<string>(), It.IsAny<FlowRequestDto>()))
                .ReturnsAsync(new FlowResponseDto(){ ErrorCode = StatusCodes.Status404NotFound});

            // Act
            var result = await _controller.GetFlowSteps(request);

            // Assert
            var actionResult = Assert.IsType<ActionResult<FlowResponseDto>>(result);
            var okResult = Assert.IsType<NotFoundObjectResult>(actionResult.Result);
            var response = Assert.IsType<FlowResponseDto>(okResult.Value);
            Assert.NotNull(response);
            Assert.Equal(StatusCodes.Status404NotFound,response.ErrorCode); // assuming 0 means success
        }

        [Fact]
        public async Task GetFlowSteps_ProcessesSteps()
        {
            // Arrange
            var request = new FlowRequestDto
            {
                ConsumerCode = "cmr-123",
                TenantCode = "ten-001"
            };
            var flowResponse = new FlowResponseDto
            {
                Steps = new List<FlowStepDto> { new FlowStepDto { StepId = 1 } }
            };
            _cohortConsumerService.Setup(x => x.GetConsumerAllCohorts("ten-001", "cmr-123"))
                .ReturnsAsync(new CohortsResponseDto { Cohorts = new List<CohortDto>() });
            _tenantClient.Setup(x => x.Post<FlowResponseDto>(It.IsAny<string>(), It.IsAny<FlowRequestDto>()))
                .ReturnsAsync(flowResponse);
            _flowStepProcessor.Setup(x => x.ProcessSteps(flowResponse.Steps, "cmr-123"))
                .ReturnsAsync(new List<FlowStepDto> { new FlowStepDto { StepId = 2 } });

            // Act
            var result = await _controller.GetFlowSteps(request);

            // Assert
            var actionResult = Assert.IsType<ActionResult<FlowResponseDto>>(result);
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            var response = Assert.IsType<FlowResponseDto>(okResult.Value);
            Assert.Single(response.Steps);
            Assert.Equal(2, response.Steps[0].StepId);
        }

        [Fact]
        public async Task GetFlowSteps_ReturnsInternalServerError_WhenExceptionThrown()
        {
            // Arrange
            var request = new FlowRequestDto
            {
                ConsumerCode = "cmr-123",
                TenantCode = "ten-001"
            };
            _tenantClient.Setup(x => x.Post<FlowResponseDto>(It.IsAny<string>(), It.IsAny<FlowRequestDto>()))
                .ThrowsAsync(new System.Exception("API error"));

            // Act
            var result = await _controller.GetFlowSteps(request);

            // Assert
            var actionResult = Assert.IsType<ActionResult<FlowResponseDto>>(result);
            var badRequest = Assert.IsType<ObjectResult>(actionResult.Result);
            var response = Assert.IsType<FlowResponseDto>(badRequest.Value);
            Assert.Equal(StatusCodes.Status500InternalServerError, response.ErrorCode);
        }

        [Fact]
        public async Task GetFlowSteps_ReturnsNotFound_WhenErrorCodeIs404()
        {
            // Arrange
            var request = new FlowRequestDto
            {
                ConsumerCode = "cmr-123",
                TenantCode = "ten-001"
            };
            _tenantClient.Setup(x => x.Post<FlowResponseDto>(It.IsAny<string>(), It.IsAny<FlowRequestDto>()))
                .ReturnsAsync(new FlowResponseDto { ErrorCode = StatusCodes.Status404NotFound });

            // Act
            var result = await _controller.GetFlowSteps(request);

            // Assert
            var actionResult = Assert.IsType<ActionResult<FlowResponseDto>>(result);
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(actionResult.Result);
            var response = Assert.IsType<FlowResponseDto>(notFoundResult.Value);
            Assert.Equal(StatusCodes.Status404NotFound, response.ErrorCode);
        }
    }
}
