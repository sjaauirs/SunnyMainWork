using Moq;
using SunnyRewards.Helios.Tenant.Core.Domain.Models;
using SunnyRewards.Helios.Tenant.Infrastructure.Repositories.Interfaces;
using SunnyRewards.Helios.Tenant.UnitTest.Fixture.MockModel;
using System.Linq.Expressions;

namespace SunnyRewards.Helios.Tenant.UnitTest.Fixture.MockRepo
{
    public class TenantMockRepo : Mock<ITenantRepo>
    {
        public TenantMockRepo()
        {
            Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<TenantModel, bool>>>(), false)).ReturnsAsync(new TenantMockModel());
            Setup(x => x.FindAllAsync()).ReturnsAsync(new List<TenantModel>());
        }
    }
}

