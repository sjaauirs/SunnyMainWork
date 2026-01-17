namespace SunnyRewards.Helios.Wallet.Core.Domain.Dtos
{
    public class GetConsumerWalletRequestDto
    {
        public string? TenantCode { get; set; }
        public string? ConsumerCode { get; set; }
    }
}
