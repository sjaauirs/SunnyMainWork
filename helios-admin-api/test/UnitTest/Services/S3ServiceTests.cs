using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Logging;
using Moq;
using SunnyRewards.Helios.Admin.Infrastructure.Helpers.Interface;
using SunnyRewards.Helios.Admin.Infrastructure.Services;
using SunnyRewards.Helios.Admin.Infrastructure.Services.Interfaces;
using Xunit;

namespace SunnyRewards.Helios.Admin.UnitTest.Services
{
    public class S3ServiceTests
    {
        private readonly Mock<ILogger<S3Service>> _loggerMock;
        private readonly Mock<ISecretHelper> _secretHelperMock;
        private readonly Mock<IAmazonS3> _s3ClientMock;
        private readonly Mock<IAmazonS3ClientService> _s3ClientServiceMock;
        private readonly S3Service _s3Service;

        public S3ServiceTests()
        {
            _loggerMock = new Mock<ILogger<S3Service>>();
            _secretHelperMock = new Mock<ISecretHelper>();
            _s3ClientMock = new Mock<IAmazonS3>();
            _s3ClientServiceMock = new Mock<IAmazonS3ClientService>();
            _s3ClientServiceMock.Setup(x => x.GetAmazonS3Client(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<RegionEndpoint>()))
            .Returns(_s3ClientMock.Object);
            _s3Service = new S3Service(_loggerMock.Object, _secretHelperMock.Object, _s3ClientServiceMock.Object);
        }

        [Fact]
        public async System.Threading.Tasks.Task DeleteFolder_ShouldDeleteAllObjectsInFolder()
        {
            // Arrange
            var bucketName = "test-bucket";
            var folderName = "test-folder";
            var s3Objects = new List<S3Object>
            {
                new S3Object { Key = "test-folder/file1.txt" },
                new S3Object { Key = "test-folder/file2.txt" }
            };
            _s3ClientServiceMock.Setup(x => x.GetAmazonS3Client(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<RegionEndpoint>()))
           .Returns(_s3ClientMock.Object);
            _secretHelperMock.Setup(x => x.GetAwsAccessKey()).ReturnsAsync("access-key");
            _secretHelperMock.Setup(x => x.GetAwsSecretKey()).ReturnsAsync("secret-key");

            _s3ClientMock.Setup(x => x.ListObjectsV2Async(It.IsAny<ListObjectsV2Request>(), default))
                .ReturnsAsync(new ListObjectsV2Response { S3Objects = s3Objects });

            _s3ClientMock.Setup(x => x.DeleteObjectAsync(It.IsAny<DeleteObjectRequest>(), default))
                .ReturnsAsync(new DeleteObjectResponse());

            // Act
            await _s3Service.DeleteFolder(bucketName, folderName);

            // Assert
            _s3ClientMock.Verify(x => x.DeleteObjectAsync(It.IsAny<DeleteObjectRequest>(), default), Times.Exactly(s3Objects.Count));
        }

        [Fact]
        public async System.Threading.Tasks.Task GetFileContent_ShouldReturnFileContent()
        {
            // Arrange
            var bucketName = "test-bucket";
            var filePath = "test-folder/file.txt";
            var fileContent = new byte[] { 1, 2, 3, 4, 5 };

            _secretHelperMock.Setup(x => x.GetAwsAccessKey()).ReturnsAsync("access-key");
            _secretHelperMock.Setup(x => x.GetAwsSecretKey()).ReturnsAsync("secret-key");

            _s3ClientMock.Setup(x => x.GetObjectAsync(It.IsAny<GetObjectRequest>(), default))
                .ReturnsAsync(new GetObjectResponse
                {
                    ResponseStream = new MemoryStream(fileContent)
                });
            _s3ClientServiceMock.Setup(x => x.GetAmazonS3Client(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<RegionEndpoint>()))
          .Returns(_s3ClientMock.Object);
            // Act
            var result = await _s3Service.GetFileContent(bucketName, filePath);

            // Assert
            Assert.Equal(fileContent, result);
        }

