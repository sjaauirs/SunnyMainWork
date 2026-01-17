
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Moq;
using NHibernate;
using SunnyRewards.Helios.Common.Core.Helpers;
using SunnyRewards.Helios.Common.Core.Helpers.Interfaces;
using SunnyRewards.Helios.Common.Core.Repositories;
using SunnyRewards.Helios.Tenant.Core.Domain.Dtos;
using SunnyRewards.Helios.User.Api.Controllers;
using SunnyRewards.Helios.User.Core.Domain.Constant;
using SunnyRewards.Helios.User.Core.Domain.Dtos;
using SunnyRewards.Helios.User.Core.Domain.Models;
using SunnyRewards.Helios.User.Infrastructure.HttpClients.Interfaces;
using SunnyRewards.Helios.User.Infrastructure.Mappings;
using SunnyRewards.Helios.User.Infrastructure.Repositories.Interfaces;
using SunnyRewards.Helios.User.Infrastructure.Services;
using SunnyRewards.Helios.User.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.User.UnitTest.Fixtures.MockDto;
using SunnyRewards.Helios.User.UnitTest.Fixtures.MockHttpClient;
using SunnyRewards.Helios.User.UnitTest.Fixtures.MockModels;
using SunnyRewards.Helios.User.UnitTest.Fixtures.MockRepositories;
using System.IdentityModel.Tokens.Jwt;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Text;
using Xunit;
using JwtPayload = SunnyRewards.Helios.User.Infrastructure.Helpers.JwtPayload;


namespace SunnyRewards.Helios.User.UnitTest.Controllers
{
    public class ConsumerLoginControllerUnitTest
    {
        private readonly Mock<ILogger<ConsumerLoginController>> _consumerLoginControllerLogger;
        private readonly Mock<ILogger<ConsumerLoginService>> _consumerLoginServiceLogger;
        private readonly Mock<IVault> _vault;
        private readonly Mock<NHibernate.ISession> _session;
        private readonly Mock<IConsumerRepo> _consumerRepo;
        private readonly Mock<IConsumerLoginRepo> _consumerLoginRepo;
        private readonly Mock<IPersonRepo> _personRepo;
        private readonly Mock<IRoleRepo> _roleRepo;
        private readonly Mock<IPersonRoleRepo> _personRoleRepo;
        private readonly Mock<IServerLoginRepo> _serverLoginRepo;
        private readonly Mock<ITenantClient> _tenantClient;
        private readonly IConsumerLoginService _consumerLoginService;
        private readonly ConsumerLoginController _consumerLoginController;
        private readonly Mock<IEncryptionHelper> _encryptionHelper;
        public ConsumerLoginControllerUnitTest()
        {
            _consumerLoginControllerLogger = new Mock<ILogger<ConsumerLoginController>>();
            _consumerLoginServiceLogger = new Mock<ILogger<ConsumerLoginService>>();
            _vault = new Mock<IVault>();
            _session = new Mock<NHibernate.ISession>();
            _consumerRepo = new ConsumerMockRepo();
            _consumerLoginRepo = new ConsumerLoginMockRepo();
            _personRepo = new PersonMockRepo();
            _roleRepo = new RoleMockRepo();
            _personRoleRepo = new PersonRoleMockRepo();
            _serverLoginRepo = new ServerLoginMockRepo();
            _tenantClient = new TenantClientMock();
            _encryptionHelper = new Mock<IEncryptionHelper>();
            _consumerLoginService = new ConsumerLoginService(_consumerLoginServiceLogger.Object, _vault.Object, _session.Object,
                _consumerRepo.Object, _consumerLoginRepo.Object, _personRepo.Object, _roleRepo.Object, _personRoleRepo.Object,
                _serverLoginRepo.Object, _tenantClient.Object, _encryptionHelper.Object);
            _consumerLoginController = new ConsumerLoginController(_consumerLoginControllerLogger.Object, _consumerLoginService);
        }

        //#1
        [Fact]
        public async Task Should_Create_AccessToken_If_Refresh_Threshold_Is_False()
        {
            _vault.Setup(x => x.GetSecret(It.IsAny<string>())).ReturnsAsync("Development");

            var transactionMock = new Mock<ITransaction>();
            var consumerLoginMap = new ConsumerLoginMap();
            _session.Setup(s => s.BeginTransaction()).Returns(transactionMock.Object);

            _consumerLoginRepo.Setup(x => x.FindAsync(It.IsAny<Expression<Func<ConsumerLoginModel, bool>>>(), false))
                .ReturnsAsync(new List<ConsumerLoginModel>() { new ConsumerLoginModel { RefreshTokenTs = new DateTime(2023, 08, 10, 00, 00, 00) } });

            var consumerLoginRequestMockDto = new ConsumerLoginRequestDto() { ConsumerCode = "test" };
            var consumerResponse = await _consumerLoginController.ConsumerLogin(consumerLoginRequestMockDto);
            var result = consumerResponse.Result as OkObjectResult;
            Assert.True(result?.StatusCode == 200);
        }

        [Fact]
        public async Task Should_Check_Refresh_Threshold_With_ConsumerCode_Path()
        {
            _vault.Setup(x => x.GetSecret(It.IsAny<string>())).ReturnsAsync("Development");

            var transactionMock = new Mock<ITransaction>();
            _session.Setup(s => s.BeginTransaction()).Returns(transactionMock.Object);

            var consumerLoginRequestMockDto = new ConsumerLoginRequestDto() { ConsumerCode = "cmr-c69905fc68ce4f36851f877bae38f22e" };
            var consumerResponse = await _consumerLoginController.ConsumerLogin(consumerLoginRequestMockDto);
            var result = consumerResponse.Result as OkObjectResult;
            Assert.True(result?.StatusCode == 200);
        }

        [Fact]
        public async Task Should_Check_Refresh_Threshold_With_MemNbr_Path()
        {
            _vault.Setup(x => x.GetSecret(It.IsAny<string>())).ReturnsAsync("Development");

            var transactionMock = new Mock<ITransaction>();
            _session.Setup(s => s.BeginTransaction()).Returns(transactionMock.Object);

            var consumerLoginRequestMockDto = new ConsumerLoginRequestDto() { MemberId = "6c267e72-b55d-4ab7-90e2-6ead89074e81", TenantCode = string.Empty };
            var consumerResponse = await _consumerLoginController.ConsumerLogin(consumerLoginRequestMockDto);
            var result = consumerResponse.Result as OkObjectResult;
            Assert.True(result?.StatusCode == 200);
        }

        [Fact]
        public async Task Should_Check_Refresh_Threshold_With_Email_Path()
        {
            _vault.Setup(x => x.GetSecret(It.IsAny<string>())).ReturnsAsync("Development");

            var transactionMock = new Mock<ITransaction>();
            _session.Setup(s => s.BeginTransaction()).Returns(transactionMock.Object);

            var consumerController = new ConsumerLoginController(_consumerLoginControllerLogger.Object, _consumerLoginService);

            var consumerLoginRequestMockDto = new ConsumerLoginRequestDto() { Email = "kailey.j@absentis.com" };
            var consumerResponse = await consumerController.ConsumerLogin(consumerLoginRequestMockDto);
            var result = consumerResponse.Result as OkObjectResult;
            Assert.True(result?.StatusCode == 200);
        }

