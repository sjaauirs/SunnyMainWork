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
using System.Dynamic;
using Xunit;
using TaskAlias = System.Threading.Tasks.Task;

namespace SunnyRewards.Helios.Admin.UnitTest.Controllers
{
    public class TaskControllerTests
    {
        private readonly Mock<ILogger<TaskController>> _controllerLogger;
        private readonly Mock<ILogger<TaskService>> _taskServiceLogger;
        private readonly Mock<IMapper> _mapper;
        private readonly Mock<ITaskClient> _taskClient;
        private readonly ITaskService _taskService;
        private readonly TaskController _taskController;

        public TaskControllerTests()
        {
            // Initialize mocks
            _controllerLogger = new Mock<ILogger<TaskController>>();
            _taskServiceLogger = new Mock<ILogger<TaskService>>();
            _mapper = new Mock<IMapper>();
            _taskClient = new Mock<ITaskClient>();
            _taskService = new TaskService(_taskServiceLogger.Object, _mapper.Object, _taskClient.Object);
            _taskController = new TaskController(_controllerLogger.Object, _taskService);
        }

        [Fact]
        public async TaskAlias CreateTask_ShouldReturnOkResult()
        {
            // Arrange
            var createTaskRequestDto = CreateTaskMockDto();
            _taskClient.Setup(x => x.Post<BaseResponseDto>(Constant.CreateTaskAPIUrl, It.IsAny<CreateTaskRequestDto>()))
                .ReturnsAsync(new BaseResponseDto() { ErrorCode = null });
            // Act
            var result = await _taskController.CreateTask(createTaskRequestDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
        }

        [Fact]
        public async TaskAlias CreateTask_ShouldReturnErrorResult_WhenCreateTaskReturnsError()
        {
            // Arrange
            var createTaskRequestDto = CreateTaskMockDto();

            _taskClient.Setup(x => x.Post<BaseResponseDto>(Constant.CreateTaskAPIUrl, It.IsAny<CreateTaskRequestDto>()))
                .ReturnsAsync(new BaseResponseDto() { ErrorCode = StatusCodes.Status500InternalServerError });

            // Act
            var result = await _taskController.CreateTask(createTaskRequestDto);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
        }
        [Fact]
        public async TaskAlias CreateTask_ShouldThrow_Exception_ErrorResult_WhenCreateTaskReturnsException()
        {
            // Arrange
            var createTaskRequestDto = CreateTaskMockDto();

            _taskClient.Setup(x => x.Post<BaseResponseDto>(Constant.CreateTaskAPIUrl, It.IsAny<CreateTaskRequestDto>()))
                    .ThrowsAsync(new Exception("An error occurred while Creating the task."));

            // Act
            var result = await _taskController.CreateTask(createTaskRequestDto);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
        }

        [Fact]
        public void SoftDeleteTask_InvalidRequest_ReturnsBadRequest()
        {
            // Arrange
            dynamic request = new ExpandoObject();
            // Intentionally not adding required fields to simulate invalid request

            var expectedResponse = new BaseResponseDto
            {
                ErrorCode = StatusCodes.Status400BadRequest,
                ErrorMessage = " Invalid data for adding Consumer to Cohort"
            };

            // Act
            BaseResponseDto result = _taskService.SoftDeleteTask(request);

            // Assert
            Assert.Equal(expectedResponse.ErrorCode, result.ErrorCode);
            Assert.Equal(expectedResponse.ErrorMessage, result.ErrorMessage);
        }

        [Fact]
        public void SoftDeleteTask_ValidRequest_ReturnsSuccessfulResponse()
        {
            // Arrange
            dynamic request = new ExpandoObject();
            request.TaskExternalCode = "Task123"; // Assuming this is a required field
            request.ConsumerCode = "Consumer456"; // Assuming this is also a required field
            request.TenantCode = "TenantXYZ"; // Assuming this is also a required field

            var deleteConsumerTaskRequestDto = new DeleteConsumerTaskRequestDto
            {
                TaskExternalCode = "Task123",
                ConsumerCode = "Consumer456",
                TenantCode = "TenantXYZ"
            };

            var expectedResponse = new BaseResponseDto
            {
                ErrorCode = null
            };


            _taskClient.Setup(client => client.Post<BaseResponseDto>("remove-consumer-task", It.IsAny<DeleteConsumerTaskRequestDto>()))
                .ReturnsAsync(expectedResponse);

            // Act
            BaseResponseDto result = _taskService.SoftDeleteTask(request);

            // Assert
            Assert.Null(result.ErrorCode);
            Assert.Equal(expectedResponse, result);
        }
        [Fact]
        public async TaskAlias GetTaskByTaskName_ShouldReturnOkResult()
        {
            // Arrange
            var createTaskRequestDto = new GetTaskByTaskNameRequestDto();
            _taskClient.Setup(x => x.Post<GetTaskByTaskNameResponseDto>(Constant.GetTaskAPIUrl, It.IsAny<GetTaskByTaskNameRequestDto>()))
                .ReturnsAsync(new GetTaskByTaskNameResponseDto() { ErrorCode = null });
            // Act
            var result = await _taskController.GetTaskByTaskName(createTaskRequestDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
        }

        [Fact]
        public async TaskAlias GetTaskByTaskName_ShouldReturnErrorResult_WhenCreateTaskReturnsError()
        {
            // Arrange
            var createTaskRequestDto = new GetTaskByTaskNameRequestDto();

            _taskClient.Setup(x => x.Post<GetTaskByTaskNameResponseDto>(Constant.GetTaskAPIUrl, It.IsAny<CreateTaskRequestDto>()))
                .ReturnsAsync(new GetTaskByTaskNameResponseDto() { ErrorCode = StatusCodes.Status500InternalServerError });

            // Act
            var result = await _taskController.GetTaskByTaskName(createTaskRequestDto);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
        }
        [Fact]
        public async TaskAlias GetTaskByTaskName_ShouldThrow_Exception_ErrorResult_WhenCreateTaskReturnsException()
        {
            // Arrange
            var createTaskRequestDto = new GetTaskByTaskNameRequestDto();

            _taskClient.Setup(x => x.Post<GetTaskByTaskNameResponseDto>(Constant.GetTaskAPIUrl, It.IsAny<CreateTaskRequestDto>()))
                    .ThrowsAsync(new Exception("An error occurred while fetching the task."));

            // Act
            var result = await _taskController.GetTaskByTaskName(createTaskRequestDto);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
        }

        [Fact]
        public async TaskAlias GetTasksAsync_ShouldReturnOkResult()
        {
            // Arrange
            var responseDto = new TasksResponseMockDto();

            _taskClient.Setup(x => x.Get<TasksResponseDto>(Constant.TasksApiUrl, new Dictionary<string, long>()))
                .ReturnsAsync(responseDto);

            // Act
            var result = await _taskController.GetTasksAsync();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);

            var returnedResponse = Assert.IsType<TasksResponseMockDto>(okResult.Value);
            Assert.NotNull(returnedResponse.Tasks);
            Assert.Equal(2, returnedResponse.Tasks.Count);
        }

        [Fact]
        public async TaskAlias GetTasksAsync_ShouldReturnErrorResult_WhenErrorOccurs()
        {
            // Arrange
            var responseDto = new TasksResponseDto
            {
                ErrorCode = StatusCodes.Status500InternalServerError,
                ErrorMessage = "Internal Server Error"
            };

            _taskClient.Setup(x => x.Get<TasksResponseDto>(Constant.TasksApiUrl, new Dictionary<string, long>()))
                .ReturnsAsync(responseDto);

            // Act
            var result = await _taskController.GetTasksAsync();

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);

            var returnedResponse = Assert.IsType<TasksResponseDto>(objectResult.Value);
            Assert.Equal(StatusCodes.Status500InternalServerError, returnedResponse.ErrorCode);
            Assert.Equal("Internal Server Error", returnedResponse.ErrorMessage);
        }

