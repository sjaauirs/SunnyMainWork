using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using SunnyRewards.Helios.Admin.Api.Controllers;
using SunnyRewards.Helios.Admin.Core.Domain.Constants;
using SunnyRewards.Helios.Admin.Infrastructure.HttpClients.Interfaces;
using SunnyRewards.Helios.Admin.Infrastructure.Services;
using SunnyRewards.Helios.Admin.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.Task.Core.Domain.Dtos;
using Xunit;
using TaskAlias = System.Threading.Tasks.Task;

namespace SunnyRewards.Helios.Admin.UnitTest.Controllers
{
    public class TenantTaskCategoryControllerUnitTests
    {
        private readonly Mock<ILogger<TenantTaskCategoryController>> _controllerLogger;
        private readonly Mock<ILogger<TenantTaskCategoryService>> _tenantTaskServiceLogger;
        private readonly Mock<ITaskClient> _taskClient;
        private readonly ITenantTaskCategoryService _tenantTaskService;
        private readonly TenantTaskCategoryController _tenantTaskController;
        public TenantTaskCategoryControllerUnitTests()
        {
            // Initialize mocks
            _controllerLogger = new Mock<ILogger<TenantTaskCategoryController>>();
            _tenantTaskServiceLogger = new Mock<ILogger<TenantTaskCategoryService>>();
            _taskClient = new Mock<ITaskClient>();
            _tenantTaskService = new TenantTaskCategoryService(_tenantTaskServiceLogger.Object, _taskClient.Object);

            _tenantTaskController = new TenantTaskCategoryController(_controllerLogger.Object, _tenantTaskService);
        }

        [Fact]
        public async TaskAlias CreateTenantTaskCategory_ShouldReturnOkResult()
        {
            // Arrange
            var requestDto = new TenantTaskCategoryRequestDto
            {
                TenantCode = "tenant-code-123",
                TaskCategoryId = 1,
                ResourceJson = "{ \"taskIconUrl\": \"/assets/icons/test-icon.png\" }",
                CreateUser = "per-915325069cdb42c783dd4601e1d27704"
            };
            _taskClient.Setup(x => x.Post<BaseResponseDto>(Constant.CreateTenantTaskCategoryUrl, It.IsAny<TenantTaskCategoryRequestDto>()))
                .ReturnsAsync(new BaseResponseDto() { ErrorCode = null });
            // Act
            var result = await _tenantTaskController.CreateTenantTaskCategory(requestDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
        }

        [Fact]
        public async TaskAlias CreateTenantTaskCategory_ShouldReturnErrorResult_WhenCreateTenantTaskCategoryReturnsError()
        {
            // Arrange
            var requestDto = new TenantTaskCategoryRequestDto
            {
                TenantCode = "tenant-code-123",
                TaskCategoryId = 1,
                ResourceJson = "{ \"taskIconUrl\": \"/assets/icons/test-icon.png\" }",
                CreateUser = "per-915325069cdb42c783dd4601e1d27704"
            };

            _taskClient.Setup(x => x.Post<BaseResponseDto>(Constant.CreateTenantTaskCategoryUrl, It.IsAny<CreateTaskRequestDto>()))
                .ReturnsAsync(new BaseResponseDto() { ErrorCode = StatusCodes.Status500InternalServerError });

            // Act
            var result = await _tenantTaskController.CreateTenantTaskCategory(requestDto);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
        }
    }
}
