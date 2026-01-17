using Moq;
using SunnyRewards.Helios.Task.Core.Domain.Models;
using SunnyRewards.Helios.Task.Infrastructure.Repositories;
using SunnyRewards.Helios.Task.Infrastructure.Repositories.Interfaces;
using SunnyRewards.Helios.Task.UnitTest.Fixtures.MockModel;
using System.Linq.Expressions;

namespace SunnyRewards.Helios.Task.UnitTest.Fixtures.MockRepositories
{
    public class TermsOfServiceMockRepo : Mock<ITermsOfServiceRepo>
    {
        public TermsOfServiceMockRepo()
        {
            Setup(x=>x.FindOneAsync(It.IsAny<Expression<Func<TermsOfServiceModel, bool>>>(), false)).ReturnsAsync(new TermsOfServiceMockModel());

            Setup(x => x.FindAsync(It.IsAny<Expression<Func<TermsOfServiceModel, bool>>>(), false)).ReturnsAsync(new TermsOfServiceMockModel().termofservice);
        }
    }
}