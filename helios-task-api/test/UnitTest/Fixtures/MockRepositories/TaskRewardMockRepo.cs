using Moq;
using SunnyRewards.Helios.Task.Core.Domain.Models;
using SunnyRewards.Helios.Task.Infrastructure.Repositories.Interfaces;
using SunnyRewards.Helios.Task.UnitTest.Fixtures.MockDtos;
using SunnyRewards.Helios.Task.UnitTest.Fixtures.MockModel;
using System.Linq.Expressions;

namespace SunnyRewards.Helios.Task.UnitTest.Fixtures.MockRepositories
{
    public class TaskRewardMockRepo : Mock<ITaskRewardRepo>
    {
        public TaskRewardMockRepo()
        {
                Setup(x => x.FindAsync(It.IsAny<Expression<Func<TaskRewardModel, bool>>>(), false)).ReturnsAsync( new TaskRewardMockModel().taskData);
                Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<TaskRewardModel, bool>>>(), false)).ReturnsAsync(new TaskRewardMockModel());

             Setup(x => x.FindAsync(It.IsAny<Expression<Func<TaskRewardModel, bool>>>(), false)).ReturnsAsync(new TaskRewardIsMockModel().taskRewardData);
             Setup(x => x.GetTaskRewardDetailsList(It.IsAny<string>(),It.IsAny<string>())).Returns(new GetTaskByTenantCodeResponseMockDto().AvailableTasks);
             Setup(x => x.GetTaskRewardDetailsList(It.IsAny<string>(),It.IsAny<string>(),It.IsAny<List<long>>())).Returns(new GetTaskByTenantCodeResponseMockDto().AvailableTasks);
        }
    }
}
