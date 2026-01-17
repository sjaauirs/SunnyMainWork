using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.ClearScript;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using NHibernate;
using NHibernate.Driver;
using SunnyRewards.Helios.Common.Core.Repositories;
using SunnyRewards.Helios.User.Core.Domain.Dtos;
using SunnyRewards.Helios.Wallet.Api.Controllers;
using SunnyRewards.Helios.Wallet.Core.Domain.Dtos;
using SunnyRewards.Helios.Wallet.Core.Domain.Models;
using SunnyRewards.Helios.Wallet.Infrastructure.HttpClients.Interfaces;
using SunnyRewards.Helios.Wallet.Infrastructure.Repositories;
using SunnyRewards.Helios.Wallet.Infrastructure.Repositories.Interfaces;
using SunnyRewards.Helios.Wallet.Infrastructure.Services;
using SunnyRewards.Helios.Wallet.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.Wallet.UnitTest.Fixtures.MockDto;
using SunnyRewards.Helios.Wallet.UnitTest.Fixtures.MockModel;
using SunnyRewards.Helios.Wallet.UnitTest.Fixtures.MockRepositories;
using SunnyRewards.Helios.Wallet.UnitTest.Helpers.HttpClientsMock;
using System.Linq.Expressions;
using Xunit;

namespace SunnyRewards.Helios.Wallet.UnitTest.Controllers
{
    public class TransactionControllerUnitTest
    {
        private readonly Mock<ILogger<TransactionController>> _transactionLogger;
        private readonly ITransactionService _transactionService;
        private readonly Mock<ILogger<TransactionService>> _transactionServiceLogger;
        private readonly Mock<ILogger<CsaWalletTransactionsService>> _csaTransactionServiceLogger;
        private readonly IMapper _mapper;
        private readonly Mock<ITransactionRepo> _transactionRepository;
        private readonly Mock<ITransactionDetailRepo> _transactionDetailRepository;
        private readonly Mock<IConsumerWalletRepo> _consumerWalletRepo;
        private readonly Mock<IWalletRepo> _walletRepo;
        private readonly Mock<IWalletTypeRepo> _walletTypeRepo;
        private readonly Mock<NHibernate.ISession> _session;
        private readonly IConfiguration _configuration;
        private readonly Mock<IUserClient> _userClient;
        private readonly TransactionController _transactionController;
        private readonly CsaWalletTransactionsService _csaWalletTransactionsService;
        private readonly Mock<ITransactionService> _mockTransactionService;
        private readonly Mock<IConsumerService> _mockConsumerService;
        public TransactionControllerUnitTest()
        {
            _transactionLogger = new Mock<ILogger<TransactionController>>();
            _transactionServiceLogger = new Mock<ILogger<TransactionService>>();
            _csaTransactionServiceLogger = new Mock<ILogger<CsaWalletTransactionsService>>();
            _transactionRepository = new TransactionMockRepo();
            _transactionDetailRepository = new TransactionDetailMockRepo();
            _consumerWalletRepo = new ConsumerWalletMockRepo();
            _walletRepo = new WalletMockRepo();
            _walletTypeRepo = new WalletTypeMockRepo();
            _session = new Mock<NHibernate.ISession>();
            _configuration = new ConfigurationBuilder()
           .SetBasePath(Directory.GetCurrentDirectory())
           .AddJsonFile("appsettings.Development.json")
           .Build();
            _userClient = new UserClientMock();
            _mockConsumerService = new Mock<IConsumerService>();
            _mapper = new Mapper(new MapperConfiguration(
                configure =>
                {
                    configure.AddMaps(typeof(Infrastructure.Mappings.MappingProfile.TransactionMapping).Assembly.FullName);
                }));
            _transactionService = new TransactionService(_transactionServiceLogger.Object, _mapper, _consumerWalletRepo.Object, _transactionRepository.Object,
                _transactionDetailRepository.Object, _walletRepo.Object, _walletTypeRepo.Object, _session.Object, _configuration, _userClient.Object, _mockConsumerService.Object);
            _csaWalletTransactionsService = new CsaWalletTransactionsService(_csaTransactionServiceLogger.Object, _session.Object, _walletRepo.Object,
                _consumerWalletRepo.Object, _transactionRepository.Object, _walletTypeRepo.Object, _configuration);
            _transactionController = new TransactionController(_transactionLogger.Object, _transactionService, _csaWalletTransactionsService);
            _mockTransactionService = new Mock<ITransactionService>();
        }
        [Fact]
        public async Task Should_Post_Transaction()
        {
            GetRecentTransactionRequestMockDto getRecentTransactionRequestMockDto = new GetRecentTransactionRequestMockDto();
            _mockConsumerService
                    .Setup(x => x.GetConsumer(It.Is<GetConsumerRequestDto>(r => r.ConsumerCode == "consumer-code")))
                    .ReturnsAsync(new GetConsumerResponseDto
                    {
                        Consumer = new ConsumerDto
                        {
                            ConsumerCode = "consumer-code",
                            MemberNbr = "123",
                            SubscriberMemberNbr = "123"
                        }
                    });

            var transactionMockDto = await _transactionController.GetRecentTransactions(getRecentTransactionRequestMockDto);
            var result = transactionMockDto.Result as OkObjectResult;
            Assert.True(result?.Value != null);
            Assert.True(result.StatusCode == 200);
        }

