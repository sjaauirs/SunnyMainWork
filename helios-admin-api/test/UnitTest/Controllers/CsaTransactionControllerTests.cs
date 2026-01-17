using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using SunnyBenefits.Fis.Core.Domain.Dtos;
using SunnyRewards.Helios.Admin.Api.Controllers;
using SunnyRewards.Helios.Admin.Infrastructure.HttpClients.Interfaces;
using SunnyRewards.Helios.Admin.Infrastructure.Services;
using SunnyRewards.Helios.Admin.UnitTest.Fixtures.MockDtos;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.Wallet.Core.Domain.Dtos;
using Xunit;
using TaskAlias = System.Threading.Tasks.Task;

namespace SunnyRewards.Helios.Admin.UnitTest.Controllers
{
    public class CsaTransactionControllerTests
    {
        private readonly Mock<IFisClient> _fisClient = new();
        private readonly Mock<IWalletClient> _walletClient = new();
        private readonly Mock<ILogger<CsaTransactionController>> _logger = new();
        private readonly Mock<ILogger<CsaTransactionService>> _loggerService = new();
        private readonly CsaTransactionService _transactionService;
        private readonly CsaTransactionController _transactionController;

        public CsaTransactionControllerTests()
        {
            _transactionService = new CsaTransactionService(
                _loggerService.Object,
                _fisClient.Object,
                _walletClient.Object
            );

            _transactionController = new CsaTransactionController(
                _logger.Object,
                _transactionService
            );
        }

        [Fact]
        public async TaskAlias DisposeTransaction_ShouldReturnOkResponse_WhenSuccessful()
        {
            // Arrange
            var requestDto = new CsaTransactionRequestDto();
            _fisClient.Setup(x => x.Post<CsaTransactionResponseDto>(It.IsAny<string>(), It.IsAny<CsaTransactionRequestDto>()))
                      .ReturnsAsync(new CsaTransactionResponseDto { CsaTransactionDto = new CsaTransactionMockDto() });
            _walletClient.Setup(x => x.Post<BaseResponseDto>(It.IsAny<string>(), It.IsAny<CsaWalletTransactionsRequestDto>()))
                         .ReturnsAsync(new BaseResponseDto());
            _fisClient.Setup(x => x.Post<TenantAccountDto>(It.IsAny<string>(), It.IsAny<TenantAccountCreateRequestDto>()))
                      .ReturnsAsync(new TenantAccountDto { TenantConfigJson = "sample json" });

            // Act
            var response = await _transactionController.DisposeCsaTransaction(requestDto);

            // Assert
            Assert.NotNull(response);
            var okObjectResult = Assert.IsType<OkObjectResult>(response);
            Assert.Equal(StatusCodes.Status200OK, okObjectResult.StatusCode);
        }
        [Fact]
        public async TaskAlias DisposeTransaction_ShouldReturnOkResponse_WhenSuccessful_Status_Is_Rejected()
        {
            // Arrange
            var requestDto = new CsaTransactionRequestDto();
            _fisClient.Setup(x => x.Post<CsaTransactionResponseDto>(It.IsAny<string>(), It.IsAny<CsaTransactionRequestDto>()))
                      .ReturnsAsync(new CsaTransactionResponseDto { CsaTransactionDto = new CsaTransactionMockDto() { Status = "Rejected" } });
            _walletClient.Setup(x => x.Post<BaseResponseDto>(It.IsAny<string>(), It.IsAny<CsaWalletTransactionsRequestDto>()))
                         .ReturnsAsync(new BaseResponseDto());
            _fisClient.Setup(x => x.Post<TenantAccountDto>(It.IsAny<string>(), It.IsAny<TenantAccountCreateRequestDto>()))
                      .ReturnsAsync(new TenantAccountDto { TenantConfigJson = "sample json" });

            // Act
            var response = await _transactionController.DisposeCsaTransaction(requestDto);

            // Assert
            Assert.NotNull(response);
            var okObjectResult = Assert.IsType<OkObjectResult>(response);
            Assert.Equal(StatusCodes.Status200OK, okObjectResult.StatusCode);
        }

        [Fact]
        public async TaskAlias DisposeTransaction_ShouldReturnBadRequest_WhenFisReturnsError()
        {
            // Arrange
            var requestDto = new CsaTransactionRequestDto();
            _fisClient.Setup(x => x.Post<CsaTransactionResponseDto>(It.IsAny<string>(), It.IsAny<CsaTransactionRequestDto>()))
                      .ReturnsAsync(new CsaTransactionResponseDto { ErrorCode = StatusCodes.Status400BadRequest });

            // Act
            var response = await _transactionController.DisposeCsaTransaction(requestDto);

            // Assert
            Assert.NotNull(response);
            var objectResult = Assert.IsType<ObjectResult>(response);
            Assert.Equal(StatusCodes.Status400BadRequest, objectResult.StatusCode);
        }
        [Fact]
        public async TaskAlias DisposeTransaction_ShouldReturnBadRequest_WhenWalletReturnsError()
        {
            // Arrange
            var requestDto = new CsaTransactionRequestDto();
            _fisClient.Setup(x => x.Post<CsaTransactionResponseDto>(It.IsAny<string>(), It.IsAny<CsaTransactionRequestDto>()))
                      .ReturnsAsync(new CsaTransactionResponseDto { CsaTransactionDto = new CsaTransactionMockDto() });
            _walletClient.Setup(x => x.Post<BaseResponseDto>(It.IsAny<string>(), It.IsAny<CsaWalletTransactionsRequestDto>()))
                         .ReturnsAsync(new BaseResponseDto() { ErrorCode = StatusCodes.Status404NotFound });

            // Act
            var response = await _transactionController.DisposeCsaTransaction(requestDto);

            // Assert
            Assert.NotNull(response);
            var objectResult = Assert.IsType<ObjectResult>(response);
            Assert.Equal(StatusCodes.Status404NotFound, objectResult.StatusCode);
        }

        [Fact]
        public async TaskAlias DisposeTransaction_ShouldReturnNotFound_WhenTenantConfigIsNull()
        {
            // Arrange
            var requestDto = new CsaTransactionRequestDto();
            _fisClient.Setup(x => x.Post<CsaTransactionResponseDto>(It.IsAny<string>(), It.IsAny<CsaTransactionRequestDto>()))
                      .ReturnsAsync(new CsaTransactionResponseDto() { CsaTransactionDto = new CsaTransactionMockDto() });
            _fisClient.Setup(x => x.Post<TenantAccountDto>(It.IsAny<string>(), It.IsAny<TenantAccountCreateRequestDto>()))
                      .ReturnsAsync(new TenantAccountDto());

            // Act
            var response = await _transactionController.DisposeCsaTransaction(requestDto);

            // Assert
            Assert.NotNull(response);
            var objectResult = Assert.IsType<ObjectResult>(response);
            Assert.Equal(StatusCodes.Status404NotFound, objectResult.StatusCode);
        }

        [Fact]
        public async TaskAlias DisposeTransaction_ShouldReturnInternalServerError_WhenExceptionOccurs()
        {
            // Arrange
            var requestDto = new CsaTransactionRequestDto();
            _fisClient.Setup(x => x.Post<CsaTransactionResponseDto>(It.IsAny<string>(), It.IsAny<CsaTransactionRequestDto>()))
                      .ThrowsAsync(new Exception("Test exception"));

            // Act
            var response = await _transactionController.DisposeCsaTransaction(requestDto);

            // Assert
            Assert.NotNull(response);
            var objectResult = Assert.IsType<ObjectResult>(response);
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
        }
    }
}
