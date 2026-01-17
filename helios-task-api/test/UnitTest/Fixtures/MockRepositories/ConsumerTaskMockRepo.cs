using Moq;
using SunnyRewards.Helios.Task.Core.Domain.Models;
using SunnyRewards.Helios.Task.Infrastructure.Repositories.Interfaces;
using SunnyRewards.Helios.Task.UnitTest.Fixtures.MockModel;
using System.Linq.Expressions;

namespace SunnyRewards.Helios.Task.UnitTest.Fixtures.MockRepositories
{
    public class ConsumerTaskMockRepo : Mock<IConsumerTaskRepo>
    {
        public ConsumerTaskMockRepo()
        {
            Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<ConsumerTaskModel, bool>>>(), false)).ReturnsAsync(new ConsumerTaskMockModel());
            Setup(x => x.FindAsync(It.IsAny<Expression<Func<ConsumerTaskModel, bool>>>(), false)).ReturnsAsync(ConsumerTaskMockModel.consumerData());
            Setup(x => x.CreateAsync(It.IsAny<ConsumerTaskModel>())).ReturnsAsync(new ConsumerTaskMockModel());
            Setup(x => x.UpdateAsync(It.IsAny<ConsumerTaskModel>())).ReturnsAsync(new ConsumerTaskMockModel());
            Setup(x => x.FindAsync(It.IsAny<Expression<Func<ConsumerTaskModel, bool>>>(), false)).ReturnsAsync(new List<ConsumerTaskModel>() { new ConsumerTaskMockModel() });
            Setup(x => x.GetConsumerTasksWithRewards(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<long>())).ReturnsAsync(new ConsumerTaskRewardMockModel());
            Setup(x => x.FindAsync(It.IsAny<Expression<Func<ConsumerTaskModel, bool>>>(), false)).ReturnsAsync(new List<ConsumerTaskModel> { new ConsumerTaskModel { TaskStatus = "IN_PROGRESS", TaskId = 2 } });
        }
    }
}
