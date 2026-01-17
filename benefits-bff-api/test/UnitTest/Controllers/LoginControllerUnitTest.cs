using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using NSubstitute.ExceptionExtensions;
using Sunny.Benefits.Bff.Api.Controllers;
using Sunny.Benefits.Bff.Core.Constants;
using Sunny.Benefits.Bff.Core.Domain.Constants;
using Sunny.Benefits.Bff.Core.Domain.Dtos;
using Sunny.Benefits.Bff.Infrastructure.Helpers;
using Sunny.Benefits.Bff.Infrastructure.Helpers.Interface;
using Sunny.Benefits.Bff.Infrastructure.HttpClients.Interfaces;
using Sunny.Benefits.Bff.Infrastructure.Repositories.Interfaces;
using Sunny.Benefits.Bff.Infrastructure.Services;
using Sunny.Benefits.Bff.Infrastructure.Services.Interfaces;
using Sunny.Benefits.Bff.UnitTest.Fixtures.MockDtos;
using Sunny.Benefits.Bff.UnitTest.HttpClients;
using Sunny.Benefits.Cms.Core.Domain.Dtos;
using SunnyBenefits.Fis.Core.Domain.Dtos;
using SunnyRewards.Helios.Admin.Core.Domain.Dtos;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.Common.Core.Helpers.Interfaces;
using SunnyRewards.Helios.Common.Core.Services.Interfaces;
using SunnyRewards.Helios.User.Core.Domain.Dtos;
using SunnyRewards.Helios.User.Core.Domain.enums;
using System.Web;
using Xunit;

namespace Sunny.Benefits.Bff.UnitTest.Controllers
{
    public class LoginControllerUnitTest
    {
        private readonly Mock<ILogger<LoginController>> _loginControllerLogger;
        private readonly Mock<ILogger<LoginService>> _loginServiceLogger;
        private readonly Mock<ILogger<EventService>> _eventServiceLogger;
        private readonly Mock<IUserClient> _userClient;
        private readonly Mock<ILogger<Auth0Helper>> _auth0HelperLogger;
        private readonly Mock<IVault> _vault;
        private readonly Mock<IConfiguration> _configuration;
        private readonly ILoginService _loginService;
        private readonly LoginController _loginController;
        private readonly IAuth0Helper _auth0Helper;
        private readonly Mock<IFisClient> _fisClient;
        private readonly Mock<ILoginService> _mockLoginService;
        private readonly LoginController _controller;
        private readonly Mock<IPersonHelper> _personHelper;
        private readonly Mock<IHashingService> _hashingService;
        private readonly Mock<ICmsService> _UploadPdfService;
        private readonly IEventService _eventService;
        private readonly Mock<IAdminClient> _adminClient;
        private readonly Mock<ITenantClient> _tenantClient;
        private readonly Mock<IHttpContextAccessor> _httpContextAccessor;
        private readonly Mock<IAuth0TokenCacheService> _tokenCacheService;

        public LoginControllerUnitTest()
        {
            _loginControllerLogger = new Mock<ILogger<LoginController>>();
            _loginServiceLogger = new Mock<ILogger<LoginService>>();
            _eventServiceLogger = new Mock<ILogger<EventService>>();
            _userClient = new UserClientMock();
            _fisClient = new Mock<IFisClient>();
            _adminClient = new Mock<IAdminClient>();
            _personHelper = new Mock<IPersonHelper>();
            _auth0HelperLogger = new Mock<ILogger<Auth0Helper>>();
            _vault = new Mock<IVault>();
            _configuration = new Mock<IConfiguration>();
            _hashingService = new Mock<IHashingService>();
            _UploadPdfService = new Mock<ICmsService>();
            _tenantClient = new TenantClientMock();
            _configuration.Setup(c => c.GetSection("OperationMaxTries").Value).Returns("1");
            _httpContextAccessor = new Mock<IHttpContextAccessor>();
            _tokenCacheService = new Mock<IAuth0TokenCacheService>();
            _tokenCacheService.Setup(x => x.GetTokenAsync())
                .ReturnsAsync(new TokenResponse { access_token = "test-token", expires_in = "3600" });
            _auth0Helper = new Auth0Helper(_auth0HelperLogger.Object, _vault.Object, _configuration.Object, _userClient.Object, _personHelper.Object, _hashingService.Object, _httpContextAccessor.Object, _tenantClient.Object, _tokenCacheService.Object);
            _eventService = new EventService(_eventServiceLogger.Object, _adminClient.Object, _fisClient.Object);
            _loginService = new LoginService(_loginServiceLogger.Object, _userClient.Object, _fisClient.Object, _personHelper.Object, _eventService, _vault.Object, _httpContextAccessor.Object, _UploadPdfService.Object);
            _loginController = new LoginController(_loginControllerLogger.Object, _loginService, _auth0Helper);
            _mockLoginService = new Mock<ILoginService>();
            _controller = new LoginController(_loginControllerLogger.Object, _mockLoginService.Object, _auth0Helper);
        }

        [Fact]
        public async Task GetConsumerByEmail()
        {
            var email = "sunnyrewards@gmail.com";
            var loginServiceMock = new Mock<ILoginService>();
            loginServiceMock.Setup(x => x.GetConsumerByPersonUniqueIdentifier(email))
                           .ReturnsAsync(new GetConsumerByEmailResponseMockDto());
            var logincontroller = new LoginController(_loginControllerLogger.Object, loginServiceMock.Object, _auth0Helper);
            var response = await logincontroller.GetConsumerByEmail(email);
            var result = response.Result as OkObjectResult;
            Assert.True(result?.Value != null);
            Assert.True(result.StatusCode == 200);
        }

        [Fact]
        public async Task GetConsumerByEmail_ReturnsNotFound_WhenPersonIsNull()
        {
            var email = "sunnyrewards@gmail.com";
            var loginServiceMock = new Mock<ILoginService>();

            loginServiceMock.Setup(x => x.GetConsumerByPersonUniqueIdentifier(email))
                           .ReturnsAsync(new GetConsumerByEmailResponseMockDto { Person = null });
            var logincontroller = new LoginController(_loginControllerLogger.Object, loginServiceMock.Object, _auth0Helper);
            var result = await logincontroller.GetConsumerByEmail(email);
            Assert.Null(result?.Value?.Person);
        }

        [Fact]
        public async Task Catch_Exception_GetConsumerByEmail()
        {
            var email = "test@example.com";
            var loginServiceMock = new Mock<ILoginService>();
            loginServiceMock.Setup(x => x.GetConsumerByPersonUniqueIdentifier(email)).ThrowsAsync(new Exception("Simulated exception"));
            var logincontroller = new LoginController(_loginControllerLogger.Object, loginServiceMock.Object, _auth0Helper);
            var result = await logincontroller.GetConsumerByEmail(email);
            Assert.Null(result?.Value?.Consumer);
        }

        [Fact]
        public async Task GetConsumerByEmail_ValidEmail_ReturnsData_Service()
        {
            var email = "test@example.com";
            var getConsumerByEmailResponseMockDto = new GetConsumerByEmailResponseMockDto();
            _userClient.Setup(x => x.GetId<GetConsumerByEmailResponseDto>("consumer/get-consumers-by-email?email=", It.IsAny<Dictionary<string, string>>()))
                          .ReturnsAsync(getConsumerByEmailResponseMockDto);
            var loginService = new LoginService(_loginServiceLogger.Object, _userClient.Object, _fisClient.Object, _personHelper.Object, _eventService, _vault.Object, _httpContextAccessor.Object, _UploadPdfService.Object);
            var response = await loginService.GetConsumerByEmail(email);
            Assert.True(response != null);
        }

        [Fact]
        public async Task Catch_Exception_GetEmailByconsumer()
        {
            try
            {
                var email = "test@example.com";
                var getConsumerByEmailResponseMockDto = new GetConsumerByEmailResponseMockDto();
                _userClient.Setup(x => x.GetId<GetConsumerByEmailResponseDto>("consumer/get-consumers-by-email?email=", It.IsAny<Dictionary<string, string>>()))
                              .ThrowsAsync(new Exception("inner exception"));
                var loginService = new LoginService(_loginServiceLogger.Object, _userClient.Object, _fisClient.Object, _personHelper.Object, _eventService, _vault.Object, _httpContextAccessor.Object, _UploadPdfService.Object);
                var response = await loginService.GetConsumerByEmail(email);
                Assert.True(response != null);
            }
            catch (Exception ex)
            {

                ex.Message.ToString();
            }
        }

        [Fact]
        public async Task PatchUser_Return_OkResponse_Controller()
        {
            var patchUserRequestDto = new PatchUserRequestDto();
            var auth0Helper = new Mock<IAuth0Helper>();
            var logger = new Mock<ILogger<LoginController>>();
            var mockResponse = new UpdateResponseMockDto();
            auth0Helper.Setup(x => x.PatchUserOuter(It.IsAny<PatchUserRequestDto>())).ReturnsAsync(mockResponse);
            var controller = new LoginController(logger.Object, _loginService, auth0Helper.Object);
            var response = await controller.PatchUser(patchUserRequestDto);
            var result = response.Result as OkObjectResult;
            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
            Assert.Equal(mockResponse, result.Value);
        }

        [Fact]
        public async Task Catch_Exception_PatchUser_Exception_Controller()
        {
            var patchUserRequestDto = new PatchUserRequestMockDto();
            var auth0Helper = new Mock<IAuth0Helper>();
            var logger = new Mock<ILogger<LoginController>>();
            auth0Helper.Setup(x => x.PatchUserOuter(It.IsAny<PatchUserRequestDto>()))
            .ThrowsAsync(new Exception("Simulated exception"));
            var controller = new LoginController(logger.Object, _loginService, auth0Helper.Object);
            var response = await controller.PatchUser(patchUserRequestDto);
            Assert.NotNull(response?.Value);
        }

