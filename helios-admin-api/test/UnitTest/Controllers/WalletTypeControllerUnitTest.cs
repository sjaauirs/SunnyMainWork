using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using SunnyRewards.Helios.Admin.Api.Controllers;
using SunnyRewards.Helios.Admin.Core.Domain.Constants;
using SunnyRewards.Helios.Admin.Infrastructure.Helpers.Interface;
using SunnyRewards.Helios.Admin.Infrastructure.HttpClients.Interfaces;
using SunnyRewards.Helios.Admin.Infrastructure.Services;
using SunnyRewards.Helios.Admin.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.Admin.UnitTest.Fixtures.MockDtos;
using SunnyRewards.Helios.Bff.Infrastructure.Services;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.Wallet.Core.Domain.Dtos;
using Xunit;
using TaskAlias = System.Threading.Tasks.Task;

namespace SunnyRewards.Helios.Admin.UnitTest.Controllers
{
    public class WalletTypeControllerUnitTest
    {
        private readonly Mock<ILogger<WalletTypeController>> _controllerLogger;
        private readonly Mock<IWalletClient> _walletClient;
        private readonly IWalletTypeService _walletTypeService;
        private readonly WalletTypeController _walletTypeController; 
        private readonly Mock<ILogger<WalletTypeService>> _serviceLogger;
        private readonly Mock<IConfiguration> _configMock;
        private readonly Mock<IWalletHelper> _walletHelper;
        private readonly Mock<IEventService> _eventService;

        public WalletTypeControllerUnitTest()
        {
            _controllerLogger = new Mock<ILogger<WalletTypeController>>();
            _walletClient = new Mock<IWalletClient>();
            _walletHelper = new Mock<IWalletHelper>();
            _eventService = new Mock<IEventService>();

            _serviceLogger = new Mock<ILogger<WalletTypeService>>();
            _configMock = new Mock<IConfiguration>();
            _walletTypeService = new WalletTypeService(_serviceLogger.Object, _walletClient.Object, _configMock.Object , _walletHelper.Object, _eventService.Object);
            _walletTypeController = new WalletTypeController(_controllerLogger.Object, _walletTypeService);

        }
        [Fact]
        public async TaskAlias GetWalletTypes_Should_Return_Success_When_WalletTypes_Are_Fetched_Successfully()
        {
            // Arrange
            IDictionary<string, long> parameters = new Dictionary<string, long>();
            _walletClient.Setup(x => x.Get<GetWalletTypeResponseDto>(Constant.WalletTypes, parameters)).ReturnsAsync(new GetWalletTypeResponseDto
            {
                WalletTypes = new List<WalletTypeDto>
                {
                    new WalletTypeDto(),
                    new WalletTypeDto()
                }
            });

            // Act
            var result = await _walletTypeController.GetAllWalletTypes();

            // Assert
            Assert.NotNull(result);
            var actionResult = Assert.IsType<ActionResult<GetWalletTypeResponseDto>>(result);
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
        }
        [Fact]
        public async TaskAlias GetWalletType_Should_Return_NotFound_When_WalletType_Not_Found()
        {
            // Arrange
            IDictionary<string, long> parameters = new Dictionary<string, long>();
            _walletClient.Setup(x => x.Get<GetWalletTypeResponseDto>(Constant.WalletTypes, parameters)).ReturnsAsync(new GetWalletTypeResponseDto
            { ErrorCode = StatusCodes.Status404NotFound });

            // Act
            var result = await _walletTypeController.GetAllWalletTypes();

            // Assert
            Assert.NotNull(result);
            var actionResult = Assert.IsType<ActionResult<GetWalletTypeResponseDto>>(result);
            var notFoundResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            Assert.Equal(StatusCodes.Status200OK, notFoundResult.StatusCode);
        }
        [Fact]
        public async TaskAlias GetWalletType_Should_Return_InternalServerError_When_Exception_Thrown()
        {
            // Arrange

            IDictionary<string, long> parameters = new Dictionary<string, long>();
            _walletClient.Setup(x => x.Get<GetWalletTypeResponseDto>(Constant.WalletTypes, parameters)).ThrowsAsync(new Exception("SimulatedException"));

            // Act
            var result = await _walletTypeController.GetAllWalletTypes();

            // Assert
            Assert.NotNull(result);
            var actionResult = Assert.IsType<ActionResult<GetWalletTypeResponseDto>>(result);
            var errorResult = Assert.IsType<ObjectResult>(actionResult.Result);
            Assert.Equal(StatusCodes.Status500InternalServerError, errorResult.StatusCode);
        }