        [Fact]
        public async System.Threading.Tasks.Task GetListOfFilesInFolder_ShouldReturnListOfFiles()
        {
            // Arrange
            var bucketName = "test-bucket";
            var folderPath = "test-folder";
            var s3Objects = new List<S3Object>
            {
                new S3Object { Key = "test-folder/file1.txt" },
                new S3Object { Key = "test-folder/file2.txt" }
            };

            _secretHelperMock.Setup(x => x.GetAwsAccessKey()).ReturnsAsync("access-key");
            _secretHelperMock.Setup(x => x.GetAwsSecretKey()).ReturnsAsync("secret-key");

            _s3ClientMock.Setup(x => x.ListObjectsV2Async(It.IsAny<ListObjectsV2Request>(), default))
                .ReturnsAsync(new ListObjectsV2Response { S3Objects = s3Objects });
            _s3ClientServiceMock.Setup(x => x.GetAmazonS3Client(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<RegionEndpoint>()))
          .Returns(_s3ClientMock.Object);
            // Act
            var result = await _s3Service.GetListOfFilesInFolder(bucketName, folderPath);

            // Assert
            Assert.Equal(s3Objects.Select(o => o.Key), result);
        }

        [Fact]
        public async System.Threading.Tasks.Task UploadFile_ShouldUploadFileToS3()
        {
            // Arrange
            var bucketName = "test-bucket";
            var fileName = "test-folder/file.txt";
            var content = "file content";
            var contentType = "text/plain";

            _secretHelperMock.Setup(x => x.GetAwsAccessKey()).ReturnsAsync("access-key");
            _secretHelperMock.Setup(x => x.GetAwsSecretKey()).ReturnsAsync("secret-key");

            _s3ClientMock.Setup(x => x.PutObjectAsync(It.IsAny<PutObjectRequest>(), default))
                .ReturnsAsync(new PutObjectResponse());
            _s3ClientServiceMock.Setup(x => x.GetAmazonS3Client(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<RegionEndpoint>()))
          .Returns(_s3ClientMock.Object);
            // Act
            await _s3Service.UploadFile(bucketName, fileName, content, contentType);

            // Assert
            _s3ClientMock.Verify(x => x.PutObjectAsync(It.IsAny<PutObjectRequest>(), default), Times.Once);
        }

        [Fact]
        public async System.Threading.Tasks.Task DownloadZipFile_ShouldReturnMemoryStream()
        {
            // Arrange
            var bucketName = "test-bucket";
            var zipFileName = "test-folder/file.zip";
            var fileContent = new byte[] { 1, 2, 3, 4, 5 };

            _secretHelperMock.Setup(x => x.GetAwsAccessKey()).ReturnsAsync("access-key");
            _secretHelperMock.Setup(x => x.GetAwsSecretKey()).ReturnsAsync("secret-key");

            _s3ClientMock.Setup(x => x.GetObjectAsync(It.IsAny<GetObjectRequest>(), default))
                .ReturnsAsync(new GetObjectResponse
                {
                    ResponseStream = new MemoryStream(fileContent)
                });
            _s3ClientServiceMock.Setup(x => x.GetAmazonS3Client(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<RegionEndpoint>()))
          .Returns(_s3ClientMock.Object);
            // Act
            var result = await _s3Service.DownloadZipFile(bucketName, zipFileName);

            // Assert
            Assert.Equal(fileContent, result.ToArray());
        }

