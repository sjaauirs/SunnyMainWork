using System.Text.Json.Serialization;

namespace Sunny.Benefits.Bff.Core.Domain.Dtos
{
    public class StepConfigDto
    {
        [JsonPropertyName("skip_steps")]
        public bool SkipSteps { get; set; } = false;

        [JsonPropertyName("connected_component")]
        public List<int>? ConnectedComponent { get; set; }

        [JsonPropertyName("supression_condition")]
        public Dictionary<string, List<Condition>>? SupressionCondition { get; set; }
    }

    public class Condition
    {
        [JsonPropertyName("attribute_name")]
        public string? AttributeName { get; set; }
        [JsonPropertyName("operator")]
        public string? Operator { get; set; }
        [JsonPropertyName("data_type")]
        public string? DataType { get; set; }
        [JsonPropertyName("attribute_value")]
        public string? AttributeValue { get; set; }
        [JsonPropertyName("criteria")]
        public string? Criteria { get; set; }
    }

}