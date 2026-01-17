namespace SunnyRewards.Helios.Wallet.Core.Domain.Constants
{
    public static class WalletConstants
    {
        public const string RewardWalletName = "TENANT_MASTER_REWARD";
        public const string SweepstakesEntriesWalletName = "TENANT_MASTER_SWEEPSTAKES_ENTRIES";
        public const string RedemptionHSAWalletName = "TENANT_MASTER_REDEMPTION:HSA";
        public const string RedemptionPrizeOutWalletName = "TENANT_MASTER_REDEMPTION:PRIZEOUT";
        public const string RedemptionSuspenseWalletName = "TENANT_MASTER_REDEMPTION:SUSPENSE_WALLET_REWARD";
        public const string SweepstakesEntriesRedemptionWalletName = "TENANT_MASTER_REDEMPTION:SWEEPSTAKES_ENTRIES";

        public const string CreateUser = "SYSTEM";
        public const string TenantMaster = "TENANT_MASTER_";
        public const string TenantMasterRedemeption = "TENANT_MASTER_REDEMPTION:SUSPENSE_WALLET_";

        public const string CsaAdjustment = "CSA Adjustment";
        public const string Benifits = "BENEFITS";
        public const string Redemption = "REDEMPTION";
        public const string RewardSuspenseWalletName = "TENANT_MASTER_REDEMPTION:SUSPENSE_WALLET_REWARD";

        public const int RetryMinWaitMS = 10; // min amount of milliseconds to wait before retrying
        public const int RetryMaxWaitMS = 101; // max amount of milliseconds to wait before retrying
        public const int MaxTries = 3;

        public const string SweepsTakes_walletTypeCode = "wat-c3b091232e974f98aeceb495d2a9f916";
        public const string ImportUser = "IMPORT_USER";
        public const string DefaultWalletTypeTransferRuleConfig = "{\"TransferRatio\":1}";

        public static class Apps
        {
            public const string Rewards = "REWARDS";
            public const string Benefits = "BENEFITS";
        }
    }

}
