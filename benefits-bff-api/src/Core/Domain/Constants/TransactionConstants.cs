using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sunny.Benefits.Bff.Core.Domain.Constants
{
    public static class TransactionConstants
    {

        public static readonly IReadOnlyDictionary<string, string> TxnTypeMapping = new Dictionary<string, string>
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

        public static readonly IReadOnlyList<string> SkipTxnType = new List<string>
    {
        "Decline",
        "Non-Mon Update"
    };
        public const string Purchase = "Purchase";
        public const string Unknown = "UNKNOWN";

    }


}


