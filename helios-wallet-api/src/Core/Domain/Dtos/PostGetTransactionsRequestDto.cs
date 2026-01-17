namespace SunnyRewards.Helios.Wallet.Core.Domain.Dtos
{
    public class PostGetTransactionsRequestDto
    {
        public string? ConsumerCode { get; set; }
        public long? WalletId { get; set; }
        public bool IsRewardAppTransactions { get; set; }
        public string? LanguageCode { get; set; }
    }
}
