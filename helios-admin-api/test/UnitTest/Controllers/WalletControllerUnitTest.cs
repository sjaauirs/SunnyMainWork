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
using SunnyRewards.Helios.Wallet.Core.Domain.Constants;
using SunnyRewards.Helios.Wallet.Core.Domain.Dtos;
using Xunit;
using TaskAlias = System.Threading.Tasks.Task;
namespace SunnyRewards.Helios.Admin.UnitTest.Controllers
{
    public class WalletControllerUnitTest
    {
        private readonly Mock<ILogger<WalletController>> _controllerLogger;
        private readonly Mock<ILogger<WalletService>> _walletServiceLogger;
        private readonly Mock<IWalletClient> _walletClient;
        private readonly Mock<IUserClient> _userClient;
        private readonly Mock<ITaskClient> _taskClient;
        private readonly IWalletService _walletService;
        private readonly WalletController _walletController;

        public WalletControllerUnitTest()
        {
            _controllerLogger = new Mock<ILogger<WalletController>>();
            _walletServiceLogger = new Mock<ILogger<WalletService>>();
            _walletClient = new Mock<IWalletClient>();
            _userClient = new Mock<IUserClient>();
            _taskClient = new Mock<ITaskClient>();
            _walletService = new WalletService(_walletServiceLogger.Object, _walletClient.Object, _userClient.Object, _taskClient.Object);
            _walletController = new WalletController(_controllerLogger.Object, _walletService);
        }

        [Fact]
        public async TaskAlias GetMasterWallet_Should_Return_Success_When_MasterWallet_Are_Fetched_Successfully()
        {
            // Arrange
            IDictionary<string, long> parameters = new Dictionary<string, long>();
            string tenantCode = "tenantCode";
            _walletClient.Setup(x => x.Get<GetAllMasterWalletsResponseDto>($"{Constant.MasterWallet}/{tenantCode}", parameters)).ReturnsAsync(new GetAllMasterWalletsResponseDto
            {
                MasterWallets = new List<TenantWalletDetailDto>
                {
                    new TenantWalletDetailDto() { Wallet = new WalletDto()}
                }
            });

            // Act
            var result = await _walletController.GetMasterWallets(tenantCode);

            // Assert
            Assert.NotNull(result);
            var actionResult = Assert.IsType<ActionResult<GetAllMasterWalletsResponseDto>>(result);
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
        }
        [Fact]
        public async TaskAlias GetMasterWallet_Should_Return_NotFound_When_MasterWallet_Not_Found()
        {
            // Arrange
            IDictionary<string, long> parameters = new Dictionary<string, long>();
            string tenantCode = "tenantCode";
            _walletClient.Setup(x => x.Get<GetAllMasterWalletsResponseDto>($"{Constant.MasterWallet}/{tenantCode}", parameters)).ReturnsAsync(new GetAllMasterWalletsResponseDto
            { ErrorCode = StatusCodes.Status404NotFound });

            // Act
            var result = await _walletController.GetMasterWallets(tenantCode);

            // Assert
            Assert.NotNull(result);
            var actionResult = Assert.IsType<ActionResult<GetAllMasterWalletsResponseDto>>(result);
            var notFoundResult = Assert.IsType<ObjectResult>(actionResult.Result);
            Assert.Equal(StatusCodes.Status404NotFound, notFoundResult.StatusCode);
        }
        [Fact]
        public async TaskAlias GetMasterWallet_Should_Return_InternalServerError_When_Exception_Thrown()
        {
            // Arrange
            IDictionary<string, long> parameters = new Dictionary<string, long>();
            string tenantCode = "tenantCode";
            _walletClient.Setup(x => x.Get<GetAllMasterWalletsResponseDto>($"{Constant.MasterWallet}/{tenantCode}", parameters)).ThrowsAsync(new Exception("simulated exception"));

            // Act
            var result = await _walletController.GetMasterWallets(tenantCode);

            // Assert
            Assert.NotNull(result);
            var actionResult = Assert.IsType<ActionResult<GetAllMasterWalletsResponseDto>>(result);
            var errorResult = Assert.IsType<ObjectResult>(actionResult.Result);
            Assert.Equal(StatusCodes.Status500InternalServerError, errorResult.StatusCode);
        }