        [Fact]
        public async TaskAlias GetTasksAsync_ShouldReturnException_WhenUnhandledErrorOccurs()
        {
            // Arrange
            var exceptionMessage = "Unhandled Exception";
            _taskClient.Setup(x => x.Get<TasksResponseDto>(Constant.TasksApiUrl, new Dictionary<string, long>()))
                .ThrowsAsync(new Exception(exceptionMessage));

            // Act
            var result = await _taskController.GetTasksAsync();

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);

            var returnedResponse = Assert.IsType<TasksResponseDto>(objectResult.Value);
            Assert.Equal(StatusCodes.Status500InternalServerError, returnedResponse.ErrorCode);
            Assert.Equal(exceptionMessage, returnedResponse.ErrorMessage);
        }

        [Fact]
        public async TaskAlias UpdateTaskAsync_ShouldReturnOkResult()
        {
            // Arrange
            var taskId = 1;
            var requestDto = new TaskRequestMockDto();
            var responseDto = new TaskResponseMockDto();

            _taskClient.Setup(x => x.Put<TaskResponseDto>($"{Constant.TasksApiUrl}/{taskId}", requestDto))
                .ReturnsAsync(responseDto);

            // Act
            var result = await _taskController.UpdateTaskAsync(taskId, requestDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);

            var returnedResponse = Assert.IsType<TaskResponseDto>(okResult.Value);
            Assert.Equal(taskId, returnedResponse?.Task?.TaskId);
            Assert.Equal("Task001", returnedResponse?.Task?.TaskCode);
        }