        [Fact]
        public async Task PatchUser_Return_OkResponse_Service()
        {
            var patchUserRequestDto = new PatchUserRequestMockDto();

            _vault.Setup(v => v.GetSecret("env")).ReturnsAsync("Development");
            _configuration.Setup(c => c.GetSection("Auth0:Auth0TokenUrl").Value).Returns("https://dev-sunny-benefits.us.auth0.com/oauth/token");

            _configuration.Setup(v => v.GetSection("Auth0:client_secret").Value).Returns("AUTH0_CLIENT_SECRET");
            _vault.Setup(c => c.GetSecret("AUTH0_CLIENT_SECRET")).ReturnsAsync("sHQ0KyJNq7leq7jEpYSLMsubqZ3Zb3hQJyPwF0iyCQbEoKpsrWm5LPgU23d8UBPc");

            var audienceSection0 = new Mock<IConfigurationSection>();
            audienceSection0.Setup(x => x.Value).Returns("https://dev-sunny-benefits.us.auth0.com/api/v2/");

            var audienceSection1 = new Mock<IConfigurationSection>();
            audienceSection1.Setup(x => x.Value).Returns("https://api.custom-audience.com/");

            var audienceArraySection = new Mock<IConfigurationSection>();
            audienceArraySection.Setup(x => x.GetChildren())
                .Returns(new List<IConfigurationSection> { audienceSection0.Object, audienceSection1.Object });

            _configuration.Setup(x => x.GetSection("Auth0:Audiences"))
                .Returns(audienceArraySection.Object);

            _configuration.Setup(v => v.GetSection("Auth0:client_id").Value).Returns("AUTH0_CLIENT_ID");
            _vault.Setup(c => c.GetSecret("AUTH0_CLIENT_ID")).ReturnsAsync("TmzHdUNt0sjiJkYFs3CEks0utYeo8FF5");

            _configuration.Setup(c => c.GetSection("Auth0:Domain").Value).Returns("dev-sunny-benefits.us.auth0.com");
            _configuration.Setup(c => c.GetSection("Auth0:grant_type").Value).Returns("client_credentials");

            _configuration.Setup(c => c.GetSection("Auth0:Auth0ApiUrl").Value).Returns("https://dev-sunny-benefits.us.auth0.com/api/v2/users/");

            _userClient.Setup(x => x.Post<BaseResponseDto>(It.IsAny<string>(), It.IsAny<PostConsumerDeviceRequestDto>())).ReturnsAsync(new BaseResponseDto());
            var httpContext = new DefaultHttpContext();
            httpContext.Items[HttpContextKeys.PersonUniqueIdentifier] = "abc_123";
            _httpContextAccessor.Setup(h => h.HttpContext).Returns(httpContext);

            var expectedResponse = new GetConsumerByPersonUniqueIdentifierResponseDto
            {
                Consumer = new[] { new ConsumerDto { MemberNbr = "999" } },
                Person = new PersonDto { Email = "abc@test.com", PersonUniqueIdentifier = "test123" }
            };

            _userClient.Setup(c => c.GetId<GetConsumerByPersonUniqueIdentifierResponseDto>(
                It.IsAny<string>(), It.IsAny<Dictionary<string, string>>()))
                .ReturnsAsync(expectedResponse);
            var response = await _loginController.PatchUser(patchUserRequestDto);
            var result = response.Result as OkObjectResult;
            Assert.True(result?.Value != null);
            Assert.True(result.StatusCode == 200);
        }

        [Fact]
        public async Task PatchUser_Return_NotFound_When_PersonUniqueIdentifier_Or_Email_NotFound()
        {
            var patchUserRequestDto = new PatchUserRequestMockDto();

            _vault.Setup(v => v.GetSecret("env")).ReturnsAsync("Development");
            _configuration.Setup(c => c.GetSection("Auth0:Auth0TokenUrl").Value).Returns("https://dev-sunny-benefits.us.auth0.com/oauth/token");

            _configuration.Setup(v => v.GetSection("Auth0:client_secret").Value).Returns("AUTH0_CLIENT_SECRET");
            _vault.Setup(c => c.GetSecret("AUTH0_CLIENT_SECRET")).ReturnsAsync("sHQ0KyJNq7leq7jEpYSLMsubqZ3Zb3hQJyPwF0iyCQbEoKpsrWm5LPgU23d8UBPc");

            var audienceSection0 = new Mock<IConfigurationSection>();
            audienceSection0.Setup(x => x.Value).Returns("https://dev-sunny-benefits.us.auth0.com/api/v2/");

            var audienceSection1 = new Mock<IConfigurationSection>();
            audienceSection1.Setup(x => x.Value).Returns("https://api.custom-audience.com/");

            var audienceArraySection = new Mock<IConfigurationSection>();
            audienceArraySection.Setup(x => x.GetChildren())
                .Returns(new List<IConfigurationSection> { audienceSection0.Object, audienceSection1.Object });

            _configuration.Setup(x => x.GetSection("Auth0:Audiences"))
                .Returns(audienceArraySection.Object);

            _configuration.Setup(v => v.GetSection("Auth0:client_id").Value).Returns("AUTH0_CLIENT_ID");
            _vault.Setup(c => c.GetSecret("AUTH0_CLIENT_ID")).ReturnsAsync("TmzHdUNt0sjiJkYFs3CEks0utYeo8FF5");

            _configuration.Setup(c => c.GetSection("Auth0:Domain").Value).Returns("dev-sunny-benefits.us.auth0.com");
            _configuration.Setup(c => c.GetSection("Auth0:grant_type").Value).Returns("client_credentials");

            _configuration.Setup(c => c.GetSection("Auth0:Auth0ApiUrl").Value).Returns("https://dev-sunny-benefits.us.auth0.com/api/v2/users/");

            _userClient.Setup(x => x.Post<BaseResponseDto>(It.IsAny<string>(), It.IsAny<PostConsumerDeviceRequestDto>())).ReturnsAsync(new BaseResponseDto());
            var httpContext = new DefaultHttpContext();
            httpContext.Items[HttpContextKeys.PersonUniqueIdentifier] = null;
            _httpContextAccessor.Setup(h => h.HttpContext).Returns(httpContext);

            var expectedResponse = new GetConsumerByPersonUniqueIdentifierResponseDto
            {
                Consumer = new[] { new ConsumerDto { MemberNbr = "999" } },
                Person = new PersonDto { Email = "abc@test.com", PersonUniqueIdentifier = "test123" }
            };

            _userClient.Setup(c => c.GetId<GetConsumerByPersonUniqueIdentifierResponseDto>(
                It.IsAny<string>(), It.IsAny<Dictionary<string, string>>()))
                .ReturnsAsync(expectedResponse);
            patchUserRequestDto.Email = null;
            var response = await _loginController.PatchUser(patchUserRequestDto);
            var result = response.Result as NotFoundObjectResult;
            Assert.True(result?.StatusCode == 404);
        }

        [Fact]
        public async Task PatchUser_Return_NotFound_When_Person_NotFound()
        {
            var patchUserRequestDto = new PatchUserRequestMockDto();

            _vault.Setup(v => v.GetSecret("env")).ReturnsAsync("Development");
            _configuration.Setup(c => c.GetSection("Auth0:Auth0TokenUrl").Value).Returns("https://dev-sunny-benefits.us.auth0.com/oauth/token");

            _configuration.Setup(v => v.GetSection("Auth0:client_secret").Value).Returns("AUTH0_CLIENT_SECRET");
            _vault.Setup(c => c.GetSecret("AUTH0_CLIENT_SECRET")).ReturnsAsync("sHQ0KyJNq7leq7jEpYSLMsubqZ3Zb3hQJyPwF0iyCQbEoKpsrWm5LPgU23d8UBPc");

            var audienceSection0 = new Mock<IConfigurationSection>();
            audienceSection0.Setup(x => x.Value).Returns("https://dev-sunny-benefits.us.auth0.com/api/v2/");

            var audienceSection1 = new Mock<IConfigurationSection>();
            audienceSection1.Setup(x => x.Value).Returns("https://api.custom-audience.com/");

            var audienceArraySection = new Mock<IConfigurationSection>();
            audienceArraySection.Setup(x => x.GetChildren())
                .Returns(new List<IConfigurationSection> { audienceSection0.Object, audienceSection1.Object });

            _configuration.Setup(x => x.GetSection("Auth0:Audiences"))
                .Returns(audienceArraySection.Object);

            _configuration.Setup(v => v.GetSection("Auth0:client_id").Value).Returns("AUTH0_CLIENT_ID");
            _vault.Setup(c => c.GetSecret("AUTH0_CLIENT_ID")).ReturnsAsync("TmzHdUNt0sjiJkYFs3CEks0utYeo8FF5");

            _configuration.Setup(c => c.GetSection("Auth0:Domain").Value).Returns("dev-sunny-benefits.us.auth0.com");
            _configuration.Setup(c => c.GetSection("Auth0:grant_type").Value).Returns("client_credentials");

            _configuration.Setup(c => c.GetSection("Auth0:Auth0ApiUrl").Value).Returns("https://dev-sunny-benefits.us.auth0.com/api/v2/users/");

            _userClient.Setup(x => x.Post<BaseResponseDto>(It.IsAny<string>(), It.IsAny<PostConsumerDeviceRequestDto>())).ReturnsAsync(new BaseResponseDto());
            var httpContext = new DefaultHttpContext();
            httpContext.Items[HttpContextKeys.PersonUniqueIdentifier] = "test1234";
            _httpContextAccessor.Setup(h => h.HttpContext).Returns(httpContext);

            var expectedResponse = new GetConsumerByPersonUniqueIdentifierResponseDto
            {
                Consumer = new[] { new ConsumerDto { MemberNbr = "999" } },
                Person = new PersonDto { Email = "abc@test.com", PersonUniqueIdentifier = null }
            };

            _userClient.Setup(c => c.GetId<GetConsumerByPersonUniqueIdentifierResponseDto>(
                It.IsAny<string>(), It.IsAny<Dictionary<string, string>>()))
                .ReturnsAsync(expectedResponse);
            patchUserRequestDto.Email = null;
            var response = await _loginController.PatchUser(patchUserRequestDto);
            var result = response.Result as NotFoundObjectResult;
            Assert.True(result?.StatusCode == 404);
        }

