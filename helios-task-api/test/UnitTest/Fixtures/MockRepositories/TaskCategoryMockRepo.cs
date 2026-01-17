using Moq;
using SunnyRewards.Helios.Task.Core.Domain.Models;
using SunnyRewards.Helios.Task.Infrastructure.Repositories.Interfaces;
using SunnyRewards.Helios.Task.UnitTest.Fixtures.MockModel;
using System.Linq.Expressions;

namespace SunnyRewards.Helios.Task.UnitTest.Fixtures.MockRepositories
{
    public class TaskCategoryMockRepo : Mock<ITaskCategoryRepo>
    {
        public TaskCategoryMockRepo()
        {
            Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<TaskCategoryModel, bool>>>(), false)).ReturnsAsync(new TaskCategoryMockModel());

            Setup(x => x.FindOneAsync(It.IsAny<int>())).ReturnsAsync(new TaskCategoryMockModel());

            Setup(x => x.FindAsync(It.IsAny<Expression<Func<TaskCategoryModel, bool>>>(), false)).ReturnsAsync(new TaskCategoryMockModel().GetTaskCategories);

            Setup(x => x.FindAllAsync()).ReturnsAsync(new TaskCategoryMockModel().GetTaskCategories);
        }
    }
}