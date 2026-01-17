using Moq;
using SunnyRewards.Helios.Task.Core.Domain.Models;
using SunnyRewards.Helios.Task.Infrastructure.Repositories;
using SunnyRewards.Helios.Task.Infrastructure.Repositories.Interfaces;
using SunnyRewards.Helios.Task.UnitTest.Fixtures.MockModel;
using System.Linq.Expressions;

namespace SunnyRewards.Helios.Task.UnitTest.Fixtures.MockRepositories
{
    public class TaskDetailMockRepo : Mock<ITaskDetailRepo>
    {
        public TaskDetailMockRepo()
        {
            Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<TaskDetailModel, bool>>>(), false)).ReturnsAsync(new TaskDetailMockModel());

            Setup(x => x.FindAsync(It.IsAny<Expression<Func<TaskDetailModel, bool>>>(), false)).ReturnsAsync(new TaskDetailMockModel().taskDetail);

           SetupSequence(repo => repo.FindOneAsync(It.IsAny<Expression<Func<TaskDetailModel, bool>>>(), false)).ReturnsAsync((TaskDetailModel)null).ReturnsAsync(new TaskDetailMockModel()).ReturnsAsync(new TaskDetailMockModel()).ReturnsAsync(new TaskDetailMockModel()).ReturnsAsync(new TaskDetailMockModel());
        }
    }
}
