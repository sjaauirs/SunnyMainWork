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
using System.Linq.Expressions;
using Xunit;

namespace SunnyRewards.Helios.Task.UnitTest.Controllers
{
    public class SubtaskControllerUnitTests
    {
        private readonly Mock<ILogger<SubtaskController>> _logger;
        private SubtaskController _subtaskController;

        private readonly SubTaskService _subtaskService;
        private readonly Mock<ITaskRepo> _taskRepoMock;
        private readonly Mock<ITaskRewardRepo> _taskRewardRepoMock;
        private readonly Mock<ITaskTypeRepo> _taskTypeRepoMock;
        private readonly Mock<ISubTaskRepo> _subtaskRepoMock;
        private readonly Mock<IConsumerTaskRepo> _consumerTaskRepoMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<ILogger<SubTaskService>> _serviceLoggerMock;

        public SubtaskControllerUnitTests()
        {
            _logger = new Mock<ILogger<SubtaskController>>();
            _taskRepoMock = new Mock<ITaskRepo>();
            _taskRewardRepoMock = new Mock<ITaskRewardRepo>();
            _taskTypeRepoMock = new Mock<ITaskTypeRepo>();
            _subtaskRepoMock = new Mock<ISubTaskRepo>();
            _consumerTaskRepoMock = new Mock<IConsumerTaskRepo>();
            _mapperMock = new Mock<IMapper>();
            _serviceLoggerMock = new Mock<ILogger<SubTaskService>>();
            
            // Initialize the service with mocks
            _subtaskService = new SubTaskService(_serviceLoggerMock.Object, _taskRepoMock.Object, _taskRewardRepoMock.Object, _taskTypeRepoMock.Object, _subtaskRepoMock.Object, _consumerTaskRepoMock.Object, _mapperMock.Object);
            _subtaskController = new SubtaskController(_logger.Object, _subtaskService);
        }

        [Fact]
        public async System.Threading.Tasks.Task UpdateSubtask_ShouldReturnOk_WhenRequestIsSuccessful()
        {
            // Arrange
            long subtaskId = 6;
            var requestDto = new SubTaskUpdateRequestDto
            {
                SubTaskId = 6,
                ParentTaskRewardId = 280,
                ChildTaskRewardId = 91,
                ConfigJson = "\"{\\\"spinwheelConfig\\\":{\\\"probability\\\":1,\\\"itemDefinition\\\":[{\\\"itemText\\\":\\\"1\\\",\\\"lowProbability\\\":\\\"0.0\\\",\\\"highProbability\\\":\\\"0.25\\\"},{\\\"itemText\\\":\\\"1\\\",\\\"lowProbability\\\":\\\"0.25\\\",\\\"highProbability\\\":\\\"0.50\\\"},{\\\"itemText\\\":\\\"1\\\",\\\"lowProbability\\\":\\\"0.50\\\",\\\"highProbability\\\":\\\"0.72\\\"},{\\\"itemText\\\":\\\"2\\\",\\\"lowProbability\\\":\\\"0.72\\\",\\\"highProbability\\\":\\\"0.8\\\"},{\\\"itemText\\\":\\\"2\\\",\\\"lowProbability\\\":\\\"0.8\\\",\\\"highProbability\\\":\\\"0.9\\\"},{\\\"itemText\\\":\\\"5\\\",\\\"lowProbability\\\":\\\"0.9\\\",\\\"highProbability\\\":\\\"1.0\\\"}],\\\"itemTextSuffix\\\":\\\"x\\\"}}\""
            };

            var existingCategory = new SubTaskModel
            {
                SubTaskId = 6,
                ParentTaskRewardId = 280,
                ChildTaskRewardId = 91,
                ConfigJson = "\"{\\\"spinwheelConfig\\\":{\\\"probability\\\":1,\\\"itemDefinition\\\":[{\\\"itemText\\\":\\\"1\\\",\\\"lowProbability\\\":\\\"0.0\\\",\\\"highProbability\\\":\\\"0.25\\\"},{\\\"itemText\\\":\\\"1\\\",\\\"lowProbability\\\":\\\"0.25\\\",\\\"highProbability\\\":\\\"0.50\\\"},{\\\"itemText\\\":\\\"1\\\",\\\"lowProbability\\\":\\\"0.50\\\",\\\"highProbability\\\":\\\"0.72\\\"},{\\\"itemText\\\":\\\"2\\\",\\\"lowProbability\\\":\\\"0.72\\\",\\\"highProbability\\\":\\\"0.8\\\"},{\\\"itemText\\\":\\\"2\\\",\\\"lowProbability\\\":\\\"0.8\\\",\\\"highProbability\\\":\\\"0.9\\\"},{\\\"itemText\\\":\\\"5\\\",\\\"lowProbability\\\":\\\"0.9\\\",\\\"highProbability\\\":\\\"1.0\\\"}],\\\"itemTextSuffix\\\":\\\"x\\\"}}\""
            };

            _subtaskRepoMock.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<SubTaskModel, bool>>>(), false))
                .ReturnsAsync(existingCategory);

