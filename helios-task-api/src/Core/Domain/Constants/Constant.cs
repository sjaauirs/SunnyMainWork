namespace SunnyRewards.Helios.Task.Core.Domain.Constants
{
    public static class Constant
    {
        public const string TaskTypeName_SpinWheel = "SPINWHEEL_SUBTASK";
        public const string Month = "MONTH";
        public const string QuarterlyPeriod = "QUARTER";
        public const string LanguageCode = "en-US";
        public const string PeriodDescriptor = "UNKNOWN";
        public const string Schedule = "SCHEDULE";
        public const string SystemUser = "SYSTEM";
        public const string Periodic = "PERIODIC";
        public const string DateFormat = "yyyy-MM-dd";
        public const string HealthCriteriaType = "HEALTH";
        public const string HealthCriteriaStepsType = "STEPS";
        public const string HealthCriteriaSleepType = "SLEEP";
        public const string MonthlyPeriodType = "MONTH";
        public const string QuarterlyPeriodType = "QUARTER";
        public const string CreateUser = "SYSTEM";
        public const string ImportUser = "IMPORT_USER";
        public const string EnrolledTaskStatus = "IN_PROGRESS";
        public const string CompletedTaskStatus = "COMPLETED";
        public const string ConsumerTaskEventType = "CONSUMER_TASK";
        public const string ConsumerTaskEventSubType = "CONSUMER_TASK_UPDATE";
        public const string TaskApiSource = "task-api";
        public const string UIComponentReportType = "UI_COMPONENT";
        public const string InvalidOperation = "INVALID_OPERATION";
        public static readonly IReadOnlyDictionary<string, string> ColumnAliasMap =
      new Dictionary<string, string>
      {
        { "task_id", "t" },
        { "self_report", "tr" }
      };


    }
}
