using Moq;
using SunnyRewards.Helios.Wallet.Core.Domain.Models;
using SunnyRewards.Helios.Wallet.Infrastructure.Repositories.Interfaces;
using SunnyRewards.Helios.Wallet.UnitTest.Fixtures.MockModel;
using System.Linq.Expressions;

namespace SunnyRewards.Helios.Wallet.UnitTest.Fixtures.MockRepositories
{
    public class RedemptionMockRepo : Mock<IRedemptionRepo>
    {
        public RedemptionMockRepo()
        {
            Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<RedemptionModel, bool>>>(), false)).ReturnsAsync(new RedemptionMockModel());

            Setup(x => x.UpdateRedemption(It.IsAny<DateTime>(), It.IsAny<long>(), It.IsAny<int>())).Returns(3);

        }
    }
}
