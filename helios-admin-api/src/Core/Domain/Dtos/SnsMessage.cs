using System.Text.Json.Serialization;

namespace SunnyRewards.Helios.Admin.Core.Domain.Dtos
{
    public class SnsMessage<T>
    {
        [JsonPropertyName("Message")]
        public string Message { get; set; }
    }
}
