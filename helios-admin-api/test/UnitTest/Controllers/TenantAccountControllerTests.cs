using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using SunnyBenefits.Fis.Core.Domain.Dtos;
using SunnyRewards.Helios.Admin.Api.Controllers;
using SunnyRewards.Helios.Admin.Core.Domain.Constants;
using SunnyRewards.Helios.Admin.Core.Domain.Dtos;
using SunnyRewards.Helios.Admin.Infrastructure.HttpClients;
using SunnyRewards.Helios.Admin.Infrastructure.HttpClients.Interfaces;
using SunnyRewards.Helios.Admin.Infrastructure.Services;
using SunnyRewards.Helios.Admin.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.Admin.UnitTest.Helpers.HttpClientsMock;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.Wallet.Core.Domain.Dtos;
using Xunit;
using TaskAlias = System.Threading.Tasks.Task;
using WalletClientMock = SunnyRewards.Helios.Admin.UnitTest.Fixtures.MockHttpClient.WalletClientMock;

namespace SunnyRewards.Helios.Admin.UnitTest.Controllers
{
    public class TenantAccountControllerTest
    {
        private readonly Mock<ILogger<TenantAccountController>> _controllerLogger;
        private readonly Mock<ILogger<TenantAccountService>> _tenantAccountServiceLogger;
        private readonly Mock<IFisClient> _fisClient;
        private readonly Mock<IWalletClient> _walletClient;
        private readonly ITenantAccountService _tenantAccountService;
        private readonly TenantAccountController _tenantAccountController;
        private readonly Mock<Mapper> _mapper;

