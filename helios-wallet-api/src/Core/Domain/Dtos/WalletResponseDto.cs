using SunnyRewards.Helios.Common.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Wallet.Core.Domain.Dtos
{
    public class WalletResponseDto : BaseResponseDto
    {
        public double GrandTotal { get; set; }
        public WalletDetailDto[] walletDetailDto { get; set; } = new WalletDetailDto[0];
    }
}
