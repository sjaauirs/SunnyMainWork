namespace SunnyRewards.Helios.Wallet.Core.Domain.Dtos
{
    public class WalletDetailDto
    {
        public WalletDto Wallet { get; set; } = new WalletDto();
        public WalletTypeDto WalletType { get; set; } = new WalletTypeDto();
        public List<TransactionEntryDto> RecentTransaction { get; set; } = new List<TransactionEntryDto>();
        public bool IsFilteredSpend { get; set; }
    }
}
