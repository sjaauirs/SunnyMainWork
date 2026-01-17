using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using SunnyRewards.Helios.Admin.Api.Controllers;
using SunnyRewards.Helios.Admin.Core.Domain.Constants;
using SunnyRewards.Helios.Admin.Infrastructure.HttpClients.Interfaces;
using SunnyRewards.Helios.Admin.Infrastructure.Services;
using SunnyRewards.Helios.Admin.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.Admin.UnitTest.Fixtures.MockDtos;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.Task.Core.Domain.Dtos;
using Xunit;
using TaskAlias = System.Threading.Tasks.Task;

namespace SunnyRewards.Helios.Admin.UnitTest.Controllers
{
    public class TaskRewardControllerUnitTests
    {
        private readonly Mock<ILogger<TaskRewardController>> _controllerLogger;
        private readonly Mock<ILogger<TaskRewardService>> _taskRewardServiceLogger;
        private readonly Mock<IMapper> _mapper;
        private readonly Mock<ITaskClient> _taskClient;
        private readonly ITaskRewardService _taskRewardService;
        private readonly TaskRewardController _taskRewardController;
        public TaskRewardControllerUnitTests()
        {
            _controllerLogger = new Mock<ILogger<TaskRewardController>>();
            _taskRewardServiceLogger = new Mock<ILogger<TaskRewardService>>();
            _mapper = new Mock<IMapper>();
            _taskClient = new Mock<ITaskClient>();
            _taskRewardService = new TaskRewardService(_taskRewardServiceLogger.Object, _mapper.Object, _taskClient.Object);

            _taskRewardController = new TaskRewardController(_controllerLogger.Object, _taskRewardService);
        }
        [Fact]
        public async TaskAlias CreateTaskReward_ShouldReturnOkResult()
        {
            // Arrange
            var createTaskRewardRequestDto = CreateTaskRewardMockDto();
            _taskClient.Setup(x => x.Post<BaseResponseDto>(Constant.CreateTaskRewardUrl, It.IsAny<CreateTaskRewardRequestDto>()))
                .ReturnsAsync(new BaseResponseDto() { ErrorCode = null });
            // Act
            var result = await _taskRewardController.CreateTaskReward(createTaskRewardRequestDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
        }

        [Fact]
        public async TaskAlias CreateTaskReward_ShouldReturnErrorResult_WhenCreateTaskReturnsError()
        {
            // Arrange
            var createTaskRewardRequestDto = CreateTaskRewardMockDto();

            _taskClient.Setup(x => x.Post<BaseResponseDto>(Constant.CreateTaskRewardUrl, It.IsAny<CreateTaskRewardRequestDto>()))
                .ReturnsAsync(new BaseResponseDto() { ErrorCode = StatusCodes.Status500InternalServerError });

            // Act
            var result = await _taskRewardController.CreateTaskReward(createTaskRewardRequestDto);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
        }
        [Fact]
        public async TaskAlias CreateTaskReward_ShouldThrow_Exception_ErrorResult_WhenCreateTaskReturnsException()
        {
            // Arrange
            var createTaskRewardRequestDto = CreateTaskRewardMockDto();

            _taskClient.Setup(x => x.Post<BaseResponseDto>(Constant.CreateTaskRewardUrl, It.IsAny<CreateTaskRewardRequestDto>()))
                    .ThrowsAsync(new Exception("An error occurred while Creating the task details."));

            // Act
            var result = await _taskRewardController.CreateTaskReward(createTaskRewardRequestDto);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
        }
        [Fact]
        public async TaskAlias GetTasksAndRewards_ShouldReturnOkResult()
        {
            // Arrange
            var getTaskRewardsRequestDto = new GetTasksAndTaskRewardsRequestDto();
            _taskClient.Setup(x => x.Post<GetTasksAndTaskRewardsResponseDto>(Constant.GetTasksAndTaskRewardsAPIUrl, It.IsAny<GetTasksAndTaskRewardsRequestDto>()))
                .ReturnsAsync(new GetTasksAndTaskRewardsResponseDto());
            // Act
            var result = await _taskRewardController.GetTasksAndTaskRewards(getTaskRewardsRequestDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
        }

        [Fact]
        public async TaskAlias GetTasksAndRewards_ShouldReturnErrorResult_WhenCreateTaskReturnsError()
        {
            // Arrange
            var getTaskRewardsRequestDto = new GetTasksAndTaskRewardsRequestDto();
            _taskClient.Setup(x => x.Post<GetTasksAndTaskRewardsResponseDto>(Constant.GetTasksAndTaskRewardsAPIUrl, It.IsAny<GetTasksAndTaskRewardsRequestDto>()))
                .ReturnsAsync(new GetTasksAndTaskRewardsResponseDto() { ErrorCode = StatusCodes.Status404NotFound });

            // Act
            var result = await _taskRewardController.GetTasksAndTaskRewards(getTaskRewardsRequestDto);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status404NotFound, objectResult.StatusCode);
        }
        [Fact]
        public async TaskAlias GetTasksAndRewards_ShouldThrow_Exception_ErrorResult_WhenCreateTaskReturnsException()
        {
            // Arrange
            var getTaskRewardsRequestDto = new GetTasksAndTaskRewardsRequestDto();

            _taskClient.Setup(x => x.Post<GetTasksAndTaskRewardsResponseDto>(Constant.GetTasksAndTaskRewardsAPIUrl, It.IsAny<GetTasksAndTaskRewardsRequestDto>()))
                    .ThrowsAsync(new Exception("An error occurred while Creating the task details."));

            // Act
            var result = await _taskRewardController.GetTasksAndTaskRewards(getTaskRewardsRequestDto);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
        }

        private static CreateTaskRewardRequestDto CreateTaskRewardMockDto()
        {
            return new CreateTaskRewardRequestDto
            {
                TaskCode = "tsk-90399d4b7682458cbc9a93206967",
                TaskReward = new TaskRewardDto
                {
                    TaskId = 9,
                    TaskRewardId = 10,
                    RewardTypeId = 1,
                    TenantCode = "ten-ecada21e57154928a2bb959e8365b8b4",
                    TaskRewardCode = "abcdergfg",
                    TaskActionUrl = "retool.com",
                    Reward = "Your Reward.",
                    Priority = 1,
                    Expiry = DateTime.UtcNow,
                    MinTaskDuration = 1,
                    MaxTaskDuration = 2,
                    TaskExternalCode = "external code",
                    ValidStartTs = DateTime.UtcNow,
                    RecurrenceDefinitionJson = "",
                    IsRecurring = false,
                    SelfReport = false,
                    TaskCompletionCriteriaJson = "Search Providers",
                    CreateUser = "per-915325069cdb42c783dd4601e1d27704"
                }
            };
        }

        [Fact]
        public async TaskAlias UpdateTaskReward_ShouldReturnOkResult()
        {
            // Arrange
            long taskRewardId = 1;
            var taskRewardRequestDto = new TaskRewardRequestMockDto();
            _taskClient.Setup(x => x.Put<TaskRewardResponseDto>(It.IsAny<string>(), It.IsAny<TaskRewardRequestDto>()))
                .ReturnsAsync(new TaskRewardResponseDto());

            // Act
            var result = await _taskRewardController.UpdateTaskRewardAsync(taskRewardId, taskRewardRequestDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
        }

        [Fact]
        public async TaskAlias UpdateTaskReward_ShouldReturnConflict_WhenUpdateTaskReturnsError()
        {
            // Arrange
            long taskRewardId = 1;
            var taskRewardRequestDto = new TaskRewardRequestMockDto();
            _taskClient.Setup(x => x.Put<TaskRewardResponseDto>(It.IsAny<string>(), It.IsAny<TaskRewardRequestDto>()))
                .ReturnsAsync(new TaskRewardResponseDto { ErrorCode = StatusCodes.Status500InternalServerError });

            // Act
            var result = await _taskRewardController.UpdateTaskRewardAsync(taskRewardId, taskRewardRequestDto);

            // Assert
            var objectResult = Assert.IsType<ConflictObjectResult>(result);
            Assert.Equal(StatusCodes.Status409Conflict, objectResult.StatusCode);
        }

        [Fact]
        public async TaskAlias UpdateTaskReward_ShouldThrow_Exception_WhenUpdateTaskThrowsException()
        {
            // Arrange
            long taskRewardId = 1;
            var taskRewardRequestDto = new TaskRewardRequestMockDto();
            _taskClient.Setup(x => x.Put<TaskRewardResponseDto>($"{Constant.TaskRewardApiUrl}/{taskRewardId}", It.IsAny<TaskRewardRequestDto>()))
                .ThrowsAsync(new Exception("Error occurred while updating task reward."));

            // Act
            var response = await _taskRewardController.UpdateTaskRewardAsync(taskRewardId, taskRewardRequestDto);

            // Assert
            var result = Assert.IsType<ConflictObjectResult>(response);
            Assert.Equal(StatusCodes.Status409Conflict, result.StatusCode);

            var returnedResponse = Assert.IsType<TaskRewardResponseDto>(result.Value);

            Assert.Equal(StatusCodes.Status500InternalServerError, returnedResponse.ErrorCode);
            Assert.Equal("Error occurred while updating task reward.", returnedResponse.ErrorMessage);
        }

        [Fact]
        public async TaskAlias GetTaskRewardDetails_ShouldReturnOkResult()
        {
            // Arrange
            string tenantCode = "tenant123";
            string languageCode = "en-US";
            _taskClient.Setup(x => x.Get<TaskRewardDetailsResponseDto>($"{Constant.TaskRewardDetailsApiUrl}?tenantCode={tenantCode}&languageCode={languageCode}", new Dictionary<string, long>()))
                .ReturnsAsync(new TaskRewardDetailsResponseDto());

            // Act
            var result = await _taskRewardController.GetTaskRewardDetails(tenantCode,"en-US");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
        }

        [Fact]
        public async TaskAlias GetTaskRewardDetails_ShouldReturnBadRequest_WhenTenantCodeIsNullOrEmpty()
        {
            // Arrange
            string tenantCode = string.Empty;

            // Act
            var result = await _taskRewardController.GetTaskRewardDetails(tenantCode,string.Empty);

            // Assert
            var objectResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(StatusCodes.Status400BadRequest, objectResult.StatusCode);
        }

        [Fact]
        public async TaskAlias GetTaskRewardDetails_ShouldReturnInternalServerError_WhenServiceThrowsException()
        {
            // Arrange
            string tenantCode = "tenant123";
            string languageCode = "en-US";
            _taskClient.Setup(x => x.Get<TaskRewardDetailsResponseDto>($"{Constant.TaskRewardDetailsApiUrl}?tenantCode={tenantCode}&languageCode={languageCode}", new Dictionary<string, long>()))
                .ThrowsAsync(new Exception("Error occurred while retrieving task reward details."));

            // Act
            var result = await _taskRewardController.GetTaskRewardDetails(tenantCode,"en-Us");

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
        }
    }
}
