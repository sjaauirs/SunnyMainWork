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
    public class TaskDetailsControllerUnitTests
    {
        private readonly Mock<ILogger<TaskDetailsController>> _controllerLogger;
        private readonly Mock<ILogger<TaskDetailsService>> _taskDetailsServiceLogger;
        private readonly Mock<IMapper> _mapper;
        private readonly Mock<ITaskClient> _taskClient;
        private readonly ITaskDetailsService _taskDetailsService;
        private readonly TaskDetailsController _taskDetailsController;

        public TaskDetailsControllerUnitTests()
        {
            _controllerLogger = new Mock<ILogger<TaskDetailsController>>();
            _taskDetailsServiceLogger = new Mock<ILogger<TaskDetailsService>>();
            _mapper = new Mock<IMapper>();
            _taskClient = new Mock<ITaskClient>();
            _taskDetailsService = new TaskDetailsService(_taskDetailsServiceLogger.Object, _mapper.Object, _taskClient.Object);

            _taskDetailsController = new TaskDetailsController(_controllerLogger.Object, _taskDetailsService);
        }
        [Fact]
        public async TaskAlias CreateTask_ShouldReturnOkResult()
        {
            // Arrange
            var createTaskDetailsRequestDto = CreateTaskDetailsMockDto();
            _taskClient.Setup(x => x.Post<BaseResponseDto>(Constant.CreateTaskDetailsUrl, It.IsAny<CreateTaskDetailsRequestDto>()))
                .ReturnsAsync(new BaseResponseDto() { ErrorCode = null });
            // Act
            var result = await _taskDetailsController.CreateTaskDetails(createTaskDetailsRequestDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
        }

        [Fact]
        public async TaskAlias CreateTask_ShouldReturnErrorResult_WhenCreateTaskReturnsError()
        {
            // Arrange
            var createTaskDetailsRequestDto = CreateTaskDetailsMockDto();

            _taskClient.Setup(x => x.Post<BaseResponseDto>(Constant.CreateTaskDetailsUrl, It.IsAny<CreateTaskDetailsRequestDto>()))
                .ReturnsAsync(new BaseResponseDto() { ErrorCode = StatusCodes.Status500InternalServerError });

            // Act
            var result = await _taskDetailsController.CreateTaskDetails(createTaskDetailsRequestDto);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
        }
        [Fact]
        public async TaskAlias CreateTaskDetails_ShouldThrow_Exception_ErrorResult_WhenCreateTaskReturnsException()
        {
            // Arrange
            var createTaskDetailsRequestDto = CreateTaskDetailsMockDto();

            _taskClient.Setup(x => x.Post<BaseResponseDto>(Constant.CreateTaskDetailsUrl, It.IsAny<CreateTaskDetailsRequestDto>()))
                    .ThrowsAsync(new Exception("An error occurred while Creating the task details."));

            // Act
            var result = await _taskDetailsController.CreateTaskDetails(createTaskDetailsRequestDto);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
        }

        private static CreateTaskDetailsRequestDto CreateTaskDetailsMockDto()
        {
            return new CreateTaskDetailsRequestDto
            {
                TaskCode = "tsk-90399d4b7682458cbc9a93206967",
                TaskDetail = new PostTaskDetailsDto
                {
                    TaskId = 9,
                    TaskDetailId = 10,
                    TermsOfServiceId = 1,
                    TaskHeader = "Select your PCP",
                    TaskDescription = "Your Primary Care provider plays in important role in your healthcare.",
                    LanguageCode = "en-US",
                    TenantCode = "ten-ecada21e57154928a2bb959e8365b8b4",
                    TaskCtaButtonText = "Search Providers",
                    CreateUser = "per-915325069cdb42c783dd4601e1d27704"
                }
            };
        }

        [Fact]
        public async TaskAlias UpdateTaskDetailAsync_ShouldReturnOkResult()
        {
            // Arrange
            var taskDetailId = 123;
            var taskDetailRequestDto = new TaskDetailRequestMockDto();
            var responseDto = new TaskDetailResponseDto { ErrorCode = null };

            _taskClient.Setup(x => x.Put<TaskDetailResponseDto>($"{Constant.TaskDetailApiUrl}/{taskDetailId}", taskDetailRequestDto))
                .ReturnsAsync(responseDto);

            // Act
            var response = await _taskDetailsController.UpdateTaskDetailAsync(taskDetailId, taskDetailRequestDto);

            // Assert
            var result = Assert.IsType<OkObjectResult>(response);
            Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
            Assert.IsType<TaskDetailResponseDto>(result.Value);
        }

        [Fact]
        public async TaskAlias UpdateTaskDetailAsync_ShouldReturnErrorResult_WhenUpdateTaskReturnsError()
        {
            // Arrange
            var taskDetailId = 123;
            var taskDetailRequestDto = new TaskDetailRequestMockDto();
            var responseDto = new TaskDetailResponseDto { ErrorCode = StatusCodes.Status409Conflict, ErrorMessage = "Conflict occurred" };

            _taskClient.Setup(x => x.Put<TaskDetailResponseDto>($"{Constant.TaskDetailApiUrl}/{taskDetailId}", taskDetailRequestDto))
                .ReturnsAsync(responseDto);

            // Act
            var result = await _taskDetailsController.UpdateTaskDetailAsync(taskDetailId, taskDetailRequestDto);

            // Assert
            var conflictResult = Assert.IsType<ConflictObjectResult>(result);
            Assert.Equal(StatusCodes.Status409Conflict, conflictResult.StatusCode);

            var returnedResponse = Assert.IsType<TaskDetailResponseDto>(conflictResult.Value);
            Assert.Equal(StatusCodes.Status409Conflict, returnedResponse.ErrorCode);
            Assert.Equal("Conflict occurred", returnedResponse.ErrorMessage);
        }

        [Fact]
        public async TaskAlias UpdateTaskDetailAsync_ShouldThrow_Exception_ErrorResult_WhenUpdateTaskReturnsException()
        {
            // Arrange
            var taskDetailId = 123;
            var taskDetailRequestDto = new TaskDetailRequestMockDto();

            _taskClient.Setup(x => x.Put<TaskDetailResponseDto>($"{Constant.TaskDetailApiUrl}/{taskDetailId}", taskDetailRequestDto))
                .ThrowsAsync(new Exception("An error occurred while updating the task details."));

            // Act
            var response = await _taskDetailsController.UpdateTaskDetailAsync(taskDetailId, taskDetailRequestDto);

            // Assert
            var result = Assert.IsType<ConflictObjectResult>(response);
            Assert.Equal(StatusCodes.Status409Conflict, result.StatusCode);

            var returnedResponse = Assert.IsType<TaskDetailResponseDto>(result.Value);
            Assert.Equal(StatusCodes.Status409Conflict, returnedResponse.ErrorCode);
            Assert.Equal("An error occurred while updating the task details.", returnedResponse.ErrorMessage);
        }
    }
}
