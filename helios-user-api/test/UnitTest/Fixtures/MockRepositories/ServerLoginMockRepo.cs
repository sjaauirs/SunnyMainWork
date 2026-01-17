using Moq;
using SunnyRewards.Helios.User.Core.Domain.Models;
using SunnyRewards.Helios.User.Infrastructure.Repositories.Interfaces;
using SunnyRewards.Helios.User.UnitTest.Fixtures.MockModels;
using System.Linq.Expressions;

namespace SunnyRewards.Helios.User.UnitTest.Fixtures.MockRepositories
{
    public class ServerLoginMockRepo : Mock<IServerLoginRepo>
    {
        public ServerLoginMockRepo()
        {
            Setup(x => x.FindAsync(It.IsAny<Expression<Func<ServerLoginModel, bool>>>(), false)).ReturnsAsync(new List<ServerLoginModel> { new ServerLoginMockModel() });
        }
    }
}
