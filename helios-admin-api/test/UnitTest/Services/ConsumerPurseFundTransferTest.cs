using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using SunnyBenefits.Fis.Core.Domain.Dtos;
using SunnyBenefits.Fis.Core.Domain.Dtos.Json;
using SunnyRewards.Helios.Admin.Infrastructure.HttpClients.Interfaces;
using SunnyRewards.Helios.Admin.Infrastructure.Services;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.Tenant.Core.Domain.Models;
using SunnyRewards.Helios.Wallet.Core.Domain.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace SunnyRewards.Helios.Admin.UnitTests.Services
{
    public class ConsumerPurseAssignmentServiceTests
    {
        private readonly Mock<IFisClient> _fisClientMock = new();
        private readonly Mock<IUserClient> _userClientMock = new();
        private readonly Mock<IWalletClient> _walletClientMock = new();
        private readonly Mock<ILogger<ConsumerPurseAssignmentService>> _loggerMock = new();
        private readonly Mock<ILoggerFactory> _loggerFactoryMock = new();

        private readonly ConsumerPurseAssignmentService _service;

        private const string ConsumerCode = "C123";
        private const string TenantCode = "T001";

        public ConsumerPurseAssignmentServiceTests()
        {
            // Default logger factory behavior
            var sagaLoggerMock = new Mock<ILogger<SagaExecutor>>();
            var fisLoggerMock = new Mock<ILogger<FisValueAdjustStep>>();
            var fisLoadLoggerMock = new Mock<ILogger<FisValueLoadStep>>();
            var walletLoggerMock = new Mock<ILogger<CreateTransactionStep>>();

            var loggerFactoryMock = new Mock<ILoggerFactory>();

            loggerFactoryMock
                .Setup(f => f.CreateLogger(It.IsAny<string>()))
                .Returns((string categoryName) =>
                {
                    if (categoryName.Contains(nameof(SagaExecutor)))
                        return sagaLoggerMock.Object;
                    if (categoryName.Contains(nameof(FisValueAdjustStep)))
                        return fisLoggerMock.Object;
                    if (categoryName.Contains(nameof(FisValueLoadStep)))
                        return fisLoadLoggerMock.Object;
                    if (categoryName.Contains(nameof(CreateTransactionStep)))
                        return walletLoggerMock.Object;

                    return new Mock<ILogger>().Object;
                });


            _service = new ConsumerPurseAssignmentService(
                _loggerMock.Object,
                _fisClientMock.Object,
                _walletClientMock.Object,
                _userClientMock.Object,
                loggerFactoryMock.Object
            );
        }

        private ConsumerAccountPurse PurseAccount(string label) => new()
        {
            PurseLabel = label
        };

        private Purse PurseTenant(string label, string type) => new()
        {
            PurseLabel = label,
            PurseWalletType = type
        };

        private FindConsumerWalletResponseDto WalletResponse(long id) => new()
        {
            ConsumerWallets = new List<ConsumerWalletDto>
            {
                new() { WalletId = id }
            }
        };

        // ---------- TEST CASES ----------

        [Fact]
        public async System.Threading.Tasks.Task PurseBalanceTransfer_ShouldReturnSuccess_WhenBalanceExists()
        {
            // Arrange
            var addedPurse = PurseAccount("OTC");
            var removedPurse = PurseAccount("CFO");
            var addedTenant = PurseTenant("OTC", "wat-bb06d4c12ac84213bc59bc2093421264");
            var removedTenant = PurseTenant("CFO", "wat-4b364ed612f04034bf732b355d84f368");

            _walletClientMock
                .Setup(x => x.Post<FindConsumerWalletResponseDto>(
                    It.Is<string>(url => url.Contains("consumer-wallet")),
                    It.IsAny<object>()))
                .ReturnsAsync(WalletResponse(101));

            _walletClientMock
                .Setup(x => x.GetById<WalletDto>("wallet/", 101))
                .ReturnsAsync(new WalletDto { WalletId = 101, Balance = 200 });

            _fisClientMock
                .Setup(x => x.Post<ExternalSyncWalletResponseDto>(
                    "fis/get-purse-balances", It.IsAny<object>()))
                .ReturnsAsync(new ExternalSyncWalletResponseDto
                {
                    Wallets = new List<ExternalSyncWalletDto>
                    {
                        new() {
                            PurseWalletType = "wat-4b364ed612f04034bf732b355d84f368",
                            Wallet = new WalletDto { Balance = 50 }
                        }
                    }
                });

            _fisClientMock
                .Setup(x => x.Post<AdjustValueResponseDto>(
                    "fis/adjust-value", It.IsAny<object>()))
                .ReturnsAsync(new AdjustValueResponseDto
                {
                    ErrorCode = null,
                    ErrorMessage = null,
                    PurseBalance = 0
                });

            _fisClientMock
                .Setup(x => x.Post<LoadValueResponseDto>(
                    "fis/load-value", It.IsAny<object>()))
                .ReturnsAsync(new LoadValueResponseDto
                {
                    ErrorCode = null,
                    ErrorMessage = null
                });

            _walletClientMock
               .Setup(x => x.Post<CreateTransactionsResponseDto>(
                   It.Is<string>(url => url.Contains("transaction/create-transactions")),
                   It.IsAny<object>()))
               .ReturnsAsync(new CreateTransactionsResponseDto() {TransactionDetailId = 100 });

            // Act
            var result = await _service.purseBalanceTransfer(
                ConsumerCode, TenantCode, addedPurse, addedTenant, removedPurse, removedTenant);

            // Assert
            Assert.NotNull(result);
            _loggerMock.Verify(x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, _) => v.ToString().Contains("purse-2-Purse Transfer result")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);
        }

        [Fact]
        public async System.Threading.Tasks.Task PurseBalanceTransfer_ShouldReturnSuccess_whenStep2lalueLoadFails()
        {
            // Arrange
            var addedPurse = PurseAccount("OTC");
            var removedPurse = PurseAccount("CFO");
            var addedTenant = PurseTenant("OTC", "wat-bb06d4c12ac84213bc59bc2093421264");
            var removedTenant = PurseTenant("CFO", "wat-4b364ed612f04034bf732b355d84f368");

            _walletClientMock
                .Setup(x => x.Post<FindConsumerWalletResponseDto>(
                    It.Is<string>(url => url.Contains("consumer-wallet")),
                    It.IsAny<object>()))
                .ReturnsAsync(WalletResponse(101));

            _walletClientMock
                .Setup(x => x.GetById<WalletDto>("wallet/", 101))
                .ReturnsAsync(new WalletDto { WalletId = 101, Balance = 200 });

            _fisClientMock
                .Setup(x => x.Post<ExternalSyncWalletResponseDto>(
                    "fis/get-purse-balances", It.IsAny<object>()))
                .ReturnsAsync(new ExternalSyncWalletResponseDto
                {
                    Wallets = new List<ExternalSyncWalletDto>
                    {
                        new() {
                            PurseWalletType = "wat-4b364ed612f04034bf732b355d84f368",
                            Wallet = new WalletDto { Balance = 50 }
                        }
                    }
                });

            _fisClientMock
                .Setup(x => x.Post<AdjustValueResponseDto>(
                    "fis/adjust-value", It.IsAny<object>()))
                .ReturnsAsync(new AdjustValueResponseDto
                {
                    ErrorCode = null,
                    ErrorMessage = null,
                    PurseBalance = 0
                });

            _fisClientMock
                .Setup(x => x.Post<LoadValueResponseDto>(
                    "fis/load-value", It.Is<LoadValueRequestDto>(r => r.PurseWalletType == "wat-bb06d4c12ac84213bc59bc2093421264")))
                .ReturnsAsync(new LoadValueResponseDto
                {
                    ErrorCode = 500,                           
                    ErrorMessage = null
                });

            _fisClientMock
               .Setup(x => x.Post<LoadValueResponseDto>(
                   "fis/load-value", It.Is<LoadValueRequestDto>(r => r.PurseWalletType == "wat-4b364ed612f04034bf732b355d84f368")))
               .ReturnsAsync(new LoadValueResponseDto
               {
                   ErrorCode = null,                            
                   ErrorMessage = null
               });

            _walletClientMock
               .Setup(x => x.Post<CreateTransactionsResponseDto>(
                   It.Is<string>(url => url.Contains("transaction/create-transactions")),
                   It.IsAny<object>()))
               .ReturnsAsync(new CreateTransactionsResponseDto() { TransactionDetailId = 100 });

            // Act
            var result = await _service.purseBalanceTransfer(
                ConsumerCode, TenantCode, addedPurse, addedTenant, removedPurse, removedTenant);

            // Assert
            Assert.NotNull(result);
            Assert.Contains("Saga failed. Compensation executed. Details: Operation failed after 3 retries.", result.ErrorMessage);
        }

        [Fact]
        public async System.Threading.Tasks.Task PurseBalanceTransfer_ShouldReturnEmpty_WhenNoFunds()
        {
            // Arrange
            var addedPurse = PurseAccount("OTC");
            var removedPurse = PurseAccount("CFO");
            var addedTenant = PurseTenant("OTC", "wat-bb06d4c12ac84213bc59bc2093421264");
            var removedTenant = PurseTenant("CFO", "wat-4b364ed612f04034bf732b355d84f368");

            _walletClientMock
                .Setup(x => x.Post<FindConsumerWalletResponseDto>(
                    It.IsAny<string>(), It.IsAny<object>()))
                .ReturnsAsync(WalletResponse(101));

            _walletClientMock
                .Setup(x => x.GetById<WalletDto>("wallet/", 101))
                .ReturnsAsync(new WalletDto { WalletId = 101, Balance = 200 });

            _fisClientMock
                .Setup(x => x.Post<ExternalSyncWalletResponseDto>(
                    "fis/get-purse-balances", It.IsAny<object>()))
                .ReturnsAsync(new ExternalSyncWalletResponseDto
                {
                    Wallets = new List<ExternalSyncWalletDto>
                    {
                        new() {
                            PurseWalletType = "OTC",
                            Wallet = new WalletDto { Balance = 0 } 
                        }
                    }
                });

            // Act
            var result = await _service.purseBalanceTransfer(
                ConsumerCode, TenantCode, addedPurse, addedTenant, removedPurse, removedTenant);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(default, result.ErrorCode);
            _loggerMock.Verify(x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, _) => v.ToString().Contains("No funds to transfer")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);
        }

        [Fact]
        public async System.Threading.Tasks.Task PurseBalanceTransfer_ShouldReturn404_WhenFisThrows()
        {
            // Arrange
            var addedPurse = PurseAccount("OTC");
            var removedPurse = PurseAccount("CFO");
            var addedTenant = PurseTenant("OTC", "wat-bb06d4c12ac84213bc59bc2093421264");
            var removedTenant = PurseTenant("CFO", "wat-4b364ed612f04034bf732b355d84f368");

            _walletClientMock
                .Setup(x => x.Post<FindConsumerWalletResponseDto>(
                    It.IsAny<string>(), It.IsAny<object>()))
                .ReturnsAsync(WalletResponse(101));

            _walletClientMock
                .Setup(x => x.GetById<WalletDto>("wallet/", 101))
                .ReturnsAsync(new WalletDto { WalletId = 101, Balance = 200 });

            _fisClientMock
                .Setup(x => x.Post<ExternalSyncWalletResponseDto>(
                    "fis/get-purse-balances", It.IsAny<object>()))
                .ThrowsAsync(new Exception("Network issue"));

            // Act
            var result = await _service.purseBalanceTransfer(
                ConsumerCode, TenantCode, addedPurse, addedTenant, removedPurse, removedTenant);

            // Assert
            Assert.Equal(StatusCodes.Status404NotFound, result.ErrorCode);
            Assert.Contains("Live Balance details not found", result.ErrorMessage);
            _loggerMock.Verify(
                x => x.Log(LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, _) => v.ToString().Contains("Live Balance details not found")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
