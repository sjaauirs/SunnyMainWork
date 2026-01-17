using System.Linq.Expressions;
using Moq;
using SunnyRewards.Helios.User.Core.Domain.Models;
using SunnyRewards.Helios.User.Infrastructure.Repositories.Interfaces;
using SunnyRewards.Helios.User.UnitTest.Fixtures.MockModels;

namespace SunnyRewards.Helios.User.UnitTest.Fixtures.MockRepositories
{
    public class PersonAddressMockRepo : Mock<IPersonAddressRepo>
    {
        public PersonAddressMockRepo()
        {
            Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<PersonAddressModel, bool>>>(), false)).ReturnsAsync(new PersonAddressMockModel());
            Setup(x => x.FindAsync(It.IsAny<Expression<Func<PersonAddressModel, bool>>>(), false)).ReturnsAsync(new List<PersonAddressModel>() { new PersonAddressModel { AddressTypeId = 1} });
        }
    }
}
