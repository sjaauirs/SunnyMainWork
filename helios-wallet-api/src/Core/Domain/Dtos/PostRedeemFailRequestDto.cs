namespace SunnyRewards.Helios.Wallet.Core.Domain.Dtos
{
    public class PostRedeemFailRequestDto
    {
        public string? TenantCode { get; set; }
        public string? ConsumerCode { get; set; }
        public string? RedemptionVendorCode { get; set; }   // For now, send “PRIZEOUT”
        public Double RedemptionAmount { get; set; }    // from Prizeout /fail method call giftcard_cost field
        public string? RedemptionRef { get; set; }  // from Prizeout /fail method call request_id field
        public string? Notes { get; set; }  // can be null
    }
}