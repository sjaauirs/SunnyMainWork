using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using SunnyRewards.Helios.Common.Core.Helpers.Interfaces;
using SunnyRewards.Helios.ETL.Common.Constants;
using SunnyRewards.Helios.ETL.Infrastructure.Helpers;

namespace SunnyRewards.Helios.ETL.UnitTests.Helpers
{
    public class SecretHelperTests
    {
        private readonly Mock<IVault> _mockVault = new();
        private readonly Mock<IConfiguration> _mockConfig = new();
        private readonly Mock<ILogger<SecretHelper>> _mockLogger = new();
        private readonly SecretHelper _secretHelper;

        public SecretHelperTests()
        {
            _mockVault.SetupGet(v => v.InvalidSecret).Returns("INVALID");
            _secretHelper = new SecretHelper(_mockVault.Object, _mockConfig.Object, _mockLogger.Object);
        }

        [Fact]
        public async System.Threading.Tasks.Task GetTenantSecret_ReturnsSecret_WhenValid()
        {
            _mockVault.Setup(v => v.GetTenantSecret("TENANT1", Constants.XApiKeySecret)).ReturnsAsync("APIKEY123");

            var result = await _secretHelper.GetTenantSecret("TENANT1", Constants.XApiKeySecret);

            Assert.Equal("APIKEY123", result);
        }

        [Fact]
        public async System.Threading.Tasks.Task GetTenantSecret_Throws_WhenSecretIsInvalid()
        {
            _mockVault.Setup(v => v.GetTenantSecret("TENANT1", Constants.XApiKeySecret)).ReturnsAsync("INVALID");

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _secretHelper.GetTenantSecret("TENANT1", Constants.XApiKeySecret));
            Assert.Equal($"{Constants.XApiKeySecret} is not configured.", ex.Message);
        }

        [Fact]
        public async System.Threading.Tasks.Task GetSecret_ReturnsSecret_WhenValid()
        {
            _mockVault.Setup(v => v.GetSecret(Constants.PostgresConnectionStringKey)).ReturnsAsync("pg-conn-str");

            var result = await _secretHelper.GetSecret(Constants.PostgresConnectionStringKey);

            Assert.Equal("pg-conn-str", result);
        }

        [Fact]
        public async System.Threading.Tasks.Task GetSecret_Throws_WhenInvalid()
        {
            _mockVault.Setup(v => v.GetSecret(Constants.PostgresConnectionStringKey)).ReturnsAsync("INVALID");

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _secretHelper.GetSecret(Constants.PostgresConnectionStringKey));
            Assert.Equal($"{Constants.PostgresConnectionStringKey} is not configured.", ex.Message);
        }

        [Fact]
        public async System.Threading.Tasks.Task GetSftpDetailsByTenantCode_ReturnsConfig_WhenValid()
        {
            _mockVault.Setup(v => v.GetTenantSecret("TENANT1", Constants.SftpHost)).ReturnsAsync("host.com");
            _mockVault.Setup(v => v.GetTenantSecret("TENANT1", Constants.SftpPort)).ReturnsAsync("2222");
            _mockVault.Setup(v => v.GetTenantSecret("TENANT1", Constants.SftpUserName)).ReturnsAsync("user");
            _mockVault.Setup(v => v.GetTenantSecret("TENANT1", Constants.SftpPrivateKey)).ReturnsAsync("key");
            _mockVault.Setup(v => v.GetTenantSecret("TENANT1", Constants.SftpPrivateKeyPassphrase)).ReturnsAsync("pass");
            _mockVault.Setup(v => v.GetTenantSecret("TENANT1", Constants.SftpRemoteDirectory)).ReturnsAsync("/out");

            var result = await _secretHelper.GetSftpDetailsByTenantCode("TENANT1");

            Assert.Equal("host.com", result.Host);
            Assert.Equal(2222, result.Port);
            Assert.Equal("user", result.UserName);
            Assert.Equal("key", result.PrivateKey);
            Assert.Equal("pass", result.PrivateKeyPassphrase);
            Assert.Equal("/out", result.RemoteDirectory);
        }

        [Fact]
        public async System.Threading.Tasks.Task GetSftpDetailsByTenantCode_UsesDefaultPort_WhenInvalid()
        {
            _mockVault.Setup(v => v.GetTenantSecret(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync("value");
            _mockVault.Setup(v => v.GetTenantSecret(It.IsAny<string>(), Constants.SftpPort)).ReturnsAsync("invalid-port");

            var result = await _secretHelper.GetSftpDetailsByTenantCode("TENANT1");

            Assert.Equal(22, result.Port);
        }
    }
}