        [Fact]
        public async Task PatchUser_Return_Exception_Service()
        {
            var patchUserRequestDto = new PatchUserRequestMockDto();
            var auth0Helper = new Mock<IAuth0Helper>();

            _vault.Setup(v => v.GetSecret("env")).ReturnsAsync("Development");
            _configuration.Setup(c => c.GetSection("Auth0:Auth0TokenUrl").Value).Returns("https://dev-sunny-benefits.us.auth0.com/oauth/token");

            _configuration.Setup(v => v.GetSection("Auth0:client_secret").Value).Returns("AUTH0_CLIENT_SECRET");
            _vault.Setup(c => c.GetSecret("AUTH0_CLIENT_SECRET")).ReturnsAsync("sHQ0KyJNq7leq7jEpYSLMsubqZ3Zb3hQJyPwF0iyCQbEoKpsrWm5LPgU23d8UBPc");

            _configuration.Setup(c => c.GetSection("Auth0:audience").Value).Returns("https://dev-sunny-benefits.us.auth0.com/api/v2/");

            _configuration.Setup(v => v.GetSection("Auth0:client_id").Value).Returns("AUTH0_CLIENT_ID");
            _vault.Setup(c => c.GetSecret("AUTH0_CLIENT_ID")).ReturnsAsync("TmzHdUNt0sjiJkYFs3CEks0utYeo8FF5");

            _configuration.Setup(c => c.GetSection("Auth0:Domain").Value).Returns("dev-sunny-benefits.us.auth0.com");
            _configuration.Setup(c => c.GetSection("Auth0:grant_type").Value).Returns("client_credentials");

            _configuration.Setup(c => c.GetSection("Auth0:Auth0ApiUrl").Value).Returns("https://dev-sunny-benefits.us.auth0.com/api/v2/users/");

            _userClient.Setup(x => x.Post<BaseResponseDto>(It.IsAny<string>(), It.IsAny<PostConsumerDeviceRequestDto>())).ThrowsAsync(new Exception("testing"));

            var response = await _loginController.PatchUser(patchUserRequestDto);
            Assert.True(response != null);
        }

        [Fact]
        public async Task PatchUser_Return_Exception_When_Consumer_Device_Throws_Exception()
        {
            var patchUserRequestDto = new PatchUserRequestMockDto();
            var postConsumerDeviceRequestDto = new PostConsumerDeviceRequestDto();

            _vault.Setup(v => v.GetSecret("env")).ReturnsAsync("Development");
            _configuration.Setup(c => c.GetSection("Auth0:Auth0TokenUrl").Value).Returns("https://dev-sunny-benefits.us.auth0.com/oauth/token");

            _configuration.Setup(v => v.GetSection("Auth0:client_secret").Value).Returns("AUTH0_CLIENT_SECRET");
            _vault.Setup(c => c.GetSecret("AUTH0_CLIENT_SECRET")).ReturnsAsync("sHQ0KyJNq7leq7jEpYSLMsubqZ3Zb3hQJyPwF0iyCQbEoKpsrWm5LPgU23d8UBPc");

            _configuration.Setup(c => c.GetSection("Auth0:audience").Value).Returns("https://dev-sunny-benefits.us.auth0.com/api/v2/");

            _configuration.Setup(v => v.GetSection("Auth0:client_id").Value).Returns("AUTH0_CLIENT_ID");
            _vault.Setup(c => c.GetSecret("AUTH0_CLIENT_ID")).ReturnsAsync("TmzHdUNt0sjiJkYFs3CEks0utYeo8FF5");

            _configuration.Setup(c => c.GetSection("Auth0:Domain").Value).Returns("dev-sunny-benefits.us.auth0.com");
            _configuration.Setup(c => c.GetSection("Auth0:grant_type").Value).Returns("client_credentials");

            _userClient.Setup(x => x.Post<BaseResponseDto>(It.IsAny<string>(), postConsumerDeviceRequestDto)).ReturnsAsync(new BaseResponseDto());

            var result = await _controller.PatchUser(patchUserRequestDto);

            Assert.Null(result.Result);
        }

        [Fact]
        public async Task VerifyMember_ReturnsNotFound_WhenErrorCodeIs404()
        {
            // Arrange
            var verifyMemberDto = new VerifyMemberDto { Email = "test@example.com", verifyOps = "DOB" };
            var response = new VerifyMemberResponseDto { ErrorCode = 404 };
            _mockLoginService.Setup(service => service.VerifyMember(verifyMemberDto))
                             .ReturnsAsync(response);
            // Act
            var result = await _controller.VerifyMember(verifyMemberDto);

            // Assert
            var actionResult = Assert.IsType<ActionResult<VerifyMemberResponseDto>>(result);
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(actionResult.Result);
            Assert.Equal(response, notFoundResult.Value);
        }

        [Fact]
        public async Task VerifyMember_ReturnsUnprocessableEntity_WhenErrorCodeIs422()
        {
            // Arrange
            var verifyMemberDto = new VerifyMemberDto { Email = "test@example.com", verifyOps = "DOB" };
            var response = new VerifyMemberResponseDto { ErrorCode = 422 };
            _mockLoginService.Setup(service => service.VerifyMember(verifyMemberDto))
                             .ReturnsAsync(response);
            // Act
            var result = await _controller.VerifyMember(verifyMemberDto);

            // Assert
            var actionResult = Assert.IsType<ActionResult<VerifyMemberResponseDto>>(result);
            var unprocessableEntityResult = Assert.IsType<UnprocessableEntityObjectResult>(actionResult.Result);
            Assert.Equal(response, unprocessableEntityResult.Value);
        }

        [Fact]
        public async Task VerifyMember_ReturnsBadRequest_WhenErrorCodeIs400()
        {
            // Arrange
            var verifyMemberDto = new VerifyMemberDto { Email = "test@example.com", verifyOps = "DOB" };
            var response = new VerifyMemberResponseDto { ErrorCode = 400 };
            _mockLoginService.Setup(service => service.VerifyMember(verifyMemberDto))
                             .ReturnsAsync(response);
            // Act
            var result = await _controller.VerifyMember(verifyMemberDto);

            // Assert
            var actionResult = Assert.IsType<ActionResult<VerifyMemberResponseDto>>(result);
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(actionResult.Result);
            Assert.Equal(response, badRequestResult.Value);
        }

        [Fact]
        public async Task VerifyMember_ReturnsOk_WhenErrorCodeIs200()
        {
            // Arrange
            var verifyMemberDto = new VerifyMemberDto { Email = "test@example.com", verifyOps = "DOB" };
            var response = new VerifyMemberResponseDto { ErrorCode = 200 };
            _mockLoginService.Setup(service => service.VerifyMember(verifyMemberDto))
                             .ReturnsAsync(response);

            // Act
            var result = await _controller.VerifyMember(verifyMemberDto);

            // Assert
            var actionResult = Assert.IsType<ActionResult<VerifyMemberResponseDto>>(result);
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            Assert.Equal(response, okResult.Value);
        }

        [Fact]
        public async Task VerifyMember_ReturnsBadRequest_OnException()
        {
            // Arrange
            var verifyMemberDto = new VerifyMemberDto { Email = "test@example.com", verifyOps = "DOB" };
            var response = new VerifyMemberResponseDto() { ErrorCode = 400 };
            _mockLoginService.Setup(service => service.VerifyMember(verifyMemberDto))
                             .ReturnsAsync(response);

            // Act
            var result = await _controller.VerifyMember(verifyMemberDto);

            // Assert
            Assert.IsType<ActionResult<VerifyMemberResponseDto>>(result);
        }

        [Fact]
        public async Task VerifyMember_ReturnsBadRequest_WhenEmailNotFound()
        {
            // Arrange
            var verifyMemberDto = new VerifyMemberDto { Email = "test@example.com", verifyOps = "" };
            var consumerDetails = new GetConsumerByEmailResponseDto { ErrorCode = StatusCodes.Status404NotFound };
            _userClient.Setup(client => client.GetId<GetConsumerByEmailResponseDto>(
                It.IsAny<string>(),
                It.IsAny<Dictionary<string, string>>()))
                .ReturnsAsync(consumerDetails);

            var expectedResponse = new GetConsumerByPersonUniqueIdentifierResponseDto
            {
                ErrorCode = StatusCodes.Status404NotFound
            };

            _userClient.Setup(c => c.GetId<GetConsumerByPersonUniqueIdentifierResponseDto>(
                It.IsAny<string>(), It.IsAny<Dictionary<string, string>>()))
                .ReturnsAsync(expectedResponse);

            // Act
            var response = await _loginService.VerifyMember(verifyMemberDto);

            // Assert
            Assert.Equal(StatusCodes.Status404NotFound, response.ErrorCode);
        }