        [Fact]
        public async TaskAlias CreateWalletType_Should_Return_Success_When_WalletType_Is_Created_Successfully()
        {
            // Arrange
            var mockRequest = new WalletTypeDto { WalletTypeCode = "WalletType123" };

            _walletClient.Setup(x => x.Post<BaseResponseDto>(Constant.WalletType, mockRequest)).ReturnsAsync(new BaseResponseDto());


            // Act
            var result = await _walletTypeController.CreateWalletType(mockRequest);

            // Assert
            Assert.NotNull(result);
            var actionResult = Assert.IsType<ActionResult<BaseResponseDto>>(result);
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
        }
        [Fact]
        public async TaskAlias CreateWalletType_Should_Return_Error_When_WalletType_Already_Exist()
        {
            // Arrange
            var mockRequest = new WalletTypeDto { WalletTypeCode = "WalletType123" };
            var mockResponse = new BaseResponseDto
            {
                ErrorCode = StatusCodes.Status409Conflict,
            };
            _walletClient.Setup(x => x.Post<BaseResponseDto>(Constant.WalletType, mockRequest)).ReturnsAsync(mockResponse);

            // Act
            var result = await _walletTypeController.CreateWalletType(mockRequest);

            // Assert
            Assert.NotNull(result);
            var actionResult = Assert.IsType<ActionResult<BaseResponseDto>>(result);
            var errorResult = Assert.IsType<ObjectResult>(actionResult.Result);
            Assert.Equal(StatusCodes.Status409Conflict, errorResult.StatusCode);
        }
        [Fact]
        public async TaskAlias CreateWalletType_Should_Return_InternalServerError_When_Exception_Is_Thrown()
        {
            // Arrange
            var mockRequest = new WalletTypeDto { WalletTypeCode = "WalletType123" };

            _walletClient.Setup(x => x.Post<BaseResponseDto>(Constant.WalletType, mockRequest)).ThrowsAsync(new Exception("simulated Exception"));

            // Act
            var result = await _walletTypeController.CreateWalletType(mockRequest);

            // Assert
            Assert.NotNull(result);
            var actionResult = Assert.IsType<ActionResult<BaseResponseDto>>(result);
            var errorResult = Assert.IsType<ObjectResult>(actionResult.Result);
            Assert.Equal(StatusCodes.Status500InternalServerError, errorResult.StatusCode);

        }
        [Fact]
        public async TaskAlias Should_GetWalletTypeCode_Controller()
        {
            // Arrange
            var walletTypeMockDto = new WalletTypeMockDto();
            _walletClient.Setup(x => x.Post<WalletTypeDto>(It.IsAny<string>(), It.IsAny<WalletTypeDto>())).ReturnsAsync(new WalletTypeMockDto());

            // Act
            var response = await _walletTypeController.GetWalletTypeCode(walletTypeMockDto);

            // Assert
            var result = Assert.IsType<OkObjectResult>(response.Result);
            Assert.NotNull(result?.Value);
            Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
        }

        [Fact]
        public async TaskAlias Should_GetWalletTypeCode_For_NotFound_Controller()
        {
            // Arrange
            var walletTypeMockDto = new WalletTypeMockDto();
            _walletClient.Setup(x => x.Post<WalletTypeDto>(Constant.WalletType, walletTypeMockDto));

            // Act
            var response = await _walletTypeController.GetWalletTypeCode(walletTypeMockDto);

            // Assert
            var result = Assert.IsType<ObjectResult>(response.Result);
            Assert.NotNull(result?.Value);
            Assert.Equal(StatusCodes.Status404NotFound, result.StatusCode);
        }

        [Fact]
        public async TaskAlias Should_Return_GetWalletTypeCode_Catch_Exception_For_Controller()
        {
            // Arrange
            var walletTypeMockDto = new WalletTypeMockDto();
            _walletClient.Setup(x => x.Post<WalletTypeDto>(It.IsAny<string>(), It.IsAny<WalletTypeDto>())).ThrowsAsync(new Exception("testing"));

            // Act
            var response = await _walletTypeController.GetWalletTypeCode(walletTypeMockDto);

            // Assert
            var result = Assert.IsType<ObjectResult>(response.Result);
            Assert.NotNull(result?.Value);
            Assert.Equal(StatusCodes.Status500InternalServerError, result.StatusCode);
        }
    }
}
