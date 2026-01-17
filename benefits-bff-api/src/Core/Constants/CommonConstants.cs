namespace Sunny.Benefits.Bff.Core.Constants
{
    public static class CommonConstants
    {
        public const string Env = "env";
        public const string ALL = "ALL";
        public const string EVERYONE = "everyone";
        public const string InternalError = "Internal Error";
        public const string Unauthorized = "Unauthorized";
        public const string NotFound = "Not Found";
        public const string ContentType = "Content-Type";
        public const string ApplicationJson = "application/json";
        public const string Authorization = "Authorization";
        public const string Bearer = "Bearer";
        public const string Subscriber = "subscriber";
        public const string Accept = "Accept";
        public const int WalletTransactionCount = 4;
        public const string FISStoreTags = "FISStoreTags";
        public const string MEMBERSHIP_DOLLARS = "MEMBERSHIP_DOLLARS";
        public const string DEVELOPMENT_ENV = "Development";
        public const string Skip = "skip";

        // Api url
        public const string PostConsumerDeviceUrl = "create-consumer-device";
        public const string GetConsumerDevices = "get-consumer-devices";
        public const string ConsumerAPIUrl = "consumer";
        public const string ConsumerActivityApiUrl = "consumer-activity";
        public const string ConsumerLoginUrl = "consumer/login";
        public const string DefaultLanguageCode = "en-US";
        public const string Completed = "COMPLETED";

        public const string BenefitsApp = "BENIFITS_MOBILE_APP";
        public const string GetHealthMetrics = "get-health-metrics";
        public const string GetTenantByCodeAPIUrl = "tenant/get-by-tenant-code";
        public const string GetPersonAndConsumerAPIUrl = "person/get-details-by-consumer-code";
        public const string GetConsumerByMemId = "consumer/get-consumer-by-memid";
        public const string TermsAndCondition = "TERMS-AND-CONDITIONS";
        public const string ConsumerSummaryAPIUrl = "/api/v1/consumer-summary";
        public const string GetConsumerAccount = "get-consumer-account";
        public const string WalletsAPIUrl = "/api/v1/wallets";
        public const string VerifyMemberInfoUrl = "/api/v1/verify-member-info";
        public const string SubscriptionAPIUrl = "/api/v1/consumer/consumer-subscription-status";
        public const string CardOperationUrl = "/api/v1/fis/card-operation";
        public const string CardOperationReissueUrl = "/api/v1/fis/reissue-card";
        public const string CardStatusUrl = "/api/v1/fis/card-status";
        public const string GetCurrentFlowStatusAPIUrl = "/api/v1/flows/get-user-flow-status";
        public const string GetFlowStepsAPIUrl = "/api/v1/flows/steps";

        public const string ValidicToken = "VALIDIC_ACCESS_TOKEN";
        public const string ValidicOrgId = "VALIDIC_ORG_ID";
        public const string UpdateEnrollmentStatusAPIUrl = "consumer/update-enrollment-status";
        public const string UpdateOnboardingFlowStatusAPIUrl = "/api/v1/flows/update-flow-status";

        public const string Auth0ClientSecretKeyName = "AUTH0_CLIENT_SECRET";
        public const string Auth0ClienIdKeyName = "AUTH0_CLIENT_ID";

        public const string purseComponent = "PURSE_DETAILS";
        public const string ConsumerAttributersUrl = "consumer/consumer-attributes";
        public const string ConsumerSubscriptionStatusUrl = "consumer/consumer-subscription-status";

    }
}