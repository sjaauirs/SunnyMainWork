using Moq;
using NSubstitute;
using SunnyRewards.Helios.User.Core.Domain.Models;
using SunnyRewards.Helios.User.Infrastructure.Repositories.Interfaces;
using SunnyRewards.Helios.User.UnitTest.Fixtures.MockDto;
using SunnyRewards.Helios.User.UnitTest.Fixtures.MockModels;
using System.Linq.Expressions;

namespace SunnyRewards.Helios.User.UnitTest.Fixtures.MockRepositories
{
    public class PersonMockRepo : Mock<IPersonRepo>
    {
        public PersonMockRepo()
        {
            Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<PersonModel, bool>>>(), false)).ReturnsAsync(new PersonMockModel());
            Setup(x => x.FindAsync(It.IsAny<Expression<Func<PersonModel, bool>>>(), false)).ReturnsAsync(PersonMockDto.personData());
            Setup(x => x.GetConsumerPersons(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
           .ReturnsAsync(ConsumersAndPersonsModelMockList.consumersAndPersonsModelsData());
        }
    }
}
