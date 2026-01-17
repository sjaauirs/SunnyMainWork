namespace SunnyRewards.Helios.Admin.Core.Domain.Dtos
{
    public class FundTransferRequestDto
    {
        public string? TenantCode { get; set; }
        public string? ConsumerCode { get; set; }
        public string? ConsumerWalletTypeCode { get; set; }
        public string? RedemptionWalletTypeCode { get; set; }
        public string? RedemptionVendorCode { get; set; }
        public double? RedemptionAmount { get; set; }
        public string? RedemptionRef { get; set; }
        public string? RedemptionItemDescription { get; set; }
        public string? Notes { get; set; }
        public string? RedemptionItemData { get; set; }
        public string? PurseWalletType { get; set; }
    }
}
