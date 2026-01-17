using Newtonsoft.Json;

namespace Sunny.Benefits.Bff.Core.Domain.Dtos
{
    public partial class VerificationEmailRequest
    {
        [JsonProperty("user_id")]
        public string UserId { get; set; } = string.Empty;
    }
}