        public TenantAccountControllerTest()
        {
            _controllerLogger = new Mock<ILogger<TenantAccountController>>();
            _tenantAccountServiceLogger = new Mock<ILogger<TenantAccountService>>();
            _fisClient = new FisClientMock();
            _walletClient = new WalletClientMock();
            _mapper = new Mock<Mapper>();
            _tenantAccountService = new TenantAccountService(_tenantAccountServiceLogger.Object, _walletClient.Object, _fisClient.Object, _mapper.Object);
            _tenantAccountController = new TenantAccountController(_controllerLogger.Object, _tenantAccountService);
        }
        [Fact]
        public async TaskAlias CreateTenant_ShouldReturnOkResult()
        {
            // Arrange
            var createTenantAccountRequestDto = new CreateTenantAccountRequestDto
            {
                TenantAccount = new PostTenantAccountDto { TenantCode = "T123" }
            };
            _fisClient.Setup(x => x.Post<BaseResponseDto>(Constant.CreateTenantAcountAPIUrl, It.IsAny<CreateTenantAccountRequestDto>()))
               .ReturnsAsync(new BaseResponseDto());
            _walletClient.Setup(x => x.Post<BaseResponseDto>(Constant.CreateTenantMasterWalletsAPIUrl, It.IsAny<CreateTenantMasterWalletsRequestDto>()))
               .ReturnsAsync(new BaseResponseDto());

            // Act
            var result = await _tenantAccountController.CreateTenantAccount(createTenantAccountRequestDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
        }

        [Fact]
        public async TaskAlias CreateTenant_ShouldReturnErrorResult_WhenCreateTenantReturnsError()
        {
            // Arrange
            var createTenantAccountRequestDto = new CreateTenantAccountRequestDto
            {
                TenantAccount = new PostTenantAccountDto { TenantCode = "T123" }
            };

            _fisClient.Setup(x => x.Post<BaseResponseDto>(Constant.CreateTenantAcountAPIUrl, It.IsAny<CreateTenantAccountRequestDto>()))
                .ReturnsAsync(new BaseResponseDto() { ErrorCode = StatusCodes.Status500InternalServerError });


            // Act
            var result = await _tenantAccountController.CreateTenantAccount(createTenantAccountRequestDto);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
        }

        [Fact]
        public async TaskAlias CreateTenant_ShouldReturnErrorResult_WhenCreateTenantMasterWalletsReturnsError()
        {
            // Arrange
            var createTenantAccountRequestDto = new CreateTenantAccountRequestDto
            {
                TenantAccount = new PostTenantAccountDto { TenantCode = "T123" }
            };

            _walletClient.Setup(x => x.Post<BaseResponseDto>(Constant.CreateTenantMasterWalletsAPIUrl, It.IsAny<CreateTenantMasterWalletsRequestDto>()))
                .ReturnsAsync(new BaseResponseDto() { ErrorCode = StatusCodes.Status500InternalServerError });

            // Act
            var result = await _tenantAccountController.CreateTenantAccount(createTenantAccountRequestDto);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
        }

        [Fact]
        public async TaskAlias CreateTenant_ShouldReturnErrorResult_WhenCreateTenantThrowsException()
        {
            // Arrange
            var createTenantAccountRequestDto = new CreateTenantAccountRequestDto
            {
                TenantAccount = new PostTenantAccountDto { TenantCode = "T123" }
            };

            _walletClient.Setup(x => x.Post<BaseResponseDto>(Constant.CreateTenantMasterWalletsAPIUrl, It.IsAny<CreateTenantMasterWalletsRequestDto>()))
                .ThrowsAsync(new Exception("Testing"));

            // Act
            var result = await _tenantAccountController.CreateTenantAccount(createTenantAccountRequestDto);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
        }
        [Fact]
        public async TaskAlias GetTenantAccount_Should_return_SuccessWhen_TenantAccount()
        {
            // Arrange
            string tenantCode = "sample tenant";
            _fisClient.Setup(x => x.GetId<GetTenantAccountResponseDto>($"{Constant.TenantAccount}/{tenantCode}", new Dictionary<string, string>())).ReturnsAsync(new GetTenantAccountResponseDto
            {
                TenantAccount = new TenantAccountRequestDto()
            });

            // Act
            var result = await _tenantAccountController.GetTenantAccountDetails(tenantCode);

            // Assert
            Assert.NotNull(result);
            var actionResult = Assert.IsType<ActionResult<GetTenantAccountResponseDto>>(result);
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);

        }
        [Fact]
        public async TaskAlias GetTenantAccount_Should_return_NotFound_When_TenantAccount_NotFound()
        {
            // Arrange
            string tenantCode = "sample tenant";
            _fisClient.Setup(x => x.GetId<GetTenantAccountResponseDto>($"{Constant.TenantAccount}/{tenantCode}", new Dictionary<string, string>())).ReturnsAsync(new GetTenantAccountResponseDto
            {
                TenantAccount = null,
                ErrorCode = StatusCodes.Status404NotFound
            });

            // Act
            var result = await _tenantAccountController.GetTenantAccountDetails(tenantCode);

            // Assert
            Assert.NotNull(result);
            var actionResult = Assert.IsType<ActionResult<GetTenantAccountResponseDto>>(result);
            var okResult = Assert.IsType<ObjectResult>(actionResult.Result);
            Assert.Equal(StatusCodes.Status404NotFound, okResult.StatusCode);

        }
        [Fact]
        public async TaskAlias GetTenantAccount_Should_Throw_Exception()
        {
            // Arrange
            string tenantCode = "sample tenant";
            _fisClient.Setup(x => x.GetId<GetTenantAccountResponseDto>($"{Constant.TenantAccount}/{tenantCode}", new Dictionary<string, string>())).ThrowsAsync(new Exception("Something wrong"));

            // Act
            var result = await _tenantAccountController.GetTenantAccountDetails(tenantCode);

            // Assert
            Assert.NotNull(result);
            var actionResult = Assert.IsType<ActionResult<GetTenantAccountResponseDto>>(result);
            var okResult = Assert.IsType<ObjectResult>(actionResult.Result);
            Assert.Equal(StatusCodes.Status500InternalServerError, okResult.StatusCode);

        }
        [Fact]
        public async TaskAlias PutTenantAccount_Should_return_SuccessWhen_TenantAccount()
        {
            // Arrange
            string tenantCode = "sample tenant";
            var requestDto = new TenantAccountRequestDto();
            _fisClient.Setup(x => x.Put<TenantAccountUpdateResponseDto>($"{Constant.TenantAccount}/{tenantCode}", requestDto)).ReturnsAsync(new TenantAccountUpdateResponseDto
            {
                TenantAccount = new TenantAccountRequestDto()
            });

            // Act
            var result = await _tenantAccountController.UpdateTenantAccount(tenantCode,requestDto);

            // Assert
            Assert.NotNull(result);
            var actionResult = Assert.IsType<ActionResult<TenantAccountUpdateResponseDto>>(result);
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);

        }
        [Fact]
        public async TaskAlias PutTenantAccount_Should_return_NotFound_When_TenantAccount_NotFound()
        {
            // Arrange
            string tenantCode = "sample tenant";
            var requestDto = new TenantAccountRequestDto();
            _fisClient.Setup(x => x.Put<TenantAccountUpdateResponseDto>($"{Constant.TenantAccount}/{tenantCode}", requestDto)).ReturnsAsync(new TenantAccountUpdateResponseDto
            {
                TenantAccount = null,
                ErrorCode = StatusCodes.Status404NotFound
            });

            // Act
            var result = await _tenantAccountController.UpdateTenantAccount(tenantCode, requestDto);

            // Assert
            Assert.NotNull(result);
            var actionResult = Assert.IsType<ActionResult<TenantAccountUpdateResponseDto>>(result);
            var okResult = Assert.IsType<ObjectResult>(actionResult.Result);
            Assert.Equal(StatusCodes.Status404NotFound, okResult.StatusCode);

        }
        [Fact]
        public async TaskAlias PutTenantAccount_Should_Throw_Exception()
        {
            // Arrange
            string tenantCode = "sample tenant";
            var requestDto = new TenantAccountRequestDto();
            _fisClient.Setup(x => x.Put<TenantAccountUpdateResponseDto>($"{Constant.TenantAccount}/{tenantCode}", requestDto)).ThrowsAsync(new Exception("Something wrong"));

            // Act
            var result = await _tenantAccountController.UpdateTenantAccount(tenantCode, requestDto);

            // Assert
            Assert.NotNull(result);
            var actionResult = Assert.IsType<ActionResult<TenantAccountUpdateResponseDto>>(result);
            var okResult = Assert.IsType<ObjectResult>(actionResult.Result);
            Assert.Equal(StatusCodes.Status500InternalServerError, okResult.StatusCode);

        }
        [Fact]
        public async TaskAlias SaveTenantAccount_Should_return_SuccessWhen_TenantAccount()
        {
            // Arrange
            var requestDto = new TenantAccountRequestDto();
            _fisClient.Setup(x => x.Post<BaseResponseDto>(Constant.TenantAccount, requestDto)).ReturnsAsync(new BaseResponseDto());

            // Act
            var result = await _tenantAccountController.SaveTenantAccount(requestDto);

            // Assert
            Assert.NotNull(result);
            var actionResult = Assert.IsType<ActionResult<BaseResponseDto>>(result);
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);

        }
        [Fact]
        public async TaskAlias SaveTenantAccount_Should_return_NotFound_When_TenantAccount_NotFound()
        {
            // Arrange
            var requestDto = new TenantAccountRequestDto();
            _fisClient.Setup(x => x.Post<BaseResponseDto>(Constant.TenantAccount, requestDto)).ReturnsAsync(new BaseResponseDto
            {
                ErrorCode = StatusCodes.Status404NotFound
            });

            // Act
            var result = await _tenantAccountController.SaveTenantAccount(requestDto);

            // Assert
            Assert.NotNull(result);
            var actionResult = Assert.IsType<ActionResult<BaseResponseDto>>(result);
            var okResult = Assert.IsType<ObjectResult>(actionResult.Result);
            Assert.Equal(StatusCodes.Status404NotFound, okResult.StatusCode);

        }
        [Fact]
        public async TaskAlias SaveTenantAccount_Should_Throw_Exception()
        {
            // Arrange
            var requestDto = new TenantAccountRequestDto();
            _fisClient.Setup(x => x.Post<BaseResponseDto>(Constant.TenantAccount, requestDto)).ThrowsAsync(new Exception("Something wrong"));

            // Act
            var result = await _tenantAccountController.SaveTenantAccount(requestDto);

            // Assert
            Assert.NotNull(result);
            var actionResult = Assert.IsType<ActionResult<BaseResponseDto>>(result);
            var okResult = Assert.IsType<ObjectResult>(actionResult.Result);
            Assert.Equal(StatusCodes.Status500InternalServerError, okResult.StatusCode);

        }
    }
}
