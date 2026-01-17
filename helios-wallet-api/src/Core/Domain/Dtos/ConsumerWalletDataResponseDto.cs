using SunnyRewards.Helios.Common.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Wallet.Core.Domain.Dtos
{
    public class ConsumerWalletDataResponseDto : BaseResponseDto
    {
        public WalletDto Wallet { get; set; } = new WalletDto();
        public ConsumerWalletDto ConsumerWallet { get; set; } = new ConsumerWalletDto();
    }
}
