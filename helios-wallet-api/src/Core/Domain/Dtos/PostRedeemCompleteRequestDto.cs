using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunnyRewards.Helios.Wallet.Core.Domain.Dtos
{
    public class PostRedeemCompleteRequestDto : XminBaseDto
    {
        public string? ConsumerCode { get; set; }
        public string? RedemptionVendorCode { get; set; }  // For now, send PRIZEOUT
        public string? RedemptionRef { get; set; }  // vendor supplied unique ID for the redemption request (Prizeout)
                                                    // calls this “request_id” in payload
    }
}
