using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using SunnyRewards.Helios.Task.Api.Controller;
using SunnyRewards.Helios.Task.Core.Domain.Dtos;
using SunnyRewards.Helios.Task.Core.Domain.Models;
using SunnyRewards.Helios.Task.Infrastructure.Repositories.Interfaces;
using SunnyRewards.Helios.Task.Infrastructure.Services;
using SunnyRewards.Helios.Task.Infrastructure.Services.Interface;
using SunnyRewards.Helios.Task.UnitTest.Fixtures.MockDtos;
using SunnyRewards.Helios.Task.UnitTest.Fixtures.MockRepositories;
using System.Linq.Expressions;
using Xunit;

namespace SunnyRewards.Helios.Task.UnitTest.Controllers
{
    public class TaskCategoryControllerUnitTest
    {
        private readonly Mock<ILogger<TaskCategoryController>> _taskCategoryControllerLogger;
        private readonly Mock<ILogger<TaskCategoryService>> _taskCategoryServiceLogger;
        private readonly Mock<IMapper> _mapper;
        private readonly Mock<ITaskCategoryRepo> _taskCategoryRepository;
        private readonly ITaskCategoryService _taskCategoryService;
        
        private readonly TaskCategoryController _controller;

        public TaskCategoryControllerUnitTest()
        {
            _taskCategoryControllerLogger = new Mock<ILogger<TaskCategoryController>>();
            _taskCategoryServiceLogger = new Mock<ILogger<TaskCategoryService>>();
            _mapper = new Mock<IMapper>();
            _taskCategoryRepository = new TaskCategoryMockRepo();

            _taskCategoryService = new TaskCategoryService(_taskCategoryServiceLogger.Object, _mapper.Object, _taskCategoryRepository.Object);
            _controller = new TaskCategoryController(_taskCategoryControllerLogger.Object, _taskCategoryService);
        }

        [Fact]
        public async void TaskCategoriesAsync_Controller_ShouldReturnOk()
        {
            _mapper.Setup(x => x.Map<TaskCategoryDto>(It.IsAny<TaskTypeModel>())).Returns(new TaskCategoryMockDto());
            var response = await _controller.GetTaskCategoriesAsync();
            var result = response as OkObjectResult;
            Assert.Equal(StatusCodes.Status200OK, result?.StatusCode);
        }

        [Fact]
        public async void TaskCategoriesAsync_Controller_ShouldHandleException()
        {
            var serviceMock = new Mock<ITaskCategoryService>();
            var controller = new TaskCategoryController(_taskCategoryControllerLogger.Object, serviceMock.Object);
            serviceMock.Setup(x => x.GetTaskCategoriesAsync()).ThrowsAsync(new Exception("inner Exception"));
            var response = await controller.GetTaskCategoriesAsync();
            var result = response as ObjectResult;
            Assert.Equal(StatusCodes.Status500InternalServerError, result?.StatusCode);
        }

        [Fact]
        public async void TaskCategoriesAsync_Service_ShouldReturnResponse()
        {
            _mapper.Setup(x => x.Map<TaskTypeDto>(It.IsAny<TaskTypeModel>())).Returns(new TaskTypeMockDto());
            var response = await _taskCategoryService.GetTaskCategoriesAsync();
            Assert.NotNull(response);
        }

        [Fact]
        public async void GetTaskCategoriesAsync_RepositoryReturnsNull_ReturnsErrorResponse()
        {
            // Arrange
            var repositoryMock = new Mock<ITaskCategoryRepo>();
            var loggerMock = new Mock<ILogger<TaskCategoryService>>();
            var mapperMock = new Mock<IMapper>();

            // Simulate the repository returning null
            repositoryMock
                .Setup(x => x.FindAsync(It.IsAny<Expression<Func<TaskCategoryModel, bool>>>(), false))
                .ReturnsAsync((List<TaskCategoryModel>)null);

            var service = new TaskCategoryService(loggerMock.Object, mapperMock.Object, repositoryMock.Object);
            var response = await service.GetTaskCategoriesAsync();

            // Act
            var expectedError = "No task category was found.";

            // Assert
            Assert.NotNull(response);
            Assert.Equal(expectedError, response.ErrorMessage);
        }

        [Fact]
        public async void TaskCategoriesAsync_Service_ShouldHandleRepositoryException()
        {
            var expectedException = new Exception("Test exception");
            _taskCategoryRepository.Setup(x => x.FindAsync(It.IsAny<Expression<Func<TaskCategoryModel, bool>>>(), false)).ThrowsAsync(expectedException);
            var result = await _taskCategoryService.GetTaskCategoriesAsync();
            
            Assert.NotNull(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, result.ErrorCode);
            Assert.Equal("Test exception", result.ErrorMessage);
        }

        [Fact]
        public async void TaskCategoriesAsync_Controller_ShouldReturnNotFound()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<TaskCategoryService>>();
            var serviceMock = new Mock<ITaskCategoryService>();
            var errorMessage = "No task category was found.";
            serviceMock.Setup(x => x.GetTaskCategoriesAsync()).ReturnsAsync(new TaskCategoriesResponseDto
            {
                ErrorCode = StatusCodes.Status404NotFound,
                ErrorMessage = errorMessage
            });
            var controller = new TaskCategoryController(_taskCategoryControllerLogger.Object, serviceMock.Object);

