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
using System.Linq.Expressions;
using Xunit;

namespace SunnyRewards.Helios.Task.UnitTest.Controllers
{
    public class TenantTaskCategoryControllerUnitTests
    {
        private readonly Mock<ILogger<TenantTaskCategoryController>> _logger;
        private TenantTaskCategoryController _tenantTaskCategoryController;

        private readonly TenantTaskCategoryService _tenantTaskCategoryService;
        private readonly Mock<ITenantTaskCategoryRepo> _tenantTaskCategoryRepoMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<ILogger<TenantTaskCategoryService>> _serviceLoggerMock;

        public TenantTaskCategoryControllerUnitTests()
        {
            _logger = new Mock<ILogger<TenantTaskCategoryController>>();
            _tenantTaskCategoryRepoMock = new Mock<ITenantTaskCategoryRepo>();
            _mapperMock = new Mock<IMapper>();
            _serviceLoggerMock = new Mock<ILogger<TenantTaskCategoryService>>();

            // Initialize the service with mocks
            _tenantTaskCategoryService = new TenantTaskCategoryService(_mapperMock.Object, _tenantTaskCategoryRepoMock.Object, _serviceLoggerMock.Object);
            _tenantTaskCategoryController = new TenantTaskCategoryController(_logger.Object, _tenantTaskCategoryService);
        }

        [Fact]
        public async System.Threading.Tasks.Task CreateTenantTaskCategory_ShouldReturnOk_WhenRequestIsSuccessful()
        {
            // Arrange
            var requestDto = new TenantTaskCategoryRequestDto
            {
                TenantCode = "tenant-code-123",
                TaskCategoryId = 1,
                ResourceJson = "{ \"taskIconUrl\": \"/assets/icons/test-icon.png\" }",
                CreateUser = "per-915325069cdb42c783dd4601e1d27704"
            };

            _tenantTaskCategoryRepoMock.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<TenantTaskCategoryModel, bool>>>(), false))
                .ReturnsAsync((TenantTaskCategoryModel)null);

            _mapperMock
                .Setup(m => m.Map<TenantTaskCategoryModel>(It.IsAny<TenantTaskCategoryRequestDto>()))
                .Returns(new TenantTaskCategoryModel());
            // Act
            var result = await _tenantTaskCategoryController.CreateTenantTaskCategory(requestDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<BaseResponseDto>(okResult.Value);
            Assert.Null(response.ErrorCode);
            Assert.Equal(200, okResult.StatusCode);
        }

        [Fact]
        public async System.Threading.Tasks.Task CreateTenantTaskCategory_ShouldReturnError_WhenConflictOccurs()
        {
            // Arrange
            var requestDto = new TenantTaskCategoryRequestDto
            {
                TenantCode = "tenant-code-123",
                TaskCategoryId = 1,
                ResourceJson = "{ \"taskIconUrl\": \"/assets/icons/test-icon.png\" }",
                CreateUser = "per-915325069cdb42c783dd4601e1d27704"
            };
            _tenantTaskCategoryRepoMock.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<TenantTaskCategoryModel, bool>>>(), false))
                                        .ReturnsAsync(new TenantTaskCategoryModel
                                        {
                                            TenantCode = requestDto.TenantCode,
                                            TaskCategoryId = requestDto.TaskCategoryId,
                                            ResourceJson = requestDto.ResourceJson
                                        });
          

            // Act
            var result = await _tenantTaskCategoryController.CreateTenantTaskCategory(requestDto);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status409Conflict, objectResult.StatusCode);
            var response = Assert.IsType<BaseResponseDto>(objectResult.Value);
            Assert.Equal(StatusCodes.Status409Conflict, response.ErrorCode);

        }

        [Fact]
        public async System.Threading.Tasks.Task UpdateTenantTaskCategory_ShouldReturnOk_WhenRequestIsSuccessful()
        {
            // Arrange
            long tenantTaskCategoryId = 1;
            var requestDto = new TenantTaskCategoryDto
            {
                TenantTaskCategoryId = 1,
                TenantCode = "tenant-code-123",
                ResourceJson = "{ \"taskIconUrl\": \"/assets/icons/test-icon.png\" }"
            };

            var existingCategory = new TenantTaskCategoryModel
            {
                TenantTaskCategoryId = 1,
                TenantCode = "tenant-code-123",
                ResourceJson = "{ \"taskIconUrl\": \"/assets/icons/old-icon.png\" }"
            };

            _tenantTaskCategoryRepoMock.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<TenantTaskCategoryModel, bool>>>(), false))
                .ReturnsAsync(existingCategory);

            _mapperMock.Setup(m => m.Map(requestDto, existingCategory));
            _tenantTaskCategoryRepoMock.Setup(x => x.UpdateAsync(existingCategory)).ReturnsAsync(existingCategory);

            // Act
            var result = await _tenantTaskCategoryController.UpdateTenantTaskCategory(tenantTaskCategoryId, requestDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<TenantTaskCategoryResponseDto>(okResult.Value);
            Assert.Null(response.ErrorCode);
            Assert.Equal(200, okResult.StatusCode);
        }

        [Fact]
        public async System.Threading.Tasks.Task UpdateTenantTaskCategory_ShouldReturnNotFound_WhenTenantTaskCategoryDoesNotExist()
        {
            // Arrange
            long tenantTaskCategoryId = 1;
            var requestDto = new TenantTaskCategoryDto
            {
                TenantTaskCategoryId = 1,
                TenantCode = "tenant-code-123"
            };

            _tenantTaskCategoryRepoMock.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<TenantTaskCategoryModel, bool>>>(), false))
                .ReturnsAsync((TenantTaskCategoryModel)null);

            // Act
            var result = await _tenantTaskCategoryController.UpdateTenantTaskCategory(tenantTaskCategoryId, requestDto);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status404NotFound, objectResult.StatusCode);
            var response = Assert.IsType<TenantTaskCategoryResponseDto>(objectResult.Value);
            Assert.Equal(StatusCodes.Status404NotFound, response.ErrorCode);
        }

        [Fact]
        public async System.Threading.Tasks.Task UpdateTenantTaskCategory_ShouldReturnBadRequest_WhenIdMismatchOccurs()
        {
            // Arrange
            var requestDto = new TenantTaskCategoryDto
            {
                TenantTaskCategoryId = 2, // Different from query param
                TenantCode = "tenant-code-123"
            };
            long tenantTaskCategoryId = 1; // Mismatch with requestDto.TenantTaskCategoryId

            // Act
            var result = await _tenantTaskCategoryController.UpdateTenantTaskCategory(tenantTaskCategoryId, requestDto);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status400BadRequest, objectResult.StatusCode);
            Assert.Equal("Mismatch between TenantTaskCategoryId in query parameter and object.", objectResult.Value);
        }

        [Fact]
        public async System.Threading.Tasks.Task UpdateTenantTaskCategory_ShouldReturnError_WhenExceptionOccurs()
        {
            // Arrange
            long tenantTaskCategoryId = 1;
            var requestDto = new TenantTaskCategoryDto
            {
                TenantTaskCategoryId = 1,
                TenantCode = "tenant-code-123"
            };

            _tenantTaskCategoryRepoMock.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<TenantTaskCategoryModel, bool>>>(), false))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _tenantTaskCategoryController.UpdateTenantTaskCategory(tenantTaskCategoryId, requestDto);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
            var response = Assert.IsType<TenantTaskCategoryResponseDto>(objectResult.Value);
            Assert.Equal(StatusCodes.Status500InternalServerError, response.ErrorCode);
            Assert.Equal("Database error", response.ErrorMessage);
        }
    }
}