        [Fact]
        public async Task VerifyMember_ReturnsUnprocessableEntity_WhenConsumerLengthNotOne()
        {
            // Arrange
            var verifyMemberDto = new VerifyMemberDto { Email = "test@example.com", verifyOps = "DOB" };
            var consumerDetails = new GetConsumerByEmailResponseDto { Consumer = new ConsumerDto[0] };
            _userClient.Setup(client => client.GetId<GetConsumerByEmailResponseDto>(
                It.IsAny<string>(),
                It.IsAny<Dictionary<string, string>>()))
                .ReturnsAsync(consumerDetails);

            // Act
            var response = await _loginService.VerifyMember(verifyMemberDto);

            // Assert
            Assert.Equal(StatusCodes.Status422UnprocessableEntity, response.ErrorCode);
        }

        [Fact]
        public async Task VerifyMember_Returns400_WhenInvalidverifyOps()
        {
            // Arrange
            var verifyMemberDto = new VerifyMemberDto { Email = "test@example.com", verifyOps = "XXX" };
            var consumerDetails = new GetConsumerByEmailResponseDto { Consumer = new ConsumerDto[1], Person = new PersonDto { DOB = new DateTime(1999, 1, 1) } };
            _userClient.Setup(client => client.GetId<GetConsumerByEmailResponseDto>(
                It.IsAny<string>(),
                It.IsAny<Dictionary<string, string>>()))
                .ReturnsAsync(consumerDetails);

            var expectedResponse = new GetConsumerByPersonUniqueIdentifierResponseDto
            {
                Consumer = new[] { new ConsumerDto { MemberNbr = "999" } },
                Person = new PersonDto { Email = "abc@test.com" }
            };

            _userClient.Setup(c => c.GetId<GetConsumerByPersonUniqueIdentifierResponseDto>(
                It.IsAny<string>(), It.IsAny<Dictionary<string, string>>()))
                .ReturnsAsync(expectedResponse);

            // Act
            var response = await _loginService.VerifyMember(verifyMemberDto);

            // Assert
            Assert.Equal(StatusCodes.Status400BadRequest, response.ErrorCode);
        }

        [Fact]
        public async Task VerifyMember_ReturnsUnprocessableEntity_WhenDOBDoesNotMatch()
        {
            // Arrange
            var verifyMemberDto = new VerifyMemberDto { Email = "test@example.com", verifyOps = "DOB", DOB = new DateTime(2000, 1, 1) };
            var consumerDetails = new GetConsumerByEmailResponseDto { Consumer = new ConsumerDto[1], Person = new PersonDto { DOB = new DateTime(1999, 1, 1) } };
            consumerDetails.Consumer[0] = new ConsumerDto()
            {
                ConsumerId = 1,
                PersonId = 1,
                TenantCode = "ten-ecada21e57154928a2bb959e8365b8b4",
                ConsumerCode = "cus - 04c211b4339348509eaa870cdea59600",
            };
            _userClient.Setup(client => client.GetId<GetConsumerByEmailResponseDto>(
                It.IsAny<string>(),
                It.IsAny<Dictionary<string, string>>()))
                .ReturnsAsync(consumerDetails);

            // Act
            var response = await _loginService.VerifyMember(verifyMemberDto);

            // Assert
            Assert.Equal(StatusCodes.Status422UnprocessableEntity, response.ErrorCode);
        }

        [Fact]
        public async Task VerifyMember_ReturnsOk_WhenDOBMatches()
        {
            // Arrange
            var verifyMemberDto = new VerifyMemberDto { Email = "test@example.com", verifyOps = "DOB", DOB = new DateTime(2000, 1, 1) };
            var consumerDetails = new GetConsumerByEmailResponseDto { Consumer = new ConsumerDto[1], Person = new PersonDto { DOB = new DateTime(2000, 1, 1) } };
            consumerDetails.Consumer[0] = new ConsumerDto()
            {
                ConsumerId = 1,
                PersonId = 1,
                TenantCode = "ten-ecada21e57154928a2bb959e8365b8b4",
                ConsumerCode = "cus - 04c211b4339348509eaa870cdea59600",
                OnBoardingState = OnboardingState.DOB_VERIFIED.ToString()
            };
            var personDto = new PersonDto() { PersonId = 1 };
            var expectedResponse = new GetConsumerByPersonUniqueIdentifierResponseDto
            {
                Consumer = consumerDetails.Consumer,
                Person = consumerDetails.Person
            };

            _userClient.Setup(c => c.GetId<GetConsumerByPersonUniqueIdentifierResponseDto>(
                It.IsAny<string>(), It.IsAny<Dictionary<string, string>>()))
                .ReturnsAsync(expectedResponse);
            _userClient.Setup(client => client.GetId<GetConsumerByEmailResponseDto>(
                It.IsAny<string>(),
                It.IsAny<Dictionary<string, string>>()))
                .ReturnsAsync(consumerDetails);
            var componentDto = new ComponentDto
            {
                DataJson = JsonConvert.SerializeObject(new AgreementDataComponentDto
                {
                    Data = new AgreementDataDto
                    {
                        Agreements = new List<Agreement> { new Agreement { DisplayName = "html", Url = "https://example.com/agreement.html" } }

                    }
                })

            };
            _UploadPdfService.Setup(service => service.GetComponent(It.IsAny<GetComponentRequestDto>()))
                             .ReturnsAsync(new GetComponentResponseDto { Component=componentDto});
            _personHelper.Setup(x => x.UpdateOnBoardingState(It.IsAny<UpdateOnboardingStateDto>())).ReturnsAsync(true);

            // Act
            var response = await _loginService.VerifyMember(verifyMemberDto);

            // Assert
            Assert.Equal(StatusCodes.Status200OK, response.ErrorCode);
        } 
        
        [Fact]
        public async Task VerifyMember_ReturnsOk_WhenVerified()
        {
            // Arrange
            var verifyMemberDto = new VerifyMemberDto { Email = "test@example.com", verifyOps = "VERIFIED", DOB = new DateTime(2000, 1, 1),ComponentCode="test" };
            var consumerDetails = new GetConsumerByEmailResponseDto { Consumer = new ConsumerDto[1], Person = new PersonDto { DOB = new DateTime(2000, 1, 1) } };
            consumerDetails.Consumer[0] = new ConsumerDto()
            {
                ConsumerId = 1,
                PersonId = 1,
                TenantCode = "ten-ecada21e57154928a2bb959e8365b8b4",
                ConsumerCode = "cus - 04c211b4339348509eaa870cdea59600",
                OnBoardingState = OnboardingState.DOB_VERIFIED.ToString()
            };
            var personDto = new PersonDto() { PersonId = 1 };
            var expectedResponse = new GetConsumerByPersonUniqueIdentifierResponseDto
            {
                Consumer = consumerDetails.Consumer,
                Person = consumerDetails.Person
            };

            _userClient.Setup(c => c.GetId<GetConsumerByPersonUniqueIdentifierResponseDto>(
                It.IsAny<string>(), It.IsAny<Dictionary<string, string>>()))
                .ReturnsAsync(expectedResponse);
            _userClient.Setup(client => client.GetId<GetConsumerByEmailResponseDto>(
                It.IsAny<string>(),
                It.IsAny<Dictionary<string, string>>()))
                .ReturnsAsync(consumerDetails);

            _personHelper.Setup(x => x.UpdateOnBoardingState(It.IsAny<UpdateOnboardingStateDto>())).ReturnsAsync(true);
            var componentDto = new ComponentDto
            {
                DataJson = JsonConvert.SerializeObject(new AgreementDataComponentDto
                {
                    Data = new AgreementDataDto
                    {
                        Agreements = new List<Agreement> { new Agreement { DisplayName = "html", Url = "https://example.com/agreement.html" } }

                    }
                })

            };
            _UploadPdfService.Setup(service => service.GetComponent(It.IsAny<GetComponentRequestDto>()))
                             .ReturnsAsync(new GetComponentResponseDto { Component = componentDto });

            // Act
            var response = await _loginService.VerifyMember(verifyMemberDto);

            // Assert
            Assert.Equal(StatusCodes.Status200OK, response.ErrorCode);
        }

        [Fact]
        public async Task VerifyMember_ReturnsOk_WhenDOBNull()
        {
            // Arrange
            var verifyMemberDto = new VerifyMemberDto { Email = "test@example.com", verifyOps = "DOB" };
            var consumerDetails = new GetConsumerByEmailResponseDto { Consumer = new ConsumerDto[1], Person = new PersonDto { DOB = new DateTime(2000, 1, 1) } };
            var personDto = new PersonDto() { PersonId = 1, OnBoardingState = OnboardingState.DOB_VERIFIED.ToString() };
            var expectedResponse = new GetConsumerByPersonUniqueIdentifierResponseDto
            {
                Consumer = consumerDetails.Consumer,
                Person = personDto
            };

            _userClient.Setup(c => c.GetId<GetConsumerByPersonUniqueIdentifierResponseDto>(
                It.IsAny<string>(), It.IsAny<Dictionary<string, string>>()))
                .ReturnsAsync(expectedResponse);
            _userClient.Setup(client => client.GetId<GetConsumerByEmailResponseDto>(
                It.IsAny<string>(),
                It.IsAny<Dictionary<string, string>>()))
                .ReturnsAsync(consumerDetails);

            _personHelper.Setup(x => x.UpdateOnBoardingState(It.IsAny<UpdateOnboardingStateDto>())).ReturnsAsync(true);

            // Act
            var response = await _loginService.VerifyMember(verifyMemberDto);

            // Assert
            Assert.Equal(StatusCodes.Status400BadRequest, response.ErrorCode);
        }

