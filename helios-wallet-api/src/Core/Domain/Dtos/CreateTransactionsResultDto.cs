namespace SunnyRewards.Helios.Wallet.Core.Domain.Dtos
{
    public class CreateTransactionsResultDto
    {
        public double? MasterWalletBalance { get; set; }
        public double? ConsumerWalletBalance { get; set; }
        public double? TotalEarned { get; set; }
    }
}
