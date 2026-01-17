using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunnyRewards.Helios.Admin.Core.Domain.Dtos.Constants
{
    public class Constant
    {
        public const string RewardTypeName_MONETARY_DOLLARS = "MONETARY_DOLLARS";
        public const string RewardTypeName_SWEEPSTAKES_ENTRIES = "SWEEPSTAKES_ENTRIES";
        public const string RewardTypeName_MEMBERSHIP_DOLLARS = "MEMBERSHIP_DOLLARS";
        public const string RedemptionVendorCode_SuspenseWalletReward = "SUSPENSE_WALLET_REWARD";
        public const string RedemptionVendorCode_SuspenseWalletHealthyLiving = "SUSPENSE_WALLET_Healthy Living Suspense";
        public const string RedemptionItemDescription_ValueLoad = "Transferred to Rewards purse";
        public const string RedemptionItemDescription_ValueLoad_HealthyLiving = "Transferred to rewards wallet";
        public const string Currency_USD = "USD";
        public const int MaxTries_Count = 3;
        public const string ImageCriteria = "IMAGE_CRITERIA";
        // API URL's
        public const string WalletRedeemStartAPIUrl = "wallet/redeem-start";
        public const string FISValueLoadAPIUrl = "fis/load-value";
        public const string WalletRedeemFailAPIUrl = "wallet/redeem-fail";
        public const string WalletRedeemCompleteAPIUrl = "wallet/redeem-complete";
        public const string ScriptLanguage = "V8_SCRIPT_ENGINE";
        public const string CreateUser = "ETL";
        public const string UpdateUser = "ETL";
        public const string Month = "MONTH";
        public const string Periodic = "PERIODIC";
        public const string Schedule = "SCHEDULE";
        public const string QuarterlyPeriod = "QUARTER";
        public const string CreateConsumerTask = "consumer-task";
        public const string UploadImage = "upload-image";

        public const string RedemptionVendorCode_SuspenseWalletCostco = "SUSPENSE_WALLET_Costco";
        public const string CostcoRedemptionItemDescription_ValueLoad = "Transferred to Costco purse";
        public const string Benefits = "BENEFITS";
        public const string ConsumerLoginDetailUrl = "consumer/get-consumer-login-detail";
        public const string ConsumerEngagementDetailUrl = "consumer/get-consumer-engagement-detail";
        public const string RedemptionItemDescription_Wallet_name = "Transferred to {walletName} wallet";
        public const string WalletNamePlaceholder = "{walletName}";
        public const string LiveTransferRedemptionNotes = "REDEMPTION_REWARD_AUTOSWEEP";

    }
}
