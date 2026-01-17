using Microsoft.Extensions.Logging;
using Moq;
using SunnyRewards.Helios.ETL.Core.Domain.Dtos;
using SunnyRewards.Helios.ETL.Infrastructure.Services;
using SunnyRewards.Helios.ETL.Infrastructure.Services.Interfaces;

namespace SunnyRewards.Helios.ETL.UnitTests.Services
{
    public class RedshiftToSftpExportServiceTests
    {
        private readonly Mock<ILogger<RedshiftToSftpExportService>> _loggerMock;
        private readonly Mock<IRedshiftDataService> _redshiftServiceMock;
        private readonly Mock<IFileExportService> _fileExportServiceMock;
        private readonly Mock<ISftpUploader> _sftpUploaderMock;
        private readonly RedshiftToSftpExportService _service;

        public RedshiftToSftpExportServiceTests()
        {
            _loggerMock = new Mock<ILogger<RedshiftToSftpExportService>>();
            _redshiftServiceMock = new Mock<IRedshiftDataService>();
            _fileExportServiceMock = new Mock<IFileExportService>();
            _sftpUploaderMock = new Mock<ISftpUploader>();

            _service = new RedshiftToSftpExportService(
                _loggerMock.Object,
                _redshiftServiceMock.Object,
                _fileExportServiceMock.Object,
                _sftpUploaderMock.Object
            );
        }

        private EtlExecutionContext GetSampleContext() => new()
        {
            TenantCode = "ABC",
            TableName = "members",
            CustomerCode = "ABC123",
            ColumnName = "updated_at",
            DateRangeStart = "2024-01-01",
            DateRangeEnd = "2024-12-31",
            OutboundFileNamePattern = "members_{yyyyMMdd_HHmmss}_{uuid}.csv",
            Delimiter = ",",
            ShouldEncrypt = true,
            ArchiveBucketName = "bucket",
            ArchiveFilePath = "export/path",
            RedshiftDatabaseName= "etl_staging"
        };

        [Fact]
        public async System.Threading.Tasks.Task ExecuteExportAsync_ShouldCompleteWithoutErrors()
        {
            // Arrange
            var context = GetSampleContext();
            var mockData = "id,name\n1,Alice";

            _redshiftServiceMock
                .Setup(r => r.FetchDataAsync(context.TableName, context.ColumnName, context.DateRangeStart, context.DateRangeEnd, context.Delimiter,context.RedshiftDatabaseName, "yyyy-MM-dd", true))
                .ReturnsAsync(mockData);

            var mockFilePath = "/tmp/members_export.csv";

            _fileExportServiceMock
                .Setup(f => f.WriteDataToFile(mockData, It.IsAny<string>()))
                .ReturnsAsync(mockFilePath);

            _fileExportServiceMock
                .Setup(f => f.EncryptFileIfRequiredThenUploadToS3Async(context, mockFilePath, It.IsAny<string>(), context.ShouldEncrypt))
                .Returns(System.Threading.Tasks.Task.CompletedTask);

            _sftpUploaderMock
                .Setup(s => s.UploadFile(context, mockFilePath))
                .Returns(System.Threading.Tasks.Task.CompletedTask);

            // Act
            await _service.ExecuteExportAsync(context);

            // Assert
            _redshiftServiceMock.VerifyAll();
            _fileExportServiceMock.VerifyAll();
            _sftpUploaderMock.VerifyAll();
        }

        [Fact]
        public async System.Threading.Tasks.Task ExecuteExportAsync_ShouldThrow_WhenRedshiftFails()
        {
            // Arrange
            var context = GetSampleContext();

            _redshiftServiceMock
                .Setup(r => r.FetchDataAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
                .ThrowsAsync(new Exception("Redshift error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _service.ExecuteExportAsync(context));
        }

        [Fact]
        public async System.Threading.Tasks.Task ExecuteExportAsync_ShouldThrow_WhenFileWriteFails()
        {
            // Arrange
            var context = GetSampleContext();
            var mockData = "id,name\n1,Alice";

            _redshiftServiceMock
                .Setup(r => r.FetchDataAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync(mockData);

            _fileExportServiceMock
                .Setup(f => f.WriteDataToFile(mockData, It.IsAny<string>()))
                .ThrowsAsync(new IOException("Disk full"));

            // Act & Assert
            await Assert.ThrowsAsync<IOException>(() => _service.ExecuteExportAsync(context));
        }

        [Fact]
        public async System.Threading.Tasks.Task ExecuteExportAsync_ShouldThrow_WhenEncryptionFails()
        {
            // Arrange
            var context = GetSampleContext();
            var mockData = "id,name\n1,Alice";
            var mockFilePath = "/tmp/members_export.csv";

            _redshiftServiceMock
                .Setup(r => r.FetchDataAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync(mockData);

            _fileExportServiceMock
                .Setup(f => f.WriteDataToFile(mockData, It.IsAny<string>()))
                .ReturnsAsync(mockFilePath);

            _fileExportServiceMock
                .Setup(f => f.EncryptFileIfRequiredThenUploadToS3Async(context, mockFilePath, It.IsAny<string>(), context.ShouldEncrypt))
                .ThrowsAsync(new Exception("PGP failure"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _service.ExecuteExportAsync(context));
        }

        [Fact]
        public async System.Threading.Tasks.Task ExecuteExportAsync_ShouldThrow_WhenSftpUploadFails()
        {
            // Arrange
            var context = GetSampleContext();
            var mockData = "id,name\n1,Alice";
            var mockFilePath = "/tmp/members_export.csv";

            _redshiftServiceMock
                .Setup(r => r.FetchDataAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync(mockData);

            _fileExportServiceMock
                .Setup(f => f.WriteDataToFile(mockData, It.IsAny<string>()))
                .ReturnsAsync(mockFilePath);

            _fileExportServiceMock
                .Setup(f => f.EncryptFileIfRequiredThenUploadToS3Async(context, mockFilePath, It.IsAny<string>(), context.ShouldEncrypt))
                .Returns(System.Threading.Tasks.Task.CompletedTask);

            _sftpUploaderMock
                .Setup(s => s.UploadFile(context, mockFilePath))
                .ThrowsAsync(new Exception("SFTP timeout"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _service.ExecuteExportAsync(context));
        }
    }
}
