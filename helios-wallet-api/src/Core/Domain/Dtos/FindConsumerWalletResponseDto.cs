using SunnyRewards.Helios.Common.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Wallet.Core.Domain.Dtos
{
    public class FindConsumerWalletResponseDto : BaseResponseDto
    {
        public List<ConsumerWalletDto>? ConsumerWallets { get; set; }
    }
}