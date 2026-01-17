using Newtonsoft.Json;

namespace SunnyRewards.Helios.Task.Core.Domain.Dtos
{
    public class TaskCompletionCriteriaJson
    {
        public HealthCriteria? HealthCriteria { get; set; }
        public string? CompletionPeriodType { get; set; }
        public string? CompletionCriteriaType { get; set; }
        public string? SelfReportType { get; set; }
    }
    public class HealthCriteria
    {
        public string? HealthTaskType { get; set; }
        public int RequiredSteps { get; set; }
        public int RequiredDays { get; set; }
        public int? RequiredUnits { get; set; }
        public string? UnitType { get; set; }
        public bool IsDialerRequired { get; set; } = true;
        public List<UiComponent>? UiComponent { get; set; }
        public RequiredSleep? RequiredSleep { get; set; }

    }
    public class UiComponent
    {
        public bool IsRequiredField { get; set; }=true;

        [JsonProperty("reportTypeLabel")]
        public Dictionary<string, string> ReportTypeLabel { get; set; }

        [JsonIgnore]
        public string? EnUSReportLabel => ReportTypeLabel != null && ReportTypeLabel.ContainsKey("en-US")
            ? ReportTypeLabel["en-US"]
            : null;
    }
   
    public class RequiredSleep
    {
        public int MinSleepDuration { get; set; }
        public int NumDaysAtOrAboveMinDuration { get; set; }
    }
}