        [Fact]
        public async System.Threading.Tasks.Task ZipFolderAndUpload_ShouldReturnTrue_WhenFilesAreZippedAndUploadedSuccessfully()
        {
            // Arrange
            var bucketName = "test-bucket";
            var folderPrefix = "test-folder/";
            var zipFileName = "test.zip";
            var s3Objects = new List<S3Object>
            {
                new S3Object { Key = "test-folder/file1.txt" },
                new S3Object { Key = "test-folder/file2.txt" }
            };

            _secretHelperMock.Setup(x => x.GetAwsAccessKey()).ReturnsAsync("access-key");
            _secretHelperMock.Setup(x => x.GetAwsSecretKey()).ReturnsAsync("secret-key");

            _s3ClientMock.Setup(x => x.ListObjectsV2Async(It.IsAny<ListObjectsV2Request>(), default))
                .ReturnsAsync(new ListObjectsV2Response { S3Objects = s3Objects });

            _s3ClientMock.Setup(x => x.GetObjectAsync(It.IsAny<GetObjectRequest>(), default))
                .ReturnsAsync(new GetObjectResponse
                {
                    ResponseStream = new MemoryStream(new byte[] { 1, 2, 3, 4, 5 })
                });

            _s3ClientMock.Setup(x => x.PutObjectAsync(It.IsAny<PutObjectRequest>(), default))
                .ReturnsAsync(new PutObjectResponse());

            // Act
            var result = await _s3Service.ZipFolderAndUpload(bucketName, folderPrefix, zipFileName);

            // Assert
            Assert.False(result);
            _s3ClientMock.Verify(x => x.ListObjectsV2Async(It.IsAny<ListObjectsV2Request>(), default), Times.Once);
            _s3ClientMock.Verify(x => x.GetObjectAsync(It.IsAny<GetObjectRequest>(), default), Times.Exactly(s3Objects.Count));
        }

        [Fact]
        public async System.Threading.Tasks.Task ZipFolderAndUpload_ShouldReturnFalse_WhenNoFilesInFolder()
        {
            // Arrange
            var bucketName = "test-bucket";
            var folderPrefix = "test-folder/";
            var zipFileName = "test.zip";

            _secretHelperMock.Setup(x => x.GetAwsAccessKey()).ReturnsAsync("access-key");
            _secretHelperMock.Setup(x => x.GetAwsSecretKey()).ReturnsAsync("secret-key");

            _s3ClientMock.Setup(x => x.ListObjectsV2Async(It.IsAny<ListObjectsV2Request>(), default))
                .ReturnsAsync(new ListObjectsV2Response { S3Objects = new List<S3Object>() });

            // Act
            var result = await _s3Service.ZipFolderAndUpload(bucketName, folderPrefix, zipFileName);

            // Assert
            Assert.False(result);
            _s3ClientMock.Verify(x => x.ListObjectsV2Async(It.IsAny<ListObjectsV2Request>(), default), Times.Once);
            _s3ClientMock.Verify(x => x.GetObjectAsync(It.IsAny<GetObjectRequest>(), default), Times.Never);
            _s3ClientMock.Verify(x => x.PutObjectAsync(It.IsAny<PutObjectRequest>(), default), Times.Never);
        }

        [Fact]
        public async System.Threading.Tasks.Task ZipFolderAndUpload_ShouldReturnFalse_WhenExceptionIsThrown()
        {
            // Arrange
            var bucketName = "test-bucket";
            var folderPrefix = "test-folder/";
            var zipFileName = "test.zip";

            _secretHelperMock.Setup(x => x.GetAwsAccessKey()).ReturnsAsync("access-key");
            _secretHelperMock.Setup(x => x.GetAwsSecretKey()).ReturnsAsync("secret-key");

            _s3ClientMock.Setup(x => x.ListObjectsV2Async(It.IsAny<ListObjectsV2Request>(), default))
                .ThrowsAsync(new Exception("Test exception"));

            // Act
            var result = await _s3Service.ZipFolderAndUpload(bucketName, folderPrefix, zipFileName);

            // Assert
            Assert.False(result);
            _s3ClientMock.Verify(x => x.ListObjectsV2Async(It.IsAny<ListObjectsV2Request>(), default), Times.Once);
            _s3ClientMock.Verify(x => x.GetObjectAsync(It.IsAny<GetObjectRequest>(), default), Times.Never);
            _s3ClientMock.Verify(x => x.PutObjectAsync(It.IsAny<PutObjectRequest>(), default), Times.Never);
        }
    }
}