        [Fact]
        public async Task VerifyMember_ReturnsBadRequest_WhenCardLast4IsNull()
        {
            // Arrange
            var verifyMemberDto = new VerifyMemberDto { Email = "test@example.com", verifyOps = "CARDLAST4" };
            var consumerDetails = new GetConsumerByEmailResponseDto { Consumer = new ConsumerDto[1] };
            consumerDetails.Consumer[0] = new ConsumerDto()
            {
                ConsumerId = 1,
                PersonId = 1,
                TenantCode = "ten-ecada21e57154928a2bb959e8365b8b4",
                ConsumerCode = "cus - 04c211b4339348509eaa870cdea59600",
            };
            _userClient.Setup(client => client.GetId<GetConsumerByEmailResponseDto>(
                It.IsAny<string>(),
                It.IsAny<Dictionary<string, string>>()))
                .ReturnsAsync(consumerDetails);

            var expectedResponse = new GetConsumerByPersonUniqueIdentifierResponseDto
            {
                Consumer = new[] { new ConsumerDto()
                {
                    ConsumerId = 1,
                    PersonId = 1,
                    TenantCode = "ten-ecada21e57154928a2bb959e8365b8b4",
                    ConsumerCode = "cus - 04c211b4339348509eaa870cdea59600",
                } },
                Person = new PersonDto { Email = "abc@test.com" }
            };

            _userClient.Setup(c => c.GetId<GetConsumerByPersonUniqueIdentifierResponseDto>(
                It.IsAny<string>(), It.IsAny<Dictionary<string, string>>()))
                .ReturnsAsync(expectedResponse);

            // Act
            var response = await _loginService.VerifyMember(verifyMemberDto);

            // Assert
            Assert.Equal(StatusCodes.Status400BadRequest, response.ErrorCode);
        }

        [Fact]
        public async Task VerifyMember_ReturnsOk_UpdateOnBoarding_COSTCO_ACTIONS_VISITED()
        {
            // Arrange
            var verifyMemberDto = new VerifyMemberDto { Email = "test@example.com", verifyOps = "COSTCO_ACTIONS_VISITED" };
            var personDto = new PersonDto() { PersonId = 1, OnBoardingState = OnboardingState.CARD_LAST_4_VERIFIED.ToString() };
            var consumerDetails = new GetConsumerByEmailResponseDto
            {
                Consumer = new ConsumerDto[] { new ConsumerDto { TenantCode = "tenant", ConsumerCode = "consumer" } }
            };
             var expectedResponse = new GetConsumerByPersonUniqueIdentifierResponseDto
            {
                Consumer = consumerDetails.Consumer ,
                Person = personDto
            };

            _userClient.Setup(c => c.GetId<GetConsumerByPersonUniqueIdentifierResponseDto>(
                It.IsAny<string>(), It.IsAny<Dictionary<string, string>>()))
                .ReturnsAsync(expectedResponse);

            _personHelper.Setup(x => x.UpdateOnBoardingState(It.IsAny<UpdateOnboardingStateDto>())).ReturnsAsync(true);

            // Act
            var response = await _loginService.VerifyMember(verifyMemberDto);

            // Assert
            Assert.Equal(StatusCodes.Status200OK, response.ErrorCode);
        }


        [Fact]
        public async Task VerifyMember_ReturnsOk_UpdateOnBoarding_VERIFIED()
        {
            // Arrange
            var verifyMemberDto = new VerifyMemberDto { Email = "test@example.com", verifyOps = "VERIFIED" };
            var personDto = new PersonDto() { PersonId = 1, OnBoardingState = OnboardingState.CARD_LAST_4_VERIFIED.ToString() };
            var consumerDetails = new GetConsumerByEmailResponseDto
            {
                Consumer = new ConsumerDto[] { new ConsumerDto { TenantCode = "tenant", ConsumerCode = "consumer" } }
            };
            var expectedResponse = new GetConsumerByPersonUniqueIdentifierResponseDto
            {
                Consumer = consumerDetails.Consumer,
                Person = personDto
            };

            _userClient.Setup(c => c.GetId<GetConsumerByPersonUniqueIdentifierResponseDto>(
                It.IsAny<string>(), It.IsAny<Dictionary<string, string>>()))
                .ReturnsAsync(expectedResponse);
            _personHelper.Setup(x => x.UpdateOnBoardingState(It.IsAny<UpdateOnboardingStateDto>())).ReturnsAsync(true);

            var walletTypesResponse = new ConsumerBenefitsWalletTypesResponseDto
            {
                BenefitsWalletTypes = new List<ConsumerBenefitWalletTypeDto>
            {
                new ConsumerBenefitWalletTypeDto { PurseLabel = "Label1" },
                    new ConsumerBenefitWalletTypeDto { PurseLabel = "Label2" }
            }
            };
            _fisClient.Setup(client => client.Post<ConsumerBenefitsWalletTypesResponseDto>(It.IsAny<string>(), It.IsAny<object>()))
                          .ReturnsAsync(walletTypesResponse);
            _adminClient.Setup(client => client.Post<PostEventResponseDto>(It.IsAny<string>(), It.IsAny<object>()))
                          .ReturnsAsync(new PostEventResponseDto());
            _UploadPdfService.Setup(client => client.GetComponentBycode(It.IsAny<GetComponentByCodeRequestDto>()))
                          .ReturnsAsync(new GetComponentByCodeResponseDto { Component = new ComponentDto { ComponentCode = "123", DataJson= "{\r\n" +
"  \"data\": {\r\n" +
"    \"graphicUrl\": {\r\n" +
"      \"mobileUrl\": \"https://app-static.dev.sunnyrewards.com/public/images/kp_first_interaction_front_mobile_image.png\",\r\n" +
"      \"desktopUrl\": \"https://app-static.dev.sunnyrewards.com/public/images/kp_first_interaction_front_desktop_image.png\"\r\n" +
"    },\r\n" +
"    \"headerText\": \"Welcome to Kaiser Permanente rewards!\",\r\n" +
"    \"description\": \"Accept terms to get started.\",\r\n" +
"    \"ctaButtonText\": \"Continue\",\r\n" +
"    \"eSignAgreementUrl\": \"https://app-static.dev.sunnyrewards.com/cms/html/ten-353ae621abde4e22be409325a1dd0eab/en-US/e_sign_agreement.html\",\r\n" +
"    \"wellnessAgreementUrl\": \"https://app-static.dev.sunnyrewards.com/cms/html/ten-353ae621abde4e22be409325a1dd0eab/en-US/wellness_agreement.html\",\r\n" +
"    \"cardHolderAgreementUrl\": \"https://app-static.dev.sunnyrewards.com/cms/html/ten-353ae621abde4e22be409325a1dd0eab/en-US/cardholder_agreement.html\",\r\n" +
"    \"promotionalAgreementUrl\": \"https://app-static.dev.sunnyrewards.com/cms/html/ten-353ae621abde4e22be409325a1dd0eab/en-US/promotional_agreement.html\"\r\n" +
"  }\r\n" +
"}"
                          }
                          });

            // Act
            var response = await _loginService.VerifyMember(verifyMemberDto);

            // Assert
            Assert.Equal(StatusCodes.Status200OK, response.ErrorCode);
        }
        [Fact]
        public async Task VerifyMember_ReturnsOk_WhenCardLast4Matches()
        {
            // Arrange
            var verifyMemberDto = new VerifyMemberDto { Email = "test@example.com", verifyOps = "CARDLAST4", CardLast4 = "1234" };
            var personDto = new PersonDto() { PersonId = 1, OnBoardingState = OnboardingState.CARD_LAST_4_VERIFIED.ToString() };
            var consumerDetails = new GetConsumerByEmailResponseDto
            {
                Consumer = new ConsumerDto[] { new ConsumerDto { TenantCode = "tenant", ConsumerCode = "consumer" } }
            };
            var expectedResponse = new GetConsumerByPersonUniqueIdentifierResponseDto
            {
                Consumer = consumerDetails.Consumer,
                Person = personDto
            };

            _userClient.Setup(c => c.GetId<GetConsumerByPersonUniqueIdentifierResponseDto>(
                It.IsAny<string>(), It.IsAny<Dictionary<string, string>>()))
                .ReturnsAsync(expectedResponse);
            var fisResponse = new VerifyMemberResponseDto { ErrorCode = StatusCodes.Status200OK };
            _userClient.Setup(client => client.GetId<GetConsumerByEmailResponseDto>(
                It.IsAny<string>(),
                It.IsAny<Dictionary<string, string>>()))
                .ReturnsAsync(consumerDetails);
            _fisClient.Setup(client => client.Post<VerifyMemberResponseDto>(
                It.IsAny<string>(),
                It.IsAny<VerifyFisMemberDto>()))
                .ReturnsAsync(fisResponse);

            _personHelper.Setup(x => x.UpdateOnBoardingState(It.IsAny<UpdateOnboardingStateDto>())).ReturnsAsync(true);

            // Act
            var response = await _loginService.VerifyMember(verifyMemberDto);

            // Assert
            Assert.Equal(StatusCodes.Status200OK, response.ErrorCode);
        }

