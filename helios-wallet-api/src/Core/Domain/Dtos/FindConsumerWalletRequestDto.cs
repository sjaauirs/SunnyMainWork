namespace SunnyRewards.Helios.Wallet.Core.Domain.Dtos
{
    public class FindConsumerWalletRequestDto 
    {
        public string? ConsumerCode { get; set; }
        public bool IncludeRedeemOnlyWallets { get; set; } = false;
    }
}
