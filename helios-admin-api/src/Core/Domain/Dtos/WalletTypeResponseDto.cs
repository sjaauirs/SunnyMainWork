using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.Wallet.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Admin.Core.Domain.Dtos
{
    public class WalletTypeResponseDto : BaseResponseDto
    {
        public WalletTypeDto WalletTypeDto { get; set; }
    }
}
