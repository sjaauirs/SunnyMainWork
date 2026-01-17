using SunnyRewards.Helios.Tenant.Core.Domain.Models;
using Xunit;

namespace SunnyRewards.Helios.Tenant.UnitTest.Fixture.MockModel
{
    public class CategoryModelTests
    {
        [Fact]
        public void CategoryModel_DefaultsAndSetters()
        {
            var model = new CategoryModel();
            Assert.Equal(string.Empty, model.Name);
            Assert.False(model.IsActive);

            model.Id = 3;
            model.Name = "Food";
            model.GoogleType = "g_type";
            model.IsActive = true;

            Assert.Equal(3, model.Id);
            Assert.Equal("Food", model.Name);
            Assert.Equal("g_type", model.GoogleType);
            Assert.True(model.IsActive);
        }
    }
}
