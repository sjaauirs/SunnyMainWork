using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Sunny.Benefits.Bff.Api.Controllers;
using Sunny.Benefits.Bff.Core.Constants;
using Sunny.Benefits.Bff.Core.Domain.Dtos;
using Sunny.Benefits.Bff.Infrastructure.Repositories.Interfaces;
using Sunny.Benefits.Bff.Infrastructure.Services;
using Sunny.Benefits.Bff.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.Cohort.Core.Domain.Dtos;
using SunnyRewards.Helios.Tenant.Core.Domain.Dtos;
using SunnyRewards.Helios.User.Core.Domain.Dtos;
using System.Collections.Generic;
using System.Runtime.ConstrainedExecution;
using System.Threading.Tasks;
using Xunit;

public class ConsumerFlowProgressControllerTests
{
    private readonly ConsumerFlowProgressController _controller;
    private readonly Mock<IUserClient> _userClient;
    private readonly Mock<IFlowStepService> _flowStepService;
    public ConsumerFlowProgressControllerTests()
    {
        
        var logger = new Mock<ILogger<ConsumerFlowProgressService>>();
        _userClient = new Mock<IUserClient>();
        _flowStepService = new Mock<IFlowStepService>();
        var cohortConsumerService = new Mock<ICohortConsumerService>(); // test double

        var service = new ConsumerFlowProgressService(logger.Object, _userClient.Object, cohortConsumerService.Object , _flowStepService.Object);

        var controllerLogger = new LoggerFactory().CreateLogger<ConsumerFlowProgressController>();
        _controller = new ConsumerFlowProgressController(controllerLogger,service);
    }

    [Fact]
    public async Task GetConsumerFlowProgressAsync_ReturnsOk_WhenRequestIsValid()
    {
        // Arrange
        var request = new GetConsumerFlowRequestDto()
        {
            ConsumerCode = "cmr-123",
            TenantCode = "ten-001",
        };

        _userClient.Setup(x => x.Post<ConsumerFlowProgressResponseDto>(CommonConstants.GetCurrentFlowStatusAPIUrl, It.IsAny<ConsumerFlowProgressRequestDto>()))
            .ReturnsAsync(new ConsumerFlowProgressResponseDto() { ConsumerFlowProgress = new ConsumerFlowProgressDto() { FlowFk = 10} });

        _flowStepService.Setup(x => x.GetFlowSteps(It.IsAny<FlowRequestDto>()))
            .ReturnsAsync(new FlowResponseDto() {FlowId = 10 , Steps = new List<FlowStepDto>() { new FlowStepDto() {ComponentName = "c" } } });

        // Act
        var result = await _controller.GetConsumerFlowProgressAsync(request);

        // Assert
        var actionResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<OnboardingFlowStepsResponseDto>(actionResult.Value);
        Assert.Null(response.ErrorCode); 
    }

    [Fact]
    public async Task GetConsumerFlowProgressAsync_ReturnsErrorResponse()
    {
        // Arrange
        var request = new GetConsumerFlowRequestDto()
        {
            ConsumerCode = "cmr - 123",
            TenantCode = "ten-001"
        };

        _userClient.Setup(x => x.Post<ConsumerFlowProgressResponseDto>(It.IsAny<string>(), It.IsAny< GetConsumerFlowRequestDto> ()))
            .ReturnsAsync(new ConsumerFlowProgressResponseDto()
            {
                ErrorCode = StatusCodes.Status500InternalServerError
            });

        // Act
        var result = await _controller.GetConsumerFlowProgressAsync(request);

        // Assert
        var statusResult = Assert.IsType<ObjectResult>(result);
        var response = Assert.IsType<OnboardingFlowStepsResponseDto>(statusResult.Value);

        Assert.Equal(404, response.ErrorCode);
    }
    [Fact]
    public async Task GetConsumerFlowProgressAsync_ReturnsErroResponse_When_UserClient_ThrowsException()
    {
        // Arrange
        var request = new GetConsumerFlowRequestDto()
        {
            ConsumerCode = "cmr - 123",
            TenantCode = "ten-001"
        };

        _userClient.Setup(x => x.Post<ConsumerFlowProgressResponseDto>(It.IsAny<string>(), It.IsAny< GetConsumerFlowRequestDto> ()))
            .ThrowsAsync(new Exception("testing"));

        // Act
        var result = await _controller.GetConsumerFlowProgressAsync(request);

        // Assert
        var statusResult = Assert.IsType<ObjectResult>(result);
        var response = Assert.IsType<OnboardingFlowStepsResponseDto>(statusResult.Value);

        Assert.Equal(404, response.ErrorCode);
    }
    private static UpdateConsumerFlowRequestDto GetValidRequest()
    {
        return new UpdateConsumerFlowRequestDto
        {
            TenantCode = "tenant1",
            ConsumerCode = "consumer1",
            FlowId = 1,
            Status = "completed",
            CurrentStepId = 1
        };
    }

