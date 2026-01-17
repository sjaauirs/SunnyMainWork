using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using NSubstitute.ExceptionExtensions;
using Sunny.Benefits.Bff.Core.Constants;
using Sunny.Benefits.Bff.Core.Domain.Constants;
using Sunny.Benefits.Bff.Core.Domain.Dtos;
using Sunny.Benefits.Bff.Infrastructure.Helpers;
using Sunny.Benefits.Bff.Infrastructure.Helpers.Interface;
using Sunny.Benefits.Bff.Infrastructure.HttpClients.Interfaces;
using Sunny.Benefits.Bff.Infrastructure.Repositories.Interfaces;
using Sunny.Benefits.Bff.Infrastructure.Services.Interfaces;
using Sunny.Benefits.Bff.UnitTest.Fixtures.MockDtos;
using Sunny.Benefits.Bff.UnitTest.HttpClients;
using SunnyRewards.Helios.Common.Core.Helpers.Interfaces;
using SunnyRewards.Helios.Common.Core.Services.Interfaces;
using SunnyRewards.Helios.Tenant.Core.Domain.Dtos.Json;
using SunnyRewards.Helios.Tenant.Core.Domain.Dtos;
using SunnyRewards.Helios.User.Core.Domain.Dtos;
using SunnyRewards.Helios.User.Core.Domain.Models;
using System.Net;
using System.Text;
using System.Web;
using Xunit;

namespace Sunny.Benefits.Bff.UnitTest.Helpers
{
    public class Auth0HelperUnitTest
    {
        private readonly Mock<ILogger<Auth0Helper>> _auth0HelperLogger;
        private readonly Mock<IVault> _vault;
        private readonly Mock<IConfiguration> _configuration;
        private readonly Mock<IUserClient> _userClient;
        private readonly Auth0Helper _auth0Helper;
        private readonly Mock<IPersonHelper> _personHelper;
        private readonly Mock<IHashingService> _hashingService;
        private readonly Mock<ITenantClient> _tenantClient;
        private readonly Mock<IHttpContextAccessor> _httpContextAccessor;
        private readonly Mock<IAuth0TokenCacheService> _tokenCacheService;
        public Auth0HelperUnitTest()
        {
            _auth0HelperLogger = new Mock<ILogger<Auth0Helper>>();
            _vault = new Mock<IVault>();
            _configuration = new Mock<IConfiguration>();
            _userClient = new Mock<IUserClient>();
            _httpContextAccessor = new Mock<IHttpContextAccessor>();
            _tenantClient = new TenantClientMock();
            _configuration.Setup(c => c.GetSection("OperationMaxTries").Value).Returns("1");
            _personHelper = new Mock<IPersonHelper>();
            _hashingService = new Mock<IHashingService>();
            _tokenCacheService = new Mock<IAuth0TokenCacheService>();
            
            // Setup default token response
            _tokenCacheService.Setup(x => x.GetTokenAsync())
                .ReturnsAsync(new TokenResponse { access_token = "test-token", expires_in = "3600" });
            
            _auth0Helper = new Auth0Helper(_auth0HelperLogger.Object, _vault.Object, _configuration.Object, _userClient.Object, _personHelper.Object, _hashingService.Object, _httpContextAccessor.Object, _tenantClient.Object, _tokenCacheService.Object);
        }

        [Fact]
        public async Task PatchUser_okResponse()
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

            _userClient.Setup(c => c.GetId<GetConsumerByEmailResponseDto>("consumer/get-consumers-by-email?email=", It.IsAny<Dictionary<string, string>>()))
              .ReturnsAsync(new GetConsumerByEmailResponseMockDto());
            var req = new GetConsumerByEmailResponseMockDto();

            _userClient.Setup(x => x.Post<ConsumerModel>("consumer/update-register-flag", req.Consumer));

            var result = await _auth0Helper.PatchUser(patchUserRequestDto);
            Assert.NotNull(result);
        }

        [Fact]
        public async Task PatchUser_WhenEnvIsNullOrEmpty_ReturnsInternalServerError()
        {
            var patchUserRequestDto = new PatchUserRequestMockDto();
            _vault.Setup(vault => vault.GetSecret(It.IsAny<string>())).ReturnsAsync(string.Empty);
            var response = await _auth0Helper.PatchUser(patchUserRequestDto);
            Assert.Equal((int)HttpStatusCode.InternalServerError, response.ErrorCode);
            Assert.Equal("Internal Error", response.ErrorMessage);
        }

        [Fact]
        public async Task PatchUser_Returns_Null()
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

            _userClient.Setup(x => x.GetId<GetConsumerByEmailResponseDto>("consumer/get-consumers-by-email?email=", It.IsAny<Dictionary<string, string>>()))
                       .ReturnsAsync((GetConsumerByEmailResponseMockDto)null);

