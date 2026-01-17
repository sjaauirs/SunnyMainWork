using AutoMapper;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NHibernate.Cfg;
using SunnyRewards.Helios.Task.Api.Controller;
using SunnyRewards.Helios.Task.Core.Domain.Dtos;
using SunnyRewards.Helios.Task.Core.Domain.Models;
using SunnyRewards.Helios.Task.Infrastructure.Mappings;
using SunnyRewards.Helios.Task.Infrastructure.Repositories.Interfaces;
using SunnyRewards.Helios.Task.Infrastructure.Services;
using SunnyRewards.Helios.Task.Infrastructure.Services.Interface;
using SunnyRewards.Helios.Task.UnitTest.Fixtures.MockDtos;
using SunnyRewards.Helios.Task.UnitTest.Fixtures.MockRepositories;
using System.Linq.Expressions;
using Xunit;

namespace SunnyRewards.Helios.Task.UnitTest.Controllers
{
    public class TaskTypeControllerUnitTest
    {
        private readonly Mock<ILogger<TaskTypeController>> _taskTypeLogger;
        private readonly Mock<ILogger<TaskTypeService>> _taskTypeServiceLogger;
        private readonly Mock<IMapper> _mapper;
        private readonly Mock<ITaskTypeRepo> _taskTypeRepo;
        private readonly ITaskTypeService _taskTypeService;
        private readonly TaskTypeController _taskTypeController;

        public TaskTypeControllerUnitTest()
        {
            _taskTypeLogger = new Mock<ILogger<TaskTypeController>>();
            _taskTypeServiceLogger = new Mock<ILogger<TaskTypeService>>();
            _mapper = new Mock<IMapper>();
            _taskTypeRepo = new TaskTypeMockRepo();
            _taskTypeService = new TaskTypeService(_taskTypeServiceLogger.Object, _mapper.Object, _taskTypeRepo.Object);
            _taskTypeController = new TaskTypeController(_taskTypeLogger.Object, _taskTypeService);
        }

        [Fact]
        public async void Should_GetTaskTypeById_Controller()
        {
            long taskTypeId = 2;
            _mapper.Setup(x => x.Map<TaskTypeDto>(It.IsAny<TaskTypeModel>())).Returns(new TaskTypeMockDto());
            var response = await _taskTypeController.GetTaskTypeById(taskTypeId);
            var result = response?.Result as OkObjectResult;
            Assert.True(result?.StatusCode == 200);
        }

        [Fact]
        public async void Should_GetTaskTypeById_Controller_Exception()
        {
            long taskTypeId = 2;
            var serviceMock = new Mock<ITaskTypeService>();
            var controller = new TaskTypeController(_taskTypeLogger.Object, serviceMock.Object);
            serviceMock.Setup(x => x.GetTaskTypeById(taskTypeId)).ThrowsAsync(new Exception("inner Exception"));
            var response = await controller.GetTaskTypeById(taskTypeId);
            Assert.True(response == null);
        }

        [Fact]
        public async void Should_GetTaskTypeById_Service()
        {
            long taskTypeId = 2;
            _mapper.Setup(x => x.Map<TaskTypeDto>(It.IsAny<TaskTypeModel>())).Returns(new TaskTypeMockDto());
            var response = await _taskTypeService.GetTaskTypeById(taskTypeId);
            Assert.True(response != null);
        }

        [Fact]
        public async void Should_GetTaskTypeById_Service_Id_Zero()
        {
            long taskTypeId = 0;
            _mapper.Setup(x => x.Map<TaskTypeDto>(It.IsAny<TaskTypeModel>())).Returns(new TaskTypeMockDto());
            var response = await _taskTypeController.GetTaskTypeById(taskTypeId);
            var result = response?.Result as NotFoundObjectResult;
            Assert.True(result?.StatusCode == 404);
        }

