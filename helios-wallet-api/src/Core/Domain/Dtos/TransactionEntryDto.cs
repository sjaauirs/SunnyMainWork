namespace SunnyRewards.Helios.Wallet.Core.Domain.Dtos
{
    public class TransactionEntryDto
    {
        public TransactionDto? Transaction { get; set; }
        public TransactionDetailDto? TransactionDetail { get; set; }

        public WalletTypeDto? TransactionWalletType { get; set; }
    }
}