        [Fact]
        public async Task VerifyMember_ReturnsOk_WhenCardLast4MatchesAndConsumerAlreadyVerified()
        {
            // Arrange
            var verifyMemberDto = new VerifyMemberDto { Email = "test@example.com", verifyOps = "CARDLAST4", CardLast4 = "1234" };
            var personDto = new PersonDto() { PersonId = 1, OnBoardingState = OnboardingState.CARD_LAST_4_VERIFIED.ToString() };
            var consumerDetails = new GetConsumerByEmailResponseDto
            {
                Consumer = new ConsumerDto[] { new ConsumerDto { TenantCode = "tenant", ConsumerCode = "consumer" , OnBoardingState = OnboardingState.VERIFIED.ToString() } }
            };
            var expectedResponse = new GetConsumerByPersonUniqueIdentifierResponseDto
            {
                Consumer = consumerDetails.Consumer,
                Person = personDto
            };

            _userClient.Setup(c => c.GetId<GetConsumerByPersonUniqueIdentifierResponseDto>(
                It.IsAny<string>(), It.IsAny<Dictionary<string, string>>()))
                .ReturnsAsync(expectedResponse);
            var fisResponse = new VerifyMemberResponseDto { ErrorCode = StatusCodes.Status200OK };
            _userClient.Setup(client => client.GetId<GetConsumerByEmailResponseDto>(
                It.IsAny<string>(),
                It.IsAny<Dictionary<string, string>>()))
                .ReturnsAsync(consumerDetails);
            _fisClient.Setup(client => client.Post<VerifyMemberResponseDto>(
                It.IsAny<string>(),
                It.IsAny<VerifyFisMemberDto>()))
                .ReturnsAsync(fisResponse);
            // Act
            var response = await _loginService.VerifyMember(verifyMemberDto);

            // Assert
            Assert.Equal(StatusCodes.Status200OK, response.ErrorCode);
            Assert.Equal("Card Last 4 verified", response.ErrorMessage);
        }

        [Fact]
        public async Task VerifyMember_ReturnsOk_UpdateOnBoarding_PICK_A_PURSE_COMPLETED()
        {
            // Arrange
            var verifyMemberDto = new VerifyMemberDto { Email = "test@example.com", verifyOps = "PICK_A_PURSE_COMPLETED" };
            var personDto = new PersonDto() { PersonId = 1, OnBoardingState = OnboardingState.CARD_LAST_4_VERIFIED.ToString() };
            var consumerDetails = new GetConsumerByEmailResponseDto
            {
                Consumer = new ConsumerDto[] { new ConsumerDto { TenantCode = "tenant", ConsumerCode = "consumer" } }
            };
            var expectedResponse = new GetConsumerByPersonUniqueIdentifierResponseDto
            {
                Consumer = consumerDetails.Consumer ,
                Person = personDto
            };

            _userClient.Setup(c => c.GetId<GetConsumerByPersonUniqueIdentifierResponseDto>(
                It.IsAny<string>(), It.IsAny<Dictionary<string, string>>()))
                .ReturnsAsync(expectedResponse);

            _personHelper.Setup(x => x.UpdateOnBoardingState(It.IsAny<UpdateOnboardingStateDto>())).ReturnsAsync(true);

            // Act
            var response = await _loginService.VerifyMember(verifyMemberDto);

            // Assert
            Assert.Equal(StatusCodes.Status200OK, response.ErrorCode);
        }

        [Fact]
        public async Task VerifyMember_ReturnsOk_When_VerifyOps_is_declined()
        {
            // Arrange
            var verifyMemberDto = new VerifyMemberDto { Email = "test@example.com", verifyOps = "DECLINED" };
            var personDto = new PersonDto() { PersonId = 1, OnBoardingState = OnboardingState.CARD_LAST_4_VERIFIED.ToString() };
            var consumerDetails = new GetConsumerByEmailResponseDto
            {
                Consumer = new ConsumerDto[] { new ConsumerDto { TenantCode = "tenant", ConsumerCode = "consumer" } }
            };
            var expectedResponse = new GetConsumerByPersonUniqueIdentifierResponseDto
            {
                Consumer = consumerDetails.Consumer,
                Person = personDto
            };

            _userClient.Setup(c => c.GetId<GetConsumerByPersonUniqueIdentifierResponseDto>(
                It.IsAny<string>(), It.IsAny<Dictionary<string, string>>()))
                .ReturnsAsync(expectedResponse);

            _personHelper.Setup(x => x.UpdateOnBoardingState(It.IsAny<UpdateOnboardingStateDto>())).ReturnsAsync(true);

            // Act
            var response = await _loginService.VerifyMember(verifyMemberDto);

            // Assert
            Assert.Equal(StatusCodes.Status200OK, response.ErrorCode);
        }

        [Fact]
        public async Task VerifyMember_ReturnsError_When_user_api_retun_error()
        {
            // Arrange
            var verifyMemberDto = new VerifyMemberDto { Email = "test@example.com", verifyOps = "DECLINED" };
            var personDto = new PersonDto() { PersonId = 1, OnBoardingState = OnboardingState.CARD_LAST_4_VERIFIED.ToString() };
            var consumerDetails = new GetConsumerByEmailResponseDto
            {
                Consumer = new ConsumerDto[] { new ConsumerDto { TenantCode = "tenant", ConsumerCode = "consumer" } }
            };
            var expectedResponse = new GetConsumerByPersonUniqueIdentifierResponseDto
            {
                Consumer = consumerDetails.Consumer,
                Person = personDto
            };
            _userClient.Setup(c => c.GetId<GetConsumerByPersonUniqueIdentifierResponseDto>(
               It.IsAny<string>(), It.IsAny<Dictionary<string, string>>()))
               .ReturnsAsync(expectedResponse);

            _userClient.Setup(c => c.Put<ConsumerResponseDto>(
                It.IsAny<string>(), It.IsAny<UpdateAgreementStatusDto>()))
                .ReturnsAsync(new ConsumerResponseDto()
                {
                    ErrorCode = StatusCodes.Status500InternalServerError
                });

            _personHelper.Setup(x => x.UpdateOnBoardingState(It.IsAny<UpdateOnboardingStateDto>())).ReturnsAsync(true);

            // Act
            var response = await _loginService.VerifyMember(verifyMemberDto);

            // Assert
            Assert.Equal(StatusCodes.Status500InternalServerError, response.ErrorCode);
        }

        [Fact]
        public async Task PostVerificationEmail_Return_Ok_Response_Controller()
        {
            var emailRequestDto = new VerificationEmailRequestMockDto();
            var auth0Helper = new Mock<IAuth0Helper>();
            var logger = new Mock<ILogger<LoginController>>();
            var mockResponse = new UpdateResponseMockDto();
            auth0Helper.Setup(x => x.PostVerificationEmail(It.IsAny<VerificationEmailRequestDto>())).ReturnsAsync(mockResponse);
            var controller = new LoginController(logger.Object, _loginService, auth0Helper.Object);
            var response = await controller.PostVerificationEmail(emailRequestDto);
            var result = response.Result as OkObjectResult;
            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
            Assert.Equal(mockResponse, result.Value);
        }

        [Fact]
        public async Task PostVerificationEmail_Catch_Exception_Controller()
        {
            var emailRequestDto = new VerificationEmailRequestMockDto();
            var auth0Helper = new Mock<IAuth0Helper>();
            var logger = new Mock<ILogger<LoginController>>();
            auth0Helper.Setup(x => x.PostVerificationEmail(It.IsAny<VerificationEmailRequestDto>()))
            .ThrowsAsync(new Exception("Simulated exception"));
            var controller = new LoginController(logger.Object, _loginService, auth0Helper.Object);
            var response = await controller.PostVerificationEmail(emailRequestDto);
            Assert.NotNull(response?.Value);
        }

        [Fact]
        public async Task PostVerificationEmail_Return_Ok_Response_Service()
        {
            var emailRequestDto = new VerificationEmailRequestMockDto();
            var auth0Helper = new Mock<IAuth0Helper>();

            _vault.Setup(v => v.GetSecret("env")).ReturnsAsync("Development");
            _configuration.Setup(c => c.GetSection("Auth0:Auth0TokenUrl").Value).Returns("https://dev-sunny-benefits.us.auth0.com/oauth/token");

            _configuration.Setup(v => v.GetSection("Auth0:client_secret").Value).Returns("AUTH0_CLIENT_SECRET");
            _vault.Setup(c => c.GetSecret("AUTH0_CLIENT_SECRET")).ReturnsAsync("sHQ0KyJNq7leq7jEpYSLMsubqZ3Zb3hQJyPwF0iyCQbEoKpsrWm5LPgU23d8UBPc");

            var audienceSection0 = new Mock<IConfigurationSection>();
            audienceSection0.Setup(x => x.Value).Returns("https://dev-sunny-benefits.us.auth0.com/api/v2/");

            var audienceSection1 = new Mock<IConfigurationSection>();
            audienceSection1.Setup(x => x.Value).Returns("https://api.custom-audience.com/");

            var audienceArraySection = new Mock<IConfigurationSection>();
            audienceArraySection.Setup(x => x.GetChildren())
                .Returns(new List<IConfigurationSection> { audienceSection0.Object, audienceSection1.Object });

            _configuration.Setup(x => x.GetSection("Auth0:Audiences"))
                .Returns(audienceArraySection.Object);

            _configuration.Setup(v => v.GetSection("Auth0:client_id").Value).Returns("AUTH0_CLIENT_ID");
            _vault.Setup(c => c.GetSecret("AUTH0_CLIENT_ID")).ReturnsAsync("TmzHdUNt0sjiJkYFs3CEks0utYeo8FF5");

            _configuration.Setup(c => c.GetSection("Auth0:Domain").Value).Returns("dev-sunny-benefits.us.auth0.com");
            _configuration.Setup(c => c.GetSection("Auth0:grant_type").Value).Returns("client_credentials");

            _configuration.Setup(c => c.GetSection("Auth0:Auth0ApiUrl").Value).Returns("https://dev-sunny-benefits.us.auth0.com/api/v2/users/");
            _configuration.Setup(c => c.GetSection("Auth0:Auth0VerificationEmailUrl").Value).Returns("https://dev-sunny-benefits.us.auth0.com/api/v2/jobs/verification-email");
            var httpContext = new DefaultHttpContext();
            httpContext.Items[HttpContextKeys.PersonUniqueIdentifier] = "abc_123";
            _httpContextAccessor.Setup(h => h.HttpContext).Returns(httpContext);

            var expectedResponse = new GetConsumerByPersonUniqueIdentifierResponseDto
            {
                Consumer = new[] { new ConsumerDto { MemberNbr = "999" } },
                Person = new PersonDto { Email = "abc@test.com", PersonUniqueIdentifier = "test123" }
            };

            _userClient.Setup(c => c.GetId<GetConsumerByPersonUniqueIdentifierResponseDto>(
                It.IsAny<string>(), It.IsAny<Dictionary<string, string>>()))
                .ReturnsAsync(expectedResponse);
            var response = await _loginController.PostVerificationEmail(emailRequestDto);
            var result = response.Result as OkObjectResult;
            Assert.True(result?.Value != null);
            Assert.True(result.StatusCode == 200);
        }

