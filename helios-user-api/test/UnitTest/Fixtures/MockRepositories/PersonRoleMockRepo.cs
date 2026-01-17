using Moq;
using SunnyRewards.Helios.User.Core.Domain.Models;
using SunnyRewards.Helios.User.Infrastructure.Repositories.Interfaces;
using SunnyRewards.Helios.User.UnitTest.Fixtures.MockModels;
using System.Linq.Expressions;

namespace SunnyRewards.Helios.User.UnitTest.Fixtures.MockRepositories
{
    public class PersonRoleMockRepo : Mock<IPersonRoleRepo>
    {
        public PersonRoleMockRepo()
        {
            Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<PersonRoleModel, bool>>>(), false)).ReturnsAsync(new PersonRoleMockModel());
        }
    }
}