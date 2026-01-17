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
    public class TaskTypeControllerTests
    {
        private readonly Mock<ILogger<TaskTypeController>> _loggerMock;
        private readonly Mock<ITaskTypeService> _serviceMock;
        private readonly TaskTypeController _controller;

        public TaskTypeControllerTests()
        {
            _loggerMock = new Mock<ILogger<TaskTypeController>>();
            _serviceMock = new Mock<ITaskTypeService>();
            _controller = new TaskTypeController(_loggerMock.Object, _serviceMock.Object);
        }

        [Fact]
        public async TaskAlias GetTaskTypesAsync_ShouldReturnOkResult()
        {
            // Arrange
            var responseDto = new TaskTypesResponseMockDto();

            _serviceMock.Setup(x => x.GetTaskTypesAsync()).ReturnsAsync(responseDto);

            // Act
            var result = await _controller.GetTaskTypesAsync();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);

            var returnedResponse = Assert.IsType<TaskTypesResponseMockDto>(okResult.Value);
            Assert.NotNull(returnedResponse.TaskTypes);
            Assert.Equal(2, returnedResponse.TaskTypes.Count);
        }

        [Fact]
        public async TaskAlias GetTaskTypesAsync_ShouldReturnConflictResult_WhenServiceReturns409()
        {
            // Arrange
            var responseDto = new TaskTypesResponseDto
            {
                ErrorCode = StatusCodes.Status409Conflict,
                ErrorMessage = "Conflict occurred"
            };

            _serviceMock.Setup(x => x.GetTaskTypesAsync()).ReturnsAsync(responseDto);

            // Act
            var result = await _controller.GetTaskTypesAsync();

            // Assert
            var conflictResult = Assert.IsType<ConflictObjectResult>(result);
            Assert.Equal(StatusCodes.Status409Conflict, conflictResult.StatusCode);

            var returnedResponse = Assert.IsType<TaskTypesResponseDto>(conflictResult.Value);
            Assert.Equal("Conflict occurred", returnedResponse.ErrorMessage);
        }

        [Fact]
        public async TaskAlias GetTaskTypesAsync_ShouldReturnInternalServerErrorResult_WhenServiceReturns500()
        {
            // Arrange
            var responseDto = new TaskTypesResponseDto
            {
                ErrorCode = StatusCodes.Status500InternalServerError,
                ErrorMessage = "Internal server error"
            };

            _serviceMock.Setup(x => x.GetTaskTypesAsync()).ReturnsAsync(responseDto);

            // Act
            var result = await _controller.GetTaskTypesAsync();

            // Assert
            var errorResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, errorResult.StatusCode);

            var returnedResponse = Assert.IsType<TaskTypesResponseDto>(errorResult.Value);
            Assert.Equal("Internal server error", returnedResponse.ErrorMessage);
        }

        [Fact]
        public async TaskAlias GetTaskTypesAsync_ShouldReturnInternalServerErrorResult_WhenExceptionThrown()
        {
            // Arrange
            var exceptionMessage = "Unhandled exception occurred";
            _serviceMock.Setup(x => x.GetTaskTypesAsync()).ThrowsAsync(new Exception(exceptionMessage));

            // Act
            var result = await _controller.GetTaskTypesAsync();

            // Assert
            var errorResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, errorResult.StatusCode);

            var returnedResponse = Assert.IsType<TaskTypesResponseDto>(errorResult.Value);
            Assert.Equal(StatusCodes.Status500InternalServerError, returnedResponse.ErrorCode);
            Assert.Equal(exceptionMessage, returnedResponse.ErrorMessage);
        }
    }
}