        [Fact]
        public async Task PostVerificationEmail_Return_Exception_Service()
        {
            var emailRequestDto = new VerificationEmailRequestMockDto();
            var auth0Helper = new Mock<IAuth0Helper>();

            _vault.Setup(v => v.GetSecret("env")).ReturnsAsync("Development");
            _configuration.Setup(c => c.GetSection("Auth0:Auth0TokenUrl").Value).Returns("https://dev-sunny-benefits.us.auth0.com/oauth/token");

            _configuration.Setup(v => v.GetSection("Auth0:client_secret").Value).Returns("AUTH0_CLIENT_SECRET");
            _vault.Setup(c => c.GetSecret("AUTH0_CLIENT_SECRET")).ReturnsAsync("sHQ0KyJNq7leq7jEpYSLMsubqZ3Zb3hQJyPwF0iyCQbEoKpsrWm5LPgU23d8UBPc");

            _configuration.Setup(c => c.GetSection("Auth0:audience").Value).Returns("https://dev-sunny-benefits.us.auth0.com/api/v2/");

            _configuration.Setup(v => v.GetSection("Auth0:client_id").Value).Returns("AUTH0_CLIENT_ID");
            _vault.Setup(c => c.GetSecret("AUTH0_CLIENT_ID")).ReturnsAsync("TmzHdUNt0sjiJkYFs3CEks0utYeo8FF5");

            _configuration.Setup(c => c.GetSection("Auth0:Domain").Value).Returns("dev-sunny-benefits.us.auth0.com");
            _configuration.Setup(c => c.GetSection("Auth0:grant_type").Value).Returns("client_credentials");

            _configuration.Setup(c => c.GetSection("Auth0:Auth0ApiUrl").Value).Returns("https://dev-sunny-benefits.us.auth0.com/api/v2/users/");
            _configuration.Setup(c => c.GetSection("Auth0:Auth0VerificationEmailUrl").Value).Throws(new Exception("Simulated exception")); ;
            var httpContext = new DefaultHttpContext();
            httpContext.Items[HttpContextKeys.PersonUniqueIdentifier] = "abc_123";
            _httpContextAccessor.Setup(h => h.HttpContext).Returns(httpContext);

            var expectedResponse = new GetConsumerByPersonUniqueIdentifierResponseDto
            {
                Consumer = new[] { new ConsumerDto { MemberNbr = "999" } },
                Person = new PersonDto { Email = "abc@test.com", PersonUniqueIdentifier = "test123" }
            };

            _userClient.Setup(c => c.GetId<GetConsumerByPersonUniqueIdentifierResponseDto>(
                It.IsAny<string>(), It.IsAny<Dictionary<string, string>>()))
                .Throws(new Exception("Simulated exception"));
            var result = await Assert.ThrowsAsync<Exception>(async () => await _auth0Helper.PostVerificationEmail(emailRequestDto));
            Assert.True(result != null);
        }

        [Fact]
        public async Task GetUserById_Return_Ok_Response_Controller()
        {
            var userRequestDto = new GetUserRequestMockDto();
            var auth0Helper = new Mock<IAuth0Helper>();
            var logger = new Mock<ILogger<LoginController>>();
            var mockResponse = new UserGetResponseDto();
            auth0Helper.Setup(x => x.GetUserById(It.IsAny<GetUserRequestDto>())).ReturnsAsync(mockResponse);
            var controller = new LoginController(logger.Object, _loginService, auth0Helper.Object);
            var response = await controller.GetUserById(userRequestDto);
            var result = response.Result as OkObjectResult;
            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
            Assert.Equal(mockResponse, result.Value);
        }

        [Fact]
        public async Task GetUserById_Catch_Exception_Controller()
        {
            var userRequestDto = new GetUserRequestMockDto();
            var auth0Helper = new Mock<IAuth0Helper>();
            var logger = new Mock<ILogger<LoginController>>();
            auth0Helper.Setup(x => x.GetUserById(It.IsAny<GetUserRequestDto>()))
            .ThrowsAsync(new Exception("Simulated exception"));
            var controller = new LoginController(logger.Object, _loginService, auth0Helper.Object);
            var response = await controller.GetUserById(userRequestDto);
            Assert.NotNull(response?.Value);
        }

        [Fact]
        public async Task GetUserById_Return_Ok_Response_Service()
        {
            var userRequestDto = new GetUserRequestMockDto();
            var auth0Helper = new Mock<IAuth0Helper>();

            _vault.Setup(v => v.GetSecret("env")).ReturnsAsync("Development");
            _configuration.Setup(c => c.GetSection("Auth0:Auth0TokenUrl").Value).Returns("https://dev-sunny-benefits.us.auth0.com/oauth/token");

            _configuration.Setup(v => v.GetSection("Auth0:client_secret").Value).Returns("AUTH0_CLIENT_SECRET");
            _vault.Setup(c => c.GetSecret("AUTH0_CLIENT_SECRET")).ReturnsAsync("sHQ0KyJNq7leq7jEpYSLMsubqZ3Zb3hQJyPwF0iyCQbEoKpsrWm5LPgU23d8UBPc");

            var audienceSection0 = new Mock<IConfigurationSection>();
            audienceSection0.Setup(x => x.Value).Returns("https://dev-sunny-benefits.us.auth0.com/api/v2/");

            var audienceSection1 = new Mock<IConfigurationSection>();
            audienceSection1.Setup(x => x.Value).Returns("https://api.custom-audience.com/");

            var audienceArraySection = new Mock<IConfigurationSection>();
            audienceArraySection.Setup(x => x.GetChildren())
                .Returns(new List<IConfigurationSection> { audienceSection0.Object, audienceSection1.Object });

            _configuration.Setup(x => x.GetSection("Auth0:Audiences"))
                .Returns(audienceArraySection.Object);

            _configuration.Setup(v => v.GetSection("Auth0:client_id").Value).Returns("AUTH0_CLIENT_ID");
            _vault.Setup(c => c.GetSecret("AUTH0_CLIENT_ID")).ReturnsAsync("TmzHdUNt0sjiJkYFs3CEks0utYeo8FF5");

            _configuration.Setup(c => c.GetSection("Auth0:Domain").Value).Returns("dev-sunny-benefits.us.auth0.com");
            _configuration.Setup(c => c.GetSection("Auth0:grant_type").Value).Returns("client_credentials");

            _configuration.Setup(c => c.GetSection("Auth0:Auth0ApiUrl").Value).Returns("https://dev-sunny-benefits.us.auth0.com/api/v2/users/");
            var httpContext = new DefaultHttpContext();
            httpContext.Items[HttpContextKeys.PersonUniqueIdentifier] = "abc_123";
            _httpContextAccessor.Setup(h => h.HttpContext).Returns(httpContext);

            var expectedResponse = new GetConsumerByPersonUniqueIdentifierResponseDto
            {
                Consumer = new[] { new ConsumerDto { MemberNbr = "999" } },
                Person = new PersonDto { Email = "abc@test.com", PersonUniqueIdentifier = "test123" }
            };

            _userClient.Setup(c => c.GetId<GetConsumerByPersonUniqueIdentifierResponseDto>(
                It.IsAny<string>(), It.IsAny<Dictionary<string, string>>()))
                .ReturnsAsync(expectedResponse);
            var response = await _loginController.GetUserById(userRequestDto);
            var result = response.Result as OkObjectResult;
            Assert.True(result?.Value != null);
            Assert.True(result.StatusCode == 200);
        }

