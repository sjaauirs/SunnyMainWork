using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Sunny.Benefits.Bff.Core.Constants;
using Sunny.Benefits.Bff.Core.Domain.Dtos;
using Sunny.Benefits.Bff.Infrastructure.Helpers.Interface;
using Sunny.Benefits.Bff.Infrastructure.HttpClients.Interfaces;
using Sunny.Benefits.Bff.Infrastructure.Services;
using Sunny.Benefits.Bff.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.User.Core.Domain.Dtos;
using Xunit;

namespace Sunny.Benefits.Bff.UnitTest.Services
{
    public class ConsumerSummaryServiceTest
    {
        private readonly Mock<ILogger<ConsumerSummaryService>> _loggerMock;
        private readonly Mock<IAuth0Helper> _auth0HelperMock;
        private readonly Mock<ILoginService> _loginServiceMock;
        private readonly Mock<ITenantService> _tenantServiceMock;
        private readonly Mock<IWalletService> _walletServiceMock;
        private readonly Mock<ITaskService> _taskServiceMock;
        private readonly Mock<ITaskClient> _taskClientMock;
        private readonly Mock<IFisClient> _fisClientMock;
        private readonly Mock<ITenantAccountService> _tenantAccountServiceMock;
        private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
        private readonly ConsumerSummaryService _consumerSummaryService;

        public ConsumerSummaryServiceTest()
        {
            _loggerMock = new Mock<ILogger<ConsumerSummaryService>>();
            _auth0HelperMock = new Mock<IAuth0Helper>();
            _loginServiceMock = new Mock<ILoginService>();
            _tenantServiceMock = new Mock<ITenantService>();
            _walletServiceMock = new Mock<IWalletService>();
            _taskServiceMock = new Mock<ITaskService>();
            _taskClientMock = new Mock<ITaskClient>();
            _fisClientMock = new Mock<IFisClient>();
            _tenantAccountServiceMock = new Mock<ITenantAccountService>();
            _httpContextAccessorMock = new Mock<IHttpContextAccessor>();

            _consumerSummaryService = new ConsumerSummaryService(
                _loggerMock.Object,
                _auth0HelperMock.Object,
                _taskClientMock.Object,
                _loginServiceMock.Object,
                _tenantServiceMock.Object,
                _walletServiceMock.Object,
                _taskServiceMock.Object,
                _tenantAccountServiceMock.Object,
                _fisClientMock.Object,
                _httpContextAccessorMock.Object
            );
        }