        [Fact]
        public async Task GetRecentTransactions_Should_Get_Transactions()
        {
            GetRecentTransactionRequestMockDto getRecentTransactionRequestMockDto = new GetRecentTransactionRequestMockDto();
            getRecentTransactionRequestMockDto.ConsumerCode = "cus-04c211b4339348509eaa870cdea59600";
            getRecentTransactionRequestMockDto.WalletId = 7;
            getRecentTransactionRequestMockDto.IsRewardAppTransactions = true;
    
            _walletTypeRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<WalletTypeModel, bool>>>(), false)).ReturnsAsync(new WalletTypeMockModel());
            var transactionMockDto = await _transactionController.GetRecentTransactions(getRecentTransactionRequestMockDto);
            var result = transactionMockDto.Result as OkObjectResult;
            Assert.NotNull(result?.Value);
            Assert.Equal(200, result.StatusCode);
        }
        [Fact]
        public async Task Should_Not_Invalid_WalletId()
        {
            _transactionRepository.Setup(x => x.FindAsync(It.IsAny<Expression<Func<TransactionModel, bool>>>(), false)).ReturnsAsync(new List<TransactionModel>());
            var getRecentTransactionRequestMockDto = new GetRecentTransactionRequestMockDto() { WalletId = 0 };
            var response = await _transactionController.GetRecentTransactions(getRecentTransactionRequestMockDto);
            var result = response.Result as BadRequestObjectResult;
            Assert.True(result?.Value == null);
        }


        [Fact]
        public async Task Should_Return_Catch_Exception_Transaction()
        {
            var transactionLogger = new Mock<ILogger<TransactionController>>();
            var transactionService = new Mock<ITransactionService>();
            var transactionController = new TransactionController(transactionLogger.Object, transactionService.Object, _csaWalletTransactionsService);
            var getRecentTransactionRequestMockDto = new GetRecentTransactionRequestMockDto();
            transactionService.Setup(x => x.GetTransactionDetails(It.IsAny<GetRecentTransactionRequestMockDto>(),null))
                .ThrowsAsync(new Exception("Transaction Exception"));
            var response = await transactionController.GetRecentTransactions(getRecentTransactionRequestMockDto);
            var result = response.Result as BadRequestObjectResult;
            Assert.True(result?.Value == null);

        }
        [Fact]
        public void GetTransactionDetails_ThrowsException_LogsError()
        {
            // Arrange
            var transactionLogger = new Mock<ILogger<TransactionService>>();
            var consumerWalletRepo = new Mock<IConsumerWalletRepo>();
            var transactionRepo = new Mock<ITransactionRepo>();
            var transactionDetailRepo = new Mock<ITransactionDetailRepo>();
            var mapper = new Mock<IMapper>();

            var transactionService = new TransactionService(
                transactionLogger.Object,
                mapper.Object,
                consumerWalletRepo.Object,
                transactionRepo.Object,
                transactionDetailRepo.Object,
                _walletRepo.Object,
                _walletTypeRepo.Object, _session.Object, _configuration, _userClient.Object , _mockConsumerService.Object
            );

            var requestDto = new GetRecentTransactionRequestDto();
            transactionRepo.Setup(repo => repo.FindAsync(It.IsAny<Expression<Func<TransactionModel, bool>>>(), false))
                          .ThrowsAsync(new Exception("Repository Exception"));

            var exception = transactionService.GetTransactionDetails(requestDto);
            Assert.NotNull(exception);
        }

        [Fact]
        public async Task RevertAllTransaction_Should_Return_Bad_Request_Result()
        {
            // Arrange
            var requestDto = new RevertTransactionsRequestMockDto();
            requestDto.ConsumerCode = string.Empty;

            // Act
            var result = await _transactionController.RevertAllTransaction(requestDto);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result.Result);
            var badRequestObjectResult = result.Result as BadRequestObjectResult;
            Assert.Equal(StatusCodes.Status400BadRequest, badRequestObjectResult?.StatusCode);
        }

        [Fact]
        public async Task RevertAllTransaction_Should_Return_Not_Found_Result()
        {
            // Arrange
            var requestDto = new RevertTransactionsRequestMockDto();
            _walletRepo.Setup(x => x.GetWalletByConsumerAndWalletType(requestDto.TenantCode, requestDto.ConsumerCode, It.IsAny<long>()));
            _walletTypeRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<WalletTypeModel, bool>>>(), false)).ReturnsAsync(new WalletTypeMockModel());

            // Act
            var result = await _transactionController.RevertAllTransaction(requestDto);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result.Result);
            var notFoundResult = result.Result as NotFoundObjectResult;
            Assert.Equal(StatusCodes.Status404NotFound, notFoundResult?.StatusCode);
        }

        [Fact]
        public async Task RevertAllTransaction_Should_Return_Ok_Result()
        {
            // Arrange
            var requestDto = new RevertTransactionsRequestMockDto();
            var mockWalletData = new WalletMockModel();
            var transactionMock = new Mock<ITransaction>();
            _session.Setup(s => s.BeginTransaction()).Returns(transactionMock.Object);
            _walletRepo.Setup(x => x.GetWalletByConsumerAndWalletType(requestDto.TenantCode, requestDto.ConsumerCode, It.IsAny<long>()))
                .ReturnsAsync(mockWalletData);
            _walletTypeRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<WalletTypeModel, bool>>>(), false)).ReturnsAsync(new WalletTypeMockModel());

            // Act
            var result = await _transactionController.RevertAllTransaction(requestDto);

            // Assert
            Assert.IsType<OkObjectResult>(result.Result);
            var okObjectResult = result.Result as OkObjectResult;
            Assert.Equal(StatusCodes.Status200OK, okObjectResult?.StatusCode);
        }

        [Fact]
        public void GetConsumerPrimaryWallet_Should_Return_wallet()
        {
            // Arrange
            var mockSession = new Mock<NHibernate.ISession>();
            mockSession.Setup(s => s.BeginTransaction()).Returns(Mock.Of<ITransaction>());
            var walletMockData = new WalletMockModel();
            var consumerWalletMockModel = new ConsumerWalletMockModel();
            consumerWalletMockModel.TenantCode = walletMockData.TenantCode;
            consumerWalletMockModel.WalletId = walletMockData.WalletId;
            var expectedWallets = new List<WalletModel>
            {
                walletMockData
            };

            var expectedConsumerWallets = new List<ConsumerWalletModel>()
            {
               consumerWalletMockModel
            };
            mockSession.Setup(x => x.Query<WalletModel>())
                .Returns(expectedWallets.AsQueryable);
            mockSession.Setup(x => x.Query<ConsumerWalletModel>())
                .Returns(expectedConsumerWallets.AsQueryable());
            var loggerMock = new Mock<ILogger<BaseRepo<WalletModel>>>();
            var walletRepo = new WalletRepo(loggerMock.Object, mockSession.Object);

            // Act
            var response = walletRepo.GetWalletByConsumerAndWalletType(consumerWalletMockModel.TenantCode, consumerWalletMockModel.ConsumerCode, walletMockData.WalletTypeId);

            // Assert
            Assert.NotNull(response);
        }

        [Fact]
        public void GetTotalAmountForConsumerByTransactionDetailType_Should_Return_Amount()
        {
            // Arrange
            var mockSession = new Mock<NHibernate.ISession>();
            mockSession.Setup(s => s.BeginTransaction()).Returns(Mock.Of<ITransaction>());
            var transactionMockModel = new TransactionMockModel();
            var transactionDetailMockModel = new TransactionDetailMockModel();
            var consumerCode = Guid.NewGuid().ToString("N");
            var transactionType = "REWARD";
            transactionDetailMockModel.ConsumerCode = consumerCode;
            transactionDetailMockModel.TransactionDetailType = transactionType;
            var expectedTransactions = new List<TransactionModel>
            {
                transactionMockModel
            };
            var expectedTransactionDetails = new List<TransactionDetailModel>()
            {
               transactionDetailMockModel
            };
            mockSession.Setup(x => x.Query<TransactionModel>())
                .Returns(expectedTransactions.AsQueryable);
            mockSession.Setup(x => x.Query<TransactionDetailModel>())
                .Returns(expectedTransactionDetails.AsQueryable());
            var loggerMock = new Mock<ILogger<BaseRepo<TransactionModel>>>();
            var transactionRepo = new TransactionRepo(loggerMock.Object, mockSession.Object,_mapper);

            // Act
            var response = transactionRepo.GetTotalAmountForConsumerByTransactionDetailType(consumerCode, transactionMockModel.WalletId, transactionType);

            // Assert
            Assert.NotNull(response);
        }

        [Fact]
        public async Task RevertAllTransaction_Should_Return_Internal_Server_Error_Result_When_Exception_Occurred()
        {
            // Arrange
            var requestDto = new RevertTransactionsRequestMockDto();
            var transactionMock = new Mock<ITransaction>();
            _session.Setup(s => s.BeginTransaction()).Returns(transactionMock.Object);
            _walletRepo.Setup(x => x.GetWalletByConsumerAndWalletType(requestDto.TenantCode, requestDto.ConsumerCode, It.IsAny<long>()))
                .ThrowsAsync(new Exception("Test Exception"));

            // Act
            var result = await _transactionController.RevertAllTransaction(requestDto);

            // Assert
            Assert.IsType<ObjectResult>(result.Result);
            var objectResult = result.Result as ObjectResult;
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult?.StatusCode);
        }

        [Fact]
        public async Task Should_Post_Get_Transaction_Controller()
        {
            var requestDto = new PostGetTransactionsRequestMockDto();
            var responseDto = new PostGetTransactionsRequestMockDto();
            var result = await _transactionController.GetTransaction(requestDto);
            var okResult = result.Result as OkObjectResult;
        }

        [Fact]
        public async Task GetTransaction_Should_Get_Transactions()
        {
            var requestDto = new PostGetTransactionsRequestMockDto();
            requestDto.WalletId = 0;
            requestDto.IsRewardAppTransactions = true;
            _walletTypeRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<WalletTypeModel, bool>>>(), false)).ReturnsAsync(new WalletTypeMockModel());
            var result = await _transactionController.GetTransaction(requestDto);
            var okResult = result.Result as OkObjectResult;
            Assert.NotNull(okResult?.Value);
            Assert.Equal(200, okResult.StatusCode);
        }

        [Fact]
        public async Task Should_Not_Found_Get_Transaction_Controller()
        {
            var postGetTransactionsRequestDto = new PostGetTransactionsRequestMockDto();
            _consumerWalletRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<ConsumerWalletModel, bool>>>(), false))
                .ReturnsAsync((ConsumerWalletMockModel)null);
            var result = await _transactionController.GetTransaction(postGetTransactionsRequestDto);
            var notFoundResult = result.Result as NotFoundObjectResult;
        }

        [Fact]
        public async Task Should_Get_Forbidden_Transaction_Controller()
        {
            var postGetTransactionDto = new PostGetTransactionsRequestMockDto { ConsumerCode = "123", WalletId = 1 };
            var result = await _transactionController.GetTransaction(postGetTransactionDto);
            Assert.NotNull(result);
            var objectResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(StatusCodes.Status403Forbidden, objectResult.StatusCode);
        }

        [Fact]
        public async Task Should_Get_Transaction_Exception()
        {
            var mockTransactionService = new Mock<ITransactionService>();
            var mockTransactionLogger = new Mock<ILogger<TransactionController>>();
            var controller = new TransactionController(_transactionLogger.Object, mockTransactionService.Object, _csaWalletTransactionsService);
            var requestDto = new PostGetTransactionsRequestDto();
            mockTransactionService.Setup(x => x.GetTransaction(requestDto))
                .ThrowsAsync(new Exception("Mock exception message"));
            await Assert.ThrowsAsync<Exception>(async () => await controller.GetTransaction(requestDto));
        }

        [Fact]
        public async Task Should_Post_Get_Transaction_Service()
        {
            var requestDto = new PostGetTransactionsRequestMockDto();
            var result = await _transactionService.GetTransaction(requestDto);
            Assert.NotNull(result);
        }

        [Fact]
        public async Task Should_NotFound_Get_Transaction_Service()
        {
            var postGetTransactionsRequestDto = new PostGetTransactionsRequestMockDto();
            _consumerWalletRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<ConsumerWalletModel, bool>>>(), false))
                .ReturnsAsync((ConsumerWalletMockModel)null);
            var result = await _transactionService.GetTransaction(postGetTransactionsRequestDto);
            Assert.NotNull(result);
            Assert.Equal(StatusCodes.Status404NotFound, result.ErrorCode);
        }

        [Fact]
        public async Task Should_Forbidden_Get_Transaction_Service()
        {
            var requestDto = new PostGetTransactionsRequestDto { ConsumerCode = "123", WalletId = 1 };
            var result = await _transactionService.GetTransaction(requestDto);
            Assert.NotNull(result);
            Assert.Equal(StatusCodes.Status403Forbidden, result.ErrorCode);
        }

        [Fact]
        public async Task Shold_Get_Transaction_Service_Exception()
        {
            var requestDto = new PostGetTransactionsRequestMockDto();
            _consumerWalletRepo.Setup(repo => repo.FindOneAsync(It.IsAny<Expression<Func<ConsumerWalletModel, bool>>>(), false))
                .ThrowsAsync(new Exception("Internal Server Error"));
            var result = await Assert.ThrowsAsync<Exception>(() => _transactionService.GetTransaction(requestDto));
        }
        [Fact]
        public async Task ValidateInputs_ShouldReturnFalse_WhenWalletNotFound()
        {
            // Arrange
            var requestDto = new CsaWalletTransactionsRequestDto
            {
                TenantCode = "ten-ecada21e57154928a2bb959e8365b8b4",
                ConsumerCode = "cmr-dd598745284b4c5ead19539e91ec3872",
                WalletId = 12345
            };

            _consumerWalletRepo.Setup(repo => repo.GetConsumerAllWallets(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new List<ConsumerWalletDetailsModel>());

            // Act
            var result = await _transactionController.ProcessCsaWalletTransactions(requestDto);

            // Assert
            Assert.NotNull(result);
            var objectResult = result as ObjectResult;
            Assert.Equal(StatusCodes.Status404NotFound, objectResult?.StatusCode);
        }

        [Fact]
        public async Task ValidateInputs_ShouldReturnFalse_WhenConsumerWalletTypeNotFound()
        {
            // Arrange
            var requestDto = new CsaWalletTransactionsRequestDto
            {
                TenantCode = "ten-ecada21e57154928a2bb959e8365b8b4",
                ConsumerCode = "cmr-dd598745284b4c5ead19539e91ec3872",
                WalletId = 4,
                TenantConfig = "{\"purseConfig\":{\"purses\":[{\"purseLabel\":\"OTC\",\"walletType\":\"wat-2d62dcaf2aa4424b9ff6c2ddb5895\",\"purseNumber\":12401,\"periodConfig\":{\"fundDate\":19,\"interval\":\"MONTH\",\"applyDateConfig\":{\"applyDate\":1,\"applyDateType\":\"NEXT_MONTH\"}},\"purseWalletType\":\"wat-2d62dcaf2aa4424b9ff6c2ddb5895077\",\"masterWalletType\":\"wat-4b364fg722f04034cv732b355d84f479\",\"masterRedemptionWalletType\":\"wat-bc8f4f7c028d479f900f0af794e385c8\"},{\"purseLabel\":\"FOD\",\"walletType\":\"wat-35d4rt62d77y6119a6t730c9b6447457\",\"purseNumber\":22401,\"periodConfig\":{\"fundDate\":19,\"interval\":\"MONTH\",\"applyDateConfig\":{\"applyDate\":1,\"applyDateType\":\"NEXT_MONTH\"}},\"purseWalletType\":\"wat-35b3ac62d74b4119a5f630c9b6446035\",\"masterWalletType\":\"wat-35d4rt62d77y6119a6t730c9b6447457\",\"masterRedemptionWalletType\":\"wat-01b4a568c1814449b64168bb434127b7\"},{\"purseLabel\":\"REW\",\"walletType\":\"wat-2d62dcaf2aa4424b9ff6c2ddb5895077\",\"purseNumber\":32401,\"periodConfig\":{\"fundDate\":19,\"interval\":\"MONTH\",\"applyDateConfig\":{\"applyDate\":1,\"applyDateType\":\"NEXT_MONTH\"}},\"purseWalletType\":\"wat-55f05107b41642c39e7a3b459223cbd8\",\"masterWalletType\":\"wat-2d62dcaf2aa4424b9ff6c2ddb5895077\",\"masterRedemptionWalletType\":\"wat-274bd71345804f09928cf451dc0f6239\"}]},\"fisProgramDetail\":{\"clientId\":\"1234500\",\"companyId\":\"1204185\",\"packageId\":\"721736\",\"subprogramId\":\"873343\"}}"
            };

            var walletDetails = new List<ConsumerWalletDetailsModel>
            {
                new ConsumerWalletDetailsModel
                {
                    Wallet = new WalletMockModel(),
                    WalletType = new WalletTypeMockModel()
                }
            };
            _consumerWalletRepo.Setup(repo => repo.GetConsumerAllWallets(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(walletDetails);

            // Act
            var result = await _transactionController.ProcessCsaWalletTransactions(requestDto);

            // Assert
            Assert.NotNull(result);
            var objectResult = result as ObjectResult;
            Assert.Equal(StatusCodes.Status404NotFound, objectResult?.StatusCode);
        }

        [Fact]
        public async Task ValidateInputs_ShouldReturnFalse_WhenPurseConfigIsNull()
        {
            // Arrange
            var requestDto = new CsaWalletTransactionsRequestDto
            {
                TenantCode = "ten-ecada21e57154928a2bb959e8365b8b4",
                ConsumerCode = "cmr-dd598745284b4c5ead19539e91ec3872",
                WalletId = 12345,
                TenantConfig = "{}"
            };

            var walletDetails = new List<ConsumerWalletDetailsModel>
            {
                new ConsumerWalletDetailsModel
                {
                    Wallet = new WalletModel { WalletId = 12345, DeleteNbr = 0 },
                    WalletType = new WalletTypeModel { WalletTypeId = 1, DeleteNbr = 0 }
                }
            };

            _consumerWalletRepo.Setup(repo => repo.GetConsumerAllWallets(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(walletDetails);

            // Act
            var result = await _transactionController.ProcessCsaWalletTransactions(requestDto);

            // Assert
            Assert.NotNull(result);
            var objectResult = result as ObjectResult;
            Assert.Equal(StatusCodes.Status404NotFound, objectResult?.StatusCode);
        }
        [Fact]
        public async Task ValidateInputs_ShouldReturnFalse_WhenConsumerWalletNotFound()
        {
            // Arrange
            var requestDto = new CsaWalletTransactionsRequestDto
            {
                TenantCode = "ten-ecada21e57154928a2bb959e8365b8b4",
                ConsumerCode = "cmr-dd598745284b4c5ead19539e91ec3872",
                WalletId = 4,
                TenantConfig = "{\"purseConfig\":{\"purses\":[{\"purseLabel\":\"OTC\",\"walletType\":\"wat-2d62dcaf2aa4424b9ff6c2ddb5895077\",\"purseNumber\":12401,\"periodConfig\":{\"fundDate\":19,\"interval\":\"MONTH\",\"applyDateConfig\":{\"applyDate\":1,\"applyDateType\":\"NEXT_MONTH\"}},\"purseWalletType\":\"wat-2d62dcaf2aa4424b9ff6c2ddb5895077\",\"masterWalletType\":\"wat-4b364fg722f04034cv732b355d84f479\",\"masterRedemptionWalletType\":\"wat-bc8f4f7c028d479f900f0af794e385c8\"},{\"purseLabel\":\"FOD\",\"walletType\":\"wat-35d4rt62d77y6119a6t730c9b6447457\",\"purseNumber\":22401,\"periodConfig\":{\"fundDate\":19,\"interval\":\"MONTH\",\"applyDateConfig\":{\"applyDate\":1,\"applyDateType\":\"NEXT_MONTH\"}},\"purseWalletType\":\"wat-35b3ac62d74b4119a5f630c9b6446035\",\"masterWalletType\":\"wat-35d4rt62d77y6119a6t730c9b6447457\",\"masterRedemptionWalletType\":\"wat-01b4a568c1814449b64168bb434127b7\"},{\"purseLabel\":\"REW\",\"walletType\":\"wat-2d62dcaf2aa4424b9ff6c2ddb5895077\",\"purseNumber\":32401,\"periodConfig\":{\"fundDate\":19,\"interval\":\"MONTH\",\"applyDateConfig\":{\"applyDate\":1,\"applyDateType\":\"NEXT_MONTH\"}},\"purseWalletType\":\"wat-55f05107b41642c39e7a3b459223cbd8\",\"masterWalletType\":\"wat-2d62dcaf2aa4424b9ff6c2ddb5895077\",\"masterRedemptionWalletType\":\"wat-274bd71345804f09928cf451dc0f6239\"}]},\"fisProgramDetail\":{\"clientId\":\"1234500\",\"companyId\":\"1204185\",\"packageId\":\"721736\",\"subprogramId\":\"873343\"}}"
            };

            var walletDetails = new List<ConsumerWalletDetailsModel>
            {
                new ConsumerWalletDetailsModel
                {
                    Wallet = new WalletMockModel(),
                    WalletType = new WalletTypeMockModel()
                }
            };
            _consumerWalletRepo.Setup(repo => repo.GetConsumerAllWallets(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(walletDetails);

            // Act
            var result = await _transactionController.ProcessCsaWalletTransactions(requestDto);

            // Assert
            Assert.NotNull(result);
            var objectResult = result as ObjectResult;
            Assert.Equal(StatusCodes.Status404NotFound, objectResult?.StatusCode);
        }

        [Fact]
        public async Task ValidateInputs_ShouldReturnFalse_When_Purse_IsNull()
        {
            // Arrange
            var requestDto = new CsaWalletTransactionsRequestDto
            {
                TenantCode = "ten-ecada21e57154928a2bb959e8365b8b4",
                ConsumerCode = "cmr-dd598745284b4c5ead19539e91ec3872",
                WalletId = 12345,
                TenantConfig = "{\"purseConfig\":{\"purses\":[{\"purseLabel\":\"OTC\",\"walletType\":\"wat-4b364fg722f04034cv732b355d84f479\",\"purseNumber\":12401,\"periodConfig\":{\"fundDate\":19,\"interval\":\"MONTH\",\"applyDateConfig\":{\"applyDate\":1,\"applyDateType\":\"NEXT_MONTH\"}},\"purseWalletType\":\"wat-4b364ed612f04034bf732b355d84f368\",\"masterWalletType\":\"wat-4b364fg722f04034cv732b355d84f479\",\"masterRedemptionWalletType\":\"wat-bc8f4f7c028d479f900f0af794e385c8\"},{\"purseLabel\":\"FOD\",\"walletType\":\"wat-35d4rt62d77y6119a6t730c9b6447457\",\"purseNumber\":22401,\"periodConfig\":{\"fundDate\":19,\"interval\":\"MONTH\",\"applyDateConfig\":{\"applyDate\":1,\"applyDateType\":\"NEXT_MONTH\"}},\"purseWalletType\":\"wat-35b3ac62d74b4119a5f630c9b6446035\",\"masterWalletType\":\"wat-35d4rt62d77y6119a6t730c9b6447457\",\"masterRedemptionWalletType\":\"wat-01b4a568c1814449b64168bb434127b7\"},{\"purseLabel\":\"REW\",\"walletType\":\"wat-2d62dcaf2aa4424b9ff6c2ddb5895077\",\"purseNumber\":32401,\"periodConfig\":{\"fundDate\":19,\"interval\":\"MONTH\",\"applyDateConfig\":{\"applyDate\":1,\"applyDateType\":\"NEXT_MONTH\"}},\"purseWalletType\":\"wat-55f05107b41642c39e7a3b459223cbd8\",\"masterWalletType\":\"wat-2d62dcaf2aa4424b9ff6c2ddb5895077\",\"masterRedemptionWalletType\":\"wat-274bd71345804f09928cf451dc0f6239\"}]},\"fisProgramDetail\":{\"clientId\":\"1234500\",\"companyId\":\"1204185\",\"packageId\":\"721736\",\"subprogramId\":\"873343\"}}"
            };

            var walletDetails = new List<ConsumerWalletDetailsModel>
            {
                new ConsumerWalletDetailsModel
                {
                    Wallet = new WalletModel { WalletId = 12345, DeleteNbr = 0 },
                    WalletType = new WalletTypeModel { WalletTypeId = 1, DeleteNbr = 0 }
                }
            };

            _consumerWalletRepo.Setup(repo => repo.GetConsumerAllWallets(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(walletDetails);

            // Act
            var result = await _transactionController.ProcessCsaWalletTransactions(requestDto);

            // Assert
            Assert.NotNull(result);
            var objectResult = result as ObjectResult;
            Assert.Equal(StatusCodes.Status404NotFound, objectResult?.StatusCode);
        }
        [Fact]
        public async Task ValidateInputs_ShouldReturnFalse_When_MasterWalletType_Notfound()
        {
            // Arrange
            var requestDto = new CsaWalletTransactionsRequestDto
            {
                TenantCode = "ten-ecada21e57154928a2bb959e8365b8b4",
                ConsumerCode = "cmr-dd598745284b4c5ead19539e91ec3872",
                WalletId = 4,
                TenantConfig = "{\"purseConfig\":{\"purses\":[{\"purseLabel\":\"OTC\",\"walletType\":\"wat-2d62dcaf2aa4424b9ff6c2ddb5895077\",\"purseNumber\":12401,\"periodConfig\":{\"fundDate\":19,\"interval\":\"MONTH\",\"applyDateConfig\":{\"applyDate\":1,\"applyDateType\":\"NEXT_MONTH\"}},\"purseWalletType\":\"wat-2d62dcaf2aa4424b9ff6c2ddb5895077\",\"masterWalletType\":\"wat-4b364fg722f04034cv732b355d84f479\",\"masterRedemptionWalletType\":\"wat-bc8f4f7c028d479f900f0af794e385c8\"},{\"purseLabel\":\"FOD\",\"walletType\":\"wat-35d4rt62d77y6119a6t730c9b6447457\",\"purseNumber\":22401,\"periodConfig\":{\"fundDate\":19,\"interval\":\"MONTH\",\"applyDateConfig\":{\"applyDate\":1,\"applyDateType\":\"NEXT_MONTH\"}},\"purseWalletType\":\"wat-35b3ac62d74b4119a5f630c9b6446035\",\"masterWalletType\":\"wat-35d4rt62d77y6119a6t730c9b6447457\",\"masterRedemptionWalletType\":\"wat-01b4a568c1814449b64168bb434127b7\"},{\"purseLabel\":\"REW\",\"walletType\":\"wat-2d62dcaf2aa4424b9ff6c2ddb5895077\",\"purseNumber\":32401,\"periodConfig\":{\"fundDate\":19,\"interval\":\"MONTH\",\"applyDateConfig\":{\"applyDate\":1,\"applyDateType\":\"NEXT_MONTH\"}},\"purseWalletType\":\"wat-55f05107b41642c39e7a3b459223cbd8\",\"masterWalletType\":\"wat-2d62dcaf2aa4424b9ff6c2ddb5895077\",\"masterRedemptionWalletType\":\"wat-274bd71345804f09928cf451dc0f6239\"}]},\"fisProgramDetail\":{\"clientId\":\"1234500\",\"companyId\":\"1204185\",\"packageId\":\"721736\",\"subprogramId\":\"873343\"}}"
            };

            var walletDetails = new List<ConsumerWalletDetailsModel>
            {
                new ConsumerWalletDetailsModel
                {
                    Wallet = new WalletMockModel(),
                    WalletType = new WalletTypeMockModel()
                }
            };

            _consumerWalletRepo.Setup(repo => repo.GetConsumerAllWallets(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(walletDetails);
            _walletTypeRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<WalletTypeModel, bool>>>(), false));

            // Act
            var result = await _transactionController.ProcessCsaWalletTransactions(requestDto);

            // Assert
            Assert.NotNull(result);
            var objectResult = result as ObjectResult;
            Assert.Equal(StatusCodes.Status404NotFound, objectResult?.StatusCode);
        }
        [Fact]
        public async Task ValidateInputs_ShouldReturnFalse_When_RedemptionWalletType_Notfound()
        {
            // Arrange
            var requestDto = new CsaWalletTransactionsRequestDto
            {
                TenantCode = "ten-ecada21e57154928a2bb959e8365b8b4",
                ConsumerCode = "cmr-dd598745284b4c5ead19539e91ec3872",
                WalletId = 4,
                TenantConfig = "{\"purseConfig\":{\"purses\":[{\"purseLabel\":\"OTC\",\"walletType\":\"wat-2d62dcaf2aa4424b9ff6c2ddb5895077\",\"purseNumber\":12401,\"periodConfig\":{\"fundDate\":19,\"interval\":\"MONTH\",\"applyDateConfig\":{\"applyDate\":1,\"applyDateType\":\"NEXT_MONTH\"}},\"purseWalletType\":\"wat-2d62dcaf2aa4424b9ff6c2ddb5895077\",\"masterWalletType\":\"wat-4b364fg722f04034cv732b355d84f479\",\"masterRedemptionWalletType\":\"wat-bc8f4f7c028d479f900f0af794e38\"},{\"purseLabel\":\"FOD\",\"walletType\":\"wat-35d4rt62d77y6119a6t730c9b6447457\",\"purseNumber\":22401,\"periodConfig\":{\"fundDate\":19,\"interval\":\"MONTH\",\"applyDateConfig\":{\"applyDate\":1,\"applyDateType\":\"NEXT_MONTH\"}},\"purseWalletType\":\"wat-35b3ac62d74b4119a5f630c9b6446035\",\"masterWalletType\":\"wat-35d4rt62d77y6119a6t730c9b6447457\",\"masterRedemptionWalletType\":\"wat-01b4a568c1814449b64168bb434127b7\"},{\"purseLabel\":\"REW\",\"walletType\":\"wat-2d62dcaf2aa4424b9ff6c2ddb5895077\",\"purseNumber\":32401,\"periodConfig\":{\"fundDate\":19,\"interval\":\"MONTH\",\"applyDateConfig\":{\"applyDate\":1,\"applyDateType\":\"NEXT_MONTH\"}},\"purseWalletType\":\"wat-55f05107b41642c39e7a3b459223cbd8\",\"masterWalletType\":\"wat-2d62dcaf2aa4424b9ff6c2ddb5895077\",\"masterRedemptionWalletType\":\"wat-274bd71345804f09928cf451dc0f6239\"}]},\"fisProgramDetail\":{\"clientId\":\"1234500\",\"companyId\":\"1204185\",\"packageId\":\"721736\",\"subprogramId\":\"873343\"}}"
            };

            var walletDetails = new List<ConsumerWalletDetailsModel>
            {
                new ConsumerWalletDetailsModel
                {
                    Wallet = new WalletMockModel(),
                    WalletType = new WalletTypeMockModel()
                }
            };

            _consumerWalletRepo.Setup(repo => repo.GetConsumerAllWallets(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(walletDetails);
            _walletTypeRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<WalletTypeModel, bool>>>(), false));
            _walletRepo.Setup(x => x.GetMasterWallet(It.IsAny<long>(), It.IsAny<string>())).ReturnsAsync(new WalletMockModel());

            // Act
            var result = await _transactionController.ProcessCsaWalletTransactions(requestDto);

            // Assert
            Assert.NotNull(result);
            var objectResult = result as ObjectResult;
            Assert.Equal(StatusCodes.Status404NotFound, objectResult?.StatusCode);
        }
        [Fact]
        public async Task ValidateInputs_ShouldReturnFalse_When_SuspenseWallet_Notfound()
        {
            // Arrange
            var requestDto = new CsaWalletTransactionsRequestDto
            {
                TenantCode = "ten-ecada21e57154928a2bb959e8365b8b4",
                ConsumerCode = "cmr-dd598745284b4c5ead19539e91ec3872",
                WalletId = 12345,
                TenantConfig = "{\"purseConfig\":{\"purses\":[{\"purseLabel\":\"OTC\",\"walletType\":\"wat-4b364fg722f04034cv732b355d84f479\",\"purseNumber\":12401,\"periodConfig\":{\"fundDate\":19,\"interval\":\"MONTH\",\"applyDateConfig\":{\"applyDate\":1,\"applyDateType\":\"NEXT_MONTH\"}},\"purseWalletType\":\"wat-4b364ed612f04034bf732b355d84f368\",\"masterWalletType\":\"wat-4b364fg722f04034cv732b355d84f479\",\"masterRedemptionWalletType\":\"wat-bc8f4f7c028d479f900f0af794e385c8\"},{\"purseLabel\":\"FOD\",\"walletType\":\"wat-35d4rt62d77y6119a6t730c9b6447457\",\"purseNumber\":22401,\"periodConfig\":{\"fundDate\":19,\"interval\":\"MONTH\",\"applyDateConfig\":{\"applyDate\":1,\"applyDateType\":\"NEXT_MONTH\"}},\"purseWalletType\":\"wat-35b3ac62d74b4119a5f630c9b6446035\",\"masterWalletType\":\"wat-35d4rt62d77y6119a6t730c9b6447457\",\"masterRedemptionWalletType\":\"wat-01b4a568c1814449b64168bb434127b7\"},{\"purseLabel\":\"REW\",\"walletType\":\"wat-2d62dcaf2aa4424b9ff6c2ddb5895077\",\"purseNumber\":32401,\"periodConfig\":{\"fundDate\":19,\"interval\":\"MONTH\",\"applyDateConfig\":{\"applyDate\":1,\"applyDateType\":\"NEXT_MONTH\"}},\"purseWalletType\":\"wat-55f05107b41642c39e7a3b459223cbd8\",\"masterWalletType\":\"wat-2d62dcaf2aa4424b9ff6c2ddb5895077\",\"masterRedemptionWalletType\":\"wat-274bd71345804f09928cf451dc0f6239\"}]},\"fisProgramDetail\":{\"clientId\":\"1234500\",\"companyId\":\"1204185\",\"packageId\":\"721736\",\"subprogramId\":\"873343\"}}"
            };

            var walletDetails = new List<ConsumerWalletDetailsModel>
            {
                new ConsumerWalletDetailsModel
                {
                    Wallet = new WalletModel { WalletId = 12345, DeleteNbr = 0 },
                    WalletType = new WalletTypeModel { WalletTypeId = 1, DeleteNbr = 0 }
                }
            };

            _consumerWalletRepo.Setup(repo => repo.GetConsumerAllWallets(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(walletDetails);
            _walletTypeRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<WalletTypeModel, bool>>>(), false)).ReturnsAsync(new WalletTypeMockModel());
            _walletRepo.Setup(x => x.GetSuspenseWallet(It.IsAny<long>(), It.IsAny<string>(), It.IsAny<string>()));

            // Act
            var result = await _transactionController.ProcessCsaWalletTransactions(requestDto);

            // Assert
            Assert.NotNull(result);
            var objectResult = result as ObjectResult;
            Assert.Equal(StatusCodes.Status404NotFound, objectResult?.StatusCode);
        }
        [Fact]
        public async Task ValidateInputs_ShouldReturnFalse_When_MasterWallet_Notfound()
        {
            // Arrange
            var requestDto = new CsaWalletTransactionsRequestDto
            {
                TenantCode = "ten-ecada21e57154928a2bb959e8365b8b4",
                ConsumerCode = "cmr-dd598745284b4c5ead19539e91ec3872",
                WalletId = 4,
                TenantConfig = "{\"purseConfig\":{\"purses\":[{\"purseLabel\":\"OTC\",\"walletType\":\"wat-2d62dcaf2aa4424b9ff6c2ddb5895077\",\"purseNumber\":12401,\"periodConfig\":{\"fundDate\":19,\"interval\":\"MONTH\",\"applyDateConfig\":{\"applyDate\":1,\"applyDateType\":\"NEXT_MONTH\"}},\"purseWalletType\":\"wat-2d62dcaf2aa4424b9ff6c2ddb5895077\",\"masterWalletType\":\"wat-4b364fg722f04034cv732b355d84f479\",\"masterRedemptionWalletType\":\"wat-bc8f4f7c028d479f900f0af794e385c8\"},{\"purseLabel\":\"FOD\",\"walletType\":\"wat-35d4rt62d77y6119a6t730c9b6447457\",\"purseNumber\":22401,\"periodConfig\":{\"fundDate\":19,\"interval\":\"MONTH\",\"applyDateConfig\":{\"applyDate\":1,\"applyDateType\":\"NEXT_MONTH\"}},\"purseWalletType\":\"wat-35b3ac62d74b4119a5f630c9b6446035\",\"masterWalletType\":\"wat-35d4rt62d77y6119a6t730c9b6447457\",\"masterRedemptionWalletType\":\"wat-01b4a568c1814449b64168bb434127b7\"},{\"purseLabel\":\"REW\",\"walletType\":\"wat-2d62dcaf2aa4424b9ff6c2ddb5895077\",\"purseNumber\":32401,\"periodConfig\":{\"fundDate\":19,\"interval\":\"MONTH\",\"applyDateConfig\":{\"applyDate\":1,\"applyDateType\":\"NEXT_MONTH\"}},\"purseWalletType\":\"wat-55f05107b41642c39e7a3b459223cbd8\",\"masterWalletType\":\"wat-2d62dcaf2aa4424b9ff6c2ddb5895077\",\"masterRedemptionWalletType\":\"wat-274bd71345804f09928cf451dc0f6239\"}]},\"fisProgramDetail\":{\"clientId\":\"1234500\",\"companyId\":\"1204185\",\"packageId\":\"721736\",\"subprogramId\":\"873343\"}}"
            };

            var walletDetails = new List<ConsumerWalletDetailsModel>
            {
                new ConsumerWalletDetailsModel
                {
                    Wallet = new WalletMockModel(),
                    WalletType = new WalletTypeMockModel()
                }
            };

            _consumerWalletRepo.Setup(repo => repo.GetConsumerAllWallets(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(walletDetails);
            _walletTypeRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<WalletTypeModel, bool>>>(), false)).ReturnsAsync(new WalletTypeMockModel());
            _walletRepo.Setup(x => x.GetMasterWallet(It.IsAny<long>(), It.IsAny<string>()));

            // Act
            var result = await _transactionController.ProcessCsaWalletTransactions(requestDto);

            // Assert
            Assert.NotNull(result);
            var objectResult = result as ObjectResult;
            Assert.Equal(StatusCodes.Status404NotFound, objectResult?.StatusCode);
        }

        [Fact]
        public async Task DisposeTransaction_Should_Return_Ok_Response_WhenSuccessful()
        {
            var requestDto = new CsaWalletTransactionsRequestDto
            {
                TenantCode = "ten-ecada21e57154928a2bb959e8365b8b4",
                ConsumerCode = "cmr-dd598745284b4c5ead19539e91ec3872",
                WalletId = 4,
                TenantConfig = "{\"purseConfig\":{\"purses\":[{\"purseLabel\":\"OTC\",\"walletType\":\"wat-4b364fg722f04034cv732b355d84f479\",\"purseNumber\":12401,\"periodConfig\":{\"fundDate\":19,\"interval\":\"MONTH\",\"applyDateConfig\":{\"applyDate\":1,\"applyDateType\":\"NEXT_MONTH\"}},\"purseWalletType\":\"wat-2d62dcaf2aa4424b9ff6c2ddb5895077\",\"masterWalletType\":\"wat-4b364fg722f04034cv732b355d84f479\",\"masterRedemptionWalletType\":\"wat-bc8f4f7c028d479f900f0af794e385c8\"},{\"purseLabel\":\"FOD\",\"walletType\":\"wat-35d4rt62d77y6119a6t730c9b6447457\",\"purseNumber\":22401,\"periodConfig\":{\"fundDate\":19,\"interval\":\"MONTH\",\"applyDateConfig\":{\"applyDate\":1,\"applyDateType\":\"NEXT_MONTH\"}},\"purseWalletType\":\"wat-35b3ac62d74b4119a5f630c9b6446035\",\"masterWalletType\":\"wat-35d4rt62d77y6119a6t730c9b6447457\",\"masterRedemptionWalletType\":\"wat-01b4a568c1814449b64168bb434127b7\"},{\"purseLabel\":\"REW\",\"walletType\":\"wat-2d62dcaf2aa4424b9ff6c2ddb5895077\",\"purseNumber\":32401,\"periodConfig\":{\"fundDate\":19,\"interval\":\"MONTH\",\"applyDateConfig\":{\"applyDate\":1,\"applyDateType\":\"NEXT_MONTH\"}},\"purseWalletType\":\"wat-55f05107b41642c39e7a3b459223cbd8\",\"masterWalletType\":\"wat-2d62dcaf2aa4424b9ff6c2ddb5895077\",\"masterRedemptionWalletType\":\"wat-274bd71345804f09928cf451dc0f6239\"}]},\"fisProgramDetail\":{\"clientId\":\"1234500\",\"companyId\":\"1204185\",\"packageId\":\"721736\",\"subprogramId\":\"873343\"}}"
            };

            var walletDetails = new List<ConsumerWalletDetailsModel>
            {
                new ConsumerWalletDetailsModel
                {
                    Wallet = new WalletMockModel(),
                    WalletType = new WalletTypeMockModel()
                },
                new ConsumerWalletDetailsModel
                {
                    Wallet = new WalletMockModel(),
                    WalletType = new WalletTypeMockModel() { WalletTypeCode="wat-4b364fg722f04034cv732b355d84f479"}
                }
            };
            var transactionMock = new Mock<ITransaction>();
            _consumerWalletRepo.Setup(repo => repo.GetConsumerAllWallets(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(walletDetails);
            _walletTypeRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<WalletTypeModel, bool>>>(), false)).ReturnsAsync(new WalletTypeMockModel());
            _walletRepo.Setup(x => x.GetMasterWallet(10, It.IsAny<string>())).ReturnsAsync(new WalletMockModel());
            _walletRepo.Setup(x => x.GetSuspenseWallet(It.IsAny<long>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(new WalletModel());
            _session.Setup(s => s.BeginTransaction()).Returns(transactionMock.Object);
            // Act
            var result = await _transactionController.ProcessCsaWalletTransactions(requestDto);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<OkObjectResult>(result);
            var objectResult = result as OkObjectResult;
            Assert.Equal(StatusCodes.Status200OK, objectResult?.StatusCode);

        }
        [Fact]
        public async Task ProcessCsa_Transactions_Should_Return_Internal_Server_error_Response_When_Redemption_Exception_Occurs()
        {
            var requestDto = new CsaWalletTransactionsRequestDto
            {
                TenantCode = "ten-ecada21e57154928a2bb959e8365b8b4",
                ConsumerCode = "cmr-dd598745284b4c5ead19539e91ec3872",
                WalletId = 4,
                TenantConfig = "{\"purseConfig\":{\"purses\":[{\"purseLabel\":\"OTC\",\"walletType\":\"wat-4b364fg722f04034cv732b355d84f479\",\"purseNumber\":12401,\"periodConfig\":{\"fundDate\":19,\"interval\":\"MONTH\",\"applyDateConfig\":{\"applyDate\":1,\"applyDateType\":\"NEXT_MONTH\"}},\"purseWalletType\":\"wat-2d62dcaf2aa4424b9ff6c2ddb5895077\",\"masterWalletType\":\"wat-4b364fg722f04034cv732b355d84f479\",\"masterRedemptionWalletType\":\"wat-bc8f4f7c028d479f900f0af794e385c8\"},{\"purseLabel\":\"FOD\",\"walletType\":\"wat-35d4rt62d77y6119a6t730c9b6447457\",\"purseNumber\":22401,\"periodConfig\":{\"fundDate\":19,\"interval\":\"MONTH\",\"applyDateConfig\":{\"applyDate\":1,\"applyDateType\":\"NEXT_MONTH\"}},\"purseWalletType\":\"wat-35b3ac62d74b4119a5f630c9b6446035\",\"masterWalletType\":\"wat-35d4rt62d77y6119a6t730c9b6447457\",\"masterRedemptionWalletType\":\"wat-01b4a568c1814449b64168bb434127b7\"},{\"purseLabel\":\"REW\",\"walletType\":\"wat-2d62dcaf2aa4424b9ff6c2ddb5895077\",\"purseNumber\":32401,\"periodConfig\":{\"fundDate\":19,\"interval\":\"MONTH\",\"applyDateConfig\":{\"applyDate\":1,\"applyDateType\":\"NEXT_MONTH\"}},\"purseWalletType\":\"wat-55f05107b41642c39e7a3b459223cbd8\",\"masterWalletType\":\"wat-2d62dcaf2aa4424b9ff6c2ddb5895077\",\"masterRedemptionWalletType\":\"wat-274bd71345804f09928cf451dc0f6239\"}]},\"fisProgramDetail\":{\"clientId\":\"1234500\",\"companyId\":\"1204185\",\"packageId\":\"721736\",\"subprogramId\":\"873343\"}}"
            };

            var walletDetails = new List<ConsumerWalletDetailsModel>
            {
                new ConsumerWalletDetailsModel
                {
                    Wallet = new WalletMockModel(),
                    WalletType = new WalletTypeMockModel()
                },
                new ConsumerWalletDetailsModel
                {
                    Wallet = new WalletMockModel(),
                    WalletType = new WalletTypeMockModel() { WalletTypeCode="wat-4b364fg722f04034cv732b355d84f479"}
                }
            };
            var transactionMock = new Mock<ITransaction>();
            _consumerWalletRepo.Setup(repo => repo.GetConsumerAllWallets(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(walletDetails);
            _walletTypeRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<WalletTypeModel, bool>>>(), false)).ReturnsAsync(new WalletTypeMockModel());
            _walletRepo.Setup(x => x.GetMasterWallet(10, It.IsAny<string>())).ReturnsAsync(new WalletMockModel());
            _walletRepo.Setup(x => x.GetSuspenseWallet(It.IsAny<long>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(new WalletModel());
            _session.Setup(s => s.BeginTransaction()).Returns(transactionMock.Object);

            _session.Setup(s => s.SaveAsync(It.IsAny<RedemptionModel>(), It.IsAny<CancellationToken>())).ThrowsAsync(new Exception("Testing"));
            // Act
            var result = await _transactionController.ProcessCsaWalletTransactions(requestDto);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<ObjectResult>(result);
            var objectResult = result as ObjectResult;
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult?.StatusCode);

        }
        [Fact]
        public async Task ProcessCsa_Transactions_Should_Return_Internal_Server_error_Response_When_TransactionDetail_Exception_Occurs()
        {
            var requestDto = new CsaWalletTransactionsRequestDto
            {
                TenantCode = "ten-ecada21e57154928a2bb959e8365b8b4",
                ConsumerCode = "cmr-dd598745284b4c5ead19539e91ec3872",
                WalletId = 4,
                TenantConfig = "{\"purseConfig\":{\"purses\":[{\"purseLabel\":\"OTC\",\"walletType\":\"wat-4b364fg722f04034cv732b355d84f479\",\"purseNumber\":12401,\"periodConfig\":{\"fundDate\":19,\"interval\":\"MONTH\",\"applyDateConfig\":{\"applyDate\":1,\"applyDateType\":\"NEXT_MONTH\"}},\"purseWalletType\":\"wat-2d62dcaf2aa4424b9ff6c2ddb5895077\",\"masterWalletType\":\"wat-4b364fg722f04034cv732b355d84f479\",\"masterRedemptionWalletType\":\"wat-bc8f4f7c028d479f900f0af794e385c8\"},{\"purseLabel\":\"FOD\",\"walletType\":\"wat-35d4rt62d77y6119a6t730c9b6447457\",\"purseNumber\":22401,\"periodConfig\":{\"fundDate\":19,\"interval\":\"MONTH\",\"applyDateConfig\":{\"applyDate\":1,\"applyDateType\":\"NEXT_MONTH\"}},\"purseWalletType\":\"wat-35b3ac62d74b4119a5f630c9b6446035\",\"masterWalletType\":\"wat-35d4rt62d77y6119a6t730c9b6447457\",\"masterRedemptionWalletType\":\"wat-01b4a568c1814449b64168bb434127b7\"},{\"purseLabel\":\"REW\",\"walletType\":\"wat-2d62dcaf2aa4424b9ff6c2ddb5895077\",\"purseNumber\":32401,\"periodConfig\":{\"fundDate\":19,\"interval\":\"MONTH\",\"applyDateConfig\":{\"applyDate\":1,\"applyDateType\":\"NEXT_MONTH\"}},\"purseWalletType\":\"wat-55f05107b41642c39e7a3b459223cbd8\",\"masterWalletType\":\"wat-2d62dcaf2aa4424b9ff6c2ddb5895077\",\"masterRedemptionWalletType\":\"wat-274bd71345804f09928cf451dc0f6239\"}]},\"fisProgramDetail\":{\"clientId\":\"1234500\",\"companyId\":\"1204185\",\"packageId\":\"721736\",\"subprogramId\":\"873343\"}}"
            };

            var walletDetails = new List<ConsumerWalletDetailsModel>
            {
                new ConsumerWalletDetailsModel
                {
                    Wallet = new WalletMockModel(),
                    WalletType = new WalletTypeMockModel()
                },
                new ConsumerWalletDetailsModel
                {
                    Wallet = new WalletMockModel(),
                    WalletType = new WalletTypeMockModel() { WalletTypeCode="wat-4b364fg722f04034cv732b355d84f479"}
                }
            };
            var transactionMock = new Mock<ITransaction>();
            _consumerWalletRepo.Setup(repo => repo.GetConsumerAllWallets(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(walletDetails);
            _walletTypeRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<WalletTypeModel, bool>>>(), false)).ReturnsAsync(new WalletTypeMockModel());
            _walletRepo.Setup(x => x.GetSuspenseWallet(It.IsAny<long>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(new WalletModel());
            _session.Setup(s => s.BeginTransaction()).Returns(transactionMock.Object);
            _session.Setup(s => s.SaveAsync(It.IsAny<TransactionDetailModel>(), It.IsAny<CancellationToken>())).ThrowsAsync(new Exception("Testing"));
            // Act
            var result = await _transactionController.ProcessCsaWalletTransactions(requestDto);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<ObjectResult>(result);
            var objectResult = result as ObjectResult;
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult?.StatusCode);

        }
        [Fact]
        public async Task ProcessCsa_Transactions_Should_Return_Internal_Server_error_Response_When_Transaction_Exception_Occurs()
        {
            var requestDto = new CsaWalletTransactionsRequestDto
            {
                TenantCode = "ten-ecada21e57154928a2bb959e8365b8b4",
                ConsumerCode = "cmr-dd598745284b4c5ead19539e91ec3872",
                WalletId = 4,
                TenantConfig = "{\"purseConfig\":{\"purses\":[{\"purseLabel\":\"OTC\",\"walletType\":\"wat-4b364fg722f04034cv732b355d84f479\",\"purseNumber\":12401,\"periodConfig\":{\"fundDate\":19,\"interval\":\"MONTH\",\"applyDateConfig\":{\"applyDate\":1,\"applyDateType\":\"NEXT_MONTH\"}},\"purseWalletType\":\"wat-2d62dcaf2aa4424b9ff6c2ddb5895077\",\"masterWalletType\":\"wat-4b364fg722f04034cv732b355d84f479\",\"masterRedemptionWalletType\":\"wat-bc8f4f7c028d479f900f0af794e385c8\"},{\"purseLabel\":\"FOD\",\"walletType\":\"wat-35d4rt62d77y6119a6t730c9b6447457\",\"purseNumber\":22401,\"periodConfig\":{\"fundDate\":19,\"interval\":\"MONTH\",\"applyDateConfig\":{\"applyDate\":1,\"applyDateType\":\"NEXT_MONTH\"}},\"purseWalletType\":\"wat-35b3ac62d74b4119a5f630c9b6446035\",\"masterWalletType\":\"wat-35d4rt62d77y6119a6t730c9b6447457\",\"masterRedemptionWalletType\":\"wat-01b4a568c1814449b64168bb434127b7\"},{\"purseLabel\":\"REW\",\"walletType\":\"wat-2d62dcaf2aa4424b9ff6c2ddb5895077\",\"purseNumber\":32401,\"periodConfig\":{\"fundDate\":19,\"interval\":\"MONTH\",\"applyDateConfig\":{\"applyDate\":1,\"applyDateType\":\"NEXT_MONTH\"}},\"purseWalletType\":\"wat-55f05107b41642c39e7a3b459223cbd8\",\"masterWalletType\":\"wat-2d62dcaf2aa4424b9ff6c2ddb5895077\",\"masterRedemptionWalletType\":\"wat-274bd71345804f09928cf451dc0f6239\"}]},\"fisProgramDetail\":{\"clientId\":\"1234500\",\"companyId\":\"1204185\",\"packageId\":\"721736\",\"subprogramId\":\"873343\"}}"
            };

            var walletDetails = new List<ConsumerWalletDetailsModel>
            {
                new ConsumerWalletDetailsModel
                {
                    Wallet = new WalletMockModel(),
                    WalletType = new WalletTypeMockModel()
                },
                new ConsumerWalletDetailsModel
                {
                    Wallet = new WalletMockModel(),
                    WalletType = new WalletTypeMockModel() { WalletTypeCode="wat-4b364fg722f04034cv732b355d84f479"}
                }
            };
            var transactionMock = new Mock<ITransaction>();
            _consumerWalletRepo.Setup(repo => repo.GetConsumerAllWallets(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(walletDetails);
            _walletTypeRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<WalletTypeModel, bool>>>(), false)).ReturnsAsync(new WalletTypeMockModel());
            _walletRepo.Setup(x => x.GetMasterWallet(10, It.IsAny<string>())).ReturnsAsync(new WalletMockModel());
            _session.Setup(s => s.BeginTransaction()).Returns(transactionMock.Object);
            _session.Setup(s => s.SaveAsync(It.IsAny<TransactionModel>(), It.IsAny<CancellationToken>())).ThrowsAsync(new Exception("Testing"));
            // Act
            var result = await _transactionController.ProcessCsaWalletTransactions(requestDto);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<ObjectResult>(result);
            var objectResult = result as ObjectResult;
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult?.StatusCode);

        }
        [Fact]
        public async Task ProcessCsa_Transactions_Should_Return_Internal_Server_error_Response_WhenException_Occur()
        {
            var requestDto = new CsaWalletTransactionsRequestDto
            {
                TenantCode = "ten-ecada21e57154928a2bb959e8365b8b4",
                ConsumerCode = "cmr-dd598745284b4c5ead19539e91ec3872",
                WalletId = 4,
                TenantConfig = "{\"purseConfig\":{\"purses\":[{\"purseLabel\":\"OTC\",\"walletType\":\"wat-4b364fg722f04034cv732b355d84f479\",\"purseNumber\":12401,\"periodConfig\":{\"fundDate\":19,\"interval\":\"MONTH\",\"applyDateConfig\":{\"applyDate\":1,\"applyDateType\":\"NEXT_MONTH\"}},\"purseWalletType\":\"wat-2d62dcaf2aa4424b9ff6c2ddb5895077\",\"masterWalletType\":\"wat-4b364fg722f04034cv732b355d84f479\",\"masterRedemptionWalletType\":\"wat-bc8f4f7c028d479f900f0af794e385c8\"},{\"purseLabel\":\"FOD\",\"walletType\":\"wat-35d4rt62d77y6119a6t730c9b6447457\",\"purseNumber\":22401,\"periodConfig\":{\"fundDate\":19,\"interval\":\"MONTH\",\"applyDateConfig\":{\"applyDate\":1,\"applyDateType\":\"NEXT_MONTH\"}},\"purseWalletType\":\"wat-35b3ac62d74b4119a5f630c9b6446035\",\"masterWalletType\":\"wat-35d4rt62d77y6119a6t730c9b6447457\",\"masterRedemptionWalletType\":\"wat-01b4a568c1814449b64168bb434127b7\"},{\"purseLabel\":\"REW\",\"walletType\":\"wat-2d62dcaf2aa4424b9ff6c2ddb5895077\",\"purseNumber\":32401,\"periodConfig\":{\"fundDate\":19,\"interval\":\"MONTH\",\"applyDateConfig\":{\"applyDate\":1,\"applyDateType\":\"NEXT_MONTH\"}},\"purseWalletType\":\"wat-55f05107b41642c39e7a3b459223cbd8\",\"masterWalletType\":\"wat-2d62dcaf2aa4424b9ff6c2ddb5895077\",\"masterRedemptionWalletType\":\"wat-274bd71345804f09928cf451dc0f6239\"}]},\"fisProgramDetail\":{\"clientId\":\"1234500\",\"companyId\":\"1204185\",\"packageId\":\"721736\",\"subprogramId\":\"873343\"}}"
            };

            var walletDetails = new List<ConsumerWalletDetailsModel>
            {
                new ConsumerWalletDetailsModel
                {
                    Wallet = new WalletMockModel(),
                    WalletType = new WalletTypeMockModel()
                },
                new ConsumerWalletDetailsModel
                {
                    Wallet = new WalletMockModel(),
                    WalletType = new WalletTypeMockModel() { WalletTypeCode="wat-4b364fg722f04034cv732b355d84f479"}
                }
            };
            _consumerWalletRepo.Setup(repo => repo.GetConsumerAllWallets(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(walletDetails);
            _walletTypeRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<WalletTypeModel, bool>>>(), false)).ReturnsAsync(new WalletTypeMockModel());
            _walletRepo.Setup(x => x.GetMasterWallet(10, It.IsAny<string>())).ReturnsAsync(new WalletMockModel());
            _walletRepo.Setup(x => x.GetSuspenseWallet(It.IsAny<long>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(new WalletModel());
            // Act
            var result = await _transactionController.ProcessCsaWalletTransactions(requestDto);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<ObjectResult>(result);
            var objectResult = result as ObjectResult;
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult?.StatusCode);
        }
       
        [Fact]
        public async Task GetConsumerWalletTopTransactions_ShouldReturnTopTransactions_WhenWalletIdsProvided()
        {
            // Arrange
            var mockSession = new Mock<NHibernate.ISession>();
            var walletIds = new List<long> { 15, 18, 22345 };
            int count = 2;

            var transactions = new List<TransactionModel>
            {
                new TransactionModel { TransactionId = 1001, WalletId = 15, DeleteNbr = 0, TransactionDetailId = 5001 },
            };
            var transactionsD = new List<TransactionDetailModel>
            {
                new TransactionDetailModel { TransactionDetailId = 5001, Notes="CARD" },
            };

            var mockQueryable = transactions.AsQueryable(); 
            var mockdetailQueryable = transactionsD.AsQueryable(); 

            mockSession.Setup(s => s.Query<TransactionModel>()).Returns(mockQueryable);
            mockSession.Setup(s => s.Query<TransactionDetailModel>()).Returns(mockdetailQueryable);
            var loggerMock = new Mock<ILogger<BaseRepo<TransactionModel>>>();
            var repo = new TransactionRepo(loggerMock.Object, mockSession.Object,_mapper);
            var tran= new List<string>(new[] { "CARD" });
            // Act
            var result = await repo.GetConsumerWalletTopTransactions(walletIds, count, tran);

            // Assert
            Assert.NotNull(result);
        }
        [Fact]
        public async Task GetConsumer_ShouldReturnConsumerResponse_WhenConsumerExists()
        {
            // Arrange
            var request = new GetConsumerRequestDto { ConsumerCode = "consumer-123" };
            var _mockLogger = new Mock<ILogger<ConsumerService>>();
            var expectedResponse = new GetConsumerResponseDto
            {
                Consumer = new ConsumerDto
                {
                    ConsumerCode = "consumer-123",
                    MemberNbr = "001",
                    SubscriberMemberNbr = "001"
                }
            };

            _userClient
                .Setup(x => x.Post<GetConsumerResponseDto>("consumer/get-consumer", request))
                .ReturnsAsync(expectedResponse);

            var consumerService = new ConsumerService(_userClient.Object, _mockLogger.Object);

            // Act
            var result = await consumerService.GetConsumer(request);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Consumer);
            Assert.Equal("consumer-123", result.Consumer.ConsumerCode);
        }
        [Fact]
        public async Task GetConsumer_ShouldReturnEmptyResponse_WhenConsumerIsNull()
        {
            // Arrange
            var request = new GetConsumerRequestDto { ConsumerCode = "missing-consumer" };
            var _mockLogger = new Mock<ILogger<ConsumerService>>();
            var responseWithNullConsumer = new GetConsumerResponseDto
            {
                Consumer = null
            };

            _userClient
                .Setup(x => x.Post<GetConsumerResponseDto>("consumer/get-consumer", request))
                .ReturnsAsync(responseWithNullConsumer);

            var consumerService = new ConsumerService(_userClient.Object, _mockLogger.Object);

            // Act
            var result = await consumerService.GetConsumer(request);

            // Assert
            Assert.NotNull(result);
            Assert.Null(result.Consumer);
        }


        [Fact]
        public async Task Wallet_Transactions_Should_Return_Ok_Response()
        {
            // Arrange
            var requestDto = new GetWalletTransactionRequestDto();
            _consumerWalletRepo.Setup(x=>x.GetConsumerWallets(It.IsAny<List<string>>(),It.IsAny<string>())).ReturnsAsync(new List<WalletModel>() { new WalletMockModel()});
            _transactionRepository.Setup(x=>x.GetWalletTransactionsQueryable(It.IsAny<List<long>>())).Returns(new List<TransactionEntryDto>() { new TransactionEntryDto() }.AsQueryable());

            // Act
            var result = await _transactionController.GetRewardWalletTransactions(requestDto);

            // Assert
            Assert.NotNull(result);
            var okObjectResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(StatusCodes.Status200OK,okObjectResult?.StatusCode);
        }
        [Fact]
        public async Task Wallet_Transactions_Should_Return_Error_Response()
        {
            // Arrange
            var requestDto = new GetWalletTransactionRequestDto();
            _consumerWalletRepo.Setup(x => x.GetConsumerWallets(It.IsAny<List<string>>(), It.IsAny<string>())).ReturnsAsync(new List<WalletModel>() { new WalletMockModel() });
            _transactionRepository.Setup(x => x.GetWalletTransactionsQueryable(It.IsAny<List<long>>())).Throws(new Exception("testing"));

            // Act
            var result = await _transactionController.GetRewardWalletTransactions(requestDto);

            // Assert
            Assert.NotNull(result);
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult?.StatusCode);
        }

    }
}

