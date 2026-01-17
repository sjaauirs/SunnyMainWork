using SunnyRewards.Helios.Common.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Wallet.Core.Domain.Dtos
{
    public class GetMasterWalletResponseDto : BaseResponseDto
    {
        public WalletDto? Wallet { get; set; }
    }
}