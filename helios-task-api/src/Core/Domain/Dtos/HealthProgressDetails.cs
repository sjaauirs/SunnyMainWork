namespace SunnyRewards.Helios.Task.Core.Domain.Dtos
{
    public class HealthProgressDetails<T>
    {
        public string? DetailType { get; set; }
        public T? HealthProgress { get; set; }
    }

    public class ActivityRollupDataDto
    {
        public TrackingDto[] ActivityLog { get; set; } = Array.Empty<TrackingDto>();
    }

    public class StepsRollupDataDto : ActivityRollupDataDto
    {
        public int TotalSteps { get; set; }
    }

    public class TrackingDto
    {
        public DateTime TimeStamp { get; set; }
        public int UnitsAdded { get; set; }
        public string Source { get; set; }
    }

    public class SleepRollupDataDto : ActivityRollupDataDto
    {
        public SleepTrackingDto? SleepTracking { get; set; }
    }

    public class HydrationRollupDataDto : ActivityRollupDataDto
    {
        public int TotalDays { get; set; }
    }

    public class SleepTrackingDto
    {
        public int MinSleepDuration { get; set; }
        public int NumDaysAtOrAboveMinDuration { get; set; }
    }

    public class OtherHealthTaksRollupDataDto : ActivityRollupDataDto
    {
        public int TotalUnits { get; set; }
        public List<HealthTrackingDto>? HealthReport { get; set; } = new List<HealthTrackingDto>();

    }
    public class HealthTrackingDto
    {
        public List<HealthTrackingDetailDto>? HealthReportData { get; set; } = new List<HealthTrackingDetailDto>();
        public DateTime? HealthReportCompletionDate { get;set; }
    }
    public class HealthTrackingDetailDto
    {
        public string? HealthReportType { get; set; }
        public string? HealthReportValue { get; set; }
    }

}
