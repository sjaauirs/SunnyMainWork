using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using SunnyRewards.Helios.Wallet.Core.Domain.Dtos;
using SunnyRewards.Helios.Wallet.Core.Domain.Models;
using SunnyRewards.Helios.Wallet.Infrastructure.Repositories.Interfaces;
using SunnyRewards.Helios.Wallet.Infrastructure.Services;
using System.Linq.Expressions;
using Xunit;

namespace SunnyRewards.Helios.Wallet.UnitTest.Services
{
    public class WalletTypeTransferRuleServiceTests
    {
        private readonly Mock<IWalletTypeTransferRuleRepo> _mockTransferRuleRepo;
        private readonly Mock<IWalletTypeRepo> _mockWalletTypeRepo;
        private readonly Mock<ILogger<WalletTypeTransferRuleService>> _mockLogger;
        private readonly WalletTypeTransferRuleService _service;

        public WalletTypeTransferRuleServiceTests()
        {
            _mockTransferRuleRepo = new Mock<IWalletTypeTransferRuleRepo>();
            _mockWalletTypeRepo = new Mock<IWalletTypeRepo>();
            _mockLogger = new Mock<ILogger<WalletTypeTransferRuleService>>();
            _service = new WalletTypeTransferRuleService(
                _mockTransferRuleRepo.Object,
                _mockWalletTypeRepo.Object,
                _mockLogger.Object
            );
        }

