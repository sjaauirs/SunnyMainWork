using Microsoft.Extensions.Logging;
using Moq;
using NHibernate;
using SunnyRewards.Helios.Common.Core.Repositories;
using SunnyRewards.Helios.Tenant.Core.Domain.Models;
using SunnyRewards.Helios.Tenant.Infrastructure.Repositories;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace SunnyRewards.Helios.Tenant.UnitTest.Fixture.MockRepo
{
    public class WalletCategoryRepoTests
    {
        [Fact]
        public void CanConstruct_WalletCategoryRepo()
        {
            var loggerMock = new Mock<ILogger<SunnyRewards.Helios.Common.Core.Repositories.BaseRepo<WalletCategoryModel>>>();
            var sessionMock = new Mock<ISession>();

            var repo = new WalletCategoryRepo(loggerMock.Object, sessionMock.Object);

            Assert.NotNull(repo);
        }

        [Fact]
        public async Task GetByTenantCodeAsync_ReturnsOnlyMatchingTenantAndNotDeleted()
        {
            var tenantCode = "TEN1";
            var data = new List<WalletCategoryModel>
            {
                new WalletCategoryModel { Id = 1, TenantCode = "TEN1", DeleteNbr = 0 },
                new WalletCategoryModel { Id = 2, TenantCode = "TEN2", DeleteNbr = 0 },
                new WalletCategoryModel { Id = 3, TenantCode = "TEN1", DeleteNbr = 1 }
            };

            var queryable = data.AsQueryable();

            var sessionMock = new Mock<ISession>();
            sessionMock.Setup(s => s.Query<WalletCategoryModel>()).Returns(queryable);

            var loggerMock = new Mock<ILogger<SunnyRewards.Helios.Common.Core.Repositories.BaseRepo<WalletCategoryModel>>>();
            var repo = new WalletCategoryRepo(loggerMock.Object, sessionMock.Object);

            var result = await repo.GetByTenantCodeAsync(tenantCode);

            Assert.Single(result);
            Assert.Equal(1, result.First().Id);
        }

        [Fact]
        public async Task GetByTenantAndWalletAsync_ReturnsMatchingRecords()
        {
            var tenantCode = "TEN1";
            var walletTypeId = 61;

            var data = new List<WalletCategoryModel>
            {
                new WalletCategoryModel { Id = 1, TenantCode = "TEN1", WalletTypeId = 61, DeleteNbr = 0 },
                new WalletCategoryModel { Id = 2, TenantCode = "TEN1", WalletTypeId = 99, DeleteNbr = 0 },
                new WalletCategoryModel { Id = 3, TenantCode = "TEN2", WalletTypeId = 61, DeleteNbr = 0 },
                new WalletCategoryModel { Id = 4, TenantCode = "TEN1", WalletTypeId = 61, DeleteNbr = 1 }
            };

            var queryable = data.AsQueryable();

            var sessionMock = new Mock<ISession>();
            sessionMock.Setup(s => s.Query<WalletCategoryModel>()).Returns(queryable);

            var loggerMock = new Mock<ILogger<SunnyRewards.Helios.Common.Core.Repositories.BaseRepo<WalletCategoryModel>>>();
            var repo = new WalletCategoryRepo(loggerMock.Object, sessionMock.Object);

            var result = await repo.GetByTenantAndWalletAsync(tenantCode, walletTypeId);

            // Repository returns a single WalletCategoryModel or null when no match
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal(walletTypeId, result.WalletTypeId);
            Assert.Equal("TEN1", result.TenantCode);
        }

        [Fact]
        public async Task GetByTenantAndWalletAsync_ReturnsEmpty_WhenNoMatches()
        {
            var tenantCode = "TEN1";
            var walletTypeId = 61;

            var data = new List<WalletCategoryModel>
            {
                new WalletCategoryModel { Id = 2, TenantCode = "TEN2", WalletTypeId = 99, DeleteNbr = 0 }
            };

            var queryable = data.AsQueryable();

            var sessionMock = new Mock<ISession>();
            sessionMock.Setup(s => s.Query<WalletCategoryModel>()).Returns(queryable);

            var loggerMock = new Mock<ILogger<SunnyRewards.Helios.Common.Core.Repositories.BaseRepo<WalletCategoryModel>>>();
            var repo = new WalletCategoryRepo(loggerMock.Object, sessionMock.Object);

            var result = await repo.GetByTenantAndWalletAsync(tenantCode, walletTypeId);

            // No matching record should yield null
            Assert.Null(result);
        }
    }
}
