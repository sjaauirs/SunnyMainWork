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
    public class TaskCategoryControllerTests
    {
        private readonly Mock<ILogger<TaskCategoryController>> _loggerMock;
        private readonly Mock<ITaskCategoryService> _serviceMock;
        private readonly TaskCategoryController _controller;

        public TaskCategoryControllerTests()
        {
            _loggerMock = new Mock<ILogger<TaskCategoryController>>();
            _serviceMock = new Mock<ITaskCategoryService>();
            _controller = new TaskCategoryController(_loggerMock.Object, _serviceMock.Object);
        }

        [Fact]
        public async TaskAlias GetTaskCategoriesAsync_ShouldReturnOkResult()
        {
            // Arrange
            var responseDto = new TaskCategoriesMockResponseDto();

            _serviceMock.Setup(x => x.GetTaskCategoriesAsync()).ReturnsAsync(responseDto);

            // Act
            var result = await _controller.GetTaskCategoriesAsync();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);

            var returnedResponse = Assert.IsType<TaskCategoriesMockResponseDto>(okResult.Value);
            Assert.NotNull(returnedResponse.TaskCategories);
            Assert.Equal(2, returnedResponse.TaskCategories.Count);
        }

        [Fact]
        public async TaskAlias GetTaskCategoriesAsync_ShouldReturnConflictResult_WhenServiceReturns409()
        {
            // Arrange
            var responseDto = new TaskCategoriesResponseDto
            {
                ErrorCode = StatusCodes.Status409Conflict,
                ErrorMessage = "Conflict occurred"
            };

            _serviceMock.Setup(x => x.GetTaskCategoriesAsync()).ReturnsAsync(responseDto);

            // Act
            var result = await _controller.GetTaskCategoriesAsync();

            // Assert
            var conflictResult = Assert.IsType<ConflictObjectResult>(result);
            Assert.Equal(StatusCodes.Status409Conflict, conflictResult.StatusCode);

            var returnedResponse = Assert.IsType<TaskCategoriesResponseDto>(conflictResult.Value);
            Assert.Equal("Conflict occurred", returnedResponse.ErrorMessage);
        }

        [Fact]
        public async TaskAlias GetTaskCategoriesAsync_ShouldReturnInternalServerError_WhenServiceReturns500()
        {
            // Arrange
            var responseDto = new TaskCategoriesResponseDto
            {
                ErrorCode = StatusCodes.Status500InternalServerError,
                ErrorMessage = "Internal server error"
            };

            _serviceMock.Setup(x => x.GetTaskCategoriesAsync()).ReturnsAsync(responseDto);

            // Act
            var result = await _controller.GetTaskCategoriesAsync();

            // Assert
            var errorResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, errorResult.StatusCode);

            var returnedResponse = Assert.IsType<TaskCategoriesResponseDto>(errorResult.Value);
            Assert.Equal("Internal server error", returnedResponse.ErrorMessage);
        }

        [Fact]
        public async TaskAlias GetTaskCategoriesAsync_ShouldReturnInternalServerError_WhenExceptionThrown()
        {
            // Arrange
            var exceptionMessage = "Unhandled exception occurred";
            _serviceMock.Setup(x => x.GetTaskCategoriesAsync()).ThrowsAsync(new Exception(exceptionMessage));

            // Act
            var result = await _controller.GetTaskCategoriesAsync();

            // Assert
            var errorResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, errorResult.StatusCode);

            var returnedResponse = Assert.IsType<TaskCategoriesResponseDto>(errorResult.Value);
            Assert.Equal(StatusCodes.Status500InternalServerError, returnedResponse.ErrorCode);
            Assert.Equal(exceptionMessage, returnedResponse.ErrorMessage);
        }
    }
}
