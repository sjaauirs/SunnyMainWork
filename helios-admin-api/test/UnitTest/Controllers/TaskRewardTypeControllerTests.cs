using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using SunnyRewards.Helios.Admin.Api.Controllers;
using SunnyRewards.Helios.Admin.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.Admin.UnitTest.Fixtures.MockDtos;
using SunnyRewards.Helios.Task.Core.Domain.Dtos;
using Xunit;
using TaskAlias = System.Threading.Tasks.Task;

namespace SunnyRewards.Helios.Admin.UnitTest.Controllers
{
    public class TaskRewardTypeControllerTests
    {
        private readonly Mock<ILogger<TaskRewardTypeController>> _loggerMock;
        private readonly Mock<ITaskRewardTypeService> _serviceMock;
        private readonly TaskRewardTypeController _controller;

        public TaskRewardTypeControllerTests()
        {
            _loggerMock = new Mock<ILogger<TaskRewardTypeController>>();
            _serviceMock = new Mock<ITaskRewardTypeService>();
            _controller = new TaskRewardTypeController(_loggerMock.Object, _serviceMock.Object);
        }

        [Fact]
        public async TaskAlias GetTaskRewardTypesAsync_ShouldReturnOkResult()
        {
            // Arrange
            var responseDto = new TaskRewardTypesResponseMockDto();

            _serviceMock.Setup(x => x.GetTaskRewardTypesAsync()).ReturnsAsync(responseDto);

            // Act
            var result = await _controller.GetTaskRewardTypesAsync();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);

            var returnedResponse = Assert.IsType<TaskRewardTypesResponseMockDto>(okResult.Value);
            Assert.NotNull(returnedResponse.TaskRewardTypes);
            Assert.Equal(2, returnedResponse.TaskRewardTypes.Count);
        }

        [Fact]
        public async TaskAlias GetTaskRewardTypesAsync_ShouldReturnConflictResult_WhenServiceReturns409()
        {
            // Arrange
            var responseDto = new TaskRewardTypesResponseDto
            {
                ErrorCode = StatusCodes.Status409Conflict,
                ErrorMessage = "Conflict occurred"
            };

            _serviceMock.Setup(x => x.GetTaskRewardTypesAsync()).ReturnsAsync(responseDto);

            // Act
            var result = await _controller.GetTaskRewardTypesAsync();

            // Assert
            var conflictResult = Assert.IsType<ConflictObjectResult>(result);
            Assert.Equal(StatusCodes.Status409Conflict, conflictResult.StatusCode);

            var returnedResponse = Assert.IsType<TaskRewardTypesResponseDto>(conflictResult.Value);
            Assert.Equal("Conflict occurred", returnedResponse.ErrorMessage);
        }

        [Fact]
        public async TaskAlias GetTaskRewardTypesAsync_ShouldReturnInternalServerError_WhenServiceReturns500()
        {
            // Arrange
            var responseDto = new TaskRewardTypesResponseDto
            {
                ErrorCode = StatusCodes.Status500InternalServerError,
                ErrorMessage = "Internal server error"
            };

            _serviceMock.Setup(x => x.GetTaskRewardTypesAsync()).ReturnsAsync(responseDto);

            // Act
            var result = await _controller.GetTaskRewardTypesAsync();

            // Assert
            var errorResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, errorResult.StatusCode);

            var returnedResponse = Assert.IsType<TaskRewardTypesResponseDto>(errorResult.Value);
            Assert.Equal("Internal server error", returnedResponse.ErrorMessage);
        }

        [Fact]
        public async TaskAlias GetTaskRewardTypesAsync_ShouldReturnInternalServerError_WhenExceptionThrown()
        {
            // Arrange
            var exceptionMessage = "Unhandled exception occurred";
            _serviceMock.Setup(x => x.GetTaskRewardTypesAsync()).ThrowsAsync(new Exception(exceptionMessage));

            // Act
            var result = await _controller.GetTaskRewardTypesAsync();

            // Assert
            var errorResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, errorResult.StatusCode);

            var returnedResponse = Assert.IsType<TaskRewardTypesResponseDto>(errorResult.Value);
            Assert.Equal(StatusCodes.Status500InternalServerError, returnedResponse.ErrorCode);
            Assert.Equal(exceptionMessage, returnedResponse.ErrorMessage);
        }
    }
}
