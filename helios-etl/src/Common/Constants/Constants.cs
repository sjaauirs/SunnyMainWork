namespace SunnyRewards.Helios.ETL.Common.Constants
{
    public static class Constants
    {
        public const int RecentTransactionCount = 3; // this is used in ConsumerSummaryService.GetRecentTransaction() of BFF API
        public const string Addition = "A";
        public const string Subtract = "S";
        public const string Insert = "I";
        public const string Update = "U";
        public const string Delete = "D";
        public const string Cancel = "C";
        public const string ALL = "ALL";
        public const string Cancel1 = "Cancel";
        public const string Complete = "COMPLETE";
        public const string COMPLETED = "COMPLETED";
        public const string InProgress = "IN_PROGRESS";
        public const string Reverted = "REVERTED";
        public const string CreateUser = "SYSTEM";
        public const string UpdateUser = "ETL";
        public const string Adjustment = "ADJUSTMENT";

        public const string ScheduleRecurrenceType = "SCHEDULE";
        public const string PeriodicRecurrenceType = "PERIODIC";
        public const string MonthlyPeriodType = "MONTH";
        public const string QuarterlyPeriodType = "QUARTER";

        public const string HealthCriteriaType = "HEALTH";

        public const string RedemptionVendorCodeHsa = "HSA";
        public const string RedemptionItemDescriptionForHsa = "Redeeming for HSA transfer";
        public const int OaepSHA256PaddingOverhead = 66;
        public const int RsaKeySize = 4096;
        public const int RsaSignatureLength = 512;
        public const int DefaultBatchSize = 5;
        public const int MemberDataBatchSize = 20;

        public const int StartDate = 01;
        public const string RedemptionTransactionDetailType = "REDEMPTION";
        public const string CreateUserAsETL = "ETL";

        public const string XApiKeySecret = "X_API_KEY";
        public const string XApiKey = "X-API-KEY";
        public const string XApiSessionKey = "X-API-SESSION-KEY";

        // Urls
        public const string Token = "token";
        public const string DatafeedInsertMembers = "data-feed/members";
        public const string DatafeedUpdateMembers = "data-feed/update-members";
        public const string DatafeedDeleteMembers = "data-feed/delete-members";
        public const string DatafeedCancelMembers = "data-feed/cancel-members";
        public const string DataFeedTaskUpdateAPIUrl = "data-feed/member-task-updates";
        public const string RetailProductUpdate = "products";
        public const string JobReport = "job-report";
        public const string JobDetailReport = "detail-report";

        public const string Started = "Started";
        public const string Processing = "Processing";
        public const string Request = "Request";
        public const string Failed = "Failed";
        public const string Error = "Error";
        public const string Ended = "Ended";
        public const string TaskStatusCompleted = "COMPLETED";
        public const string TaskStatusInProgress = "IN_PROGRESS";
        public const string Invalid = "Invalid";
        public const string TenantCode = "Tenant Code";
        public const string WalletTypeCode = "Wallet Type Code";
        public const string Wallet = "Wallet";
        public const string ConsumerWallet = "Consumer Wallet";
        public const string WalletCode = "Wallet Code";
        public const string Consumers = "Consumers";
        public const string ConsumerCode = "Consumer Code";
        public const string PersonId = "Person Id";
        public const string No = "No";
        public const string ForTheGiven = "For The Given";
        public const string Found = "Found";
        public const string MediaType = "Media Type";
        public const string Outbound = "outbound";

        public const string Purchase = "Purchase";
        public const string yyyyMMdd_HHmmss = "yyyyMMdd_HHmmss";
        public const string txtFileExtension = ".txt";
        public const string Unknown = "UNKNOWN";
        public const string TextByCsv = "text/csv";

        public const string RedemptionStatusCompleted = "COMPLETED";

        public static readonly Dictionary<string, string> TxnTypeMapping = new Dictionary<string, string>
        {

            {"ValueLoad", "FUND"},
            {"Reversal", "RETURN"},
            {"ChargeBack", "RETURN"},
            {"Returns", "RETURN"},
            {"BalancedFee", "REDEMPTION"},
            {"Fees", "REDEMPTION"},
            {"Purchase", "REDEMPTION"},
            {"Adjustment", "ADJUSTMENT"},
            {"Fee WCS", "REDEMPTION"}
        };

        public static readonly string[] SkipTxnType = { "Decline", "Non-Mon Update" };
        public static readonly int skipTransactionCurrencyCode = 840;
        public const string FIS_OUTBOUND_FOLDER = "FIS/Batch/outbound";

        // Folder Names
        public const string INCOMING_FOLDER = "incoming";
        public const string PROCESSING_FOLDER = "processing";
        public const string ARCHIVE_FOLDER = "archive";
        public const string TASK_UPDATE_CUSTOM_FORMAT = "1";
        public const string ASSIGNMENT_STATUS_COMPLETED = "COMPLETED";
        public const string HSA_INBOUND_FOLDER = "hsa/inbound";
        public const string HSA_OUTBOUND_FOLDER = "hsa/outbound";
        public const string WALLET_BALANCES_FOLDER = "reports/wallet-balances";

        // Apps
        public const string Rewards = "REWARDS";
        public const string Benefits = "BENEFITS";

        //SweepsTakes
        public const string SWEEPSTAKES_INBOUND_FOLDER = "inbound";
        public const string SWEEPSTAKES_INBOUND_ARCHIVE_FOLDER = "inbound/archive/";
        public static readonly string[] SWEEPSTAKES_FORMAT = { "realtime-media" };
        public const string SWEEPSTAKES_CONSUME_FILENAME = "rtm_sweepstakes_winners_report_";

        // Media types
        public const string RealtimeMedia = "realtime-media";

        //DynamoDb constants
        public const string DYNAMODB_COSTCO_SQS_TABLE_MESSAGEID = "messageId";
        public const string DYNAMODB_COSTCO_SQS_TABLE_MESSAGEBODY = "messageBody";
        public const string DYNAMODB_COSTCO_SQS_TABLE_PROCESSEDMESSAGEBODY = "processedMessageBody";
        public const string DYNAMODB_COSTCO_SQS_TABLE_EPOCHTS = "epochTs";
        public const string DYNAMODB_BETWEEN_MIN_MAX_VALUE_CONDITION = "BETWEEN :minVal AND :maxVal";
        public const string DYNAMODB_MIN_VALUE_KEY_NAME = ":minVal";
        public const string DYNAMODB_MAX_VALUE_KEY_NAME = ":maxVal";

        //sweepstakes instance status
        public const string SWEEPSTAKES_ENTRIES_REPORT_STARTED_STATUS = "SWEEPSTAKES_ENTRIES_REPORT_STARTED";
        public const string SWEEPSTAKES_ENTRIES_REPORT_INPROGRESS_STATUS = "SWEEPSTAKES_ENTRIES_REPORT_INPROGRESS";
        public const string SWEEPSTAKES_ENTRIES_REPORT_SUCCESS_STATUS = "SWEEPSTAKES_ENTRIES_REPORT_SUCCESS";
        public const string SWEEPSTAKES_ENTRIES_REPORT_ERROR_STATUS = "SWEEPSTAKES_ENTRIES_REPORT_ERROR";
        public const string SWEEPSTAKES_ENTRIES_REPORT_DUPLICATE_STATUS = "SWEEPSTAKES_ENTRIES_REPORT_DUPLICATE";

        public const string SWEEPSTAKES_WINNERS_REPORT_INPROGRESS_STATUS = "SWEEPSTAKES_WINNERS_REPORT_INPROGRESS";
        public const string SWEEPSTAKES_WINNERS_REPORT_SUCCESS_STATUS = "SWEEPSTAKES_WINNERS_REPORT_SUCCESS";
        public const string SWEEPSTAKES_WINNERS_REPORT_ERROR_STATUS = "SWEEPSTAKES_WINNERS_REPORT_ERROR";
        public const string INVALID = "Invalid";
        public const string VALID = "Valid";
        public const string ADD = "Add";
        public const string UPDATE = "Update";
        public const string DELETE = "Delete";
        public const string CANCEL = "Cancel";
        public const string TOTAL = "Total";
        public const string MemberImportFileType = "MemberEligibility";
        public const string SWEEPSTAKES_WINNER_TASK_PRIZE_TYPE = "TASK_REWARD";

        //Job history status
        public const string JOB_HISTORY_STARTED_STATUS = "STARTED";
        public const string JOB_HISTORY_SUCCESS_STATUS = "SUCCESS";
        public const string JOB_HISTORY_PARTIAL_SUCCESS_STATUS = "PARTIAL SUCCESS";
        public const string JOB_HISTORY_FAILURE_STATUS = "FAILURE";

        public const string DUMMY_CUSTOMER_PREFIX = "DUMMY_CUSTOMER";
        public static string DUMMY_SPONSOR_PREFIX = "DUMMY_SPONSOR";
        public static string DUMMY_TENANT_PREFIX = "DUMMY_TENANT";
        public static string DUMMY_JOB_ID_PREFIX = "ETL_STARTED";
        public static string JOB_HISTORY_ID_PREFIX = "jhi";
        public static string DEFAULT_SCHEDULE_TIME = "08:30";
        public static string DEFAULT_JOB_HISTORY_ID = "DEFAULT";
        public static string RecurringTaskApi = "get-available-recurring-tasks";
        public static string CreateConsumerTask = "admin/consumer-task";

        public static string DefaultPlan = "defaultPlan";

        public const string Encrypt = "Encrypt";
        public const string Decrypt = "Decrypt";

        public const string RedShiftConnectionStringKey= "RedShiftConnectionString";
        public const string PostgresConnectionStringKey = "SRConnectionString";

        public const string RedshiftSyncMemberImportDataType = "MemberImport";
        
        public const string SunnyPrivateKeyPassphrase = "SUNNY_PRIVATE_KEY_PASS_PHRASE";
        public const string SunnyPrivateKey = "SUNNY_PRIVATE_KEY";
        public const string TenantSpecificPublicKey = "TENANT_PUBLIC_KEY";
        public const string SftpHost = "SFTP_HOST";
        public const string SftpPort = "SFTP_PORT";
        public const string SftpUserName = "SFTP_USER_NAME";
        public const string SftpPrivateKey = "SFTP_PRIVATE_KEY";
        public const string SftpRemoteDirectory = "SFTP_REMOTE_DIRECTORY";
        public const string SftpPrivateKeyPassphrase = "SFTP_PRIVATE_KEY_PASSPHRASE";
        public const int DefaultCacheDurationForMemberImportFile = 1;        
        public const string HealthyLivingRedumtionVendorCode = "SUSPENSE_WALLET_Healthy Living Suspense";
        public const string ENROLLMENT_STATUS_ACTIVE = "ENROLLED";

        public const string HealthTaskTypeStep = "STEP";
        public const string HealthTaskTypeSleep = "SLEEP";
        public const string HealthTaskTypeHydration = "HYDRATION";
        public const string HealthTaskTypeOther = "OTHER";
        public const string HealthTaskTypeWalk = "WALK";
        public const string DateFormat = "yyyyMMddHHmmss";
        public const string txtExtention = "txt";

        public const string FreezeCardOperation = "FREEZE";
        public const string BenefitsCardOperation = "fis/card-operation";
    }
}