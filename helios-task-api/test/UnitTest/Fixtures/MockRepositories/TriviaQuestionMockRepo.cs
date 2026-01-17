using Moq;
using SunnyRewards.Helios.Task.Core.Domain.Models;
using SunnyRewards.Helios.Task.Infrastructure.Repositories.Interfaces;
using SunnyRewards.Helios.Task.UnitTest.Fixtures.MockModel;
using System.Linq.Expressions;

namespace SunnyRewards.Helios.Task.UnitTest.Fixtures.MockRepositories
{
    public class TriviaQuestionMockRepo : Mock<ITriviaQuestionRepo>
    {
        public TriviaQuestionMockRepo()
        {
            Setup(x => x.FindAsync(It.IsAny<Expression<Func<TriviaQuestionModel, bool>>>(), false)).ReturnsAsync(new List<TriviaQuestionModel>() { new TriviaQuestionMockModel() });
        }
    }
}
