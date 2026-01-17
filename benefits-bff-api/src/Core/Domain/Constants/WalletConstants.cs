using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sunny.Benefits.Bff.Core.Domain.Constants
{
    public static class WalletConstants
    {
        public const string getConsumerApi = "consumer/get-consumer";
        public const string liveBalanceApi = "fis/get-purse-balances";
        public const string GetFundingDescription = "get-funding-description";
        public const string updateWalletBalanceApi = "wallet/update-wallet-balance";
        public const string GetConsumerBenefitWalletTypesAPIUrl = "consumer-benefits-wallet-types";
        public const string WalletRedeemStartAPIUrl = "wallet/redeem-start";
        public const string FISValueLoadAPIUrl = "fis/load-value";
        public const string WalletRedeemFailAPIUrl = "wallet/redeem-fail";
        public const string WalletRedeemCompleteAPIUrl = "wallet/redeem-complete";
        public const string GetAllConsumerWalletsAPIUrl = "consumer-wallet/get-all-consumer-wallets";

        public const string RedemptionVendorCode_SuspenseWalletReward = "SUSPENSE_WALLET_REWARD";
        public const string Redemption_PickAPurse = "REDEMPTION_REWARD_PICKAPURSE";
        public const string RedemptionItemDescription_ValueLoad = "Transferred to {0} purse";
        public const string Currency_USD = "USD";
        public const int MaxTries_Count = 3;
        public const string GetConsumerAccount = "get-consumer-account";
        public const string GetTenantAccount = "tenant-account-export";
        public const string MerchantNameForFundTransfer = "Transfer from Rewards";
        public const string GetTenantAccountByTenantCode = "get-tenant-account";
        public const string DisabledPickAPurseStatus = "DISABLED";
    }
}
