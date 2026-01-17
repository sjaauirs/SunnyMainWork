using Microsoft.Extensions.Logging;
using Moq;
using Sunny.Benefits.Bff.Infrastructure.HttpClients.Interfaces;
using Sunny.Benefits.Bff.Infrastructure.Services;
using SunnyRewards.Helios.Tenant.Core.Domain.Dtos;
using Xunit;

namespace Sunny.Benefits.Bff.UnitTest.Services
{
    public class WalletCategoryServiceTests
    {
        private readonly Mock<ITenantClient> _tenantClientMock;
        private readonly WalletCategoryService _service;

        public WalletCategoryServiceTests()
        {
            _tenantClientMock = new Mock<ITenantClient>();
            var loggerMock = new Mock<ILogger<WalletCategoryService>>();
            _service = new WalletCategoryService(loggerMock.Object, _tenantClientMock.Object);
        }

        [Fact]
        public async Task GetByTenant_ReturnsData_WhenClientResponds()
        {
            // Arrange
            var tenantCode = "T1";
            var expected = new List<WalletCategoryResponseDto>
        {
            new WalletCategoryResponseDto { Id = 1 }
        };

            _tenantClientMock
                .Setup(c => c.Get<IEnumerable<WalletCategoryResponseDto>>(
                    $"wallet-category/tenant/{tenantCode}", It.IsAny<Dictionary<string, long>>()))
                .ReturnsAsync(expected);

            // Act
            var result = await _service.GetByTenant(tenantCode);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal(expected[0].Id, result.First().Id);
        }

        [Fact]
        public async Task GetByTenant_ReturnsEmpty_WhenClientReturnsNull()
        {
            // Arrange
            var tenantCode = "T1";
            _tenantClientMock
                .Setup(c => c.Get<IEnumerable<WalletCategoryResponseDto>>(
                    $"wallet-category/tenant/{tenantCode}", It.IsAny<Dictionary<string, long>>()))
                .ReturnsAsync((IEnumerable<WalletCategoryResponseDto>?)null);

            // Act
            var result = await _service.GetByTenant(tenantCode);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetByTenant_ReturnsEmpty_WhenClientThrows()
        {
            // Arrange
            var tenantCode = "T1";
            _tenantClientMock
                .Setup(c => c.Get<IEnumerable<WalletCategoryResponseDto>>(
                    $"wallet-category/tenant/{tenantCode}", It.IsAny<Dictionary<string, long>>()))
                .ThrowsAsync(new Exception("fail"));

            // Act
            var result = await _service.GetByTenant(tenantCode);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetById_ReturnsData_WhenClientResponds()
        {
            // Arrange
            int id = 1;
            var expected = new WalletCategoryResponseDto { Id = id};

            _tenantClientMock
                .Setup(c => c.Get<WalletCategoryResponseDto>(
                    $"wallet-category/{id}", It.IsAny<Dictionary<string, long>>()))
                .ReturnsAsync(expected);

            // Act
            var result = await _service.GetById(id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expected.Id, result.Id);
        }

        [Fact]
        public async Task GetById_ReturnsNull_WhenClientThrows()
        {
            // Arrange
            long id = 1;
            _tenantClientMock
                .Setup(c => c.Get<WalletCategoryResponseDto>(
                    $"wallet-category/{id}", It.IsAny<Dictionary<string, long>>()))
                .ThrowsAsync(new Exception("fail"));

            // Act
            var result = await _service.GetById(id);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetByTenantAndWallet_ReturnsData_WhenClientResponds()
        {
            // Arrange
            var tenantCode = "T1";
            long walletTypeId = 2;
            var expected = new List<WalletCategoryResponseDto>
        {
            new WalletCategoryResponseDto { Id = 10 }
        };

            _tenantClientMock
                .Setup(c => c.Get<IEnumerable<WalletCategoryResponseDto>>(
                    $"wallet-category/tenant/{tenantCode}/wallet/{walletTypeId}",
                    It.IsAny<Dictionary<string, long>>()))
                .ReturnsAsync(expected);

            // Act
            var result = await _service.GetByTenantAndWallet(tenantCode, walletTypeId);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal(expected[0].Id, result.First().Id);
        }

        [Fact]
        public async Task GetByTenantAndWallet_ReturnsEmpty_WhenClientThrows()
        {
            // Arrange
            var tenantCode = "T1";
            long walletTypeId = 2;

            _tenantClientMock
                .Setup(c => c.Get<IEnumerable<WalletCategoryResponseDto>>(
                    $"wallet-category/tenant/{tenantCode}/wallet/{walletTypeId}",
                    It.IsAny<Dictionary<string, long>>()))
                .ThrowsAsync(new Exception("fail"));

            // Act
            var result = await _service.GetByTenantAndWallet(tenantCode, walletTypeId);

            // Assert
            Assert.Empty(result);
        }
    }
}