            _mapperMock.Setup(m => m.Map(requestDto, existingCategory));
            _subtaskRepoMock.Setup(x => x.UpdateAsync(existingCategory)).ReturnsAsync(existingCategory);

            // Act
            var result = await _subtaskController.UpdateSubtask(subtaskId, requestDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<SubtaskResponseDto>(okResult.Value);
            Assert.Null(response.ErrorCode);
            Assert.Equal(200, okResult.StatusCode);
        }

        [Fact]
        public async System.Threading.Tasks.Task UpdateSubtask_ShouldReturnNotFound_WhenSubtaskDoesNotExist()
        {
            // Arrange
            long subtaskId = 6;
            var requestDto = new SubTaskUpdateRequestDto
            {
                SubTaskId = 6,
                ParentTaskRewardId = 280,
                ChildTaskRewardId = 91,
                ConfigJson = "\"{\\\"spinwheelConfig\\\":{\\\"probability\\\":1,\\\"itemDefinition\\\":[{\\\"itemText\\\":\\\"1\\\",\\\"lowProbability\\\":\\\"0.0\\\",\\\"highProbability\\\":\\\"0.25\\\"},{\\\"itemText\\\":\\\"1\\\",\\\"lowProbability\\\":\\\"0.25\\\",\\\"highProbability\\\":\\\"0.50\\\"},{\\\"itemText\\\":\\\"1\\\",\\\"lowProbability\\\":\\\"0.50\\\",\\\"highProbability\\\":\\\"0.72\\\"},{\\\"itemText\\\":\\\"2\\\",\\\"lowProbability\\\":\\\"0.72\\\",\\\"highProbability\\\":\\\"0.8\\\"},{\\\"itemText\\\":\\\"2\\\",\\\"lowProbability\\\":\\\"0.8\\\",\\\"highProbability\\\":\\\"0.9\\\"},{\\\"itemText\\\":\\\"5\\\",\\\"lowProbability\\\":\\\"0.9\\\",\\\"highProbability\\\":\\\"1.0\\\"}],\\\"itemTextSuffix\\\":\\\"x\\\"}}\""
            };

            _subtaskRepoMock.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<SubTaskModel, bool>>>(), false))
                .ReturnsAsync((SubTaskModel)null);

            // Act
            var result = await _subtaskController.UpdateSubtask(subtaskId, requestDto);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status404NotFound, objectResult.StatusCode);
            var response = Assert.IsType<SubtaskResponseDto>(objectResult.Value);
            Assert.Equal(StatusCodes.Status404NotFound, response.ErrorCode);
        }

        [Fact]
        public async System.Threading.Tasks.Task UpdateSubtask_ShouldReturnBadRequest_WhenIdMismatchOccurs()
        {
            // Arrange
            var requestDto = new SubTaskUpdateRequestDto
            {
                SubTaskId = 2, // Different from query param
            };
            long subtaskId = 1; // Mismatch with requestDto.SubtaskId

            // Act
            var result = await _subtaskController.UpdateSubtask(subtaskId, requestDto);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status400BadRequest, objectResult.StatusCode);
            Assert.Equal("Mismatch between SubtaskId in query parameter and object.", objectResult.Value);
        }

        [Fact]
        public async System.Threading.Tasks.Task UpdateSubtask_ShouldReturnError_WhenExceptionOccurs()
        {
            // Arrange
            long subtaskId = 1;
            var requestDto = new SubTaskUpdateRequestDto
            {
                SubTaskId = 1,
            };

            _subtaskRepoMock.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<SubTaskModel, bool>>>(), false))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _subtaskController.UpdateSubtask(subtaskId, requestDto);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
            var response = Assert.IsType<SubtaskResponseDto>(objectResult.Value);
            Assert.Equal(StatusCodes.Status500InternalServerError, response.ErrorCode);
            Assert.Equal("Database error", response.ErrorMessage);
        }
    }
}