        [Fact]
        public async Task GetConsumerDetails_ShouldReturnConsumerDetails_WhenValidConsumerCodeProvided()
        {
            // Arrange
            var consumerCode = "consumer123";
            var consumerSummaryRequestDto = new ConsumerSummaryRequestDto { consumerCode = consumerCode };

            var consumerByEmailResponse = new GetConsumerByEmailResponseDto
            {
                Consumer = new ConsumerDto[] { new ConsumerDto { ConsumerCode = consumerCode } },
                Person = new PersonDto { PersonId = 123 }
            };

            _httpContextAccessorMock.Setup(x => x.HttpContext).Returns((HttpContext)null);
            _loginServiceMock.Setup(x => x.GetPersonAndConsumerDetails(consumerCode))
                .ReturnsAsync(consumerByEmailResponse);

            // Act
            var result = await _consumerSummaryService.GetConsumerDetails(consumerSummaryRequestDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(consumerCode, result.Consumer.FirstOrDefault()?.ConsumerCode);
            Assert.Equal(123, result.Person?.PersonId);
            _loginServiceMock.Verify(x => x.GetPersonAndConsumerDetails(consumerCode), Times.Once);
        }

        [Fact]
        public async Task GetConsumerDetails_ShouldReturnCachedConsumerDetails_WhenCachedDataExists()
        {
            // Arrange
            var consumerCode = "consumer123";
            var consumerSummaryRequestDto = new ConsumerSummaryRequestDto { consumerCode = consumerCode };

            var cachedConsumerDetails = new GetConsumerByPersonUniqueIdentifierResponseDto
            {
                Consumer = new ConsumerDto[] { new ConsumerDto { ConsumerCode = consumerCode } },
                Person = new PersonDto { PersonId = 123 }
            };

            var httpContextMock = new Mock<HttpContext>();
            var items = new Dictionary<object, object>
            {
                { HttpContextKeys.ConsumerInfo, cachedConsumerDetails }
            };
            httpContextMock.Setup(x => x.Items).Returns(items);
            _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContextMock.Object);

            // Act
            var result = await _consumerSummaryService.GetConsumerDetails(consumerSummaryRequestDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(consumerCode, result.Consumer.FirstOrDefault()?.ConsumerCode);
            Assert.Equal(123, result.Person?.PersonId);
            _loginServiceMock.Verify(x => x.GetPersonAndConsumerDetails(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task GetConsumerDetails_ShouldReturnBadRequest_WhenConsumerCodeIsNullOrEmpty()
        {
            // Arrange
            var consumerSummaryRequestDto = new ConsumerSummaryRequestDto { consumerCode = null };

            // Act
            var result = await _consumerSummaryService.GetConsumerDetails(consumerSummaryRequestDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(StatusCodes.Status400BadRequest, result.ErrorCode);
            Assert.Equal("Invalid Input Data, ConsumerCode is Required", result.ErrorMessage);
            _loginServiceMock.Verify(x => x.GetPersonAndConsumerDetails(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task GetConsumerDetails_ShouldReturnBadRequest_WhenConsumerCodeIsWhitespace()
        {
            // Arrange
            var consumerSummaryRequestDto = new ConsumerSummaryRequestDto { consumerCode = "   " };

            // Act
            var result = await _consumerSummaryService.GetConsumerDetails(consumerSummaryRequestDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(StatusCodes.Status400BadRequest, result.ErrorCode);
            Assert.Equal("Invalid Input Data, ConsumerCode is Required", result.ErrorMessage);
        }

        [Fact]
        public async Task GetConsumerDetails_ShouldThrowException_WhenApiResponseHasErrorCode()
        {
            // Arrange
            var consumerCode = "consumer123";
            var consumerSummaryRequestDto = new ConsumerSummaryRequestDto { consumerCode = consumerCode };

            var errorResponse = new GetConsumerByEmailResponseDto
            {
                ErrorCode = StatusCodes.Status404NotFound,
                ErrorMessage = "Consumer not found"
            };

            _httpContextAccessorMock.Setup(x => x.HttpContext).Returns((HttpContext)null);
            _loginServiceMock.Setup(x => x.GetPersonAndConsumerDetails(consumerCode))
                .ReturnsAsync(errorResponse);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidProgramException>(
                () => _consumerSummaryService.GetConsumerDetails(consumerSummaryRequestDto));
            Assert.Equal("Consumer not found", exception.Message);
        }

        [Fact]
        public async Task GetConsumerDetails_ShouldThrowInvalidProgramException_WhenLoginServiceThrowsException()
        {
            // Arrange
            var consumerCode = "consumer123";
            var consumerSummaryRequestDto = new ConsumerSummaryRequestDto { consumerCode = consumerCode };

            _httpContextAccessorMock.Setup(x => x.HttpContext).Returns((HttpContext)null);
            _loginServiceMock.Setup(x => x.GetPersonAndConsumerDetails(consumerCode))
                .ThrowsAsync(new Exception("Network error"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidProgramException>(
                () => _consumerSummaryService.GetConsumerDetails(consumerSummaryRequestDto));
        }

        [Fact]
        public async Task GetConsumerDetails_ShouldReturnValidResponse_WithCompleteConsumerAndPersonData()
        {
            // Arrange
            var consumerCode = "consumer456";
            var consumerSummaryRequestDto = new ConsumerSummaryRequestDto { consumerCode = consumerCode };

            var consumerByEmailResponse = new GetConsumerByEmailResponseDto
            {
                Consumer = new ConsumerDto[]
                {
                    new ConsumerDto 
                    { 
                        ConsumerCode = consumerCode,
                        TenantCode = "tenant123"
                    }
                },
                Person = new PersonDto 
                { 
                    PersonId = 456,
                    Email = "test@example.com"
                }
            };

            _httpContextAccessorMock.Setup(x => x.HttpContext).Returns((HttpContext)null);
            _loginServiceMock.Setup(x => x.GetPersonAndConsumerDetails(consumerCode))
                .ReturnsAsync(consumerByEmailResponse);

            // Act
            var result = await _consumerSummaryService.GetConsumerDetails(consumerSummaryRequestDto);

            // Assert
            Assert.NotNull(result);
            Assert.Null(result.ErrorCode);
            Assert.NotNull(result.Consumer);
            Assert.Single(result.Consumer);
            Assert.Equal("tenant123", result.Consumer.FirstOrDefault()?.TenantCode);
            Assert.NotNull(result.Person);
            Assert.Equal("test@example.com", result.Person.Email);
        }
    }
}