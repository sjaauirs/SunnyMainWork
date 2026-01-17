namespace SunnyRewards.Helios.Wallet.Core.Domain.Models
{
    public class ConsumerWalletDetailsModel
    {
        public WalletModel? Wallet { get; set; }
        public WalletTypeModel? WalletType { get; set; }
        public ConsumerWalletModel? ConsumerWallet { get; set; }
    }
}
