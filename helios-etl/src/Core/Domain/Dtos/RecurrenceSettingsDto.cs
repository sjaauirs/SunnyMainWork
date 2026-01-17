using Newtonsoft.Json;

namespace SunnyRewards.Helios.ETL.Core.Domain.Dtos
{
    public class RecurrenceSettingsDto
    {
        [JsonProperty("recurrenceType", NullValueHandling = NullValueHandling.Ignore)]
        public string? RecurrenceType { get; set; }

        [JsonProperty("periodic", NullValueHandling = NullValueHandling.Ignore)]
        public PeriodicSettings? Periodic { get; set; }

        [JsonProperty("schedules", NullValueHandling = NullValueHandling.Ignore)]
        public ScheduleSettings[]? Schedules { get; set; }
    }

    public class PeriodicSettings
    {
        [JsonProperty("period", NullValueHandling = NullValueHandling.Ignore)]
        public string? Period { get; set; }

        [JsonProperty("periodRestartDate", NullValueHandling = NullValueHandling.Ignore)]
        public int PeriodRestartDate { get; set; }
    }

    public class ScheduleSettings
    {
        [JsonProperty("startDate", NullValueHandling = NullValueHandling.Ignore)]
        public string? StartDate { get; set; }

        [JsonProperty("expiryDate", NullValueHandling = NullValueHandling.Ignore)]
        public string? ExpiryDate { get; set; }
    }
}
