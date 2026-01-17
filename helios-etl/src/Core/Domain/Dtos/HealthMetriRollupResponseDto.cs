using SunnyRewards.Helios.Common.Core.Domain.Dtos;

namespace SunnyRewards.Helios.ETL.Core.Domain.Dtos
{
    public class HealthMetriRollupResponseDto
    {
        public HealthMetricRollUpDto? HealthMetricRollUpData { get; set; }
    }

    public class HealthMetricRollUpDto : BaseDto
    {
        public long HealthMetricRollupId { get; set; }
        public long RollupPeriodTypeId { get; set; }
        public string? TenantCode { get; set; }
        public string? ConsumerCode { get; set; }
        public DateTime RollupPeriodStartTs { get; set; }
        public DateTime RollupPeriodEndTs { get; set; }
        public RollupDataDto? RollupData { get; set; }
    }

    public class RollupDataDto
    {
        public int TotalSteps { get; set; }
        public double TotalDistanceMiles { get; set; }
        public double AverageHeartRate { get; set; }
        public int HeartRateDenom { get; set; }
        public double HeartRateAvg { get; set; }
        public int NumDays { get; set; }
        public SleepTrackingDto? SleepTracking { get; set; }
    }

    public class SleepTrackingDto
    {
        public int MinSleepDuration { get; set; }
        public int NumDaysAtOrAboveMinDuration { get; set; }
    }
}
