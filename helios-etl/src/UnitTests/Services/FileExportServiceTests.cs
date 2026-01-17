using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using SunnyRewards.Helios.ETL.Infrastructure.Services;
using SunnyRewards.Helios.ETL.Infrastructure.Helpers.Interfaces;
using SunnyRewards.Helios.ETL.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.ETL.Core.Domain.Dtos;
using System.Text;
using TaskAlias = System.Threading.Tasks.Task;

namespace SunnyRewards.Helios.ETL.UnitTests.Services
{
    public class FileExportServiceTests
    {
        private readonly Mock<ILogger<FileExportService>> _mockLogger = new();
        private readonly Mock<IAwsS3Service> _mockS3Service = new();
        private readonly Mock<IPgpS3FileEncryptionHelper> _mockEncryptionHelper = new();
        private readonly Mock<ISecretHelper> _mockSecretHelper = new();

        private readonly FileExportService _service;

        public FileExportServiceTests()
        {
            _service = new FileExportService(_mockLogger.Object, _mockS3Service.Object, _mockEncryptionHelper.Object, _mockSecretHelper.Object);
        }

        [Fact]
        public async TaskAlias WriteDataToFile_WritesContentSuccessfully()
        {
            // Arrange
            string fileName = $"test_{Guid.NewGuid()}.txt";
            string data = "sample test data";

            // Act
            var filePath = await _service.WriteDataToFile(data, fileName);

            // Assert
            Assert.True(File.Exists(filePath));
            var content = await File.ReadAllTextAsync(filePath, Encoding.UTF8);
            Assert.Equal(data, content);

            // Cleanup
            File.Delete(filePath);
        }

        [Fact]
        public async TaskAlias EncryptFileIfRequiredThenUploadToS3Async_WithEncryption_CallsEncryptAndUpload()
        {
            // Arrange
            var context = new EtlExecutionContext
            {
                TenantCode = "ABC",
                ArchiveBucketName = "test-bucket",
                ArchiveFilePath = "archive"
            };
            string data = "secret data";
            string fileName = "output.csv";
            string filePath = Path.Combine(Path.GetTempPath(), fileName);

            await File.WriteAllTextAsync(filePath, data, Encoding.UTF8);

            var publicKey = Convert.ToBase64String(Encoding.UTF8.GetBytes("mock-public-key"));
            _mockSecretHelper.Setup(x => x.GetTenantSpecificPublicKey(context.TenantCode)).ReturnsAsync(publicKey);

            _mockEncryptionHelper
                .Setup(x => x.EncryptFile(It.IsAny<Stream>(), It.IsAny<Stream>(), It.IsAny<Stream>()))
                .Verifiable();

            _mockS3Service
                .Setup(x => x.UploadStreamToS3(It.IsAny<Stream>(), context.ArchiveBucketName, It.IsAny<string>()))
                .Returns(TaskAlias.CompletedTask)
                .Verifiable();

            // Act
            await _service.EncryptFileIfRequiredThenUploadToS3Async(context, filePath, fileName, encrypt: true);

            // Assert
            _mockEncryptionHelper.Verify();
            _mockS3Service.Verify();

            File.Delete(filePath);
        }

        [Fact]
        public async TaskAlias EncryptFileIfRequiredThenUploadToS3Async_WithoutEncryption_CopiesAndUploads()
        {
            // Arrange
            var context = new EtlExecutionContext
            {
                TenantCode = "ABC",
                ArchiveBucketName = "test-bucket",
                ArchiveFilePath = "archive"
            };
            string data = "plain text";
            string fileName = "plain.csv";
            string filePath = Path.Combine(Path.GetTempPath(), fileName);
            await File.WriteAllTextAsync(filePath, data, Encoding.UTF8);

            _mockS3Service
                .Setup(x => x.UploadStreamToS3(It.IsAny<Stream>(), context.ArchiveBucketName, It.IsAny<string>()))
                .Returns(TaskAlias.CompletedTask)
                .Verifiable();

            // Act
            await _service.EncryptFileIfRequiredThenUploadToS3Async(context, filePath, fileName, encrypt: false);

            // Assert
            _mockEncryptionHelper.Verify(x => x.EncryptFile(It.IsAny<Stream>(), It.IsAny<Stream>(), It.IsAny<Stream>()), Times.Never);
            _mockS3Service.Verify();

            File.Delete(filePath);
        }
    }
}
