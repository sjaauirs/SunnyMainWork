using SunnyRewards.Helios.Tenant.Core.Domain.Models;
using Xunit;

namespace SunnyRewards.Helios.Tenant.UnitTest.Fixture.MockModel
{
    public class WalletCategoryModelTests
    {
        [Fact]
        public void WalletCategoryModel_DefaultsAndSetters()
        {
            var model = new WalletCategoryModel();
            Assert.Equal(string.Empty, model.TenantCode);
            Assert.Equal(string.Empty, model.WalletTypeCode);

            model.Id = 5;
            model.TenantCode = "T1";
            model.CategoryFk = 42;

            Assert.Equal(5, model.Id);
            Assert.Equal("T1", model.TenantCode);
            Assert.Equal(42, model.CategoryFk);
        }
    }
}
