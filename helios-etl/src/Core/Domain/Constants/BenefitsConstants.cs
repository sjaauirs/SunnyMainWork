namespace SunnyRewards.Helios.ETL.Core.Domain.Constants
{
    public static class BenefitsConstants
    {
        public const string FISPeriodRecurrenceType = "PERIOD";
        public const string FISOnBoardingRecurrenceType = "ONBOARDING";
        public const string BenefitTransactionDetailType = "BENEFIT";
        public const string ConsumerOwnerRole = "O";
        public const string MonthlyPeriodType = "MONTH";
        public const string QuarterlyPeriodType = "QUARTER";
        public const string AdhocPeriodType = "ADHOC";
        public const string RedemptionVendorCode = "SUSPENSE_WALLET";
        public const string RedemptionTransactionDetailType = "REDEMPTION";
        public const string RegexForAlphaNumeric = "[^a-zA-Z0-9 ]";
        public const string NewLineSequenceToCRLF = "\r\n";
        public const string RevertRedemptionTransactionDetailType = "REVERT_REDEMPTION";
        public const string RewardSuspenseWalletName = "TENANT_MASTER_REDEMPTION:SUSPENSE_WALLET_REWARD";
        public const string RequestedCardRequestStatus = "REQUESTED";
        public const string RequestedCardRequestStatusNotApplicable = "NOT_APPLICABLE";
        public const string IssuedCardRequestStatus = "ISSUED";
        public const string ProxyNumber = "0000";
        public const string EligibleCardIssueStatus = "ELIGIBLE_FOR_FIS_BATCH_PROCESS";
        public const string ImmediateCardFlowType = "IMMEDIATE";
        public const string Card30BatchSentStatus = "FIS_BATCH_SENT";
        public const string EligibleForActivationCardIssueStatus = "ELIGIBLE_FOR_ACTIVATION";
        public const string MonetaryDollarRewardTypeCode = "rtc-a5a943d3fc2a4506ab12218204d60805";
        public const string MonetaryDollarRewardType = "MONETARY_DOLLARS";
        public const string SubmitCard60 = "isSubmitCard60Job";
        public const string Card60JobNamePrefix = "card60Job";
        public const string Card60FileNamePrefix = "card60ConsumerInput";
        public const string ProcessDepositInstructionsFile = "PROCESS_DEPOSIT_INSTRUCTIONS_FILE";
    }
}
