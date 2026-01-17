using SunnyRewards.Helios.Common.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Wallet.Core.Domain.Dtos
{
    public class GetWalletTypeResponseDto : BaseResponseDto
    {
        public List<WalletTypeDto>? WalletTypes { get; set; }
    }
}
