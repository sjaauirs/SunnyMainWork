using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Sunny.Benefits.Cms.Core.Domain.Dtos;
using SunnyRewards.Helios.Admin.Api.Controllers;
using SunnyRewards.Helios.Admin.Core.Domain.Constants;
using SunnyRewards.Helios.Admin.Infrastructure.HttpClients.Interfaces;
using SunnyRewards.Helios.Admin.Infrastructure.Services;
using SunnyRewards.Helios.Admin.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using Xunit;
using TaskAlias = System.Threading.Tasks.Task;

namespace SunnyRewards.Helios.Admin.UnitTest.Controllers
{
    public class ComponentControllerUnitTests
    {
        private readonly Mock<ILogger<ComponentController>> _controllerLogger;
        private readonly Mock<ILogger<ComponentService>> _componentServiceLogger;
        private readonly Mock<ICmsClient> _cmsClient;
        private readonly IComponentService _componentService;
        private readonly ComponentController _componentController;

        public ComponentControllerUnitTests()
        {
            _controllerLogger = new Mock<ILogger<ComponentController>>();
            _componentServiceLogger = new Mock<ILogger<ComponentService>>();
            _cmsClient = new Mock<ICmsClient>();
            _componentService = new ComponentService(_componentServiceLogger.Object, _cmsClient.Object);

            _componentController = new ComponentController(_controllerLogger.Object, _componentService);
        }
        [Fact]
        public async TaskAlias CreateComponent_ShouldReturnOkResult()
        {
            // Arrange
            var requestDto = ComponentRequestMock();
            _cmsClient.Setup(x => x.Post<BaseResponseDto>(Constant.CreateComponentUrl, It.IsAny<ComponentRequestDto>()))
                .ReturnsAsync(new BaseResponseDto() { ErrorCode = null });
            // Act
            var result = await _componentController.CreateComponent(requestDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
        }

        [Fact]
        public async TaskAlias CreateComponent_ShouldReturnErrorResult_WhenCreateComponentReturnsError()
        {
            // Arrange
            var requestDto = ComponentRequestMock();

            _cmsClient.Setup(x => x.Post<BaseResponseDto>(Constant.CreateComponentUrl, It.IsAny<ComponentRequestDto>()))
                .ReturnsAsync(new BaseResponseDto() { ErrorCode = StatusCodes.Status500InternalServerError });

            // Act
            var result = await _componentController.CreateComponent(requestDto);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
        }
        [Fact]
        public async TaskAlias UpdateComponent_ShouldReturnOkResult()
        {
            // Arrange
            var requestDto = ComponentRequestMock();
            _cmsClient.Setup(x => x.Put<UpdateComponentResponseDto>(Constant.CreateComponentUrl, It.IsAny<ComponentRequestDto>()))
                .ReturnsAsync(new UpdateComponentResponseDto() { ErrorCode = null, Component = new ComponentDto()});
            // Act
            var result = await _componentController.UpdateComponent(requestDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
        }

        [Fact]
        public async TaskAlias UpdateComponent_ShouldReturnErrorResult_WhenUpdateComponentReturnsError()
        {
            // Arrange
            var requestDto = ComponentRequestMock();

            _cmsClient.Setup(x => x.Put<UpdateComponentResponseDto>(Constant.CreateComponentUrl, It.IsAny<ComponentRequestDto>()))
                .ReturnsAsync(new UpdateComponentResponseDto() { ErrorCode = StatusCodes.Status500InternalServerError });

            // Act
            var result = await _componentController.UpdateComponent(requestDto);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
        }
        [Fact]
        public async TaskAlias GetAllComponents_ShouldReturnErrorResult_WhenGetAllComponentReturnsError()
        {
            // Arrange
            var requestDto = new GetAllComponentsRequestDto { TenantCode = "ten-ecada21e57154928a2bb959e8365b8b4" };

            _cmsClient.Setup(x => x.Post<GetAllComponentsResponseDto>(Constant.GetAllComponents, It.IsAny<GetAllComponentsRequestDto>()))
                .ReturnsAsync(new GetAllComponentsResponseDto() { ErrorCode = StatusCodes.Status500InternalServerError });

            // Act
            var result = await _componentController.GetAllComponents(requestDto);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
        }
        [Fact]
        public async TaskAlias GetAllComponents_ShouldReturnOkResult_WhenGetAllComponentReturns()
        {
            // Arrange
            var requestDto = new GetAllComponentsRequestDto { TenantCode = "ten-ecada21e57154928a2bb959e8365b8b4" };

            _cmsClient.Setup(x => x.Post<GetAllComponentsResponseDto>(Constant.GetAllComponents, It.IsAny<GetAllComponentsRequestDto>()))
                .ReturnsAsync(new GetAllComponentsResponseDto() { ErrorCode = null });

            // Act
            var result = await _componentController.GetAllComponents(requestDto);

            // Assert
            var objectResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(StatusCodes.Status200OK, objectResult.StatusCode);
        }
        [Fact]
        public async TaskAlias GetAllComponentTypes_ShouldReturnErrorResult_WhenGetAllComponentReturnsError()
        {
            // Arrange
            _cmsClient.Setup(x => x.Post<GetAllComponentsResponseDto>(Constant.GetAllComponentTypes, null))
                .ReturnsAsync(new GetAllComponentsResponseDto() { ErrorCode = StatusCodes.Status500InternalServerError });

            // Act
            var result = await _componentController.GetAllComponentTypes();

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
        }
        [Fact]
        public async TaskAlias GetAllComponentTypes_ShouldReturnOkResult_WhenGetAllComponentTypeReturns()
        {
            // Arrange
            _cmsClient.Setup(x => x.Get<GetAllComponentTypesResponseDto>(Constant.GetAllComponentTypes, null))
                .ReturnsAsync(new GetAllComponentTypesResponseDto() { ErrorCode = null });

            // Act
            var result = await _componentController.GetAllComponentTypes();

            // Assert
            var objectResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(StatusCodes.Status200OK, objectResult.StatusCode);
        }

        private static ComponentRequestDto ComponentRequestMock()
        {
            return new ComponentRequestDto
            {
                ComponentId = 1,
                ComponentTypeCode ="test" ,
                TenantCode = "ten-ecada21e57154928a2bb959e8365b8b4",
                ComponentCode = "someComponent Code",
                ComponentOverrideCode = "someComponent Code",
                ComponentName = "ComponentName",
                DataJson = "{\u0022data\u0022: {\u0022iconSvg\u0022: \u0022\u0022, u0022sweepstakesDetailsText\u0022: \u0022\u0022}}",
                MetadataJson = "{}",
                CreateUser = "per-915325069cdb42c783dd4601e1d27704",
                LanguageCode = "en-US",
                
            };
        }
    }
}