        [Fact]
        public async TaskAlias UpdateTaskAsync_ShouldReturnErrorResult_WhenErrorOccurs()
        {
            // Arrange
            var taskId = 1;
            var requestDto = new TaskRequestMockDto();
            var responseDto = new TaskResponseDto
            {
                ErrorCode = StatusCodes.Status500InternalServerError,
                ErrorMessage = "Object reference not set to an instance of an object."
            };

            _taskClient.Setup(x => x.Put<TaskResponseDto>(
                    $"{Constant.TasksApiUrl}/{taskId}", It.IsAny<TaskRequestDto>()))
                .ReturnsAsync(responseDto);

            // Act
            var result = await _taskController.UpdateTaskAsync(taskId, requestDto);

            // Assert
            var conflictResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, conflictResult.StatusCode);

            var returnedResponse = Assert.IsType<TaskResponseDto>(conflictResult.Value);
            Assert.Equal(responseDto.ErrorCode, returnedResponse.ErrorCode);
            Assert.Equal(responseDto.ErrorMessage, returnedResponse.ErrorMessage);
        }

        [Fact]
        public async TaskAlias UpdateTaskAsync_ShouldReturnException_WhenUnhandledErrorOccurs()
        {
            // Arrange
            var taskId = 1;
            var requestDto = new TaskRequestMockDto();
            var exceptionMessage = "Object reference not set to an instance of an object.";

            _taskClient.Setup(x => x.Put<TaskResponseDto>(
                    $"{Constant.TasksApiUrl}/{taskId}", It.IsAny<TaskRequestDto>()))
                .ThrowsAsync(new Exception(exceptionMessage));

            // Act
            var result = await _taskController.UpdateTaskAsync(taskId, requestDto);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);

            var errorResponse = Assert.IsType<TaskResponseDto>(objectResult.Value);
            Assert.Equal(exceptionMessage, errorResponse.ErrorMessage);
        }


        private static CreateTaskRequestDto CreateTaskMockDto()
        {
            return new CreateTaskRequestDto
            {
                TaskId = 47,
                TaskTypeId = 1,
                TaskCode = "tsk-90399d4b7682458cbc9a93206967",
                TaskName = "Enroll in pre diabetes program",
                SelfReport = false,
                ConfirmReport = false,
                TaskCategoryId = 0,
                IsSubtask = false,
                CreateUser = "per-915325069cdb42c783dd4601e1d27704"
            };
        }

    }
}
