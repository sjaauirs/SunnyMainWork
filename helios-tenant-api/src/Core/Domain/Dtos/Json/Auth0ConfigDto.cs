using Newtonsoft.Json;

namespace SunnyRewards.Helios.Tenant.Core.Domain.Dtos.Json
{
    public class AuthConfig
    {
        [JsonProperty("auth0")]
        public Auth0ConfigDto? Auth0 { get; set; }
    }

    public class Auth0ConfigDto
    {
        [JsonProperty("domain")]
        public string? Domain { get; set; }

        [JsonProperty("audience")]
        public string[] Audience { get; set; } = Array.Empty<string>();

        [JsonProperty("issuer")]
        public string? Issuer { get; set; }

        [JsonProperty("grantType")]
        public string? GrantType { get; set; }

        [JsonProperty("auth0ApiUrl")]
        public string? Auth0ApiUrl { get; set; }

        [JsonProperty("auth0TokenUrl")]
        public string? Auth0TokenUrl { get; set; }

        [JsonProperty("auth0UserInfoUrl")]
        public string? Auth0UserInfoUrl { get; set; }

        [JsonProperty("auth0UserInfoByEmailUrl")]
        public string? Auth0UserInfoByEmailUrl { get; set; }

        [JsonProperty("auth0VerificationEmailUrl")]
        public string? Auth0VerificationEmailUrl { get; set; }

        [JsonProperty("jwksUrl")]
        public string? JwksUrl { get; set; }
    }
}