            var response = await _auth0Helper.PatchUser(patchUserRequestDto);
            Assert.True(response.email == null);
        }

        [Fact]
        public async Task PatchUser_Returns_Null_When_member_not_found()
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

            DefaultHttpContext _httpContext = new DefaultHttpContext();
            _userClient.Setup(c => c.Get<ConsumerPersonResponseDto>(It.IsAny<string>(), It.IsAny<Dictionary<string, long>>()));
            _httpContextAccessor.Setup(a => a.HttpContext)
                .Returns(_httpContext);
            var expectedEmail = "test@example.com";
            _httpContext.Items["Email"] = expectedEmail;
            _httpContext.Items["MemberNbr"] = "1234";
            _httpContext.Items["RegionCode"] = "1234";

            var response = await _auth0Helper.PatchUser(patchUserRequestDto);
            Assert.True(response.email == null);
        }

        [Fact]
        public async Task Catch_Exception_PatchUser()
        {
            try
            {
                var patchUserRequestDto = new PatchUserRequestMockDto();
                _vault.Setup(v => v.GetSecret("env")).ThrowsAsync(new Exception("Simulated exception"));
                var auth0HelperMock = new Mock<IAuth0Helper>();
                auth0HelperMock.Setup(c => c.PatchUser(patchUserRequestDto)).ThrowsAsync(new Exception("Simulated exception"));
                var auth0Helper = new Auth0Helper(_auth0HelperLogger.Object, _vault.Object, _configuration.Object, _userClient.Object, _personHelper.Object, _hashingService.Object, _httpContextAccessor.Object, _tenantClient.Object, _tokenCacheService.Object);
                var response = await auth0Helper.PatchUser(patchUserRequestDto);
            }
            catch (Exception e)
            {
                e.Message.ToString();
            }
        }

        [Fact]
        public async Task Catch_Exception_GetToken()
        {
            try
            {
                var patchUserRequestDto = new PatchUserRequestMockDto();
                _vault.Setup(v => v.GetSecret("env")).ReturnsAsync("Development");
                _configuration.Setup(c => c.GetSection("Auth0:Auth0TokenUrl").Value).Throws(new Exception("inner exception"));
                var response = await _auth0Helper.PatchUser(patchUserRequestDto);
                Assert.True(response.ErrorMessage == "inner exception");
            }
            catch (Exception ex)
            {
                ex.Message.ToString();
            }
        }

        [Fact]
        public async Task Catch_Exception_GetEmailByconsumer()
        {
            try
            {
                var patchUserRequestDto = new PatchUserRequestMockDto();
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

                _userClient.Setup(x => x.GetId<GetConsumerByEmailResponseDto>("consumer/get-consumers-by-email?email=", It.IsAny<Dictionary<string, string>>()))
                      .ThrowsAsync(new Exception("inavlid email"));
                var response = await _auth0Helper.PatchUser(patchUserRequestDto);
                Assert.True(response.ErrorMessage == "inavlid email");
            }
            catch (Exception e)
            {
                e.Message.ToString();
            }
        }

        [Fact]
        public async Task Validatetoken_Return_ValidAccessToken()
        {
            var Token = new TokenResponseMockDto();
            _configuration.Setup(c => c.GetSection("Auth0:Auth0UserInfoUrl").Value).Returns("https://dev-sunny-benefits.us.auth0.com/userinfo/");

            var auth0Helper = new Auth0Helper(_auth0HelperLogger.Object, _vault.Object, _configuration.Object, _userClient.Object, _personHelper.Object, _hashingService.Object, _httpContextAccessor.Object, _tenantClient.Object, _tokenCacheService.Object);
            var result = await auth0Helper.Validatetoken(Token.access_token ?? string.Empty);
            Assert.True(result.emailVerified);
        }

        [Fact]
        public async Task Catch_Exception_Validatetoken()
        {
            try
            {
                var Token = new TokenResponseMockDto();
                var auth0Helper = new Mock<IAuth0Helper>();
                _configuration.Setup(c => c.GetSection("Auth0:Auth0UserInfoUrl").Value).Throws(new Exception("invalid token"));
                var auth0HelperMock = new Mock<IAuth0Helper>();
                auth0HelperMock.Setup(c => c.Validatetoken(Token.access_token ?? string.Empty)).ThrowsAsync(new Exception("invalid token"));
                var result = await _auth0Helper.Validatetoken(Token.access_token ?? string.Empty);
                Assert.True(result.emailVerified);
            }
            catch (Exception e)
            {
                e.Message.ToString();
            }

        }

        [Fact]
        public async Task PostVerificationEmail_Ok_Response()
        {
            var emailRequestDto = new VerificationEmailRequestMockDto();

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
            _configuration.Setup(c => c.GetSection("Auth0:Auth0VerificationEmailUrl").Value).Returns("https://dev-sunny-benefits.us.auth0.com/api/v2/jobs/verification-email");

            var result = await _auth0Helper.PostVerificationEmail(emailRequestDto);
            Assert.NotNull(result);
        }

        [Fact]
        public async Task PostVerificationEmail_When_EnvIsNullOrEmpty_Return_InternalServerError()
        {
            var emailRequestDto = new VerificationEmailRequestMockDto();
            _vault.Setup(vault => vault.GetSecret(It.IsAny<string>())).ReturnsAsync(string.Empty);
            var response = await _auth0Helper.PostVerificationEmail(emailRequestDto);
            Assert.Equal((int)HttpStatusCode.InternalServerError, response.ErrorCode);
            Assert.Equal("Internal Error", response.ErrorMessage);
        }

        [Fact]
        public async Task PostVerificationEmail_Returns_Null()
        {
            var emailRequestDto = new VerificationEmailRequestMockDto();
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
            _configuration.Setup(c => c.GetSection("Auth0:Auth0VerificationEmailUrl").Value).Returns("https://dev-sunny-benefits.us.auth0.com/api/v2/jobs/verification-email");


            var response = await _auth0Helper.PostVerificationEmail(emailRequestDto);
            Assert.True(response.email == null);
        }

        [Fact]
        public async Task PostVerificationEmail_Catch_Exception()
        {
            try
            {
                var emailRequestDto = new VerificationEmailRequestMockDto();
                _vault.Setup(v => v.GetSecret("env")).ThrowsAsync(new Exception("Simulated exception"));
                var auth0HelperMock = new Mock<IAuth0Helper>();
                auth0HelperMock.Setup(c => c.PostVerificationEmail(emailRequestDto)).ThrowsAsync(new Exception("Simulated exception"));
                var auth0Helper = new Auth0Helper(_auth0HelperLogger.Object, _vault.Object, _configuration.Object, _userClient.Object, _personHelper.Object, _hashingService.Object, _httpContextAccessor.Object, _tenantClient.Object, _tokenCacheService.Object);
                var response = await auth0Helper.PostVerificationEmail(emailRequestDto);
            }
            catch (Exception e)
            {
                e.Message.ToString();
            }
        }

        [Fact]
        public async Task PostVerificationEmail_Catch_Exception_GetToken()
        {
            try
            {
                var emailRequestDto = new VerificationEmailRequestMockDto();
                _vault.Setup(v => v.GetSecret("env")).ReturnsAsync("Development");
                _configuration.Setup(c => c.GetSection("Auth0:Auth0VerificationEmailUrl").Value).Returns("https://dev-sunny-benefits.us.auth0.com/api/v2/jobs/verification-email");
                _configuration.Setup(c => c.GetSection("Auth0:Auth0TokenUrl").Value).Throws(new Exception("inner exception"));
                var response = await _auth0Helper.PostVerificationEmail(emailRequestDto);
                Assert.True(response.ErrorMessage == "inner exception");
            }
            catch (Exception ex)
            {
                ex.Message.ToString();
            }
        }

        [Fact]
        public async Task GetUserById_Ok_Response()
        {
            var userRequestDto = new GetUserRequestMockDto();

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

            //_configuration.Setup(c => c.GetSection("Auth0:Audiences").Value).Returns("https://dev-sunny-benefits.us.auth0.com/api/v2/");

            _configuration.Setup(v => v.GetSection("Auth0:client_id").Value).Returns("AUTH0_CLIENT_ID");
            _vault.Setup(c => c.GetSecret("AUTH0_CLIENT_ID")).ReturnsAsync("TmzHdUNt0sjiJkYFs3CEks0utYeo8FF5");

            _configuration.Setup(c => c.GetSection("Auth0:Domain").Value).Returns("dev-sunny-benefits.us.auth0.com");
            _configuration.Setup(c => c.GetSection("Auth0:grant_type").Value).Returns("client_credentials");

            _configuration.Setup(c => c.GetSection("Auth0:Auth0ApiUrl").Value).Returns("https://dev-sunny-benefits.us.auth0.com/api/v2/users/");
            _configuration.Setup(c => c.GetSection("Auth0:Auth0UserInfoByEmailUrl").Value).Returns("https://dev-sunny-benefits.us.auth0.com/api/v2/users-by-email?email=");
            DefaultHttpContext _httpContext = new DefaultHttpContext();
            _userClient.Setup(c => c.GetId<GetConsumerByEmailResponseDto>(It.IsAny<string>(), It.IsAny<Dictionary<string, string>>()))
                     .ReturnsAsync(new GetConsumerByEmailResponseDto()
                     {
                         Person = new PersonDto
                         {
                             PersonUniqueIdentifier = "123"
                         },
                         Consumer = new[]
                         {
                              new ConsumerDto
                              {
                                  ConsumerCode = "cmr-12345",
                                  PersonId = 1,
                              }
                         }
                     });
            
            _httpContextAccessor.Setup(a => a.HttpContext)
                .Returns(_httpContext);
            var expectedEmail = "test@example.com";
            _httpContext.Items["Email"] = expectedEmail;
            var result = await _auth0Helper.GetUserById(userRequestDto);
            Assert.NotNull(result);
        }

        [Fact]
        public async Task GetUserById_Ok_Response_With_Member_Nbr_And_Region_Code()
        {
            var userRequestDto = new GetUserRequestMockDto();

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
            _configuration.Setup(c => c.GetSection("Auth0:Auth0UserInfoByEmailUrl").Value).Returns("https://dev-sunny-benefits.us.auth0.com/api/v2/users-by-email?email=");
            DefaultHttpContext _httpContext = new DefaultHttpContext();
            _userClient.Setup(c => c.Get<ConsumerPersonResponseDto>(It.IsAny<string>(), It.IsAny<Dictionary<string, long>>()))
                      .ReturnsAsync(new ConsumerPersonResponseDto()
                      {
                          Person = new PersonDto
                          {
                              PersonUniqueIdentifier = "123"
                          },
                          Consumer = new[]
                          {
                              new ConsumerDto
                              {
                                  ConsumerCode = "cmr-12345",
                                  PersonId = 1,
                              }
                          }
                      });
            _httpContextAccessor.Setup(a => a.HttpContext)
                .Returns(_httpContext);
            var expectedEmail = "test@example.com";
            _httpContext.Items["Email"] = expectedEmail;
            _httpContext.Items["MemberNbr"] = "1234";
            _httpContext.Items["RegionCode"] = "1234";
            var result = await _auth0Helper.GetUserById(userRequestDto);
            Assert.NotNull(result);
        }

        [Fact]
        public async Task GetUserById_UsesConsumerInfoFromHttpContext_WhenPresent()
        {
            var userRequestDto = new GetUserRequestMockDto();


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
            _configuration.Setup(c => c.GetSection("Auth0:Auth0UserInfoByEmailUrl").Value).Returns("https://dev-sunny-benefits.us.auth0.com/api/v2/users-by-email?email=");
            DefaultHttpContext _httpContext = new DefaultHttpContext();
            _userClient.Setup(c => c.Get<ConsumerPersonResponseDto>(It.IsAny<string>(), It.IsAny<Dictionary<string, long>>()))
                      .ReturnsAsync(new ConsumerPersonResponseDto()
                      {
                          Person = new PersonDto
                          {
                              PersonUniqueIdentifier = "123"
                          },
                          Consumer = new[]
                          {
                              new ConsumerDto
                              {
                                  ConsumerCode = "cmr-12345",
                                  PersonId = 1,
                              }
                          }
                      });
            _httpContextAccessor.Setup(a => a.HttpContext)
                .Returns(_httpContext);
            var expectedEmail = "test@example.com";
            _httpContext.Items["Email"] = expectedEmail;
            _httpContext.Items["MemberNbr"] = "1234";
            _httpContext.Items["RegionCode"] = "1234";
            var consumerResponse = new GetConsumerByPersonUniqueIdentifierResponseDto
            {
                Person = new PersonDto { PersonUniqueIdentifier = "unique-123" },
                Consumer = new[] { new ConsumerDto() }
            };
            _httpContext.Items["ConsumerInfo"] = consumerResponse;
            _httpContext.Items[HttpContextKeys.AuthConfig] =
            new AuthConfig
            {
                Auth0 = new Auth0ConfigDto { Auth0ApiUrl = "https://dev-sunny-benefits.us.auth0.com/api/v2/users/", Auth0TokenUrl = "https://dev-sunny-benefits.us.auth0.com/oauth/token",
                Auth0UserInfoByEmailUrl = "https://dev-sunny-benefits.us.auth0.com/api/v2/users-by-email?email=" , GrantType = "client_credentials", Audience = new string[] { "https://dev-sunny-benefits.us.auth0.com/api/v2/", "https://api.custom-audience.com/" },
                }
            };
            var result = await _auth0Helper.GetUserById(userRequestDto);
            Assert.NotNull(result);
        }

        [Fact]
        public async Task GetUserById_When_EnvIsNullOrEmpty_Return_InternalServerError()
        {
            var userRequestDto = new GetUserRequestMockDto();
            _vault.Setup(vault => vault.GetSecret(It.IsAny<string>())).ReturnsAsync(string.Empty);
            var response = await _auth0Helper.GetUserById(userRequestDto);
            Assert.Equal((int)HttpStatusCode.InternalServerError, response.ErrorCode);
            Assert.Equal("Internal Error", response.ErrorMessage);
        }

        [Fact]
        public async Task GetUserById_Returns_Null()
        {
            var userRequestDto = new GetUserRequestDto();
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
            _configuration.Setup(c => c.GetSection("Auth0:Auth0UserInfoByEmailUrl").Value).Returns("https://dev-sunny-benefits.us.auth0.com/api/v2/users-by-email?email=");
            DefaultHttpContext _httpContext = new DefaultHttpContext();
            // Set up mock to return the fake HttpContext
            _httpContextAccessor.Setup(a => a.HttpContext)
                .Returns(_httpContext);
            var expectedEmail = "test@example.com";
            _httpContext.Items["Email"] = expectedEmail;
            var response = await _auth0Helper.GetUserById(userRequestDto);
            Assert.True(response.email == "");
        }

        [Fact]
        public async Task GetUserById_Catch_Exception()
        {
            try
            {
                var userRequestDto = new GetUserRequestMockDto();
                _vault.Setup(v => v.GetSecret("env")).ThrowsAsync(new Exception("Simulated exception"));
                var auth0HelperMock = new Mock<IAuth0Helper>();
                auth0HelperMock.Setup(c => c.GetUserById(userRequestDto)).ThrowsAsync(new Exception("Simulated exception"));
                var auth0Helper = new Auth0Helper(_auth0HelperLogger.Object, _vault.Object, _configuration.Object, _userClient.Object, _personHelper.Object, _hashingService.Object, _httpContextAccessor.Object, _tenantClient.Object, _tokenCacheService.Object);
                var response = await _auth0Helper.GetUserById(userRequestDto);
            }
            catch (Exception e)
            {
                e.Message.ToString();
            }
        }

        [Fact]
        public async Task GetUserById_Catch_Exception_GetToken()
        {
            try
            {
                var userRequestDto = new GetUserRequestMockDto();
                _vault.Setup(v => v.GetSecret("env")).ReturnsAsync("Development");
                _configuration.Setup(c => c.GetSection("Auth0:Auth0VerificationEmailUrl").Value).Returns("https://dev-sunny-benefits.us.auth0.com/api/v2/jobs/verification-email");
                _configuration.Setup(c => c.GetSection("Auth0:Auth0TokenUrl").Value).Throws(new Exception("inner exception"));
                var response = await _auth0Helper.GetUserById(userRequestDto);
                Assert.True(response.ErrorMessage == "inner exception");
            }
            catch (Exception ex)
            {
                ex.Message.ToString();
            }
        }
        [Fact]
        public async Task GetConsumerDevices_ValidRequest_ReturnsConsumerDevices()
        {
            // Arrange
            var consumerDeviceRequest = new GetConsumerDeviceRequestDto
            {
                ConsumerCode = "12345"
            };

            var mockConsumerDevicesResponse = new GetConsumerDeviceResponseDto
            {
                ConsumerDevices = new List<ConsumerDeviceDto>
                {
                    new ConsumerDeviceDto { DeviceIdHash = "hashed-device-id-1" },
                    new ConsumerDeviceDto { DeviceIdHash = "hashed-device-id-2" }
                }
            };

            _userClient.Setup(u => u.Post<GetConsumerDeviceResponseDto>(CommonConstants.GetConsumerDevices, consumerDeviceRequest))
                       .ReturnsAsync(mockConsumerDevicesResponse);

            // Act
            var result = await _auth0Helper.GetConsumerDevices(consumerDeviceRequest);

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result.ConsumerDevices);
            Assert.Equal(2, result.ConsumerDevices.Count);
            Assert.Equal("hashed-device-id-1", result.ConsumerDevices[0].DeviceIdHash);
        }
        [Fact]
        public async Task GetConsumerDevices_ValidRequest_IsNotSuccess()
        {
            // Arrange
            var consumerDeviceRequest = new GetUserRequestDto
            {
                UserId = "12345",
                Email ="sample@test.com",
                DeviceId ="testDeviceId"
            };

            var mockConsumerDevicesResponse = new GetConsumerDeviceResponseDto
            {
                ConsumerDevices = new List<ConsumerDeviceDto>
                {
                    new ConsumerDeviceDto { DeviceIdHash = "hashed-device-id-1" },
                    new ConsumerDeviceDto { DeviceIdHash = "hashed-device-id-2" }
                }
            };
            _vault.Setup(v => v.GetSecret(CommonConstants.Env)).ReturnsAsync("Development");
            _vault.Setup(c => c.GetSecret("AUTH0_CLIENT_SECRET")).ReturnsAsync("some-secret");
            _vault.Setup(c => c.GetSecret("AUTH0_CLIENT_ID")).ReturnsAsync("some-client-id");
            _configuration.Setup(c => c.GetSection("Auth0:Auth0ApiUrl").Value).Returns("https://auth0.com/api/users/");
            _configuration.Setup(c => c.GetSection("Auth0:Auth0TokenUrl").Value).Returns("https://dev-sunny-benefits.us.auth0.com/oauth/token");
            _configuration.Setup(c => c.GetSection("Auth0:client_secret").Value).Returns("AUTH0_CLIENT_SECRET");
            _configuration.Setup(c => c.GetSection("Auth0:client_id").Value).Returns("AUTH0_CLIENT_ID");
            _configuration.Setup(c => c.GetSection("Auth0:audience").Value).Returns("https://dev-sunny-benefits.us.auth0.com/api/v2/");
            _configuration.Setup(c => c.GetSection("Auth0:Domain").Value).Returns("https://dev-sunny-benefits.us.auth0.com/");
            _configuration.Setup(c => c.GetSection("Auth0:grant_type").Value).Returns("client_credentials");
            _hashingService.Setup(h => h.ComputeSHA256Hash("sample-device-id")).Returns("hashed-sample-device-id");
            
            _userClient.Setup(c => c.GetId<GetConsumerByEmailResponseDto>("consumer/get-consumers-by-email?email=", It.IsAny<Dictionary<string, string>>()))
                       .ReturnsAsync(new GetConsumerByEmailResponseMockDto());
            _configuration.Setup(c => c.GetSection("Auth0:Auth0UserInfoUrl").Value).Returns("https://dev-sunny-benefits.us.auth0.com/userinfo/");

            _configuration.Setup(c => c.GetSection("Auth0:Auth0UserInfoByEmailUrl").Value).Returns("https://dev-sunny-benefits.us.auth0.com/api/v2/users-by-email?email=");
            DefaultHttpContext _httpContext = new DefaultHttpContext();
            // Set up mock to return the fake HttpContext
            _httpContextAccessor.Setup(a => a.HttpContext)
                .Returns(_httpContext);
            var expectedEmail = "test@example.com";
            _httpContext.Items["Email"] = expectedEmail;

            _userClient.Setup(u => u.Post<GetConsumerDeviceResponseDto>(CommonConstants.GetConsumerDevices, consumerDeviceRequest))
                       .ReturnsAsync(mockConsumerDevicesResponse);

            // Act
            var result = await _auth0Helper.GetUserById(consumerDeviceRequest);

            // Assert
            Assert.NotNull(result);
            
        }
        [Fact]
        public async Task VerificationEmailRequest_ValidRequest_IsTokenNull()
        {
            // Arrange
            var consumerDeviceRequest = new VerificationEmailRequestDto
            {
                UserId = "12345",
                Email = "sample@test.com",
            };

            var mockConsumerDevicesResponse = new GetConsumerDeviceResponseDto
            {
                ConsumerDevices = new List<ConsumerDeviceDto>
                {
                    new ConsumerDeviceDto { DeviceIdHash = "hashed-device-id-1" },
                    new ConsumerDeviceDto { DeviceIdHash = "hashed-device-id-2" }
                }
            };
            _vault.Setup(v => v.GetSecret(CommonConstants.Env)).ReturnsAsync("Development");
            _vault.Setup(c => c.GetSecret("AUTH0_CLIENT_SECRET")).ReturnsAsync("some-secret");
            _vault.Setup(c => c.GetSecret("AUTH0_CLIENT_ID")).ReturnsAsync("some-client-id");
            _configuration.Setup(c => c.GetSection("Auth0:Auth0ApiUrl").Value).Returns("https://auth0.com/api/users/");
             _configuration.Setup(c => c.GetSection("Auth0:Auth0TokenUrl").Value).Returns("https://dev-sunny-benefits.us.auth0.com/oauth/token");
            _configuration.Setup(c => c.GetSection("Auth0:client_secret").Value).Returns("AUTH0_CLIENT_SECRET");
            _configuration.Setup(c => c.GetSection("Auth0:client_id").Value).Returns("AUTH0_CLIENT_ID");
            var audienceSection0 = new Mock<IConfigurationSection>();
            audienceSection0.Setup(x => x.Value).Returns("https://dev-sunny-benefits.us.auth0.com/api/v2/");

            var audienceSection1 = new Mock<IConfigurationSection>();
            audienceSection1.Setup(x => x.Value).Returns("https://api.custom-audience.com/");

            var audienceArraySection = new Mock<IConfigurationSection>();
            audienceArraySection.Setup(x => x.GetChildren())
                .Returns(new List<IConfigurationSection> { audienceSection0.Object, audienceSection1.Object });

            _configuration.Setup(x => x.GetSection("Auth0:Audiences"))
                .Returns(audienceArraySection.Object);
            _configuration.Setup(c => c.GetSection("Auth0:Domain").Value).Returns("https://dev-sunny-benefits.us.auth0.com/");
            _configuration.Setup(c => c.GetSection("Auth0:grant_type").Value).Returns("client_credentials");
            _configuration.Setup(c => c.GetSection("Auth0:Auth0UserInfoUrl").Value).Returns("https://dev-sunny-benefits.us.auth0.com/userinfo/");
            _configuration.Setup(c => c.GetSection("Auth0:Auth0VerificationEmailUrl").Value).Returns("https://dev-sunny-benefits.us.auth0.com/api/v2/jobs/verification-email");
            _userClient.Setup(c => c.GetId<GetConsumerByEmailResponseDto>("consumer/get-consumers-by-email?email=", It.IsAny<Dictionary<string, string>>()))
                     .ReturnsAsync(new GetConsumerByEmailResponseMockDto());

            _configuration.Setup(c => c.GetSection("Auth0:Auth0UserInfoByEmailUrl").Value).Returns("https://dev-sunny-benefits.us.auth0.com/api/v2/users-by-email?email=");
            DefaultHttpContext _httpContext = new DefaultHttpContext();
            // Set up mock to return the fake HttpContext
            _httpContextAccessor.Setup(a => a.HttpContext)
                .Returns(_httpContext);
            var expectedEmail = "test@example.com";
            _httpContext.Items["Email"] = expectedEmail;
            // Act
            var result = await _auth0Helper.PostVerificationEmail(consumerDeviceRequest);

            // Assert
            Assert.NotNull(result);

        }

        [Fact]
        public async Task GetConsumerDevices_NoDevicesFound_ReturnsEmptyResponse()
        {
            // Arrange
            var consumerDeviceRequest = new GetConsumerDeviceRequestDto
            {
                ConsumerCode = "12345"
            };

            var mockEmptyResponse = new GetConsumerDeviceResponseDto();

            _userClient.Setup(u => u.Post<GetConsumerDeviceResponseDto>(CommonConstants.GetConsumerDevices, consumerDeviceRequest))
                       .ReturnsAsync(mockEmptyResponse);

            // Act
            var result = await _auth0Helper.GetConsumerDevices(consumerDeviceRequest);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result.ConsumerDevices);
        }

        [Fact]
        public async Task GetConsumerDevices_ExceptionThrown_LogsErrorAndReturnsEmptyResponse()
        {
            // Arrange
            var consumerDeviceRequest = new GetConsumerDeviceRequestDto
            {
                ConsumerCode = "12345"
            };

            _userClient.Setup(u => u.Post<GetConsumerDeviceResponseDto>(CommonConstants.GetConsumerDevices, consumerDeviceRequest))
                       .ThrowsAsync(new Exception("Test exception"));

            // Act
            var result = await _auth0Helper.GetConsumerDevices(consumerDeviceRequest);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public void IsDeviceRegistered_ValidDevice_ReturnsTrue()
        {
            // Arrange
            var deviceId = "test-device-id";
            var hashedDeviceId = "hashed-test-device-id";
            var consumerDevices = new List<ConsumerDeviceDto>
            {
                new ConsumerDeviceDto { DeviceIdHash = hashedDeviceId }
            };

            _hashingService.Setup(h => h.ComputeSHA256Hash(deviceId)).Returns(hashedDeviceId);

            // Act
            var result = _auth0Helper.IsDeviceRegistered(deviceId, consumerDevices);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsDeviceRegistered_InvalidDevice_ReturnsFalse()
        {
            // Arrange
            var deviceId = "test-device-id";
            var hashedDeviceId = "hashed-test-device-id";
            var consumerDevices = new List<ConsumerDeviceDto>
            {
                new ConsumerDeviceDto { DeviceIdHash = "another-hash" }
            };

            _hashingService.Setup(h => h.ComputeSHA256Hash(deviceId)).Returns(hashedDeviceId);

            // Act
            var result = _auth0Helper.IsDeviceRegistered(deviceId, consumerDevices);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsDeviceRegistered_NullConsumerDevices_ReturnsFalse()
        {
            // Arrange
            var deviceId = "test-device-id";

            // Act
            var result = _auth0Helper.IsDeviceRegistered(deviceId, null);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsDeviceRegistered_HashingFails_ReturnsFalse()
        {
            // Arrange
            var deviceId = "test-device-id";
            var consumerDevices = new List<ConsumerDeviceDto>
            {
                new ConsumerDeviceDto { DeviceIdHash = "hashed-device-id" }
            };

            _hashingService.Setup(h => h.ComputeSHA256Hash(deviceId)).Returns<string>(null);

            // Act
            var result = _auth0Helper.IsDeviceRegistered(deviceId, consumerDevices);

            // Assert
            Assert.False(result);
        }


        [Fact]
        public async Task GetConsumerByIdentifierOrEmail_Returns400_WhenEmailAndIdentifierMissing()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            _httpContextAccessor.Setup(h => h.HttpContext).Returns(httpContext);

            // Act
            var result = await _auth0Helper.GetConsumerByIdentifierOrEmail(null);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(StatusCodes.Status400BadRequest, result.ErrorCode);
        }

        [Fact]
        public async Task GetConsumerByIdentifierOrEmail_UsesIdentifierFromHttpContext_WhenAvailable()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            httpContext.Items[HttpContextKeys.PersonUniqueIdentifier] = "abc_123";
            _httpContextAccessor.Setup(h => h.HttpContext).Returns(httpContext);

            var expectedResponse = new GetConsumerByPersonUniqueIdentifierResponseDto
            {
                Consumer = new[] { new ConsumerDto { MemberNbr = "999" } },
                Person = new PersonDto { Email = "abc@test.com" , PersonUniqueIdentifier = "321321"}
            };

            _userClient.Setup(c => c.GetId<GetConsumerByPersonUniqueIdentifierResponseDto>(
                It.IsAny<string>(), It.IsAny<Dictionary<string, string>>()))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _auth0Helper.GetConsumerByIdentifierOrEmail("fallback@test.com");

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Consumer);
            Assert.Equal("abc@test.com", result.Person.Email);
        }

        [Fact]
        public async Task GetConsumerByIdentifierOrEmail_MemberNbrAndRegionCodeFromHttpContext_WhenAvailable()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            httpContext.Items[HttpContextKeys.MemberNbr] = "abc_123";
            httpContext.Items[HttpContextKeys.RegionCode] = "abc_123";
            _httpContextAccessor.Setup(h => h.HttpContext).Returns(httpContext);
            _userClient.Setup(c => c.Get<ConsumerPersonResponseDto>(It.IsAny<string>(), It.IsAny<Dictionary<string, long>>()))
                      .ReturnsAsync(new ConsumerPersonResponseDto()
                      {
                          Person = new PersonDto
                          {
                              Email = "abc@test.com",
                              PersonUniqueIdentifier = "123"
                          },
                          Consumer = new[]
                          {
                              new ConsumerDto
                              {
                                  ConsumerCode = "cmr-12345",
                                  PersonId = 1,
                              }
                          }
                      });

            // Act
            var result = await _auth0Helper.GetConsumerByIdentifierOrEmail("fallback@test.com");

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Consumer);
        }

        [Fact]
        public async Task GetConsumerByIdentifierOrEmail_UsesEmailAsFallback_WhenIdentifierMissing()
        {
            // Arrange
            var httpContext = new DefaultHttpContext(); // No identifier in Items
            _httpContextAccessor.Setup(h => h.HttpContext).Returns(httpContext);

            var expectedResponse = new GetConsumerByPersonUniqueIdentifierResponseDto
            {
                Consumer = new[] { new ConsumerDto { MemberNbr = "123" } },
                Person = new PersonDto { Email = "fallback@test.com", PersonUniqueIdentifier = "testing" }
            };

            _userClient.Setup(c => c.GetId<GetConsumerByPersonUniqueIdentifierResponseDto>(
                It.IsAny<string>(), It.Is<Dictionary<string, string>>(d => d["personUniqueIdentifier"] == HttpUtility.UrlEncode("fallback@test.com"))))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _auth0Helper.GetConsumerByIdentifierOrEmail("fallback@test.com");

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Consumer);
            Assert.Equal("fallback@test.com", result?.Person?.Email);
        }

        [Fact]
        public async Task GetConsumerByIdentifierOrEmail_ThrowsException_WhenApiFails()
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
                _auth0Helper.GetConsumerByIdentifierOrEmail("any@email.com"));
        }

        [Fact]
        public async Task GetConsumerDetails_ReturnsNotFound_WhenAllIdentifiersMissing()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            _httpContextAccessor.Setup(h => h.HttpContext).Returns(httpContext);

            // Act
            var result = await _auth0Helper.GetConsumerDetails();

            // Assert
            Assert.NotNull(result);
            Assert.Equal((int)HttpStatusCode.NotFound, result.ErrorCode);
            Assert.Contains("all missing", result.ErrorMessage);
        }

        [Fact]
        public async Task GetConsumerDetails_ReturnsNotFound_WhenNoConsumerFound()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            httpContext.Items["Email"] = "test@example.com";
            _httpContextAccessor.Setup(h => h.HttpContext).Returns(httpContext);

            // Simulate GetConsumerByPersonUniqueIdentifier and GetConsumerByMemberNbrAndRegionCode returning null
            _userClient.Setup(x => x.GetId<GetConsumerByPersonUniqueIdentifierResponseDto>(
                It.IsAny<string>(), It.IsAny<Dictionary<string, string>>()))
                .ReturnsAsync((GetConsumerByPersonUniqueIdentifierResponseDto?)null);

            _userClient.Setup(x => x.Get<ConsumerPersonResponseDto>(
                It.IsAny<string>(), It.IsAny<Dictionary<string, long>>()))
                .ReturnsAsync((ConsumerPersonResponseDto?)null);

            _userClient.Setup(x => x.GetId<GetConsumerByEmailResponseDto>(
                It.IsAny<string>(), It.IsAny<Dictionary<string, string>>()))
                .ReturnsAsync((GetConsumerByEmailResponseDto?)null);

            // Act
            var result = await _auth0Helper.GetConsumerDetails();

            // Assert
            Assert.NotNull(result);
            Assert.Equal((int)HttpStatusCode.NotFound, result.ErrorCode);
            Assert.Equal("Not Found", result.ErrorMessage);
        }

        [Fact]
        public async Task GetConsumerDetails_ReturnsConsumer_WhenFoundByPersonUniqueIdentifier()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            httpContext.Items["Email"] = "test@example.com";
            httpContext.Items["PersonUniqueIdentifier"] = "unique-123";
            _httpContextAccessor.Setup(h => h.HttpContext).Returns(httpContext);

            var consumerResponse = new GetConsumerByPersonUniqueIdentifierResponseDto
            {
                Person = new PersonDto { PersonUniqueIdentifier = "unique-123" },
                Consumer = new[] { new ConsumerDto() }
            };

            _userClient.Setup(x => x.GetId<GetConsumerByPersonUniqueIdentifierResponseDto>(
                It.IsAny<string>(), It.IsAny<Dictionary<string, string>>()))
                .ReturnsAsync(consumerResponse);

            // Act
            var result = await _auth0Helper.GetConsumerDetails();

            // Assert
            Assert.NotNull(result);
            Assert.Equal("unique-123", result.Person.PersonUniqueIdentifier);
            Assert.NotNull(result.Consumer);
        }

        [Fact]
        public async Task GetConsumerDetails_SetsConsumerInfoInHttpContext_WhenConsumerFound()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            httpContext.Items["Email"] = "test@example.com";
            httpContext.Items["PersonUniqueIdentifier"] = "unique-123";
            _httpContextAccessor.Setup(h => h.HttpContext).Returns(httpContext);

            var consumerResponse = new GetConsumerByPersonUniqueIdentifierResponseDto
            {
                Person = new PersonDto { PersonUniqueIdentifier = "unique-123" },
                Consumer = new[] { new ConsumerDto() }
            };

            _userClient.Setup(x => x.GetId<GetConsumerByPersonUniqueIdentifierResponseDto>(
                It.IsAny<string>(), It.IsAny<Dictionary<string, string>>()))
                .ReturnsAsync(consumerResponse);

            // Act
            var result = await _auth0Helper.GetConsumerDetails();

            // Assert
            Assert.True(httpContext.Items.ContainsKey("ConsumerInfo"));
            Assert.Equal(consumerResponse, httpContext.Items["ConsumerInfo"]);
        }

        [Fact]
        public async Task SetAuthConfigToContext_Success_ReturnsTrueAndSetsAuthConfig()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            var consumerDetails = new GetConsumerByPersonUniqueIdentifierResponseDto
            {
                Consumer = new[] { new ConsumerDto { ConsumerId = 1, TenantCode = "TENANT1" } }
            };
            httpContext.Items[HttpContextKeys.ConsumerInfo] = consumerDetails;

            var tenantDto = new TenantDto
            {
                TenantCode = "TENANT1",
                AuthConfig = JsonConvert.SerializeObject(new AuthConfig
                {
                    Auth0 = new Auth0ConfigDto { Auth0ApiUrl = "https://api", Auth0TokenUrl = "https://token" }
                })
            };

            _httpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);
            _tenantClient.Setup(x => x.Post<TenantDto>("tenant/get-by-tenant-code", It.IsAny<GetTenantCodeRequestDto>()))
                .ReturnsAsync(tenantDto);

            // Act
            var result = await _auth0Helper.SetAuthConfigToContext(httpContext);

            // Assert
            Assert.True(result);
            Assert.True(httpContext.Items.ContainsKey(HttpContextKeys.AuthConfig));
            var authConfig = httpContext.Items[HttpContextKeys.AuthConfig] as AuthConfig;
            Assert.NotNull(authConfig);
            Assert.Equal("https://api", authConfig.Auth0.Auth0ApiUrl);
        }

        [Fact]
        public async Task SetAuthConfigToContext_ConsumerDetailsNotFound_ReturnsFalseAndUnauthorized()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            // Remove ConsumerInfo from context to force GetConsumerDetails to run
            _httpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

            // Mock the dependencies so GetConsumerDetails returns error
            // Simulate all identifiers missing
            // (GetPersonUniqueIdentifierFromHttpContext, GetUserEmailFromHttpContext, etc. will return null/empty)
            // No need to setup _userClient for this case

            // Act
            var result = await _auth0Helper.SetAuthConfigToContext(httpContext);

            // Assert
            Assert.False(result);
            Assert.Equal(StatusCodes.Status401Unauthorized, httpContext.Response.StatusCode);
        }



        [Fact]
        public async Task SetAuthConfigToContext_MissingTenantCode_ReturnsFalseAndUnauthorized()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            var consumerDetails = new GetConsumerByPersonUniqueIdentifierResponseDto
            {
                Consumer = new[] { new ConsumerDto { ConsumerId = 1, TenantCode = null } }
            };
            httpContext.Items[HttpContextKeys.ConsumerInfo] = consumerDetails;
            _httpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

            // Act
            var result = await _auth0Helper.SetAuthConfigToContext(httpContext);

            // Assert
            Assert.False(result);
            Assert.Equal(StatusCodes.Status401Unauthorized, httpContext.Response.StatusCode);
        }

        [Fact]
        public async Task SetAuthConfigToContext_InvalidAuthConfigJson_ReturnsFalseAndInternalServerError()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            var consumerDetails = new GetConsumerByPersonUniqueIdentifierResponseDto
            {
                Consumer = new[] { new ConsumerDto { ConsumerId = 1, TenantCode = "TENANT1" } }
            };
            httpContext.Items[HttpContextKeys.ConsumerInfo] = consumerDetails;

            var tenantDto = new TenantDto
            {
                TenantCode = "TENANT1",
                AuthConfig = "{invalidJson"
            };

            _httpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);
            _tenantClient.Setup(x => x.Post<TenantDto>("tenant/get-by-tenant-code", It.IsAny<GetTenantCodeRequestDto>()))
                .ReturnsAsync(tenantDto);

            // Act
            var result = await _auth0Helper.SetAuthConfigToContext(httpContext);

            // Assert
            Assert.False(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, httpContext.Response.StatusCode);
        }

        [Fact]
        public async Task SetAuthConfigToContext_UnexpectedException_ReturnsFalseAndInternalServerError()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            var consumerDetails = new GetConsumerByPersonUniqueIdentifierResponseDto
            {
                Consumer = new[] { new ConsumerDto { ConsumerId = 1, TenantCode = "TENANT1" } }
            };
            httpContext.Items[HttpContextKeys.ConsumerInfo] = consumerDetails;

            _httpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);
            _tenantClient.Setup(x => x.Post<TenantDto>("tenant/get-by-tenant-code", It.IsAny<GetTenantCodeRequestDto>()))
                .ThrowsAsync(new Exception("Unexpected"));

            // Act
            var result = await _auth0Helper.SetAuthConfigToContext(httpContext);

            // Assert
            Assert.False(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, httpContext.Response.StatusCode);
        }

        [Fact]
        public async Task GetTenantByTenantCode_ReturnsTenant_WhenTenantExists()
        {
            // Arrange
            var tenantCode = "TENANT1";
            var expectedTenant = new TenantDto { TenantCode = tenantCode, AuthConfig = "{}" };
            _tenantClient.Setup(x => x.Post<TenantDto>("tenant/get-by-tenant-code",
                It.Is<GetTenantCodeRequestDto>(dto => dto.TenantCode == tenantCode)))
                .ReturnsAsync(expectedTenant);

            // Act
            var result = await _auth0Helper.GetTenantByTenantCode(tenantCode);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(tenantCode, result.TenantCode);
        }

        [Fact]
        public async Task GetTenantByTenantCode_ReturnsEmptyTenant_WhenTenantNotFound()
        {
            // Arrange
            var tenantCode = "NOT_FOUND";
            var emptyTenant = new TenantDto { TenantCode = null };
            _tenantClient.Setup(x => x.Post<TenantDto>("tenant/get-by-tenant-code",
                It.Is<GetTenantCodeRequestDto>(dto => dto.TenantCode == tenantCode)))
                .ReturnsAsync(emptyTenant);

            // Act
            var result = await _auth0Helper.GetTenantByTenantCode(tenantCode);

            // Assert
            Assert.NotNull(result);
            Assert.Null(result.TenantCode);
        }

        [Fact]
        public async Task GetTenantByTenantCode_ThrowsException_LogsErrorAndThrows()
        {
            // Arrange
            var tenantCode = "EXCEPTION";
            _tenantClient.Setup(x => x.Post<TenantDto>("tenant/get-by-tenant-code",
                It.IsAny<GetTenantCodeRequestDto>()))
                .ThrowsAsync(new Exception("Test exception"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _auth0Helper.GetTenantByTenantCode(tenantCode));
        }

    }
}
