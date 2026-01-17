using Moq;
using SunnyRewards.Helios.Task.Core.Domain.Models;
using SunnyRewards.Helios.Task.Infrastructure.Repositories.Interfaces;
using SunnyRewards.Helios.Task.UnitTest.Fixtures.MockModel;
using System.Linq.Expressions;

namespace SunnyRewards.Helios.Task.UnitTest.Fixtures.MockRepositories
{
    public class TenantTaskCategoryRepo : Mock<ITenantTaskCategoryRepo>
    {
        public TenantTaskCategoryRepo()
        {
            Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<TenantTaskCategoryModel, bool>>>(), false)).ReturnsAsync(new TenantTaskCategoryMockModel());

        }
    }
}
