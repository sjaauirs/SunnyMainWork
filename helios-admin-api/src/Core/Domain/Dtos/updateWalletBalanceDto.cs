using SunnyRewards.Helios.Wallet.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Admin.Core.Domain.Dtos
{
    public class updateWalletBalanceDto
    {
        public WalletDto wallets { get; set; }
        public Double Amount { get; set; }

    }
}