        [Fact]
        public async TaskAlias CreateWallet_Should_Return_Success_When_WalletType_Is_Created_Successfully()
        {
            // Arrange
            var mockRequest = new WalletRequestDto { WalletCode = "Wallet123" };

            _walletClient.Setup(x => x.Post<BaseResponseDto>(Constant.Wallet, mockRequest)).ReturnsAsync(new BaseResponseDto());


            // Act
            var result = await _walletController.CreateWallet(mockRequest);

            // Assert
            Assert.NotNull(result);
            var actionResult = Assert.IsType<ActionResult<BaseResponseDto>>(result);
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
        }
        [Fact]
        public async TaskAlias CreateWallet_Should_Return_Error_When_Wallet_Already_Exist()
        {
            // Arrange
            var mockRequest = new WalletRequestDto { WalletCode = "Wallet123" };
            var mockResponse = new BaseResponseDto
            {
                ErrorCode = StatusCodes.Status409Conflict,
            };
            _walletClient.Setup(x => x.Post<BaseResponseDto>(Constant.Wallet, mockRequest)).ReturnsAsync(mockResponse);

            // Act
            var result = await _walletController.CreateWallet(mockRequest);

            // Assert
            Assert.NotNull(result);
            var actionResult = Assert.IsType<ActionResult<BaseResponseDto>>(result);
            var okResult = Assert.IsType<ObjectResult>(actionResult.Result);
            Assert.Equal(StatusCodes.Status409Conflict, okResult.StatusCode);
        }
        [Fact]
        public async TaskAlias CreateWallet_Should_Return_InternalServerError_When_Exception_Is_Thrown()
        {
            // Arrange
            var mockRequest = new WalletRequestDto { WalletCode = "Wallet123" };

            _walletClient.Setup(x => x.Post<BaseResponseDto>(Constant.Wallet, mockRequest)).ThrowsAsync(new Exception("simulated Exception"));

            // Act
            var result = await _walletController.CreateWallet(mockRequest);

            // Assert
            Assert.NotNull(result);
            var actionResult = Assert.IsType<ActionResult<BaseResponseDto>>(result);
            var okResult = Assert.IsType<ObjectResult>(actionResult.Result);
            Assert.Equal(StatusCodes.Status500InternalServerError, okResult.StatusCode);

        }
        [Fact]
        public async TaskAlias CreateTenantMasterWallets_ShouldReturnOk_WhenWalletsCreatedSuccessfully()
        {
            // Arrange
            var request = new CreateTenantMasterWalletsRequestDto
            {
                TenantCode = "Tenant123",
                Apps = [WalletConstants.Apps.Rewards],
                CustomerCode = "Test customer code",
                SponsorCode = "Test sponsor code",
            };
            _walletClient.Setup(x => x.Post<BaseResponseDto>(It.IsAny<string>(), It.IsAny<CreateTenantMasterWalletsRequestDto>())).ReturnsAsync(new BaseResponseDto());
            // Act
            var result = await _walletController.CreateTenantMasterWallets(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult);
            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
        }

        [Fact]
        public async TaskAlias CreateTenantMasterWallets_ShouldReturnBadRequest_WhenAppsArrayIsEmpty()
        {
            // Arrange
            var request = new CreateTenantMasterWalletsRequestDto
            {
                TenantCode = "Tenant123",
                Apps = []
            };
            _walletClient.Setup(x => x.Post<BaseResponseDto>(It.IsAny<string>(), It.IsAny<CreateTenantMasterWalletsRequestDto>())).
                ReturnsAsync(new BaseResponseDto() { ErrorCode = StatusCodes.Status400BadRequest });

            // Act
            var result = await _walletController.CreateTenantMasterWallets(request);

            // Assert
            var badRequestResult = Assert.IsType<ObjectResult>(result);
            Assert.NotNull(badRequestResult);
            Assert.Equal(StatusCodes.Status400BadRequest, badRequestResult.StatusCode);
        }

        [Fact]
        public async TaskAlias CreateTenantMasterWallets_ShouldReturnInternalServerError_WhenExceptionOccurs()
        {
            // Arrange
            var request = new CreateTenantMasterWalletsRequestDto
            {
                TenantCode = "Tenant123",
                Apps = [WalletConstants.Apps.Rewards]
            };
            _walletClient.Setup(x => x.Post<BaseResponseDto>(It.IsAny<string>(), It.IsAny<CreateTenantMasterWalletsRequestDto>()))
                .ThrowsAsync(new Exception("testing"));

            // Act
            var result = await _walletController.CreateTenantMasterWallets(request);

            // Assert
            var internalServerErrorResult = Assert.IsType<ObjectResult>(result);
            Assert.NotNull(internalServerErrorResult);
            Assert.Equal(StatusCodes.Status500InternalServerError, internalServerErrorResult.StatusCode);
        }
    }
}
