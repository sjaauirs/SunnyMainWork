using System.Text.Json.Serialization;

namespace SunnyRewards.Helios.ETL.Core.Domain.Dtos.Json
{
    public class NotificationFrequencyConfig
    {
        /// <summary>
        /// The frequency interval (e.g., DAILY, WEEKLY, MONTHLY).
        /// </summary>
        [JsonPropertyName("interval")]
        public string? Interval { get; set; } = string.Empty;

        /// <summary>
        /// The specific day associated with the frequency (e.g., for WEEKLY: 1 = Monday, for MONTHLY: day of the month).
        /// </summary>
        [JsonPropertyName("day")]
        public int? Day { get; set; }

        /// <summary>
        /// The specific date of rule execution - for ADHOC rule
        /// </summary>
        [JsonPropertyName("date")]
        public DateOnly? Date { get; set; }

        /// <summary>
        /// The specific time of rule execution
        /// </summary>
        [JsonPropertyName("schedule")]
        public TimeOnly? Schedule { get; set; }
    }
}
