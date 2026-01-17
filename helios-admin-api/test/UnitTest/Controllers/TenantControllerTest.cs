using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using SunnyRewards.Helios.Admin.Api.Controllers;
using SunnyRewards.Helios.Admin.Core.Domain.Constants;
using SunnyRewards.Helios.Admin.Infrastructure.HttpClients.Interfaces;
using SunnyRewards.Helios.Admin.Infrastructure.Services;
using SunnyRewards.Helios.Admin.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.Admin.UnitTest.Fixtures.MockHttpClient;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.Tenant.Core.Domain.Dtos;
using SunnyRewards.Helios.User.Core.Domain.Dtos;
using SunnyRewards.Helios.Wallet.Core.Domain.Dtos;
using Xunit;
using TaskAlias = System.Threading.Tasks.Task;
namespace SunnyRewards.Helios.Admin.UnitTest.Controllers
{
    public class TenantControllerTest
    {
        private readonly Mock<ILogger<TenantController>> _controllerLogger;
        private readonly Mock<ILogger<TenantService>> _tenantServiceLogger;
        private readonly Mock<ITenantClient> _tenantClient;
        private readonly Mock<IWalletClient> _walletClient;
        private readonly Mock<IUserClient> _userClient;
        private readonly ITenantService _tenantService;
        private readonly TenantController _tenantController;

        public TenantControllerTest()
        {
            _controllerLogger = new Mock<ILogger<TenantController>>();
            _tenantServiceLogger = new Mock<ILogger<TenantService>>();
            _tenantClient = new TenantClientMock();
            _walletClient = new WalletClientMock();
            _userClient = new UserClientMock();
            _tenantService = new TenantService(_tenantServiceLogger.Object, _tenantClient.Object, _walletClient.Object, _userClient.Object);
            _tenantController = new TenantController(_controllerLogger.Object, _tenantService);
        }

