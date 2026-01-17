using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using SunnyRewards.Helios.Admin.Api.Controllers;
using SunnyRewards.Helios.Admin.Infrastructure.HttpClients.Interfaces;
using SunnyRewards.Helios.Admin.Infrastructure.Services;
using SunnyRewards.Helios.Admin.UnitTest.Fixtures.MockDtos;
using SunnyRewards.Helios.Admin.UnitTest.Fixtures.MockModels;
using SunnyRewards.Helios.Wallet.Core.Domain.Dtos;
using Xunit;
using TaskAlias = System.Threading.Tasks.Task;
namespace SunnyRewards.Helios.Admin.UnitTest.Controllers
{
    public class ConsumerWalletControllerTest
    {
        private readonly ConsumerWalletController _consumerWalletController;
        private readonly Mock<ILogger<ConsumerWalletController>> _logger;
        private readonly ConsumerWalletService _service;
        private readonly Mock<IWalletClient> _walletClientMock;

        public ConsumerWalletControllerTest()
        {
            _walletClientMock = new Mock<IWalletClient>();
            _logger = new Mock<ILogger<ConsumerWalletController>>();
            _service = new ConsumerWalletService(_walletClientMock.Object);
            _consumerWalletController = new ConsumerWalletController(_logger.Object, _service);
        }

