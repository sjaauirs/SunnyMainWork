using Sunny.Benefits.Bff.Core.Domain.Enums;
using System.Collections.Immutable;

namespace Sunny.Benefits.Bff.Core.Domain.Constants
{
    public static class CardOperationConstants
    {
        private static readonly ImmutableDictionary<string, string> _cardOperationMappings = new Dictionary<string, string>
        {
            { "FREEZE", "SUSPEND" },
            { "UNFREEZE", "UNSUSPEND" },
            { "ACTIVATE", "ACTIVATE" },
            { "LOSTORSTOLEN", "LOSTSTOLEN" }
        }.ToImmutableDictionary();
        private static readonly ImmutableDictionary<string, string> _reverseCardOperationMappings = new Dictionary<string, string>
        {
            { nameof(CardStatus.SUSPENDED), "Freeze" },
            { nameof(CardStatus.ACTIVE), "Active" },
            { nameof(CardStatus.LOST), "LostOrStolen" },
            { nameof(CardStatus.READY), "ReadyForActivation" },
            { nameof(CardStatus.REPLACED), "Replaced" },
        }.ToImmutableDictionary();

        public static IReadOnlyDictionary<string, string> CardOperationMappings => _cardOperationMappings;
        public static IReadOnlyDictionary<string, string> ReverseCardOperationMappings => _reverseCardOperationMappings;

        public const string UnknownStatus = "Unknown";
        public const string FisReplaceCardApiUrl = "fis/replace-card";
        public const string FisCardOperationApiUrl = "fis/card-operation";
        public const string FisCardReissueApiUrl = "fis/reissue-card";
        public const string FisCardStatusApiUrl = "fis/card-status";
        public const string Redemption = "REDEMPTION";
        public const string TransactionTypeSubtract = "S";
        public const string TransactionTypeAdd = "A";
        public const int txnType = 16;
        public const int PendingAndAdjustTxnType = 20;
        public const int Days = 0;
        public const string FiscardTxnApi = "fis/card-transactions";
        public const string FisSetCardStatus = "UNSUSPEND";
        public const string FisSetCardStatusLost = "LOSTSTOLEN";
        public const string GetPersonAndConsumerDetails = "person/get-details-by-consumer-code";
        public const string HealthyLivingRedumtionVendorCode = "SUSPENSE_WALLET_Healthy Living Suspense";
        public const string CardOrderedNotificationEventName = "Card%20Ordered%20Event";
        public const string CardFreezeNotificationEventName = "Card%20Freezed%20Event";
        public const string NotificationEventSourceModule = "BenefitsAPI";

    }
}
