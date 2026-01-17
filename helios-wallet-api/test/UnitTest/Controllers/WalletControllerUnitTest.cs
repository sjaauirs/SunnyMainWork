using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using NHibernate;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.Common.Core.Repositories;
using SunnyRewards.Helios.Common.Core.Services.Interfaces;
using SunnyRewards.Helios.Wallet.Api.Controllers;
using SunnyRewards.Helios.Wallet.Core.Domain.Constants;
using SunnyRewards.Helios.Wallet.Core.Domain.Dtos;
using SunnyRewards.Helios.Wallet.Core.Domain.Dtos.Json;
using SunnyRewards.Helios.Wallet.Core.Domain.Models;
using SunnyRewards.Helios.Wallet.Infrastructure.Helpers.Interfaces;
using SunnyRewards.Helios.Wallet.Infrastructure.HttpClients.Interfaces;
using SunnyRewards.Helios.Wallet.Infrastructure.Repositories;
using SunnyRewards.Helios.Wallet.Infrastructure.Repositories.Interfaces;
using SunnyRewards.Helios.Wallet.Infrastructure.Services;
using SunnyRewards.Helios.Wallet.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.Wallet.UnitTest.Fixtures.MockDto;
using SunnyRewards.Helios.Wallet.UnitTest.Fixtures.MockHelpers;
using SunnyRewards.Helios.Wallet.UnitTest.Fixtures.MockModel;
using SunnyRewards.Helios.Wallet.UnitTest.Fixtures.MockRepositories;
using SunnyRewards.Helios.Wallet.UnitTest.Helpers.HttpClientsMock;
using System.Linq.Expressions;
using Xunit;

namespace SunnyRewards.Helios.Wallet.UnitTest.Controllers
{
    public class WalletControllerUnitTest
    {
        private readonly Mock<ILogger<WalletController>> _walletLogger;
        private readonly Mock<ILogger<WalletService>> _walletServiceLogger;
        private readonly Mock<ILogger<TransactionService>> _transactionServiceLogger;
        private readonly IMapper _mapper;
        private readonly Mock<IWalletRepo> _walletRepository;
        private readonly Mock<IWalletTypeRepo> _walletTypeRepo;
        private readonly Mock<IWalletRepo> _walletRepo;
        private readonly Mock<ITransactionRepo> _transactionRepo;
        private readonly Mock<ITransactionDetailRepo> _transactionDetailRepo;
        private readonly Mock<NHibernate.ISession> _session;
        private readonly Mock<IRedemptionRepo> _redemptionRepo;
        private readonly IConfiguration _configuration;
        private readonly Mock<IAuditTrailService> _auditTrailService;
        private readonly IWalletService _walletService;
        private readonly ITransactionService _transactionService;
        private readonly WalletController _walletController;
        private readonly Mock<IConsumerWalletRepo> _consumerWalletRepo;
        private readonly Mock<IUserClient> _userClient;
        private readonly Mock<IWalletService> _walletServiceMock;
        private readonly Mock<ISecretHelper> _secretHelper;
        private readonly Mock<IWalletTypeTransferRuleRepo> _walletTypeTransferRuleRepo;
        private readonly Mock<IConsumerWalletService> _consumerWalletServiceMock;
        private readonly Mock<ILogger<ConsumerWalletService>> _consumerWalletServiceLogger;
        private readonly Mock<IWalletTypeRepo> _walletTypeRepository;
        private readonly Mock<IConsumerWalletRepo> _consumerWalletRepository;
        private readonly Mock<IConsumerService> _mockConsumerService;


        public WalletControllerUnitTest()
        {
            _walletLogger = new Mock<ILogger<WalletController>>();
            _walletServiceLogger = new Mock<ILogger<WalletService>>();
            _transactionServiceLogger = new Mock<ILogger<TransactionService>>();
            _consumerWalletServiceLogger = new Mock<ILogger<ConsumerWalletService>>();
            _walletRepository = new WalletMockRepo();
            _walletTypeRepo = new WalletTypeMockRepo();
            _consumerWalletRepository = new ConsumerWalletMockRepo();
            _walletRepo = new WalletMockRepo();
            _walletTypeRepository = new WalletTypeMockRepo();
            _transactionRepo = new TransactionMockRepo();
            _transactionDetailRepo = new TransactionDetailMockRepo();
            _session = new Mock<NHibernate.ISession>();
            _redemptionRepo = new RedemptionMockRepo();
            _consumerWalletRepo = new ConsumerWalletMockRepo();
            _walletServiceMock = new Mock<IWalletService>();
            _auditTrailService = new Mock<IAuditTrailService>();
            _secretHelper = new MockSecretHelper();
            _configuration = new ConfigurationBuilder()
           .SetBasePath(Directory.GetCurrentDirectory())
           .AddJsonFile("appsettings.json")
           .AddJsonFile("appsettings.Development.json")
           .Build();
            _userClient = new UserClientMock();
            _mockConsumerService = new Mock<IConsumerService>();
            _mapper = new Mapper(new MapperConfiguration(
                configure =>
                {
                    configure.AddMaps(typeof(Infrastructure.Mappings.MappingProfile.WalletMapping).Assembly.FullName);
                }));
            _walletTypeTransferRuleRepo = new Mock<IWalletTypeTransferRuleRepo>();
            _transactionService = new TransactionService(_transactionServiceLogger.Object, _mapper, _consumerWalletRepo.Object, _transactionRepo.Object,
                _transactionDetailRepo.Object, _walletRepo.Object, _walletTypeRepo.Object, _session.Object, _configuration, _userClient.Object, _mockConsumerService.Object);
            _consumerWalletServiceMock = new Mock<IConsumerWalletService>();
            _walletService = new WalletService(_walletServiceLogger.Object, _mapper, _walletRepository.Object, _walletTypeRepo.Object,
             _transactionRepo.Object, _transactionDetailRepo.Object, _consumerWalletRepo.Object, _session.Object, _redemptionRepo.Object, _configuration, _auditTrailService.Object, _transactionService, _secretHelper.Object, _walletTypeTransferRuleRepo.Object, _consumerWalletServiceMock.Object
             , _mockConsumerService.Object);
            _walletController = new WalletController(_walletLogger.Object, _walletService);

        }

