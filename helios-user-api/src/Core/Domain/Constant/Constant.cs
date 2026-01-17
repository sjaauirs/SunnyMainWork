namespace SunnyRewards.Helios.User.Core.Domain.Constant
{
    public static class Constant
    {
        public const string scope = "user";
        public const string PartnerCodeClaim = "partner_code";
        public const string MemberIdClaim = "member_id";
        public const string KeyIdClaim = "key_id";
        public const string EmailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
        public const string All = "All";
        public const string ConsumerCode = "consumer_code";
        public const string Exp = "exp";
        public const string Env = "env";
        public const string NotVerified = "NOT_VERIFIED";
        public const string NotStarted = "NOT_STARTED";
        public const string InProgress = "in_progress";
        public const string Completed = "completed";
        public const string Success = "success";

        // Role Codes
        public const string Admin = "rol-d2ad5b10f47940eaa1e190387433e64c";
        public const string Subscriber = "rol-46c2740cafc44869a8b1f822bf5fa712";
        public const string CustomerAdmin = "rol-938b67672b3f4dcaba54435e95dda6c5";
        public const string SponsorAdmin = "rol-2b394f0307554d99b106e8fe0518bd04";
        public const string TenantAdmin = "rol-44e107f38dda4423bf653525fb9c1938";
        public const string ReportUser = "rol-f7ca3ef923614892907094230365bb82";
        public const string PathSettings = "Paths";
        public const string AgreementFilePath = "AgreementFilePath";
        public const string AgreementurlPath = "cms/html/{tenant_code}/{language_code}/{html_fileName}";
        public const string ChromeFilePath = "ChromeFilePath";
        public const string DefaultLanguageCode = "en-US";
        // URL
        public const string GetTenantSponsorCustomer = "customer/tenant-sponsor-customer/";
        public const string GetTenantByTenantCode = "tenant/get-by-tenant-code/";

        public const string ConsumerHistoryEvent = "CONSUMER_HISTORY";
        public const string ConsumerHistoryEventSubType = "UPDATE";

        public const string AdminApp = "ADMIN";
        public const string RewardApp = "REWARD";

        public const string CohortEventType = "COHORT_EVENT";
        public const string CohortEventSubType = "COHORT_ASSIGNMENT";
        
        public const string AgreementsVerifiedEventType = "CARD_ISSUE_STATUS_UPDATE";
        public const string AgreementsVerifiedEventSubType = "AGREEMENTS_VERIFIED";
        
        public const string UserService = "UserService";
        public const string Import = "Import";
        public const int RetryMinWaitMS = 10;
        public const int RetryMaxWaitMS = 101;
        public const int RetryCount = 3;
    }
}