        [Fact]
        public async Task ImportWalletTypeTransferRules_AllValidEntries_ShouldCreateOrUpdate()
        {
            // Arrange
            var dto = new ExportWalletTypeTransferRuleDto
            {
                TenantCode = "TENANT",
                WalletTypeTransferRuleCode = "RULE001",
                SourceWalletTypeCode = "SRC",
                TargetWalletTypeCode = "TGT",
                TransferRule = "SOME_RULE"
            };

            var sourceWallet = new WalletTypeModel { WalletTypeId = 1, WalletTypeCode = "SRC" };
            var targetWallet = new WalletTypeModel { WalletTypeId = 2, WalletTypeCode = "TGT" };
            _mockWalletTypeRepo
                .Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<WalletTypeModel, bool>>>(), false))
                .ReturnsAsync(sourceWallet);
            _mockWalletTypeRepo
                .Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<WalletTypeModel, bool>>>(), false))
                .ReturnsAsync(targetWallet);

            _mockTransferRuleRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<WalletTypeTransferRuleModel, bool>>>(), false))
                .ReturnsAsync((WalletTypeTransferRuleModel)null);

            // Act
            var result = await _service.ImportWalletTypeTransferRules(new ImportWalletTypeTransferRuleRequestDto
            {
                TenantCode = "Tenant123",
                WalletTypeTransferRules = new List<ExportWalletTypeTransferRuleDto> { dto }
            });

            // Assert
            Assert.Null(result.ErrorCode);
            _mockTransferRuleRepo.Verify(x => x.CreateAsync(It.IsAny<WalletTypeTransferRuleModel>()), Times.Once);
        }

        [Fact]
        public async Task ImportWalletTypeTransferRules_ReturnsPartialContent_WhenSomeRulesFail()
        {
            // Arrange
            var walletTypeTransferRules = new List<ExportWalletTypeTransferRuleDto>
            {
                new ExportWalletTypeTransferRuleDto
                {
                    WalletTypeTransferRuleCode = "RULE001",
                    TenantCode = "Tenant123",
                    SourceWalletTypeCode = "SRC001",
                    TargetWalletTypeCode = "TGT001",
                    TransferRule = "Allow"
                },
                new ExportWalletTypeTransferRuleDto
                {
                    WalletTypeTransferRuleCode = "RULE002",
                    TenantCode = "Tenant123",
                    SourceWalletTypeCode = "SRC002",
                    TargetWalletTypeCode = "TGT002",
                    TransferRule = "Deny"
                }
            };
            var request = new ImportWalletTypeTransferRuleRequestDto
            {
                TenantCode = "Tenant123",
                WalletTypeTransferRules = walletTypeTransferRules
            };

            _mockWalletTypeRepo
              .SetupSequence(repo => repo.FindOneAsync(It.IsAny<Expression<Func<WalletTypeModel, bool>>>(), false))
              .ReturnsAsync(new WalletTypeModel { WalletTypeId = 1 }) // First rule succeeds
              .ReturnsAsync((WalletTypeModel)null)
              .ReturnsAsync(new WalletTypeModel { WalletTypeId = 1 })
              .ReturnsAsync(new WalletTypeModel { WalletTypeId = 2 });

            _mockTransferRuleRepo
                .Setup(repo => repo.FindOneAsync(It.IsAny<Expression<Func<WalletTypeTransferRuleModel, bool>>>(), false))
                .ReturnsAsync((WalletTypeTransferRuleModel)null);

            // Act
            var result = await _service.ImportWalletTypeTransferRules(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(StatusCodes.Status206PartialContent, result.ErrorCode);
            Assert.Equal("1 errors occurred while importing transfer rules.", result.ErrorMessage);
        }

        [Fact]
        public async Task ImportWalletTypeTransferRules_UpdatesExistingRule_WhenRuleExists()
        {
            // Arrange
            var walletTypeTransferRules = new List<ExportWalletTypeTransferRuleDto>
            {
                new ExportWalletTypeTransferRuleDto
                {
                    WalletTypeTransferRuleCode = "RULE001",
                    TenantCode = "Tenant123",
                    SourceWalletTypeCode = "SRC001",
                    TargetWalletTypeCode = "TGT001",
                    TransferRule = "Allow"
                }
            };
            var request = new ImportWalletTypeTransferRuleRequestDto
            {
                TenantCode = "Tenant123",
                WalletTypeTransferRules = walletTypeTransferRules
            };
            var existingRule = new WalletTypeTransferRuleModel
            {
                WalletTypeTransferRuleId = 1,
                WalletTypeTransferRuleCode = "RULE001",
                TenantCode = "Tenant123",
                SourceWalletTypeId = 1,
                TargetWalletTypeId = 2,
                TransferRule = "Deny"
            };

            _mockWalletTypeRepo
                .Setup(repo => repo.FindOneAsync(It.IsAny<Expression<Func<WalletTypeModel, bool>>>(), false))
                .ReturnsAsync(new WalletTypeModel { WalletTypeId = 1 });

            _mockTransferRuleRepo
                .Setup(repo => repo.FindOneAsync(It.IsAny<Expression<Func<WalletTypeTransferRuleModel, bool>>>(), false))
                .ReturnsAsync(existingRule);

            // Act
            var result = await _service.ImportWalletTypeTransferRules(request);

            // Assert
            Assert.NotNull(result);
            Assert.Null(result.ErrorCode);
            Assert.Null(result.ErrorMessage);
            _mockTransferRuleRepo.Verify(repo => repo.UpdateAsync(It.Is<WalletTypeTransferRuleModel>(r =>
                r.TransferRule == "Allow" &&
                r.SourceWalletTypeId == 1 &&
                r.TargetWalletTypeId == 1
            )), Times.Once);
        }

        [Fact]
        public async Task ImportWalletTypeTransferRules_ThrowsException_WhenUnexpectedErrorOccurs()
        {
            // Arrange
            var walletTypeTransferRules = new List<ExportWalletTypeTransferRuleDto>
            {
                new ExportWalletTypeTransferRuleDto
                {
                    WalletTypeTransferRuleCode = "RULE001",
                    TenantCode = "Tenant123",
                    SourceWalletTypeCode = "SRC001",
                    TargetWalletTypeCode = "TGT001",
                    TransferRule = "Allow"
                }
            };
            var request = new ImportWalletTypeTransferRuleRequestDto
            {
                TenantCode = "Tenant123",
                WalletTypeTransferRules = walletTypeTransferRules
            };

            _mockWalletTypeRepo
                .Setup(repo => repo.FindOneAsync(It.IsAny<Expression<Func<WalletTypeModel, bool>>>(), false))
                .ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _service.ImportWalletTypeTransferRules(request));
        }
    }
}
