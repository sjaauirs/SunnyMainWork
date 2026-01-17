using Amazon.S3.Model;
using Amazon.S3;
using Microsoft.Extensions.Logging;
using Moq;
using SunnyRewards.Helios.Common.Core.Helpers.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Microsoft.Extensions.Configuration;
using Amazon;
using SunnyRewards.Helios.User.Infrastructure.Helpers;
using SunnyRewards.Helios.User.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.User.Infrastructure.AWSConfig.Interface;

namespace SunnyRewards.Helios.User.UnitTest.Helpers
{
    public class S3HelperTests
    {
        private readonly Mock<ILogger<S3Helper>> _loggerMock = new();
        private readonly Mock<IAmazonS3ClientService> _amazonClientServiceMock = new();
        private readonly Mock<IAmazonS3> _amazonS3Mock = new();
        private readonly Mock<IVault> _vaultMock = new();
        private readonly Mock<IAwsConfiguration> _awsconfigurationMock = new();
        private readonly Mock<IConfiguration> _configMock = new();

        [Fact]
        public async Task UploadFileToS3_ShouldCallPutObjectAsync_WhenInValidInput()
        {
            // Arrange
            var fakeBucket = "test-bucket";
            var fakeFile = "test.pdf";
            var fakeStream = new MemoryStream(new byte[] { 1, 2, 3 });
            var mockBucketSection = new Mock<IConfigurationSection>();
            mockBucketSection.Setup(x => x.Value).Returns("my-s3-bucket");

            _configMock.Setup(x => x.GetSection("AWS:AWS_ACCESS_KEY_NAME"))
                              .Returns(mockBucketSection.Object);
            var mockAWSSection = new Mock<IConfigurationSection>();
            mockBucketSection.Setup(x => x.Value).Returns("my-sy-bucket");

            _configMock.Setup(x => x.GetSection("AWS:AWS_SECRET_KEY_NAME"))
                              .Returns(mockBucketSection.Object);
            _amazonS3Mock.Setup(x => x.PutObjectAsync(It.IsAny<PutObjectRequest>(), It.IsAny<CancellationToken>()))
                         .ReturnsAsync(new PutObjectResponse());

            _amazonClientServiceMock.Setup(x =>
                x.GetAmazonS3Client(It.IsAny<string>(), It.IsAny<string>(), RegionEndpoint.USEast2))
                .Returns(_amazonS3Mock.Object);

            var s3Helper = new S3Helper(
                _loggerMock.Object,
                _vaultMock.Object,
                _configMock.Object,
                _amazonClientServiceMock.Object, _awsconfigurationMock.Object
            );
            _awsconfigurationMock.Setup(x => x.GetAwsAccessKey())
                     .ReturnsAsync("public-dev");
            _awsconfigurationMock.Setup(x => x.GetAwsSecretKey())
                       .ReturnsAsync("public-devs");

            // Act
            await s3Helper.UploadFileToS3(fakeStream, fakeBucket, fakeFile);

            // Assert
            _amazonS3Mock.Verify(x =>
                x.PutObjectAsync(It.Is<PutObjectRequest>(r =>
                    r.BucketName == fakeBucket &&
                    r.Key == fakeFile &&
                    r.ContentType == "application/pdf"
                ), It.IsAny<CancellationToken>()),
                Times.Once);
        }
        [Fact]
        public async Task UploadFileToS3_ShouldCallPutObjectAsync_WhenValidInput()
        {
            // Arrange
            var fakeBucket = "test-bucket";
            var fakeFile = "test.pdf";
            var fakeStream = new MemoryStream(new byte[] { 1, 2, 3 });
            var mockBucketSection = new Mock<IConfigurationSection>();
            mockBucketSection.Setup(x => x.Value).Returns("my-s3-bucket");

            _configMock.Setup(x => x.GetSection("AWS:AWS_ACCESS_KEY_NAME"))
                              .Returns(mockBucketSection.Object);
            var mockAWSSection = new Mock<IConfigurationSection>();
            mockBucketSection.Setup(x => x.Value).Returns("my-sy-bucket");

            _configMock.Setup(x => x.GetSection("AWS:AWS_SECRET_KEY_NAME"))
                              .Returns(mockBucketSection.Object);
            _amazonS3Mock.Setup(x => x.PutObjectAsync(It.IsAny<PutObjectRequest>(), It.IsAny<CancellationToken>()))
                         .ReturnsAsync(new PutObjectResponse { HttpStatusCode=System.Net.HttpStatusCode.OK});

            _amazonClientServiceMock.Setup(x =>
                x.GetAmazonS3Client(It.IsAny<string>(), It.IsAny<string>(), RegionEndpoint.USEast2))
                .Returns(_amazonS3Mock.Object);

            var s3Helper = new S3Helper(
                _loggerMock.Object,
                _vaultMock.Object,
                _configMock.Object,
                _amazonClientServiceMock.Object, _awsconfigurationMock.Object
            );
            _awsconfigurationMock.Setup(x => x.GetAwsAccessKey())
                     .ReturnsAsync("public-dev");
            _awsconfigurationMock.Setup(x => x.GetAwsSecretKey())
                       .ReturnsAsync("public-devs");
            // Act
            await s3Helper.UploadFileToS3(fakeStream, fakeBucket, fakeFile);

            // Assert
            _amazonS3Mock.Verify(x =>
                x.PutObjectAsync(It.Is<PutObjectRequest>(r =>
                    r.BucketName == fakeBucket &&
                    r.Key == fakeFile &&
                    r.ContentType == "application/pdf"
                ), It.IsAny<CancellationToken>()),
                Times.Once);
        }
        [Fact]
        public async Task UploadFileToS3_ShouldCallexception_WhenInValidInput()
        {
            // Arrange
            var fakeBucket = "test-bucket";
            var fakeFile = "test.pdf";
            var fakeStream = new MemoryStream(new byte[] { 1, 2, 3 });
            var mockBucketSection = new Mock<IConfigurationSection>();
            mockBucketSection.Setup(x => x.Value).Throws(new InvalidOperationException("Simulated exception"));

            _configMock.Setup(x => x.GetSection("AWS:AWS_ACCESS_KEY_NAME"))
                              .Returns(mockBucketSection.Object);
            var mockAWSSection = new Mock<IConfigurationSection>();
            mockBucketSection.Setup(x => x.Value).Returns("my-sy-bucket");

            _configMock.Setup(x => x.GetSection("AWS:AWS_SECRET_KEY_NAME"))
                              .Returns(mockBucketSection.Object);
            _amazonS3Mock.Setup(x => x.PutObjectAsync(It.IsAny<PutObjectRequest>(), It.IsAny<CancellationToken>()))
                         .ThrowsAsync(new AmazonS3Exception("Simulated exception"));

            _amazonClientServiceMock.Setup(x =>
                x.GetAmazonS3Client(It.IsAny<string>(), It.IsAny<string>(), RegionEndpoint.USEast2))
                .Returns(_amazonS3Mock.Object);
            _awsconfigurationMock.Setup(x => x.GetAwsAccessKey())
                     .ReturnsAsync("public-dev");
            _awsconfigurationMock.Setup(x => x.GetAwsSecretKey())
                       .ReturnsAsync("public-devs");
            var s3Helper = new S3Helper(
                _loggerMock.Object,
                _vaultMock.Object,
                _configMock.Object,
                _amazonClientServiceMock.Object, _awsconfigurationMock.Object
            );

            // Act
            await s3Helper.UploadFileToS3(fakeStream, fakeBucket, fakeFile);

            // Assert
            _amazonS3Mock.Verify(x =>
                x.PutObjectAsync(It.Is<PutObjectRequest>(r =>
                    r.BucketName == fakeBucket &&
                    r.Key == fakeFile &&
                    r.ContentType == "application/pdf"
                ), It.IsAny<CancellationToken>()),
                Times.Once);
        }
        [Fact]
        public async Task UploadFileToS3_ShouldCallGetObjectAsync_WhenValidInput()
        {
            // Arrange
            var fakeBucket = "test-bucket";
            var fakeFile = "test.pdf";
            var fakeStream = new MemoryStream(new byte[] { 1, 2, 3 });
            var mockBucketSection = new Mock<IConfigurationSection>();
            mockBucketSection.Setup(x => x.Value).Returns("my-s3-bucket");

            _configMock.Setup(x => x.GetSection("AWS:AWS_ACCESS_KEY_NAME"))
                              .Returns(mockBucketSection.Object);
            var mockAWSSection = new Mock<IConfigurationSection>();
            mockBucketSection.Setup(x => x.Value).Returns("my-sy-bucket");

            _configMock.Setup(x => x.GetSection("AWS:AWS_SECRET_KEY_NAME"))
                              .Returns(mockBucketSection.Object);
            var htmlContent = "<html><body><h1>Test HTML</h1></body></html>";
            var htmlStream = new MemoryStream(Encoding.UTF8.GetBytes(htmlContent));

            // Mock GetObjectAsync to return the stream
            _amazonS3Mock.Setup(x =>
                x.GetObjectAsync( It.IsAny<GetObjectRequest>(),  It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetObjectResponse
                {
                    ResponseStream = htmlStream
                });
            _awsconfigurationMock.Setup(x => x.GetAwsAccessKey())
                       .ReturnsAsync("public-dev");
            _awsconfigurationMock.Setup(x => x.GetAwsSecretKey())
                       .ReturnsAsync("public-devs");
            _amazonClientServiceMock.Setup(x =>
                x.GetAmazonS3Client(It.IsAny<string>(), It.IsAny<string>(), RegionEndpoint.USEast2))
                .Returns(_amazonS3Mock.Object);

            var s3Helper = new S3Helper(
                _loggerMock.Object,
                _vaultMock.Object,
                _configMock.Object,
                _amazonClientServiceMock.Object, _awsconfigurationMock.Object
            );

            // Act
            await s3Helper.GetHtmlFromS3Async(It.IsAny<string>(), It.IsAny<string>());

            // Assert
            _amazonS3Mock.Verify(x =>
                x.GetObjectAsync(It.Is<GetObjectRequest>(r =>
                    r.BucketName == fakeBucket &&
                    r.Key == fakeFile 
                ), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
