using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NHibernate;
using SunnyRewards.Helios.Common.Core.Repositories;
using SunnyRewards.Helios.Wallet.Api.Controllers;
using SunnyRewards.Helios.Wallet.Core.Domain.Dtos;
using SunnyRewards.Helios.Wallet.Core.Domain.Models;
using SunnyRewards.Helios.Wallet.Infrastructure.Repositories;
using SunnyRewards.Helios.Wallet.Infrastructure.Repositories.Interfaces;
using SunnyRewards.Helios.Wallet.Infrastructure.Services;
using SunnyRewards.Helios.Wallet.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.Wallet.UnitTest.Fixtures.MockDto;
using SunnyRewards.Helios.Wallet.UnitTest.Fixtures.MockModel;
using SunnyRewards.Helios.Wallet.UnitTest.Fixtures.MockRepositories;
using System.Linq.Expressions;
using Xunit;

namespace SunnyRewards.Helios.Wallet.UnitTest.Controllers
{
    public class ConsumerWalletControllerUnitTest
    {

        private readonly IConsumerWalletService _consumerWalletService;
        private readonly IMapper _mapper;
        private readonly Mock<ILogger<ConsumerWalletController>> _loggerMock;
        private readonly Mock<ILogger<ConsumerWalletService>> _consumerWalletServiceLogger;
        private readonly Mock<IConsumerWalletRepo> _consumerWalletRepository;
        private readonly Mock<IWalletRepo> _walletRepository;
        private readonly Mock<IWalletTypeRepo> _walletTypeRepository;
        private readonly Mock<NHibernate.ISession> _session;
        private readonly ConsumerWalletController _consumerWalletController;
        public ConsumerWalletControllerUnitTest()
        {
            _loggerMock = new Mock<ILogger<ConsumerWalletController>>();
            _consumerWalletServiceLogger = new Mock<ILogger<ConsumerWalletService>>();
            _consumerWalletRepository = new ConsumerWalletMockRepo();
            _walletRepository = new WalletMockRepo();
            _walletTypeRepository = new WalletTypeMockRepo();
            _session = new Mock<NHibernate.ISession>();
            _mapper = new Mapper(new MapperConfiguration(
                configure =>
                {
                    configure.AddMaps(typeof(Infrastructure.Mappings.MappingProfile.ConsumerWalletMapping).Assembly.FullName);
                }));
            _consumerWalletService = new ConsumerWalletService(_consumerWalletServiceLogger.Object, _consumerWalletRepository.Object,
                _mapper, _walletRepository.Object, _walletTypeRepository.Object, _session.Object);
            _consumerWalletController = new ConsumerWalletController(_loggerMock.Object, _consumerWalletService);
        }
        [Fact]
        public async Task GetAllConsumerRedeemableWallets_ReturnsOk_WhenWalletsRetrievedSuccessfully()
        {
            // Arrange
            FindConsumerWalletRequestMockDto consumerWalletRequestMockDto = new FindConsumerWalletRequestMockDto();

            _consumerWalletRepository
                .Setup(svc => svc.GetConsumerAllWallets(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new List<ConsumerWalletDetailsModel>() { new ConsumerWalletDetailsModel() });

            // Act
            var result = await _consumerWalletController.GetAllConsumerRedeemableWallets(consumerWalletRequestMockDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var actualResponse = Assert.IsType<ConsumerWalletResponseDto>(okResult.Value);
            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
        }

        [Fact]
        public async Task GetAllConsumerRedeemableWallets_ReturnsInternalServerError_WhenExceptionIsThrown()
        {
            // Arrange
            var requestDto = new FindConsumerWalletRequestDto
            {
                ConsumerCode = "test-consumer1"
            };

            _consumerWalletRepository
                .Setup(x => x.GetConsumerWalletsWithDetails(
                    It.IsAny<string>(),
                    It.IsAny<bool>(),
                    It.IsAny<long?>()))
                .ThrowsAsync(new Exception("Repository Exception"));

            // Act
            var result = await _consumerWalletController
                .GetAllConsumerRedeemableWallets(requestDto);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);

            var actualResponse =
                Assert.IsType<ConsumerWalletResponseDto>(objectResult.Value);

            Assert.Equal(
                StatusCodes.Status500InternalServerError,
                actualResponse.ErrorCode);
        }


        [Fact]
        public async Task GetAllConsumerRedeemableWallets_Returns404_WhenNoWalletsFound()
        {
            // Arrange
            var requestDto = new FindConsumerWalletRequestDto
            {
                ConsumerCode = "test-consumer2"
            };

            _consumerWalletRepository
                .Setup(x => x.GetConsumerWalletsWithDetails(
                    It.IsAny<string>(),
                    It.IsAny<bool>(),
                    It.IsAny<long?>()))
                .ReturnsAsync(new List<ConsumerWalletDetailsModel>()); // EMPTY

            // Act
            var result = await _consumerWalletController
                .GetAllConsumerRedeemableWallets(requestDto);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(StatusCodes.Status404NotFound, objectResult.StatusCode);

            var response =
                Assert.IsType<ConsumerWalletResponseDto>(objectResult.Value);

            Assert.Equal(StatusCodes.Status404NotFound, response.ErrorCode);
        }

        [Fact]
        public async Task Should_Get_ConsumerWallet()
        {
            FindConsumerWalletRequestMockDto consumerWalletRequestMockDto = new FindConsumerWalletRequestMockDto();
            var consumerWalletMockDto = await _consumerWalletController.FindConsumerWallet(consumerWalletRequestMockDto);
            var result = consumerWalletMockDto.Result as OkObjectResult;
            Assert.True(result?.Value != null);
            Assert.True(result.StatusCode == 200);
        }


        [Fact]
        public async Task Should_Not_Get_Consumer()
        {
            FindConsumerWalletRequestMockDto consumerWalletRequestMockDto = new FindConsumerWalletRequestMockDto();
            _consumerWalletRepository.Setup(x => x.FindAsync(It.IsAny<Expression<Func<ConsumerWalletModel, bool>>>(), false)).ReturnsAsync(new List<ConsumerWalletModel>());
            var response = await _consumerWalletController.FindConsumerWallet(consumerWalletRequestMockDto);
            var result = response.Result as BadRequestObjectResult;
            Assert.True(result?.Value == null);
        }

        [Fact]
        public async Task Should_Return_Catch_Exception_ConsumerWallet()
        {
            var consumerLogger = new Mock<ILogger<ConsumerWalletController>>();
            var consumerService = new Mock<IConsumerWalletService>();
            var consumerWalletController = new ConsumerWalletController(consumerLogger.Object, consumerService.Object);
            var consumerWalletRequestMockDto = new FindConsumerWalletRequestMockDto();
            consumerService.Setup(x => x.GetConsumerWallet(It.IsAny<FindConsumerWalletRequestMockDto>())).
                         ThrowsAsync(new Exception("ConsumerWallet Exception"));
            var response = await consumerWalletController.FindConsumerWallet(consumerWalletRequestMockDto);
            var result = response.Result as ObjectResult;
            Assert.True(result == null);
        }

        [Fact]
        public async Task GetConsumerWallet_ExceptionHandling_Test()
        {
            var consumerWalletRepo = new Mock<IConsumerWalletRepo>();
            var consumerWalletLogger = new Mock<ILogger<ConsumerWalletService>>();
            var mapper = new Mock<IMapper>();
            var consumerService = new ConsumerWalletService(consumerWalletLogger.Object, consumerWalletRepo.Object, mapper.Object, _walletRepository.Object, _walletTypeRepository.Object, _session.Object);
            var requestDto = new FindConsumerWalletRequestDto();
            consumerWalletRepo.Setup(x => x.FindAsync(It.IsAny<Expression<Func<ConsumerWalletModel, bool>>>(), false)).ThrowsAsync(new Exception("Repository Exception"));
            var exception = await consumerService.GetConsumerWallet(requestDto);
            Assert.NotNull(exception);
        }

        [Fact]
        public async Task Should_Post_Consumer_Wallets()
        {
            var transactionMock = new Mock<ITransaction>();
            _session.Setup(s => s.BeginTransaction()).Returns(transactionMock.Object);
            var consumerWalletDataMockDto = new ConsumerWalletDataMockDto();
            var consumerWallets = await _consumerWalletController.PostConsumerWallets(new List<ConsumerWalletDataDto> { consumerWalletDataMockDto });
            var result = consumerWallets.Result as OkObjectResult;
            Assert.True(result?.Value != null);
            Assert.True(result?.StatusCode == 200);
        }
        [Fact]
        public async Task Catch_Exception_Post_Consumer_Wallets_Controller()
        {
            var consumerService = new Mock<IConsumerWalletService>();
            consumerService.Setup(service => service.PostConsumerWallets(It.IsAny<IList<ConsumerWalletDataDto>>()))
                       .ThrowsAsync(new Exception("Sample exception message"));
            var consumerWalletController = new Mock<ILogger<ConsumerWalletController>>();
            var controller = new ConsumerWalletController(consumerWalletController.Object, consumerService.Object);
            var consumerDataRequestDto = new List<ConsumerWalletDataDto>();
            var result = await controller.PostConsumerWallets(consumerDataRequestDto);
            Assert.True(result?.Value != null);
        }

        [Fact]
        public async Task Catch_Exception_Post_Consumer_Wallets_Service()
        {
            var consumerWalletService = new Mock<IConsumerWalletService>();
            var consumerWalletController = new ConsumerWalletController(_loggerMock.Object, _consumerWalletService);
            var consumerWalletDataMockDto = new ConsumerWalletDataMockDto();
            consumerWalletService.Setup(x => x.PostConsumerWallets(It.IsAny<IList<ConsumerWalletDataDto>>()))
                .ThrowsAsync(new Exception("Simulated exception message"));
            var result = await consumerWalletController.PostConsumerWallets(new List<ConsumerWalletDataDto> { consumerWalletDataMockDto });
            Assert.True(result?.Value != null);
        }

        [Fact]
        public async Task Should_not_Post_Consumer_Wallets_For_Invalid_walletModel()
        {
            var transactionMock = new Mock<ITransaction>();
            _session.Setup(s => s.BeginTransaction()).Returns(transactionMock.Object);
            _walletRepository.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<WalletModel, bool>>>(), false));
            _walletTypeRepository.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<WalletTypeModel, bool>>>(), false)).ReturnsAsync(new WalletTypeMockModel());
            var consumerWalletDataMockDto = new ConsumerWalletDataMockDto();
            consumerWalletDataMockDto.walletDto.WalletId = 0;
            var wallet = await _consumerWalletService.PostConsumerWallets((new List<ConsumerWalletDataDto> { consumerWalletDataMockDto }));
            var result = wallet.Select(wallet => wallet.ErrorCode) as BadRequestObjectResult;
            Assert.True(result?.Value == null);
        }

        [Fact]
        public async Task GetConsumerWalletsByWalletType_ReturnsOk()
        {
            // Arrange
            var requestDto = new FindConsumerWalletByWalletTypeRequestMockDto();
            var expectedModel = new ConsumerWalletMockModel();
            _walletTypeRepository.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<WalletTypeModel, bool>>>(), false)).ReturnsAsync(new WalletTypeMockModel());

            // Act
            var result = await _consumerWalletController.GetConsumerWalletsByWalletType(requestDto);

            // Assert
            Assert.IsType<ActionResult<FindConsumerWalletResponseDto>>(result);
            var actionResult = Assert.IsType<OkObjectResult>(result.Result);
            var actualResponseDto = Assert.IsType<FindConsumerWalletResponseDto>(actionResult.Value);
            Assert.NotNull(actualResponseDto.ConsumerWallets);
            Assert.Single(actualResponseDto.ConsumerWallets);
            Assert.Equal(expectedModel.ConsumerCode, actualResponseDto.ConsumerWallets[0].ConsumerCode);
        }

        [Fact]
        public async Task GetConsumerWalletsByWalletType_ReturnsWalletTypeNotFound()
        {
            // Arrange
            var requestDto = new FindConsumerWalletByWalletTypeRequestMockDto();
            var expectedResponseDto = new FindConsumerWalletResponseDto
            {
                ErrorCode = StatusCodes.Status404NotFound,
                ErrorMessage = "Wallet type not found"
            };
            _walletTypeRepository.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<WalletTypeModel, bool>>>(), false));

            // Act
            var result = await _consumerWalletController.GetConsumerWalletsByWalletType(requestDto);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result.Result);
            var notFoundResult = result.Result as NotFoundObjectResult;
            var responseDto = notFoundResult?.Value as FindConsumerWalletResponseDto;
            Assert.Equal(expectedResponseDto.ErrorCode, responseDto?.ErrorCode);
            Assert.Equal(expectedResponseDto.ErrorMessage, responseDto?.ErrorMessage);
        }

        [Fact]
        public async Task GetConsumerWalletsByWalletType_ReturnsConsumerWalletNotFound()
        {
            // Arrange
            var requestDto = new FindConsumerWalletByWalletTypeRequestMockDto();

            var expectedResponseDto = new FindConsumerWalletResponseDto
            {
                ErrorCode = 404,
                ErrorMessage = "Consumer wallet not found"
            };
            List<ConsumerWalletModel> consumerWalletModelList = new();
            _walletTypeRepository.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<WalletTypeModel, bool>>>(), false))
                .ReturnsAsync(new WalletTypeModel() { WalletTypeId = 1 });
            _consumerWalletRepository.Setup(x => x.GetConsumerWalletsByWalletType(It.IsAny<string>(), It.IsAny<long>()))
                .ReturnsAsync(consumerWalletModelList);

            // Act
            var result = await _consumerWalletController.GetConsumerWalletsByWalletType(requestDto);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result.Result);
            var notFoundResult = result.Result as NotFoundObjectResult;
            var responseDto = notFoundResult?.Value as FindConsumerWalletResponseDto;
            Assert.Equal(expectedResponseDto.ErrorCode, responseDto?.ErrorCode);
            Assert.Equal(expectedResponseDto.ErrorMessage, responseDto?.ErrorMessage);
        }

        [Fact]
        public async Task GetConsumerWalletsByWalletType_ReturnsInternalServerError()
        {
            // Arrange
            var requestDto = new FindConsumerWalletByWalletTypeRequestMockDto();
            var exceptionMessage = "Test exception message";
            _consumerWalletRepository.Setup(x => x.GetConsumerWalletsByWalletType(It.IsAny<string>(), It.IsAny<long>()))
                .ThrowsAsync(new Exception(exceptionMessage));
            _walletTypeRepository.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<WalletTypeModel, bool>>>(), false)).ReturnsAsync(new WalletTypeMockModel());

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
        public async Task GetAllConsumerWallets_ReturnsOk_WhenWalletsRetrievedSuccessfully()
        {
            // Arrange
            var requestDto = new GetConsumerWalletRequestDto
            {
                TenantCode = "test-tenant",
                ConsumerCode = "test-consumer"
            };

            _consumerWalletRepository
                .Setup(svc => svc.GetConsumerAllWallets(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new List<ConsumerWalletDetailsModel>() { new ConsumerWalletDetailsModel()});

            // Act
            var result = await _consumerWalletController.GetAllConsumerWallets(requestDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var actualResponse = Assert.IsType<ConsumerWalletResponseDto>(okResult.Value);
            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
        }

        [Fact]
        public async Task GetAllConsumerWallets_ReturnsErrorCode_WhenServiceReturnsError()
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

            _consumerWalletRepository
                .Setup(svc => svc.GetConsumerAllWallets(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new List<ConsumerWalletDetailsModel>());

            // Act
            var result = await _consumerWalletController.GetAllConsumerWallets(requestDto);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(StatusCodes.Status404NotFound, objectResult.StatusCode);
            var actualResponse = Assert.IsType<ConsumerWalletResponseDto>(objectResult.Value);
            Assert.Equal(StatusCodes.Status404NotFound, actualResponse.ErrorCode);
        }

        [Fact]
        public async Task GetAllConsumerWallets_ReturnsInternalServerError_WhenExceptionIsThrown()
        {
            // Arrange
            var requestDto = new GetConsumerWalletRequestDto
            {
                TenantCode = "test-tenant",
                ConsumerCode = "test-consumer"
            };

            _consumerWalletRepository
                .Setup(svc => svc.GetConsumerAllWallets(It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new Exception("An unexpected error occurred"));

            // Act
            var result = await _consumerWalletController.GetAllConsumerWallets(requestDto);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
            var actualResponse = Assert.IsType<ConsumerWalletResponseDto>(objectResult.Value);
            Assert.Equal(StatusCodes.Status500InternalServerError, actualResponse.ErrorCode);
            
        }
        [Fact]
        public async Task GetConsumerWalletsWithDetails_ShouldReturnDetails_WhenConsumerCodeIsValid()
        {
            // Arrange
            var mockDetails = new List<ConsumerWalletDetailsModel>
            {
                new ConsumerWalletDetailsModel
                {
                    ConsumerWallet = new ConsumerWalletModel { WalletId = 1, ConsumerCode = "CON123", DeleteNbr = 0 },
                    Wallet = new WalletModel { WalletId = 1, WalletTypeId = 2, DeleteNbr = 0 },
                    WalletType = new WalletTypeModel { WalletTypeId = 2, WalletTypeName = "Cashback" }
                }
            }.AsQueryable();

            var mockSession = new Mock<NHibernate.ISession>();
            var loggerMock = new Mock<ILogger<BaseRepo<ConsumerWalletModel>>>();

            var repo = new ConsumerWalletRepo(loggerMock.Object, mockSession.Object);

            // Act
            var result = await repo.GetConsumerWalletsWithDetails("CON123");

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
        }

    }
}

