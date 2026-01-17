using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NHibernate;
using SunnyRewards.Helios.Common.Core.Domain;
using SunnyRewards.Helios.Common.Core.Helpers.Interfaces;
using SunnyRewards.Helios.User.Api.Controllers;
using SunnyRewards.Helios.User.Core.Domain.Dtos;
using SunnyRewards.Helios.User.Core.Domain.Models;
using SunnyRewards.Helios.User.Infrastructure.HttpClients.Interfaces;
using SunnyRewards.Helios.User.Infrastructure.Mappings;
using SunnyRewards.Helios.User.Infrastructure.Repositories.Interfaces;
using SunnyRewards.Helios.User.Infrastructure.Services;
using SunnyRewards.Helios.User.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.User.UnitTest.Fixtures.MockHttpClient;
using SunnyRewards.Helios.User.UnitTest.Fixtures.MockRepositories;
using System.Linq.Expressions;
using Xunit;


namespace SunnyRewards.Helios.User.UnitTest.Controllers
{
    public class AdminLoginControllerUnitTest
    {
        private readonly Mock<ILogger<AdminLoginController>> _adminLoginControllerLogger;
        private readonly Mock<ILogger<AdminLoginService>> _adminLoginServiceLogger;
        private readonly Mock<IVault> _vault;
        private readonly Mock<NHibernate.ISession> _session;
        private readonly Mock<IConsumerRepo> _consumerRepo;
        private readonly Mock<IConsumerLoginRepo> _consumerLoginRepo;
        private readonly Mock<IPersonRepo> _personRepo;
        private readonly Mock<IRoleRepo> _roleRepo;
        private readonly Mock<IPersonRoleRepo> _personRoleRepo;
        private readonly Mock<IServerLoginRepo> _serverLoginRepo;
        private readonly Mock<ITenantClient> _tenantClient;
        private readonly IAdminLoginService _adminLoginService;
        private readonly AdminLoginController _adminLoginController;
        private readonly Mock<IEncryptionHelper> _encryptionHelper;
        public AdminLoginControllerUnitTest()
        {
            _adminLoginControllerLogger = new Mock<ILogger<AdminLoginController>>();
            _adminLoginServiceLogger = new Mock<ILogger<AdminLoginService>>();
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

            _adminLoginService = new AdminLoginService(_adminLoginServiceLogger.Object, _vault.Object, _session.Object,
                _consumerRepo.Object, _consumerLoginRepo.Object, _personRepo.Object, _roleRepo.Object, _personRoleRepo.Object,
                _serverLoginRepo.Object, _tenantClient.Object, _encryptionHelper.Object);

            _adminLoginController = new AdminLoginController(_adminLoginControllerLogger.Object, _adminLoginService);
        }


        [Fact]
        public async Task Should_Return_Internal_Error_When_Environment_Configuration_Is_Invalid()
        {
            // Arrange
            _vault.Setup(x => x.GetSecret("env")).ReturnsAsync(string.Empty); // Simulate missing environment
            var adminLoginRequestMockDto = new AdminLoginRequestDto { ConsumerCode = "test" };

            // Act
            var adminResponse = await _adminLoginService.GenerateAdminTokenAsync(adminLoginRequestMockDto);

            // Assert
            Assert.NotNull(adminResponse);
            Assert.Equal(StatusCodes.Status500InternalServerError, adminResponse.ErrorCode);
            Assert.Equal("Internal Error", adminResponse.ErrorMessage);
        }

        [Fact]
        public async Task Should_Return_Internal_Error_When_Environment_Configuration_Is_InvalidSecret()
        {
            // Arrange
            _vault.Setup(x => x.GetSecret("env")).ReturnsAsync(_vault.Object.InvalidSecret); // Simulate invalid environment secret
            var adminLoginRequestMockDto = new AdminLoginRequestDto { ConsumerCode = "test" };

            // Act
            var adminResponse = await _adminLoginService.GenerateAdminTokenAsync(adminLoginRequestMockDto);

            // Assert
            Assert.NotNull(adminResponse);
            Assert.Equal(StatusCodes.Status500InternalServerError, adminResponse.ErrorCode);
            Assert.Equal("Internal Error", adminResponse.ErrorMessage);
        }
        
