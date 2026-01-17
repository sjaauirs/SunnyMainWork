using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using SunnyBenefits.Fis.Core.Domain.Dtos;
using SunnyRewards.Helios.Admin.Infrastructure.Helpers;
using SunnyRewards.Helios.Admin.Infrastructure.HttpClients.Interfaces;
using SunnyRewards.Helios.Admin.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.Common.Core.Extensions;
using SunnyRewards.Helios.Tenant.Core.Domain.Dtos;
using SunnyRewards.Helios.Tenant.Core.Domain.Dtos.Json;
using SunnyRewards.Helios.User.Core.Domain.Dtos;
using SunnyRewards.Helios.Wallet.Core.Domain.Dtos;
using Xunit;
using TaskAlias = System.Threading.Tasks.Task;

namespace SunnyRewards.Helios.Admin.UnitTest.Helpers
{
    public class WalletHelperUnitTest
    {
        private readonly Mock<IConfiguration> _configMock;
        private readonly Mock<ILogger<WalletHelper>> _loggerMock;
        private readonly Mock<IConsumerWalletService> _consumerWalletServiceMock;
        private readonly Mock<IConsumerService> _consumerServiceMock;
        private readonly Mock<ITenantService> _tenantServiceMock;
        private readonly Mock<ITenantAccountService> _tenantAccountServiceMock;
        private readonly Mock<IWalletClient> _walletClientMock;

        private readonly WalletHelper _walletHelper;

        public WalletHelperUnitTest()
        {
            _configMock = new Mock<IConfiguration>();
            _loggerMock = new Mock<ILogger<WalletHelper>>();
            _consumerWalletServiceMock = new Mock<IConsumerWalletService>();
            _consumerServiceMock = new Mock<IConsumerService>();
            _tenantServiceMock = new Mock<ITenantService>();
            _tenantAccountServiceMock = new Mock<ITenantAccountService>();
            _walletClientMock = new Mock<IWalletClient>();

            _walletHelper = new WalletHelper(
                _configMock.Object,
                _loggerMock.Object,
                _consumerWalletServiceMock.Object,
                _consumerServiceMock.Object,
                _tenantServiceMock.Object,
                _tenantAccountServiceMock.Object,
                _walletClientMock.Object
            );

            _configMock.Setup(config => config.GetSection("Health_Actions_Reward_Wallet_Type_Code").Value).Returns("wat-11112222");
            _configMock.Setup(config => config.GetSection("Sweepstakes_Entries_Wallet_Type_Code").Value).Returns("wat-2222333");
            _configMock.Setup(config => config.GetSection("Health_Actions_Membership_Reward_Wallet_Type_Code").Value).Returns("wat-333444");
        }

        // ============================================================
        //  TEST 1: Consumer Not Found → Error 400
        // ============================================================

        [Fact]
        public async TaskAlias CreateWalletsForConsumer_Should_Return_400_When_Consumer_Not_Found()
        {
            // Arrange
            _consumerServiceMock
                .Setup(x => x.GetConsumerData(It.IsAny<GetConsumerRequestDto>()))
                .ReturnsAsync(new GetConsumerResponseDto { Consumer = null });

            // Act
            var result = await _walletHelper.CreateWalletsForConsumer("CON123");

            // Assert
            Assert.Equal(StatusCodes.Status400BadRequest, result.ErrorCode);
            Assert.Contains("Invalid tenant code", result.ErrorMessage);
        }

        // ============================================================
        //  TEST 2: Tenant Not Found → Error 400
        // ============================================================

        [Fact]
        public async TaskAlias CreateWalletsForConsumer_Should_Return_400_When_Invalid_Tenant()
        {
            // Arrange
            _consumerServiceMock
                .Setup(x => x.GetConsumerData(It.IsAny<GetConsumerRequestDto>()))
                .ReturnsAsync(new GetConsumerResponseDto
                {
                    Consumer = new ConsumerDto { ConsumerCode = "CON1", TenantCode = "TEN1" }
                });

            _tenantServiceMock
                .Setup(x => x.GetTenantDetails("TEN1"))
                .ReturnsAsync(new TenantResponseDto { ErrorCode = 400 });

            // Act
            var result = await _walletHelper.CreateWalletsForConsumer("CON1");

            // Assert
            Assert.Equal(StatusCodes.Status400BadRequest, result.ErrorCode);
        }

        // ============================================================
        //  TEST 3: Tenant Account Invalid → Error 400
        // ============================================================