        [Fact]
        public async Task Should_Post_RedeemStart_WithWalletId()
        {
            var transactionMock = new Mock<ITransaction>();
            _session.Setup(s => s.BeginTransaction()).Returns(transactionMock.Object);
            _walletTypeRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<WalletTypeModel, bool>>>(), false))
                 .ReturnsAsync(new WalletTypeMockModel());
            var postRedeemStartRequestMockDto = new PostRedeemStartRequestMockDto()
            {
                ConsumerWalletTypeCode = "",
                RedemptionWalletTypeCode = "redwal-3d62dcaf2aa4424b9ff6c2ddb5895077",
                TenantCode = "ten-ecada21e57154928a2bb959e8365b8b4",
                ConsumerCode = "cus-04c211b4339348509eaa870cdea59600",
                RedemptionVendorCode = "PRIZE_OUT1233",
                RedemptionAmount = 5000,
                RedemptionRef = "5",
                RedemptionItemDescription = "Gift card redeemed",
                Notes = "Ok",
                RedemptionItemData = "Dominos",
                WalletId = 1
            };
            _walletRepository.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<WalletModel, bool>>>(), false))
                .ReturnsAsync(new WalletMockModel());
            var WalletModelMock = new List<WalletModel>() { new WalletModel() };
            _walletRepository.Setup(repo => repo.FindAsync(It.IsAny<Expression<Func<WalletModel, bool>>>(), false))
                           .ReturnsAsync(WalletModelMock);
            var response = await _walletController.RedeemStart(postRedeemStartRequestMockDto);
            var result = response.Result as OkObjectResult;

            Assert.True(result?.Value != null);
            Assert.True(result.StatusCode == 200);

        }

        [Fact]
        public async Task Should_Post_RedeemStart()
        {
            var transactionMock = new Mock<ITransaction>();
            _session.Setup(s => s.BeginTransaction()).Returns(transactionMock.Object);
            _walletTypeRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<WalletTypeModel, bool>>>(), false))
                 .ReturnsAsync(new WalletTypeMockModel());
            var postRedeemStartRequestMockDto = new PostRedeemStartRequestMockDto();
            _walletRepository.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<WalletModel, bool>>>(), false))
                .ReturnsAsync(new WalletMockModel());
            var WalletModelMock = new List<WalletModel>() { new WalletModel() };
            _walletRepository.Setup(repo => repo.FindAsync(It.IsAny<Expression<Func<WalletModel, bool>>>(), false))
                           .ReturnsAsync(WalletModelMock);
            var response = await _walletController.RedeemStart(postRedeemStartRequestMockDto);
            var result = response.Result as OkObjectResult;
            Assert.True(result?.Value != null);
            Assert.True(result.StatusCode == 200);

        }

        [Fact]
        public async Task Should_Get_WalletId()
        {
            var walletMockDto = await _walletController.GetWallet(3);
            var result = walletMockDto.Result as OkObjectResult;
            Assert.True(result?.Value != null);
            Assert.True(result.StatusCode == 200);
        }

        [Fact]
        public async Task Should_not_Get_Wallet_For_Invalid_WalletId()
        {
            _walletRepository.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<WalletModel, bool>>>(), false))
                .ReturnsAsync(new WalletMockModel());
            var wallet = await _walletController.GetWallet(0);
            var result = wallet.Result as BadRequestObjectResult;
            Assert.True(result?.Value == null);
        }
        [Fact]
        public async Task Should_not_Get_Wallet_For_Invalid_WalletId_Exception()
        {
            var loggerMock = new Mock<ILogger<WalletController>>();
            var serviceMock = new Mock<IWalletService>();
            var controller = new WalletController(loggerMock.Object, serviceMock.Object);

            var walletDto = new WalletDto();
            var expectedException = new Exception("Test exception");
            serviceMock.Setup(x => x.GetWalletData(walletDto.WalletId))
           .ThrowsAsync(expectedException);
            var response = await controller.GetWallet(walletDto.WalletId);
            Assert.False(response == null);
        }
        [Fact]
        public async Task Should_Throw_Exception_For_Invalid_WalletId()
        {
            var loggerMock = new Mock<ILogger<WalletService>>();
            var walletRepoMock = new Mock<IWalletRepo>();
            var walletMapperMock = new Mock<IMapper>();
            var walletTypeRepoMock = new Mock<IWalletTypeRepo>();
            var transactionRepoMock = new Mock<ITransactionRepo>();
            var transactionDetailRepoMock = new Mock<ITransactionDetailRepo>();
            var session = new Mock<NHibernate.ISession>();
            var redemptionRepoMock = new Mock<IRedemptionRepo>();

            var configuration = new ConfigurationBuilder()
           .SetBasePath(Directory.GetCurrentDirectory())
           .AddJsonFile("appsettings.json")
           .Build();
            var auditTrail = new Mock<IAuditTrailService>();

            var walletService = new WalletService(loggerMock.Object, walletMapperMock.Object, walletRepoMock.Object, walletTypeRepoMock.Object,
                transactionRepoMock.Object, transactionDetailRepoMock.Object, _consumerWalletRepo.Object, session.Object, redemptionRepoMock.Object,
                configuration, auditTrail.Object, _transactionService, _secretHelper.Object, _walletTypeTransferRuleRepo.Object, _consumerWalletServiceMock.Object, _mockConsumerService.Object);

            long invalidWalletId = 123;
            walletRepoMock.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<WalletModel, bool>>>(), false))
                          .ThrowsAsync(new Exception("Test exception"));

            await Assert.ThrowsAsync<Exception>(async () => await walletService.GetWalletData(invalidWalletId));
        }

        [Fact]
        public async Task Should_not_Post_Reward_Wallet_For_Invalid_Exception()
        {
            var postRewardRequestDto = new PostRewardRequestDto();
            var response = await _walletService.RewardDetailsOuter(postRewardRequestDto);
            Assert.False(response == null);
        }

        [Fact]
        public async Task Should_GetWalletTypeCode_Controller()
        {
            var walletTypeMockDto = new WalletTypeMockDto();
            var walletMockDto = await _walletController.GetWalletTypeCode(walletTypeMockDto);
            var result = walletMockDto.Result as OkObjectResult;
            Assert.True(result?.Value != null);
            Assert.True(result.StatusCode == 200);
        }

        [Fact]
        public async Task Should_GetWalletTypeCode_For_NotFound_Controller()
        {
            var walletTypeMockDto = new WalletTypeMockDto();
            _walletTypeRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<WalletTypeModel, bool>>>(), false)).ReturnsAsync((WalletTypeMockModel)null);
            var result = await _walletController.GetWalletTypeCode(walletTypeMockDto);
            Assert.True(result?.Value == null);
        }

        [Fact]
        public async Task Should_Return_GetWalletTypeCode_Catch_Exception_For_Controller()
        {
            var walletTypeMockDto = new WalletTypeMockDto();
            var walletTypeService = new Mock<IWalletService>();
            var walletController = new WalletController(_walletLogger.Object, walletTypeService.Object);
            walletTypeService.Setup(x => x.GetWalletTypeCode(It.IsAny<WalletTypeDto>()))
               .ThrowsAsync(new Exception("Transaction Exception"));
            var response = await walletController.GetWalletTypeCode(walletTypeMockDto);
            Assert.True(response.Result == null);

        }

        [Fact]
        public async Task Should_GetWalletTypeCode_WalletService()
        {
            var walletTypeMockDto = new WalletTypeMockDto();
            var walletMockDto = await _walletService.GetWalletTypeCode(walletTypeMockDto);
            Assert.True(walletMockDto != null);
        }

        [Fact]
        public async Task Should_GetWalletTypeCode_For_NotFound_WalletService()
        {
            var walletTypeMockDto = new WalletTypeMockDto();
            _walletTypeRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<WalletTypeModel, bool>>>(), false)).ReturnsAsync((WalletTypeMockModel)null);
            var result = await _walletService.GetWalletTypeCode(walletTypeMockDto);
            Assert.True(result?.WalletTypeLabel == null);
            Assert.True(result?.WalletTypeName == null);
        }

        [Fact]
        public async Task Should_Return_GetWalletTypeCode_Catch_Exception_For_WalletService()
        {
            var walletTypeMockDto = new WalletTypeMockDto();

            _walletTypeRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<WalletTypeModel, bool>>>(), false))
               .ThrowsAsync(new Exception("Transaction Exception"));
            await Assert.ThrowsAsync<Exception>(async () => await _walletService.GetWalletTypeCode(walletTypeMockDto));

        }

        [Fact]
        public async Task Should_Post_Reward()
        {
            var transactionMock = new Mock<ITransaction>();
            _session.Setup(s => s.BeginTransaction()).Returns(transactionMock.Object);
            _walletTypeRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<WalletTypeModel, bool>>>(), false))
                .ReturnsAsync(new WalletTypeMockModel());
            var postRewardRequestMockDto = new PostRewardRequestMockDto();
            var response = await _walletController.PostReward(postRewardRequestMockDto);
            var result = response.Result as OkObjectResult;
            Assert.True(result?.Value != null);
            Assert.True(result.StatusCode == 200);
        }

        [Fact]
        public async Task Should_Return_Catch_Exception_Post_Reward()
        {
            var walletLogger = new Mock<ILogger<WalletController>>();
            var walletService = new Mock<IWalletService>();
            var walletController = new WalletController(walletLogger.Object, walletService.Object);
            var postRewardRequestMock = new PostRewardRequestMockDto();
            walletService.Setup(x => x.RewardDetailsOuter(It.IsAny<PostRewardRequestMockDto>())).
                ThrowsAsync(new Exception("Post_Reward Exception"));
            var response = await walletController.PostReward(postRewardRequestMock);
            var result = response.Result as ObjectResult;
            Assert.True(result == null);
        }

        [Fact]
        public async Task Catch_Exception_Post_Reward_Service()
        {
            var walletService = new Mock<IWalletService>();
            var walletController = new WalletController(_walletLogger.Object, _walletService);
            var postRewardRequestMock = new PostRewardRequestMockDto();
            walletService.Setup(x => x.RewardDetailsOuter(It.IsAny<PostRewardRequestMockDto>()))
                .ThrowsAsync(new Exception("Simulated exception message"));
            Exception? exception = null;
            try
            {
                await walletController.PostReward(postRewardRequestMock);
            }
            catch (Exception ex)
            {
                exception = ex;
            }
            Assert.True(exception == null);
        }

        [Fact]
        public async Task RewardDetails_Concurrency_Error_Handling()
        {
            var walletTypeRepo = new Mock<IWalletTypeRepo>();
            var walletRepo = new Mock<IWalletRepo>();
            var transactionMock = new Mock<ITransaction>();
            _session.Setup(s => s.BeginTransaction()).Returns(transactionMock.Object);

            var walletService = new WalletService(_walletServiceLogger.Object, _mapper, walletRepo.Object, walletTypeRepo.Object,
              _transactionRepo.Object, _transactionDetailRepo.Object, _consumerWalletRepo.Object, _session.Object, _redemptionRepo.Object, _configuration,
              _auditTrailService.Object, _transactionService, _secretHelper.Object, _walletTypeTransferRuleRepo.Object, _consumerWalletServiceMock.Object, _mockConsumerService.Object);

            walletTypeRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<WalletTypeModel, bool>>>(), false))
                .ReturnsAsync(new WalletTypeMockModel());

            walletRepo.Setup(x => x.GetMasterWallet(It.IsAny<int>(), It.IsAny<string>())).ThrowsAsync(new Exception("test Exception"));

            var postRewardRequestDto = new PostRewardRequestDto();
            var result = await walletService.RewardDetails(postRewardRequestDto);
            Assert.True(result.ErrorCode == 400);
        }

        [Fact]
        public async Task RewardDetails_ExceptionHandling()
        {
            var walletRepo = new Mock<IWalletRepo>();
            var walletTypeRepo = new Mock<IWalletTypeRepo>();
            var transactionMock = new Mock<ITransaction>();
            _session.Setup(s => s.BeginTransaction()).Returns(transactionMock.Object);

            var walletService = new WalletService(_walletServiceLogger.Object, _mapper, walletRepo.Object, walletTypeRepo.Object,
              _transactionRepo.Object, _transactionDetailRepo.Object, _consumerWalletRepo.Object, _session.Object, _redemptionRepo.Object, _configuration,
              _auditTrailService.Object, _transactionService, _secretHelper.Object, _walletTypeTransferRuleRepo.Object, _consumerWalletServiceMock.Object, _mockConsumerService.Object);

            walletRepo.Setup(x => x.GetConsumerWallet(It.IsAny<int>(), It.IsAny<string>())).ReturnsAsync(new WalletMockModel());
            walletTypeRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<WalletTypeModel, bool>>>(), false))
                .ReturnsAsync(new WalletTypeMockModel());

            var postRewardRequestDto = new PostRewardRequestDto();
            var result = await walletService.RewardDetails(postRewardRequestDto);
            Assert.Null(result?.PostRewardResponses?[0].TransactionDetail);
            Assert.Null(result?.PostRewardResponses?[0].AddEntry);
            Assert.Null(result?.PostRewardResponses?[0].SubEntry);
        }

        [Fact]
        public async Task Should_Return_Catch_Exception_RedeemStart()
        {
            var walletLogger = new Mock<ILogger<WalletController>>();
            var walletService = new Mock<IWalletService>();
            var walletController = new WalletController(walletLogger.Object, walletService.Object);
            var postRedeemStartRequestMockDto = new PostRedeemStartRequestMockDto();
            walletService.Setup(x => x.RedeemStartOuter(It.IsAny<PostRedeemStartRequestMockDto>())).
                ThrowsAsync(new Exception("RedeemStart Exception"));
            var response = await walletController.RedeemStart(postRedeemStartRequestMockDto);
            var result = response.Result as ObjectResult;
            Assert.True(result == null);
        }

        [Fact]
        public async Task Catch_Exception_Post_RedeemStart_Service()
        {
            var walletService = new Mock<IWalletService>();
            var walletController = new WalletController(_walletLogger.Object, _walletService);
            var postRedeemStartRequestMockDto = new PostRedeemStartRequestMockDto();
            walletService.Setup(x => x.RedeemStartOuter(It.IsAny<PostRedeemStartRequestMockDto>()))
                .ThrowsAsync(new Exception("Simulated exception message"));
            Exception? exception = null;
            try
            {
                await walletController.RedeemStart(postRedeemStartRequestMockDto);
            }
            catch (Exception ex)
            {
                exception = ex;
            }
            Assert.True(exception == null);
        }

        [Fact]
        public async Task RedeemStart_Concurrency_Error_Handling()
        {
            var walletTypeRepo = new Mock<IWalletTypeRepo>();
            var walletRepo = new Mock<IWalletRepo>();
            var transactionRepo = new Mock<ITransactionRepo>();
            var transactionMock = new Mock<ITransaction>();
            _session.Setup(s => s.BeginTransaction()).Returns(transactionMock.Object);

            var walletService = new WalletService(_walletServiceLogger.Object, _mapper, walletRepo.Object, walletTypeRepo.Object,
              transactionRepo.Object, _transactionDetailRepo.Object, _consumerWalletRepo.Object, _session.Object, _redemptionRepo.Object, _configuration,
              _auditTrailService.Object, _transactionService, _secretHelper.Object, _walletTypeTransferRuleRepo.Object, _consumerWalletServiceMock.Object, _mockConsumerService.Object);

            walletTypeRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<WalletTypeModel, bool>>>(), false))
                .ReturnsAsync(new WalletTypeMockModel());
            walletRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<WalletModel, bool>>>(), false)).ReturnsAsync(new WalletMockModel());

            transactionRepo.Setup(x => x.GetMaxTransactionIdByWallet(It.IsAny<int>())).ThrowsAsync(new Exception("test Exception"));
            var postRedeemStartRequestMockDto = new PostRedeemStartRequestMockDto();
            var result = await walletService.RedeemStart(postRedeemStartRequestMockDto);
            Assert.True(result.ErrorCode == 404);
        }

        [Fact]
        public async Task RedeemStart_ExceptionHandling()
        {
            var walletRepo = new Mock<IWalletRepo>();
            var walletTypeRepo = new Mock<IWalletTypeRepo>();
            var transactionRepo = new Mock<ITransactionRepo>();
            var transactionMock = new Mock<ITransaction>();
            var session = new Mock<NHibernate.ISession>();
            session.Setup(s => s.BeginTransaction()).Returns(transactionMock.Object);

            var walletService = new WalletService(_walletServiceLogger.Object, _mapper, walletRepo.Object, walletTypeRepo.Object,
             transactionRepo.Object, _transactionDetailRepo.Object, _consumerWalletRepo.Object, session.Object, _redemptionRepo.Object, _configuration,
              _auditTrailService.Object, _transactionService, _secretHelper.Object, _walletTypeTransferRuleRepo.Object, _consumerWalletServiceMock.Object, _mockConsumerService.Object);

            walletTypeRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<WalletTypeModel, bool>>>(), false))
                .ReturnsAsync(new WalletTypeMockModel());
            walletRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<WalletModel, bool>>>(), false)).ReturnsAsync(new WalletMockModel());

            transactionRepo.Setup(x => x.GetMaxTransactionIdByWallet(It.IsAny<long>())).ReturnsAsync(4);

            transactionRepo.Setup(repo => repo.FindOneAsync(It.IsAny<Expression<Func<TransactionModel, bool>>>(), false))
                           .ReturnsAsync(new TransactionMockModel());

            walletRepo.Setup(x => x.UpdateRedemptionWalletBalance(It.IsAny<DateTime>(), It.IsAny<double>(), It.IsAny<long>(), It.IsAny<int>()))
                .Throws(new Exception("Simulated exception"));

            var postRedeemStartRequestMockDto = new PostRedeemStartRequestMockDto();
            var result = await walletService.RedeemStart(postRedeemStartRequestMockDto);
            Assert.True(result.TransactionDetail == null);
            Assert.True(result.AddEntry == null);
            Assert.True(result.SubEntry == null);
            Assert.True(result.Redemption == null);
        }

        [Fact]
        public async Task Should_Post_RedeemComplete()
        {
            var transactionMock = new Mock<ITransaction>();
            _session.Setup(s => s.BeginTransaction()).Returns(transactionMock.Object);
            _redemptionRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<RedemptionModel, bool>>>(), false))
                 .ReturnsAsync(new RedemptionMockModel());

            _transactionRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<TransactionModel, bool>>>(), false)).
               ReturnsAsync(new TransactionMockModel());

            _transactionDetailRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<TransactionDetailModel, bool>>>(), false))
                .ReturnsAsync(new TransactionDetailMockModel());
            var postRedeemCompleteRequest = new PostRedeemCompleteRequestMockDto();
            var response = await _walletController.RedeemComplete(postRedeemCompleteRequest);
            var result = response.Result as OkObjectResult;
            Assert.True(result?.Value != null);
            Assert.True(result.StatusCode == 200);
        }

        [Fact]
        public async Task Should_Return_Catch_Exception_RedeemComplete()
        {
            var walletLogger = new Mock<ILogger<WalletController>>();
            var walletService = new Mock<IWalletService>();
            var walletController = new WalletController(walletLogger.Object, walletService.Object);
            var postRedeemCompleteRequest = new PostRedeemCompleteRequestMockDto();
            walletService.Setup(x => x.RedeemCompleteOuter(It.IsAny<PostRedeemCompleteRequestMockDto>())).
                ThrowsAsync(new Exception("RedeemComplete Exception"));
            var response = await walletController.RedeemComplete(postRedeemCompleteRequest);
            var result = response.Result as ObjectResult;
            Assert.True(result == null);
        }

        [Fact]
        public async Task Catch_Exception_Post_RedeemComplete_Service()
        {
            var walletService = new Mock<IWalletService>();
            var walletController = new WalletController(_walletLogger.Object, _walletService);
            var postRedeemCompleteRequest = new PostRedeemCompleteRequestMockDto();
            walletService.Setup(x => x.RedeemCompleteOuter(It.IsAny<PostRedeemCompleteRequestMockDto>()))
                .ThrowsAsync(new Exception("Simulated exception message"));
            Exception? exception = null;
            try
            {
                await walletController.RedeemComplete(postRedeemCompleteRequest);
            }
            catch (Exception ex)
            {
                exception = ex;
            }
            Assert.True(exception == null);
        }

        [Fact]
        public async Task RedeemComplete_Concurrency_Error_Handling()
        {
            var transactionRepo = new Mock<ITransactionRepo>();
            var transactionMock = new Mock<ITransaction>();
            var transactionDetailRepo = new Mock<ITransactionDetailRepo>();
            var redemptionRepo = new Mock<IRedemptionRepo>();
            _session.Setup(s => s.BeginTransaction()).Returns(transactionMock.Object);

            var walletService = new WalletService(_walletServiceLogger.Object, _mapper, _walletRepository.Object, _walletTypeRepo.Object,
              transactionRepo.Object, _transactionDetailRepo.Object, _consumerWalletRepo.Object, _session.Object, redemptionRepo.Object, _configuration,
              _auditTrailService.Object, _transactionService, _secretHelper.Object, _walletTypeTransferRuleRepo.Object, _consumerWalletServiceMock.Object, _mockConsumerService.Object);

            redemptionRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<RedemptionModel, bool>>>(), false)).ReturnsAsync(new RedemptionMockModel());
            transactionRepo.Setup(repo => repo.FindOneAsync(It.IsAny<Expression<Func<TransactionModel, bool>>>(), false))
                          .ReturnsAsync(new TransactionMockModel());
            transactionDetailRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<TransactionDetailModel, bool>>>(), false)).ReturnsAsync(new TransactionDetailMockModel());

            transactionRepo.Setup(x => x.GetMaxTransactionIdByWallet(It.IsAny<int>())).ThrowsAsync(new Exception("test Exception"));
            var postRedeemCompleteRequest = new PostRedeemCompleteRequestMockDto();
            var result = await walletService.RedeemComplete(postRedeemCompleteRequest);
            Assert.True(result.ErrorCode == 409);
        }

        [Fact]
        public async Task RedeemComplete_ExceptionHandling()
        {
            var transactionRepo = new Mock<ITransactionRepo>();
            var transactionMock = new Mock<ITransaction>();
            var redemptionRepo = new Mock<IRedemptionRepo>();
            _session.Setup(s => s.BeginTransaction()).Returns(transactionMock.Object);

            var walletService = new WalletService(_walletServiceLogger.Object, _mapper, _walletRepository.Object, _walletTypeRepo.Object,
                _transactionRepo.Object, _transactionDetailRepo.Object, _consumerWalletRepo.Object, _session.Object, redemptionRepo.Object, _configuration,
                _auditTrailService.Object, _transactionService, _secretHelper.Object, _walletTypeTransferRuleRepo.Object, _consumerWalletServiceMock.Object, _mockConsumerService.Object);

            redemptionRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<RedemptionModel, bool>>>(), false)).ReturnsAsync(new RedemptionMockModel());
            transactionRepo.Setup(repo => repo.FindOneAsync(It.IsAny<Expression<Func<TransactionModel, bool>>>(), false))
                          .ReturnsAsync(new TransactionMockModel());

            var postRedeemCompleteRequest = new PostRedeemCompleteRequestMockDto();
            var result = await walletService.RedeemComplete(postRedeemCompleteRequest);
            Assert.True(result.Redemption == null);
        }

        [Fact]
        public async Task Should_Post_RedeemFail()
        {
            var transactionMock = new Mock<ITransaction>();
            _session.Setup(s => s.BeginTransaction()).Returns(transactionMock.Object);
            _redemptionRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<RedemptionModel, bool>>>(), false))
                .ReturnsAsync(new RedemptionMockModel());
            _transactionRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<TransactionModel, bool>>>(), false)).
                ReturnsAsync(new TransactionMockModel());
            var postRedeemFailRequestDto = new PostRedeemFailRequestMockDto();
            var response = await _walletController.RedeemFail(postRedeemFailRequestDto);
            var result = response.Result as OkObjectResult;
            Assert.True(result?.Value != null);
            Assert.True(result.StatusCode == 200);
        }

        [Fact]
        public async Task Should_Return_Catch_Exception_RedeemFail()
        {
            var walletLogger = new Mock<ILogger<WalletController>>();
            var walletService = new Mock<IWalletService>();
            var walletController = new WalletController(walletLogger.Object, walletService.Object);
            var postRedeemFailRequestDto = new PostRedeemFailRequestMockDto();
            walletService.Setup(x => x.RedeemFailOuter(It.IsAny<PostRedeemFailRequestMockDto>())).
                ThrowsAsync(new Exception("RedeemFail Exception"));
            var response = await walletController.RedeemFail(postRedeemFailRequestDto);
            var result = response.Result as ObjectResult;
            Assert.True(result == null);
        }

        [Fact]
        public async Task Catch_Exception_Post_RedeemFail_Service()
        {
            var walletService = new Mock<IWalletService>();
            var walletController = new WalletController(_walletLogger.Object, _walletService);
            var postRedeemFailRequestDto = new PostRedeemFailRequestMockDto();
            walletService.Setup(x => x.RedeemFailOuter(It.IsAny<PostRedeemFailRequestMockDto>()))
                .ThrowsAsync(new Exception("Simulated exception message"));
            Exception? exception = null;
            try
            {
                await walletController.RedeemFail(postRedeemFailRequestDto);
            }
            catch (Exception ex)
            {
                exception = ex;
            }
            Assert.True(exception == null);
        }

        [Fact]
        public async Task RedeemFail_Concurrency_Error_Handling()
        {
            var transactionRepo = new Mock<ITransactionRepo>();
            var redemptionRepo = new Mock<IRedemptionRepo>();
            var walletRepo = new Mock<IWalletRepo>();
            var transactionMock = new Mock<ITransaction>();

            _session.Setup(s => s.BeginTransaction()).Returns(transactionMock.Object);

            var walletService = new WalletService(_walletServiceLogger.Object, _mapper, walletRepo.Object, _walletTypeRepo.Object,
              transactionRepo.Object, _transactionDetailRepo.Object, _consumerWalletRepo.Object, _session.Object, redemptionRepo.Object, _configuration,
              _auditTrailService.Object, _transactionService, _secretHelper.Object, _walletTypeTransferRuleRepo.Object, _consumerWalletServiceMock.Object, _mockConsumerService.Object);

            redemptionRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<RedemptionModel, bool>>>(), false)).ReturnsAsync(new RedemptionMockModel());
            transactionRepo.Setup(repo => repo.FindOneAsync(It.IsAny<Expression<Func<TransactionModel, bool>>>(), false))
                          .ReturnsAsync(new TransactionMockModel());

            walletRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<WalletModel, bool>>>(), false)).ReturnsAsync(new WalletMockModel());

            walletRepo.Setup(x => x.UpdateRedemptionWalletBalance(It.IsAny<DateTime>(), It.IsAny<double>(), It.IsAny<long>(), It.IsAny<int>())).Returns(0);

            var postRedeemFailRequestDto = new PostRedeemFailRequestMockDto();
            var result = await walletService.RedeemFail(postRedeemFailRequestDto);
            Assert.True(result.ErrorCode == 409);
        }

        [Fact]
        public async Task RedeemFail_ExceptionHandling()
        {
            var transactionRepo = new Mock<ITransactionRepo>();
            var redemptionRepo = new Mock<IRedemptionRepo>();
            var walletRepo = new Mock<IWalletRepo>();
            var transactionMock = new Mock<ITransaction>();
            var session = new Mock<NHibernate.ISession>();
            session.Setup(s => s.BeginTransaction()).Returns(transactionMock.Object);

            var walletService = new WalletService(_walletServiceLogger.Object, _mapper, walletRepo.Object, _walletTypeRepo.Object,
              transactionRepo.Object, _transactionDetailRepo.Object, _consumerWalletRepo.Object, session.Object, redemptionRepo.Object, _configuration,
              _auditTrailService.Object, _transactionService, _secretHelper.Object, _walletTypeTransferRuleRepo.Object, _consumerWalletServiceMock.Object, _mockConsumerService.Object);

            redemptionRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<RedemptionModel, bool>>>(), false))
                .ReturnsAsync(new RedemptionMockModel());

            transactionRepo.Setup(repo => repo.FindOneAsync(It.IsAny<Expression<Func<TransactionModel, bool>>>(), false))
                .ReturnsAsync(new TransactionMockModel());

            walletRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<WalletModel, bool>>>(), false)).ReturnsAsync(new WalletMockModel());

            walletRepo.Setup(x => x.UpdateRedemptionWalletBalance(It.IsAny<DateTime>(), It.IsAny<double>(), It.IsAny<long>(), It.IsAny<int>()))
                .Throws(new Exception("Simulated exception"));

            var postRedeemFailRequestDto = new PostRedeemFailRequestMockDto();

            var result = await walletService.RedeemFail(postRedeemFailRequestDto);
            Assert.True(result.Redemption == null);
        }

        [Fact]
        public async Task Should_Get_Wallet()
        {
            var findConsumerWalletRequestMockDto = new FindConsumerWalletRequestMockDto();
            var walletMockDto = await _walletController.GetWallets(findConsumerWalletRequestMockDto);
            var result = walletMockDto.Result as OkObjectResult;
            Assert.True(result?.Value != null);
            Assert.True(result.StatusCode == 200);
        }

        [Fact]
        public async Task Should_Get_Wallet_When_Sweepstakes_Wallet_Not_Exist()
        {
            var findConsumerWalletRequestMockDto = new FindConsumerWalletRequestMockDto();
            _walletTypeRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<WalletTypeModel, bool>>>(), false));
            var walletMockDto = await _walletController.GetWallets(findConsumerWalletRequestMockDto);
            var result = walletMockDto.Result as OkObjectResult;
            Assert.True(result?.Value != null);
            Assert.True(result.StatusCode == 200);
        }

        [Fact]
        public async Task Should_Get_Wallet_For_Null()
        {
            var findConsumerWalletRequestMockDto = new FindConsumerWalletRequestMockDto() { ConsumerCode = null };
            _consumerWalletRepo.Setup(x => x.GetConsumerWalletsExcludingWalletType(It.IsAny<string>(), It.IsAny<long>()));
            var result = await _walletController.GetWallets(findConsumerWalletRequestMockDto);
            Assert.True(result?.Value == null);
        }

        [Fact]
        public async Task Should_Return_GetWallet_Catch_Exception_For_Controller()
        {
            var findConsumerWalletRequestDto = new FindConsumerWalletRequestDto();
            _consumerWalletRepo.Setup(x => x.GetConsumerWalletsExcludingWalletType(It.IsAny<string>(), It.IsAny<long>()))
                .ThrowsAsync(new Exception("Inner Exception"));
            _walletTypeRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<WalletTypeModel, bool>>>(), false)).ReturnsAsync(new WalletTypeMockModel());
            await Assert.ThrowsAsync<Exception>(async () => await _walletController.GetWallets(findConsumerWalletRequestDto));
        }

        [Fact]
        public async Task Should_Get_Wallet_For_Service()
        {
            var findConsumerWalletRequestMockDto = new FindConsumerWalletRequestMockDto();
            var response = await _walletService.GetWallets(findConsumerWalletRequestMockDto);
            Assert.NotNull(response);
        }

        [Fact]
        public async Task Should_Get_Wallet_For_NotFound()
        {
            var findConsumerWalletRequestMockDto = new FindConsumerWalletRequestMockDto() { ConsumerCode = "dfdsfersefdfsfsdf" };
            var walletService = new Mock<IWalletService>();

            var notFoundResponse = new WalletResponseDto { ErrorCode = 404 };
            walletService.Setup(service => service.GetWallets(It.IsAny<FindConsumerWalletRequestDto>()))
                         .ReturnsAsync(notFoundResponse);

            var walletController = new WalletController(_walletLogger.Object, walletService.Object);

            var result = await walletController.GetWallets(findConsumerWalletRequestMockDto);

            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
            Assert.True(notFoundResult.StatusCode == 404);

            var responseDto = Assert.IsType<WalletResponseDto>(notFoundResult.Value);
            Assert.Equal(404, responseDto.ErrorCode);
        }

        [Fact]
        public async Task Catch_Exception_GetWallets_For_Service()
        {
            var findConsumerWalletRequestDto = new FindConsumerWalletRequestDto();
            _consumerWalletRepo.Setup(x => x.GetConsumerWalletsExcludingWalletType(It.IsAny<string>(), It.IsAny<long>()))
                .ThrowsAsync(new Exception("Inner Exception"));
            _walletTypeRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<WalletTypeModel, bool>>>(), false)).ReturnsAsync(new WalletTypeMockModel());
            await Assert.ThrowsAsync<Exception>(async () => await _walletService.GetWallets(findConsumerWalletRequestDto));
        }

        [Fact]
        public async Task GetConsumerWalletsExcludingWalletType_ExceptionHandling()
        {
            // Arrange
            var mockSession = new Mock<NHibernate.ISession>();
            var loggerMock = new Mock<ILogger<BaseRepo<ConsumerWalletModel>>>();
            var repository = new ConsumerWalletRepo(loggerMock.Object, mockSession.Object);
            mockSession.Setup(m => m.Query<ConsumerWalletModel>()).Throws(new Exception("Test Exception"));

            //Act
            var exceptionThrown = false;
            try
            {
                await repository.GetConsumerWalletsExcludingWalletType("consumerCode", 123);
            }
            catch (Exception ex)
            {
                exceptionThrown = true;
                Assert.Equal("Test Exception", ex.Message);
            }

            // Assert
            Assert.True(exceptionThrown);
        }
        [Fact]
        public async Task ClearEntriesWallet_Should_Returns_Not_Found_Result()
        {
            // Arrange
            var requestDto = new ClearEntriesWalletRequestMockDto();
            var expectedResponseDto = new BaseResponseDto
            {
                ErrorCode = StatusCodes.Status404NotFound,
                ErrorMessage = "Wallet type not found"
            };
            _walletTypeRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<WalletTypeModel, bool>>>(), false));

            // Act
            var result = await _walletController.ClearEntriesWallet(requestDto);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result.Result);
            var notFoundResult = result.Result as NotFoundObjectResult;
            var responseDto = notFoundResult?.Value as BaseResponseDto;
            Assert.Equal(expectedResponseDto.ErrorCode, responseDto?.ErrorCode);
            Assert.Equal(expectedResponseDto.ErrorMessage, responseDto?.ErrorMessage);
        }

        [Fact]
        public async Task GetConsumerWalletsByWalletType_Should_Return_Success_Result()
        {
            // Arrange
            var requestDto = new ClearEntriesWalletRequestMockDto();
            _walletTypeRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<WalletTypeModel, bool>>>(), false))
               .ReturnsAsync(new WalletTypeMockModel());
            _walletRepository.Setup(x => x.ClearEntriesWalletBalance(It.IsAny<string>(), It.IsAny<long>()));

            // Act
            var result = await _walletController.ClearEntriesWallet(requestDto);

            // Assert
            Assert.IsType<ActionResult<BaseResponseDto>>(result);
            var actionResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.True(actionResult.StatusCode == 200);
        }

        [Fact]
        public async Task ClearEntriesWallet_Should_Returns_Internal_Server_Error()
        {
            // Arrange
            var requestDto = new ClearEntriesWalletRequestMockDto();
            var exceptionMessage = "Test exception message";
            _walletTypeRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<WalletTypeModel, bool>>>(), false))
                .ThrowsAsync(new Exception(exceptionMessage));

            // Act
            var result = await _walletController.ClearEntriesWallet(requestDto);

            // Assert
            Assert.IsType<ObjectResult>(result.Result);
            var objectResult = result.Result as ObjectResult;
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult?.StatusCode);
            Assert.IsType<BaseResponseDto>(objectResult?.Value);
            var responseDto = objectResult.Value as BaseResponseDto;
            Assert.Equal(exceptionMessage, responseDto?.ErrorMessage);
        }
        [Fact]
        public async Task Clear_Entries_Wallet_Balance_Repository_Should_Throw_An_Exception()
        {
            // Arrange
            var mockSession = new Mock<NHibernate.ISession>();
            mockSession.Setup(s => s.BeginTransaction()).Returns(Mock.Of<ITransaction>());
            var walletMockData = new WalletMockModel();
            var expectedWallets = new List<WalletModel>
            {
                walletMockData
            };

            var expectedConsumerWallets = new List<ConsumerWalletModel>()
            {
                new() {
                    TenantCode = walletMockData.TenantCode,
                    WalletId = walletMockData.WalletId,
                    ConsumerCode = "cmr-c8580fffa8044d80a9097b17ba1ac5a1",
                    DeleteNbr = 0
                }
            };

            mockSession.Setup(x => x.Query<WalletModel>())
                .Returns(expectedWallets.AsQueryable);
            mockSession.Setup(x => x.Query<ConsumerWalletModel>())
                .Returns(expectedConsumerWallets.AsQueryable());

            var loggerMock = new Mock<ILogger<BaseRepo<WalletModel>>>();
            var walletRepo = new WalletRepo(loggerMock.Object, mockSession.Object);

            // Act
            await Assert.ThrowsAsync<NotSupportedException>(() => walletRepo.ClearEntriesWalletBalance(walletMockData.TenantCode, walletMockData.WalletTypeId));

            // Assert
            mockSession.Verify(s => s.BeginTransaction(), Times.Once);
        }
        [Fact]
        public async Task Should_Get_Wallet_type()
        {
            var walletTypeId = 1;
            var result = await _walletController.GetWalletType(walletTypeId);
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsType<WalletTypeDto>(okResult.Value);
            Assert.NotNull(response);
        }

        [Fact]
        public async Task Should_Get_Wallet_Type_Exception()
        {
            long walletTypeId = 123;
            var walletService = new Mock<IWalletService>();
            var controller = new WalletController(_walletLogger.Object, walletService.Object);
            walletService.Setup(x => x.GetWalletType(walletTypeId)).ThrowsAsync(new Exception("Transaction Exception"));
            var res = async () => await controller.GetWalletType(walletTypeId);
            var ex = Assert.ThrowsAsync<Exception>(res);
        }

        [Fact]
        public async Task Should_Get_Wallet_Type_Service()
        {
            var walletTypeId = 2;
            var response = await _walletService.GetWalletType(walletTypeId);
            Assert.True(response != null);
        }

        [Fact]
        public async Task Should_Get_Wallet_Type_NullCheck_Service()
        {
            int walletTypeId = 1;
            var walletMockDto = new WalletTypeMockDto();
            _walletTypeRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<WalletTypeModel, bool>>>(), false))
                .ReturnsAsync((WalletTypeMockModel)null);
            var result = await _walletService.GetWalletType(walletTypeId);
            Assert.True(result?.WalletTypeLabel == null);
            Assert.True(result?.WalletTypeName == null);
        }

        [Fact]
        public void Should_Get_Wallet_Type_Exception_Service()
        {
            long walletTypeId = 123;
            _walletTypeRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<WalletTypeModel, bool>>>(), false))
                .ThrowsAsync(new Exception("Simulated exception"));
            var response = async () => await _walletService.GetWalletType(walletTypeId);
            var result = Assert.ThrowsAsync<Exception>(response);
        }
        [Fact]
        public async Task UpdateWalletBalance_Returns_OkResult()
        {
            // Arrange
            var walletModels = new List<WalletMockModel>(); // Prepare test data if needed
            _walletRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<WalletModel, bool>>>(), false))
                .ReturnsAsync(new WalletMockModel());
            _walletRepo.Setup(x => x.UpdateWalletBalance(It.IsAny<WalletModel>()))
                .Returns(1);
            var walletMockDto = await _walletController.UpdateWalletBalance(It.IsAny<IList<WalletModel>>());
            var response = await _walletService.UpdateWalletBalance(It.IsAny<IList<WalletModel>>());
            Assert.NotNull(response);
            var result = walletMockDto.Result as OkObjectResult;
            Assert.NotNull(result?.Value);
            Assert.Equal(200, result.StatusCode);

        }

        [Fact]
        public async Task UpdateWalletBalance_Returns_NotFoundResult()
        {
            // Arrange
            _walletRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<WalletModel, bool>>>(), false));

            var result = await _walletService.UpdateWalletBalance(It.IsAny<IList<WalletModel>>());

            var response = Assert.IsType<BaseResponseDto>(result);
            Assert.Equal(500, response.ErrorCode);


        }
        [Fact]
        public async Task UpdateWalletBalance_Returns_NotFoundResult_controller()
        {

            _walletServiceMock.Setup(x => x.UpdateWalletBalance(It.IsAny<List<WalletModel>>()))
          .ReturnsAsync(new BaseResponseDto()
          {

              ErrorCode = 404
          });
            var _walletController = new WalletController(_walletLogger.Object, _walletServiceMock.Object);
            var response = await _walletController.UpdateWalletBalance(It.IsAny<List<WalletModel>>());
            Assert.IsType<NotFoundObjectResult>(response.Result);

            var notFoundResult = response.Result as NotFoundObjectResult;

            var responseDto = notFoundResult?.Value as BaseResponseDto;
            Assert.Equal(404, responseDto?.ErrorCode);

        }
        [Fact]
        public void UpdateWalletBalance_Returns_InternalServerError_controller()
        {

            _walletServiceMock.Setup(x => x.UpdateWalletBalance(It.IsAny<List<WalletModel>>()))
        .ThrowsAsync(new Exception("Simulated exception"));
            var _walletController = new WalletController(_walletLogger.Object, _walletServiceMock.Object);
            var response = async () => await _walletController.UpdateWalletBalance(It.IsAny<List<WalletModel>>());
            var result = Assert.ThrowsAsync<Exception>(response);

        }

        [Fact]
        public void UpdateWalletBalance_Returns_InternalServerError()
        {
            // Arrange
            _walletRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<WalletModel, bool>>>(), false))
              .ThrowsAsync(new Exception("Simulated exception"));
            _walletRepo.Setup(x => x.UpdateWalletBalance(It.IsAny<WalletModel>()))
                .Throws(new Exception("Simulated exception")); ;
            var response = async () => await _walletService.UpdateWalletBalance(It.IsAny<IList<WalletModel>>());
            var result = Assert.ThrowsAsync<Exception>(response);

        }
        [Fact]
        public async Task UpdateWalletBalance_Successful()
        {
            // Arrange
            var walletModelList = new List<WalletModel>
        {
            new WalletModel { WalletId = 1, Balance = 500 },
            new WalletModel { WalletId = 2, Balance = 700 }
        };
            var WalletModel = new WalletModel { WalletId = 1, Balance = 500 };

            // Mock FindOneAsync and UpdateWalletBalance methods
            _walletRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<WalletModel, bool>>>(), false))
              .ReturnsAsync(WalletModel);

            _walletRepo.Setup(repo => repo.UpdateWalletBalance(WalletModel))
                .Returns(1); // Return 1 to indicate one record updated

            // Act
            var response = await _walletService.UpdateWalletBalance(walletModelList);

            // Assert
            Assert.NotNull(response);

        }

        [Fact]
        public async Task CreateTenantMasterWallets_ShouldReturnOk_WhenWalletsCreatedSuccessfully()
        {
            // Arrange
            var request = new CreateTenantMasterWalletsRequestDto
            {
                TenantCode = "Tenant123",
                Apps = [WalletConstants.Apps.Rewards],
                CustomerCode = "Test customer code",
                SponsorCode = "Test sponsor code",
            };
            var transactionMock = new Mock<ITransaction>();
            _session.Setup(s => s.BeginTransaction()).Returns(transactionMock.Object);
            _walletTypeRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<WalletTypeModel, bool>>>(), false)).ReturnsAsync(new WalletTypeMockModel());

            // Act
            var result = await _walletController.CreateTenantMasterWallets(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
        }

        [Fact]
        public async Task CreateTenantMasterWallets_ShouldReturnBadRequest_WhenAppsArrayIsEmpty()
        {
            // Arrange
            var request = new CreateTenantMasterWalletsRequestDto
            {
                TenantCode = "Tenant123",
                Apps = []
            };

            // Act
            var result = await _walletController.CreateTenantMasterWallets(request);

            // Assert
            var badRequestResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status400BadRequest, badRequestResult.StatusCode);
        }

        [Fact]
        public async Task CreateTenantMasterWallets_ShouldReturnInternalServerError_WhenExceptionOccurs()
        {
            // Arrange
            var request = new CreateTenantMasterWalletsRequestDto
            {
                TenantCode = "Tenant123",
                Apps = [WalletConstants.Apps.Rewards]
            };
            var transactionMock = new Mock<ITransaction>();
            _session.Setup(s => s.BeginTransaction()).Returns(transactionMock.Object);
            _session.Setup(s => s.SaveAsync(It.IsAny<object>(), It.IsAny<CancellationToken>())).ThrowsAsync(new Exception("Testing"));


            // Act
            var result = await _walletController.CreateTenantMasterWallets(request);

            // Assert
            var internalServerErrorResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, internalServerErrorResult.StatusCode);
        }
        [Fact]
        public async Task Create_Benifits_TenantMasterWallets_ShouldReturnOk_WhenWalletsCreatedSuccessfully()
        {
            // Arrange
            var purses = new List<Purse>
            { new Purse { PurseWalletType = "wat-1234",PurseNumber=1, WalletType = "wat-123" ,PeriodConfig=new PeriodConfig(){ ApplyDateConfig = new ApplyDateConfig()}},
              new Purse { PurseWalletType = "wat-5678",PurseNumber=1, WalletType = "wat-567" ,PeriodConfig=new PeriodConfig(){ ApplyDateConfig = new ApplyDateConfig()}} ,
              new Purse { PurseWalletType = "wat-9101",PurseNumber=1, WalletType = "wat-789" ,PeriodConfig=new PeriodConfig(){ ApplyDateConfig = new ApplyDateConfig()}}
            };
            var request = new CreateTenantMasterWalletsRequestDto
            {
                TenantCode = "Tenant123",
                Apps = [WalletConstants.Apps.Benefits],
                CustomerCode = "Test customer code",
                SponsorCode = "Test sponsor code",
                PurseConfig = new PurseConfig() { Purses = purses }
            };

            var transactionMock = new Mock<ITransaction>();
            _session.Setup(s => s.BeginTransaction()).Returns(transactionMock.Object);
            _walletTypeRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<WalletTypeModel, bool>>>(), false)).ReturnsAsync(new WalletTypeMockModel());

            // Act
            var result = await _walletController.CreateTenantMasterWallets(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
        }
        [Fact]
        public async Task Create_Benifits_TenantMasterWallets_ShouldReturnBadRequest_WhenPurses_Null()
        {
            // Arrange
            var request = new CreateTenantMasterWalletsRequestDto
            {
                TenantCode = "Tenant123",
                Apps = [WalletConstants.Apps.Benefits],
                CustomerCode = "Test customer code",
                SponsorCode = "Test sponsor code",
                PurseConfig = new PurseConfig()
                {
                    Purses = null
                }
            };

            var transactionMock = new Mock<ITransaction>();
            _session.Setup(s => s.BeginTransaction()).Returns(transactionMock.Object);

            // Act
            var result = await _walletController.CreateTenantMasterWallets(request);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status400BadRequest, objectResult.StatusCode);
        }
        [Fact]
        public async Task Create_Benifits_TenantMasterWallets_ShouldReturnBadRequest_WhenPurseConfig_Isnull()
        {
            // Arrange

            var request = new CreateTenantMasterWalletsRequestDto
            {
                TenantCode = "Tenant123",
                Apps = [WalletConstants.Apps.Benefits],
                CustomerCode = "Test customer code",
                SponsorCode = "Test sponsor code",
                PurseConfig = null
            };

            var transactionMock = new Mock<ITransaction>();
            _session.Setup(s => s.BeginTransaction()).Returns(transactionMock.Object);

            // Act
            var result = await _walletController.CreateTenantMasterWallets(request);

            // Assert
            var objectresult = result as ObjectResult;
            Assert.Equal(StatusCodes.Status400BadRequest, objectresult?.StatusCode);
        }
        [Fact]
        public async Task Create_Benifits_TenantMasterWallets_ShouldReturnInternalServerError_WhenExceptionOccurs()
        {
            // Arrange
            var purses = new List<Purse>
            { new Purse { PurseWalletType = "wat-1234",PurseNumber=1, WalletType = "wat-123" ,PeriodConfig=new PeriodConfig(){ ApplyDateConfig = new ApplyDateConfig()}},
              new Purse { PurseWalletType = "wat-5678",PurseNumber=1, WalletType = "wat-567" ,PeriodConfig=new PeriodConfig(){ ApplyDateConfig = new ApplyDateConfig()}} ,
              new Purse { PurseWalletType = "wat-9101",PurseNumber=1, WalletType = "wat-789" ,PeriodConfig=new PeriodConfig(){ ApplyDateConfig = new ApplyDateConfig()}}
            };
            var request = new CreateTenantMasterWalletsRequestDto
            {
                TenantCode = "Tenant123",
                Apps = [WalletConstants.Apps.Benefits],
                CustomerCode = "Test customer code",
                SponsorCode = "Test sponsor code",
                PurseConfig = new PurseConfig() { Purses = purses }
            };
            var transactionMock = new Mock<ITransaction>();
            _session.Setup(s => s.BeginTransaction()).Returns(transactionMock.Object);
            _session.Setup(s => s.SaveAsync(It.IsAny<object>(), It.IsAny<CancellationToken>())).ThrowsAsync(new Exception("Testing"));
            _walletTypeRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<WalletTypeModel, bool>>>(), false)).ReturnsAsync(new WalletTypeMockModel());
            _walletRepository.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<WalletModel, bool>>>(), false)).ThrowsAsync(new Exception("Testing"));

            // Act
            var result = await _walletController.CreateTenantMasterWallets(request);

            // Assert
            var internalServerErrorResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, internalServerErrorResult.StatusCode);
        }
        [Fact]
        public async Task Create_Benifits_TenantMasterWallets_ShouldReturn_NotFound_When_Master_Wallet_type_IsNull()
        {
            // Arrange
            var purses = new List<Purse>
            { new Purse { PurseWalletType = "wat-1234",PurseNumber=1, WalletType = "wat-123" ,PeriodConfig=new PeriodConfig(){ ApplyDateConfig = new ApplyDateConfig()}},
              new Purse { PurseWalletType = "wat-5678",PurseNumber=1, WalletType = "wat-567" ,PeriodConfig=new PeriodConfig(){ ApplyDateConfig = new ApplyDateConfig()}} ,
              new Purse { PurseWalletType = "wat-9101",PurseNumber=1, WalletType = "wat-789" ,PeriodConfig=new PeriodConfig(){ ApplyDateConfig = new ApplyDateConfig()}}
            };
            var request = new CreateTenantMasterWalletsRequestDto
            {
                TenantCode = "Tenant123",
                Apps = [WalletConstants.Apps.Benefits],
                CustomerCode = "Test customer code",
                SponsorCode = "Test sponsor code",
                PurseConfig = new PurseConfig() { Purses = purses }
            };
            var transactionMock = new Mock<ITransaction>();
            _session.Setup(s => s.BeginTransaction()).Returns(transactionMock.Object);
            _walletTypeRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<WalletTypeModel, bool>>>(), false));

            // Act
            var result = await _walletController.CreateTenantMasterWallets(request);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status404NotFound, objectResult.StatusCode);
        }
        [Fact]
        public async Task GetAllWalletTypes_Should_Return_All_Wallet_Types()
        {
            // Arrange
            var walletTypesMock = new List<WalletTypeDto>
            {
                new WalletTypeDto { WalletTypeId = 1,  WalletTypeCode = "sample wallet type" },
                new WalletTypeDto { WalletTypeId = 2,  WalletTypeCode = "sample wallet type" },
            };

            _walletTypeRepo
                .Setup(x => x.FindAsync(It.IsAny<Expression<Func<WalletTypeModel, bool>>>(), false)).ReturnsAsync(new List<WalletTypeModel>
                {
                    new WalletTypeModel { WalletTypeId = 1, },
                    new WalletTypeModel { WalletTypeId = 2, }
                });
            Mock<IMapper> _mapper = new Mock<IMapper>();
            _mapper.Setup(x => x.Map<IList<WalletTypeDto>>(It.IsAny<IList<WalletTypeModel>>())).Returns(walletTypesMock);

            // Act
            var result = await _walletController.GetAllWalletTypes();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);

        }
        [Fact]
        public async Task GetAllWalletTypes_Should_ThrowException()
        {
            // Arrange
            _walletTypeRepo
                .Setup(x => x.FindAsync(It.IsAny<Expression<Func<WalletTypeModel, bool>>>(), false)).ThrowsAsync(new Exception("something wrong"));

            // Act
            var result = await _walletController.GetAllWalletTypes();

            // Assert
            var okResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(StatusCodes.Status500InternalServerError, okResult.StatusCode);

        }

        [Fact]
        public async Task CreateWalletType_Should_Return_Ok_Response()
        {
            // Arrange
            _session.Setup(s => s.BeginTransaction()).Returns(Mock.Of<ITransaction>());
            var createRequestDto = new WalletTypeDto();
            createRequestDto.WalletTypeCode = "wat-test";
            var walletTypeModel = new WalletTypeModel();
            Mock<IMapper> _mapper = new Mock<IMapper>();
            _mapper.Setup(m => m.Map<WalletTypeModel>(createRequestDto))
                     .Returns(walletTypeModel);
            _walletTypeRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<WalletTypeModel, bool>>>(), false));


            // Act
            var result = await _walletController.CreateWalletType(createRequestDto);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<OkObjectResult>(result.Result);
            var okObjectResult = result.Result as OkObjectResult;
            Assert.Equal(StatusCodes.Status200OK, okObjectResult?.StatusCode);

        }
        [Fact]
        public async Task CreateWalletType_Should_Return_Conflict_Response_When_WalletType_Exist()
        {
            // Arrange
            var createRequestDto = new WalletTypeDto();
            createRequestDto.WalletTypeCode = "wat-test";
            var walletTypeModel = new WalletTypeModel();
            walletTypeModel.WalletTypeCode = "wat-test";
            Mock<IMapper> _mapper = new Mock<IMapper>();
            _mapper.Setup(m => m.Map<WalletTypeModel>(createRequestDto))
                     .Returns(walletTypeModel);
            _walletTypeRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<WalletTypeModel, bool>>>(), false)).ReturnsAsync(walletTypeModel);

            // Act
            var result = await _walletController.CreateWalletType(createRequestDto);

            // Assert
            Assert.NotNull(result);
            var objectResult = result.Result as ObjectResult;
            Assert.Equal(StatusCodes.Status409Conflict, objectResult?.StatusCode);

        }
        [Fact]
        public async Task CreateWalletType_Should_Return_Internal_Server_Error_When_Exception_Occurs()
        {
            // Arrange
            var createRequestDto = new WalletTypeDto();
            createRequestDto.WalletTypeCode = "wat-test";
            var walletTypeModel = new WalletTypeModel();
            Mock<IMapper> _mapper = new Mock<IMapper>();
            _mapper.Setup(m => m.Map<WalletTypeModel>(createRequestDto))
                     .Returns(walletTypeModel);
            _walletTypeRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<WalletTypeModel, bool>>>(), false)).ThrowsAsync(new Exception("Some thing wrong"));

            // Act
            var result = await _walletController.CreateWalletType(createRequestDto);

            // Assert
            Assert.NotNull(result);
            var objectResult = result.Result as ObjectResult;
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult?.StatusCode);

        }
        [Fact]
        public async Task CreateWalletType_Should_Return_BadRequest_When_WalletTypeCode_IsNull()
        {
            // Arrange
            var createRequestDto = new WalletTypeDto();
            createRequestDto.WalletTypeCode = null;

            // Act
            var result = await _walletController.CreateWalletType(createRequestDto);

            // Assert
            Assert.NotNull(result);
            var objectResult = result.Result as ObjectResult;
            Assert.Equal(StatusCodes.Status400BadRequest, objectResult?.StatusCode);

        }
        [Fact]
        public async Task GetMasterWallets_ShouldReturnOk_When_MasterWallets_ReturnsSuccess()
        {
            // Arrange
            var requestDto = "Tenant123";
            var WalletModelMock = new List<WalletModel>() { new WalletModel() };
            _walletRepository.Setup(repo => repo.FindAsync(It.IsAny<Expression<Func<WalletModel, bool>>>(), false))
                           .ReturnsAsync(WalletModelMock);
            // Act
            var result = await _walletController.GetMasterWallets(requestDto);

            // Assert
            var actionResult = Assert.IsType<ActionResult<GetAllMasterWalletsResponseDto>>(result);
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            Assert.Equal(200, okResult.StatusCode);
        }
        [Fact]
        public async Task GetMasterWallets_ShouldReturn_When_MasterWallets_Return_NotFound()
        {
            // Arrange
            var requestDto = "Tenant123";
            var walletModelMock = new List<WalletModel>();

            _walletRepository
                .Setup(repo => repo.FindAsync(It.IsAny<Expression<Func<WalletModel, bool>>>(), false))
                .ReturnsAsync(walletModelMock);
            // Act
            var result = await _walletController.GetMasterWallets(requestDto);

            // Assert
            var actionResult = Assert.IsType<ActionResult<GetAllMasterWalletsResponseDto>>(result);
            var okResult = Assert.IsType<ObjectResult>(actionResult.Result);
            Assert.Equal(404, okResult.StatusCode);
        }
        [Fact]
        public async Task GetMasterWallets_ShouldReturnOk_When_MasterWallets_Return_NotFound()
        {
            // Arrange
            var requestDto = "Tenant123";
            _walletServiceMock.Setup(x => x.GetMasterWallets(It.IsAny<string>()))
              .ReturnsAsync(new GetAllMasterWalletsResponseDto()
              {
                  ErrorCode = 404
              });
            var _walletController = new WalletController(_walletLogger.Object, _walletServiceMock.Object);
            // Act
            var result = await _walletController.GetMasterWallets(requestDto);

            // Assert
            var actionResult = Assert.IsType<ActionResult<GetAllMasterWalletsResponseDto>>(result);
            var okResult = Assert.IsType<ObjectResult>(actionResult.Result);
            Assert.Equal(404, okResult.StatusCode);
        }
        [Fact]
        public async Task GetMasterWallets_ShouldReturnOk_When_MasterWallets_ThrowsException()
        {
            // Arrange
            var requestDto = "Tenant123";
            _walletRepo
                .Setup(repo => repo.FindAsync(It.IsAny<Expression<Func<WalletModel, bool>>>(), false))
                .ThrowsAsync(new Exception());
            // Act
            var result = await _walletController.GetMasterWallets(requestDto);

            // Assert
            var actionResult = Assert.IsType<ActionResult<GetAllMasterWalletsResponseDto>>(result);
            var okResult = Assert.IsType<ObjectResult>(actionResult.Result);
            Assert.Equal(500, okResult.StatusCode);
        }


        [Fact]
        public async Task CreateWallet_Should_Return_Ok_Response()
        {
            // Arrange
            _session.Setup(s => s.BeginTransaction()).Returns(Mock.Of<ITransaction>());
            var createRequestDto = new WalletRequestDto();
            createRequestDto.WalletCode = "wat-test";
            createRequestDto.WalletTypeId = 1;
            var walletTypeModel = new WalletTypeModel();
            walletTypeModel.WalletTypeId = 1;
            Mock<IMapper> _mapper = new Mock<IMapper>();
            _mapper.Setup(m => m.Map<WalletTypeModel>(createRequestDto))
                     .Returns(walletTypeModel);
            _walletTypeRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<WalletTypeModel, bool>>>(), false)).ReturnsAsync(walletTypeModel);
            _walletRepository.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<WalletModel, bool>>>(), false));

            // Act
            var result = await _walletController.CreateWallet(createRequestDto);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<OkObjectResult>(result.Result);
            var okObjectResult = result.Result as OkObjectResult;
            Assert.Equal(StatusCodes.Status200OK, okObjectResult?.StatusCode);

        }
        [Fact]
        public async Task CreateWallet_Should_Return_Conflict_Response_When_Wallet_Exist()
        {
            // Arrange
            var createRequestDto = new WalletRequestDto();
            createRequestDto.WalletCode = "wat-test";
            createRequestDto.WalletTypeId = 1;
            var walletTypeModel = new WalletTypeModel();
            walletTypeModel.WalletTypeCode = "watType-test";
            var walletModel = new WalletModel();
            walletModel.WalletCode = "wat-test";
            Mock<IMapper> _mapper = new Mock<IMapper>();
            _mapper.Setup(m => m.Map<WalletTypeModel>(createRequestDto))
                     .Returns(walletTypeModel);
            _walletTypeRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<WalletTypeModel, bool>>>(), false)).ReturnsAsync(walletTypeModel);
            _walletRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<WalletModel, bool>>>(), false)).ReturnsAsync(walletModel);

            // Act
            var result = await _walletController.CreateWallet(createRequestDto);

            // Assert
            Assert.NotNull(result);
            var objectResult = result.Result as ObjectResult;
            Assert.Equal(StatusCodes.Status409Conflict, objectResult?.StatusCode);

        }
        [Fact]
        public async Task CreateWallet_Should_Return_Internal_Server_Error_When_Exception_Occurs()
        {
            // Arrange
            var createRequestDto = new WalletRequestDto();
            createRequestDto.WalletCode = "wat-test";
            var walletTypeModel = new WalletTypeModel();

            _walletTypeRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<WalletTypeModel, bool>>>(), false)).ThrowsAsync(new Exception("Some thing wrong"));

            // Act
            var result = await _walletController.CreateWallet(createRequestDto);

            // Assert
            Assert.NotNull(result);
            var objectResult = result.Result as ObjectResult;
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult?.StatusCode);

        }

        [Fact]
        public async Task RewardDetails_ReturnsError_WhenWalletTypeNotFound()
        {
            // Arrange
            var request = new PostRewardRequestDto
            {
                MasterWalletTypeCode = "MasterType",
                ConsumerWalletTypeCode = "ConsumerType",
                ConsumerCode = "12345",
                RewardAmount = 100
            };

            _walletRepo.Setup(repo => repo.FindOneAsync(It.IsAny<Expression<Func<WalletModel, bool>>>(), false));

            // Act
            var result = await _walletService.RewardDetails(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(StatusCodes.Status400BadRequest, result.ErrorCode);
            Assert.Equal("Invalid wallet type", result.ErrorMessage);
        }

        [Fact]
        public async Task RewardDetails_ReturnsError_WhenWalletsNotFound()
        {
            // Arrange
            var request = new PostRewardRequestDto
            {
                MasterWalletTypeCode = "MasterType",
                ConsumerWalletTypeCode = "ConsumerType",
                ConsumerCode = "12345",
                RewardAmount = 100
            };

            var walletType = new WalletTypeModel { WalletTypeId = 1, ConfigJson = "{\r\n  \"currency\": \"ENTRIES\"\r\n}" };

            _walletTypeRepo.Setup(repo => repo.FindOneAsync(It.IsAny<Expression<Func<WalletTypeModel, bool>>>(), false))
                .ReturnsAsync(walletType);




            // Act
            var result = await _walletService.RewardDetails(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(StatusCodes.Status400BadRequest, result.ErrorCode);
            Assert.Equal("Wallet/ConsumerWallet not found", result.ErrorMessage);
        }


        [Fact]
        public async Task RewardDetails_RewardLimitReached_true_NOSplit()
        {
            // Arrange
            var request = new PostRewardRequestDto
            {
                MasterWalletTypeCode = "MasterType",
                ConsumerWalletTypeCode = "ConsumerType",
                ConsumerCode = "cmr-12345",
                RewardAmount = 100
            };

            var walletType = new WalletTypeModel { WalletTypeId = 2 , ConfigJson = "{\r\n  \"currency\": \"ENTRIES\"\r\n}" };

            _walletTypeRepo.Setup(repo => repo.FindOneAsync(It.IsAny<Expression<Func<WalletTypeModel, bool>>>(), false))
                .ReturnsAsync(walletType);


            // Act
            var result = await _walletService.RewardDetails(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(StatusCodes.Status200OK, result.ErrorCode);
            Assert.Equal("Consumer has reached earn maximum allowed", result.ErrorMessage);
        }


        [Fact]
        public async Task RewardDetails_RewardLimitReached_true_Split_but_NoTransferRule()
        {
            // Arrange
            var request = new PostRewardRequestDto
            {
                MasterWalletTypeCode = "MasterType",
                ConsumerWalletTypeCode = "ConsumerType",
                ConsumerCode = "cmr-12345",
                RewardAmount = 100,
                SplitRewardOverflow = true
            };

            var walletType = new WalletTypeModel { WalletTypeId = 2, ConfigJson = "{\r\n  \"currency\": \"ENTRIES\"\r\n}" };

            _walletTypeRepo.Setup(repo => repo.FindOneAsync(It.IsAny<Expression<Func<WalletTypeModel, bool>>>(), false))
                .ReturnsAsync(walletType);

            _walletTypeTransferRuleRepo.Setup(repo => repo.FindOneAsync(It.IsAny<Expression<Func<WalletTypeTransferRuleModel, bool>>>(), false));

            // Act
            var result = await _walletService.RewardDetails(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(StatusCodes.Status400BadRequest, result.ErrorCode);
            Assert.Equal("Transfer rule not found", result.ErrorMessage);
        }

        [Fact]
        public async Task RewardDetails_RewardLimitReached_true_Split_but_INvalidTransferRule_1()
        {
            // Arrange
            var request = new PostRewardRequestDto
            {
                MasterWalletTypeCode = "MasterType",
                ConsumerWalletTypeCode = "ConsumerType",
                ConsumerCode = "cmr-12345",
                RewardAmount = 100,
                SplitRewardOverflow = true
            };

            var walletType = new WalletTypeModel { WalletTypeId = 2 , ConfigJson = "{\r\n  \"currency\": \"USD\"\r\n}" };

            _walletTypeRepo.Setup(repo => repo.FindOneAsync(It.IsAny<Expression<Func<WalletTypeModel, bool>>>(), false))
                .ReturnsAsync(walletType);

            _walletTypeTransferRuleRepo.Setup(repo => repo.FindOneAsync(It.IsAny<Expression<Func<WalletTypeTransferRuleModel, bool>>>(), false))
            .ReturnsAsync(new WalletTypeTransferRuleModel() { WalletTypeTransferRuleId = 2, TransferRule = "{}", TargetWalletTypeId = 1 });

            // Act
            var result = await _walletService.RewardDetails(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(StatusCodes.Status400BadRequest, result.ErrorCode);
            Assert.Equal("Transfer Ratio Not Defined", result.ErrorMessage);
        }

        [Fact]
        public async Task RewardDetails_RewardLimitReached_true_Split_but_InvalidTransferRule()
        {
            // Arrange
            var request = new PostRewardRequestDto
            {
                MasterWalletTypeCode = "MasterType",
                ConsumerWalletTypeCode = "ConsumerType",
                ConsumerCode = "cmr-12345",
                RewardAmount = 100,
                SplitRewardOverflow = true
            };

            var walletType = new WalletTypeModel { WalletTypeId = 2 , ConfigJson = "{\r\n  \"currency\": \"USD\"\r\n}" };

            _walletTypeRepo.Setup(repo => repo.FindOneAsync(It.IsAny<Expression<Func<WalletTypeModel, bool>>>(), false))
                .ReturnsAsync(walletType);

            _walletTypeTransferRuleRepo.Setup(repo => repo.FindOneAsync(It.IsAny<Expression<Func<WalletTypeTransferRuleModel, bool>>>(), false))
            .ReturnsAsync(new WalletTypeTransferRuleModel() { WalletTypeTransferRuleId = 2, TransferRule = "{ \"unknown\": \"xxx\" }", TargetWalletTypeId = 1 });

            // Act
            var result = await _walletService.RewardDetails(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(StatusCodes.Status400BadRequest, result.ErrorCode);
            Assert.Equal("Transfer Ratio Not Defined", result.ErrorMessage);
        }

        [Fact]
        public async Task RewardDetails_RewardLimitReached_true_Split_but_InvalidTransferRule_2()
        {
            // Arrange
            var request = new PostRewardRequestDto
            {
                MasterWalletTypeCode = "MasterType",
                ConsumerWalletTypeCode = "ConsumerType",
                ConsumerCode = "cmr-12345",
                RewardAmount = 100,
                SplitRewardOverflow = true
            };

            var walletType = new WalletTypeModel { WalletTypeId = 2 , ConfigJson = "{\r\n  \"currency\": \"USD\"\r\n}" };

            _walletTypeRepo.Setup(repo => repo.FindOneAsync(It.IsAny<Expression<Func<WalletTypeModel, bool>>>(), false))
                .ReturnsAsync(walletType);

            _walletTypeTransferRuleRepo.Setup(repo => repo.FindOneAsync(It.IsAny<Expression<Func<WalletTypeTransferRuleModel, bool>>>(), false))
            .ReturnsAsync(new WalletTypeTransferRuleModel() { WalletTypeTransferRuleId = 2, TransferRule = "{ \"transferRatio\": \"xxx\" }", TargetWalletTypeId = 1 });

            // Act
            var result = await _walletService.RewardDetails(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(StatusCodes.Status400BadRequest, result.ErrorCode);
            Assert.Contains("Transfer Ratio Not Defined", result.ErrorMessage);
        }


        [Fact]
        public async Task RewardDetails_RewardLimitReached_true_Split_but_targetConsumerWalletTypeNotExist()
        {
            // Arrange
            var request = new PostRewardRequestDto
            {
                MasterWalletTypeCode = "MasterType",
                ConsumerWalletTypeCode = "ConsumerType",
                ConsumerCode = "cmr-12345",
                RewardAmount = 100,
                SplitRewardOverflow = true
            };

            var walletType = new WalletTypeModel { WalletTypeId = 2, ConfigJson = "{\r\n  \"currency\": \"USD\"\r\n}" };

            _walletTypeRepo
                        .Setup(repo => repo.FindOneAsync(It.IsAny<Expression<Func<WalletTypeModel, bool>>>(), false))
                        .ReturnsAsync((Expression<Func<WalletTypeModel, bool>> predicate, bool _) =>
                        {
                            var compiledPredicate = predicate.Compile();

                            if (compiledPredicate(new WalletTypeModel { WalletTypeId = 1, DeleteNbr = 0 }))
                                return null;

                            return walletType; // Default return if no match is found
                        });


            _walletTypeTransferRuleRepo.Setup(repo => repo.FindOneAsync(It.IsAny<Expression<Func<WalletTypeTransferRuleModel, bool>>>(), false))
            .ReturnsAsync(new WalletTypeTransferRuleModel() { WalletTypeTransferRuleId = 2, TransferRule = "{\"transferRatio\":0.25}", TargetWalletTypeId = 1 });

            // Act
            var result = await _walletService.RewardDetails(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(StatusCodes.Status400BadRequest, result.ErrorCode);
            Assert.Contains("Wallet type not found for", result.ErrorMessage);
        }

        [Fact]
        public async Task RewardDetails_RewardLimitReached_true_Split_but_targetConsumerWalletSuccess()
        {
            // Arrange
            var transactionMock = new Mock<ITransaction>();
            _session.Setup(s => s.BeginTransaction()).Returns(transactionMock.Object);
            var request = new PostRewardRequestDto
            {
                MasterWalletTypeCode = "MasterType",
                ConsumerWalletTypeCode = "ConsumerType",
                ConsumerCode = "cmr-12345",
                RewardAmount = 100,
                SplitRewardOverflow = true
            };

            var walletType = new WalletTypeModel { WalletTypeId = 2, ConfigJson = "{\r\n  \"currency\": \"USD\"\r\n}" };

            _walletTypeRepo
                        .Setup(repo => repo.FindOneAsync(It.IsAny<Expression<Func<WalletTypeModel, bool>>>(), false))
                        .ReturnsAsync((Expression<Func<WalletTypeModel, bool>> predicate, bool _) =>
                        {
                            var compiledPredicate = predicate.Compile();

                            if (compiledPredicate(new WalletTypeModel { WalletTypeId = 7, DeleteNbr = 0, ConfigJson = "{\r\n  \"currency\": \"ENTRIES\"\r\n}" }))
                                return new WalletTypeModel() { WalletTypeId = 7, WalletTypeCode = "wtc7", ConfigJson = "{\r\n  \"currency\": \"ENTRIES\"\r\n}" };

                            return walletType; // Default return if no match is found
                        });


            _walletTypeTransferRuleRepo.Setup(repo => repo.FindOneAsync(It.IsAny<Expression<Func<WalletTypeTransferRuleModel, bool>>>(), false))
            .ReturnsAsync(new WalletTypeTransferRuleModel() { WalletTypeTransferRuleId = 2, TransferRule = "{\"transferRatio\":0.25}", TargetWalletTypeId = 7 });

            // Act
            var result = await _walletService.RewardDetails(request);

            // Assert
            Assert.NotNull(result);
            Assert.Null( result.ErrorCode);
        }

        [Fact]
        public async Task RewardDetails_RewardLimitReached_true_Split_sweepsTakesWallet()
        {
            // Arrange
            var transactionMock = new Mock<ITransaction>();
            _session.Setup(s => s.BeginTransaction()).Returns(transactionMock.Object);
            var request = new PostRewardRequestDto
            {
                MasterWalletTypeCode = "MasterType",
                ConsumerWalletTypeCode = "ConsumerType",
                ConsumerCode = "cmr-12345",
                RewardAmount = 97,
                SplitRewardOverflow = true
            };

            var walletType = new WalletTypeModel { WalletTypeId = 2, ConfigJson = "{\r\n  \"currency\": \"USD\"\r\n}" };

            _walletTypeRepo
                        .Setup(repo => repo.FindOneAsync(It.IsAny<Expression<Func<WalletTypeModel, bool>>>(), false))
                        .ReturnsAsync((Expression<Func<WalletTypeModel, bool>> predicate, bool _) =>
                        {
                            var compiledPredicate = predicate.Compile();

                            if (compiledPredicate(new WalletTypeModel { WalletTypeId = 7, DeleteNbr = 0, ConfigJson = "{\r\n  \"currency\": \"ENTRIES\"\r\n}" }))
                                return new WalletTypeModel() { WalletTypeId = 7, WalletTypeCode = "wat-c3b091232e974f98aeceb495d2a9f916", ConfigJson = "{\r\n  \"currency\": \"ENTRIES\"\r\n}" };

                            return walletType; // Default return if no match is found
                        });


            _walletTypeTransferRuleRepo.Setup(repo => repo.FindOneAsync(It.IsAny<Expression<Func<WalletTypeTransferRuleModel, bool>>>(), false))
            .ReturnsAsync(new WalletTypeTransferRuleModel() { WalletTypeTransferRuleId = 2, TransferRule = "{\"transferRatio\":0.25}", TargetWalletTypeId = 7 });

            // Act
            var result = await _walletService.RewardDetails(request);

            // Assert
            Assert.NotNull(result);
            Assert.Null(result.ErrorCode);

            
        }

        [Fact]
        public async Task RewardDetails_RewardLimitNotReached_true_Split_sweepsTakesWallet()
        {
            // Arrange
            var transactionMock = new Mock<ITransaction>();
            _session.Setup(s => s.BeginTransaction()).Returns(transactionMock.Object);
            var request = new PostRewardRequestDto
            {
                MasterWalletTypeCode = "MasterType",
                ConsumerWalletTypeCode = "ConsumerType",
                ConsumerCode = "cmr-12345",
                RewardAmount = 97,
                SplitRewardOverflow = true
            };

            var walletType = new WalletTypeModel { WalletTypeId = 22, ConfigJson = "{\r\n  \"currency\": \"USD\"\r\n}" };
            _walletRepo.Setup(x => x.GetConsumerWallet(It.IsAny<int>(), It.IsAny<string>())).ReturnsAsync(new WalletMockModel()
            {
                TotalEarned= 100
            });
            _walletTypeRepo
                        .Setup(repo => repo.FindOneAsync(It.IsAny<Expression<Func<WalletTypeModel, bool>>>(), false))
                        .ReturnsAsync((Expression<Func<WalletTypeModel, bool>> predicate, bool _) =>
                        {
                            var compiledPredicate = predicate.Compile();

                            if (compiledPredicate(new WalletTypeModel { WalletTypeId = 7, DeleteNbr = 0, ConfigJson = "{\r\n  \"currency\": \"ENTRIES\"\r\n}" }))
                                return new WalletTypeModel() { WalletTypeId = 7, WalletTypeCode = "wat-c3b091232e974f98aeceb495d2a9f916", ConfigJson = "{\r\n  \"currency\": \"ENTRIES\"\r\n}" };

                            return walletType; // Default return if no match is found
                        });


            _walletTypeTransferRuleRepo.Setup(repo => repo.FindOneAsync(It.IsAny<Expression<Func<WalletTypeTransferRuleModel, bool>>>(), false))
            .ReturnsAsync(new WalletTypeTransferRuleModel() { WalletTypeTransferRuleId = 2, TransferRule = "{\"transferRatio\":0.25}", TargetWalletTypeId = 7 });

            // Act
            var result = await _walletService.RewardDetails(request);

            // Assert
            Assert.NotNull(result);
            Assert.Null(result.ErrorCode);

        }

        [Fact]
        public async Task RewardDetails_RewardLimitIsOverFlowed_true_Split_sweepsTakesWallet()
        {
            // Arrange
            var transactionMock = new Mock<ITransaction>();
            _session.Setup(s => s.BeginTransaction()).Returns(transactionMock.Object);
            var request = new PostRewardRequestDto
            {
                MasterWalletTypeCode = "MasterType",
                ConsumerWalletTypeCode = "ConsumerType",
                ConsumerCode = "cmr-12345",
                RewardAmount = 2000,
                SplitRewardOverflow = true
            };

            var walletType = new WalletTypeModel { WalletTypeId = 22, ConfigJson = "{\r\n  \"currency\": \"USD\"\r\n}" };
            _walletRepo.Setup(x => x.GetConsumerWallet(It.IsAny<int>(), It.IsAny<string>())).ReturnsAsync(new WalletMockModel()
            {
                TotalEarned = 100
            });
            _walletTypeRepo
                        .Setup(repo => repo.FindOneAsync(It.IsAny<Expression<Func<WalletTypeModel, bool>>>(), false))
                        .ReturnsAsync((Expression<Func<WalletTypeModel, bool>> predicate, bool _) =>
                        {
                            var compiledPredicate = predicate.Compile();

                            if (compiledPredicate(new WalletTypeModel { WalletTypeId = 7, DeleteNbr = 0, ConfigJson = "{\r\n  \"currency\": \"ENTRIES\"\r\n}" }))
                                return new WalletTypeModel() { WalletTypeId = 7, WalletTypeCode = "wat-c3b091232e974f98aeceb495d2a9f916", ConfigJson = "{\r\n  \"currency\": \"ENTRIES\"\r\n}" };

                            return walletType; // Default return if no match is found
                        });


            _walletTypeTransferRuleRepo.Setup(repo => repo.FindOneAsync(It.IsAny<Expression<Func<WalletTypeTransferRuleModel, bool>>>(), false))
            .ReturnsAsync(new WalletTypeTransferRuleModel() { WalletTypeTransferRuleId = 2, TransferRule = "{\"transferRatio\":0.25}", TargetWalletTypeId = 7 });

            // Act
            var result = await _walletService.RewardDetails(request);

            // Assert
            Assert.NotNull(result);
            Assert.Null(result.ErrorCode);

        }

        [Fact]
        public async Task GetWalletTypeTransferRule_ReturnsSuccess_WhenOverflowAndRuleExist()
        {
            // Arrange
            var consumerCode = "CONSUMER123";
            var tenantCode = "TENANT123";

            var request = new GetWalletTypeTransferRule
            {
                ConsumerCode = consumerCode,
                TenantCode = tenantCode
            };

            var walletDetail1 = new ConsumerWalletDetailDto
            {
                Wallet = new WalletDto { TotalEarned = 500, EarnMaximum = 500 },
                WalletType = new WalletTypeDto { WalletTypeCode = "wat-2d62dcaf2aa4424b9ff6c2ddb5895077", WalletTypeId = 1 },
                ConsumerWallet = new ConsumerWalletDto() { ConsumerCode = consumerCode , TenantCode = tenantCode } 
            };

            var walletDetail2 = new ConsumerWalletDetailDto
            {
                Wallet = new WalletDto { TotalEarned = 500, EarnMaximum = 500000 },
                WalletType = new WalletTypeDto { WalletTypeCode = "wat-c616be8b26c1449687ad0afda1512e7c", WalletTypeId = 7 },
                ConsumerWallet = new ConsumerWalletDto() { ConsumerCode = consumerCode, TenantCode = tenantCode }
            };


            _consumerWalletServiceMock
                .Setup(s => s.GetAllConsumerWalletsAsync(It.IsAny<GetConsumerWalletRequestDto>()))
                .ReturnsAsync(new ConsumerWalletResponseDto
                {
                    ConsumerWalletDetails = new List<ConsumerWalletDetailDto> { walletDetail1, walletDetail2 }
                });

            _walletTypeTransferRuleRepo.Setup(repo => repo.FindOneAsync(It.IsAny<Expression<Func<WalletTypeTransferRuleModel, bool>>>(), false))
           .ReturnsAsync(new WalletTypeTransferRuleModel() { WalletTypeTransferRuleId = 2, TransferRule = "{\"transferRatio\":0.25}", TargetWalletTypeId = 7 });


            var walletType = new WalletTypeModel { WalletTypeId = 2 };

            _walletTypeRepo.Setup(repo => repo.FindOneAsync(It.IsAny<Expression<Func<WalletTypeModel, bool>>>(), false))
                .ReturnsAsync(walletType);

            _walletRepo.Setup(r => r.GetMasterWallet(It.IsAny<long>(), tenantCode))
                .ReturnsAsync(new WalletModel());

            _walletRepo.Setup(r => r.GetConsumerWallet(2, consumerCode))
                .ReturnsAsync(new WalletModel { Xmin = 1 });

            // Act
            var result = await _walletController.GetWalletTypeTransferRule(request);

            // Assert
            Assert.NotNull(result);
            var actionResult = Assert.IsType<ActionResult<MaxWalletTransferRuleResponseDto>>(result);
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            var response = Assert.IsType<MaxWalletTransferRuleResponseDto>(okResult.Value);
            Assert.True(response.WalletOverFlowed);
            Assert.Null(response.ErrorCode);
        }


        [Fact]
        public async Task GetWalletTypeTransferRule_ReturnsFail_ConsumerWalletNotExist()
        {
            // Arrange
            var consumerCode = "CONSUMER123";
            var tenantCode = "TENANT123";

            var request = new GetWalletTypeTransferRule
            {
                ConsumerCode = consumerCode,
                TenantCode = tenantCode
            };

            // Act
            var result = await _walletController.GetWalletTypeTransferRule(request);

            // Assert
            Assert.NotNull(result);
            var actionResult = Assert.IsType<ActionResult<MaxWalletTransferRuleResponseDto>>(result);
            var okResult = Assert.IsType<ObjectResult>(actionResult.Result);
            var response = Assert.IsType<MaxWalletTransferRuleResponseDto>(okResult.Value);
            Assert.False(response.WalletOverFlowed);
            Assert.Contains("Consumer wallet not found for ConsumerCode", response.ErrorMessage);
            Assert.NotNull(response.ErrorCode);
        }


        [Fact]
        public async Task GetWalletTypeTransferRule_Returnsfail_ConsumerHasNoRelevantWalletst()
        {
            // Arrange
            var consumerCode = "CONSUMER123";
            var tenantCode = "TENANT123";

            var request = new GetWalletTypeTransferRule
            {
                ConsumerCode = consumerCode,
                TenantCode = tenantCode
            };

            var walletDetail1 = new ConsumerWalletDetailDto
            {
                Wallet = new WalletDto { TotalEarned = 500, EarnMaximum = 500 },
                WalletType = new WalletTypeDto { WalletTypeCode = "abc", WalletTypeId = 1 },
                ConsumerWallet = new ConsumerWalletDto() { ConsumerCode = consumerCode, TenantCode = tenantCode }
            };

            var walletDetail2 = new ConsumerWalletDetailDto
            {
                Wallet = new WalletDto { TotalEarned = 500, EarnMaximum = 500000 },
                WalletType = new WalletTypeDto { WalletTypeCode = "pqr", WalletTypeId = 7 },
                ConsumerWallet = new ConsumerWalletDto() { ConsumerCode = consumerCode, TenantCode = tenantCode }
            };


            _consumerWalletServiceMock
                .Setup(s => s.GetAllConsumerWalletsAsync(It.IsAny<GetConsumerWalletRequestDto>()))
                .ReturnsAsync(new ConsumerWalletResponseDto
                {
                    ConsumerWalletDetails = new List<ConsumerWalletDetailDto> { walletDetail1, walletDetail2 }
                });


            // Act
            var result = await _walletController.GetWalletTypeTransferRule(request);

            // Assert
            Assert.NotNull(result);
            var actionResult = Assert.IsType<ActionResult<MaxWalletTransferRuleResponseDto>>(result);
            var okResult = Assert.IsType<ObjectResult>(actionResult.Result);
            var response = Assert.IsType<MaxWalletTransferRuleResponseDto>(okResult.Value);
            Assert.False(response.WalletOverFlowed);
            Assert.Contains("Consumer wallet not found for ConsumerCode", response.ErrorMessage);
            Assert.NotNull(response.ErrorCode);
        }

        [Fact]
        public async Task GetWalletTypeTransferRule_ReturnsSuccess_NoWalletOverFlow()
        {
            // Arrange
            var consumerCode = "CONSUMER123";
            var tenantCode = "TENANT123";

            var request = new GetWalletTypeTransferRule
            {
                ConsumerCode = consumerCode,
                TenantCode = tenantCode
            };

            var walletDetail1 = new ConsumerWalletDetailDto
            {
                Wallet = new WalletDto { TotalEarned = 500, EarnMaximum = 5000 },
                WalletType = new WalletTypeDto { WalletTypeCode = "wat-2d62dcaf2aa4424b9ff6c2ddb5895077", WalletTypeId = 1 },
                ConsumerWallet = new ConsumerWalletDto() { ConsumerCode = consumerCode, TenantCode = tenantCode }
            };

            var walletDetail2 = new ConsumerWalletDetailDto
            {
                Wallet = new WalletDto { TotalEarned = 500, EarnMaximum = 500000 },
                WalletType = new WalletTypeDto { WalletTypeCode = "wat-c616be8b26c1449687ad0afda1512e7c", WalletTypeId = 7 },
                ConsumerWallet = new ConsumerWalletDto() { ConsumerCode = consumerCode, TenantCode = tenantCode }
            };

            _consumerWalletServiceMock
                .Setup(s => s.GetAllConsumerWalletsAsync(It.IsAny<GetConsumerWalletRequestDto>()))
                .ReturnsAsync(new ConsumerWalletResponseDto
                {
                    ConsumerWalletDetails = new List<ConsumerWalletDetailDto> { walletDetail1, walletDetail2 }
                });

            // Act
            var result = await _walletController.GetWalletTypeTransferRule(request);

            // Assert
            Assert.NotNull(result);
            var actionResult = Assert.IsType<ActionResult<MaxWalletTransferRuleResponseDto>>(result);
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            var response = Assert.IsType<MaxWalletTransferRuleResponseDto>(okResult.Value);
            Assert.False(response.WalletOverFlowed);
            Assert.Null(response.ErrorCode);
        }


        [Fact]
        public async Task GetWalletTypeTransferRule_ReturnsSuccess_TargetConsumerWalletNotFound()
        {
            // Arrange
            var consumerCode = "CONSUMER123";
            var tenantCode = "TENANT123";

            var request = new GetWalletTypeTransferRule
            {
                ConsumerCode = consumerCode,
                TenantCode = tenantCode
            };

            var walletDetail1 = new ConsumerWalletDetailDto
            {
                Wallet = new WalletDto { TotalEarned = 500, EarnMaximum = 500 },
                WalletType = new WalletTypeDto { WalletTypeCode = "wat-2d62dcaf2aa4424b9ff6c2ddb5895077", WalletTypeId = 1 },
                ConsumerWallet = new ConsumerWalletDto() { ConsumerCode = consumerCode, TenantCode = tenantCode }
            };

            var walletDetail2 = new ConsumerWalletDetailDto
            {
                Wallet = new WalletDto { TotalEarned = 500, EarnMaximum = 500000 },
                WalletType = new WalletTypeDto { WalletTypeCode = "wat-c616be8b26c1449687ad0afda1512e7c", WalletTypeId = 7 },
                ConsumerWallet = new ConsumerWalletDto() { ConsumerCode = consumerCode, TenantCode = tenantCode }
            };

            _consumerWalletServiceMock
                .Setup(s => s.GetAllConsumerWalletsAsync(It.IsAny<GetConsumerWalletRequestDto>()))
                .ReturnsAsync(new ConsumerWalletResponseDto
                {
                    ConsumerWalletDetails = new List<ConsumerWalletDetailDto> { walletDetail1, walletDetail2 }
                });

            _walletTypeTransferRuleRepo.Setup(repo => repo.FindOneAsync(It.IsAny<Expression<Func<WalletTypeTransferRuleModel, bool>>>(), false))
          .ReturnsAsync(new WalletTypeTransferRuleModel() { WalletTypeTransferRuleId = 2, TransferRule = "{\"transferRatio\":0.25}", TargetWalletTypeId = 4 });


            // Act
            var result = await _walletController.GetWalletTypeTransferRule(request);

            // Assert
            Assert.NotNull(result);
            var actionResult = Assert.IsType<ActionResult<MaxWalletTransferRuleResponseDto>>(result);
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            var response = Assert.IsType<MaxWalletTransferRuleResponseDto>(okResult.Value);
            Assert.True(response.WalletOverFlowed);
            Assert.Null(response.walletTypeTransferRules[0].TargetConsumerWallet);
        }


        [Fact]
        public async Task GetWalletTypeTransferRule_Returnsfail_WhenException()
        {
            // Arrange
            var consumerCode = "CONSUMER123";
            var tenantCode = "TENANT123";

            var request = new GetWalletTypeTransferRule
            {
                ConsumerCode = consumerCode,
                TenantCode = tenantCode
            };




            _consumerWalletServiceMock
                .Setup(s => s.GetAllConsumerWalletsAsync(It.IsAny<GetConsumerWalletRequestDto>()))
                .ThrowsAsync(new Exception("Test Exeption"));


            // Act
            var result = await _walletController.GetWalletTypeTransferRule(request);

            // Assert
            Assert.NotNull(result);
            var actionResult = Assert.IsType<ActionResult<MaxWalletTransferRuleResponseDto>>(result);
            var okResult = Assert.IsType<ObjectResult>(actionResult.Result);
            var response = Assert.IsType<MaxWalletTransferRuleResponseDto>(okResult.Value);
            Assert.False(response.WalletOverFlowed);
            Assert.Contains("Test Exeption", response.ErrorMessage);
            Assert.NotNull(response.ErrorCode);
        }
        [Fact]
        public async Task ImportWalletTypes_ReturnsOk_WhenImportIsSuccessful()
        {
            // Arrange
            var request = new ImportWalletTypeRequestDto
            {
                WalletTypes = new List<WalletTypeDto>
                {
                    new WalletTypeDto { WalletTypeCode = "wat-2d62dcaf2aa4424b9ff6c2ddb5895099", WalletTypeName = "OTC" }
                }
            };

           _walletTypeRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<WalletTypeModel, bool>>>(), false)).ReturnsAsync(new WalletTypeMockModel());

            // Act
            var result = await _walletController.ImportWalletTypesAsync(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
        }
        [Fact]
        public async Task ImportWalletTypes_ReturnsOk_WhenImportIsSuccessful_With_Existing_WalletTypes()
        {
            // Arrange
            var request = new ImportWalletTypeRequestDto
            {
                WalletTypes = new List<WalletTypeDto>
                {
                    new WalletTypeDto { WalletTypeCode = "wat-2d62dcaf2aa4424b9ff6c2ddb5895066", WalletTypeName = "Health Actions Reward" }
                }
            };
            _walletTypeRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<WalletTypeModel, bool>>>(), false)).ReturnsAsync(new WalletTypeMockModel());

            // Act
            var result = await _walletController.ImportWalletTypesAsync(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
        }
        [Fact]
        public async Task ImportWalletTypes_ReturnsPartialContent_WhenSomeErrorsOccurred()
        {
            // Arrange
            var request = new ImportWalletTypeRequestDto
            {
                WalletTypes = new List<WalletTypeDto>
                {
                    new WalletTypeDto { WalletTypeCode = "wat-2d62dcaf2aa4424b9ff6c2ddb5895077", WalletTypeName = "Reward" }
                }
            };
            
            _walletTypeRepo.Setup(x => x.CreateAsync(It.IsAny<WalletTypeModel>())).Throws(new Exception("data base error"));

            // Act
            var result = await _walletController.ImportWalletTypesAsync(request);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status206PartialContent, objectResult.StatusCode);
        }
        [Fact]
        public async Task ImportWalletTypes_ReturnsInternalServerError_WhenExceptionThrown()
        {
            // Arrange
            var request = new ImportWalletTypeRequestDto
            {
                WalletTypes = new List<WalletTypeDto>
                {
                    new WalletTypeDto { WalletTypeCode = "wat-2d62dcaf2aa4424b9ff6c2ddb5895077", WalletTypeName = "FOOD" }
                }
            };
            var service = new Mock<IWalletService>();
            service.Setup(x => x.ImportWalletTypesAsync(It.IsAny<ImportWalletTypeRequestDto>())).ThrowsAsync(new Exception("Simulated Failure"));
            var controller = new WalletController(_walletLogger.Object, service.Object);

            // Act
            var result = await controller.ImportWalletTypesAsync(request);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
        }
    }
}