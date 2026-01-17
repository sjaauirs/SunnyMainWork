using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace SunnyRewards.Helios.User.Core.Domain.Dtos
{
    public class PostEventRequestDto
    {
        [Required]
        public string? EventType { get; set; } 

        [Required]
        public string? EventSubtype { get; set; }

        [Required]
        public string? EventSource { get; set; }

        [Required]
        public string? TenantCode { get; set; }

        [Required]
        public string? ConsumerCode { get; set; }

        [JsonConverter(typeof(DynamicConvertor))]
        public dynamic? EventData { get; set; }
    }

    public class DynamicConvertor : JsonConverter<dynamic>
    {
        public override dynamic Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            using JsonDocument jsonDocument = JsonDocument.ParseValue(ref reader);
            return jsonDocument.RootElement.GetRawText();
        }

        public override void Write(Utf8JsonWriter writer, dynamic value, JsonSerializerOptions options)
        {
            using JsonDocument jsonDocument = (JsonDocument)JsonDocument.Parse(value);
            jsonDocument.RootElement.WriteTo(writer);
        }
    }
}