        [Fact]
        public async Task Should_Create_AccessToken_If_Refresh_Threshold_Is_Valid()
        {
            _vault.Setup(x => x.GetSecret(It.IsAny<string>())).ReturnsAsync("Development");

            var transactionMock = new Mock<ITransaction>();
            var adminLoginMap = new ConsumerLoginMap();
            _session.Setup(s => s.BeginTransaction()).Returns(transactionMock.Object);

            _consumerLoginRepo.Setup(x => x.FindAsync(It.IsAny<Expression<Func<ConsumerLoginModel, bool>>>(), false))
                .ReturnsAsync([new() { RefreshTokenTs = new DateTime(2023, 08, 10, 00, 00, 00) }]);

            var adminLoginRequestMockDto = new AdminLoginRequestDto() { ConsumerCode = "test" };
            var actionResult = await _adminLoginController.GenerateAdminTokenAsync(adminLoginRequestMockDto);
            var objectResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            Assert.Equal(StatusCodes.Status200OK, objectResult?.StatusCode);
        }

        [Fact]
        public async Task Should_Check_Refresh_Threshold_Based_On_ConsumerCode()
        {
            _vault.Setup(x => x.GetSecret(It.IsAny<string>())).ReturnsAsync("Development");

            var transactionMock = new Mock<ITransaction>();
            _session.Setup(s => s.BeginTransaction()).Returns(transactionMock.Object);

            var adminLoginRequestMockDto = new AdminLoginRequestDto() { ConsumerCode = "cmr-c69905fc68ce4f36851f877bae38f22e" };
            var actionResult = await _adminLoginController.GenerateAdminTokenAsync(adminLoginRequestMockDto);
            var objectResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            Assert.Equal(StatusCodes.Status200OK, objectResult?.StatusCode);
        }

        [Fact]
        public async Task Should_Return_NotFound_When_ConsumerCode_Is_Null()
        {
            _vault.Setup(x => x.GetSecret(It.IsAny<string>())).ReturnsAsync("Development");

            var transactionMock = new Mock<ITransaction>();
            _session.Setup(s => s.BeginTransaction()).Returns(transactionMock.Object);

            _consumerRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<ConsumerModel, bool>>>(), false)).ReturnsAsync(new ConsumerModel());

