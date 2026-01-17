using SunnyRewards.Helios.ETL.Core.Domain.Models;

namespace SunnyRewards.Helios.ETL.Core.Domain.Dtos
{
    public class RedemptionRequestDto
    {
        public string? TenantCode { get; set; }
        public string? ConsumerCode { get; set; }
        public string? RedemptionWalletTypeCode { get; set; }
        public long MasterRedemptionWalletId { get; set; }
        public ETLWalletModel? ConsumerWallet { get; set; }
        public double RedemptionAmount { get; set; }
        public string? RedemptionVendorCode { get; set; }
        public string? RedemptionRef { get; set; }
        public string? Notes { get; set; }
        public string? RedemptionItemDescription { get; set; }
        public string? RedemptionItemData { get; set; }
        public string? TransactionDetailType { get; set; }
        public string? NewTransactionCode { get; set; }
    }
}
