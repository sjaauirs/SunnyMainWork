using SunnyRewards.Helios.Common.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Wallet.Core.Domain.Dtos
{
    public class ConsumerWalletResponseDto : BaseResponseDto
    {
        public List<ConsumerWalletDetailDto> ConsumerWalletDetails { get; set; } = new List<ConsumerWalletDetailDto>();
    }

    public class ConsumerWalletDetailDto
    {
        public WalletDto? Wallet { get; set; }
        public WalletTypeDto? WalletType { get; set; }
        public ConsumerWalletDto? ConsumerWallet { get; set; }
    }

}
