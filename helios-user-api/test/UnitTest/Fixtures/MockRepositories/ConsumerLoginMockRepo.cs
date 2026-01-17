using Moq;
using SunnyRewards.Helios.User.Core.Domain.Models;
using SunnyRewards.Helios.User.Infrastructure.Repositories.Interfaces;
using SunnyRewards.Helios.User.UnitTest.Fixtures.MockDto;
using SunnyRewards.Helios.User.UnitTest.Fixtures.MockModels;
using System.Linq.Expressions;

namespace SunnyRewards.Helios.User.UnitTest.Fixtures.MockRepositories
{
    public class ConsumerLoginMockRepo : Mock<IConsumerLoginRepo>
    {
        public ConsumerLoginMockRepo()
        {
            Setup(x => x.FindAsync(It.IsAny<Expression<Func<ConsumerLoginModel, bool>>>(), false)).ReturnsAsync(ConsumerLoginTestClass.ConsumerLoginList());
            
            Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<ConsumerLoginModel, bool>>>(), false)).ReturnsAsync(new ConsumerLoginMockModel());
        }
    }
}