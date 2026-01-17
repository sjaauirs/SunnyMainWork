using System.Text.Json.Serialization;

namespace SunnyRewards.Helios.ETL.Core.Domain.Dtos.Json
{
    public class NotificationContextConfig
    {
        /// <summary>
        /// The type of context (e.g., TASK, COHORT, SYSTEM).
        /// </summary>
        [JsonPropertyName("contextType")]
        public string ContextType { get; set; } = string.Empty;

        /// <summary>
        /// The ID associated with the context.
        /// </summary>
        [JsonPropertyName("contextId")]
        public string ContextId { get; set; } = string.Empty;

        /// <summary>
        /// Additional attributes in key-value format.
        /// </summary>
        [JsonPropertyName("contextAttributes")]
        public Dictionary<string, string> ContextAttributes { get; set; } = new();
    }
}
