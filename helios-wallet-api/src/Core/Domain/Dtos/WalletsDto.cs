using SunnyRewards.Helios.Wallet.Core.Domain.Models;

namespace SunnyRewards.Helios.Wallet.Core.Domain.Dtos
{
    public class WalletsDto
    {
        public bool IsValid { get; set; }
        public WalletModel? ConsumerWallet { get; set; }
        public WalletModel? MasterWallet { get; set; }
        public WalletModel? RedemptionWallet { get; set; }

    }
}
