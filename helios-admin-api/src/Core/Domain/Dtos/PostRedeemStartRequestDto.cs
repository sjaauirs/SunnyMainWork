using SunnyRewards.Helios.Common.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Admin.Core.Domain.Dtos
{
    public class PostRedeemStartRequestDto : XminBaseDto
    {
        public string? ConsumerWalletTypeCode { get; set; }
        public string? RedemptionWalletTypeCode { get; set; }
        public string? TenantCode { get; set; }
        public string? ConsumerCode { get; set; }
        public string? RedemptionVendorCode { get; set; }  // For now, send “PRIZEOUT”
        public double? RedemptionAmount { get; set; }
        public string? RedemptionRef { get; set; }  // vendor supplied unique ID for the redemption request
        public string? RedemptionItemDescription { get; set; }
        public string? Notes { get; set; }  // can be null
        public string? RedemptionItemData { get; set; }
        public long? WalletId { get; set; }
    }
}
