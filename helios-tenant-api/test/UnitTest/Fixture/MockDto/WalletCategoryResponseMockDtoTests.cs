using Xunit;
using SunnyRewards.Helios.Tenant.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Tenant.UnitTest.Fixture.MockDto
{
    public class WalletCategoryResponseDtoTests
    {
        [Fact]
        public void WalletCategoryResponseDto_DefaultsAndSetters()
        {
            // Arrange & Act
            var dto = new WalletCategoryResponseDto();

            // Assert default values
            Assert.Equal(string.Empty, dto.TenantCode);
            Assert.Equal(string.Empty, dto.WalletTypeCode);
            Assert.Null(dto.ErrorCode);
            Assert.Null(dto.ErrorMessage);

            // Set values
            dto.Id = 7;
            dto.CategoryFk = 99;
            dto.ConfigJson = "{ \"a\":1 }";
            dto.ErrorCode = 400;
            dto.ErrorMessage = "Bad Request";

            // Assert set values
            Assert.Equal(7, dto.Id);
            Assert.Equal(99, dto.CategoryFk);
            Assert.Equal("{ \"a\":1 }", dto.ConfigJson);
            Assert.Equal(400, dto.ErrorCode);
            Assert.Equal("Bad Request", dto.ErrorMessage);
        }
    }
}
