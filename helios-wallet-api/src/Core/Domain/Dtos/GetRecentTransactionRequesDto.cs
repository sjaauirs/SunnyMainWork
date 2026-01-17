namespace SunnyRewards.Helios.Wallet.Core.Domain.Dtos
{
    public class GetRecentTransactionRequestDto
    {
        public string? ConsumerCode { get; set; }
        public long WalletId { get; set; }
        public int Count { get; set; } = 1; //for most recent, send Count = 1
        public bool IsRewardAppTransactions { get; set; }
        public bool IsIndividualWallet { get; set; }

        public string? LanguageCode { get; set; }
        public List<string>? skipTransactionType { get; set; }
    }
}
