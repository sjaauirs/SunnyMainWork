namespace SunnyRewards.Helios.Wallet.Core.Domain.Dtos
{
    public class FindConsumerWalletByWalletTypeRequestDto
    {
        public string? ConsumerCode { get; set; }
        public string? WalletTypeCode { get; set; }
    }
}
