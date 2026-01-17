using System.Text.Json.Serialization;

namespace Sunny.Benefits.Bff.Core.Domain.Dtos
{
    public class Auth0AppMetadataDto
    {
        [JsonPropertyName("consumerCode")]
        public string? ConsumerCode { get; set; }

        [JsonPropertyName("env")]
        public string? Env { get; set; }

        [JsonPropertyName("role")]
        public string? Role { get; set; }

        [JsonPropertyName("tenantCode")]
        public string? TenantCode { get; set; }

        [JsonPropertyName("postalCode")]
        public string? PostalCode { get; set; }

        [JsonPropertyName("isSSOUser")]
        public bool IsSSOUser { get; set; } = false;

        [JsonPropertyName("personUniqueIdentifier")]
        public string? PersonUniqueIdentifier { get; set; }

        [JsonPropertyName("userName")]
        public string? UserName { get; set; }
        [JsonPropertyName("memberNbr")]
        public string? MemberNbr { get; set; }
        [JsonPropertyName("regionCode")]
        public string? RegionCode { get; set; }
    }
}
