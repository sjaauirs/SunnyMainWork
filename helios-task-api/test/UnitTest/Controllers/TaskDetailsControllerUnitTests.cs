using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.Task.Api.Controller;
using SunnyRewards.Helios.Task.Core.Domain.Dtos;
using SunnyRewards.Helios.Task.Core.Domain.Models;
using SunnyRewards.Helios.Task.Infrastructure.Repositories.Interfaces;
using SunnyRewards.Helios.Task.Infrastructure.Services;
using SunnyRewards.Helios.Task.UnitTest.Fixtures.MockModel;
using System.Linq.Expressions;
using Xunit;

namespace SunnyRewards.Helios.Task.UnitTest.Controllers
{
    public class TaskDetailsControllerUnitTests
    {
        private readonly Mock<ILogger<TaskDetailsController>> _logger;
        private readonly Mock<ITaskDetailRepo> _taskDetailRepoMock;
        private TaskDetailsController _taskDetailController;

        private readonly TaskDetailsService _taskDetailService;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<ILogger<TaskDetailsService>> _serviceLoggerMock;
        private readonly Mock<ITaskRepo> _taskRepo;

        public TaskDetailsControllerUnitTests()
        {
            _logger = new Mock<ILogger<TaskDetailsController>>();
            _taskDetailRepoMock = new Mock<ITaskDetailRepo>();
            _mapperMock = new Mock<IMapper>();
            _serviceLoggerMock = new Mock<ILogger<TaskDetailsService>>();
            _taskRepo = new Mock<ITaskRepo>();

            // Initialize the service with mocks
            _taskDetailService = new TaskDetailsService(_taskDetailRepoMock.Object, _serviceLoggerMock.Object, _mapperMock.Object, _taskRepo.Object);
            _taskDetailController = new TaskDetailsController(_logger.Object, _taskDetailService);
        }