    private static UpdateConsumerFlowRequestDto GetValidRequest1()
    {
        return new UpdateConsumerFlowRequestDto
        {
            TenantCode = "tenant1",
            ConsumerCode = "consumer1",
            FlowId = 1,
            Status = "skip",
            CurrentStepId = 1
        };
    }

    [Fact]
    public async Task UpdateConsumerOnboardingFlowAsync_ReturnsOk_WhenNoError()
    {
        // Arrange
        var request = GetValidRequest1();
        var response = new ConsumerFlowProgressResponseDto
        {
            ErrorCode = null,
            ConsumerFlowProgress = new ConsumerFlowProgressDto()
        };

        var json = "{\"skip_steps\": true, \"connected_component\": [109, 110, 111, 112]}";

        _flowStepService.Setup(x => x.GetFlowSteps(It.IsAny<FlowRequestDto>()))
            .ReturnsAsync(new FlowResponseDto() { FlowId = 10, Steps = new List<FlowStepDto>() { new FlowStepDto() { 
                ComponentName = "c" , StepId = 1 , StepConfigJson = json} } });

        _userClient.Setup(x => x.Post<ConsumerFlowProgressResponseDto>(CommonConstants.GetCurrentFlowStatusAPIUrl, It.IsAny<ConsumerFlowProgressRequestDto>()))
            .ReturnsAsync(new ConsumerFlowProgressResponseDto() { ConsumerFlowProgress = new ConsumerFlowProgressDto() { FlowFk = 10 } });

        // Act
        var result = await _controller.UpdateConsumerOnboardingFlowAsync(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response1 = Assert.IsType<OnboardingFlowStepsResponseDto>(okResult.Value);
        Assert.Null(response1.ErrorCode);
    }

    [Fact]
    public async Task UpdateConsumerOnboardingFlowAsync_ReturnsStatusCode_WhenErrorCodePresent()
    {
        // Arrange
        var request = GetValidRequest1();
        var response = new ConsumerFlowProgressResponseDto
        {
            ErrorCode = 500
        };
        var json = "{\"skip_steps\": true, \"connected_component\": [109, 110, 111, 112]}";

        _flowStepService.Setup(x => x.GetFlowSteps(It.IsAny<FlowRequestDto>()))
            .ReturnsAsync(new FlowResponseDto()
            {
                FlowId = 10,
                Steps = new List<FlowStepDto>() { new FlowStepDto() {
                ComponentName = "c" , StepId = 2 , StepConfigJson = json} }
            });

        _userClient.Setup(x => x.Post<ConsumerFlowProgressResponseDto>(CommonConstants.GetCurrentFlowStatusAPIUrl, It.IsAny<ConsumerFlowProgressRequestDto>()))
            .ReturnsAsync(new ConsumerFlowProgressResponseDto() { ConsumerFlowProgress = new ConsumerFlowProgressDto() { FlowFk = 10 } });


        // Act
        var result = await _controller.UpdateConsumerOnboardingFlowAsync(request);

        // Assert
        var statusResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, statusResult.StatusCode);
    }

    [Fact]
    public async Task UpdateConsumerOnboardingFlowAsync_ReturnsInternalServerError_OnException()
    {
        // Arrange
        var request = GetValidRequest();
        _userClient.Setup(x => x.Post<ConsumerFlowProgressResponseDto>(It.IsAny<string>(), It.IsAny<UpdateFlowStatusRequestDto>()))
            .ThrowsAsync(new Exception("testing"));

        // Act
        var result = await _controller.UpdateConsumerOnboardingFlowAsync(request);

        // Assert
        var statusResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, statusResult.StatusCode);
        var errorResponse = Assert.IsType<ConsumerFlowProgressResponseDto>(statusResult.Value);
        Assert.Equal(500, errorResponse.ErrorCode);
    }
}


