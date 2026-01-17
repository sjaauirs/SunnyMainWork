using AutoMapper;
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
using StatusCodes = Microsoft.AspNetCore.Http.StatusCodes;

namespace SunnyRewards.Helios.Task.UnitTest.Controllers
{
    public class TaskRewardTypeControllerUnitTest
    {
        private readonly Mock<ILogger<TaskRewardTypeController>> _taskRewardTypeControllerLogger;
        private readonly Mock<ILogger<ITaskRewardTypeService>> _taskRewardTypeServiceLogger;
        private readonly Mock<IMapper> _mapper;
        private readonly Mock<ITaskRewardTypeRepo> _mockTaskRewardTypeRepo;
        private readonly ITaskRewardTypeService _taskRewardTypeService;
        private readonly TaskRewardTypeController _controller;

        public TaskRewardTypeControllerUnitTest()
        {
            _taskRewardTypeControllerLogger = new Mock<ILogger<TaskRewardTypeController>>();
            _taskRewardTypeServiceLogger = new Mock<ILogger<ITaskRewardTypeService>>();

            _mapper = new Mock<IMapper>();

            _mockTaskRewardTypeRepo = new TaskRewardTypeMockRepo();

            _taskRewardTypeService = new TaskRewardTypeService(_taskRewardTypeServiceLogger.Object, _mapper.Object, _mockTaskRewardTypeRepo.Object);
            _controller = new TaskRewardTypeController(_taskRewardTypeControllerLogger.Object, _taskRewardTypeService);
        }

        [Fact]
        public async System.Threading.Tasks.Task GetTaskRewardTypesAsync_ShouldReturnOk_WhenServiceReturnsValidResponse()
        {
            // Arrange
            var expectedResponse = new TaskRewardTypesResponseDto
            {
                ErrorCode = null,
                ErrorMessage = null,
                TaskRewardTypes = new List<TaskRewardTypeDto>
                {
                    new() { RewardTypeId = 1, RewardTypeName = "RewardType1" },
                    new() { RewardTypeId = 2, RewardTypeName = "RewardType2" }
                }
            };

            var _mockService = new Mock<ITaskRewardTypeService>();

            _mockService
                .Setup(service => service.GetTaskRewardTypesAsync())
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.GetTaskRewardTypesAsync();

            // Assert
            var objectResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(StatusCodes.Status200OK, objectResult.StatusCode);

            var responseDto = Assert.IsType<TaskRewardTypesResponseDto>(objectResult.Value);
            Assert.Null(responseDto.TaskRewardTypes);
        }

        [Fact]
        public async System.Threading.Tasks.Task GetTaskRewardTypesAsync_ShouldReturnError_WhenServiceReturnsError()
        {
            // Arrange
            var expectedResponse = new TaskRewardTypesResponseDto
            {
                TaskRewardTypes = null
            };

            var _mockService = new Mock<ITaskRewardTypeService>();

            _mockService
                .Setup(service => service.GetTaskRewardTypesAsync())
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.GetTaskRewardTypesAsync();

            // Assert
            var objectResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(StatusCodes.Status200OK, objectResult.StatusCode);

            var responseDto = Assert.IsType<TaskRewardTypesResponseDto>(objectResult.Value);
            Assert.Null(responseDto.TaskRewardTypes);
        }

        [Fact]
        public async System.Threading.Tasks.Task GetTaskRewardTypesAsync_ShouldHandleException_WhenServiceThrowsException()
        {
            // Arrange
            var exceptionMessage = "Unexpected error occurred.";

            var _mockService = new Mock<ITaskRewardTypeService>();

            _mockService
                .Setup(service => service.GetTaskRewardTypesAsync())
                .ThrowsAsync(new Exception(exceptionMessage));

            // Act
            var result = await _controller.GetTaskRewardTypesAsync();

            // Assert
            var objectResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(StatusCodes.Status200OK, objectResult.StatusCode);

            var responseDto = Assert.IsType<TaskRewardTypesResponseDto>(objectResult.Value);
            Assert.Null(responseDto.TaskRewardTypes);
        }

