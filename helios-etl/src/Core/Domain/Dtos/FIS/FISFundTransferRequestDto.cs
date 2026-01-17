using SunnyRewards.Helios.ETL.Core.Domain.Models;

namespace SunnyRewards.Helios.ETL.Core.Domain.Dtos.FIS
{
    public class FISFundTransferRequestDto
    {
        public string? TenantCode { get; set; }
        public string? ConsumerCode { get; set; }
        public ETLWalletModel? MasterWallet { get; set; }
        public ETLWalletModel? ConsumerWallet { get; set; }
        public double Amount { get; set; }

        public string? TransactionDetailType { get; set; }
        public string? RewardDescription { get; set; }
        public int RuleNumber { get; set; }
        public string? RedemptionRef { get; set; }
    }
}
