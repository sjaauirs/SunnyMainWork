using Moq;
using SunnyRewards.Helios.Wallet.Core.Domain.Models;
using SunnyRewards.Helios.Wallet.Infrastructure.Repositories;
using SunnyRewards.Helios.Wallet.Infrastructure.Repositories.Interfaces;
using SunnyRewards.Helios.Wallet.UnitTest.Fixtures.MockModel;
using System.Linq.Expressions;

namespace SunnyRewards.Helios.Wallet.UnitTest.Fixtures.MockRepositories
{
    public class WalletMockRepo : Mock<IWalletRepo>
    {
        public WalletMockRepo()
        {
            Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<WalletModel, bool>>>(), false)).ReturnsAsync(new WalletMockModel());
            Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<WalletModel, bool>>>(), false)).ReturnsAsync(new WalletMockModel());
            Setup(x => x.FindAsync(It.IsAny<Expression<Func<WalletModel, bool>>>(), false)).ReturnsAsync(new List<WalletModel>() { new WalletMockModel() });
            Setup(x => x.FindAsync(It.IsAny<Expression<Func<WalletModel, bool>>>(), false)).ReturnsAsync(new List<WalletModel>());
            Setup(x => x.FindAsync(It.IsAny<Expression<Func<WalletModel, bool>>>(), false)).ThrowsAsync(new Exception());



            Setup(repo => repo.GetConsumerWallet(It.IsAny<long>(), It.IsAny<string>()))
                .ReturnsAsync((long id, string code) =>
                {
                    if (id == 1 && code == "12345")
                    {
                        return null;
                    }

                    if (id == 2 && code == "cmr-12345")
                    {
                        return new WalletMockModel { TotalEarned = 500, EarnMaximum = 500 };
                    }

                    if (id == 7 && code == "cmr-12345")
                    {
                        return new WalletMockModel { TotalEarned = 0, EarnMaximum = 500 };
                    }
                    if (id == 22 && code == "12345")
                    {
                        return new WalletMockModel { TotalEarned = 200, EarnMaximum = 500 };
                    }
                    return new WalletMockModel { };
                });

            Setup(repo => repo.GetMasterWallet(It.IsAny<long>(), It.IsAny<string>()))
                .ReturnsAsync(new WalletMockModel());
            Setup(repo => repo.GetSuspenseWallet(It.IsAny<long>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new WalletMockModel());

            Setup(x => x.UpdateMasterWalletBalance(It.IsAny<DateTime>(), It.IsAny<double>(), It.IsAny<long>(), It.IsAny<int>())).Returns(1);

            Setup(x => x.UpdateConsumerWalletBalance(It.IsAny<DateTime>(), It.IsAny<double>(), It.IsAny<double>(), It.IsAny<long>(), It.IsAny<int>())).Returns(2);

            Setup(x => x.UpdateRedemptionWalletBalance(It.IsAny<DateTime>(), It.IsAny<double>(), It.IsAny<long>(), It.IsAny<int>())).Returns(3);

            Setup(x => x.UpdateRedemptionWalletBalance(It.IsAny<DateTime>(), It.IsAny<double>(), It.IsAny<long>(), It.IsAny<int>())).Returns(4);
            Setup(x => x.UpdateWalletBalance(It.IsAny<WalletMockModel>())).Returns(1);
        }
    }
}
