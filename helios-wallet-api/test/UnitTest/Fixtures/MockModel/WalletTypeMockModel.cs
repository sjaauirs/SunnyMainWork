using SunnyRewards.Helios.Wallet.Core.Domain.Models;

namespace SunnyRewards.Helios.Wallet.UnitTest.Fixtures.MockModel
{
    public class WalletTypeMockModel : WalletTypeModel
    {
        public WalletTypeMockModel()
        {
            WalletTypeId = 1;
            WalletTypeCode = "wat-2d62dcaf2aa4424b9ff6c2ddb5895077";
            WalletTypeName = "Health Actions Reward";
            CreateTs = DateTime.Now;
            UpdateTs = DateTime.Now;
            CreateUser = "sunny";
            UpdateUser = "sunny rewards";
            DeleteNbr = 0;
            ShortLabel = "OTC";
            ConfigJson = "{\"currency\":\"ENTRIES\"}";
        }
    }
}
