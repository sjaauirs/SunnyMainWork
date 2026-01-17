namespace SunnyRewards.Helios.ETL.Core.Domain.Constants
{
    public static class AdminConstants
    {
        public const string GetTenant = "tenant";
        public const string TenantAccount = "tenant-account";
        public const string GetTenantSponsorCustomer = "tenant-sponsor-customer";
        public const string CreateTenantMasterWalletsAPIUrl = "wallet/create-tenant-master-wallets";
        public const string GetConsumersByTenantCode = "tenant/consumers";
        public const string ConsumerCodesAll = "ALL";
        public const int GetConsumersByTenantCodePageSize = 50;

        public const string BenefitWalletType = "BenefitWalletType";
        public const string BenefitPurseWalletType = "BenefitPurseWalletType";
        public const string Reward = "Reward";
        public const string SweepstakesReward = "SweepstakesReward";
        public const string MembershipDollars = "MEMBERSHIP_DOLLARS";
        public const string WalletRewards = "REWARDS";
        public const string WalletSweepstakesReward = "SWEEPSTAKES_REWARD";
        public const string RewardWalletTypeCode = "Reward_Wallet_Type_Code";
        public const string SweepstakesEntriesWalletTypeCode = "Sweepstakes_Entries_Wallet_Type_Code";
        public const string MembershipDollarsWalletTypeCode = "Membership_Dollars_Wallet_Type_Code";
        public const string GetWalletTypeByCode = "wallet-type/wallet-type-code";
        public const string GetAllConsumerWallets = "consumer-wallet/get-all-consumer-wallets";
        public const string GetConsumerAccount = "admin/get-consumer-account";
        public const string ConsumerAccountAPIUrl = "consumer-account";

        public const string GetConsumerByMemberNumber= "consumer/get-consumer-by-mem-nbr";
        public const string GetConsumerWalletByWalletType = "consumer-wallet/consumer-wallet-by-wallet-type";
        public const string PostConsumerWallets = "post-consumer-wallets";
        public const string GetConsumer = "consumer/get-consumer";
        public const string ConsumerRoleAsOwner = "O";
        public const string ConsumerRoleAsContributor = "C";
        public const string Completed = "COMPLETED";
        public const string Enrolled = "IN_PROGRESS";
        public const string GetAllTenantsAPIUrl = "tenant/tenants";
        public const string AddCohortToConsumer = "add-cohort-consumer";
        public const string RemoveCohortToConsumer = "remove-cohort-consumer";

        public const double MembershipWalletEarnMax = 500;
        public static class Apps
        {
            public const string Rewards = "REWARDS";
            public const string Benefits = "BENEFITS";
        }

        public const string ConsumerHistoryEvent = "CONSUMER_HISTORY";
        public const string ConsumerHistoryEventSubType = "UPDATE";
        public const string MemberImportEvent = "MEMBER_IMPORT";
        public const string MemberImportEventSubType = "CREATE";
        public const string MemberImportEventTopicName = "AWS_MEMBER_IMPORT_SNS_TOPIC_NAME";
        public const string CohortingEvent = "COHORTING_EVENT";
        public const string CohortingEventSubType = "COHORT";
        public const string CohortingEventTopicName = "AWS_COHORTING_EVENT_SNS_TOPIC_NAME";
    }
}
