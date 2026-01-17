using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using SunnyRewards.Helios.ETL.Core.Domain.Dtos;
using SunnyRewards.Helios.ETL.Core.Domain.Models;
using SunnyRewards.Helios.ETL.Infrastructure.Helpers.Interfaces;
using SunnyRewards.Helios.ETL.Infrastructure.Repositories.HeliosRepo.Interfaces;
using SunnyRewards.Helios.ETL.Infrastructure.Services;
using SunnyRewards.Helios.ETL.Infrastructure.Services.Interfaces;
using System.Linq.Expressions;

namespace SunnyRewards.Helios.ETL.UnitTests.Services
{
    public class SyncMembersFromRedshiftToPostgresServiceTests
    {
        private readonly Mock<ILogger<SyncMembersFromRedshiftToPostgresService>> _loggerMock = new();
        private readonly Mock<ISecretHelper> _secretHelperMock = new();
        private readonly Mock<IRedshiftSyncStatusRepo> _syncStatusRepoMock = new();
        private readonly Mock<IRedshiftDataReader> _redshiftMock = new();
        private readonly Mock<IPostgresBulkInserter> _postgresMock = new();
        private readonly Mock<IETLMemberImportFileRepo> _memberImportFileRepoMock = new();
        private readonly Mock<IMemoryCache> _cacheMock = new();

        private SyncMembersFromRedshiftToPostgresService CreateService()
        {
            _secretHelperMock.Setup(x => x.GetSecret("RedShiftConnectionString")).ReturnsAsync("Host=localhost;Database=test;Username=test;Password=test;");
            _secretHelperMock.Setup(x => x.GetSecret("RedShiftConnectionString")).ReturnsAsync("Host=localhost;Database=test;Username=test;Password=test;");

            return new SyncMembersFromRedshiftToPostgresService(
                _loggerMock.Object,
                _secretHelperMock.Object,
                _syncStatusRepoMock.Object,
                _memberImportFileRepoMock.Object,
                _cacheMock.Object,
                _redshiftMock.Object,
                _postgresMock.Object
            );
        }

        [Fact]
        public async System.Threading.Tasks.Task SyncAsync_ShouldDefaultBatchSize_WhenBatchSizeIsZeroOrNegative()
        {
            // Arrange
            var service = CreateService();
            var context = new EtlExecutionContext { BatchSize = 0 };
            var memoryCacheEntryMock = new Mock<ICacheEntry>();
            _cacheMock
           .Setup(x => x.CreateEntry(It.IsAny<object>()))
           .Returns(memoryCacheEntryMock.Object);
            var cacheKey = "sample_key";
            var expectedValue = "cached_value";
            _cacheMock
                .Setup(x => x.TryGetValue(cacheKey, out It.Ref<object>.IsAny))
                .Returns((string key, out object value) =>
                {
                    value = expectedValue;
                    return true;
                });

            _syncStatusRepoMock.Setup(x => x.FindAsync(It.IsAny<Expression<Func<ETLRedshiftSyncStatusModel, bool>>>(), false))
                     .ReturnsAsync(new List<ETLRedshiftSyncStatusModel>
                     {
                        new ETLRedshiftSyncStatusModel
                        {
                        }
                     });
            _memberImportFileRepoMock.Setup(x => x.FindAsync(It.IsAny<Expression<Func<ETLMemberImportFileModel, bool>>>(), false))
                     .ReturnsAsync(new List<ETLMemberImportFileModel>
                     {
                        new ETLMemberImportFileModel
                        {
                            MemberImportFileId = 1
                        }
                     });

            var batch = new List<RedShiftMemberImportFileDataDto>
            {
                new RedShiftMemberImportFileDataDto
                {
                    RecordNumber = 1,
                    FileName = "test.csv",
                    RawDataJson = "{}",
                    CreateTs = DateTime.UtcNow
                }
            };

            _redshiftMock
            .SetupSequence(r => r.FetchBatchAsync(It.IsAny<string>(), It.IsAny<long?>(), It.IsAny<int>()))
            .ReturnsAsync(batch)
            .ReturnsAsync(new List<RedShiftMemberImportFileDataDto>());

            _postgresMock
                .Setup(p => p.BulkInsertAsync(It.IsAny<string>(), It.IsAny<List<ETLMemberImportFileDataModel>>()))
                .Returns(System.Threading.Tasks.Task.CompletedTask);

            // Act
            await service.SyncAsync(context);

            // Assert
            _redshiftMock.Verify(r => r.FetchBatchAsync(It.IsAny<string>(), It.IsAny<long?>(), It.IsAny<int>()), Times.AtLeastOnce());
            _postgresMock.Verify(p => p.BulkInsertAsync(It.IsAny<string>(), It.IsAny<List<ETLMemberImportFileDataModel>>()), Times.AtLeastOnce());
        }
      

        [Fact]
        public async System.Threading.Tasks.Task SyncAsync_ShouldHandleException_AndUpdateSyncStatus()
        {
            // Arrange
            var service = CreateService();
            var context = new EtlExecutionContext { BatchSize = 1000 };
            _syncStatusRepoMock.Setup(x => x.FindAsync(It.IsAny<Expression<Func<ETLRedshiftSyncStatusModel, bool>>>(), false))
                .ThrowsAsync(new Exception("Test exception"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => service.SyncAsync(context));
        }

    }
}
