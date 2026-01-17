using Moq;
using SunnyRewards.Helios.Task.Core.Domain.Models;
using SunnyRewards.Helios.Task.Infrastructure.Repositories.Interfaces;
using SunnyRewards.Helios.Task.UnitTest.Fixtures.MockModel;
using System.Linq.Expressions;

namespace SunnyRewards.Helios.Task.UnitTest.Fixtures.MockRepositories
{
    public class TriviaQuestionGroupMockRepo : Mock<ITriviaQuestionGroupRepo>
    {
        public TriviaQuestionGroupMockRepo()
        {
            Setup(x => x.FindAsync(It.IsAny<Expression<Func<TriviaQuestionGroupModel, bool>>>(),false)).ReturnsAsync(new List<TriviaQuestionGroupModel>() { new TriviaQuestionGroupMockModel ()});
        }
       

    }
}
