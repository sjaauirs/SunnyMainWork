using Google.Apis.Http;
using SunnyRewards.Helios.Admin.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Admin.Core.Domain.Constants
{
    public static class Constant
    {
        public const string SweepstakesInstanceUrl = "sweepstakes/create-sweepstakes-instance";
        public const string CohortExportAPIUrl = "cohort/cohort-export";
        public const string TaskExportAPIUrl = "task/task-export";
        public const string AdventureExportAPIUrl = "export-adventures";
        public const string TaskRewardCollectionExportAPIUrl = "export-taskreward-collection";
        public const string WalletTypeTransferRuleExportAPIUrl = "export-walletType-transfer-rule";
        public const string WalletTypeTransferRuleImportAPIUrl = "import-walletType-transfer-rule";
        public const string CmsExportAPIUrl = "cms/cms-export";
        public const string GetTenantAccountAPIUrl = "tenant-account-export";
        public const string GetTenantProgramConfigAPIUrl = "export-tenant-program-config";
        public const string SweepstakesExportAPIUrl = "sweepstakes/sweepstakes-export";
        public const string CreateTenantAPIUrl = "tenant";
        public const string CreateTenantAcountAPIUrl = "create-tenant-account";
        public const string CreateTenantMasterWalletsAPIUrl = "wallet/create-tenant-master-wallets";
        public const string CreateCohortAPIUrl = "cohort";
        public const string CreateCohortTenantTaskRewardAPIUrl = "cohort-tenant-task-reward";
        public const string GetCohortTenantTaskRewardAPIUrl = "cohort-tenant-task-reward/cohort-tenant-task-reward";
        public const string GetTasksAndTaskRewardsAPIUrl = "get-task-rewards";
        public const string TaskCategoriesApiUrl = "task-categories";
        public const string TaskApiUrl = "task";
        public const string TasksApiUrl = "tasks";
        public const string TaskDetailApiUrl = "task-detail";
        public const string TaskRewardApiUrl = "task-reward";
        public const string TaskRewardDetailsApiUrl = "task-reward-details";
        public const string HealthTaskRewardsApiUrl = "health-task-rewards";
        public const string TaskRewardTypesApiUrl = "task-reward-types";
        public const string TaskTypesApiUrl = "task-types";
        public const string ImportTaskApiUrl = "import-task";
        public const string ImportTriviaApiUrl = "import-trivia";
        public const string ImportQuestionnaireApiUrl = "import-questionnaire";
        public const string ImportTenantProgramApiUrl = "import-tenant-program-config";
        public const string ImportCMSApiUrl = "cms/import-cms";
        public const string GetCmsComponents = "component/get-components";
        public const string ImportSweepstakesApiUrl = "import-sweepstakes";
        public const string ImportCohortApiUrl = "import-cohort";
        public const string ImportTaskRewardCollectionApiUrl = "import-task-reward-collection";
        public const string ImportAdventuresAndTenantAdventures = "import-adventures";
        public const string ImportUser = "IMPORT_USER";
        public const string SpanishCode = "es";
        public const string CollectionComponentTypeCode = "cty-39f775083eb0499cb3413a53238c5a05";
        public static readonly string[] validOptions = new[] { "COHORT", "TASK", "TRIVIA", "CMS", "FIS", "SWEEPSTAKES", "WALLET", "ADMIN", "QUESTIONNAIRE" };
        public static string ImportTaskTypes = "import-task-types";
        public static string ImportTaskCategories = "import-task-categories";
        public static string ImportRewardTypes = "import-reward-types";


        public const string TenantExportFolderName = "tenant-export";
        public const string ExportJsonFilesFolderName = "json-files";
        public const string CreateTaskAPIUrl = "task";
        public const string GetTaskAPIUrl = "task/get-task-by-task-name";
        public const string CreateTaskExternalMappingRequest = "task/task-external-mapping";
        public const string CreateTriviaRequest = "trivia/trivia";
        public const string CreateTriviaQuestionRequest = "trivia/trivia-question";
        public const string CreateTriviaQuestionGroupRequest = "trivia/trivia-question-group";
        public const string CreateSubTaskAPIUrl = "subtask";
        public const string CreateTenantTaskCategoryUrl = "tenant-task-category";
        public const string CreateTaskDetailsUrl = "task-detail";
        public const string CreateTaskRewardUrl = "task-reward";
        public const string CreateTermsOfServiceUrl = "terms-of-service";
        public const string CreateComponentUrl = "component";
        public const string GetAllComponentTypes = "component/component-type";
        public const string CreateSweepStakesUrl = "sweepstakes";
        public const string CreateTenantSweepStakesUrl = "tenantsweepstakes";
        public const string GetAllComponents = "component/get-all-components";
        public const string RemoveConsumer = "remove-consumer";
        public const string AddConsumer = "add-consumer";
        public const string CreateUserAsETL = "ETL";
        public const string SweepstakesInstanceGetUrl = "sweepstakes/sweepstakes-instance";
        public const string GetAllTriviaAPIUrl = "trivia";
        public const string TriviaQuestionsAPIUrl = "trivia-questions";
        public const string TriviaQuestionGroupsAPIUrl = "trivia-question-groups";
        public const string TaskRewardDetailsAPIUrl = "task-reward-details";
        public const string ConsumerTaskAPIUrl = "consumer-task";
        public const string GetConsumerAPIUrl = "consumer/get-consumer";
        public const string GetConsumerByMemId = "consumer/get-consumer-by-memid";
        public const string GetConsumer = "consumer/get-consumer";
        public const string Tenants = "tenant/get-tenants";
        public const string Sponsors = "customer/get-sponsors";
        public const string Customers = "customer/get-customers";
        public const string Sponsor = "customer/create-sponsor";
        public const string Customer = "customer/create-customer";
        public const string UpdateTenant = "tenant";
        public const string TenantAccount = "tenant-account";
        public const string Wallet = "wallet/wallet";
        public const string WalletTypes = "wallet/wallet-types";
        public const string WalletType = "wallet/wallet-type";
        public const string WalletTypeCode = "wallet/wallet-type-code";
        public const string GetConsumerWallet = "consumer-wallet/find-consumer-wallet-by-wallet-type";
        public const string GetAllConsumerWallets = "consumer-wallet/get-all-consumer-wallets";
        public const string GetAllConsumerRedeemableWallets = "consumer-wallet/get-all-consumer-reedemable-wallets";
        public const string PostConsumerWallet = "consumer-wallet/post-consumer-wallets";
        public const string ImportWalletTypes = "wallet/import-wallet-types";
        public const string ImportComponentTypes = "cms/import-component-types";

        public const string ConsumerWallets = "wallet/get-wallets";
        public const string MasterWallet = "wallet/master-wallets";
        public const string CreateMasterWallet = "wallet/create-tenant-master-wallets";
        public const string PurseFundingAPIUrl = "purse-funding";
        public const string FundingHistoryAPIUrl = "funding-history";
        public const string FISOnBoardingRecurrenceType = "ONBOARDING";
        public const string BenefitTransactionDetailType = "BENEFIT";
        public const string RedemptionTransactionDetailType = "REDEMPTION";
        public const string RewardSuspenseWalletName = "TENANT_MASTER_REDEMPTION:SUSPENSE_WALLET_REWARD";
        public const string PersonRoles = "get-person-roles";
        public const string PersonAccessControlList = "access-control-list";
        public const string HealthMetricsAPIUrl = "health-metrics";
        public const string GetTenant = "tenant";
        public const string GetTenantSponsorCustomer = "customer/tenant-sponsor-customer";
        public const string ConsumerAccountAPIUrl = "consumer-account";
        public const string ConsumerCardsStatusAPIUrl = "fis/get-consumer-cards-status";
        public const string UpdateCohortAPIUrl = "cohort";
        public const string UpdateSweepStakesUrl = "sweepstakes";
        public const string GetTenantByTenantCode = "tenant/get-by-tenant-code";
        public const string GetAllConsumerTasks = "get-all-consumer-tasks";
        public const string UpdateCardIssueStatus = "update-card-issue-status";
        public const string GetConsumerAccount = "get-consumer-account";
        public const string GetAllTasksByTenantCode = "get-all-task-by-tenantcode";
        public const string GetAllConsumerWalletsAPIUrl = "consumer-wallet/get-all-consumer-wallets";
        public const string WalletRedeemStartAPIUrl = "wallet/redeem-start";
        public const string FISValueLoadAPIUrl = "fis/load-value";
        public const string WalletRedeemFailAPIUrl = "wallet/redeem-fail";
        public const string WalletRedeemCompleteAPIUrl = "wallet/redeem-complete";
        public const string Currency_USD = "USD";
        public const int MaxTries_Count = 3;
        public const string TenantMasterRedemptionWalletPrefix = "TENANT_MASTER_REDEMPTION:";



        // Dispose Csa Transaction
        public const string FisCsatransaction = "dispose-csa-transaction";
        public const string WalletTransactions = "transaction/csa-value-load-transactions";
        public const string GetTenantAccount = "get-tenant-account";

        public const string MerchantNameForFundTransfer = "Transfer from Rewards";
        public const string MerchantNameForInitialValueLoad = "Initial Value Load";

        //transactions
        public const string RewardsWalletsTransactions = "transaction/rewards-wallets-transactions";

        public static class TaskStatus
        {
            public const string InProgress = "IN_PROGRESS";
            public const string Completed = "COMPLETED";
        }
        public static class Apps
        {
            public const string Rewards = "REWARDS";
            public const string Benefits = "BENEFITS";
        }

        public const string ConsumerTask = "CONSUMER_TASK";
        public const string ConsumerTaskUpdate = "CONSUMER_TASK_UPDATE";

        public const string ProxyNumberDefaultValue = "0000";

        public const string GetPlanCohortPurseMappingAPIUrl = "plan-cohort-purse-mapping";

        public const string PURSECOHORT = "PLAN_COHORT_PURSE_ASSIGNMENT";
        public const string Add = "Add";
        public const string Remove = "Remove";
        public const string DefaultPlanId = "default";

        //wallet helper Constants
        public const string ConsumerRoleAsOwner = "O";
        public const string ConsumerRoleAsContributor = "C";
        public const double MembershipWalletEarnMax = 500;

        public const string BenefitWalletType = "BenefitWalletType";
        public const string BenefitPurseWalletType = "BenefitPurseWalletType";
        public const string Reward = "Reward";
        public const string SweepstakesReward = "SweepstakesReward";
        public const string MembershipDollars = "MEMBERSHIP_DOLLARS";
        public const string WalletRewards = "REWARDS";
        public const string WalletSweepstakesReward = "SWEEPSTAKES_REWARD";
        public const double SweepsTasksWalletEarnMax = 1000000;
        public const string RewardWalletTypeCode = "Health_Actions_Reward_Wallet_Type_Code";
        public const string SweepstakesEntriesWalletTypeCode = "Sweepstakes_Entries_Wallet_Type_Code";
        public const string MembershipDollarsWalletTypeCode = "Health_Actions_Membership_Reward_Wallet_Type_Code";
        public const string GetWalletTypeByCode = "wallet-type/wallet-type-code";

       
        public const string GetConsumerWalletByWalletType = "consumer-wallet/consumer-wallet-by-wallet-type";

        public const string Benefits = "BENEFITS";
        public const string Rewards = "REWARDS";

    }
}
