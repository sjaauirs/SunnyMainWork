using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Sunny.Benefits.Bff.Core.Constants;
using Sunny.Benefits.Bff.Infrastructure.Helpers;
using Sunny.Benefits.Bff.Infrastructure.Repositories.Interfaces;
using SunnyRewards.Helios.User.Core.Domain.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Sunny.Benefits.Bff.UnitTest.Helpers
{
    public class CommonHelperUnitTest
    {
        private readonly Mock<IUserClient> _userClientMock;
        private readonly Mock<IHttpContextAccessor> _httpContextMock;
        private readonly Mock<ILogger<CommonHelper>> _loggerMock;

        public CommonHelperUnitTest()
        {
            _userClientMock = new Mock<IUserClient>();
            _httpContextMock = new Mock<IHttpContextAccessor>();
            var httpContext = new DefaultHttpContext(); // This is a concrete implementation
            httpContext.Items[HttpContextKeys.JwtConsumerCode] = "Code123";
            _httpContextMock.Setup(x => x.HttpContext).Returns(httpContext);
            _loggerMock = new Mock<ILogger<CommonHelper>>();
        }

        [Fact]
        public async void GetLanguageCode_ConsumerCodeNotSet_ReturnsDefault()
        {
            var helper = new CommonHelper(_userClientMock.Object, _loggerMock.Object, _httpContextMock.Object);

            var result = await helper.GetLanguageCode();

            Assert.Equal(CommonConstants.DefaultLanguageCode, result);
        }

        [Fact]
        public async void GetLanguageCode_ValidResponseWithLanguageCode_ReturnsLanguageCode()
        {
            var expectedLanguageCode = "fr";
            
            _userClientMock
                .Setup(x => x.Post<GetPersonAndConsumerResponseDto>(It.IsAny<string>(), It.IsAny<GetConsumerRequestDto>()))
                .ReturnsAsync(new GetPersonAndConsumerResponseDto ()
                {
                    Person = new PersonDto { LanguageCode = expectedLanguageCode }
                });

            var helper = new CommonHelper(_userClientMock.Object, _loggerMock.Object, _httpContextMock.Object);


            var result = await helper.GetLanguageCode();

            Assert.Equal(expectedLanguageCode, result);
        }

        [Fact]
        public async void GetLanguageCode_NullResponse_ReturnsDefaultLanguageCode()
        {
            _userClientMock
                .Setup(x => x.Post<GetPersonAndConsumerResponseDto>(It.IsAny<string>(), It.IsAny<GetConsumerRequestDto>()))
                .ReturnsAsync((GetPersonAndConsumerResponseDto)null);

            var helper = new CommonHelper(_userClientMock.Object, _loggerMock.Object, _httpContextMock.Object);

            var result = await helper.GetLanguageCode();

            Assert.Equal(CommonConstants.DefaultLanguageCode, result);
        }

        [Fact]
        public async void GetLanguageCode_ExceptionThrown_ReturnsDefaultLanguageCodeAndLogsError()
        {
            _userClientMock
                .Setup(x => x.Post<GetPersonAndConsumerResponseDto>(It.IsAny<string>(), It.IsAny<GetConsumerRequestDto>()))
                .ThrowsAsync(new Exception("API failure"));

            var helper = new CommonHelper(_userClientMock.Object, _loggerMock.Object, _httpContextMock.Object);


            var result = await helper.GetLanguageCode();

            Assert.Equal(CommonConstants.DefaultLanguageCode, result);
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("API - ERROR Msg")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
