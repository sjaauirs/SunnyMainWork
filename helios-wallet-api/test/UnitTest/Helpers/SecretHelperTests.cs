using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using SunnyRewards.Helios.Common.Core.Helpers.Interfaces;
using SunnyRewards.Helios.Wallet.Infrastructure.Helpers;
using SunnyRewards.Helios.Wallet.UnitTest.Fixtures.MockAppSettings;
using Xunit;

namespace SunnyRewards.Helios.Wallet.UnitTest.Helpers
{
    public class SecretHelperTests
    {
        private readonly IConfiguration _configurationMock;
        private readonly Mock<IVault> _vaultMock;
        private readonly Mock<ILogger<SecretHelper>> _loggerMock;
        private readonly SecretHelper _secretHelper;

        public SecretHelperTests()
        {
            _configurationMock = new ConfigurationBuilder()
                .AddInMemoryCollection(AppSettingsMock.AppSettings)
                .Build();
            _vaultMock = new Mock<IVault>();
            _loggerMock = new Mock<ILogger<SecretHelper>>();
            _secretHelper = new SecretHelper(_configurationMock, _vaultMock.Object, _loggerMock.Object);
        }

        

        [Fact]
        public void GetRewardWalletTypeCode_ShouldReturnCode_WhenConfigured()
        {
            // Arrange
            const string rewardWalletTypeCode = "wat-2d62dcaf2aa4424b9ff6c2ddb5895077";

            // Act
            var result = _secretHelper.GetRewardWalletTypeCode();

            // Assert
            Assert.Equal(rewardWalletTypeCode, result);
        }

        [Fact]
        public void GetRedemptionWalletTypeCode_ShouldReturnCode_WhenConfigured()
        {
            // Arrange
            const string redemptionWalletTypeCode = "wat-274bd71345804f09928cf451dc0f6239";

            // Act
            var result = _secretHelper.GetRedemptionWalletTypeCode();

            // Assert
            Assert.Equal(redemptionWalletTypeCode, result);
        }

        [Fact]
        public void GetSweepstakesEntriesWalletTypeCode_ShouldReturnCode_WhenConfigured()
        {
            // Arrange
            const string redemptionWalletTypeCode = "wat-c3b091232e974f98aeceb495d2a9f916";

            // Act
            var result = _secretHelper.GetSweepstakesEntriesWalletTypeCode();

            // Assert
            Assert.Equal(redemptionWalletTypeCode, result);
        }

        [Fact]
        public void GetSweepstakesEntriesRedemptionWalletTypeCode_ShouldReturnCode_WhenConfigured()
        {
            // Arrange
            const string sweepstakesEntriesRedemptionWalletTypeCode = "wat-e2c6076b59db46febd8d76fd019ae0b0";

            // Act
            var result = _secretHelper.GetSweepstakesEntriesRedemptionWalletTypeCode();

            // Assert
            Assert.Equal(sweepstakesEntriesRedemptionWalletTypeCode, result);
        }

    }
}
