using Moq;
using SunnyRewards.Helios.Task.Core.Domain.Models;
using SunnyRewards.Helios.Task.Infrastructure.Repositories.Interfaces;
using System.Linq.Expressions;

namespace SunnyRewards.Helios.Task.UnitTest.Fixtures.MockRepositories
{
    public class TaskExternalMappingMockRepo : Mock<ITaskExternalMappingRepo>
    {
        public TaskExternalMappingMockRepo()
        {
            Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<TaskExternalMappingModel, bool>>>(), false)).ReturnsAsync(new TaskExternalMappingModel());
        }
    }
}
