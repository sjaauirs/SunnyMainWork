using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.Common.Core.Repositories;
using SunnyRewards.Helios.Wallet.Api.Controllers;
using SunnyRewards.Helios.Wallet.Core.Domain.Dtos;
using SunnyRewards.Helios.Wallet.Core.Domain.Models;
using SunnyRewards.Helios.Wallet.Infrastructure.Repositories;
using SunnyRewards.Helios.Wallet.Infrastructure.Repositories.Interfaces;
using SunnyRewards.Helios.Wallet.Infrastructure.Services;
using SunnyRewards.Helios.Wallet.Infrastructure.Services.Interfaces;
using Xunit;

namespace SunnyRewards.Helios.Wallet.UnitTest.Controllers
{
    public class WalletTypeTransferRuleControllerTests
    {
        private readonly Mock<IWalletTypeTransferRuleService> _mockService;
        private readonly Mock<IWalletTypeTransferRuleRepo> _mockRepo;
        private readonly Mock<ILogger<WalletTypeTransferRuleController>> _mockLogger;
        private readonly Mock<ILogger<WalletTypeTransferRuleService>> _serviceLogger;
        private readonly WalletTypeTransferRuleController _controller;

        public WalletTypeTransferRuleControllerTests()
        {
            _mockRepo = new Mock<IWalletTypeTransferRuleRepo>();
            _serviceLogger = new Mock<ILogger<WalletTypeTransferRuleService>>();
            _mockService = new Mock<IWalletTypeTransferRuleService>();
            _mockLogger = new Mock<ILogger<WalletTypeTransferRuleController>>();
            _controller = new WalletTypeTransferRuleController(_mockService.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task ExportWalletTypeTransferRule_ReturnsOk_WhenServiceReturnsResponse()
        {
            // Arrange
            var request = new ExportWalletTypeTransferRuleRequestDto { TenantCode = "Tenant123" };
            var response = new List<ExportWalletTypeTransferRuleDto>
            {
                new ExportWalletTypeTransferRuleDto
                {
                    SourceWalletTypeCode = "wty-c008f49aa31f4acd9aa6e2114bfb820e",
                    TargetWalletTypeCode = "wty-ecada21e57154928a2bb959e8365b8b4",
                    WalletTypeTransferRuleId = 1,
                    WalletTypeTransferRuleCode = "wtytr-c008f49aa31f4acd9aa6e2114bfb820e",
                    TenantCode = "ten-ecada21e57154928a2bb959e8365b8b4"
                }
            };

            _mockRepo.Setup(x => x.GetWalletTypeTransferRules(It.IsAny<string>())).ReturnsAsync(response);
            _mockService.Setup(x=>x.ExportWalletTypeTransferRules(It.IsAny<ExportWalletTypeTransferRuleRequestDto>()))
                .ReturnsAsync(new ExportWalletTypeTransferRuleResponseDto
                {
                    WalletTypeTransferRules = response
                });

            // Act
            var result = await _controller.ExportWalletTypeTransferRule(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var responseValue = Assert.IsType<ExportWalletTypeTransferRuleResponseDto>(okResult.Value);
            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
        }

        [Fact]
        public async Task ExportWalletTypeTransferRule_Returns500_WhenExceptionIsThrown()
        {
            // Arrange
            var request = new ExportWalletTypeTransferRuleRequestDto { TenantCode = "Tenant123" };


            _mockRepo.Setup(x => x.GetWalletTypeTransferRules(It.IsAny<string>())).ThrowsAsync(new Exception());
            _mockService.Setup(x => x.ExportWalletTypeTransferRules(It.IsAny<ExportWalletTypeTransferRuleRequestDto>()))
                .ThrowsAsync(new Exception("Internal Server Error"));
            // Act
            var result = await _controller.ExportWalletTypeTransferRule(request);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, objectResult.StatusCode);

            var errorResponse = Assert.IsType<BaseResponseDto>(objectResult.Value);
            Assert.Equal(500, errorResponse.ErrorCode);
            Assert.Equal("Internal Server Error", errorResponse.ErrorMessage);
        }
        [Fact]
        public async Task ExportWalletTypeTransferRule_Returns_NotFound_Response()
        {
            // Arrange
            var request = new ExportWalletTypeTransferRuleRequestDto { TenantCode = "Tenant123" };

            _mockRepo.Setup(x => x.GetWalletTypeTransferRules(It.IsAny<string>())).ReturnsAsync(new List<ExportWalletTypeTransferRuleDto>());
            _mockService.Setup(x => x.ExportWalletTypeTransferRules(It.IsAny<ExportWalletTypeTransferRuleRequestDto>()))
                .ReturnsAsync(new ExportWalletTypeTransferRuleResponseDto{});

            // Act
            var result = await _controller.ExportWalletTypeTransferRule(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var responseValue = Assert.IsType<ExportWalletTypeTransferRuleResponseDto>(okResult.Value);
            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
        }
        [Fact]
        public async Task GetWalletTypeTransferRules_ShouldReturnRules_ForGivenTenantCode()
        {
            // Arrange
            var tenantCode = "Tenant123";
            var rules = new List<WalletTypeTransferRuleModel>
            {
                new WalletTypeTransferRuleModel
                {
                    WalletTypeTransferRuleId = 1,
                    WalletTypeTransferRuleCode = "RULE001",
                    TenantCode = tenantCode,
                    SourceWalletTypeId = 101,
                    TargetWalletTypeId = 102,
                    TransferRule = "Allow",
                    DeleteNbr = 0
                }
            }.AsQueryable();

            var walletTypes = new List<WalletTypeModel>
            {
                new WalletTypeModel
                {
                    WalletTypeId = 101,
                    WalletTypeCode = "SRC",
                    DeleteNbr = 0
                },
                new WalletTypeModel
                {
                    WalletTypeId = 102,
                    WalletTypeCode = "TGT",
                    DeleteNbr = 0
                }
            }.AsQueryable();

            var mockSession = new Mock<NHibernate.ISession>();


            var logger = new Mock<ILogger<BaseRepo<WalletTypeTransferRuleModel>>>();
            var repo = new WalletTypeTransferRuleRepo(logger.Object, mockSession.Object);

            // Act
            var result = await repo.GetWalletTypeTransferRules(tenantCode);

            // Assert
            Assert.Single(result);
        }

        [Fact]
        public async Task ImportWalletTypeTransferRule_ReturnsOkResult_WhenServiceSucceeds()
        {
            // Arrange
            var walletTypeTransferRules = new List<ExportWalletTypeTransferRuleDto>
            {
                new ExportWalletTypeTransferRuleDto { WalletTypeTransferRuleId = 1, TenantCode = "Tenant123" }
            };
            var request = new ImportWalletTypeTransferRuleRequestDto
            {
                TenantCode = "Tenant123",
                WalletTypeTransferRules = walletTypeTransferRules
            };
            var response = new BaseResponseDto { ErrorCode = 0, ErrorMessage = "Success" };
            _mockService.Setup(s => s.ImportWalletTypeTransferRules(request)).ReturnsAsync(response);

            // Act
            var result = await _controller.ImportWalletTypeTransferRule(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(response, okResult.Value);
        }

        [Fact]
        public async Task ImportWalletTypeTransferRule_ReturnsInternalServerError_WhenServiceThrowsException()
        {
            // Arrange
            var walletTypeTransferRules = new List<ExportWalletTypeTransferRuleDto>
            {
                new ExportWalletTypeTransferRuleDto { WalletTypeTransferRuleId = 1, TenantCode = "Tenant123" }
            };
            var request = new ImportWalletTypeTransferRuleRequestDto
            {
                TenantCode = "Tenant123",
                WalletTypeTransferRules = walletTypeTransferRules
            };
            _mockService.Setup(s => s.ImportWalletTypeTransferRules(request)).ThrowsAsync(new System.Exception("Service error"));

            // Act
            var result = await _controller.ImportWalletTypeTransferRule(request);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusCodeResult.StatusCode);
            var response = Assert.IsType<BaseResponseDto>(statusCodeResult.Value);
            Assert.Equal(500, response.ErrorCode);
            Assert.Equal("Internal Server Error", response.ErrorMessage);
        }
    }
}