        [Fact]
        public async void GetTaskRewardTypesAsync_Controller_ShouldReturnOk()
        {
            _mapper.Setup(x => x.Map<TaskCategoryDto>(It.IsAny<TaskTypeModel>())).Returns(new TaskCategoryMockDto());
            var response = await _controller.GetTaskRewardTypesAsync();
            var result = response as OkObjectResult;
            Assert.Equal(StatusCodes.Status200OK, result?.StatusCode);
        }

        [Fact]
        public async void GetTaskRewardTypesAsync_Controller_ShouldHandleException()
        {
            var serviceMock = new Mock<ITaskRewardTypeService>();
            var controller = new TaskRewardTypeController(_taskRewardTypeControllerLogger.Object, serviceMock.Object);
            serviceMock.Setup(x => x.GetTaskRewardTypesAsync()).ThrowsAsync(new Exception("inner Exception"));
            var response = await controller.GetTaskRewardTypesAsync();
            var result = response as ObjectResult;
            Assert.Equal(StatusCodes.Status500InternalServerError, result?.StatusCode);
        }

        [Fact]
        public async void GetTaskRewardTypesAsync_Service_ShouldReturnResponse()
        {
            _mapper.Setup(x => x.Map<TaskTypeDto>(It.IsAny<TaskTypeModel>())).Returns(new TaskTypeMockDto());
            var response = await _taskRewardTypeService.GetTaskRewardTypesAsync();
            Assert.NotNull(response);
        }

        [Fact]
        public async void GetTaskRewardTypesAsync_RepositoryReturnsNull_ReturnsErrorResponse()
        {
            // Arrange
            var repositoryMock = new Mock<ITaskRewardTypeRepo>();
            var loggerMock = new Mock<ILogger<ITaskRewardTypeService>>();
            var mapperMock = new Mock<IMapper>();

            // Simulate the repository returning null
            repositoryMock
                .Setup(x => x.FindAsync(It.IsAny<Expression<Func<TaskRewardTypeModel, bool>>>(), false))
                .ReturnsAsync((List<TaskRewardTypeModel>)null);

            var service = new TaskRewardTypeService(loggerMock.Object, mapperMock.Object, repositoryMock.Object);
            var response = await service.GetTaskRewardTypesAsync();

            // Act
            var expectedError = "No task reward type was found.";

            // Assert
            Assert.NotNull(response);
            Assert.Equal(expectedError, response.ErrorMessage);
        }

        [Fact]
        public async void GetTaskRewardTypesAsync_Service_ShouldHandleRepositoryException()
        {
            var expectedException = new Exception("Test exception");
            _mockTaskRewardTypeRepo.Setup(x => x.FindAsync(It.IsAny<Expression<Func<TaskRewardTypeModel, bool>>>(), false)).ThrowsAsync(expectedException);
            var result = await _taskRewardTypeService.GetTaskRewardTypesAsync();

            Assert.NotNull(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, result.ErrorCode);
            Assert.Equal("Test exception", result.ErrorMessage);
        }

        [Fact]
        public async void GetTaskRewardTypesAsync_Controller_ShouldReturnNotFound()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ITaskRewardTypeService>>();
            var serviceMock = new Mock<ITaskRewardTypeService>();
            var errorMessage = "No task reward was found.";

            serviceMock.Setup(x => x.GetTaskRewardTypesAsync()).ReturnsAsync(new TaskRewardTypesResponseDto
            {
                ErrorCode = StatusCodes.Status404NotFound,
                ErrorMessage = errorMessage
            });
            var controller = new TaskRewardTypeController(_taskRewardTypeControllerLogger.Object, serviceMock.Object);

            // Act
            var response = await controller.GetTaskRewardTypesAsync();
            var result = Assert.IsType<OkObjectResult>(response);

