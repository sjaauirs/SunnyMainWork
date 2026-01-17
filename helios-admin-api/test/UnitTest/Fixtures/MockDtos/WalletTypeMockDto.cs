using SunnyRewards.Helios.Wallet.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Admin.UnitTest.Fixtures.MockDtos
{
    public class WalletTypeMockDto : WalletTypeDto
    {
        public WalletTypeMockDto()
        {
            WalletTypeId = 1;
            WalletTypeCode = "test";
            WalletTypeName = "Health Actions Reward";
            WalletTypeLabel = "test";
        }
    }
}