        [Fact]
        public async TaskAlias CreateWalletsForConsumer_Should_Return_400_When_TenantAccount_NotFound()
        {
            // Arrange
            _consumerServiceMock
                .Setup(x => x.GetConsumerData(It.IsAny<GetConsumerRequestDto>()))
                .ReturnsAsync(new GetConsumerResponseDto
                {
                    Consumer = new ConsumerDto { ConsumerCode = "X1", TenantCode = "TEN1" }
                });

            _tenantServiceMock
                .Setup(x => x.GetTenantDetails("TEN1"))
                .ReturnsAsync(new TenantResponseDto
                {
                    Tenant = new TenantDto { TenantCode = "TEN1" }
                });

            _tenantAccountServiceMock
                .Setup(x => x.GetTenantAccount("TEN1"))
                .ReturnsAsync(new GetTenantAccountResponseDto { ErrorCode = 400 });

            // Act
            var result = await _walletHelper.CreateWalletsForConsumer("X1");

            // Assert
            Assert.Equal(StatusCodes.Status400BadRequest, result.ErrorCode);
        }

        // ============================================================
        //  TEST 4: Successful Execution → Success DTO
        // ============================================================

        [Fact]
        public async TaskAlias CreateWalletsForConsumer_Should_Return_Success_When_Wallets_Created()
        {
            // Arrange
            _consumerServiceMock
                .Setup(x => x.GetConsumerData(It.IsAny<GetConsumerRequestDto>()))
                .ReturnsAsync(new GetConsumerResponseDto
                {
                    Consumer = new ConsumerDto { ConsumerCode = "C111", TenantCode = "TEN1" }
                });

            _tenantServiceMock
                .Setup(x => x.GetTenantDetails("TEN1"))
                .ReturnsAsync(new TenantResponseDto
                {
                    Tenant = new TenantDto { TenantCode = "TEN1" , TenantOption = new TenantOption() { 
                        Apps = new List<string>() { "BENEFITS","REWARDS"}
                    }.ToJson() }
                });

            _tenantAccountServiceMock
                .Setup(x => x.GetTenantAccount("TEN1"))
                .ReturnsAsync(new GetTenantAccountResponseDto
                {
                    TenantAccount = new SunnyBenefits.Fis.Core.Domain.Dtos.TenantAccountRequestDto()
                    {
                        TenantConfigJson = new TenantConfigDto()
                        {
                            PurseConfig = new PurseConfigDto()
                            {
                                Purses = new List<PurseDto>
                                {
                                    new PurseDto { PurseLabel = "SJWallet2026" , WalletType = "wat-2222" 
                                    ,ActiveEndTs = new DateTime(2026, 12, 31) , ActiveStartTs = new DateTime(2026, 01, 01)
                                    , RedeemEndTs = new DateTime(2027, 12, 31) , PurseWalletType = "wat-1111"},
                                }
                            }
                        }.ToJson()
                    }
                });

            _walletClientMock
                .Setup(x => x.Post<WalletTypeDto>(It.IsAny<string>(), It.IsAny<WalletTypeDto>()))
                .ReturnsAsync(new WalletTypeDto() { WalletTypeId = 10 , WalletTypeCode = "wat-2222"});

            _consumerWalletServiceMock
                .Setup(x => x.GetAllConsumerWalletsAsync(It.IsAny<GetConsumerWalletRequestDto>()))
                .ReturnsAsync(new ConsumerWalletResponseDto());

            _consumerWalletServiceMock
                .Setup(x => x.PostConsumerWallets(It.IsAny<List<ConsumerWalletDataDto>>()))
                .ReturnsAsync(new List<ConsumerWalletDataResponseDto>());

            // Act
            var result = await _walletHelper.CreateWalletsForConsumer("C111");

            // Assert
            Assert.Null(result.ErrorCode); // success
        }

        // ============================================================
        //  TEST 5: Exception → Should Return 500
        // ============================================================

        [Fact]
        public async TaskAlias CreateWalletsForConsumer_Should_Return_500_On_Exception()
        {
            // Arrange
            _consumerServiceMock
                .Setup(x => x.GetConsumerData(It.IsAny<GetConsumerRequestDto>()))
                .ThrowsAsync(new Exception("Something failed"));

            // Act
            var result = await _walletHelper.CreateWalletsForConsumer("TEST");

            // Assert
            Assert.Equal(StatusCodes.Status500InternalServerError, result.ErrorCode);
            Assert.Contains("Internal error", result.ErrorMessage);
        }
    }
}
