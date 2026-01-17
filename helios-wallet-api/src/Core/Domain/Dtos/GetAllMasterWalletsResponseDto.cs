using SunnyRewards.Helios.Common.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Wallet.Core.Domain.Dtos
{
    public class GetAllMasterWalletsResponseDto : BaseResponseDto
    {
        public List<TenantWalletDetailDto>? MasterWallets { get; set; }
    }

    public class TenantWalletDetailDto
    {
        public WalletDto? Wallet { get; set; }
        public WalletTypeDto? WalletType { get; set; }
    }
}