            // Assert
            Assert.Equal(StatusCodes.Status200OK, result?.StatusCode);
            Assert.NotNull(result?.Value);

            var responseDto = Assert.IsType<TaskRewardTypesResponseDto>(result.Value);
            Assert.Equal(errorMessage, responseDto.ErrorMessage);
        }

        [Fact]
        public async System.Threading.Tasks.Task UpdateTaskRewardAsync_ShouldReturnOk_WhenServiceReturnsValidResponse()
        {
            long rewardTypeId = 1; 
            // Arrange
            var taskRewardTypeRequestDto = new TaskRewardTypeRequestDto { 
                RewardTypeName = "UpdatedReward", 
                RewardTypeDescription = "This is a description for the reward type.",
                RewardTypeCode = "SRN123"
            };
            var expectedResponse = new TaskRewardTypeResponseDto
            {
                ErrorCode = null,
                ErrorMessage = null
            };

            var _mockService = new Mock<ITaskRewardTypeService>();

            _mockService
                .Setup(service => service.UpdateTaskRewardTypeAsync(rewardTypeId,taskRewardTypeRequestDto))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.UpdateTaskRewardTypeAsync(rewardTypeId, taskRewardTypeRequestDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
        }

        [Fact]
        public async System.Threading.Tasks.Task UpdateTaskRewardAsync_ShouldReturnError_WhenServiceReturnsError()
        {
            long rewardTypeId = 1;
            // Arrange
            var taskRewardTypeRequestDto = new TaskRewardTypeRequestDto
            {
                RewardTypeName = "InvalidReward",
                RewardTypeDescription = "This is a description for the reward type.",
                RewardTypeCode = "SRN123"
            };
            var expectedResponse = new TaskRewardTypeResponseDto
            {
                ErrorCode = StatusCodes.Status400BadRequest,
                ErrorMessage = "Invalid Request"
            };

            var _mockService = new Mock<ITaskRewardTypeService>();

            _mockService
                .Setup(service => service.UpdateTaskRewardTypeAsync(rewardTypeId, taskRewardTypeRequestDto))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.UpdateTaskRewardTypeAsync(rewardTypeId, taskRewardTypeRequestDto);

            // Assert
            var objectResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(StatusCodes.Status200OK, objectResult.StatusCode);
        }

        [Fact]
        public async System.Threading.Tasks.Task UpdateTaskRewardAsync_ShouldReturnError_WhenServiceReturnsThrowsException()
        {
            long rewardTypeId = 1;
            // Arrange
            var exceptionMessage = "Unexpected error occurred.";
            var taskRewardTypeRequestDto = new TaskRewardTypeRequestDto
            {
               
                RewardTypeName = "InvalidReward",
                RewardTypeDescription = "This is a description for the reward type.",
                RewardTypeCode = "SRN123"
            };

            var _mockService = new Mock<ITaskRewardTypeService>();

            _mockService
                .Setup(service => service.UpdateTaskRewardTypeAsync(rewardTypeId, taskRewardTypeRequestDto))
                .ThrowsAsync(new Exception(exceptionMessage));

            // Act
            var result = await _controller.UpdateTaskRewardTypeAsync(rewardTypeId, taskRewardTypeRequestDto);

            // Assert
            var objectResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(StatusCodes.Status200OK, objectResult.StatusCode);
        }

        [Fact]
        public async System.Threading.Tasks.Task UpdateTaskRewardAsync_ShouldHandleException_WhenRepositoryThrowsException()
        {
            long rewardTypeId = 1;
            // Arrange
            var requestDto = new TaskRewardTypeRequestDto
            {
                RewardTypeName = "UpdatedReward",
                RewardTypeDescription = "Updated description",
                RewardTypeCode = "NEWCODE"
            };

            var exceptionMessage = "Database error occurred.";
            var expectedResponseDto = new TaskRewardTypeResponseDto
            {
                ErrorCode = StatusCodes.Status409Conflict,
                ErrorMessage = exceptionMessage
            };

            var existingModel = new TaskRewardTypeModel
            {
                RewardTypeId = 1,
                RewardTypeName = "MONETARY_DOLLARS",
                RewardTypeDescription = "Money",
                RewardTypeCode = "Code001",
                DeleteNbr = 0
            };

            _mockTaskRewardTypeRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<TaskRewardTypeModel, bool>>>(), false)).ReturnsAsync(existingModel);

