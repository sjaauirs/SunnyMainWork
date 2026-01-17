using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.ClearScript;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using SunnyRewards.Helios.Admin.Api.Controllers;
using SunnyRewards.Helios.Admin.Infrastructure.HttpClients.Interfaces;
using SunnyRewards.Helios.Admin.Infrastructure.Services;
using SunnyRewards.Helios.Admin.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.Admin.UnitTest.Fixtures.MockDtos;
using SunnyRewards.Helios.Wallet.Core.Domain.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace SunnyRewards.Helios.Admin.UnitTest.Controllers
{
    public class TransactionControllerTest
    {
        private readonly TransactionController _transactionController;
        private readonly TransactionService _transactionService;
        private readonly Mock<ILogger<TransactionService>> _serviceLogger;
        private readonly Mock<ILogger<TransactionController>> _logger;
        private readonly Mock<IWalletClient> _walletClient;
        private readonly Mock<IWalletTypeService> _walletTypeService;
        private readonly Mock<IConfiguration> _config;
        public TransactionControllerTest()
        {
            _config = new Mock<IConfiguration>();
            _walletTypeService = new Mock<IWalletTypeService>();
            _walletClient = new Mock<IWalletClient>();
            _serviceLogger = new Mock<ILogger<TransactionService>>();
            _logger = new Mock<ILogger<TransactionController>>();
            _transactionService = new TransactionService(_serviceLogger.Object,_walletClient.Object,_walletTypeService.Object,_config.Object);
            _transactionController = new TransactionController(_logger.Object, _transactionService);
        }
        [Fact]
        public async System.Threading.Tasks.Task Get_Reward_Wallet_Transactions_Return_Ok_Response()
        {
            var requestDto = new GetWalletTransactionRequestDto();
            var responseDto = new GetWalletTransactionResponseDto() 
            { 
                Wallets = new List<WalletDto>() { new WalletDto() { WalletTypeId =1 } },
                Transactions = new List<TransactionEntryDto> { new TransactionEntryMockDto() }
            };

            _walletClient.Setup(x=>x.Post<GetWalletTransactionResponseDto>(It.IsAny<string>(),It.IsAny<GetWalletTransactionRequestDto>())).ReturnsAsync(responseDto);
            _walletTypeService.Setup(x => x.GetWalletTypeCode(It.IsAny<WalletTypeDto>())).ReturnsAsync(new WalletTypeMockDto());
            var mockSection = new Mock<IConfigurationSection>();
            mockSection.Setup(x => x.Value).Returns("wat-61377107367187610");
            _config.Setup(x => x.GetSection(It.IsAny<string>())).Returns(mockSection.Object);

            var response = await _transactionController.GetRewardsWalletTransactions(requestDto);

            Assert.NotNull(response);
            var okObjectResponse = Assert.IsType<OkObjectResult>(response);
            Assert.Equal(StatusCodes.Status200OK, okObjectResponse.StatusCode);
        }
        [Fact]
        public async System.Threading.Tasks.Task Get_Reward_Wallet_Transactions_Return_Error_Response_When_WalletType_NotFound()
        {
            // Arrange
            var requestDto = new GetWalletTransactionRequestDto();
            var responseDto = new GetWalletTransactionResponseDto()
            {
                Wallets = new List<WalletDto>() { new WalletDto() },
                Transactions = new List<TransactionEntryDto> { new TransactionEntryDto() }
            };
            var mockSection = new Mock<IConfigurationSection>();
            mockSection.Setup(x => x.Value).Returns("wat-61377107367187610");
            _config.Setup(x => x.GetSection(It.IsAny<string>())).Returns(mockSection.Object);
            _walletClient.Setup(x => x.Post<GetWalletTransactionResponseDto>(It.IsAny<string>(), It.IsAny<GetWalletTransactionRequestDto>())).ReturnsAsync(responseDto);
            _walletTypeService.Setup(x => x.GetWalletTypeCode(It.IsAny<WalletTypeDto>())).ReturnsAsync(new WalletTypeMockDto());

            // Act
            var response = await _transactionController.GetRewardsWalletTransactions(requestDto);

            // Assert
            Assert.NotNull(response);
            var objectResponse = Assert.IsType<ObjectResult>(response);
            Assert.Equal(StatusCodes.Status404NotFound, objectResponse.StatusCode);
        }
        [Fact]
        public async System.Threading.Tasks.Task Get_Reward_Wallet_Transactions_Return_Error_Response_When_WalletClient_Return_ErrorResponse()
        {
            // Arrange
            var requestDto = new GetWalletTransactionRequestDto();
            var responseDto = new GetWalletTransactionResponseDto()
            {
                ErrorCode = StatusCodes.Status500InternalServerError
            };

            _walletClient.Setup(x => x.Post<GetWalletTransactionResponseDto>(It.IsAny<string>(), It.IsAny<GetWalletTransactionRequestDto>())).ReturnsAsync(responseDto);

            // Act
            var response = await _transactionController.GetRewardsWalletTransactions(requestDto);

            // Assert
            Assert.NotNull(response);
            var objectResponse = Assert.IsType<ObjectResult>(response);
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResponse.StatusCode);
        }
        [Fact]
        public async System.Threading.Tasks.Task Get_Reward_Wallet_Transactions_Return_Error_Response_When_Exception_Occurs()
        {
            // Arrange
            var requestDto = new GetWalletTransactionRequestDto();
            var responseDto = new GetWalletTransactionResponseDto()
            {
                Wallets = new List<WalletDto>() { new WalletDto() },
                Transactions = new List<TransactionEntryDto> { new TransactionEntryDto() }
            };

            _walletClient.Setup(x => x.Post<GetWalletTransactionResponseDto>(It.IsAny<string>(), It.IsAny<GetWalletTransactionRequestDto>())).ThrowsAsync(new Exception("testing"));

            // Act
            var response = await _transactionController.GetRewardsWalletTransactions(requestDto);

            // Assert
            Assert.NotNull(response);
            var objectResponse = Assert.IsType<ObjectResult>(response);
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResponse.StatusCode);
        }
    }
}
