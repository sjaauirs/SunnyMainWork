using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using SunnyRewards.Helios.Admin.Api.Controllers;
using SunnyRewards.Helios.Admin.Core.Domain.Constants;
using SunnyRewards.Helios.Admin.Infrastructure.HttpClients.Interfaces;
using SunnyRewards.Helios.Admin.Infrastructure.Services;
using SunnyRewards.Helios.Admin.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.Task.Core.Domain.Dtos;
using Xunit;
using TaskAlias = System.Threading.Tasks.Task;

namespace SunnyRewards.Helios.Admin.UnitTest.Controllers
{
    public class TaskExternalMappingControllerUnitTest
    {
        private readonly Mock<ILogger<TaskExternalMappingController>> _controllerLogger;
        private readonly Mock<ILogger<TaskExternalMappingService>> _taskServiceLogger;
        private readonly Mock<ITaskClient> _taskClient;
        private readonly ITaskExternalMappingService _taskService;
        private readonly TaskExternalMappingController _taskController;

        public TaskExternalMappingControllerUnitTest()
        {
            // Initialize mocks
            _controllerLogger = new Mock<ILogger<TaskExternalMappingController>>();
            _taskServiceLogger = new Mock<ILogger<TaskExternalMappingService>>();
            _taskClient = new Mock<ITaskClient>();
            _taskService = new TaskExternalMappingService(_taskServiceLogger.Object, _taskClient.Object);

            _taskController = new TaskExternalMappingController(_controllerLogger.Object, _taskService);
        }
        [Fact]
        public async TaskAlias CreateMappingTask_ShouldReturnOkResult()
        {
            var requestDto = CreateTaskExternalMappicMockDto();

            _taskClient.Setup(x => x.Post<BaseResponseDto>(Constant.CreateTaskExternalMappingRequest, It.IsAny<TaskExternalMappingRequestDto>()))
                     .ReturnsAsync(new BaseResponseDto() { ErrorCode = null });
            // Act
            var result = await _taskController.CreateTaskExternalMapping(requestDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
        }

        [Fact]
        public async TaskAlias CreateMappingTask_ShouldReturnErrorResult_WhenCreateTaskReturnsError()
        {
            // Arrange
            var requestDto = CreateTaskExternalMappicMockDto();

            _taskClient.Setup(x => x.Post<BaseResponseDto>(Constant.CreateTaskExternalMappingRequest, It.IsAny<TaskExternalMappingRequestDto>()))
                    .ReturnsAsync(new BaseResponseDto() { ErrorCode = StatusCodes.Status500InternalServerError });

            // Act
            var result = await _taskController.CreateTaskExternalMapping(requestDto);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
        }
        [Fact]
        public async TaskAlias CreateMappingTask_ShouldThrow_Exception_ErrorResult_WhenCreateTaskReturnsException()
        {
            // Arrange
            var requestDto = CreateTaskExternalMappicMockDto();

            _taskClient.Setup(x => x.Post<BaseResponseDto>(Constant.CreateTaskExternalMappingRequest, It.IsAny<TaskExternalMappingRequestDto>()))
                    .ThrowsAsync(new Exception("An error occurred while Creating the task."));

            // Act
            var result = await _taskController.CreateTaskExternalMapping(requestDto);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
        }

        private static TaskExternalMappingRequestDto CreateTaskExternalMappicMockDto()
        {
            return new TaskExternalMappingRequestDto
            {

                TenantCode = "ten-90399d4b7682458cbc9a93206967",
                TaskThirdPartyCode = "tsn-90399d4b7682458cbc9a93206967",
                TaskExternalCode = "tsp-90399d4b7682458cbc9a93206967",

                CreateUser = "per-915325069cdb42c783dd4601e1d27704"
            };
        }

    }
}