        [Fact]
        public async TaskAlias GetAllConsumerWallets_ReturnsOk_WhenWalletsRetrievedSuccessfully()
        {
            // Arrange
            var requestDto = new GetConsumerWalletRequestDto
            {
                TenantCode = "test-tenant",
                ConsumerCode = "test-consumer"
            };

            _walletClientMock.Setup(x => x.Post<ConsumerWalletResponseDto>(It.IsAny<string>(), It.IsAny<GetConsumerWalletRequestDto>()))
                .ReturnsAsync(new ConsumerWalletResponseDto());

            // Act
            var result = await _consumerWalletController.GetAllConsumerWallets(requestDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.IsType<ConsumerWalletResponseDto>(okResult.Value);
            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
        }

        [Fact]
        public async TaskAlias GetAllConsumerWallets_ReturnsErrorCode_WhenServiceReturnsError()
        {
            // Arrange
            var requestDto = new GetConsumerWalletRequestDto
            {
                TenantCode = "test-tenant",
                ConsumerCode = "test-consumer"
            };

            var responseDto = new ConsumerWalletResponseDto
            {
                ErrorCode = StatusCodes.Status400BadRequest,
                ErrorMessage = "TenantCode mismatch for ConsumerCode"
            };
            _walletClientMock.Setup(x => x.Post<ConsumerWalletResponseDto>(It.IsAny<string>(), It.IsAny<GetConsumerWalletRequestDto>()))
                .ReturnsAsync(responseDto);

            // Act
            var result = await _consumerWalletController.GetAllConsumerWallets(requestDto);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(StatusCodes.Status400BadRequest, objectResult.StatusCode);
            var actualResponse = Assert.IsType<ConsumerWalletResponseDto>(objectResult.Value);
            Assert.Equal(StatusCodes.Status400BadRequest, actualResponse.ErrorCode);
        }

        [Fact]
        public async TaskAlias GetAllConsumerWallets_ReturnsInternalServerError_WhenExceptionIsThrown()
        {
            // Arrange
            var requestDto = new GetConsumerWalletRequestDto
            {
                TenantCode = "test-tenant",
                ConsumerCode = "test-consumer"
            };
            _walletClientMock.Setup(x => x.Post<ConsumerWalletResponseDto>(It.IsAny<string>(), It.IsAny<GetConsumerWalletRequestDto>()))
                .ThrowsAsync(new Exception("Testing"));

            // Act
            var result = await _consumerWalletController.GetAllConsumerWallets(requestDto);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
            var actualResponse = Assert.IsType<ConsumerWalletResponseDto>(objectResult.Value);
            Assert.Equal(StatusCodes.Status500InternalServerError, actualResponse.ErrorCode);
        }
        [Fact]
        public async TaskAlias GetConsumerWalletsByWalletType_ReturnsOk()
        {
            // Arrange
            var requestDto = new FindConsumerWalletByWalletTypeRequestMockDto();
            var expectedModel = new ConsumerWalletMockModel();
            _walletClientMock.Setup(x => x.Post<FindConsumerWalletResponseDto>(It.IsAny<string>(), It.IsAny<FindConsumerWalletByWalletTypeRequestDto>()))
                        .ReturnsAsync(new FindConsumerWalletResponseDto());

            // Act
            var result = await _consumerWalletController.GetConsumerWalletsByWalletType(requestDto);

            // Assert
            Assert.IsType<ActionResult<FindConsumerWalletResponseDto>>(result);
            var actionResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(StatusCodes.Status200OK, actionResult.StatusCode);
        }

        [Fact]
        public async TaskAlias GetConsumerWalletsByWalletType_ReturnsWalletTypeNotFound()
        {
            // Arrange
            var requestDto = new FindConsumerWalletByWalletTypeRequestMockDto();
            var expectedResponseDto = new FindConsumerWalletResponseDto
            {
                ErrorCode = StatusCodes.Status404NotFound,
                ErrorMessage = "Wallet type not found"
            };

            _walletClientMock.Setup(x => x.Post<FindConsumerWalletResponseDto>(It.IsAny<string>(), It.IsAny<FindConsumerWalletByWalletTypeRequestDto>()))
                            .ReturnsAsync(expectedResponseDto);

            // Act
            var result = await _consumerWalletController.GetConsumerWalletsByWalletType(requestDto);

            // Assert
            Assert.IsType<ObjectResult>(result.Result);
            var notFoundResult = result.Result as ObjectResult;
            var responseDto = notFoundResult?.Value as FindConsumerWalletResponseDto;
            Assert.Equal(expectedResponseDto.ErrorCode, responseDto?.ErrorCode);
            Assert.Equal(expectedResponseDto.ErrorMessage, responseDto?.ErrorMessage);
        }

        [Fact]
        public async TaskAlias GetConsumerWalletsByWalletType_ReturnsConsumerWalletNotFound()
        {
            // Arrange
            var requestDto = new FindConsumerWalletByWalletTypeRequestMockDto();

            var expectedResponseDto = new FindConsumerWalletResponseDto
            {
                ErrorCode = 404,
                ErrorMessage = "Consumer wallet not found"
            };
            _walletClientMock.Setup(x => x.Post<FindConsumerWalletResponseDto>(It.IsAny<string>(), It.IsAny<FindConsumerWalletByWalletTypeRequestDto>()))
                    .ReturnsAsync(expectedResponseDto);

            // Act
            var result = await _consumerWalletController.GetConsumerWalletsByWalletType(requestDto);

            // Assert
            Assert.IsType<ObjectResult>(result.Result);
            var notFoundResult = result.Result as ObjectResult;
            var responseDto = notFoundResult?.Value as FindConsumerWalletResponseDto;
            Assert.Equal(expectedResponseDto.ErrorCode, responseDto?.ErrorCode);
            Assert.Equal(expectedResponseDto.ErrorMessage, responseDto?.ErrorMessage);
        }

        [Fact]
        public async TaskAlias GetConsumerWalletsByWalletType_ReturnsInternalServerError()
        {
            // Arrange
            var requestDto = new FindConsumerWalletByWalletTypeRequestMockDto();
            var exceptionMessage = "Test exception message";
            _walletClientMock.Setup(x => x.Post<FindConsumerWalletResponseDto>(It.IsAny<string>(), It.IsAny<FindConsumerWalletByWalletTypeRequestMockDto>()))
                    .ThrowsAsync(new Exception(exceptionMessage));

            // Act
            var result = await _consumerWalletController.GetConsumerWalletsByWalletType(requestDto);

            // Assert
            Assert.IsType<ObjectResult>(result.Result);
            var objectResult = result.Result as ObjectResult;
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult?.StatusCode);
            Assert.IsType<FindConsumerWalletResponseDto>(objectResult?.Value);
            var responseDto = objectResult.Value as FindConsumerWalletResponseDto;
            Assert.Equal(exceptionMessage, responseDto?.ErrorMessage);
        }
        [Fact]
        public async TaskAlias Should_Post_Consumer_Wallets()
        {
            // Arrange
            var response = new List<ConsumerWalletDataResponseDto>() {
                                new ConsumerWalletDataResponseDto()
                                {
                                    Wallet = new WalletDto()
                                    {
                                        WalletCode ="40891787891"
                                    }
                                }
                            };
            var consumerWalletDataMockDto = new ConsumerWalletDataMockDto();
            _walletClientMock.Setup(x => x.Post<List<ConsumerWalletDataResponseDto>>(It.IsAny<string>(), It.IsAny<List<ConsumerWalletDataDto>>()))
                            .ReturnsAsync(response);
            // Act
            var consumerWallets = await _consumerWalletController.PostConsumerWallets(new List<ConsumerWalletDataDto> { consumerWalletDataMockDto });

            // Assert
            var result = consumerWallets.Result as OkObjectResult;
            Assert.NotNull(result?.Value);
            Assert.Equal(StatusCodes.Status200OK, result?.StatusCode);
        }

        [Fact]
        public async TaskAlias Catch_Exception_Post_Consumer_Wallets_Service()
        {
            // Arrange
            var consumerWalletDataMockDto = new ConsumerWalletDataMockDto();
            _walletClientMock.Setup(x => x.Post<List<ConsumerWalletDataResponseDto>>(It.IsAny<string>(), It.IsAny<IList<ConsumerWalletDataDto>>()))
                            .ThrowsAsync(new Exception("testing"));

            // Act
            var response = await _consumerWalletController.PostConsumerWallets(new List<ConsumerWalletDataDto> { consumerWalletDataMockDto });

            // Assert
            var result = response.Result as ObjectResult;
            Assert.Equal(StatusCodes.Status500InternalServerError, result?.StatusCode);
        }
    }
}
