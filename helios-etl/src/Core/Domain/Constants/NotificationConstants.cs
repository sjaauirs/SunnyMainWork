namespace SunnyRewards.Helios.ETL.Core.Domain.Constants
{
    public static class NotificationConstants
    {
        public const string TenantCodesAll = "ALL";
        public const string GetAllNotificationRulesAPIUrl = "notification-rules/all-rules";
        public const string GetAllNotificationRuleHistoryAPIUrl = "notification-rule-history/get-all-notification-rule-history-by-notification-rule-id";
        public const string CreateNotificationRuleHistoryAPIUrl = "notification-rule-history/create-notification-rule-history";
        public const string GetNotificationEventTypeAPIUrl = "notification-event-type";
        public const string NotificationEventTSourceModule = "NotificationETL";
        public const string GetAllCatetoriesAPIUrl = "notification-category/all-categories";

        public const int RetryMinWaitMS = 10;
        public const int RetryMaxWaitMS = 101;
        public const int MaxTries = 3;
    }
}