        [Fact]
        public async void Should_GetTaskTypeById_Service_Exception()
        {
            long taskTypeId = 1;
            var expectedException = new Exception("Test exception");
            _taskTypeRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<TaskTypeModel, bool>>>(), false)).ThrowsAsync(expectedException);
            var result = await _taskTypeService.GetTaskTypeById(taskTypeId);
            Assert.NotNull(result);
        }

        [Fact]
        public async void Should_GetTaskTypeByCode_Controller()
        {
            var taskTypeCode = "typ-cdfjdxjfvj5654656";
            _mapper.Setup(x => x.Map<TaskTypeDto>(It.IsAny<TaskTypeModel>())).Returns(new TaskTypeMockDto());
            var response = await _taskTypeController.GetTaskTypeByCode(taskTypeCode);
            var result = response?.Result as OkObjectResult;
            Assert.True(result?.StatusCode == 200);
        }

        [Fact]
        public async void Should_GetTaskTypeByCode_Controller_Exception()
        {
            var taskTypeCode = "typ-cdfjdxjfvj5654656";
            var serviceMock = new Mock<ITaskTypeService>();
            var controller = new TaskTypeController(_taskTypeLogger.Object, serviceMock.Object);
            serviceMock.Setup(x => x.GetTaskTypeByTypeCode(taskTypeCode)).ThrowsAsync(new Exception("inner Exception"));
            var response = await controller.GetTaskTypeByCode(taskTypeCode);
            Assert.True(response == null);
        }

        [Fact]
        public async void Should_GetTaskTypeByCode_Service()
        {
            var taskTypeCode = "typ-cdfjdxjfvj5654656";
            _mapper.Setup(x => x.Map<TaskTypeDto>(It.IsAny<TaskTypeModel>())).Returns(new TaskTypeMockDto());
            var response = await _taskTypeService.GetTaskTypeByTypeCode(taskTypeCode);
            Assert.True(response != null);
        }

        [Fact]
        public async void Should_GetTaskTypeByCode_Service_NullCheck()
        {
            var taskTypeCode = "";
            _mapper.Setup(x => x.Map<TaskTypeDto>(It.IsAny<TaskTypeModel>())).Returns(new TaskTypeMockDto());
            var response = await _taskTypeController.GetTaskTypeByCode(taskTypeCode);
            var result = response?.Result as NotFoundObjectResult;
            Assert.True(result?.StatusCode == 404);
        }

        [Fact]
        public async void Should_GetTaskTypeByCode_Service_Exception()
        {
            var taskTypeCode = "typ-cdfjdxjfvj5654656";
            _taskTypeRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<TaskTypeModel, bool>>>(), false)).ThrowsAsync(new Exception("inner Exception"));
            var result = await _taskTypeService.GetTaskTypeByTypeCode(taskTypeCode);
            Assert.NotNull(result);
        }

        [Fact]
        public void Should_return_TaskTypeMap()
        {
            Configuration _configuration;
            _configuration = Fluently.Configure()
                    .Database(SQLiteConfiguration.Standard.InMemory().ShowSql())
                    .Mappings(m => m.FluentMappings.Add<TaskTypeMap>())
                    .BuildConfiguration();
        }

        [Fact]
        public async void GetTaskTypesAsync_Controller_ShouldReturnOk()
        {
            _mapper.Setup(x => x.Map<TaskTypeDto>(It.IsAny<TaskTypeModel>())).Returns(new TaskTypeMockDto());
            var response = await _taskTypeController.GetTaskTypesAsync();
            var result = response as OkObjectResult;
            Assert.Equal(StatusCodes.Status200OK, result?.StatusCode);
        }

        [Fact]
        public async void GetTaskTypesAsync_Controller_ShouldHandleException()
        {
            var serviceMock = new Mock<ITaskTypeService>();
            var controller = new TaskTypeController(_taskTypeLogger.Object, serviceMock.Object);
            serviceMock.Setup(x => x.GetTaskTypesAsync()).ThrowsAsync(new Exception("inner Exception"));
            var response = await controller.GetTaskTypesAsync();
            var result = response as ObjectResult;
            Assert.Equal(StatusCodes.Status500InternalServerError, result?.StatusCode);
        }

        [Fact]
        public async void GetTaskTypesAsync_Service_ShouldReturnResponse()
        {
            _mapper.Setup(x => x.Map<TaskTypeDto>(It.IsAny<TaskTypeModel>())).Returns(new TaskTypeMockDto());
            var response = await _taskTypeService.GetTaskTypesAsync();
            Assert.NotNull(response);
        }

        [Fact]
        public async void GetGetTaskTypesAsync_RepositoryReturnsNull_ReturnsErrorResponse()
        {
            // Arrange
            var repositoryMock = new Mock<ITaskTypeRepo>();
            var loggerMock = new Mock<ILogger<TaskTypeService>>();
            var mapperMock = new Mock<IMapper>();

            // Simulate the repository returning null
            repositoryMock
                .Setup(x => x.FindAsync(It.IsAny<Expression<Func<TaskTypeModel, bool>>>(), false))
                .ReturnsAsync((List<TaskTypeModel>)null);

            var service = new TaskTypeService(loggerMock.Object, mapperMock.Object, repositoryMock.Object);
            var response = await service.GetTaskTypesAsync();

            // Act
            var expectedError = "No task type was found.";

            // Assert
            Assert.NotNull(response);
            Assert.Equal(expectedError, response.ErrorMessage);
        }

        [Fact]
        public async void GetTaskTypesAsync_Service_ShouldHandleRepositoryException()
        {
            var expectedException = new Exception("Test exception");
            _taskTypeRepo.Setup(x => x.FindAsync(It.IsAny<Expression<Func<TaskTypeModel, bool>>>(), false)).ThrowsAsync(expectedException);
            var result = await _taskTypeService.GetTaskTypesAsync();

            Assert.NotNull(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, result.ErrorCode);
            Assert.Equal("Test exception", result.ErrorMessage);
        }

        [Fact]
        public async void GetTaskTypesAsync_Controller_ShouldReturnNotFound()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<TaskTypeService>>();
            var serviceMock = new Mock<ITaskTypeService>();
            var errorMessage = "No task type was found.";
            serviceMock.Setup(x => x.GetTaskTypesAsync()).ReturnsAsync(new TaskTypesResponseDto
            {
                ErrorMessage = errorMessage
            });
            var controller = new TaskTypeController(_taskTypeLogger.Object, serviceMock.Object);

            // Act
            var response = await controller.GetTaskTypesAsync();
            var result = Assert.IsType<OkObjectResult>(response);

            // Assert
            Assert.Equal(StatusCodes.Status200OK, result?.StatusCode);
            Assert.Equal(StatusCodes.Status200OK, result?.StatusCode);
            Assert.NotNull(result?.Value);

            var responseDto = Assert.IsType<TaskTypesResponseDto>(result.Value);
            Assert.Equal(errorMessage, responseDto.ErrorMessage);
        }

        [Fact]
        public async System.Threading.Tasks.Task ImportTaskTypes_ReturnsOk_WhenImportIsSuccessful()
        {
            // Arrange
            var request = new ImportTaskTypeRequestDto
            {
                TaskTypes = new List<TaskTypeDto>
                {
                    new TaskTypeDto { TaskTypeCode = "tsk-8b5e2f02-2d2f-4a74-80c3-62d59e4ad2cf", TaskTypeName = "Fitness" }
                }
            };
            var taskTypeModel = new TaskTypeModel() 
            { 
                TaskTypeCode = "tsk-8b5e2f02-2d2f-4a74-80c3-62d59e4ad2cf",
                TaskTypeName = "Fitness"
            };

            _mapper.Setup(x => x.Map<TaskTypeModel>(It.IsAny<TaskTypeDto>())).Returns(taskTypeModel);
            var taskTypeRepo = new Mock<ITaskTypeRepo>();
            var service = new TaskTypeService(_taskTypeServiceLogger.Object, _mapper.Object, taskTypeRepo.Object);
            var controller = new TaskTypeController(_taskTypeLogger.Object, service);

            // Act
            var result = await controller.ImportTaskTypesAsync(request);

            // Assert
            var okObjectResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okObjectResult);
            Assert.Equal(StatusCodes.Status200OK, okObjectResult.StatusCode);
        }
        [Fact]
        public async System.Threading.Tasks.Task ImportTaskTypes_ReturnsOk_WhenImportIsSuccessful_With_Existing_Task_Types()
        {
            // Arrange
            var request = new ImportTaskTypeRequestDto
            {
                TaskTypes = new List<TaskTypeDto>
                {
                    new TaskTypeDto { TaskTypeCode = "tsk-8b5e2f02-2d2f-4a74-80c3-62d59e4ad2cf", TaskTypeName = "Fitness" }
                }
            };

            // Act
            var result = await _taskTypeController.ImportTaskTypesAsync(request);

            // Assert
            var okObjectResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okObjectResult);
            Assert.Equal(StatusCodes.Status200OK, okObjectResult.StatusCode);
        }

        [Fact]
        public async System.Threading.Tasks.Task ImportTaskTypes_ReturnsPartialContent_WhenSomeErrorsOccurred()
        {
            // Arrange
            var request = new ImportTaskTypeRequestDto
            {
                TaskTypes = new List<TaskTypeDto>
                {
                    new TaskTypeDto { TaskTypeCode = "tsk-8b5e2f02-2d2f-4a74-80c3-62d59e4adc87", TaskTypeName = "HealthyEating" }
                }
            };
            var taskTypeModel = new TaskTypeModel()
            {
                TaskTypeCode = "tsk-8b5e2f02-2d2f-4a74-80c3-62d59e4ad2cf",
                TaskTypeName = "Fitness"
            };

            _mapper.Setup(x => x.Map<TaskTypeModel>(It.IsAny<TaskTypeDto>())).Returns(taskTypeModel);
            var taskTypeRepo = new Mock<ITaskTypeRepo>();
            var service = new TaskTypeService(_taskTypeServiceLogger.Object, _mapper.Object, taskTypeRepo.Object);
            var controller = new TaskTypeController(_taskTypeLogger.Object, service);
            taskTypeRepo.Setup(x => x.CreateAsync(It.IsAny<TaskTypeModel>())).ThrowsAsync(new Exception("Duplicate key violation "));

            // Act
            var result = await controller.ImportTaskTypesAsync(request);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.NotNull(objectResult);
            Assert.Equal(StatusCodes.Status206PartialContent, objectResult.StatusCode);
        }

        [Fact]
        public async System.Threading.Tasks.Task ImportTaskTypes_ReturnsInternalServerError_WhenExceptionThrown()
        {
            // Arrange
            var request = new ImportTaskTypeRequestDto
            {
                TaskTypes = new List<TaskTypeDto>
                {
                    new TaskTypeDto { TaskTypeCode = "tsk-8b5e2f02-2d2f-4a74-80c3-62d59e4ad", TaskTypeName = "Wellness" }
                }
            };
            var service = new Mock<ITaskTypeService>();
            var controller = new TaskTypeController(_taskTypeLogger.Object, service.Object);
            service.Setup(x => x.ImportTaskTypesAsync(It.IsAny<ImportTaskTypeRequestDto>())).Throws(new Exception("Testing"));

            // Act
            var result = await controller.ImportTaskTypesAsync(request);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.NotNull(objectResult);
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
        }
    }
}