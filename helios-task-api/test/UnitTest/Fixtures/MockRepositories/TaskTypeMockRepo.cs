using Moq;
using SunnyRewards.Helios.Task.Core.Domain.Models;
using SunnyRewards.Helios.Task.Infrastructure.Repositories.Interfaces;
using SunnyRewards.Helios.Task.UnitTest.Fixtures.MockModel;
using System.Linq.Expressions;

namespace SunnyRewards.Helios.Task.UnitTest.Fixtures.MockRepositories
{
    public class TaskTypeMockRepo : Mock<ITaskTypeRepo>
    {
        public TaskTypeMockRepo()
        {
            Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<TaskTypeModel, bool>>>(), false)).ReturnsAsync(new TaskTypeMockModel());

            Setup(x => x.FindOneAsync(It.IsAny<int>())).ReturnsAsync(new TaskTypeMockModel());

            Setup(x => x.FindAsync(It.IsAny<Expression<Func<TaskTypeModel, bool>>>(), false)).ReturnsAsync(new TaskTypeMockModel().tasktype);

            Setup(x => x.FindAllAsync()).ReturnsAsync(new TaskTypeMockModel().tasktype);
        }
    }
}
