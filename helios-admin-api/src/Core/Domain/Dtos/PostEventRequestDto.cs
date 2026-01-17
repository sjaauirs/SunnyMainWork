using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SunnyRewards.Helios.Admin.Core.Domain.Dtos
{
    public class PostEventRequestDto
    {
        [Required]
        public string EventType { get; set; } = null!;
        [Required]
        public string EventSubtype { get; set; } = null!;
        [Required]
        public string EventSource { get; set; } = null!;

        [Required]
        public string TenantCode { get; set; } = null!;

        [Required]
        public string ConsumerCode { get; set; } = null!;

        [JsonConverter(typeof(DynamicConvertor))]
        public dynamic? EventData { get; set; }

    }
    public class DynamicConvertor : JsonConverter<dynamic>
    {
        public override dynamic Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            using (JsonDocument document = JsonDocument.ParseValue(ref reader))
            {
                var root = document.RootElement;
                return root.GetRawText();
            }
        }

        public override void Write(Utf8JsonWriter writer, dynamic value, JsonSerializerOptions options)
        {
            using (JsonDocument document = JsonDocument.Parse(value))
            {
                document.RootElement.WriteTo(writer);
            }
        }
    }
}

