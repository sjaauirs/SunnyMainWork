using Moq;
using SunnyRewards.Helios.Wallet.Infrastructure.Repositories.Interfaces;

namespace SunnyRewards.Helios.Wallet.UnitTest.Fixtures.MockRepositories
{

    public class WalletTypeMockRepo : Mock<IWalletTypeRepo>
    {
        public WalletTypeMockRepo()
        {
            // Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<WalletTypeModel, bool>>>(), false)).ReturnsAsync(new WalletTypeMockModel());

        }
    }
}

