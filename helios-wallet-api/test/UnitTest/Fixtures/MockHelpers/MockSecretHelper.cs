using Moq;
using SunnyRewards.Helios.Wallet.Infrastructure.Helpers.Interfaces;

namespace SunnyRewards.Helios.Wallet.UnitTest.Fixtures.MockHelpers
{
    public class MockSecretHelper : Mock<ISecretHelper>
    {
        public MockSecretHelper()
        {
            Setup(x => x.GetRewardWalletTypeCode()).Returns("Test_Code");
            Setup(x => x.GetSweepstakesEntriesWalletTypeCode()).Returns("Test_Code");
            Setup(x => x.GetRedemptionWalletTypeCode()).Returns("Test_Code");
            Setup(x => x.GetSweepstakesEntriesRedemptionWalletTypeCode()).Returns("Test_Code");
        }
    }
}