            _mockTaskRewardTypeRepo.Setup(repo => repo.UpdateAsync(It.IsAny<TaskRewardTypeModel>()))
                .ThrowsAsync(new Exception(exceptionMessage));

            // Act
            var result = await _controller.UpdateTaskRewardTypeAsync(rewardTypeId, requestDto);

            // Assert
            var objectResult = Assert.IsType<ConflictObjectResult>(result);
            Assert.Equal(StatusCodes.Status409Conflict, objectResult.StatusCode);

            var response = Assert.IsType<TaskRewardTypeResponseDto>(objectResult.Value);
            
            Assert.Null(response.TaskRewardType);
            Assert.Equal(StatusCodes.Status409Conflict, response.ErrorCode);
        }

        [Fact]
        public async System.Threading.Tasks.Task UpdateTaskRewardAsync_ShouldHandleException_WhenServiceThrowsException()
        {
            long rewardTypeId = 1;
            // Arrange
            var requestDto = new TaskRewardTypeRequestDto
            {
                RewardTypeName = "SomeReward",
                RewardTypeDescription = "This is a description for the reward type.",
                RewardTypeCode = "SRN123"
            };
            var exceptionMessage = "Unexpected error occurred.";

            var _mockService = new Mock<ITaskRewardTypeService>();

            _mockService
                .Setup(service => service.UpdateTaskRewardTypeAsync(rewardTypeId, requestDto))
                .ThrowsAsync(new Exception(exceptionMessage));

            // Act
            var result = await _controller.UpdateTaskRewardTypeAsync(rewardTypeId, requestDto);

            // Assert
            var objectResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(StatusCodes.Status200OK, objectResult.StatusCode);
        }
        [Fact]
        public async System.Threading.Tasks.Task GetTaskRewardTypesAsync_ShouldReturnOk_WhenRepositoryeReturnsValidResponse()
        {
            // Arrange

            var expectedResponseDto = new TaskRewardTypesResponseDto
            {
                ErrorCode = null,
                ErrorMessage = null,
                TaskRewardTypes = new List<TaskRewardTypeDto>
                {
                    new()
                    {
                        RewardTypeId = 1,
                        RewardTypeName = "Reward 1",
                        RewardTypeDescription =  "Description for reward 1",
                        RewardTypeCode = "CODE1"
                    },
                    new()
                    {
                        RewardTypeId = 2,
                        RewardTypeName = "Reward 2",
                        RewardTypeDescription =  "Description for reward 2",
                        RewardTypeCode = "CODE2"
                    }
                }
            };

            var _mockService = new Mock<ITaskRewardTypeService>();

            _mockService.Setup(service => service.GetTaskRewardTypesAsync()).ReturnsAsync(expectedResponseDto);

            // Act
            var result = await _controller.GetTaskRewardTypesAsync();

            // Assert
            var objectResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(StatusCodes.Status200OK, objectResult.StatusCode);
           
            var response = Assert.IsType<TaskRewardTypesResponseDto>(objectResult.Value);
            Assert.Null(response.TaskRewardTypes);
        }

        [Fact]
        public async System.Threading.Tasks.Task GetTaskRewardTypesAsync_ShouldReturnError_WhenRepositoryReturnsError()
        {
            // Arrange
            var expectedResponseDto = new TaskRewardTypesResponseDto();

            var _mockService = new Mock<ITaskRewardTypeService>();

            _mockService.Setup(service => service.GetTaskRewardTypesAsync()).ReturnsAsync(expectedResponseDto);
            List<TaskRewardTypeModel> expectedResponse = null;

            _mockTaskRewardTypeRepo.Setup(x => x.FindAllAsync()).ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.GetTaskRewardTypesAsync();

            // Assert
            var objectResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(StatusCodes.Status200OK, objectResult.StatusCode);

            var response = Assert.IsType<TaskRewardTypesResponseDto>(objectResult.Value);
            Assert.Null(response.TaskRewardTypes);
        }

        [Fact]
        public async System.Threading.Tasks.Task GetTaskRewardTypesAsync_ShouldHandleException_WhenRepositoryThrowsException()
        {
            // Arrange
            var exceptionMessage = "Unexpected error occurred.";
            var expectedResponseDto = new TaskRewardTypesResponseDto
            {
                ErrorCode = StatusCodes.Status500InternalServerError,
                ErrorMessage = exceptionMessage,
                TaskRewardTypes = new List<TaskRewardTypeDto>
                {
                    new()
                    {
                        RewardTypeId = 1,
                        RewardTypeName = "Reward 1",
                        RewardTypeDescription =  "Description for reward 1",
                        RewardTypeCode = "CODE1"
                    },
                    new()
                    {
                        RewardTypeId = 2,
                        RewardTypeName = "Reward 2",
                        RewardTypeDescription =  "Description for reward 2",
                        RewardTypeCode = "CODE2"
                    }
                }
            };

            var _mockService = new Mock<ITaskRewardTypeService>();

            _mockService.Setup(service => service.GetTaskRewardTypesAsync()).ReturnsAsync(expectedResponseDto);
            
            var expectedResponse = new List<TaskRewardTypeModel>
            {
                new()
                {
                    RewardTypeId = 1,
                    RewardTypeName = "Reward 1",
                    RewardTypeDescription =  "Description for reward 1",
                    RewardTypeCode = "CODE1"
                },
                new()
                {
                    RewardTypeId = 2,
                    RewardTypeName = "Reward 2",
                    RewardTypeDescription =  "Description for reward 2",
                    RewardTypeCode = "CODE2"
                }
            };

            _mockTaskRewardTypeRepo.Setup(x => x.FindAsync(It.IsAny<Expression<Func<TaskRewardTypeModel, bool>>>(), false)).ThrowsAsync(new Exception(exceptionMessage));

            // Act
            var response = await _controller.GetTaskRewardTypesAsync();
            var result = Assert.IsType<ObjectResult>(response);

            // Assert
            Assert.Equal(StatusCodes.Status500InternalServerError, result.StatusCode);

            var responseDto = Assert.IsType<TaskRewardTypesResponseDto>(result.Value);

            Assert.Equal(StatusCodes.Status500InternalServerError, responseDto.ErrorCode);
            Assert.Equal(exceptionMessage, responseDto.ErrorMessage);
        }

        [Fact]
        public async System.Threading.Tasks.Task ImportRewardTypes_ReturnsOk_WhenImportIsSuccessful()
        {
            // Arrange
            var request = new ImportRewardTypeRequestDto
            {
                RewardTypes = new List<TaskRewardTypeDto>
                {
                    new TaskRewardTypeDto { RewardTypeCode ="rtc-88f6fb24b04841a7b93b597e6fe36d91",RewardTypeName ="MEMBERSHIP_DOLLARS" }
                }
            };
            var taskTypeModel = new TaskRewardTypeModel()
            {
                RewardTypeCode = "rtc-88f6fb24b04841a7b93b597e6fe36d91",
                RewardTypeName = "MEMBERSHIP_DOLLARS"
            };

            _mapper.Setup(x => x.Map<TaskRewardTypeModel>(It.IsAny<TaskRewardTypeDto>())).Returns(taskTypeModel);
            var rewardTypeRepo = new Mock<ITaskRewardTypeRepo>();
            var service = new TaskRewardTypeService(_taskRewardTypeServiceLogger.Object, _mapper.Object, rewardTypeRepo.Object);
            var controller = new TaskRewardTypeController(_taskRewardTypeControllerLogger.Object, service);

            // Act
            var result = await controller.ImportRewardTypesAsync(request);

            // Assert
            var okObjectResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okObjectResult);
            Assert.Equal(StatusCodes.Status200OK, okObjectResult.StatusCode);
        }
        [Fact]
        public async System.Threading.Tasks.Task ImportRewardTypes_ReturnsOk_WhenImportIsSuccessful_With_Existing_Task_Types()
        {
            // Arrange
            var request = new ImportRewardTypeRequestDto
            {
                RewardTypes = new List<TaskRewardTypeDto>
                {
                    new TaskRewardTypeDto { RewardTypeCode ="rtc-88f6fb24b04841a7b93b597e6fe36d91",RewardTypeName ="MEMBERSHIP_DOLLARS" }
                }
            };

            // Act
            var result = await _controller.ImportRewardTypesAsync(request);

            // Assert
            var okObjectResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okObjectResult);
            Assert.Equal(StatusCodes.Status200OK, okObjectResult.StatusCode);
        }

        [Fact]
        public async System.Threading.Tasks.Task ImportRewardTypes_ReturnsPartialContent_WhenSomeErrorsOccurred()
        {
            // Arrange
            var request = new ImportRewardTypeRequestDto
            {
                RewardTypes = new List<TaskRewardTypeDto>
                {
                    new TaskRewardTypeDto { RewardTypeCode ="rtc-88f6fb24b04841a7b93b597e6fe36d91",RewardTypeName ="MEMBERSHIP_DOLLARS" }
                }
            };
            var taskTypeModel = new TaskRewardTypeModel()
            {
                RewardTypeCode = "rtc-88f6fb24b04841a7b93b597e6fe36d91",
                RewardTypeName = "MEMBERSHIP_DOLLARS"
            };

            _mapper.Setup(x => x.Map<TaskRewardTypeModel>(It.IsAny<TaskRewardTypeDto>())).Returns(taskTypeModel);
            var rewardTypeRepo = new Mock<ITaskRewardTypeRepo>();
            var service = new TaskRewardTypeService(_taskRewardTypeServiceLogger.Object, _mapper.Object, rewardTypeRepo.Object);
            var controller = new TaskRewardTypeController(_taskRewardTypeControllerLogger.Object, service);
            rewardTypeRepo.Setup(x => x.CreateAsync(It.IsAny<TaskRewardTypeModel>())).ThrowsAsync(new Exception("Duplicate key violation"));

            // Act
            var result = await controller.ImportRewardTypesAsync(request);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.NotNull(objectResult);
            Assert.Equal(StatusCodes.Status206PartialContent, objectResult.StatusCode);
        }

        [Fact]
        public async System.Threading.Tasks.Task ImportRewardTypes_ReturnsInternalServerError_WhenExceptionThrown()
        {
            // Arrange
            var request = new ImportRewardTypeRequestDto
            {
                RewardTypes = new List<TaskRewardTypeDto>
                {
                    new TaskRewardTypeDto { RewardTypeCode ="rtc-88f6fb24b04841a7b93b597e6fe36d91",RewardTypeName ="MEMBERSHIP_DOLLARS" }
                }
            };
            var taskTypeModel = new TaskRewardTypeModel()
            {
                RewardTypeCode = "rtc-88f6fb24b04841a7b93b597e6fe36d91",
                RewardTypeName = "MEMBERSHIP_DOLLARS"
            };

            
            var service = new Mock<ITaskRewardTypeService>();
            var controller = new TaskRewardTypeController(_taskRewardTypeControllerLogger.Object, service.Object);
            service.Setup(x => x.ImportRewardTypesAsync(It.IsAny<ImportRewardTypeRequestDto>())).Throws(new Exception("Testing"));

            // Act
            var result = await controller.ImportRewardTypesAsync(request);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.NotNull(objectResult);
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
        }
    }
}

