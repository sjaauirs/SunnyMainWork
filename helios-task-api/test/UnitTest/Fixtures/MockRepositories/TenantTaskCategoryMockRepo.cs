using Moq;
using SunnyRewards.Helios.Task.Core.Domain.Models;
using SunnyRewards.Helios.Task.Infrastructure.Repositories.Interfaces;
using SunnyRewards.Helios.Task.UnitTest.Fixtures.MockModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SunnyRewards.Helios.Task.UnitTest.Fixtures.MockRepositories
{
    public class TenantTaskCategoryMockRepo : Mock<ITenantTaskCategoryRepo>
    {
        public TenantTaskCategoryMockRepo()
        {
          Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<TenantTaskCategoryModel, bool>>>(), false)).ReturnsAsync(new TenantTaskCategoryMockModel());

            Setup(x => x.FindAsync(It.IsAny<Expression<Func<TenantTaskCategoryModel, bool>>>(), false)).ReturnsAsync(new TenantTaskCategoryMockModel().tenantcategory);


        }
    }
}
