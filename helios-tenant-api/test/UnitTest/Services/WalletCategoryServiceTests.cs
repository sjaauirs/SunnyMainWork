using Microsoft.Extensions.Logging;
using Moq;
using SunnyRewards.Helios.Tenant.Core.Domain.Models;
using SunnyRewards.Helios.Tenant.Infrastructure.Repositories.Interfaces;
using SunnyRewards.Helios.Tenant.Infrastructure.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace SunnyRewards.Helios.Tenant.UnitTest.Services
{
    public class WalletCategoryServiceTests
    {
        private readonly Mock<IWalletCategoryRepo> _repoMock;
        private readonly Mock<ILogger<WalletCategoryService>> _loggerMock;
        private readonly WalletCategoryService _service;

        public WalletCategoryServiceTests()
        {
            _repoMock = new Mock<IWalletCategoryRepo>();
            _loggerMock = new Mock<ILogger<WalletCategoryService>>();
            _service = new WalletCategoryService(_repoMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task GetByTenantCodeAsync_ReturnsResults()
        {
            var tenantCode = "T1";
            var expected = new List<WalletCategoryModel>
            {
                new WalletCategoryModel { Id = 1, TenantCode = tenantCode, WalletTypeCode = "W1", CategoryFk = 10 }
            };

            _repoMock.Setup(r => r.GetByTenantCodeAsync(tenantCode)).ReturnsAsync(expected);

            var result = await _service.GetByTenantCodeAsync(tenantCode);

            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal(expected[0].Id, result.First().Id);
        }

        [Fact]
        public async Task GetByTenantCodeAsync_ThrowsException_IsLoggedAndRethrown()
        {
            var tenantCode = "T1";
            _repoMock.Setup(r => r.GetByTenantCodeAsync(tenantCode))
                     .ThrowsAsync(new Exception("DB error"));

            await Assert.ThrowsAsync<Exception>(() => _service.GetByTenantCodeAsync(tenantCode));

            _loggerMock.Verify(
                x => x.Log(LogLevel.Error,
                           It.IsAny<EventId>(),
                           It.Is<It.IsAnyType>((o, _) => o.ToString()!.Contains("Error fetching wallet categories")),
                           It.IsAny<Exception>(),
                           It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsModel_WhenFound()
        {
            var model = new WalletCategoryModel { Id = 1, TenantCode = "T1", WalletTypeCode = "W1" };
            _repoMock.Setup(r => r.FindOneAsync(It.IsAny<System.Linq.Expressions.Expression<Func<WalletCategoryModel, bool>>>(), false))
                     .ReturnsAsync(model);

            var result = await _service.GetByIdAsync(1);

            Assert.NotNull(result);
            Assert.Equal(model.Id, result!.Id);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsNull_WhenNotFound()
        {
            _repoMock.Setup(r => r.FindOneAsync(It.IsAny<System.Linq.Expressions.Expression<Func<WalletCategoryModel, bool>>>(), false))
                     .ReturnsAsync((WalletCategoryModel?)null);

            var result = await _service.GetByIdAsync(123);

            Assert.Null(result);
        }

        [Fact]
        public async Task GetByIdAsync_ThrowsException_IsLoggedAndRethrown()
        {
            _repoMock.Setup(r => r.FindOneAsync(It.IsAny<System.Linq.Expressions.Expression<Func<WalletCategoryModel, bool>>>(), false))
                     .ThrowsAsync(new Exception("DB error"));

            await Assert.ThrowsAsync<Exception>(() => _service.GetByIdAsync(1));

            _loggerMock.Verify(
                x => x.Log(LogLevel.Error,
                           It.IsAny<EventId>(),
                           It.Is<It.IsAnyType>((o, _) => o.ToString()!.Contains("Error fetching wallet category")),
                           It.IsAny<Exception>(),
                           It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task GetByTenantAndWalletAsync_ReturnsResults()
        {
            var tenantCode = "T1";
            var walletTypeId = 61;
            var expected = new WalletCategoryModel { Id = 1, TenantCode = tenantCode, WalletTypeId = walletTypeId, WalletTypeCode = "W1", CategoryFk = 5 };

            _repoMock.Setup(r => r.GetByTenantAndWalletAsync(tenantCode, walletTypeId))
                     .ReturnsAsync(expected);

            var result = await _service.GetByTenantAndWalletAsync(tenantCode, walletTypeId);

            Assert.NotNull(result);
            Assert.Equal(walletTypeId, result!.WalletTypeId);
        }

        [Fact]
        public async Task GetByTenantAndWalletAsync_ReturnsEmpty_WhenNoMatches()
        {
            _repoMock.Setup(r => r.GetByTenantAndWalletAsync("T1", 61))
                     .ReturnsAsync((WalletCategoryModel?)null);

            var result = await _service.GetByTenantAndWalletAsync("T1", 61);

            Assert.Null(result);
        }

        [Fact]
        public async Task GetByTenantAndWalletAsync_ThrowsException_IsLoggedAndRethrown()
        {
            _repoMock.Setup(r => r.GetByTenantAndWalletAsync("T1", 61))
                     .ThrowsAsync(new Exception("DB error"));

            await Assert.ThrowsAsync<Exception>(() => _service.GetByTenantAndWalletAsync("T1", 61));

            _loggerMock.Verify(
                x => x.Log(LogLevel.Error,
                           It.IsAny<EventId>(),
                           It.Is<It.IsAnyType>((o, _) => o.ToString()!.Contains("Error fetching wallet category for tenantCode")),
                           It.IsAny<Exception>(),
                           It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}
