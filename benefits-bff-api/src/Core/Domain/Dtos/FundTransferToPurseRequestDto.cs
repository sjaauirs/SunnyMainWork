namespace Sunny.Benefits.Bff.Core.Domain.Dtos
{
    public class FundTransferToPurseRequestDto
    {
        public string? TenantCode { get; set; }
        public string? ConsumerCode { get; set; }
        public string? ConsumerWalletTypeCode { get; set; }
        public string? RedemptionVendorCode { get; set; }
        public double? RedemptionAmount { get; set; }
        public string? Notes { get; set; }
        public string? PurseWalletType { get; set; }
        public long? WalletId { get; set; }
    }

}
