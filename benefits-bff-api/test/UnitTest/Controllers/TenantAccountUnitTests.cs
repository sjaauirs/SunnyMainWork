using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Sunny.Benefits.Bff.Infrastructure.HttpClients.Interfaces;
using Sunny.Benefits.Bff.Infrastructure.Services;
using SunnyBenefits.Fis.Core.Domain.Dtos;
using Xunit;

namespace Sunny.Benefits.Bff.UnitTest.Controllers
{
    public class TenantAccountUnitTests
    {
        private readonly Mock<IFisClient> _mockFisClient;
        private readonly Mock<ILogger<TenantAccountService>> _mockLogger;
        private readonly TenantAccountService _tenantAccountService;

        public TenantAccountUnitTests()
        {
            _mockFisClient = new Mock<IFisClient>();
            _mockLogger = new Mock<ILogger<TenantAccountService>>();
            _tenantAccountService = new TenantAccountService(_mockLogger.Object, _mockFisClient.Object);
        }
        [Fact]
        public async Task GetTenantAccount_ReturnsTenantAccountDto_WhenTenantExists()
        {
            // Arrange
            var tenantAccountRequest = new ExportTenantAccountRequestDto { TenantCode = "tenant123" };
            var expectedTenantAccount = new ExportTenantAccountResponseDto
            {
                TenantAccount = new GetTenantAccountDto
                {
                    AccLoadConfig = "test",
                    FundingConfigJson = "{}",
                    TenantCode = "tenantCode",
                    TenantConfigJson = "{}",
                    TenantAccountCode = "tenantAccountCode"
                }
            };

            _mockFisClient.Setup(client => client.Post<ExportTenantAccountResponseDto>("tenant-account-export", tenantAccountRequest))
                          .ReturnsAsync(expectedTenantAccount);

            // Act
            var result = await _tenantAccountService.GetTenantAccount(tenantAccountRequest);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("tenantCode", result.TenantAccount?.TenantCode);
        }

        [Fact]
        public async Task GetTenantAccount_ReturnsNotFound_WhenTenantAccountIsNull()
        {
            // Arrange
            var tenantAccountRequest = new ExportTenantAccountRequestDto { TenantCode = "tenant123" };

            _mockFisClient.Setup(client => client.Post<ExportTenantAccountResponseDto>("tenant-account-export", tenantAccountRequest));

            // Act
            var result = await _tenantAccountService.GetTenantAccount(tenantAccountRequest);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(StatusCodes.Status404NotFound, result.ErrorCode);
            Assert.Contains("Tenant Account Details Not Found", result.ErrorMessage);
        }

        [Fact]
        public async Task GetTenantAccount_ThrowsException_WhenErrorOccurs()
        {
            // Arrange
            var tenantAccountRequest = new ExportTenantAccountRequestDto { TenantCode = "tenant123" };

            _mockFisClient.Setup(client => client.Post<ExportTenantAccountResponseDto>("tenant-account-export", tenantAccountRequest))
                          .ThrowsAsync(new Exception("Test exception"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _tenantAccountService.GetTenantAccount(tenantAccountRequest));
        }

        [Fact]
        public async Task GetTenantAccount_ReturnsTenantAccount_WhenFound()
        {
            // Arrange
            var requestDto = new TenantAccountCreateRequestDto { TenantCode = "T123" };
            var expectedResponse = new TenantAccountDto { TenantAccountCode = "T1234" };

            _mockFisClient
                .Setup(c => c.Post<TenantAccountDto>("get-tenant-account", requestDto))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _tenantAccountService.GetTenantAccount(requestDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("T1234", result.TenantAccountCode);
        }

        [Fact]
        public async Task GetTenantAccount_ReturnsNotFoundDto_WhenTenantAccountIsNull()
        {
            // Arrange
            var requestDto = new TenantAccountCreateRequestDto { TenantCode = "T123" };

            _mockFisClient
                .Setup(c => c.Post<TenantAccountDto>("get-tenant-account", requestDto))
                .ReturnsAsync((TenantAccountDto)null);

            // Act
            var result = await _tenantAccountService.GetTenantAccount(requestDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(StatusCodes.Status404NotFound, result.ErrorCode);
            Assert.Contains("Tenant Account Details Not Found", result.ErrorMessage);
        }

        [Fact]
        public async Task GetTenantAccount_ThrowsException_WhenFisClientThrows()
        {
            // Arrange
            var requestDto = new TenantAccountCreateRequestDto { TenantCode = "T123" };

            _mockFisClient
                .Setup(c => c.Post<TenantAccountDto>("get-tenant-account", requestDto))
                .ThrowsAsync(new Exception("Something went wrong"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _tenantAccountService.GetTenantAccount(requestDto));
            Assert.Equal("Something went wrong", exception.Message);
        }
    }
}
