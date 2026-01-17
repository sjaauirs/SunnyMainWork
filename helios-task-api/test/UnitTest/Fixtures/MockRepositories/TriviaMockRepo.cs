using Moq;
using SunnyRewards.Helios.Task.Core.Domain.Models;
using SunnyRewards.Helios.Task.Infrastructure.Repositories.Interfaces;
using SunnyRewards.Helios.Task.UnitTest.Fixtures.MockModel;
using System.Linq.Expressions;

namespace SunnyRewards.Helios.Task.UnitTest.Fixtures.MockRepositories
{
    public class TriviaMockRepo : Mock<ITriviaRepo>
    {
        public TriviaMockRepo()
        {
            Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<TriviaModel, bool>>>(), false)).ReturnsAsync(new TriviaMockModel());

        }
    }
}