        [Fact]
        public async TaskAlias CreateTenant_ShouldReturnOkResult()
        {
            // Arrange
            var createTenantRequestDto = new CreateTenantRequestDto
            {
                Tenant = new PostTenantDto { TenantCode = "T123" }
            };

            // Act
            var result = await _tenantController.CreateTenant(createTenantRequestDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
        }

        [Fact]
        public async TaskAlias CreateTenant_ShouldReturnErrorResult_WhenCreateTenantReturnsError()
        {
            // Arrange
            var createTenantRequestDto = new CreateTenantRequestDto
            {
                Tenant = new PostTenantDto { TenantCode = "T123" }
            };

            _tenantClient.Setup(x => x.Post<BaseResponseDto>(Constant.CreateTenantAPIUrl, It.IsAny<CreateTenantRequestDto>()))
                .ReturnsAsync(new BaseResponseDto() { ErrorCode = StatusCodes.Status500InternalServerError });

            // Act
            var result = await _tenantController.CreateTenant(createTenantRequestDto);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
        }

        [Fact]
        public async TaskAlias CreateTenant_ShouldReturnErrorResult_WhenCreateTenantMasterWalletsReturnsError()
        {
            // Arrange
            var createTenantRequestDto = new CreateTenantRequestDto
            {
                Tenant = new PostTenantDto { TenantCode = "T123" }
            };

            _walletClient.Setup(x => x.Post<BaseResponseDto>(Constant.CreateTenantMasterWalletsAPIUrl, It.IsAny<CreateTenantMasterWalletsRequestDto>()))
                .ReturnsAsync(new BaseResponseDto() { ErrorCode = StatusCodes.Status500InternalServerError });

            // Act
            var result = await _tenantController.CreateTenant(createTenantRequestDto);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
        }

        [Fact]
        public async TaskAlias CreateTenant_ShouldReturnErrorResult_WhenCreateTenantThrowsException()
        {
            // Arrange
            var createTenantRequestDto = new CreateTenantRequestDto
            {
                Tenant = new PostTenantDto { TenantCode = "T123" }
            };

            _walletClient.Setup(x => x.Post<BaseResponseDto>(Constant.CreateTenantMasterWalletsAPIUrl, It.IsAny<CreateTenantMasterWalletsRequestDto>()))
                .ThrowsAsync(new Exception("Testing"));

            // Act
            var result = await _tenantController.CreateTenant(createTenantRequestDto);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
        }

        [Fact]
        public async TaskAlias GetAllConsumers_ShouldReturnOk_WhenConsumersRetrievedSuccessfully()
        {
            // Arrange
            var requestDto = new GetConsumerByTenantRequestDto() { TenantCode = "ten-abc" };
            var responseDto = new ConsumersAndPersonsListResponseDto() { ErrorCode = null, ConsumerAndPersons = new List<ConsumersAndPersons>() { new ConsumersAndPersons() { Consumer = new ConsumerDto() { ConsumerId = 1 }, Person = new PersonDto() { PersonId = 1 } } } };
            _userClient.Setup(client => client.Post<ConsumersAndPersonsListResponseDto>("consumer/get-consumers-by-tenant-code", It.IsAny<GetConsumerByTenantRequestDto>()))
                .ReturnsAsync(responseDto);

            // Act
            var result = await _tenantController.GetAllConsumers(requestDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
            Assert.Equal(responseDto, okResult.Value);
        }

        [Fact]
        public async TaskAlias GetAllConsumers_ShouldReturnError_WhenServiceReturnsErrorCode()
        {
            // Arrange
            var requestDto = new GetConsumerByTenantRequestDto() { TenantCode = "ten-abc" };
            var responseDto = new ConsumersAndPersonsListResponseDto() { ErrorCode = StatusCodes.Status404NotFound, ConsumerAndPersons = null };
            _userClient.Setup(client => client.Post<ConsumersAndPersonsListResponseDto>("consumer/get-consumers-by-tenant-code", It.IsAny<GetConsumerByTenantRequestDto>()))
                .ReturnsAsync(responseDto);

            // Act
            var result = await _tenantController.GetAllConsumers(requestDto);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status404NotFound, statusCodeResult.StatusCode);
            Assert.Equal(responseDto, statusCodeResult.Value);
        }

        [Fact]
        public async TaskAlias GetCohort_ShouldReturnInternalServerError_WhenExceptionIsThrown()
        {
            // Arrange
            var requestDto = new GetConsumerByTenantRequestDto() { TenantCode = "ten-abc" };
            _userClient.Setup(client => client.Post<ConsumersAndPersonsListResponseDto>("consumer/get-consumers-by-tenant-code", It.IsAny<GetConsumerByTenantRequestDto>()))
               .ThrowsAsync(new Exception("Testing"));


            // Act
            var result = await _tenantController.GetAllConsumers(requestDto);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, statusCodeResult.StatusCode);

            var responseDto = Assert.IsType<BaseResponseDto>(statusCodeResult.Value);
            Assert.Equal(StatusCodes.Status500InternalServerError, responseDto.ErrorCode);
        }

