using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using NHibernate;
using SunnyRewards.Helios.Wallet.Api.Controllers;
using SunnyRewards.Helios.Wallet.Core.Domain.Models;
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
    public class PurseFundingControllerTests
    {
        private readonly Mock<ILogger<PurseFundingController>> _controllerLogger;
        private readonly IPurseFundingService _purseFundingService;
        private readonly Mock<ILogger<PurseFundingService>> _serviceLogger;
        private readonly Mock<IWalletTypeRepo> _walletTypeRepo;
        private readonly Mock<IWalletRepo> _walletRepo;
        private readonly Mock<NHibernate.ISession> _session;
        private readonly Mock<ITransactionRepo> _transactionRepo;
        private readonly IConfiguration _configuration;
        private readonly PurseFundingController _purseFundingController;

        public PurseFundingControllerTests()
        {
            _controllerLogger = new Mock<ILogger<PurseFundingController>>();
            _serviceLogger = new Mock<ILogger<PurseFundingService>>();
            _walletTypeRepo = new WalletTypeMockRepo();
            _walletRepo = new WalletMockRepo();
            _transactionRepo = new TransactionMockRepo();
            _session = new Mock<NHibernate.ISession>();
            _configuration = new ConfigurationBuilder()
              .SetBasePath(Directory.GetCurrentDirectory())
              .AddJsonFile("appsettings.Development.json")
              .Build();
            _purseFundingService = new PurseFundingService(_serviceLogger.Object, _walletTypeRepo.Object, _walletRepo.Object,
                _session.Object, _transactionRepo.Object, _configuration);
            _purseFundingController = new PurseFundingController(_controllerLogger.Object, _purseFundingService);

        }

        [Fact]
        public async Task PurseFundingAsync_ShouldReturnOk_WhenFundingIsSuccessful()
        {
            // Arrange
            var request = new PurseFundingRequestMockDto();
            var transactionMock = new Mock<ITransaction>();
            _session.Setup(s => s.BeginTransaction()).Returns(transactionMock.Object);
            _walletTypeRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<WalletTypeModel, bool>>>(), false))
              .ReturnsAsync(new WalletTypeMockModel());
            _walletRepo.Setup(x => x.GetWalletByConsumerAndWalletType(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<long>())).
                ReturnsAsync(new WalletMockModel());

            // Act
            var result = await _purseFundingController.PurseFundingAsync(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
        }

        [Fact]
        public async Task PurseFundingAsync_ShouldReturnErrorResult_WalletTypeNotFound()
        {
            // Arrange
            var request = new PurseFundingRequestMockDto();
            var transactionMock = new Mock<ITransaction>();
            _session.Setup(s => s.BeginTransaction()).Returns(transactionMock.Object);

            // Act
            var result = await _purseFundingController.PurseFundingAsync(request);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(StatusCodes.Status500InternalServerError, statusCodeResult.StatusCode);
        }

        [Fact]
        public async Task PurseFundingAsync_ShouldReturnErrorResult_WalletNotFound()
        {
            // Arrange
            var request = new PurseFundingRequestMockDto();
            var transactionMock = new Mock<ITransaction>();
            _session.Setup(s => s.BeginTransaction()).Returns(transactionMock.Object);
            _walletTypeRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<WalletTypeModel, bool>>>(), false))
              .ReturnsAsync(new WalletTypeMockModel());

            // Act
            var result = await _purseFundingController.PurseFundingAsync(request);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(StatusCodes.Status500InternalServerError, statusCodeResult.StatusCode);
        }

        [Fact]
        public async Task PurseFundingAsync_ShouldReturnErrorResult_WhenUpdateConsumerWalletBalanceFailed()
        {
            // Arrange
            var request = new PurseFundingRequestMockDto();
            var transactionMock = new Mock<ITransaction>();
            _session.Setup(s => s.BeginTransaction()).Returns(transactionMock.Object);
            _walletTypeRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<WalletTypeModel, bool>>>(), false))
              .ReturnsAsync(new WalletTypeMockModel());
            _walletRepo.Setup(x => x.GetWalletByConsumerAndWalletType(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<long>())).
                ReturnsAsync(new WalletMockModel());
            _walletRepo.Setup(x => x.UpdateMasterWalletBalance(It.IsAny<DateTime>(), It.IsAny<double?>(), It.IsAny<long>(), It.IsAny<int>())).
                Returns(0);
            // Act
            var result = await _purseFundingController.PurseFundingAsync(request);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(StatusCodes.Status500InternalServerError, statusCodeResult.StatusCode);
        }

        [Fact]
        public async Task PurseFundingAsync_ShouldReturnErrorResult_WhenUpdateMasterWalletBalanceFailed()
        {
            // Arrange
            var request = new PurseFundingRequestMockDto();
            var transactionMock = new Mock<ITransaction>();
            _session.Setup(s => s.BeginTransaction()).Returns(transactionMock.Object);
            _walletTypeRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<WalletTypeModel, bool>>>(), false))
              .ReturnsAsync(new WalletTypeMockModel());
            _walletRepo.Setup(x => x.GetWalletByConsumerAndWalletType(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<long>())).
                ReturnsAsync(new WalletMockModel());
            _walletRepo.Setup(x => x.UpdateMasterWalletBalance(It.IsAny<DateTime>(), It.IsAny<double?>(), It.IsAny<long>(), It.IsAny<int>())).
                Returns(0);
            // Act
            var result = await _purseFundingController.PurseFundingAsync(request);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(StatusCodes.Status500InternalServerError, statusCodeResult.StatusCode);
        }

        [Fact]
        public async Task PurseFundingAsync_ShouldReturnInternalServerError_WhenExceptionOccurs()
        {
            // Arrange
            var request = new PurseFundingRequestMockDto();
            var mockPurseFundingService = new Mock<IPurseFundingService>();
            mockPurseFundingService
                .Setup(service => service.PurseFundingAsync(request))
                .ThrowsAsync(new Exception("Database connection failed"));
            var controller = new PurseFundingController(_controllerLogger.Object, mockPurseFundingService.Object);

            // Act
            var result = await controller.PurseFundingAsync(request);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(StatusCodes.Status500InternalServerError, statusCodeResult.StatusCode);
        }
    }
}