            var adminLoginRequestMockDto = new AdminLoginRequestDto() { ConsumerCode = "test" };
            var actionResult = await _adminLoginController.GenerateAdminTokenAsync(adminLoginRequestMockDto);
            var objectResult = Assert.IsType<NotFoundObjectResult>(actionResult.Result);
            Assert.Equal(StatusCodes.Status404NotFound, objectResult?.StatusCode);
        }

        [Fact]
        public async Task Should_Handle_Exception_At_Controller_Level()
        {
            var service = new Mock<IAdminLoginService>();
            service.Setup(x => x.GenerateAdminTokenAsync(It.IsAny<AdminLoginRequestDto>())).ThrowsAsync(new Exception("intended exception"));

            var adminLoginRequestMockDto = new AdminLoginRequestDto();
            var adminLoginController = new AdminLoginController(_adminLoginControllerLogger.Object, service.Object);
            var actionResult = await adminLoginController.GenerateAdminTokenAsync(adminLoginRequestMockDto);
            var objectResult = Assert.IsType<ObjectResult>(actionResult.Result);
            var adminResponse = Assert.IsType<AdminLoginResponseDto>(objectResult.Value);
            Assert.Equal("intended exception", adminResponse.ErrorMessage);
        }

        [Fact]
        public async Task Should_Handle_Internal_Exception_In_ConsumerLogin_Service()
        {
            _vault.Setup(x => x.GetSecret(It.IsAny<string>())).ReturnsAsync("Development");

            var transactionMock = new Mock<ITransaction>();
            _session.Setup(s => s.BeginTransaction()).Returns(transactionMock.Object);
            _session.Setup(x => x.UpdateAsync(It.IsAny<object>(), default)).ThrowsAsync(new Exception("intended exception"));

            _consumerLoginRepo.Setup(x => x.FindAsync(It.IsAny<Expression<Func<ConsumerLoginModel, bool>>>(), false))
                .ReturnsAsync([new ConsumerLoginModel() { RefreshTokenTs = new DateTime(2022, 10, 10, 00, 00, 00) }]);

            var adminLoginRequestMockDto = new AdminLoginRequestDto() { ConsumerCode = "cmr-c69905fc68ce4f36851f877bae38f22e" };
            var adminResponse = await _adminLoginService.GenerateAdminTokenAsync(adminLoginRequestMockDto);
            Assert.Equal("Value cannot be null. (Parameter 'source')", adminResponse.ErrorMessage);
        }

        [Fact]
        public async Task Should_Handle_Exception_In_ConsumerLogin_Service()
        {
            _vault.Setup(x => x.GetSecret(It.IsAny<string>())).ReturnsAsync("Development");

            var transactionMock = new Mock<ITransaction>();
            _session.Setup(s => s.BeginTransaction()).Returns(transactionMock.Object);

            _consumerRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<ConsumerModel, bool>>>(), false)).ThrowsAsync(new Exception("intended exception"));

            var adminLoginRequestMockDto = new AdminLoginRequestDto() { ConsumerCode = "cmr-c69905fc68ce4f36851f877bae38f22e" };
            var adminResponse = await _adminLoginService.GenerateAdminTokenAsync(adminLoginRequestMockDto);
            Assert.Equal("intended exception", adminResponse.ErrorMessage);
        }

        [Fact]
        public async Task Should_Handle_Internal_Exception_In_ValidateApiToken_Process()
        {
            _vault.Setup(x => x.GetSecret(It.IsAny<string>())).ReturnsAsync("Development");
            var transactionMock = new Mock<ITransaction>();
            _session.Setup(s => s.BeginTransaction()).Returns(transactionMock.Object);
            _serverLoginRepo.Setup(x => x.FindAsync(It.IsAny<Expression<Func<ServerLoginModel, bool>>>(), false))
               .ThrowsAsync(new Exception("intended exception"));
            var adminLoginRequestMockDto = new AdminLoginRequestDto() { ConsumerCode = "cmr-c69905fc68ce4f36851f877bae38f22e"};
            var adminResponse = await _adminLoginService.GenerateAdminTokenAsync(adminLoginRequestMockDto);
            Assert.Equal("Value cannot be null. (Parameter 'source')", adminResponse.ErrorMessage);
        }

        [Fact]
        public async Task Should_Return_Valid_Token_When_Token_Is_Still_Valid()
        {
            // Arrange
            _vault.Setup(x => x.GetSecret("env")).ReturnsAsync("Development"); // Simulate missing environment
            var validTokenTimestamp = DateTime.UtcNow.AddSeconds(-Constants.USER_JWT_EXPIRY_SECONDS + 10); // Token within expiry time
            var consumerLoginModel = new ConsumerLoginModel
            {
                RefreshTokenTs = validTokenTimestamp,
                AccessToken = "validAccessToken"
            };
            _consumerLoginRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<ConsumerLoginModel, bool>>>(), false)).ReturnsAsync(consumerLoginModel);
            var adminLoginRequestMockDto = new AdminLoginRequestDto { ConsumerCode = "cmr-c69905fc68ce4f36851f877bae38f22e" };

            // Arrange
            var consumer = new ConsumerModel { PersonId = 123 };

            // Mock data for PersonRoles
            var personRoles = new List<PersonRoleModel>
            {
                new() { PersonId = 123, RoleId = 1, DeleteNbr = 0, CustomerCode = "C1", SponsorCode = "S1", TenantCode = "T1" },
                new() { PersonId = 123, RoleId = 2, DeleteNbr = 0, CustomerCode = "C2", SponsorCode = "S2", TenantCode = "T2" }
            };

            // Mock data for Roles
            var roles = new List<RoleModel>
            {
                new() { RoleId = 1, RoleName = "Admin", DeleteNbr = 0 },
                new() { RoleId = 2, RoleName = "User", DeleteNbr = 0 }
            };

            // Setup repository mocks
            _personRoleRepo.Setup(x => x.FindAsync(It.IsAny<Expression<Func<PersonRoleModel, bool>>>(), false))
            .ReturnsAsync(personRoles);

            _roleRepo.Setup(x => x.FindAsync(It.IsAny<Expression<Func<RoleModel, bool>>>(), false))
                .ReturnsAsync(roles);

            var transactionMock = new Mock<ITransaction>();
            _session.Setup(s => s.BeginTransaction()).Returns(transactionMock.Object);

            // Act
            var adminResponse = await _adminLoginService.GenerateAdminTokenAsync(adminLoginRequestMockDto);

            // Assert
            Assert.NotNull(adminResponse);
            Assert.Equal("InValidToken", adminResponse.Jwt);
            Assert.Equal("cmr-c69905fc68ce4f36851f877bae38f22e", adminResponse.ConsumerCode);
        }


        [Fact]
        public async Task Should_Create_New_Token_When_Previous_Token_Is_Expired()
        {
            // Arrange
            _vault.Setup(x => x.GetSecret("env")).ReturnsAsync("Development"); // Simulate missing environment
            _vault.Setup(x => x.GetSecret("JWT_SECRET_KEY")).ReturnsAsync("SunnyRewards_Helios_Secret_Key_2023"); // Simulate missing environment

            // Mock data for ConsumerLogin
            var consumerLoginData = new List<ConsumerLoginModel>
            {
                new() { ConsumerLoginId = 1, ConsumerId = 1, DeleteNbr = 0, AccessToken = "token1", RefreshTokenTs = DateTime.UtcNow.AddDays(-8) },
                new() { ConsumerLoginId = 2, ConsumerId = 1, DeleteNbr = 0, AccessToken = "token2", RefreshTokenTs = DateTime.UtcNow.AddDays(-4) }, // Latest entry
                new() { ConsumerLoginId = 3, ConsumerId = 1, DeleteNbr = 0, AccessToken = "token3", RefreshTokenTs = DateTime.UtcNow.AddDays(-2) }  // Different ConsumerId
            };

            // Arrange
            var consumer = new ConsumerModel { ConsumerId = 1, PersonId = 1 };

            // Mock FindAsync to return consumerLoginData filtered by the predicate
            _consumerLoginRepo.Setup(x => x.FindAsync(It.IsAny<Expression<Func<ConsumerLoginModel, bool>>>(), false))
                .ReturnsAsync(consumerLoginData);


            var consumerLogin = (await _consumerLoginRepo.Object.FindAsync(x => x.ConsumerId == consumer.ConsumerId && x.DeleteNbr == 0))
                .OrderByDescending(x => x.ConsumerLoginId)
                .FirstOrDefault();

            var adminLoginRequestMockDto = new AdminLoginRequestDto { ConsumerCode = "cmr-c69905fc68ce4f36851f877bae38f22e" };

            // Mock data for PersonRoles
            var personRoles = new List<PersonRoleModel>
            {
                new() { PersonId = 1, RoleId = 1, DeleteNbr = 0, CustomerCode = "C1", SponsorCode = "S1", TenantCode = "T1" },
                new() { PersonId = 1, RoleId = 2, DeleteNbr = 0, CustomerCode = "C2", SponsorCode = "S2", TenantCode = "T2" }
            };

            // Mock data for Roles
            var roles = new List<RoleModel>
            {
                new() { RoleId = 1, RoleName = "Admin", DeleteNbr = 0 },
                new() { RoleId = 2, RoleName = "User", DeleteNbr = 0 }
            };

            // Setup repository mocks
            _personRoleRepo.Setup(x => x.FindAsync(It.IsAny<Expression<Func<PersonRoleModel, bool>>>(), false))
            .ReturnsAsync(personRoles);

            _roleRepo.Setup(x => x.FindAsync(It.IsAny<Expression<Func<RoleModel, bool>>>(), false))
                .ReturnsAsync(roles);

            var transactionMock = new Mock<ITransaction>();
            _session.Setup(s => s.BeginTransaction()).Returns(transactionMock.Object);

            // Act
            var adminResponse = await _adminLoginService.GenerateAdminTokenAsync(adminLoginRequestMockDto);

            // Assert
            Assert.NotNull(adminResponse);
            Assert.NotNull(adminResponse.Jwt);
            Assert.Equal(2, adminResponse.Acl.Count);
            Assert.Equal("cmr-c69905fc68ce4f36851f877bae38f22e", adminResponse.ConsumerCode);
        }
    }
}