        [Fact]
        public async TaskAlias PutTenant_Should_return_SuccessWhen_Tenant_Update()
        {
            // Arrange
            string tenantCode = "sample tenant";
            var requestDto = new UpdateTenantDto();
            requestDto.TenantCode = tenantCode;
            _tenantClient.Setup(x => x.Put<UpdateTenantResponseDto>($"{Constant.UpdateTenant}/{tenantCode}", requestDto)).ReturnsAsync(new UpdateTenantResponseDto
            {
                UpdateTenant = new TenantDto
                {
                    TenantCode = tenantCode
                }
            });

            // Act
            var result = await _tenantController.UpdateTenant(tenantCode, requestDto);

            // Assert
            Assert.NotNull(result);
            var actionResult = Assert.IsType<ActionResult<UpdateTenantResponseDto>>(result);
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);

        }
        [Fact]
        public async TaskAlias PutTenant_Should_return_NotFound_When_TenantAccount_NotFound()
        {
            // Arrange
            string tenantCode = "sample tenant";
            var requestDto = new UpdateTenantDto();
            _tenantClient.Setup(x => x.Put<UpdateTenantResponseDto>($"{Constant.UpdateTenant}/{tenantCode}", requestDto)).ReturnsAsync(new UpdateTenantResponseDto { UpdateTenant = null, ErrorCode = StatusCodes.Status404NotFound });

            // Act
            var result = await _tenantController.UpdateTenant(tenantCode, requestDto);

            // Assert
            Assert.NotNull(result);
            var actionResult = Assert.IsType<ActionResult<UpdateTenantResponseDto>>(result);
            var okResult = Assert.IsType<ObjectResult>(actionResult.Result);
            Assert.Equal(StatusCodes.Status404NotFound, okResult.StatusCode);

        }
        [Fact]
        public async TaskAlias PutTenant_Should_Throw_Exception()
        {
            // Arrange
            string tenantCode = "sample tenant";
            var requestDto = new UpdateTenantDto();
            _tenantClient.Setup(x => x.Put<UpdateTenantResponseDto>($"{Constant.UpdateTenant}/{tenantCode}", requestDto)).ThrowsAsync(new Exception("Simulated"));

            // Act
            var result = await _tenantController.UpdateTenant(tenantCode, requestDto);

            // Assert
            Assert.NotNull(result);
            var actionResult = Assert.IsType<ActionResult<UpdateTenantResponseDto>>(result);
            var okResult = Assert.IsType<ObjectResult>(actionResult.Result);
            Assert.Equal(StatusCodes.Status500InternalServerError, okResult.StatusCode);

        }
        [Fact]
        public async TaskAlias GetAllTenant_Should_return_SuccessWhen_Tenant_Update()
        {
            // Arrange
            IDictionary<string, long> parameters = new Dictionary<string, long>();
            _tenantClient.Setup(x => x.Get<TenantsResponseDto>(Constant.Tenants, parameters)).ReturnsAsync(new TenantsResponseDto
            {
                Tenants = new List<TenantDto>
                {
                    new TenantDto (),
                    new TenantDto ()
                }
            });

            // Act
            var result = await _tenantController.GetTenants();

            // Assert
            Assert.NotNull(result);
            var actionResult = Assert.IsType<ActionResult<TenantsResponseDto>>(result);
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);

        }
        [Fact]
        public async TaskAlias GetAllTenant_Should_return_NotFound_When_TenantAccount_NotFound()
        {
            // Arrange
            IDictionary<string, long> parameters = new Dictionary<string, long>();
            _tenantClient.Setup(x => x.Get<TenantsResponseDto>(Constant.Tenants, parameters)).ReturnsAsync(new TenantsResponseDto
            { ErrorCode = StatusCodes.Status404NotFound });

            // Act
            var result = await _tenantController.GetTenants();

            // Assert
            Assert.NotNull(result);
            var actionResult = Assert.IsType<ActionResult<TenantsResponseDto>>(result);
            var okResult = Assert.IsType<ObjectResult>(actionResult.Result);
            Assert.Equal(StatusCodes.Status404NotFound, okResult.StatusCode);

        }
        [Fact]
        public async TaskAlias GetAllTenants_Should_Throw_Exception()
        {
            // Arrange
            IDictionary<string, long> parameters = new Dictionary<string, long>();
            _tenantClient.Setup(x => x.Get<TenantsResponseDto>(Constant.Tenants, parameters)).ThrowsAsync(new Exception("Simulated Exception"));

            // Act
            var result = await _tenantController.GetTenants();

            // Assert
            Assert.NotNull(result);
            var actionResult = Assert.IsType<ActionResult<TenantsResponseDto>>(result);
            var okResult = Assert.IsType<ObjectResult>(actionResult.Result);
            Assert.Equal(StatusCodes.Status500InternalServerError, okResult.StatusCode);

        }
    }
}
