using System.IO.Compression;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using SunnyRewards.Helios.Admin.Infrastructure.Helpers;
using SunnyRewards.Helios.Admin.Infrastructure.Services.Interfaces;
using Xunit;
using ISecretHelper = SunnyRewards.Helios.Admin.Infrastructure.Helpers.Interface.ISecretHelper;

namespace SunnyRewards.Helios.Admin.UnitTest.Helpers
{
    public class S3HelperTests
    {
        private readonly Mock<ILogger<S3Helper>> _loggerMock;
        private readonly Mock<ISecretHelper> _secretHelperMock;
        private readonly Mock<IAmazonS3> _s3ClientMock;
        private readonly Mock<IAmazonS3ClientService> _s3ClientServiceMock;
        private readonly S3Helper _s3Helper;


        public S3HelperTests()
        {
            _loggerMock = new Mock<ILogger<S3Helper>>();
            _secretHelperMock = new Mock<ISecretHelper>();
            _s3ClientMock = new Mock<IAmazonS3>();
            _s3ClientServiceMock = new Mock<IAmazonS3ClientService>();
            _s3ClientServiceMock.Setup(x => x.GetAmazonS3Client(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<RegionEndpoint>()))
            .Returns(_s3ClientMock.Object);
            _s3Helper = new S3Helper(_loggerMock.Object, _secretHelperMock.Object, _s3ClientServiceMock.Object);
        }

        [Fact]
        public async void UploadFileToS3_ShouldReturnTrue_WhenFileUploadSucceeds()
        {
            // Arrange
            var formFileMock = new Mock<IFormFile>();
            formFileMock.Setup(f => f.FileName).Returns("test.zip");
            formFileMock.Setup(f => f.OpenReadStream()).Returns(new MemoryStream(new byte[100])); // Mock file stream


            _s3ClientServiceMock.Setup(x => x.GetAmazonS3Client(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<RegionEndpoint>()))
           .Returns(_s3ClientMock.Object);
            _secretHelperMock.Setup(x => x.GetAwsAccessKey()).ReturnsAsync("access-key");
            _secretHelperMock.Setup(x => x.GetAwsSecretKey()).ReturnsAsync("secret-key");


            _s3ClientMock.Setup(x => x.PutObjectAsync(It.IsAny<PutObjectRequest>(), default))
                 .ReturnsAsync(new PutObjectResponse());

            // Act
            var result = await _s3Helper.UploadFileToS3("test", formFileMock.Object, "test");

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async void UploadFileToS3_ShouldThrowException_WhenS3UploadFails()
        {
            // Arrange
            var formFileMock = new Mock<IFormFile>();
            formFileMock.Setup(f => f.FileName).Returns("test.zip");
            formFileMock.Setup(f => f.OpenReadStream()).Returns(new MemoryStream(new byte[100]));


            _s3ClientMock
                .Setup(client => client.PutObjectAsync(It.IsAny<PutObjectRequest>(), default))
                .ThrowsAsync(new AmazonS3Exception("S3 upload failed"));



            // Act & Assert
            await Assert.ThrowsAsync<AmazonS3Exception>(() => _s3Helper.UploadFileToS3("test-bucket", formFileMock.Object, "test-key"));
        }

        [Fact]
        public async void UnzipAndProcessJsonFromS3_ShouldReturnValidImportDto_WhenJsonFilesArePresent()
        {
            // Arrange
            var bucketName = "test-bucket";
            var s3Key = "test.zip";
            var tenantCode = "Tenant123";

         

            var zipStream = new MemoryStream();
            using (var archive = new ZipArchive(zipStream, ZipArchiveMode.Create, true))
            {
                var entry = archive.CreateEntry("task.json");
                using (var entryStream = entry.Open())
                using (var writer = new StreamWriter(entryStream))
                {
                    await writer.WriteAsync("{ \"TaskData\": { \"SomeProperty\": \"TestValue\" } }");
                }
            }
            zipStream.Position = 0;


            _secretHelperMock.Setup(x => x.GetAwsAccessKey()).ReturnsAsync("access-key");
            _secretHelperMock.Setup(x => x.GetAwsSecretKey()).ReturnsAsync("secret-key");

            _s3ClientMock.Setup(x => x.GetObjectAsync(It.IsAny<GetObjectRequest>(), default))
                .ReturnsAsync(new GetObjectResponse
                {
                    ResponseStream = zipStream
                });


            // Act
            var result = await _s3Helper.UnzipAndProcessJsonFromS3(s3Key, tenantCode, bucketName);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.TaskData);
        }

        [Fact]
        public async void UnzipAndProcessJsonFromS3_ShouldThrowException_WhenProcessingFails()
        {
            // Arrange
            var bucketName = "test-bucket";
            var s3Key = "test.zip";
            var tenantCode = "Tenant123";

            _secretHelperMock.Setup(x => x.GetAwsAccessKey()).ReturnsAsync("access-key");
            _secretHelperMock.Setup(x => x.GetAwsSecretKey()).ReturnsAsync("secret-key");

            _s3ClientMock.Setup(x => x.GetObjectAsync(It.IsAny<GetObjectRequest>(), default))
                .ThrowsAsync(new AmazonS3Exception("S3 retrieval failed"));
           
            
            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _s3Helper.UnzipAndProcessJsonFromS3(s3Key, tenantCode, bucketName));
        }
    }
}
