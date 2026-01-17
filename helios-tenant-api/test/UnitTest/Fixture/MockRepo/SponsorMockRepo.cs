using Moq;
using SunnyRewards.Helios.Tenant.Core.Domain.Models;
using SunnyRewards.Helios.Tenant.Infrastructure.Repositories.Interfaces;
using SunnyRewards.Helios.Tenant.UnitTest.Fixture.MockModel;
using System.Linq.Expressions;

namespace SunnyRewards.Helios.Tenant.UnitTest.Fixture.MockRepo
{
    public class SponsorMockRepo : Mock<ISponsorRepo>
    {
        public SponsorMockRepo()
        {
            Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<SponsorModel, bool>>>(), false)).ReturnsAsync(new SponsorMockModel());
            Setup(x => x.FindAllAsync()).ReturnsAsync(new List<SponsorModel>());
        }
    }
}

