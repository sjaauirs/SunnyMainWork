using Microsoft.Extensions.Logging;
using Moq;

using SunnyRewards.Helios.Common.Core.Helpers.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

using Xunit;
using Moq.Protected;
using System.Net;
using NSubstitute;
using Microsoft.AspNetCore.Http;
using SunnyRewards.Helios.User.Infrastructure.Services;
using SunnyRewards.Helios.User.Infrastructure.Helpers.Interface;
using SunnyRewards.Helios.User.Core.Domain.Constant;
using SunnyRewards.Helios.User.Core.Domain.Dtos;
using SunnyRewards.Helios.User.Infrastructure.AWSConfig.Interface;

namespace SunnyRewards.Helios.User.UnitTest.Controllers
{
    public class UploadAgreementPDFServiceTest
    {
        private readonly Mock<ILogger<UploadAgreementPDFService>> _loggerMock = new();
        private readonly Mock<IVault> _vaultMock = new();
        private readonly Mock<IConfiguration> _configurationMock = new();
        private readonly Mock<IAwsConfiguration> _awsconfigurationMock = new();
        private readonly Mock<IS3Helper> _s3HelperMock = new();

        private UploadAgreementPDFService CreateService()
        {
            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Get),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("<html>Mocked HTML</html>")
                });

            var httpClient = new HttpClient(handlerMock.Object);
            // Chrome path section
            var mockChromeSection = new Mock<IConfigurationSection>();
            mockChromeSection.Setup(x => x.Value).Returns("fake_chrome_path");

            // Agreement file path section
            var mockAgreementSection = new Mock<IConfigurationSection>();
            mockAgreementSection.Setup(x => x.Value).Returns("{tenantCode}/{consumerCode}/{fileName}");

            // PathSettings section with nested Agreement and Chrome config
            var mockPathSettingsSection = new Mock<IConfigurationSection>();
            mockPathSettingsSection.Setup(x => x.GetSection(Constant.ChromeFilePath))
                                   .Returns(mockChromeSection.Object);
            mockPathSettingsSection.Setup(x => x.GetSection(Constant.AgreementFilePath))
                                   .Returns(mockAgreementSection.Object);

            // AWS section with nested public bucket
            var mockBucketSection = new Mock<IConfigurationSection>();
            mockBucketSection.Setup(x => x.Value).Returns("my-s3-bucket");

            _configurationMock.Setup(x => x.GetSection("AWS:AWS_PUBLIC_BUCKET_NAME"))
                              .Returns(mockBucketSection.Object);

            // IConfiguration setup
            _configurationMock.Setup(x => x.GetSection(Constant.PathSettings))
                              .Returns(mockPathSettingsSection.Object);

            return new UploadAgreementPDFService(
                _loggerMock.Object,
                _vaultMock.Object,
                _configurationMock.Object,
                _s3HelperMock.Object,
            _awsconfigurationMock.Object
            );
        }
        private UploadAgreementPDFService CreateInvalidService()
        {
            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Get),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("<html>Mocked HTML</html>")
                });

            var httpClient = new HttpClient(handlerMock.Object);
            // Chrome path section
            var mockChromeSection = new Mock<IConfigurationSection>();
            mockChromeSection.Setup(x => x.Value).Returns("fake_chrome_path");

            // Agreement file path section
            var mockAgreementSection = new Mock<IConfigurationSection>();
            mockAgreementSection.Setup(x => x.Value);

            // PathSettings section with nested Agreement and Chrome config
            var mockPathSettingsSection = new Mock<IConfigurationSection>();
            mockPathSettingsSection.Setup(x => x.GetSection(Constant.ChromeFilePath))
                                   .Returns(mockChromeSection.Object);
            mockPathSettingsSection.Setup(x => x.GetSection(Constant.AgreementFilePath))
                                   .Returns(mockAgreementSection.Object);

            // AWS section with nested public bucket
            var mockBucketSection = new Mock<IConfigurationSection>();
            mockBucketSection.Setup(x => x.Value).Returns("my-s3-bucket");

            _configurationMock.Setup(x => x.GetSection("AWS:AWS_PUBLIC_BUCKET_NAME"))
                              .Returns(mockBucketSection.Object);

            // IConfiguration setup
            _configurationMock.Setup(x => x.GetSection(Constant.PathSettings))
                              .Returns(mockPathSettingsSection.Object);

            return new UploadAgreementPDFService(
                _loggerMock.Object,
                _vaultMock.Object,
                _configurationMock.Object,
                _s3HelperMock.Object, _awsconfigurationMock.Object
            );
        }
        [Fact]
        public async Task UploadAgreementPDf_ShouldReturnFileName_WhenSuccess()
        {
            // Arrange
            var service = CreateService();
            var verifyMemberDto = new UpdateOnboardingStateDto
            {LanguageCode="en-US",
                HtmlFileName = new Dictionary<string, string> { { "key", "test" } },
                OnboardingState = Core.Domain.enums.OnboardingState.VERIFIED };
            string tenantCode = "TESTTENANT";
            string consumerCode = "USER123";

            _s3HelperMock.Setup(x => x.GetHtmlFromS3Async(It.IsAny<string>(), It.IsAny<string>()))
                        .ReturnsAsync("testHtml");

            _s3HelperMock.Setup(x => x.UploadFileToS3(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>()))
                         .ReturnsAsync(true);
            _awsconfigurationMock.Setup(x => x.GetAwsPublicS3BucketName())
                         .Returns("public-dev");
            _awsconfigurationMock.Setup(x => x.AgreementPublicFolderPath())
                         .ReturnsAsync("test/{tenant_code}/{language_code},{html_fileName}");

            // Act
            var result = await service.UploadAgreementPDf(verifyMemberDto, tenantCode, consumerCode);

            // Assert
            Assert.NotNull(result);
        }
        [Fact]
        public async Task UploadAgreementPDf_ShouldReturnFileName_When500()
        {
            // Arrange
            var service = CreateService();
            var verifyMemberDto = new UpdateOnboardingStateDto
            {LanguageCode="en-US",
                HtmlFileName = new Dictionary<string, string> { { "key", "test" } },
                OnboardingState = Core.Domain.enums.OnboardingState.VERIFIED };
            string tenantCode = "TESTTENANT";
            string consumerCode = "USER123";

            _s3HelperMock.Setup(x => x.GetHtmlFromS3Async(It.IsAny<string>(), It.IsAny<string>()))
                        .ReturnsAsync("testHtml");

            _s3HelperMock.Setup(x => x.UploadFileToS3(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>()))
                         .ReturnsAsync(true);
            _awsconfigurationMock.Setup(x => x.GetAwsPublicS3BucketName())
                         .Returns("public-dev");
            _awsconfigurationMock.Setup(x => x.AgreementPublicFolderPath())
                         .ThrowsAsync(new Exception ("test"));

            // Act
            var result = await service.UploadAgreementPDf(verifyMemberDto, tenantCode, consumerCode);

            // Assert
            Assert.NotNull(result);
        }
        [Fact]
        public async Task UploadAgreementPDf_ShouldReturnFileName_Wheninvalidurl()
        {
            // Arrange
            var service = CreateService();
            var verifyMemberDto = new UpdateOnboardingStateDto
            {
                HtmlFileName = new Dictionary<string, string> { { "key", "test" } },
                OnboardingState = Core.Domain.enums.OnboardingState.VERIFIED };
            string tenantCode = "TESTTENANT";
            string consumerCode = "USER123";

          
            _s3HelperMock.Setup(x => x.UploadFileToS3(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>()))
                         .ReturnsAsync(true);
            _awsconfigurationMock.Setup(x => x.GetAwsPublicS3BucketName())
                        .Returns("public-dev");
            // Act
            var result = await service.UploadAgreementPDf(verifyMemberDto, tenantCode, consumerCode);

            // Assert
            Assert.NotNull(result);
        }
        [Fact]
        public async Task UploadAgreementPDf_ShouldReturnFileName_WhenErrorOccured()
        {
            // Arrange
            var service = CreateService();
            var verifyMemberDto = new UpdateOnboardingStateDto { HtmlFileName = new Dictionary<string, string> { { "key", "test" } }, OnboardingState = Core.Domain.enums.OnboardingState.VERIFIED };
            string tenantCode = "TESTTENANT";
            string consumerCode = "USER123";            

           

            _s3HelperMock.Setup(x => x.UploadFileToS3(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>()))
                         .ReturnsAsync(true);
            _awsconfigurationMock.Setup(x => x.GetAwsPublicS3BucketName())
                        .Returns("public-dev");

            // Act
            var result = await service.UploadAgreementPDf(verifyMemberDto, tenantCode, consumerCode);

            // Assert
            Assert.NotNull(result);
        }
        [Fact]
        public async Task UploadAgreementPDf_ShouldReturnInvalidFileName_WhenErrorOccured()
        {
            // Arrange
            var service = CreateInvalidService();
            var verifyMemberDto = new UpdateOnboardingStateDto { HtmlFileName = new Dictionary<string, string> { { "key", "test" } }, OnboardingState = Core.Domain.enums.OnboardingState.VERIFIED };
            string tenantCode = "TESTTENANT";
            string consumerCode = "USER123";
          

            _s3HelperMock.Setup(x => x.UploadFileToS3(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>()))
                         .ReturnsAsync(true);
            _awsconfigurationMock.Setup(x => x.GetAwsPublicS3BucketName())
                        .Returns("public-dev");
            // Act
            var result = await service.UploadAgreementPDf(verifyMemberDto, tenantCode, consumerCode);

            // Assert
            Assert.NotNull(result);
        }
    }
}