        [Fact]
        public async Task GetUserById_Return_Person_Not_Found()
        {
            var userRequestDto = new GetUserRequestMockDto();
            var auth0Helper = new Mock<IAuth0Helper>();

            _vault.Setup(v => v.GetSecret("env")).ReturnsAsync("Development");
            _configuration.Setup(c => c.GetSection("Auth0:Auth0TokenUrl").Value).Returns("https://dev-sunny-benefits.us.auth0.com/oauth/token");

            _configuration.Setup(v => v.GetSection("Auth0:client_secret").Value).Returns("AUTH0_CLIENT_SECRET");
            _vault.Setup(c => c.GetSecret("AUTH0_CLIENT_SECRET")).ReturnsAsync("sHQ0KyJNq7leq7jEpYSLMsubqZ3Zb3hQJyPwF0iyCQbEoKpsrWm5LPgU23d8UBPc");

            _configuration.Setup(c => c.GetSection("Auth0:audience").Value).Returns("https://dev-sunny-benefits.us.auth0.com/api/v2/");

            _configuration.Setup(v => v.GetSection("Auth0:client_id").Value).Returns("AUTH0_CLIENT_ID");
            _vault.Setup(c => c.GetSecret("AUTH0_CLIENT_ID")).ReturnsAsync("TmzHdUNt0sjiJkYFs3CEks0utYeo8FF5");

            _configuration.Setup(c => c.GetSection("Auth0:Domain").Value).Returns("dev-sunny-benefits.us.auth0.com");
            _configuration.Setup(c => c.GetSection("Auth0:grant_type").Value).Returns("client_credentials");

            _configuration.Setup(c => c.GetSection("Auth0:Auth0ApiUrl").Value).Throws(new Exception("Simulated exception"));
            var result =  await _auth0Helper.GetUserById(userRequestDto);
            Assert.True(result != null);
        }
        [Fact]
        public async Task InternalLogin_Return_Ok_Response()
        {
            var requestDto = new LoginRequestDto(){ ConsumerCode = "sample-consumer" };
            var mockResponse = new ConsumerLoginResponseDto()
            { 
                ConsumerCode = "sample-consumer",
                Jwt = "sample-jwt"
             };
            var auth0Helper = new Mock<IAuth0Helper>();
            _vault.Setup(v => v.GetSecret("env")).ReturnsAsync("Development");

            _userClient.Setup(client => client.Post<ConsumerLoginResponseDto>(CommonConstants.ConsumerLoginUrl, It.IsAny<ConsumerLoginRequestDto>()))
                            .ReturnsAsync(mockResponse);
            var loginService = new LoginService(_loginServiceLogger.Object, _userClient.Object, _fisClient.Object, _personHelper.Object, _eventService, _vault.Object, _httpContextAccessor.Object, _UploadPdfService.Object);
            var controller = new LoginController(_loginControllerLogger.Object, loginService, auth0Helper.Object);

            // Act
            var response = await controller.InternalLogin(requestDto);
            var result = response.Result as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
        }
        [Fact]
        public async Task InternalLogin_Return_Return_NotFound_Response()
        {
            var requestDto = new LoginRequestDto() { ConsumerCode = "sample-consumer" };
            var mockResponse = new ConsumerLoginResponseDto()
            {
                ConsumerCode = "sample-consumer",
                Jwt = null
            };
            var auth0Helper = new Mock<IAuth0Helper>();
            _vault.Setup(v => v.GetSecret("env")).ReturnsAsync("Development");

            _userClient.Setup(client => client.Post<ConsumerLoginResponseDto>(CommonConstants.ConsumerLoginUrl, It.IsAny<ConsumerLoginRequestDto>()))
                              .ReturnsAsync(mockResponse);
            var loginService = new LoginService(_loginServiceLogger.Object, _userClient.Object, _fisClient.Object, _personHelper.Object, _eventService, _vault.Object, _httpContextAccessor.Object, _UploadPdfService.Object);
            var controller = new LoginController(_loginControllerLogger.Object, loginService, auth0Helper.Object);

            // Act
            var response = await controller.InternalLogin(requestDto);
            var result = response.Result as ObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(404, result.StatusCode);
        }
        [Fact]
        public async Task InternalLogin_Return_Return_NotFound_whenenvNotThere_Response()
        {
            var requestDto = new LoginRequestDto() { ConsumerCode = "sample-consumer" };
            var auth0Helper = new Mock<IAuth0Helper>();
            _vault.Setup(v => v.GetSecret("env")).ReturnsAsync("");

            var loginService = new LoginService(_loginServiceLogger.Object, _userClient.Object, _fisClient.Object, _personHelper.Object, _eventService, _vault.Object, _httpContextAccessor.Object, _UploadPdfService.Object);
            var controller = new LoginController(_loginControllerLogger.Object, loginService, auth0Helper.Object);

            // Act
            var response = await controller.InternalLogin(requestDto);
            var result = response.Result as ObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(403, result.StatusCode);
        }
        [Fact]
        public async Task InternalLogin_ThrowsException()
        {
            var requestDto = new LoginRequestDto() { ConsumerCode = "sample-consumer" };
            var mockResponse = new ConsumerLoginResponseDto()
            {
                ConsumerCode = "sample-consumer",
                Jwt = null
            };
            var auth0Helper = new Mock<IAuth0Helper>();
            _vault.Setup(v => v.GetSecret("env")).ReturnsAsync("Development");

            _userClient.Setup(client => client.Post<ConsumerLoginResponseDto>(CommonConstants.ConsumerLoginUrl, It.IsAny<ConsumerLoginRequestDto>()))
                              .ThrowsAsync(new Exception("simulated Exception"));
            var loginService = new LoginService(_loginServiceLogger.Object, _userClient.Object, _fisClient.Object, _personHelper.Object, _eventService, _vault.Object, _httpContextAccessor.Object, _UploadPdfService.Object);
            var controller = new LoginController(_loginControllerLogger.Object, loginService, auth0Helper.Object);

            // Act
            var response = await controller.InternalLogin(requestDto);
            var result = response.Result as ObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(500, result.StatusCode);
        }

        [Fact]
        public async Task GetConsumerByPersonUniqueIdentifier_ReturnsValidResponse_WhenDataExists()
        {
            // Arrange
            var consumerCode = "C999";
            var expectedPerson = new PersonDto { PersonId = 1 };
            var expectedConsumer = new ConsumerDto { ConsumerId = 10, PersonId = 1 };

            _userClient.Setup(x => x.Post<GetPersonAndConsumerResponseDto>(
                CommonConstants.GetPersonAndConsumerAPIUrl,
                It.Is<GetConsumerRequestDto>(dto => dto.ConsumerCode == consumerCode)))
                .ReturnsAsync(new GetPersonAndConsumerResponseDto
                {
                    Person = expectedPerson,
                    Consumer = expectedConsumer
                });

            // Act
            var result = await _loginService.GetPersonAndConsumerDetails(consumerCode);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedPerson.PersonId, result?.Person?.PersonId);
            Assert.Equal(expectedConsumer.ConsumerId, result.Consumer[0].ConsumerId);
        }

        [Fact]
        public async Task GetPersonAndConsumerDetails_ReturnsEmptyResponse_WhenDataIsMissing()
        {
            // Arrange
            var consumerCode = "C999";

            _userClient.Setup(x => x.Post<GetPersonAndConsumerResponseDto>(
                CommonConstants.GetPersonAndConsumerAPIUrl,
                It.Is<GetConsumerRequestDto>(dto => dto.ConsumerCode == consumerCode)))
                .ReturnsAsync(new GetPersonAndConsumerResponseDto());

            // Act
            var result = await _loginService.GetPersonAndConsumerDetails(consumerCode);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result.Consumer);
        }

        [Fact]
        public async Task GetPersonAndConsumerDetails_ThrowsException_WhenClientFails()
        {
            // Arrange
            var consumerCode = "CERROR";

            _userClient.Setup(x => x.Post<GetPersonAndConsumerResponseDto>(
                CommonConstants.GetPersonAndConsumerAPIUrl,
                It.IsAny<GetConsumerRequestDto>()))
                .ThrowsAsync(new Exception("Service Unavailable"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() =>
                _loginService.GetPersonAndConsumerDetails(consumerCode));

            Assert.Equal("Service Unavailable", exception.Message);
        }

        [Fact]
        public async Task GetConsumerByPersonUniqueIdentifier_Returns400_WhenEmailAndIdentifierMissing()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            _httpContextAccessor.Setup(h => h.HttpContext).Returns(httpContext);

            // Act
            var result = await _loginService.GetConsumerByPersonUniqueIdentifier(null);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(StatusCodes.Status400BadRequest, result.ErrorCode);
            Assert.Equal("Email or PersonUniqueIdentifier is required", result.ErrorMessage);
        }

        [Fact]
        public async Task GetConsumerByPersonUniqueIdentifier_UsesIdentifierFromHttpContext_WhenAvailable()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            httpContext.Items[HttpContextKeys.PersonUniqueIdentifier] = "abc_123";
            _httpContextAccessor.Setup(h => h.HttpContext).Returns(httpContext);

            var expectedResponse = new GetConsumerByPersonUniqueIdentifierResponseDto
            {
                Consumer = new[] { new ConsumerDto { MemberNbr = "999" } },
                Person = new PersonDto { Email = "abc@test.com" }
            };

            _userClient.Setup(c => c.GetId<GetConsumerByPersonUniqueIdentifierResponseDto>(
                It.IsAny<string>(), It.IsAny<Dictionary<string, string>>()))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _loginService.GetConsumerByPersonUniqueIdentifier("fallback@test.com");

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Consumer);
        }

        [Fact]
        public async Task GetConsumerByPersonUniqueIdentifier_UsesEmailAsFallback_WhenIdentifierMissing()
        {
            // Arrange
            var httpContext = new DefaultHttpContext(); // No identifier in Items
            _httpContextAccessor.Setup(h => h.HttpContext).Returns(httpContext);

            var expectedResponse = new GetConsumerByPersonUniqueIdentifierResponseDto
            {
                Consumer = new[] { new ConsumerDto { MemberNbr = "123" } },
                Person = new PersonDto { Email = "fallback@test.com" }
            };

            _userClient.Setup(c => c.GetId<GetConsumerByPersonUniqueIdentifierResponseDto>(
                It.IsAny<string>(), It.Is<Dictionary<string, string>>(d => d["personUniqueIdentifier"] == HttpUtility.UrlEncode("fallback@test.com"))))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _loginService.GetConsumerByPersonUniqueIdentifier("fallback@test.com");

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Consumer);
        }

        [Fact]
        public async Task GetConsumerByPersonUniqueIdentifier_ThrowsException_WhenApiFails()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            httpContext.Items[HttpContextKeys.PersonUniqueIdentifier] = "abc_123";
            _httpContextAccessor.Setup(h => h.HttpContext).Returns(httpContext);

            _userClient.Setup(c => c.GetId<GetConsumerByPersonUniqueIdentifierResponseDto>(
                It.IsAny<string>(), It.IsAny<Dictionary<string, string>>()))
                .ThrowsAsync(new Exception("API failure"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() =>
                _loginService.GetConsumerByPersonUniqueIdentifier("any@email.com"));
        }
    }
}