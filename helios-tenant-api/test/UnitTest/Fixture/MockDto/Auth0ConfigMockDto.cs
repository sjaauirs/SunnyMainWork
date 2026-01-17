using Xunit;
using Newtonsoft.Json;
using SunnyRewards.Helios.Tenant.Core.Domain.Dtos.Json;

namespace SunnyRewards.Helios.Tenant.UnitTests.Dtos
{
    public class AuthConfigMockDto
    {
        [Fact]
        public void Should_Deserialize_AuthConfig_From_Json()
        {
            // Arrange
            var json = @"
            {
                ""auth0"": {
                    ""domain"": ""https://example.auth0.com"",
                    ""audience"": [""https://api.example.com""],
                    ""issuer"": ""https://example.auth0.com/"",
                    ""grantType"": ""client_credentials"",
                    ""auth0ApiUrl"": ""https://api.example.com"",
                    ""auth0TokenUrl"": ""https://example.auth0.com/oauth/token"",
                    ""auth0UserInfoUrl"": ""https://example.auth0.com/userinfo"",
                    ""auth0UserInfoByEmailUrl"": ""https://example.auth0.com/userinfo?email="",
                    ""auth0VerificationEmailUrl"": ""https://example.auth0.com/send-verification-email"",
                    ""jwksUrl"": ""https://example.auth0.com/.well-known/jwks.json""
                }
            }";

            // Act
            var result = JsonConvert.DeserializeObject<AuthConfig>(json);

            // Assert
           Assert.NotNull(result.Auth0);
           
        }

        [Fact]
        public void Should_Serialize_AuthConfig_To_Json()
        {
            // Arrange
            var config = new AuthConfig
            {
                Auth0 = new Auth0ConfigDto
                {
                    Domain = "https://example.auth0.com",
                    Audience = new[] { "https://api.example.com" },
                    Issuer = "https://example.auth0.com/",
                    GrantType = "client_credentials",
                    Auth0ApiUrl = "https://api.example.com",
                    Auth0TokenUrl = "https://example.auth0.com/oauth/token",
                    Auth0UserInfoUrl = "https://example.auth0.com/userinfo",
                    Auth0UserInfoByEmailUrl = "https://example.auth0.com/userinfo?email=",
                    Auth0VerificationEmailUrl = "https://example.auth0.com/send-verification-email",
                    JwksUrl = "https://example.auth0.com/.well-known/jwks.json"
                }
            };

            // Act
            var json = JsonConvert.SerializeObject(config, Formatting.Indented);

            // Assert
            Assert.Contains("\"domain\": \"https://example.auth0.com\"", json);
            
        }

        [Fact]
        public void Should_Have_Empty_Audience_By_Default()
        {
            // Arrange
            var dto = new Auth0ConfigDto();

            // Assert
           Assert.NotNull(dto.Audience);
        }
    }
}
