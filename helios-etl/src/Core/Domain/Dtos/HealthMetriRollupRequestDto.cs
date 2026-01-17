namespace SunnyRewards.Helios.ETL.Core.Domain.Dtos
{
    public class HealthMetriRollupRequestDto
    {
        public string? ConsumerCode { get; set; }
        public string? TenantCode { get; set; }
        public string? RollUpPeriodTypeName { get; set; }
        public string? RollUpPeriodData { get; set; }
        public int MinSleepDuration { get; set; }

    }

    public class MonthDetail
    {
        public int Year { get; set; }
        public int Month { get; set; }
    }

    public class RollUpPeriodData
    {
        public MonthDetail? MonthDetail { get; set; }
    }
}
