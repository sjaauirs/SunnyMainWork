namespace SunnyRewards.Helios.ETL.Core.Domain.Dtos
{
    public class NotificationRuleProcessedContextDto
    {
        /// <summary>
        /// The type of context (e.g., TASK, COHORT, SYSTEM).
        /// </summary>
        public string? ContextType { get; set; } = string.Empty;

        /// <summary>
        /// The ID associated with the context.
        /// </summary>
        public string? ContextId { get; set; } = string.Empty;

        /// <summary>
        /// The number of records processed for this context.
        /// </summary>
        public int? RecordsCount { get; set; }

        /// <summary>
        /// The timestamp when the processing occurred.
        /// </summary>
        public DateTime? ProcessedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Additional attributes specific to the context, stored in key-value format.
        /// </summary>
        public Dictionary<string, string>? ContextAttributes { get; set; } = new();
    }
}
