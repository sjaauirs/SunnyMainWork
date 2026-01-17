using Moq;
using SunnyRewards.Helios.Task.Core.Domain.Models;
using SunnyRewards.Helios.Task.Infrastructure.Repositories.Interfaces;
using SunnyRewards.Helios.Task.UnitTest.Fixtures.MockModel;
using System.Linq.Expressions;

namespace SunnyRewards.Helios.Task.UnitTest.Fixtures.MockRepositories
{
    public class TaskRewardTypeMockRepo : Mock<ITaskRewardTypeRepo>
    {
        public TaskRewardTypeMockRepo()
        {
            Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<TaskRewardTypeModel, bool>>>(), false)).ReturnsAsync(new TaskRewardTypeMockModel());

            Setup(x => x.FindAsync(It.IsAny<Expression<Func<TaskRewardTypeModel, bool>>>(), false)).ReturnsAsync(new TaskRewardTypeMockModel().taskreward);
        }
    }
}