            // Act
            var response = await controller.GetTaskCategoriesAsync();
            var result = Assert.IsType<OkObjectResult>(response);

            // Assert
            Assert.Equal(StatusCodes.Status200OK, result?.StatusCode);
            Assert.NotNull(result?.Value);
            
            var responseDto = Assert.IsType<TaskCategoriesResponseDto>(result.Value);
            Assert.Equal(errorMessage, responseDto.ErrorMessage);
        }
        [Fact]
        public async System.Threading.Tasks.Task ImportTaskCategories_ReturnsOk_WhenImportIsSuccessful()
        {
            // Arrange
            var request = new ImportTaskCategoryRequestDto
            {
                TaskCategories = new List<TaskCategoryDto>
                {
                    new TaskCategoryDto { TaskCategoryCode = "tcc-6766ccde6fc04a5d88a2fc4ba7ce8e29", TaskCategoryName ="Virtual Care"}
                }
            };
            var taskCategoryModel = new TaskCategoryModel()
            {
                TaskCategoryCode = "tcc-6766ccde6fc04a5d88a2fc4ba7ce8e29",
                TaskCategoryName = "Virtual Care"
            };

            _mapper.Setup(x => x.Map<TaskCategoryModel>(It.IsAny<TaskCategoryDto>())).Returns(taskCategoryModel);
            var taskCategoryRepo = new Mock<ITaskCategoryRepo>();
            var service = new TaskCategoryService(_taskCategoryServiceLogger.Object, _mapper.Object, taskCategoryRepo.Object);
            var controller = new TaskCategoryController(_taskCategoryControllerLogger.Object, service);

            // Act
            var result = await controller.ImportTaskCategoriesAsync(request);

            // Assert
            var okObjectResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okObjectResult);
            Assert.Equal(StatusCodes.Status200OK, okObjectResult.StatusCode);
        }
        [Fact]
        public async System.Threading.Tasks.Task ImportTaskCategories_ReturnsOk_WhenImportIsSuccessful_With_Existing_Task_Types()
        {
            // Arrange
            var request = new ImportTaskCategoryRequestDto
            {
                TaskCategories = new List<TaskCategoryDto>
                {
                    new TaskCategoryDto { TaskCategoryCode = "tcc-6766ccde6fc04a5d88a2fc4ba7ce8e29", TaskCategoryName ="Virtual Care"}
                }
            };

            // Act
            var result = await _controller.ImportTaskCategoriesAsync(request);

            // Assert
            var okObjectResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okObjectResult);
            Assert.Equal(StatusCodes.Status200OK, okObjectResult.StatusCode);
        }

        [Fact]
        public async System.Threading.Tasks.Task ImportTaskCategories_ReturnsPartialContent_WhenSomeErrorsOccurred()
        {
            // Arrange
            var request = new ImportTaskCategoryRequestDto
            {
                TaskCategories = new List<TaskCategoryDto>
                {
                    new TaskCategoryDto { TaskCategoryCode = "tcc-6766ccde6fc04a5d88a2fc4ba7ce8e29", TaskCategoryName ="Virtual Care"}
                }
            };
            var taskCategoryModel = new TaskCategoryModel()
            {
                TaskCategoryCode = "tcc-6766ccde6fc04a5d88a2fc4ba7ce8e29",
                TaskCategoryName = "Virtual Care"
            };

            _mapper.Setup(x => x.Map<TaskCategoryModel>(It.IsAny<TaskCategoryDto>())).Returns(taskCategoryModel);
            var taskCategoryRepo = new Mock<ITaskCategoryRepo>();
            var service = new TaskCategoryService(_taskCategoryServiceLogger.Object, _mapper.Object, taskCategoryRepo.Object);
            var controller = new TaskCategoryController(_taskCategoryControllerLogger.Object, service);
            taskCategoryRepo.Setup(x => x.CreateAsync(It.IsAny<TaskCategoryModel>())).ThrowsAsync(new Exception("Duplicate key violation"));

            // Act
            var result = await controller.ImportTaskCategoriesAsync(request);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.NotNull(objectResult);
            Assert.Equal(StatusCodes.Status206PartialContent, objectResult.StatusCode);
        }

        [Fact]
        public async System.Threading.Tasks.Task ImportTaskCategories_ReturnsInternalServerError_WhenExceptionThrown()
        {
            // Arrange
            var request = new ImportTaskCategoryRequestDto
            {
                TaskCategories = new List<TaskCategoryDto>
                {
                    new TaskCategoryDto { TaskCategoryCode = "tcc-6766ccde6fc04a5d88a2fc4ba7ce8e29", TaskCategoryName ="Virtual Care"}
                }
            };
            var service = new Mock<ITaskCategoryService>();
            var controller = new TaskCategoryController(_taskCategoryControllerLogger.Object, service.Object);
            service.Setup(x => x.ImportTaskCategoriesAsync(It.IsAny<ImportTaskCategoryRequestDto>())).Throws(new Exception("Testing"));

            // Act
            var result = await controller.ImportTaskCategoriesAsync(request);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.NotNull(objectResult);
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
        }
    }
}