using Moq;
using SunnyRewards.Helios.Task.Core.Domain.Dtos;
using SunnyRewards.Helios.Task.Core.Domain.Models;
using SunnyRewards.Helios.Task.Infrastructure.Repositories;
using SunnyRewards.Helios.Task.Infrastructure.Repositories.Interfaces;
using SunnyRewards.Helios.Task.UnitTest.Fixtures.MockDtos;
using SunnyRewards.Helios.Task.UnitTest.Fixtures.MockModel;
using System.Linq.Expressions;

namespace SunnyRewards.Helios.Task.UnitTest.Fixtures.MockRepositories
{
    public class TaskMockRepo : Mock<ITaskRepo>
    {
        public TaskMockRepo()
        {
             Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<TaskModel, bool>>>(), false)).ReturnsAsync(new TaskMockModel());

            Setup(x => x.FindAsync(It.IsAny<Expression<Func<TaskModel, bool>>>(), false)).ReturnsAsync(new TaskMockModel().taskModel);

        }
    }
}
