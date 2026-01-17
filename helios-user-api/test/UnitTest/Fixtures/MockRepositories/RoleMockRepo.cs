using Moq;
using SunnyRewards.Helios.User.Core.Domain.Models;
using SunnyRewards.Helios.User.Infrastructure.Repositories.Interfaces;
using SunnyRewards.Helios.User.UnitTest.Fixtures.MockModels;
using System.Linq.Expressions;

namespace SunnyRewards.Helios.User.UnitTest.Fixtures.MockRepositories
{
    public class RoleMockRepo : Mock<IRoleRepo>
    {
        public RoleMockRepo()
        {
            Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<RoleModel, bool>>>(), false)).ReturnsAsync(new RoleMockModel());
        }
    }
}