        [Fact]
        public async System.Threading.Tasks.Task CreateTaskDetails_ShouldReturnOk_WhenRequestIsSuccessful()
        {
            // Arrange
            var requestDto = TaskDetailsMock();
            _taskRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<TaskModel, bool>>>(), false)).ReturnsAsync(new TaskMockModel());
            _taskDetailRepoMock.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<TaskDetailModel, bool>>>(), false));
                

            _mapperMock
                .Setup(m => m.Map<TaskDetailModel>(It.IsAny<PostTaskDetailsDto>()))
                .Returns(new TaskDetailModel());
            // Act
            var result = await _taskDetailController.CreateTaskDetails(requestDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<BaseResponseDto>(okResult.Value);
            Assert.Null(response.ErrorCode);
            Assert.Equal(200, okResult.StatusCode);
        }

        [Fact]
        public async System.Threading.Tasks.Task CreateTaskDetails_ShouldReturn_409_WhenRequestIsAlreadyExisted()
        {
            var requestDto = TaskDetailsMock();
            _taskRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<TaskModel, bool>>>(), false)).ReturnsAsync(new TaskMockModel());
            _taskDetailRepoMock.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<TaskDetailModel, bool>>>(), false))
                                        .ReturnsAsync(new TaskDetailModel
                                        {
                                            TenantCode = requestDto.TaskDetail.TenantCode,
                                            TaskId = requestDto.TaskDetail.TaskId,
                                        });


            // Act
            var result = await _taskDetailController.CreateTaskDetails(requestDto);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status409Conflict, objectResult.StatusCode);
            var response = Assert.IsType<BaseResponseDto>(objectResult.Value);
            Assert.Equal(StatusCodes.Status409Conflict, response.ErrorCode);


        }

        [Fact]
        public async System.Threading.Tasks.Task CreateTaskDetails_ShouldReturn_404_WhenRequestTaskCodeIsNotFound()
        {
            var requestDto = TaskDetailsMock();
            // Act
            var result = await _taskDetailController.CreateTaskDetails(requestDto);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status404NotFound, objectResult.StatusCode);
            var response = Assert.IsType<BaseResponseDto>(objectResult.Value);
            Assert.Equal(StatusCodes.Status404NotFound, response.ErrorCode);

        }

        [Fact]
        public async System.Threading.Tasks.Task CreateTaskDetails_ShouldThrow_Exception()
        {
            var requestDto = TaskDetailsMock();
            _taskRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<TaskModel, bool>>>(), false)).ThrowsAsync(new Exception("An error occurred while fetching the task."));

            // Act
            var result = await _taskDetailController.CreateTaskDetails(requestDto);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusCodeResult.StatusCode);

        }

        private static CreateTaskDetailsRequestDto TaskDetailsMock()
        {
            return new CreateTaskDetailsRequestDto
            {
                TaskCode = "Some taskCode",
                TaskDetail = new PostTaskDetailsDto
                {
                    TaskId = 3,
                    TaskDetailId = 4,
                    TermsOfServiceId = 1,
                    TaskHeader = "saving for college",
                    TaskDescription = "",
                    LanguageCode = "en-US",
                    TenantCode = "ten-ecada21e57154928a2bb959e8365b8b4",
                    TaskCtaButtonText = "Learn More",
                    CreateUser = "per-915325069cdb42c783dd4601e1d27704"
                }
            };
        }

        [Fact]
        public async System.Threading.Tasks.Task UpdateTaskDetailAsync_ShouldReturnOk_WhenUpdateIsSuccessful()
        {
            long taskDetailId = 2001;
            // Arrange
            var updateTaskDetailRequestDto = new TaskDetailRequestDto
            {
                TaskId = 1001,
                TermsOfServiceId = 3001,
                TaskHeader = "Header for Task",
                TaskDescription = "Description for Task",
                LanguageCode = "en-US",
                TenantCode = "tenant-001",
                TaskCtaButtonText = "Click Here"
            };

            var responseDto = new TaskDetailResponseDto
            {
                ErrorCode = null,
                TaskDetail = new TaskDetailDto
                {
                    TaskId = 1001,
                    TaskDetailId = 2001,
                    TermsOfServiceId = 3001,
                    TaskHeader = "Header for Task",
                    TaskDescription = "Description for Task",
                    LanguageCode = "en-US",
                    TenantCode = "tenant-001",
                    TaskCtaButtonText = "Click Here",
                    UpdateTs = DateTime.UtcNow
                }
            };

            _taskRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<TaskModel, bool>>>(), false)).ReturnsAsync(new TaskMockModel());
            _taskDetailRepoMock.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<TaskDetailModel, bool>>>(), false))
                                        .ReturnsAsync(new TaskDetailModel
                                        {
                                            TaskId = 1001,
                                            TaskDetailId = 2001,
                                            TermsOfServiceId = 3001,
                                            TaskHeader = "Header for Task",
                                            TaskDescription = "Description for Task",
                                            LanguageCode = "en-US",
                                            TenantCode = "tenant-001",
                                            TaskCtaButtonText = "Click Here",
                                            UpdateTs = DateTime.UtcNow
                                        });
            // Act
            var result = await _taskDetailController.UpdateTaskDetailAsync(taskDetailId, updateTaskDetailRequestDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);

            var response = Assert.IsType<TaskDetailResponseDto>(okResult.Value);
            Assert.Null(response.ErrorCode);
            Assert.Null(response.ErrorMessage);

        }

        [Fact]
        public async System.Threading.Tasks.Task UpdateTaskDetailAsync_ShouldReturnConflict_WhenServiceReturnsConflict()
        {
            long taskDetailId = 2001;
            // Arrange
            var updateTaskDetailRequestDto = new TaskDetailRequestDto
            {
                TaskId = 1001,
                TermsOfServiceId = 3001,
                TaskHeader = "Header for Task",
                TaskDescription = "Description for Task",
                LanguageCode = "en-US",
                TenantCode = "tenant-001",
                TaskCtaButtonText = "Click Here"
            };

            var responseDto = new TaskDetailResponseDto
            {
                ErrorCode = 404, // error,
                ErrorMessage = "No task details found for given TaskDetailId: 2001",
            };

            // Act
            var result = await _taskDetailController.UpdateTaskDetailAsync(taskDetailId, updateTaskDetailRequestDto);

            // Assert
            var conflictResult = Assert.IsType<ConflictObjectResult>(result);
            Assert.Equal(StatusCodes.Status409Conflict, conflictResult.StatusCode);

            var response = Assert.IsType<TaskDetailResponseDto>(conflictResult.Value);
            Assert.Equal(StatusCodes.Status404NotFound, response.ErrorCode);
            Assert.Equal(responseDto.ErrorMessage, response.ErrorMessage);
        }

        [Fact]
        public async System.Threading.Tasks.Task UpdateTaskDetailAsync_ShouldReturnInternalServerError_WhenUnhandledExceptionOccurs()
        {
            long taskDetailId = 2001;
            // Arrange
            var updateTaskDetailRequestDto = new TaskDetailRequestDto
            {
                TaskId = 1001,
                TermsOfServiceId = 3001,
                TaskHeader = "Header for Task",
                TaskDescription = "Description for Task",
                LanguageCode = "en-US",
                TenantCode = "tenant-001",
                TaskCtaButtonText = "Click Here"
            };

            _taskDetailRepoMock.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<TaskDetailModel, bool>>>(), false)).ThrowsAsync(new Exception("An error occurred while fetching the task."));

            // Act
            var result = await _taskDetailController.UpdateTaskDetailAsync(taskDetailId, updateTaskDetailRequestDto);

            // Assert
            var errorResult = Assert.IsType<ConflictObjectResult>(result);
            Assert.Equal(StatusCodes.Status409Conflict, errorResult.StatusCode);
            Assert.NotNull(errorResult.Value);
            var responseDto = Assert.IsType<TaskDetailResponseDto>(errorResult.Value);
            Assert.Equal(StatusCodes.Status409Conflict, responseDto.ErrorCode);
        }
    }
}
