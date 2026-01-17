using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using SunnyRewards.Helios.Admin.Infrastructure.Helpers;
using SunnyRewards.Helios.Admin.UnitTest.Fixtures.MockDtos;
using SunnyRewards.Helios.Common.Core.Helpers.Interfaces;
using Xunit;

namespace SunnyRewards.Helios.Admin.UnitTest.Helpers
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
        public async System.Threading.Tasks.Task GetAwsAccessKey_ShouldReturnAccessKey_WhenConfigured()
        {
            // Arrange
            const string accessKey = "access-key";
            const string secretValue = "secret-access-key";

            _vaultMock.Setup(x => x.GetSecret(accessKey)).ReturnsAsync(secretValue);

            // Act
            var result = await _secretHelper.GetAwsAccessKey();

            // Assert
            Assert.Equal(secretValue, result);
        }

        [Fact]
        public async System.Threading.Tasks.Task GetAwsSecretKey_ShouldReturnSecretKey_WhenConfigured()
        {
            // Arrange
            const string secretKey = "secret-key";
            const string secretValue = "secret-secret-key";

            _vaultMock.Setup(x => x.GetSecret(secretKey)).ReturnsAsync(secretValue);

            // Act
            var result = await _secretHelper.GetAwsSecretKey();

            // Assert
            Assert.Equal(secretValue, result);
        }

        [Fact]
        public async System.Threading.Tasks.Task GetEnvironment_ShouldReturnEnvironment_WhenConfigured()
        {
            // Arrange
            const string secretValue = "production";

            _vaultMock.Setup(x => x.GetSecret("env")).ReturnsAsync(secretValue);

            // Act
            var result = await _secretHelper.GetEnvironment();

            // Assert
            Assert.Equal(secretValue, result);
        }

        [Fact]
        public void GetAwsTmpS3BucketName_ShouldReturnBucketName_WhenConfigured()
        {
            // Arrange
            const string bucketName = "tmp-bucket";

            // Act
            var result = _secretHelper.GetAwsTmpS3BucketName();

            // Assert
            Assert.Equal(bucketName, result);
        }

        [Fact]
        public void GetExportTenantVersion_ShouldReturnVersion_WhenConfigured()
        {
            // Arrange
            const string version = "1.0";

            // Act
            var result = _secretHelper.GetExportTenantVersion();

            // Assert
            Assert.Equal(version, result);
        }

        [Fact]
        public async System.Threading.Tasks.Task GetAwsAccessKey_ShouldThrowException_WhenSecretNotConfigured()
        {
            // Arrange
            var accessKey = "access-key";

            _vaultMock.Setup(x => x.GetSecret(accessKey)).ReturnsAsync(string.Empty);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _secretHelper.GetAwsAccessKey());
        }

        [Fact]
        public async System.Threading.Tasks.Task GetAwsSecretKey_ShouldThrowException_WhenSecretNotConfigured()
        {
            // Arrange
            const string secretKey = "secret-key";

            _vaultMock.Setup(x => x.GetSecret(secretKey)).ReturnsAsync(string.Empty);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _secretHelper.GetAwsSecretKey());
        }
       
    }
}