        [Fact]
        public async Task Should_Check_If_Secret_Is_Null()
        {
            _vault.Setup(x => x.GetSecret(It.IsAny<string>())).ReturnsAsync("<SECRET_NOT_FOUND>");
            _vault.Setup(x => x.InvalidSecret).Returns("<SECRET_NOT_FOUND>");

            var consumerLoginRequestMockDto = new ConsumerLoginRequestDto() { ConsumerCode = "test" };
            var consumerResponse = await _consumerLoginController.ConsumerLogin(consumerLoginRequestMockDto);
            var result = consumerResponse.Result as ObjectResult;
            Assert.True(result?.StatusCode == 500);
        }

        [Fact]
        public async Task Should_Check_ConsumerCode_Null_Path()
        {
            _vault.Setup(x => x.GetSecret(It.IsAny<string>())).ReturnsAsync("Development");

            var transactionMock = new Mock<ITransaction>();
            _session.Setup(s => s.BeginTransaction()).Returns(transactionMock.Object);

            _consumerRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<ConsumerModel, bool>>>(), false)).ReturnsAsync(new ConsumerModel());

            var consumerLoginRequestMockDto = new ConsumerLoginRequestDto() { ConsumerCode = "test" };
            var consumerResponse = await _consumerLoginController.ConsumerLogin(consumerLoginRequestMockDto);
            var result = consumerResponse.Result as NotFoundObjectResult;
            Assert.True(result?.StatusCode == 404);
        }

        [Fact]
        public async Task Should_Check_If_Consumer_Is_Not_Subscriber_Type()
        {
            _vault.Setup(x => x.GetSecret(It.IsAny<string>())).ReturnsAsync("Development");
            var roleMap = new RoleMap();
            var transactionMock = new Mock<ITransaction>();
            _session.Setup(s => s.BeginTransaction()).Returns(transactionMock.Object);

            _roleRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<RoleModel, bool>>>(), false)).ReturnsAsync(new RoleModel() { RoleName = "Not_Subscriber" });

            _consumerLoginRepo.Setup(x => x.FindAsync(It.IsAny<Expression<Func<ConsumerLoginModel, bool>>>(), false))
                .ReturnsAsync(new List<ConsumerLoginModel>() { new ConsumerLoginModel() { RefreshTokenTs = new DateTime(2022, 10, 10, 00, 00, 00) } });

            var consumerLoginRequestMockDto = new ConsumerLoginRequestDto() { ConsumerCode = "cmr-c69905fc68ce4f36851f877bae38f22e" };
            var consumerResponse = await _consumerLoginService.CreateToken(consumerLoginRequestMockDto);
            Assert.Null(consumerResponse.Jwt);
        }

        [Fact]
        public async Task Should_Catch_Login_Controller_Level_Exception()
        {
            var service = new Mock<IConsumerLoginService>();
            service.Setup(x => x.CreateToken(It.IsAny<ConsumerLoginRequestDto>())).ThrowsAsync(new Exception("intended exception"));

            var consumerLoginRequestMockDto = new ConsumerLoginRequestDto();
            var consumerLoginController = new ConsumerLoginController(_consumerLoginControllerLogger.Object, service.Object);
            var consumerResponse = await consumerLoginController.ConsumerLogin(consumerLoginRequestMockDto);
            Assert.True(consumerResponse.Value?.ErrorMessage == "intended exception");
        }

        [Fact]
        public async Task Should_Catch_ConsumerLogin_Service_Level_Internal_Exception()
        {
            _vault.Setup(x => x.GetSecret(It.IsAny<string>())).ReturnsAsync("Development");

            var transactionMock = new Mock<ITransaction>();
            _session.Setup(s => s.BeginTransaction()).Returns(transactionMock.Object);
            _session.Setup(x => x.UpdateAsync(It.IsAny<object>(), default)).ThrowsAsync(new Exception("intended exception"));

            _consumerLoginRepo.Setup(x => x.FindAsync(It.IsAny<Expression<Func<ConsumerLoginModel, bool>>>(), false))
                .ReturnsAsync(new List<ConsumerLoginModel>() { new ConsumerLoginModel() { RefreshTokenTs = new DateTime(2022, 10, 10, 00, 00, 00) } });

            var consumerLoginRequestMockDto = new ConsumerLoginRequestDto() { ConsumerCode = "cmr-c69905fc68ce4f36851f877bae38f22e" };
            var consumerResponse = await _consumerLoginService.CreateToken(consumerLoginRequestMockDto);
            Assert.True(consumerResponse.ErrorMessage == "intended exception");
        }

        [Fact]
        public async Task Should_Catch_ConsumerLogin_Service_Level_Exception()
        {
            _vault.Setup(x => x.GetSecret(It.IsAny<string>())).ReturnsAsync("Development");

            var transactionMock = new Mock<ITransaction>();
            _session.Setup(s => s.BeginTransaction()).Returns(transactionMock.Object);

            _consumerRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<ConsumerModel, bool>>>(), false)).ThrowsAsync(new Exception("intended exception"));

            var consumerLoginRequestMockDto = new ConsumerLoginRequestDto() { ConsumerCode = "cmr-c69905fc68ce4f36851f877bae38f22e" };
            var consumerResponse = await _consumerLoginService.CreateToken(consumerLoginRequestMockDto);
            Assert.True(consumerResponse.ErrorMessage == "intended exception");
        }

        [Fact]
        public async Task Should_Catch_ConsumerLogin_Service_ValidateApiToken_Level_Internal_Exception()
        {
            _vault.Setup(x => x.GetSecret(It.IsAny<string>())).ReturnsAsync("Development");
            var transactionMock = new Mock<ITransaction>();
            _session.Setup(s => s.BeginTransaction()).Returns(transactionMock.Object);
            _serverLoginRepo.Setup(x => x.FindAsync(It.IsAny<Expression<Func<ServerLoginModel, bool>>>(), false))
               .ThrowsAsync(new Exception("intended exception"));
            var consumerLoginRequestMockDto = new ConsumerLoginRequestDto() { ConsumerCode = "cmr-c69905fc68ce4f36851f877bae38f22e",ApiToken= "AIzaSyBO_Z628eqFWQ00YP0y87bIN-CeoL7REIY" };
            var consumerResponse = await _consumerLoginService.CreateToken(consumerLoginRequestMockDto);
            Assert.True(consumerResponse.ErrorMessage == "intended exception");
        }

        //#2
        [Fact]
        public async Task Should_Refresh_Token()
        {
            _vault.Setup(x => x.GetSecret(It.IsAny<string>())).ReturnsAsync("Development");

            var transactionMock = new Mock<ITransaction>();
            _session.Setup(s => s.BeginTransaction()).Returns(transactionMock.Object);

            var refreshTokenRequestDto = new RefreshTokenRequestDto() { ConsumerCode = "cmr-c69905fc68ce4f36851f877bae38f22e", AccessToken = "ValidToken" };
            var consumerResponse = await _consumerLoginController.RefreshToken(refreshTokenRequestDto);
            var result = consumerResponse.Result as OkObjectResult;
            Assert.True(result?.StatusCode == 200);
        }

        [Fact]
        public async Task Should_Check_Secret_Null_Path()
        {
            _vault.Setup(x => x.GetSecret(It.IsAny<string>())).ReturnsAsync("<SECRET_NOT_FOUND>");
            _vault.Setup(x => x.InvalidSecret).Returns("<SECRET_NOT_FOUND>");

            var refreshTokenRequestDto = new RefreshTokenRequestDto() { ConsumerCode = "cmr-c69905fc68ce4f36851f877bae38f22e", AccessToken = "InValidToken" };
            var consumerResponse = await _consumerLoginController.RefreshToken(refreshTokenRequestDto);
            var result = consumerResponse.Result as ObjectResult;
            Assert.True(result?.StatusCode == 500);
        }

        [Fact]
        public async Task Should_Check_Consumer_Null_Path()
        {
            _vault.Setup(x => x.GetSecret(It.IsAny<string>())).ReturnsAsync("Development");

            var transactionMock = new Mock<ITransaction>();
            _session.Setup(s => s.BeginTransaction()).Returns(transactionMock.Object);

            _consumerRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<ConsumerModel, bool>>>(), false)).ReturnsAsync(new ConsumerModel());

            var refreshTokenRequestDto = new RefreshTokenRequestDto() { ConsumerCode = "cmr-c69905fc68ce4f36851f877bae38f22e", AccessToken = "InValidToken" };
            var consumerResponse = await _consumerLoginController.RefreshToken(refreshTokenRequestDto);
            var result = consumerResponse.Result as NotFoundObjectResult;
            Assert.True(result?.StatusCode == 404);
        }

        [Fact]
        public async Task Should_Check_ConsumerLogin_Null_Path()
        {
            _vault.Setup(x => x.GetSecret(It.IsAny<string>())).ReturnsAsync("Development");

            var transactionMock = new Mock<ITransaction>();
            _session.Setup(s => s.BeginTransaction()).Returns(transactionMock.Object);

            _consumerLoginRepo.Setup(x => x.FindAsync(It.IsAny<Expression<Func<ConsumerLoginModel, bool>>>(), false)).ReturnsAsync(new List<ConsumerLoginModel>());

            var refreshTokenRequestDto = new RefreshTokenRequestDto() { ConsumerCode = "cmr-c69905fc68ce4f36851f877bae38f22e", AccessToken = "InValidToken" };
            var consumerResponse = await _consumerLoginController.RefreshToken(refreshTokenRequestDto);
            var result = consumerResponse.Result as NotFoundObjectResult;
            Assert.True(result?.StatusCode == 404);
        }

        [Fact]
        public async Task Should_Check_Refresh_Threshold_False_Path()
        {
            _vault.Setup(x => x.GetSecret(It.IsAny<string>())).ReturnsAsync("Development");

            var transactionMock = new Mock<ITransaction>();
            _session.Setup(s => s.BeginTransaction()).Returns(transactionMock.Object);

            _consumerLoginRepo.Setup(x => x.FindAsync(It.IsAny<Expression<Func<ConsumerLoginModel, bool>>>(), false))
                .ReturnsAsync(new List<ConsumerLoginModel>() { new ConsumerLoginModel { RefreshTokenTs = new DateTime(2023, 08, 10, 00, 00, 00) } });

            var refreshTokenRequestDto = new RefreshTokenRequestDto() { ConsumerCode = "cmr-c69905fc68ce4f36851f877bae38f22e", AccessToken = "InValidToken" };
            var consumerResponse = await _consumerLoginController.RefreshToken(refreshTokenRequestDto);
            var result = consumerResponse.Result as BadRequestObjectResult;
            Assert.True(result?.StatusCode == 400);
        }

        [Fact]
        public async Task Should_Catch_RefreshToken_Controller_Level_Exception()
        {
            _vault.Setup(x => x.GetSecret(It.IsAny<string>())).ReturnsAsync("Development");

            var service = new Mock<IConsumerLoginService>();
            service.Setup(x => x.RefreshToken(It.IsAny<RefreshTokenRequestDto>())).ThrowsAsync(new Exception("intended exception"));

            var refreshTokenRequestDto = new RefreshTokenRequestDto() { ConsumerCode = "cmr-c69905fc68ce4f36851f877bae38f22e", AccessToken = "InValidToken" };
            var consumerLoginController = new ConsumerLoginController(_consumerLoginControllerLogger.Object, service.Object);
            var consumerResponse = await consumerLoginController.RefreshToken(refreshTokenRequestDto);
            Assert.True(consumerResponse?.Value?.ErrorMessage == "intended exception");
        }

        [Fact]
        public async Task Should_Catch_RefreshToken_Service_Level_Exception()
        {
            _vault.Setup(x => x.GetSecret(It.IsAny<string>())).ReturnsAsync("Development");

            _consumerRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<ConsumerModel, bool>>>(), false)).ThrowsAsync(new Exception("intended exception"));

            var consumerLoginService = new ConsumerLoginService(_consumerLoginServiceLogger.Object, _vault.Object, _session.Object, _consumerRepo.Object,
                _consumerLoginRepo.Object, _personRepo.Object, _roleRepo.Object, _personRoleRepo.Object, _serverLoginRepo.Object, _tenantClient.Object,
                _encryptionHelper.Object);

            var refreshTokenRequestDto = new RefreshTokenRequestDto() { ConsumerCode = "cmr-c69905fc68ce4f36851f877bae38f22e", AccessToken = "InValidToken" };
            var consumerResponseMockDto = await consumerLoginService.RefreshToken(refreshTokenRequestDto);
            Assert.True(consumerResponseMockDto.ErrorMessage == "intended exception");
        }

        //#3
        [Fact]
        public async Task Should_Check_If_Token_Is_Valid()
        {
            var jwtPayload = new JwtPayload()
            {
                ConsumerCode = "cmr-c69905fc68ce4f36851f877bae38f22e",
                Email = "test@test.com",
                PersonUniqueIdentifier = "123",
                TenantCode = "ten-ecada21e57154928a2bb959e8365b8b4",
                Role = "subscriber",
                Expiry = DateTime.UtcNow.AddDays(1),
                Environment = "Development",
            };

            _consumerLoginRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<ConsumerLoginModel, bool>>>(), false))
                .ReturnsAsync(new ConsumerLoginMockModel() { AccessToken = await GenerateJWToken(jwtPayload) });

            var validateTokenRequestDto = new ValidateTokenRequestDto() { AccessToken = "Valid" };
            var response = await _consumerLoginController.ValidateToken(validateTokenRequestDto);
            var result = response.Result as OkObjectResult;
            Assert.True(result?.StatusCode == 200);
        }

        [Fact]
        public async Task Should_Check_If_Token_Is_InValid()
        {
            var validateTokenRequestDto = new ValidateTokenRequestDto() { AccessToken = "InValid" };
            var response = await _consumerLoginController.ValidateToken(validateTokenRequestDto);
            var result = response.Result as OkObjectResult;
            Assert.True(result?.StatusCode == 200);
        }

        [Fact]
        public async Task Should_Check_If_Request_Dto_Is_NullOrEmpty()
        {
            var validateTokenRequestDto = new ValidateTokenRequestDto();
            var response = await _consumerLoginController.ValidateToken(validateTokenRequestDto);
            var result = response.Result as BadRequestObjectResult;
            Assert.True((string?)result?.Value == "token cannot be null/empty");
            Assert.True(result.StatusCode == 400);
        }

        [Fact]
        public async Task Should_Check_If_Token_Is_NullOrEmpty()
        {
            _consumerLoginRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<ConsumerLoginModel, bool>>>(), false))
                .ReturnsAsync(new ConsumerLoginMockModel() { AccessToken = string.Empty });

            var validateTokenRequestDto = new ValidateTokenRequestDto() { AccessToken = "Test" };
            var response = await _consumerLoginController.ValidateToken(validateTokenRequestDto);
            var result = response.Result as NotFoundObjectResult;
            Assert.True(((Common.Core.Domain.Dtos.BaseResponseDto?)result?.Value)?.ErrorMessage == "token not found");
            Assert.True(result?.StatusCode == 404);
        }

        [Fact]
        public async Task Should_Catch_ValidateToken_Controller_Level_Exception()
        {
            var consumerLoginService = new Mock<IConsumerLoginService>();
            consumerLoginService.Setup(x => x.ValidateToken(It.IsAny<ValidateTokenRequestDto>()))
                .ThrowsAsync(new Exception("intended exception"));

            var consumerLoginController = new ConsumerLoginController(_consumerLoginControllerLogger.Object, consumerLoginService.Object);

            var validateTokenRequestDto = new ValidateTokenRequestDto() { AccessToken = "test" };
            var response = await consumerLoginController.ValidateToken(validateTokenRequestDto);
            Assert.True(response.Value?.ErrorMessage == "intended exception");
        }

        [Fact]
        public async Task Should_Catch_ValidateToken_Service_Level_Exception()
        {
            _consumerLoginRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<ConsumerLoginModel, bool>>>(), false))
                .ThrowsAsync(new Exception("intended exception"));

            var validateTokenRequestDto = new ValidateTokenRequestDto() { AccessToken = "test" };
            var response = await _consumerLoginService.ValidateToken(validateTokenRequestDto);
            Assert.True(response.ErrorMessage == "intended exception");
        }

        [Fact]
        public async Task ConsumerLogin_Should_Returns_Unauthorized_Result_When_Tenant_Not_Available()
        {
            // Arrange
            var consumerLoginRequestMockDto = new ConsumerLoginRequestDto() { ConsumerCode = "Test ConsumerCode", ApiToken = "Test ApiToken" };
            var expectedResponseDto = new ConsumerLoginResponseDto()
            {
                ErrorCode = StatusCodes.Status401Unauthorized,
                ErrorMessage = "Please provide a valid ApiToken"
            };
            _vault.Setup(x => x.GetSecret(It.IsAny<string>())).ReturnsAsync("Development");
            _tenantClient.Setup(c => c.Post<GetTenantByTenantCodeResponseDto>("tenant/get-by-tenant-code", It.IsAny<GetTenantByTenantCodeRequestDto>()))
                .ReturnsAsync(new TenantMockDto() { TenantCode = null });

            // Act
            var result = await _consumerLoginController.ConsumerLogin(consumerLoginRequestMockDto);

            // Assert
            Assert.IsType<UnauthorizedObjectResult>(result.Result);
            var unauthorizedObjectResult = result.Result as UnauthorizedObjectResult;
            var responseDto = unauthorizedObjectResult?.Value as ConsumerLoginResponseDto;
            Assert.Equal(expectedResponseDto.ErrorCode, responseDto?.ErrorCode);
            Assert.Equal(expectedResponseDto.ErrorMessage, responseDto?.ErrorMessage);
        }

        [Fact]
        public async Task ConsumerLogin_Should_Returns_Ok_Object_Result_When_Tenant_Not_Enable_Server_Login()
        {
            // Arrange
            var consumerLoginRequestMockDto = new ConsumerLoginRequestDto() { ConsumerCode = "Test ConsumerCode", ApiToken = "Test ApiToken" };
            var transactionMock = new Mock<ITransaction>();

            _vault.Setup(x => x.GetSecret(It.IsAny<string>())).ReturnsAsync("Development");
            _tenantClient.Setup(c => c.Post<GetTenantByTenantCodeResponseDto>("tenant/get-by-tenant-code", It.IsAny<GetTenantByTenantCodeRequestDto>()))
                .ReturnsAsync(new TenantMockDto() { TenantCode = "123", EnableServerLogin = false });
            _session.Setup(s => s.BeginTransaction()).Returns(transactionMock.Object);

            // Act
            var result = await _consumerLoginController.ConsumerLogin(consumerLoginRequestMockDto);

            // Assert
            Assert.IsType<OkObjectResult>(result.Result);
            var okObjectResult = result.Result as OkObjectResult;
            Assert.Equal(StatusCodes.Status200OK, okObjectResult?.StatusCode);
        }

        [Fact]
        public async Task ConsumerLogin_Should_Returns_Unauthorized_Result_When_Server_Login_Not_Available()
        {
            // Arrange
            var consumerLoginRequestMockDto = new ConsumerLoginRequestDto() { ConsumerCode = "Test ConsumerCode", ApiToken = "Test ApiToken" };
            var expectedResponseDto = new ConsumerLoginResponseDto()
            {
                ErrorCode = StatusCodes.Status401Unauthorized,
                ErrorMessage = "Please provide a valid ApiToken"
            };
            _vault.Setup(x => x.GetSecret(It.IsAny<string>())).ReturnsAsync("Development");
            _tenantClient.Setup(c => c.Post<GetTenantByTenantCodeResponseDto>("tenant/get-by-tenant-code", It.IsAny<GetTenantByTenantCodeRequestDto>()))
                .ReturnsAsync(new TenantMockDto() { TenantCode = "123", EnableServerLogin = true });
            _serverLoginRepo.Setup(x => x.FindAsync(It.IsAny<Expression<Func<ServerLoginModel, bool>>>(), false));
            // Act
            var result = await _consumerLoginController.ConsumerLogin(consumerLoginRequestMockDto);

            // Assert
            Assert.IsType<UnauthorizedObjectResult>(result.Result);
            var unauthorizedObjectResult = result.Result as UnauthorizedObjectResult;
            var responseDto = unauthorizedObjectResult?.Value as ConsumerLoginResponseDto;
            Assert.Equal(expectedResponseDto.ErrorCode, responseDto?.ErrorCode);
            Assert.Equal(expectedResponseDto.ErrorMessage, responseDto?.ErrorMessage);
        }

        [Fact]
        public async Task ConsumerLogin_Should_Returns_Unauthorized_Result_When_ApiToken_Expired()
        {
            // Arrange
            var consumerLoginRequestMockDto = new ConsumerLoginRequestDto() { ConsumerCode = "Test ConsumerCode", ApiToken = "Test ApiToken" };
            var serverLoginMockModel = new ServerLoginMockModel() { RefreshTokenTs = DateTime.UtcNow.AddDays(-1) };
            var serverLoginMockModelList = new List<ServerLoginModel> { serverLoginMockModel };
            var expectedResponseDto = new ConsumerLoginResponseDto()
            {
                ErrorCode = StatusCodes.Status401Unauthorized,
                ErrorMessage = "Please provide a valid ApiToken"
            };
            _vault.Setup(x => x.GetSecret(It.IsAny<string>())).ReturnsAsync("Development");
            _tenantClient.Setup(c => c.Post<GetTenantByTenantCodeResponseDto>("tenant/get-by-tenant-code", It.IsAny<GetTenantByTenantCodeRequestDto>()))
                .ReturnsAsync(new TenantMockDto() { TenantCode = "123", EnableServerLogin = true });
            _serverLoginRepo.Setup(x => x.FindAsync(
             It.IsAny<Expression<Func<ServerLoginModel, bool>>>(), false))
             .ReturnsAsync(serverLoginMockModelList);
            // Act
            var result = await _consumerLoginController.ConsumerLogin(consumerLoginRequestMockDto);

            // Assert
            Assert.IsType<UnauthorizedObjectResult>(result.Result);
            var unauthorizedObjectResult = result.Result as UnauthorizedObjectResult;
            var responseDto = unauthorizedObjectResult?.Value as ConsumerLoginResponseDto;
            Assert.Equal(expectedResponseDto.ErrorCode, responseDto?.ErrorCode);
            Assert.Equal(expectedResponseDto.ErrorMessage, responseDto?.ErrorMessage);
        }

        [Fact]
        public async Task ConsumerLogin_Should_Returns_Ok_Object_Result_When_ApiToken_Is_Valid()
        {
            // Arrange
            var consumerLoginRequestMockDto = new ConsumerLoginRequestDto() { ConsumerCode = "Test ConsumerCode", ApiToken = "Test ApiToken" };
            var transactionMock = new Mock<ITransaction>();
            var serverLoginMockModel = new ServerLoginMockModel();
            var serverLoginMockModelList = new List<ServerLoginModel> { serverLoginMockModel };

            _vault.Setup(x => x.GetSecret(It.IsAny<string>())).ReturnsAsync("Development");
            _tenantClient.Setup(c => c.Post<GetTenantByTenantCodeResponseDto>("tenant/get-by-tenant-code", It.IsAny<GetTenantByTenantCodeRequestDto>()))
                .ReturnsAsync(new TenantMockDto() { TenantCode = "123", EnableServerLogin = true });
            _serverLoginRepo.Setup(x => x.FindAsync(
              It.IsAny<Expression<Func<ServerLoginModel, bool>>>(), false))
              .ReturnsAsync(serverLoginMockModelList);
            _session.Setup(s => s.BeginTransaction()).Returns(transactionMock.Object);

            // Act
            var result = await _consumerLoginController.ConsumerLogin(consumerLoginRequestMockDto);

            // Assert
            Assert.IsType<OkObjectResult>(result.Result);
            var okObjectResult = result.Result as OkObjectResult;
            Assert.Equal(StatusCodes.Status200OK, okObjectResult?.StatusCode);
        }


        private async Task<string> GenerateJWToken(JwtPayload jwtPayload)
        {
            // Create claims
            var claims = new[]
            {
            new Claim("consumer_code", jwtPayload.ConsumerCode ?? string.Empty),
            new Claim("email", jwtPayload.Email ?? string.Empty),
            new Claim("person_unique_identifier", jwtPayload.PersonUniqueIdentifier ?? string.Empty),
            new Claim("isSSOUser", jwtPayload.PersonUniqueIdentifier ?? string.Empty),
            new Claim("tenant_code", jwtPayload.TenantCode ?? string.Empty),
            new Claim("role", jwtPayload.Role ?? string.Empty),
            new Claim("exp", jwtPayload.Expiry.ToString() ?? string.Empty),
            new Claim("env", jwtPayload.Environment?.ToString() ?? string.Empty)
        };

            // Create token key
            string jwtSecretKey = "SunnyRewards_Helios_Secret_Key_2023";
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecretKey));

            // Create signing credentials
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Create token
            var token = new JwtSecurityToken(
                issuer: JwtSettings.Issuer,
                audience: JwtSettings.Audience,
                claims: claims,
                expires: jwtPayload.Expiry,
                signingCredentials: creds);

            // Serialize token to string
            var tokenHandler = new JwtSecurityTokenHandler();
            var accessToken = tokenHandler.WriteToken(token);

            return await Task.FromResult(accessToken);
        }

        [Fact]
        public async Task ConsumerLogin_Should_Returns_UnauthorizedObjectResult_Result_When_Tenant_Is_InValid()
        {
            // Arrange
            var consumerLoginRequestMockDto = new ConsumerLoginRequestDto() { EncKeyId = "Test KeyId", EncToken = "Test Enc Token" };

            // Act
            var result = await _consumerLoginController.ConsumerLogin(consumerLoginRequestMockDto);

            // Assert
            Assert.IsType<UnauthorizedObjectResult>(result.Result);
            var resultObject = result.Result as UnauthorizedObjectResult;
            resultObject?.StatusCode.Equals(StatusCodes.Status401Unauthorized);
        }


        [Fact]
        public async Task ConsumerLogin_Should_Returns_UnauthorizedObjectResult_Result_When_EncToken_Is_InValid()
        {
            // Arrange
            var consumerLoginRequestMockDto = new ConsumerLoginRequestDto() { TenantCode = "Test Tenant", EncKeyId = "Test KeyId" };

            // Act
            var result = await _consumerLoginController.ConsumerLogin(consumerLoginRequestMockDto);

            // Assert
            Assert.IsType<UnauthorizedObjectResult>(result.Result);
            var resultObject = result.Result as UnauthorizedObjectResult;
            resultObject?.StatusCode.Equals(StatusCodes.Status401Unauthorized);
        }

        [Fact]
        public async Task ConsumerLogin_Should_Returns_UnauthorizedObjectResult_When_symmetricEncKey_Is_InValid()
        {
            // Arrange
            var consumerLoginRequestMockDto = new ConsumerLoginRequestDto()
            {
                TenantCode = "Test Tenant",
                EncKeyId = "Test KeyId",
                EncToken = "Test Enc Token"
            };
            _vault.Setup(x => x.InvalidSecret).Returns("<SECRET_NOT_FOUND>");
            _vault.Setup(x => x.GetTenantSecret(consumerLoginRequestMockDto.TenantCode, SecretName.SymmetricEncryptionKey))
                .ReturnsAsync("<SECRET_NOT_FOUND>");

            // Act
            var result = await _consumerLoginController.ConsumerLogin(consumerLoginRequestMockDto);

            // Assert
            Assert.IsType<UnauthorizedObjectResult>(result.Result);
            var resultObject = result.Result as UnauthorizedObjectResult;
            resultObject?.StatusCode.Equals(StatusCodes.Status401Unauthorized);
        }

        [Fact]
        public async Task ConsumerLogin_Should_Returns_UnauthorizedObjectResult_When_jwtValidationKey_Is_InValid()
        {
            // Arrange
            var consumerLoginRequestMockDto = new ConsumerLoginRequestDto()
            {
                TenantCode = "Test Tenant",
                EncKeyId = "Test KeyId",
                EncToken = "Test Enc Token"
            };
            var symmetricEncryptionKey = "Test Symmetric Key";
            var decryptedJwt = "Test Decrypted Jwt";
            _vault.Setup(x => x.InvalidSecret).Returns("<SECRET_NOT_FOUND>");
            _vault.Setup(x => x.GetTenantSecret(consumerLoginRequestMockDto.TenantCode, SecretName.SymmetricEncryptionKey))
                .ReturnsAsync(symmetricEncryptionKey);

            _encryptionHelper.Setup(x => x.Decrypt(consumerLoginRequestMockDto.EncToken, Convert.FromBase64String(symmetricEncryptionKey)))
                .Returns(decryptedJwt);
            _vault.Setup(x => x.GetTenantSecret(consumerLoginRequestMockDto.TenantCode, SecretName.CustomerJwtValidationKey))
                .ReturnsAsync("<SECRET_NOT_FOUND>");

            // Act
            var result = await _consumerLoginController.ConsumerLogin(consumerLoginRequestMockDto);

            // Assert
            Assert.IsType<UnauthorizedObjectResult>(result.Result);
            var resultObject = result.Result as UnauthorizedObjectResult;
            resultObject?.StatusCode.Equals(StatusCodes.Status401Unauthorized);
        }

        [Fact]
        public async Task ConsumerLogin_Should_Returns_UnauthorizedObjectResult_When_jwtIssuer_Is_InValid()
        {
            // Arrange
            var consumerLoginRequestMockDto = new ConsumerLoginRequestDto()
            {
                TenantCode = "Test Tenant",
                EncKeyId = "Test KeyId",
                EncToken = "Test Enc Token"
            };
            var symmetricEncryptionKey = "Test Symmetric Key";
            var decryptedJwt = "Test Decrypted Jwt";
            var jwtValidationKey = "Test Jwt Validation Key";
            _vault.Setup(x => x.InvalidSecret).Returns("<SECRET_NOT_FOUND>");
            _vault.Setup(x => x.GetTenantSecret(consumerLoginRequestMockDto.TenantCode, SecretName.SymmetricEncryptionKey))
                .ReturnsAsync(symmetricEncryptionKey);

            _encryptionHelper.Setup(x => x.Decrypt(consumerLoginRequestMockDto.EncToken, Convert.FromBase64String(symmetricEncryptionKey)))
                .Returns(decryptedJwt);
            _vault.Setup(x => x.GetTenantSecret(consumerLoginRequestMockDto.TenantCode, SecretName.CustomerJwtValidationKey))
                .ReturnsAsync(jwtValidationKey);
            _vault.Setup(x => x.GetTenantSecret(consumerLoginRequestMockDto.TenantCode, SecretName.TokenIssuer))
                .ReturnsAsync("<SECRET_NOT_FOUND>");

            // Act
            var result = await _consumerLoginController.ConsumerLogin(consumerLoginRequestMockDto);

            // Assert
            Assert.IsType<UnauthorizedObjectResult>(result.Result);
            var resultObject = result.Result as UnauthorizedObjectResult;
            resultObject?.StatusCode.Equals(StatusCodes.Status401Unauthorized);
        }

        [Fact]
        public async Task ConsumerLogin_Should_Returns_forbiden_Result_When_jwt_token_Is_InValid()
        {
            // Arrange
            var consumerLoginRequestMockDto = new ConsumerLoginRequestDto()
            {
                TenantCode = "Test Tenant",
                EncKeyId = "Test KeyId",
                EncToken = "Test Enc Token"
            };
            var symmetricEncryptionKey = "Test Symmetric Key";
            var decryptedJwt = "Test Decrypted Jwt";
            var jwtValidationKey = "Test Jwt Validation Key";
            var tokenIssuer = "Test Token Issuer";
            var claims = new Dictionary<string, string>();

            _vault.Setup(x => x.InvalidSecret).Returns("<SECRET_NOT_FOUND>");
            _vault.Setup(x => x.GetTenantSecret(consumerLoginRequestMockDto.TenantCode, SecretName.SymmetricEncryptionKey))
                .ReturnsAsync(symmetricEncryptionKey);

            _encryptionHelper.Setup(x => x.Decrypt(consumerLoginRequestMockDto.EncToken, Convert.FromBase64String(symmetricEncryptionKey)))
                .Returns(decryptedJwt);
            _vault.Setup(x => x.GetTenantSecret(consumerLoginRequestMockDto.TenantCode, SecretName.CustomerJwtValidationKey))
                .ReturnsAsync(jwtValidationKey);
            _vault.Setup(x => x.GetTenantSecret(consumerLoginRequestMockDto.TenantCode, SecretName.TokenIssuer))
                .ReturnsAsync(tokenIssuer);

            // Act
            var result = await _consumerLoginController.ConsumerLogin(consumerLoginRequestMockDto);

            // Assert
            Assert.IsType<ObjectResult>(result.Result);
            var resultObject = result.Result as ObjectResult;
            resultObject?.StatusCode.Equals(StatusCodes.Status403Forbidden);
        }

        private static async Task<string> GenerateCustomerJWToken(string partnerCode, string memberNbr, string encKeyId, bool isClaimMissing = false)
        {
            // Create claims
            var claims = new List<Claim>
            {
                new Claim(Constant.PartnerCodeClaim, partnerCode),
                new Claim(Constant.MemberIdClaim, memberNbr)
            };

            if (!isClaimMissing)
            {
                claims.Add(new Claim(Constant.KeyIdClaim, encKeyId));
            }

            // Create token key
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtSettings.JwtSecretKey));   // secretKey from Helios.Common.Core

            // Create signing credentials
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Create token
            var token = new JwtSecurityToken(
                issuer: JwtSettings.Issuer,
                audience: JwtSettings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddDays(1),
                signingCredentials: creds);

            // Serialize token to string
            var tokenHandler = new JwtSecurityTokenHandler();
            var accessToken = tokenHandler.WriteToken(token);

            return await Task.FromResult(accessToken);
        }

        [Fact]
        public async Task ConsumerLogin_Should_Returns_forbiden_Result_When_encKeyId_Is_InValid()
        {
            // Arrange
            var consumerLoginRequestMockDto = new ConsumerLoginRequestDto()
            {
                TenantCode = "Test Tenant",
                EncKeyId = "Test KeyId",
                EncToken = "Test Enc Token"
            };
            var symmetricEncryptionKey = "Test Symmetric Key";
            var decryptedJwt = await GenerateCustomerJWToken("test_partner_code", "test_member_nbr", "test_enc_key_id");
            var jwtValidationKey = JwtSettings.JwtSecretKey;
            var tokenIssuer = JwtSettings.Issuer;
            var claims = new Dictionary<string, string>();
            var _consumerLoginServiceMock = new Mock<IConsumerLoginService>();

            _vault.Setup(x => x.InvalidSecret).Returns("<SECRET_NOT_FOUND>");
            _vault.Setup(x => x.GetTenantSecret(consumerLoginRequestMockDto.TenantCode, SecretName.SymmetricEncryptionKey))
                .ReturnsAsync(symmetricEncryptionKey);

            _encryptionHelper.Setup(x => x.Decrypt(consumerLoginRequestMockDto.EncToken, Convert.FromBase64String(symmetricEncryptionKey)))
                .Returns(decryptedJwt);
            _vault.Setup(x => x.GetTenantSecret(consumerLoginRequestMockDto.TenantCode, SecretName.CustomerJwtValidationKey))
                .ReturnsAsync(jwtValidationKey);
            _vault.Setup(x => x.GetTenantSecret(consumerLoginRequestMockDto.TenantCode, SecretName.TokenIssuer))
                .ReturnsAsync(tokenIssuer);

            _consumerLoginServiceMock.Setup(x => x.ValidateAndExtractClaims(decryptedJwt, jwtValidationKey, tokenIssuer, out claims)).Returns(true);
            // Act
            var result = await _consumerLoginController.ConsumerLogin(consumerLoginRequestMockDto);

            // Assert
            Assert.IsType<ObjectResult>(result.Result);
            var resultObject = result.Result as ObjectResult;
            resultObject?.StatusCode.Equals(StatusCodes.Status403Forbidden);
        }

        [Fact]
        public async Task ConsumerLogin_Should_Returns_forbiden_Result_When_partnerCode_Is_InValid()
        {
            // Arrange
            var consumerLoginRequestMockDto = new ConsumerLoginRequestDto()
            {
                TenantCode = "Test Tenant",
                EncKeyId = "Test KeyId",
                EncToken = "Test Enc Token"
            };
            var symmetricEncryptionKey = "Test Symmetric Key";
            var decryptedJwt = await GenerateCustomerJWToken("test_partner_code", "test_member_nbr", consumerLoginRequestMockDto.EncKeyId);
            var jwtValidationKey = JwtSettings.JwtSecretKey;
            var tokenIssuer = JwtSettings.Issuer;
            var claims = new Dictionary<string, string>();
            var _consumerLoginServiceMock = new Mock<IConsumerLoginService>();

            _vault.Setup(x => x.InvalidSecret).Returns("<SECRET_NOT_FOUND>");
            _vault.Setup(x => x.GetTenantSecret(consumerLoginRequestMockDto.TenantCode, SecretName.SymmetricEncryptionKey))
                .ReturnsAsync(symmetricEncryptionKey);

            _encryptionHelper.Setup(x => x.Decrypt(consumerLoginRequestMockDto.EncToken, Convert.FromBase64String(symmetricEncryptionKey)))
                .Returns(decryptedJwt);
            _vault.Setup(x => x.GetTenantSecret(consumerLoginRequestMockDto.TenantCode, SecretName.CustomerJwtValidationKey))
                .ReturnsAsync(jwtValidationKey);
            _vault.Setup(x => x.GetTenantSecret(consumerLoginRequestMockDto.TenantCode, SecretName.TokenIssuer))
                .ReturnsAsync(tokenIssuer);

            _consumerLoginServiceMock.Setup(x => x.ValidateAndExtractClaims(decryptedJwt, jwtValidationKey, tokenIssuer, out claims)).Returns(true);
            // Act
            var result = await _consumerLoginController.ConsumerLogin(consumerLoginRequestMockDto);

            // Assert
            Assert.IsType<ObjectResult>(result.Result);
            var resultObject = result.Result as ObjectResult;
            resultObject?.StatusCode.Equals(StatusCodes.Status403Forbidden);
        }

        [Fact]
        public async Task ConsumerLogin_Should_Returns_internal_server_error_Result_When_partnerCode_Is_Valid()
        {
            // Arrange
            var consumerLoginRequestMockDto = new ConsumerLoginRequestDto()
            {
                TenantCode = "Test Tenant",
                EncKeyId = "Test KeyId",
                EncToken = "Test Enc Token"
            };
            var symmetricEncryptionKey = "Test Symmetric Key";
            var decryptedJwt = await GenerateCustomerJWToken("par-7e92b06aa4fe405198d27d2427bf3de4", "test_member_nbr", consumerLoginRequestMockDto.EncKeyId);
            var jwtValidationKey = JwtSettings.JwtSecretKey;
            var tokenIssuer = JwtSettings.Issuer;
            var claims = new Dictionary<string, string>();
            var _consumerLoginServiceMock = new Mock<IConsumerLoginService>();

            _vault.Setup(x => x.InvalidSecret).Returns("<SECRET_NOT_FOUND>");
            _vault.Setup(x => x.GetTenantSecret(consumerLoginRequestMockDto.TenantCode, SecretName.SymmetricEncryptionKey))
                .ReturnsAsync(symmetricEncryptionKey);

            _encryptionHelper.Setup(x => x.Decrypt(consumerLoginRequestMockDto.EncToken, Convert.FromBase64String(symmetricEncryptionKey)))
                .Returns(decryptedJwt);
            _vault.Setup(x => x.GetTenantSecret(consumerLoginRequestMockDto.TenantCode, SecretName.CustomerJwtValidationKey))
                .ReturnsAsync(jwtValidationKey);
            _vault.Setup(x => x.GetTenantSecret(consumerLoginRequestMockDto.TenantCode, SecretName.TokenIssuer))
                .ReturnsAsync(tokenIssuer);

            _consumerLoginServiceMock.Setup(x => x.ValidateAndExtractClaims(decryptedJwt, jwtValidationKey, tokenIssuer, out claims)).Returns(true);

            _tenantClient.Setup(c => c.Post<GetTenantByPartnerCodeResponseDto>("tenant/get-by-partner-code", It.IsAny<GetTenantByPartnerCodeRequestDto>()))
                .ReturnsAsync(new GetTenantByPartnerCodeResponseDto()
                {
                    Tenant = new TenantDto()
                    {
                        TenantCode = consumerLoginRequestMockDto.TenantCode,
                        PartnerCode = "par-7e92b06aa4fe405198d27d2427bf3de4",
                        EncKeyId = consumerLoginRequestMockDto.EncKeyId
                    }
                });
            // Act
            var result = await _consumerLoginController.ConsumerLogin(consumerLoginRequestMockDto);

            // Assert
            Assert.IsType<ObjectResult>(result.Result);
            var resultObject = result.Result as ObjectResult;
            resultObject?.StatusCode.Equals(StatusCodes.Status500InternalServerError);
        }

        [Fact]
        public async Task ConsumerLogin_Should_Returns_forbiden_Result_When_claims_are_InValid()
        {
            // Arrange
            var consumerLoginRequestMockDto = new ConsumerLoginRequestDto()
            {
                TenantCode = "Test Tenant",
                EncKeyId = "Test KeyId",
                EncToken = "Test Enc Token"
            };
            var symmetricEncryptionKey = "Test Symmetric Key";
            var decryptedJwt = await GenerateCustomerJWToken("test_partner_code", "test_member_nbr", consumerLoginRequestMockDto.EncKeyId, true);
            var jwtValidationKey = JwtSettings.JwtSecretKey;
            var tokenIssuer = JwtSettings.Issuer;
            var claims = new Dictionary<string, string>();
            var _consumerLoginServiceMock = new Mock<IConsumerLoginService>();

            _vault.Setup(x => x.InvalidSecret).Returns("<SECRET_NOT_FOUND>");
            _vault.Setup(x => x.GetTenantSecret(consumerLoginRequestMockDto.TenantCode, SecretName.SymmetricEncryptionKey))
                .ReturnsAsync(symmetricEncryptionKey);

            _encryptionHelper.Setup(x => x.Decrypt(consumerLoginRequestMockDto.EncToken, Convert.FromBase64String(symmetricEncryptionKey)))
                .Returns(decryptedJwt);
            _vault.Setup(x => x.GetTenantSecret(consumerLoginRequestMockDto.TenantCode, SecretName.CustomerJwtValidationKey))
                .ReturnsAsync(jwtValidationKey);
            _vault.Setup(x => x.GetTenantSecret(consumerLoginRequestMockDto.TenantCode, SecretName.TokenIssuer))
                .ReturnsAsync(tokenIssuer);

            _consumerLoginServiceMock.Setup(x => x.ValidateAndExtractClaims(decryptedJwt, jwtValidationKey, tokenIssuer, out claims)).Returns(true);
            // Act
            var result = await _consumerLoginController.ConsumerLogin(consumerLoginRequestMockDto);

            // Assert
            Assert.IsType<ObjectResult>(result.Result);
            var resultObject = result.Result as ObjectResult;
            resultObject?.StatusCode.Equals(StatusCodes.Status403Forbidden);
        }

        [Fact]
        public void ConsumerLoginRepo_Constructor_ShouldInstantiate_WhenDependenciesAreProvided()
        {
            var mockLogger = new Mock<ILogger<BaseRepo<ConsumerLoginModel>>>();
            var mockSession = new Mock<NHibernate.ISession>();
            var repo = new Infrastructure.Repositories.ConsumerLoginRepo(mockLogger.Object, mockSession.Object);
            Assert.NotNull(repo);
        }

        [Fact]
        public void RoleRepo_Constructor_ShouldInstantiate_WhenDependenciesAreProvided()
        {
            var mockLogger = new Mock<ILogger<BaseRepo<RoleModel>>>();
            var mockSession = new Mock<NHibernate.ISession>();
            var repo = new Infrastructure.Repositories.RoleRepo(mockLogger.Object, mockSession.Object);
            Assert.NotNull(repo);
        }

        [Fact]
        public async Task GetConsumerLoginDetail_ShouldReturnNotFound_WhenLoginDateNotFound()
        {
            // Arrange
            var consumerCode = "missing-user";
            _consumerLoginRepo.Setup(r => r.GetFirstLoginDateAsync(It.IsAny<long>()))
                     .ReturnsAsync((DateTime?)null);

            // Act
            var result = await _consumerLoginController.GetConsumerLoginDetail(consumerCode);

            // Assert
            var notFound = Assert.IsType<NotFoundObjectResult>(result.Result);
            var dto = Assert.IsType<ConsumerLoginDateResponseDto>(notFound.Value);
            Assert.Equal(404, dto.ErrorCode);
            Assert.Equal("Invalid Login Date", dto.ErrorMessage);
        }

        [Fact]
        public async Task GetConsumerLoginDetail_ShouldReturnOk_WhenValidConsumer()
        {
            // Arrange
            var consumerCode = "valid-user";
            var expectedDate = new DateTime(2025, 9, 25);
            _consumerLoginRepo.Setup(r => r.GetFirstLoginDateAsync(It.IsAny<long>()))
                     .ReturnsAsync(expectedDate);

            // Act
            var result = await _consumerLoginController.GetConsumerLoginDetail(consumerCode);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var dto = Assert.IsType<ConsumerLoginDateResponseDto>(okResult.Value);
            Assert.Equal(consumerCode, dto.ConsumerCode);
            Assert.Equal(expectedDate, dto.LoginTs);
        }
        [Fact]
        public async Task GetConsumerLoginDetail_ShouldReturnBadRequest()
        {
            
            var result = await _consumerLoginController.GetConsumerLoginDetail(string.Empty);

            // Assert
            var requestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Equal(StatusCodes.Status400BadRequest, requestResult.StatusCode);
            
        }
        [Fact]
        public async Task GetConsumerLoginDetail_ShouldReturnErrorDto_WhenRepoThrows()
        {
            // Arrange
            var consumerCode = "throws-error";
            _consumerLoginRepo.Setup(r => r.GetFirstLoginDateAsync(It.IsAny<long>()))
                     .ThrowsAsync(new Exception("DB is down"));

            // Act
            var result = await _consumerLoginController.GetConsumerLoginDetail(consumerCode);

            // Assert
            var dto = Assert.IsType<ConsumerLoginDateResponseDto>(result.Value);
            Assert.Equal(StatusCodes.Status500InternalServerError, dto.ErrorCode);
        }
        [Fact]
        public async Task GetConsumerEngagementDetail_ShouldReturnNotFound_WhenLoginDateNotFound()
        {
            // Arrange
            var consumerLoginRequestDto = new GetConsumerEngagementDetailRequestDto
            {
                ConsumerCode = "missing-user",
                EngagementFrom = DateTime.UtcNow.AddDays(-10),
                EngagementUntil = DateTime.UtcNow
            };
            _consumerLoginRepo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<ConsumerLoginModel, bool>>>(), false));
                     

            // Act
            var result = await _consumerLoginController.GetConsumerEngagementDetail(consumerLoginRequestDto);

            // Assert
            var notFound = Assert.IsType<NotFoundObjectResult>(result.Result);
            var dto = Assert.IsType<ConsumerLoginDateResponseDto>(notFound.Value);
            Assert.Equal(404, dto.ErrorCode);
            Assert.Equal("Invalid Login Date", dto.ErrorMessage);
        }
        [Fact]
        public async Task GetConsumerEngagementDetail_ShouldReturnNotFound_WhenBadRequest()
        {
            // Arrange
            var consumerLoginRequestDto = new GetConsumerEngagementDetailRequestDto
            {
                EngagementFrom = DateTime.UtcNow.AddDays(-10),
                EngagementUntil = DateTime.UtcNow
            };
            _consumerLoginRepo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<ConsumerLoginModel, bool>>>(), false));


            // Act
            var result = await _consumerLoginController.GetConsumerEngagementDetail(consumerLoginRequestDto);

            // Assert
            var notFound = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Equal(StatusCodes.Status400BadRequest, notFound.StatusCode);
        }

        [Fact]
        public async Task GetConsumerEngagementDetail_ShouldReturnOk_WhenValidConsumer()
        {
            // Arrange
            var consumerLoginRequestDto = new GetConsumerEngagementDetailRequestDto
            {
                ConsumerCode = "user",
                EngagementFrom = DateTime.UtcNow.AddDays(-10),
                EngagementUntil = DateTime.UtcNow
            }; 
            _consumerLoginRepo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<ConsumerLoginModel, bool>>>(), false))

                     .ReturnsAsync(new List<ConsumerLoginModel> { new ConsumerLoginModel { ConsumerId=1} });

            // Act
            var result = await _consumerLoginController.GetConsumerEngagementDetail(consumerLoginRequestDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
        }
        [Fact]
        public async Task GetConsumerEngagementDetail_ShouldReturnErrorDto_WhenRepoThrows()
        {
            // Arrange
            var consumerLoginRequestDto = new GetConsumerEngagementDetailRequestDto
            {
                ConsumerCode = "user",
                EngagementFrom = DateTime.UtcNow.AddDays(-10),
                EngagementUntil = DateTime.UtcNow
            };
            _consumerLoginRepo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<ConsumerLoginModel, bool>>>(), false))
                     .ThrowsAsync(new Exception("DB is down"));

            // Act
            var result = await _consumerLoginController.GetConsumerEngagementDetail(consumerLoginRequestDto);

            // Assert
            var dto = Assert.IsType<ConsumerLoginDateResponseDto>(result.Value);
            Assert.Equal(StatusCodes.Status500InternalServerError, dto.ErrorCode);
        }


    }
}