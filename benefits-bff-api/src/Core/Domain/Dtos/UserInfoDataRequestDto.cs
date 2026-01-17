using System.Text.Json.Serialization;

namespace Sunny.Benefits.Bff.Core.Domain.Dtos
{
    public class UserInfoDataRequestDto
    {
        [JsonPropertyName("app_metadata")]
        public AppMetadata app_metadata { get; set; } = new AppMetadata();

        [JsonPropertyName("name")]
        public string? name { get; set; }
    }

    public class AppMetadata
    {
        [JsonPropertyName("consumer_code")]
        public string ConsumerCode { get; set; } = string.Empty;

        [JsonPropertyName("env")]
        public string Env { get; set; } = string.Empty;

        [JsonPropertyName("role")]
        public string Role { get; set; } = string.Empty;

        [JsonPropertyName("tenant_code")]
        public string TenantCode { get; set; } = string.Empty;

        [JsonPropertyName("postal_code")]
        public string PostalCode { get; set; } = string.Empty;

        [JsonPropertyName("personUniqueIdentifier")]
        public string? PersonUniqueIdentifier { get; set; }

        [JsonPropertyName("isSSOUser")]
        public bool IsSSOUser { get; set; } = false;

        [JsonPropertyName("memberId")]
        public string? MemberId { get; set; }
    }
}
