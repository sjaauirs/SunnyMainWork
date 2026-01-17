namespace SunnyRewards.Helios.Admin.Core.Domain.Dtos
{
    public class FundTransferToPurseRequestDto
    {
        public string? TenantCode { get; set; }
        public string? ConsumerCode { get; set; }
        public string? ConsumerWalletTypeCode { get; set; }   // Source wallet
        public string? RedemptionVendorCode { get; set; }   // need to set - RedemptionVendorCode_SuspenseWalletHealthyLiving
        public double? RedemptionAmount { get; set; }    // Amount
        public string? Notes { get; set; }
        public string? PurseWalletType { get; set; }   // targetWallet  --GetHealthyLivingPurseWalletTypeCode
        public long? WalletId { get; set; }
    }
}
