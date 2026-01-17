using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using SunnyBenefits.Fis.Core.Domain.Dtos;
using SunnyBenefits.Fis.Core.Domain.Dtos.Json;
using SunnyRewards.Helios.Admin.Core.Domain.Dtos;
using SunnyRewards.Helios.Admin.Infrastructure.Exceptions;
using SunnyRewards.Helios.Admin.Infrastructure.Helpers;
using SunnyRewards.Helios.Admin.Infrastructure.Helpers.Interface;
using SunnyRewards.Helios.Admin.Infrastructure.HttpClients.Interfaces;
using SunnyRewards.Helios.Admin.Infrastructure.Services;
using SunnyRewards.Helios.Admin.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.Wallet.Core.Domain.Dtos;
using Xunit;
using Constant = SunnyRewards.Helios.Admin.Core.Domain.Constants.Constant;
using FundingConfigJson = SunnyBenefits.Fis.Core.Domain.Dtos.Json.FundingConfigJson;
using TaskAlias = System.Threading.Tasks.Task;
namespace SunnyRewards.Helios.Admin.UnitTest.Services
{
    public class OnBoardingInitialFundingServiceTests
    {
        private readonly Mock<ILogger<OnBoardingInitialFundingService>> _loggerMock;
        private readonly Mock<ITenantAccountService> _tenantAccountServiceMock;
        private readonly Mock<IConsumerAccountService> _consumerAccountServiceMock;
        private readonly Mock<IWalletClient> _walletClientMock;
        private readonly Mock<IFisClient> _fisClientMock;
        private readonly OnBoardingInitialFundingService _service;
        private readonly IConfiguration _configuration;
        private readonly IConsumerCohortHelper _consumerCohortHelper;

        public OnBoardingInitialFundingServiceTests()
        {
            _loggerMock = new Mock<ILogger<OnBoardingInitialFundingService>>();
            _tenantAccountServiceMock = new Mock<ITenantAccountService>();
            _consumerAccountServiceMock = new Mock<IConsumerAccountService>();
            _walletClientMock = new Mock<IWalletClient>();
            _fisClientMock = new Mock<IFisClient>();
            _consumerCohortHelper = new Mock<IConsumerCohortHelper>().Object;
            _configuration = new ConfigurationBuilder()
           .SetBasePath(Directory.GetCurrentDirectory())
           .AddJsonFile("appsettings.Development.json")
           .Build();
            _service = new OnBoardingInitialFundingService(
                _loggerMock.Object,
                _tenantAccountServiceMock.Object,
                _consumerAccountServiceMock.Object,
                _walletClientMock.Object,
                _fisClientMock.Object,
                _configuration,
                _consumerCohortHelper
            );
        }

        [Fact]
        public async TaskAlias ProcessInitialFundingAsync_ShouldReturnError_WhenValidationFails()
        {
            // Arrange
            var invalidRequest = new InitialFundingRequestDto
            {
                TenantCode = "tenant123",  
                ConsumerCode = null
            };

            // Act
            var result =  _service.ProcessInitialFundingAsync(invalidRequest);

            // Assert
            Assert.Equal(StatusCodes.Status400BadRequest, result.ErrorCode);
            Assert.Contains("Tenant code or consumer code is invalid", result.ErrorMessage);
        }

        [Fact]
        public async TaskAlias ProcessInitialFundingAsync_ShouldReturnError_WhenSelectedPursesNotAvailable()
        {
            // Arrange
            var invalidRequest = new InitialFundingRequestDto
            {
                TenantCode = "tenant123",
                ConsumerCode = null
            };

            // Act
            var result =  _service.ProcessInitialFundingAsync(invalidRequest);

            // Assert
            Assert.Equal(StatusCodes.Status400BadRequest, result.ErrorCode);
        }

        [Fact]
        public async TaskAlias ProcessInitialFundingAsync_ShouldReturnError_WhenTenantAccountReturnsError()
        {
            // Arrange
            var validRequest = new InitialFundingRequestDto
            {
                TenantCode = "tenant123",
                ConsumerCode = "consumer123",
                SelectedPurses = ["OTC", "REW"]
               
            };

            _tenantAccountServiceMock
                .Setup(service => service.GetTenantAccount(It.IsAny<string>()))
                .ReturnsAsync(new GetTenantAccountResponseDto()
                {
                    ErrorCode = StatusCodes.Status400BadRequest
                });

            // Act
            var result =  _service.ProcessInitialFundingAsync(validRequest);

            // Assert
            Assert.Equal(StatusCodes.Status400BadRequest, result.ErrorCode);
        }

        [Fact]
        public async TaskAlias ProcessInitialFundingAsync_ShouldReturnError_WhenTenantAccountNotFound()
        {
            // Arrange
            var validRequest = new InitialFundingRequestDto
            {
                TenantCode = "tenant123",
                ConsumerCode = "consumer123",
                SelectedPurses = ["OTC", "REW"]

            };

            _tenantAccountServiceMock
                .Setup(service => service.GetTenantAccount(It.IsAny<string>()))
                .ReturnsAsync(new GetTenantAccountResponseDto()
                {
                    TenantAccount = null
                });

            // Act
            var result =  _service.ProcessInitialFundingAsync(validRequest);

            // Assert
            Assert.Equal(StatusCodes.Status404NotFound, result.ErrorCode);
        }

        [Fact]
        public async TaskAlias ProcessInitialFundingAsync_ShouldReturnError_WhenConsumerAccountReturnsError()
        {
            // Arrange
            var validRequest = new InitialFundingRequestDto
            {
                TenantCode = "tenant123",
                ConsumerCode = "consumer123",
                SelectedPurses = ["OTC", "REW"]

            };

            _consumerAccountServiceMock
                 .Setup(service => service.GetConsumerAccount(It.IsAny<GetConsumerAccountRequestDto>()))
                 .ReturnsAsync(new GetConsumerAccountResponseDto()
                 {
                     ErrorCode = StatusCodes.Status404NotFound
                 });

            // Act
            var result =  _service.ProcessInitialFundingAsync(validRequest);

            // Assert
            Assert.Equal(StatusCodes.Status404NotFound, result.ErrorCode);
        }

        [Fact]
        public async TaskAlias ProcessInitialFundingAsync_ShouldReturnError_WhenConsumerAccountNotFound()
        {
            // Arrange
            var validRequest = new InitialFundingRequestDto
            {
                TenantCode = "tenant123",
                ConsumerCode = "consumer123",
                SelectedPurses = ["OTC", "REW"]

            };

            _consumerAccountServiceMock
                .Setup(service => service.GetConsumerAccount(It.IsAny<GetConsumerAccountRequestDto>()))
                .ReturnsAsync(new GetConsumerAccountResponseDto()
                {
                    ConsumerAccount = null
                });

            // Act
            var result =  _service.ProcessInitialFundingAsync(validRequest);

            // Assert
            Assert.Equal(StatusCodes.Status404NotFound, result.ErrorCode);
        }

        [Fact]
        public async TaskAlias ProcessInitialFundingAsync_ShouldReturnErrorResult_WhenFundingJsonNotFound()
        {
            // Arrange
            var validRequest = new InitialFundingRequestDto
            {
                TenantCode = "tenant123",
                ConsumerCode = "consumer123",
                SelectedPurses = new List<string> { "purse1" }
            };

            var tenantAccount = new GetTenantAccountResponseDto
            {
                TenantAccount = new TenantAccountRequestDto()
                {
                    FundingConfigJson = null
                }

            };

            var consumerAccount = new ConsumerAccountDto();

            _tenantAccountServiceMock
                .Setup(service => service.GetTenantAccount(It.IsAny<string>()))
                .ReturnsAsync(tenantAccount);

            _consumerAccountServiceMock
                .Setup(service => service.GetConsumerAccount(It.IsAny<GetConsumerAccountRequestDto>()))
                .ReturnsAsync(new GetConsumerAccountResponseDto
                {
                    ConsumerAccount = consumerAccount
                });

            _walletClientMock
                .Setup(client => client.Get<List<TenantWalletDetailDto>>(It.IsAny<string>(), It.IsAny<Dictionary<string, long>>()))
                .ReturnsAsync(new List<TenantWalletDetailDto>());

            // Act
            var result =  _service.ProcessInitialFundingAsync(validRequest);

            // Assert
            Assert.Equal(StatusCodes.Status404NotFound, result.ErrorCode);
        }

        [Fact]
        public async TaskAlias ProcessInitialFundingAsync_ShouldReturnErrorResult_WhenFundingRulesNotFound()
        {
            // Arrange
            var validRequest = new InitialFundingRequestDto
            {
                TenantCode = "tenant123",
                ConsumerCode = "consumer123",
                SelectedPurses = new List<string> { "purse1" }
            };

            var tenantAccount = new GetTenantAccountResponseDto
            {
                TenantAccount = new TenantAccountRequestDto()
                {
                    FundingConfigJson = JsonConvert.SerializeObject(new FundingConfigJson { FundingRules = new List<FundingRule>() })
                }

            };

            var consumerAccount = new ConsumerAccountDto();

            _tenantAccountServiceMock
                .Setup(service => service.GetTenantAccount(It.IsAny<string>()))
                .ReturnsAsync(tenantAccount);

            _consumerAccountServiceMock
                .Setup(service => service.GetConsumerAccount(It.IsAny<GetConsumerAccountRequestDto>()))
                .ReturnsAsync(new GetConsumerAccountResponseDto
                {
                    ConsumerAccount = consumerAccount
                });

            _walletClientMock
                .Setup(client => client.Get<List<TenantWalletDetailDto>>(It.IsAny<string>(), It.IsAny<Dictionary<string, long>>()))
                .ReturnsAsync(new List<TenantWalletDetailDto>());

            // Act
            var result =  _service.ProcessInitialFundingAsync(validRequest);

            // Assert
            Assert.Equal(StatusCodes.Status404NotFound, result.ErrorCode);
        }

        [Fact]
        public async TaskAlias ProcessInitialFundingAsync_ShouldReturnErrorResult_WhenOnBoardingRuleNotFund()
        {
            // Arrange
            var validRequest = new InitialFundingRequestDto
            {
                TenantCode = "tenant123",
                ConsumerCode = "consumer123",
                SelectedPurses = new List<string> { "purse1" }
            };

            var tenantAccount = new GetTenantAccountResponseDto
            {
                TenantAccount = new TenantAccountRequestDto()
                {
                    FundingConfigJson = JsonConvert.SerializeObject(new FundingConfigJson { FundingRules = new List<FundingRule>() {
                        new FundingRule()
                        {
                            Amount = 10,
                            ConsumerWalletType = "Test-ConsumerWalletType",
                            MasterWalletType = "Test-MasterWalletType",
                            Enabled = true,
                            RuleNumber = 3,
                            RuleDescription = "Test-RuleDescription",
                            RecurrenceType = "PERIOD",
                            EffectiveEndDate = DateTime.UtcNow.AddDays(10).ToString(),
                            EffectiveStartDate = DateTime.UtcNow.AddDays(-10).ToString(),
                        }
                    } })
                }
                
            };

            var consumerAccount = new ConsumerAccountDto();

            _tenantAccountServiceMock
                .Setup(service => service.GetTenantAccount(It.IsAny<string>()))
                .ReturnsAsync(tenantAccount);

            _consumerAccountServiceMock
                .Setup(service => service.GetConsumerAccount(It.IsAny<GetConsumerAccountRequestDto>()))
                .ReturnsAsync(new GetConsumerAccountResponseDto
                {
                    ConsumerAccount = consumerAccount
                });

            _walletClientMock
                .Setup(client => client.Get<List<TenantWalletDetailDto>>(It.IsAny<string>(), It.IsAny<Dictionary<string, long>>()))
                .ReturnsAsync(new List<TenantWalletDetailDto>());

            // Act
            var result =  _service.ProcessInitialFundingAsync(validRequest);

            // Assert
            Assert.Equal(StatusCodes.Status404NotFound, result.ErrorCode);
        }

        [Fact]
        public async TaskAlias ProcessInitialFundingAsync_ShouldReturnError_WhenMasterWalletReturnError()
        {
            // Arrange
            var validRequest = new InitialFundingRequestDto
            {
                TenantCode = "tenant123",
                ConsumerCode = "consumer123",
                SelectedPurses = new List<string> { "purse1" }
            };

            var tenantAccount = new GetTenantAccountResponseDto
            {
                TenantAccount = new TenantAccountRequestDto()
                {
                    FundingConfigJson = JsonConvert.SerializeObject(new FundingConfigJson
                    {
                        FundingRules = new List<FundingRule>() {
                        new FundingRule()
                        {
                            Amount = 10,
                            ConsumerWalletType = "Test-ConsumerWalletType",
                            MasterWalletType = "Test-MasterWalletType",
                            Enabled = true,
                            RuleNumber = 3,
                            RuleDescription = "Test-RuleDescription",
                            RecurrenceType = "ONBOARDING",
                             EffectiveEndDate = DateTime.UtcNow.AddDays(10).ToString(),
                            EffectiveStartDate = DateTime.UtcNow.AddDays(-10).ToString()
                        }
                    }
                    })
                }

            };

            var consumerAccount = new ConsumerAccountDto();

            _tenantAccountServiceMock
                .Setup(service => service.GetTenantAccount(It.IsAny<string>()))
                .ReturnsAsync(tenantAccount);

            _consumerAccountServiceMock
                .Setup(service => service.GetConsumerAccount(It.IsAny<GetConsumerAccountRequestDto>()))
                .ReturnsAsync(new GetConsumerAccountResponseDto
                {
                    ConsumerAccount = consumerAccount
                });

            _walletClientMock
                .Setup(client => client.Get<List<TenantWalletDetailDto>>(It.IsAny<string>(), It.IsAny<Dictionary<string, long>>()))
                .ReturnsAsync(new List<TenantWalletDetailDto>());

            _walletClientMock
                .Setup(client => client.Get<GetAllMasterWalletsResponseDto>(It.IsAny<string>(), It.IsAny<Dictionary<string, long>>()))
                .ReturnsAsync(new GetAllMasterWalletsResponseDto
                {
                    ErrorCode = StatusCodes.Status400BadRequest
                });

            // Act
            var result =  _service.ProcessInitialFundingAsync(validRequest);

            // Assert
            Assert.Equal(StatusCodes.Status400BadRequest, result.ErrorCode);
        }

        [Fact]
        public async TaskAlias ProcessInitialFundingAsync_ShouldReturnError_WhenMasterWalletNotFound()
        {
            // Arrange
            var validRequest = new InitialFundingRequestDto
            {
                TenantCode = "tenant123",
                ConsumerCode = "consumer123",
                SelectedPurses = new List<string> { "purse1" }
            };

            var tenantAccount = new GetTenantAccountResponseDto
            {
                TenantAccount = new TenantAccountRequestDto()
                {
                    FundingConfigJson = JsonConvert.SerializeObject(new FundingConfigJson
                    {
                        FundingRules = new List<FundingRule>() {
                        new FundingRule()
                        {
                            Amount = 10,
                            ConsumerWalletType = "Test-ConsumerWalletType",
                            MasterWalletType = "Test-MasterWalletType",
                            Enabled = true,
                            RuleNumber = 3,
                            RuleDescription = "Test-RuleDescription",
                            RecurrenceType = "ONBOARDING",
                             EffectiveEndDate = DateTime.UtcNow.AddDays(10).ToString(),
                            EffectiveStartDate = DateTime.UtcNow.AddDays(-10).ToString(),
                        }
                    }
                    })
                }

            };

            var consumerAccount = new ConsumerAccountDto();

            _tenantAccountServiceMock
                .Setup(service => service.GetTenantAccount(It.IsAny<string>()))
                .ReturnsAsync(tenantAccount);

            _consumerAccountServiceMock
                .Setup(service => service.GetConsumerAccount(It.IsAny<GetConsumerAccountRequestDto>()))
                .ReturnsAsync(new GetConsumerAccountResponseDto
                {
                    ConsumerAccount = consumerAccount
                });

            _walletClientMock
                .Setup(client => client.Get<List<TenantWalletDetailDto>>(It.IsAny<string>(), It.IsAny<Dictionary<string, long>>()))
                .ReturnsAsync(new List<TenantWalletDetailDto>());

            _walletClientMock
                .Setup(client => client.Get<GetAllMasterWalletsResponseDto>(It.IsAny<string>(), It.IsAny<Dictionary<string, long>>()))
                .ReturnsAsync(new GetAllMasterWalletsResponseDto
                {
                    MasterWallets = null
                });

            // Act
            var result =  _service.ProcessInitialFundingAsync(validRequest);

            // Assert
            Assert.Equal(StatusCodes.Status404NotFound, result.ErrorCode);
        }

        [Fact]
        public async TaskAlias ProcessInitialFundingAsync_ShouldReturnError_WhenTenantConfigJsonNull()
        {
            // Arrange
            var validRequest = new InitialFundingRequestDto
            {
                TenantCode = "tenant123",
                ConsumerCode = "consumer123",
                SelectedPurses = new List<string> { "purse1" }
            };

            var tenantAccount = new GetTenantAccountResponseDto
            {
                TenantAccount = new TenantAccountRequestDto()
                {
                    FundingConfigJson = JsonConvert.SerializeObject(new FundingConfigJson
                    {
                        FundingRules = new List<FundingRule>() {
                        new FundingRule()
                        {
                            Amount = 10,
                            ConsumerWalletType = "Test-ConsumerWalletType",
                            MasterWalletType = "Test-MasterWalletType",
                            Enabled = true,
                            RuleNumber = 3,
                            RuleDescription = "Test-RuleDescription",
                            RecurrenceType = "ONBOARDING",
                             EffectiveEndDate = DateTime.UtcNow.AddDays(10).ToString(),
                            EffectiveStartDate = DateTime.UtcNow.AddDays(-10).ToString(),
                        }
                    }
                    }),
                    TenantConfigJson = null
                }

            };

            var consumerAccount = new ConsumerAccountDto();

            _tenantAccountServiceMock
                .Setup(service => service.GetTenantAccount(It.IsAny<string>()))
                .ReturnsAsync(tenantAccount);

            _consumerAccountServiceMock
                .Setup(service => service.GetConsumerAccount(It.IsAny<GetConsumerAccountRequestDto>()))
                .ReturnsAsync(new GetConsumerAccountResponseDto
                {
                    ConsumerAccount = consumerAccount
                });

            _walletClientMock
                .Setup(client => client.Get<List<TenantWalletDetailDto>>(It.IsAny<string>(), It.IsAny<Dictionary<string, long>>()))
                .ReturnsAsync(new List<TenantWalletDetailDto>());

            _walletClientMock
                .Setup(client => client.Get<GetAllMasterWalletsResponseDto>(It.IsAny<string>(), It.IsAny<Dictionary<string, long>>()))
                .ReturnsAsync(new GetAllMasterWalletsResponseDto
                {
                    MasterWallets = new List<TenantWalletDetailDto>()
                    {
                        new TenantWalletDetailDto()
                        {
                            Wallet = new WalletDto(),
                            WalletType = new WalletTypeDto()
                        }
                    }
                });

            // Act
            var result =  _service.ProcessInitialFundingAsync(validRequest);

            // Assert
            Assert.Equal(500,result.ErrorCode);
            Assert.Contains("Purse configuration not found for", result.ErrorMessage);
        }

        [Fact]
        public async TaskAlias ProcessInitialFundingAsync_ShouldReturnError_FundingRuleNotFound()
        {
            // Arrange
            var validRequest = new InitialFundingRequestDto
            {
                TenantCode = "tenant123",
                ConsumerCode = "consumer123",
                SelectedPurses = new List<string> { "OTC" }
            };

            var tenantAccount = new GetTenantAccountResponseDto
            {
                TenantAccount = new TenantAccountRequestDto()
                {
                    FundingConfigJson = JsonConvert.SerializeObject(new FundingConfigJson
                    {
                        FundingRules = new List<FundingRule>() {
                        new FundingRule()
                        {
                            Amount = 10,
                            ConsumerWalletType = "Test-ConsumerWalletType",
                            MasterWalletType = "Test-MasterWalletType",
                            Enabled = true,
                            RuleNumber = 3,
                            RuleDescription = "Test-RuleDescription",
                            RecurrenceType = "ONBOARDING",
                             EffectiveEndDate = DateTime.UtcNow.AddDays(10).ToString(),
                            EffectiveStartDate = DateTime.UtcNow.AddDays(-10).ToString(),
                        }
                    }
                    }),
                    TenantConfigJson = "{\"purseConfig\":{\"purses\":[{\"purseLabel\":\"OTC\",\"walletType\":\"wat-4b364fg722f04034cv732b355d84f479\",\"purseNumber\":12401,\"periodConfig\":{\"fundDate\":19,\"interval\":\"MONTH\",\"applyDateConfig\":{\"applyDate\":1,\"applyDateType\":\"NEXT_MONTH\"}},\"purseWalletType\":\"wat-4b364ed612f04034bf732b355d84f368\",\"masterWalletType\":\"wat-4b364fg722f04034cv732b355d84f479\",\"masterRedemptionWalletType\":\"wat-bc8f4f7c028d479f900f0af794e385c8\"},{\"purseLabel\":\"FOD\",\"walletType\":\"wat-35d4rt62d77y6119a6t730c9b6447457\",\"purseNumber\":22401,\"periodConfig\":{\"fundDate\":19,\"interval\":\"MONTH\",\"applyDateConfig\":{\"applyDate\":1,\"applyDateType\":\"NEXT_MONTH\"}},\"purseWalletType\":\"wat-35b3ac62d74b4119a5f630c9b6446035\",\"masterWalletType\":\"wat-35d4rt62d77y6119a6t730c9b6447457\",\"masterRedemptionWalletType\":\"wat-01b4a568c1814449b64168bb434127b7\"},{\"purseLabel\":\"REW\",\"walletType\":\"wat-2d62dcaf2aa4424b9ff6c2ddb5895077\",\"purseNumber\":32401,\"periodConfig\":{\"fundDate\":19,\"interval\":\"MONTH\",\"applyDateConfig\":{\"applyDate\":1,\"applyDateType\":\"NEXT_MONTH\"}},\"purseWalletType\":\"wat-55f05107b41642c39e7a3b459223cbd8\",\"masterWalletType\":\"wat-2d62dcaf2aa4424b9ff6c2ddb5895077\",\"masterRedemptionWalletType\":\"wat-274bd71345804f09928cf451dc0f6239\"}]},\"fisProgramDetail\":{\"clientId\":\"1234500\",\"companyId\":\"1204185\",\"packageId\":\"721736\",\"subprogramId\":\"873343\"}}"
                }

            };

            var consumerAccount = new ConsumerAccountDto();

            _tenantAccountServiceMock
                .Setup(service => service.GetTenantAccount(It.IsAny<string>()))
                .ReturnsAsync(tenantAccount);

            _consumerAccountServiceMock
                .Setup(service => service.GetConsumerAccount(It.IsAny<GetConsumerAccountRequestDto>()))
                .ReturnsAsync(new GetConsumerAccountResponseDto
                {
                    ConsumerAccount = consumerAccount
                });

            _walletClientMock
                .Setup(client => client.Get<List<TenantWalletDetailDto>>(It.IsAny<string>(), It.IsAny<Dictionary<string, long>>()))
                .ReturnsAsync(new List<TenantWalletDetailDto>());

            _walletClientMock
                .Setup(client => client.Get<GetAllMasterWalletsResponseDto>(It.IsAny<string>(), It.IsAny<Dictionary<string, long>>()))
                .ReturnsAsync(new GetAllMasterWalletsResponseDto
                {
                    MasterWallets = new List<TenantWalletDetailDto>()
                    {
                        new TenantWalletDetailDto()
                        {
                            Wallet = new WalletDto(),
                            WalletType = new WalletTypeDto()
                        }
                    }
                });

            // Act
            var result =  _service.ProcessInitialFundingAsync(validRequest);

            // Assert
            Assert.Equal(500, result.ErrorCode);
            Assert.Contains("No funding rule found for wallet type", result.ErrorMessage);
        }

        [Fact]
        public async TaskAlias ProcessInitialFundingAsync_ShouldReturnError_WhenRedumptionCodeNotFound()
        {
            // Arrange
            var validRequest = new InitialFundingRequestDto
            {
                TenantCode = "tenant123",
                ConsumerCode = "consumer123",
                SelectedPurses = new List<string> { "OTC" }
            };

            var tenantAccount = new GetTenantAccountResponseDto
            {
                TenantAccount = new TenantAccountRequestDto()
                {
                    FundingConfigJson = JsonConvert.SerializeObject(new FundingConfigJson
                    {
                        FundingRules = new List<FundingRule>() {
                        new FundingRule()
                        {
                            Amount = 100,
                            ConsumerWalletType = "wat-4b364fg722f04034cv732b355d84f479",
                            MasterWalletType = "wat-4b364fg722f04034cv732b355d84f479",
                            Enabled = true,
                            RuleNumber = 3,
                            RuleDescription = "Upon onboarding, deposit $100 into the FOD Wallet",
                            RecurrenceType = "ONBOARDING",
                             EffectiveEndDate = DateTime.UtcNow.AddDays(10).ToString(),
                            EffectiveStartDate = DateTime.UtcNow.AddDays(-10).ToString(),
                        }
                    }
                    }),
                    TenantConfigJson = "{\"purseConfig\":{\"purses\":[{\"purseLabel\":\"OTC\",\"walletType\":\"wat-4b364fg722f04034cv732b355d84f479\",\"purseNumber\":12401,\"periodConfig\":{\"fundDate\":19,\"interval\":\"MONTH\",\"applyDateConfig\":{\"applyDate\":1,\"applyDateType\":\"NEXT_MONTH\"}},\"purseWalletType\":\"wat-4b364ed612f04034bf732b355d84f368\",\"masterWalletType\":\"wat-4b364fg722f04034cv732b355d84f479\",\"masterRedemptionWalletType\":\"wat-bc8f4f7c028d479f900f0af794e385c8\"},{\"purseLabel\":\"FOD\",\"walletType\":\"wat-35d4rt62d77y6119a6t730c9b6447457\",\"purseNumber\":22401,\"periodConfig\":{\"fundDate\":19,\"interval\":\"MONTH\",\"applyDateConfig\":{\"applyDate\":1,\"applyDateType\":\"NEXT_MONTH\"}},\"purseWalletType\":\"wat-35b3ac62d74b4119a5f630c9b6446035\",\"masterWalletType\":\"wat-35d4rt62d77y6119a6t730c9b6447457\",\"masterRedemptionWalletType\":\"wat-01b4a568c1814449b64168bb434127b7\"},{\"purseLabel\":\"REW\",\"walletType\":\"wat-2d62dcaf2aa4424b9ff6c2ddb5895077\",\"purseNumber\":32401,\"periodConfig\":{\"fundDate\":19,\"interval\":\"MONTH\",\"applyDateConfig\":{\"applyDate\":1,\"applyDateType\":\"NEXT_MONTH\"}},\"purseWalletType\":\"wat-55f05107b41642c39e7a3b459223cbd8\",\"masterWalletType\":\"wat-2d62dcaf2aa4424b9ff6c2ddb5895077\",\"masterRedemptionWalletType\":\"wat-274bd71345804f09928cf451dc0f6239\"}]},\"fisProgramDetail\":{\"clientId\":\"1234500\",\"companyId\":\"1204185\",\"packageId\":\"721736\",\"subprogramId\":\"873343\"}}"
                }

            };

            var consumerAccount = new ConsumerAccountDto();

            _tenantAccountServiceMock
                .Setup(service => service.GetTenantAccount(It.IsAny<string>()))
                .ReturnsAsync(tenantAccount);

            _consumerAccountServiceMock
                .Setup(service => service.GetConsumerAccount(It.IsAny<GetConsumerAccountRequestDto>()))
                .ReturnsAsync(new GetConsumerAccountResponseDto
                {
                    ConsumerAccount = consumerAccount
                });

            _walletClientMock
                .Setup(client => client.Get<List<TenantWalletDetailDto>>(It.IsAny<string>(), It.IsAny<Dictionary<string, long>>()))
                .ReturnsAsync(new List<TenantWalletDetailDto>());

            _walletClientMock
                .Setup(client => client.Get<GetAllMasterWalletsResponseDto>(It.IsAny<string>(), It.IsAny<Dictionary<string, long>>()))
                .ReturnsAsync(new GetAllMasterWalletsResponseDto
                {
                    MasterWallets = new List<TenantWalletDetailDto>()
                    {
                        new TenantWalletDetailDto()
                        {
                            Wallet = new WalletDto(),
                            WalletType = new WalletTypeDto()
                        }
                    }
                });

            // Act
            var result =  _service.ProcessInitialFundingAsync(validRequest);

            // Assert
            Assert.Equal(500, result.ErrorCode);
            Assert.Contains("Redemption vendor code not found for wallet type", result.ErrorMessage);
        }

        [Fact]
        public async TaskAlias ProcessInitialFundingAsync_ShouldReturnSuccess_WhenFundingHistoryReturnsError()
        {
            // Arrange
            var validRequest = new InitialFundingRequestDto
            {
                TenantCode = "tenant123",
                ConsumerCode = "consumer123",
                SelectedPurses = new List<string> { "OTC" }
            };

            var tenantAccount = new GetTenantAccountResponseDto
            {
                TenantAccount = new TenantAccountRequestDto()
                {
                    FundingConfigJson = JsonConvert.SerializeObject(new FundingConfigJson
                    {
                        FundingRules = new List<FundingRule>() {
                        new FundingRule()
                        {
                            Amount = 100,
                            ConsumerWalletType = "wat-4b364fg722f04034cv732b355d84f479",
                            MasterWalletType = "wat-4b364fg722f04034cv732b355d84f479",
                            Enabled = true,
                            RuleNumber = 3,
                            RuleDescription = "Upon onboarding, deposit $100 into the FOD Wallet",
                            RecurrenceType = "ONBOARDING",
                             EffectiveEndDate = DateTime.UtcNow.AddDays(10).ToString(),
                            EffectiveStartDate = DateTime.UtcNow.AddDays(-10).ToString(),
                        }
                    }
                    }),
                    TenantConfigJson = "{\"purseConfig\":{\"purses\":[{\"purseLabel\":\"OTC\",\"walletType\":\"wat-4b364fg722f04034cv732b355d84f479\",\"purseNumber\":12401,\"periodConfig\":{\"fundDate\":19,\"interval\":\"MONTH\",\"applyDateConfig\":{\"applyDate\":1,\"applyDateType\":\"NEXT_MONTH\"}},\"purseWalletType\":\"wat-4b364ed612f04034bf732b355d84f368\",\"masterWalletType\":\"wat-4b364fg722f04034cv732b355d84f479\",\"masterRedemptionWalletType\":\"wat-bc8f4f7c028d479f900f0af794e385c8\"},{\"purseLabel\":\"FOD\",\"walletType\":\"wat-35d4rt62d77y6119a6t730c9b6447457\",\"purseNumber\":22401,\"periodConfig\":{\"fundDate\":19,\"interval\":\"MONTH\",\"applyDateConfig\":{\"applyDate\":1,\"applyDateType\":\"NEXT_MONTH\"}},\"purseWalletType\":\"wat-35b3ac62d74b4119a5f630c9b6446035\",\"masterWalletType\":\"wat-35d4rt62d77y6119a6t730c9b6447457\",\"masterRedemptionWalletType\":\"wat-01b4a568c1814449b64168bb434127b7\"},{\"purseLabel\":\"REW\",\"walletType\":\"wat-2d62dcaf2aa4424b9ff6c2ddb5895077\",\"purseNumber\":32401,\"periodConfig\":{\"fundDate\":19,\"interval\":\"MONTH\",\"applyDateConfig\":{\"applyDate\":1,\"applyDateType\":\"NEXT_MONTH\"}},\"purseWalletType\":\"wat-55f05107b41642c39e7a3b459223cbd8\",\"masterWalletType\":\"wat-2d62dcaf2aa4424b9ff6c2ddb5895077\",\"masterRedemptionWalletType\":\"wat-274bd71345804f09928cf451dc0f6239\"}]},\"fisProgramDetail\":{\"clientId\":\"1234500\",\"companyId\":\"1204185\",\"packageId\":\"721736\",\"subprogramId\":\"873343\"}}"
                }

            };

            var consumerAccount = new ConsumerAccountDto();

            _tenantAccountServiceMock
                .Setup(service => service.GetTenantAccount(It.IsAny<string>()))
                .ReturnsAsync(tenantAccount);

            _consumerAccountServiceMock
                .Setup(service => service.GetConsumerAccount(It.IsAny<GetConsumerAccountRequestDto>()))
                .ReturnsAsync(new GetConsumerAccountResponseDto
                {
                    ConsumerAccount = consumerAccount
                });

            _walletClientMock
                .Setup(client => client.Get<List<TenantWalletDetailDto>>(It.IsAny<string>(), It.IsAny<Dictionary<string, long>>()))
                .ReturnsAsync(new List<TenantWalletDetailDto>());

            _walletClientMock
                .Setup(client => client.Get<GetAllMasterWalletsResponseDto>(It.IsAny<string>(), It.IsAny<Dictionary<string, long>>()))
                .ReturnsAsync(new GetAllMasterWalletsResponseDto
                {
                    MasterWallets = new List<TenantWalletDetailDto>()
                    {
                        new TenantWalletDetailDto()
                        {
                            Wallet = new WalletDto()
                            {
                                WalletName = "TENANT_MASTER_REDEMPTION:SUSPENSE_WALLET_OTC"

                            },
                            WalletType = new WalletTypeDto()
                            {
                                WalletTypeCode = "wat-bc8f4f7c028d479f900f0af794e385c8"
                            }
                        }
                    }
                });

            _fisClientMock
               .Setup(client => client.Get<FundingHistoryResponseDto>(It.IsAny<string>(), It.IsAny<Dictionary<string, long>>()))
               .ReturnsAsync(new FundingHistoryResponseDto()
               {
                   ErrorCode = StatusCodes.Status400BadRequest
               });

            _walletClientMock
               .Setup(client => client.Post<PurseFundingResponseDto>(It.IsAny<string>(), It.IsAny<PurseFundingRequestDto>()))
               .ReturnsAsync(new PurseFundingResponseDto()
               {
                   ErrorCode = StatusCodes.Status400BadRequest
               });

            // Act
            var result =  _service.ProcessInitialFundingAsync(validRequest);

            // Assert
            // Assert
            Assert.Equal(500, result.ErrorCode);
            Assert.Contains("Funding failed for RuleNumber", result.ErrorMessage);
        }

        [Fact]
        public async TaskAlias ProcessInitialFundingAsync_ShouldReturn500_WhenFundingHistoryExist()
        {
            // Arrange
            var validRequest = new InitialFundingRequestDto
            {
                TenantCode = "tenant123",
                ConsumerCode = "consumer123",
                SelectedPurses = new List<string> { "OTC" }
            };

            var tenantAccount = new GetTenantAccountResponseDto
            {
                TenantAccount = new TenantAccountRequestDto()
                {
                    FundingConfigJson = JsonConvert.SerializeObject(new FundingConfigJson
                    {
                        FundingRules = new List<FundingRule>() {
                        new FundingRule()
                        {
                            Amount = 100,
                            ConsumerWalletType = "wat-4b364fg722f04034cv732b355d84f479",
                            MasterWalletType = "wat-4b364fg722f04034cv732b355d84f479",
                            Enabled = true,
                            RuleNumber = 3,
                            RuleDescription = "Upon onboarding, deposit $100 into the FOD Wallet",
                            RecurrenceType = "ONBOARDING",
                             EffectiveEndDate = DateTime.UtcNow.AddDays(10).ToString(),
                            EffectiveStartDate = DateTime.UtcNow.AddDays(-10).ToString(),
                        }
                    }
                    }),
                    TenantConfigJson = "{\"purseConfig\":{\"purses\":[{\"purseLabel\":\"OTC\",\"walletType\":\"wat-4b364fg722f04034cv732b355d84f479\",\"purseNumber\":12401,\"periodConfig\":{\"fundDate\":19,\"interval\":\"MONTH\",\"applyDateConfig\":{\"applyDate\":1,\"applyDateType\":\"NEXT_MONTH\"}},\"purseWalletType\":\"wat-4b364ed612f04034bf732b355d84f368\",\"masterWalletType\":\"wat-4b364fg722f04034cv732b355d84f479\",\"masterRedemptionWalletType\":\"wat-bc8f4f7c028d479f900f0af794e385c8\"},{\"purseLabel\":\"FOD\",\"walletType\":\"wat-35d4rt62d77y6119a6t730c9b6447457\",\"purseNumber\":22401,\"periodConfig\":{\"fundDate\":19,\"interval\":\"MONTH\",\"applyDateConfig\":{\"applyDate\":1,\"applyDateType\":\"NEXT_MONTH\"}},\"purseWalletType\":\"wat-35b3ac62d74b4119a5f630c9b6446035\",\"masterWalletType\":\"wat-35d4rt62d77y6119a6t730c9b6447457\",\"masterRedemptionWalletType\":\"wat-01b4a568c1814449b64168bb434127b7\"},{\"purseLabel\":\"REW\",\"walletType\":\"wat-2d62dcaf2aa4424b9ff6c2ddb5895077\",\"purseNumber\":32401,\"periodConfig\":{\"fundDate\":19,\"interval\":\"MONTH\",\"applyDateConfig\":{\"applyDate\":1,\"applyDateType\":\"NEXT_MONTH\"}},\"purseWalletType\":\"wat-55f05107b41642c39e7a3b459223cbd8\",\"masterWalletType\":\"wat-2d62dcaf2aa4424b9ff6c2ddb5895077\",\"masterRedemptionWalletType\":\"wat-274bd71345804f09928cf451dc0f6239\"}]},\"fisProgramDetail\":{\"clientId\":\"1234500\",\"companyId\":\"1204185\",\"packageId\":\"721736\",\"subprogramId\":\"873343\"}}"
                }

            };

            var consumerAccount = new ConsumerAccountDto();

            _tenantAccountServiceMock
                .Setup(service => service.GetTenantAccount(It.IsAny<string>()))
                .ReturnsAsync(tenantAccount);

            _consumerAccountServiceMock
                .Setup(service => service.GetConsumerAccount(It.IsAny<GetConsumerAccountRequestDto>()))
                .ReturnsAsync(new GetConsumerAccountResponseDto
                {
                    ConsumerAccount = consumerAccount
                });

            _walletClientMock
                .Setup(client => client.Get<List<TenantWalletDetailDto>>(It.IsAny<string>(), It.IsAny<Dictionary<string, long>>()))
                .ReturnsAsync(new List<TenantWalletDetailDto>());

            _walletClientMock
                .Setup(client => client.Get<GetAllMasterWalletsResponseDto>(It.IsAny<string>(), It.IsAny<Dictionary<string, long>>()))
                .ReturnsAsync(new GetAllMasterWalletsResponseDto
                {
                    MasterWallets = new List<TenantWalletDetailDto>()
                    {
                        new TenantWalletDetailDto()
                        {
                            Wallet = new WalletDto()
                            {
                                WalletName = "TENANT_MASTER_REDEMPTION:SUSPENSE_WALLET_OTC"

                            },
                            WalletType = new WalletTypeDto()
                            {
                                WalletTypeCode = "wat-bc8f4f7c028d479f900f0af794e385c8"
                            }
                        }
                    }
                });

            _fisClientMock
               .Setup(client => client.Get<FundingHistoryResponseDto>(It.IsAny<string>(), It.IsAny<Dictionary<string, long>>()))
               .ReturnsAsync(new FundingHistoryResponseDto()
               {
                   FundingHistoryList = new List<FundingHistoryDto>()
                   {
                       new FundingHistoryDto()
                       {
                           TenantCode = "tenant123",
                           ConsumerCode = "consumer123",
                           FundRuleNumber = 3
                       }
                   }
               });

            _walletClientMock
               .Setup(client => client.Post<PurseFundingResponseDto>(It.IsAny<string>(), It.IsAny<PurseFundingRequestDto>()))
               .ReturnsAsync(new PurseFundingResponseDto()
               {
                   ErrorCode = StatusCodes.Status400BadRequest
               });

            // Act
            var result =  _service.ProcessInitialFundingAsync(validRequest);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(500, result.ErrorCode);
            Assert.Contains("Funding already exists for TenantCode", result.ErrorMessage);

        }

        [Fact]
        public async TaskAlias ProcessInitialFundingAsync_ShouldReturn500_WhenRedeemStartFail()
        {
            // Arrange
            var validRequest = new InitialFundingRequestDto
            {
                TenantCode = "tenant123",
                ConsumerCode = "consumer123",
                SelectedPurses = new List<string> { "OTC" }
            };

            var tenantAccount = new GetTenantAccountResponseDto
            {
                TenantAccount = new TenantAccountRequestDto()
                {
                    FundingConfigJson = JsonConvert.SerializeObject(new FundingConfigJson
                    {
                        FundingRules = new List<FundingRule>() {
                        new FundingRule()
                        {
                            Amount = 100,
                            ConsumerWalletType = "wat-4b364fg722f04034cv732b355d84f479",
                            MasterWalletType = "wat-4b364fg722f04034cv732b355d84f479",
                            Enabled = true,
                            RuleNumber = 3,
                            RuleDescription = "Upon onboarding, deposit $100 into the FOD Wallet",
                            RecurrenceType = "ONBOARDING",
                             EffectiveEndDate = DateTime.UtcNow.AddDays(10).ToString(),
                            EffectiveStartDate = DateTime.UtcNow.AddDays(-10).ToString(),
                        }
                    }
                    }),
                    TenantConfigJson = "{\"purseConfig\":{\"purses\":[{\"purseLabel\":\"OTC\",\"walletType\":\"wat-4b364fg722f04034cv732b355d84f479\",\"purseNumber\":12401,\"periodConfig\":{\"fundDate\":19,\"interval\":\"MONTH\",\"applyDateConfig\":{\"applyDate\":1,\"applyDateType\":\"NEXT_MONTH\"}},\"purseWalletType\":\"wat-4b364ed612f04034bf732b355d84f368\",\"masterWalletType\":\"wat-4b364fg722f04034cv732b355d84f479\",\"masterRedemptionWalletType\":\"wat-bc8f4f7c028d479f900f0af794e385c8\"},{\"purseLabel\":\"FOD\",\"walletType\":\"wat-35d4rt62d77y6119a6t730c9b6447457\",\"purseNumber\":22401,\"periodConfig\":{\"fundDate\":19,\"interval\":\"MONTH\",\"applyDateConfig\":{\"applyDate\":1,\"applyDateType\":\"NEXT_MONTH\"}},\"purseWalletType\":\"wat-35b3ac62d74b4119a5f630c9b6446035\",\"masterWalletType\":\"wat-35d4rt62d77y6119a6t730c9b6447457\",\"masterRedemptionWalletType\":\"wat-01b4a568c1814449b64168bb434127b7\"},{\"purseLabel\":\"REW\",\"walletType\":\"wat-2d62dcaf2aa4424b9ff6c2ddb5895077\",\"purseNumber\":32401,\"periodConfig\":{\"fundDate\":19,\"interval\":\"MONTH\",\"applyDateConfig\":{\"applyDate\":1,\"applyDateType\":\"NEXT_MONTH\"}},\"purseWalletType\":\"wat-55f05107b41642c39e7a3b459223cbd8\",\"masterWalletType\":\"wat-2d62dcaf2aa4424b9ff6c2ddb5895077\",\"masterRedemptionWalletType\":\"wat-274bd71345804f09928cf451dc0f6239\"}]},\"fisProgramDetail\":{\"clientId\":\"1234500\",\"companyId\":\"1204185\",\"packageId\":\"721736\",\"subprogramId\":\"873343\"}}"
                }

            };

            var consumerAccount = new ConsumerAccountDto();

            _tenantAccountServiceMock
                .Setup(service => service.GetTenantAccount(It.IsAny<string>()))
                .ReturnsAsync(tenantAccount);

            _consumerAccountServiceMock
                .Setup(service => service.GetConsumerAccount(It.IsAny<GetConsumerAccountRequestDto>()))
                .ReturnsAsync(new GetConsumerAccountResponseDto
                {
                    ConsumerAccount = consumerAccount
                });

            _walletClientMock
                .Setup(client => client.Get<List<TenantWalletDetailDto>>(It.IsAny<string>(), It.IsAny<Dictionary<string, long>>()))
                .ReturnsAsync(new List<TenantWalletDetailDto>());

            _walletClientMock
                .Setup(client => client.Get<GetAllMasterWalletsResponseDto>(It.IsAny<string>(), It.IsAny<Dictionary<string, long>>()))
                .ReturnsAsync(new GetAllMasterWalletsResponseDto
                {
                    MasterWallets = new List<TenantWalletDetailDto>()
                    {
                        new TenantWalletDetailDto()
                        {
                            Wallet = new WalletDto()
                            {
                                WalletName = "TENANT_MASTER_REDEMPTION:SUSPENSE_WALLET_OTC"

                            },
                            WalletType = new WalletTypeDto()
                            {
                                WalletTypeCode = "wat-bc8f4f7c028d479f900f0af794e385c8"
                            }
                        }
                    }
                });

            _fisClientMock
               .Setup(client => client.Get<FundingHistoryResponseDto>(It.IsAny<string>(), It.IsAny<Dictionary<string, long>>()))
               .ReturnsAsync(new FundingHistoryResponseDto()
               {
                   FundingHistoryList = new List<FundingHistoryDto>()
                   {
                       new FundingHistoryDto()
                       {
                           TenantCode = "tenant123",
                           ConsumerCode = "consumer123",
                           FundRuleNumber = 1
                       }
                   }
               });

            _walletClientMock
               .Setup(client => client.Post<PurseFundingResponseDto>(It.IsAny<string>(), It.IsAny<PurseFundingRequestDto>()))
               .ReturnsAsync(new PurseFundingResponseDto());

            _fisClientMock
               .Setup(client => client.Post<CreateFundingHistoryResponse>(It.IsAny<string>(), It.IsAny<FundingHistoryDto>()))
               .ReturnsAsync(new CreateFundingHistoryResponse()
               {
                   ErrorCode = null
               });

            _walletClientMock
               .Setup(client => client.Post<PostRedeemStartResponseDto>(It.IsAny<string>(), It.IsAny<Core.Domain.Dtos.PostRedeemStartRequestDto>()))
               .ReturnsAsync(new PostRedeemStartResponseDto()
               {
                   ErrorCode = StatusCodes.Status400BadRequest
               });

            // Act
            var result = _service.ProcessInitialFundingAsync(validRequest);

            // Assert
            // Assert
            Assert.NotNull(result);
            Assert.Equal(500, result.ErrorCode);
            Assert.Contains("An error occurred while redeeming wallet balance of Consumer", result.ErrorMessage);
        } 

        [Fact]
        public async TaskAlias ProcessInitialFundingAsync_ShouldCallRedumtionFail_WhenValueLoadFail()
        {
            // Arrange
            var validRequest = new InitialFundingRequestDto
            {
                TenantCode = "tenant123",
                ConsumerCode = "consumer123",
                SelectedPurses = new List<string> { "OTC" }
            };

            var tenantAccount = new GetTenantAccountResponseDto
            {
                TenantAccount = new TenantAccountRequestDto()
                {
                    FundingConfigJson = JsonConvert.SerializeObject(new FundingConfigJson
                    {
                        FundingRules = new List<FundingRule>() {
                        new FundingRule()
                        {
                            Amount = 100,
                            ConsumerWalletType = "wat-4b364fg722f04034cv732b355d84f479",
                            MasterWalletType = "wat-4b364fg722f04034cv732b355d84f479",
                            Enabled = true,
                            RuleNumber = 3,
                            RuleDescription = "Upon onboarding, deposit $100 into the FOD Wallet",
                            RecurrenceType = "ONBOARDING",
                             EffectiveEndDate = DateTime.UtcNow.AddDays(10).ToString(),
                            EffectiveStartDate = DateTime.UtcNow.AddDays(-10).ToString(),
                        }
                    }
                    }),
                    TenantConfigJson = "{\"purseConfig\":{\"purses\":[{\"purseLabel\":\"OTC\",\"walletType\":\"wat-4b364fg722f04034cv732b355d84f479\",\"purseNumber\":12401,\"periodConfig\":{\"fundDate\":19,\"interval\":\"MONTH\",\"applyDateConfig\":{\"applyDate\":1,\"applyDateType\":\"NEXT_MONTH\"}},\"purseWalletType\":\"wat-4b364ed612f04034bf732b355d84f368\",\"masterWalletType\":\"wat-4b364fg722f04034cv732b355d84f479\",\"masterRedemptionWalletType\":\"wat-bc8f4f7c028d479f900f0af794e385c8\"},{\"purseLabel\":\"FOD\",\"walletType\":\"wat-35d4rt62d77y6119a6t730c9b6447457\",\"purseNumber\":22401,\"periodConfig\":{\"fundDate\":19,\"interval\":\"MONTH\",\"applyDateConfig\":{\"applyDate\":1,\"applyDateType\":\"NEXT_MONTH\"}},\"purseWalletType\":\"wat-35b3ac62d74b4119a5f630c9b6446035\",\"masterWalletType\":\"wat-35d4rt62d77y6119a6t730c9b6447457\",\"masterRedemptionWalletType\":\"wat-01b4a568c1814449b64168bb434127b7\"},{\"purseLabel\":\"REW\",\"walletType\":\"wat-2d62dcaf2aa4424b9ff6c2ddb5895077\",\"purseNumber\":32401,\"periodConfig\":{\"fundDate\":19,\"interval\":\"MONTH\",\"applyDateConfig\":{\"applyDate\":1,\"applyDateType\":\"NEXT_MONTH\"}},\"purseWalletType\":\"wat-55f05107b41642c39e7a3b459223cbd8\",\"masterWalletType\":\"wat-2d62dcaf2aa4424b9ff6c2ddb5895077\",\"masterRedemptionWalletType\":\"wat-274bd71345804f09928cf451dc0f6239\"}]},\"fisProgramDetail\":{\"clientId\":\"1234500\",\"companyId\":\"1204185\",\"packageId\":\"721736\",\"subprogramId\":\"873343\"}}"
                }

            };

            var consumerAccount = new ConsumerAccountDto();

            _tenantAccountServiceMock
                .Setup(service => service.GetTenantAccount(It.IsAny<string>()))
                .ReturnsAsync(tenantAccount);

            _consumerAccountServiceMock
                .Setup(service => service.GetConsumerAccount(It.IsAny<GetConsumerAccountRequestDto>()))
                .ReturnsAsync(new GetConsumerAccountResponseDto
                {
                    ConsumerAccount = consumerAccount
                });

            _walletClientMock
                .Setup(client => client.Get<List<TenantWalletDetailDto>>(It.IsAny<string>(), It.IsAny<Dictionary<string, long>>()))
                .ReturnsAsync(new List<TenantWalletDetailDto>());

            _walletClientMock
                .Setup(client => client.Get<GetAllMasterWalletsResponseDto>(It.IsAny<string>(), It.IsAny<Dictionary<string, long>>()))
                .ReturnsAsync(new GetAllMasterWalletsResponseDto
                {
                    MasterWallets = new List<TenantWalletDetailDto>()
                    {
                        new TenantWalletDetailDto()
                        {
                            Wallet = new WalletDto()
                            {
                                WalletName = "TENANT_MASTER_REDEMPTION:SUSPENSE_WALLET_OTC"

                            },
                            WalletType = new WalletTypeDto()
                            {
                                WalletTypeCode = "wat-bc8f4f7c028d479f900f0af794e385c8"
                            }
                        }
                    }
                });

            _fisClientMock
               .Setup(client => client.Get<FundingHistoryResponseDto>(It.IsAny<string>(), It.IsAny<Dictionary<string, long>>()))
               .ReturnsAsync(new FundingHistoryResponseDto()
               {
                   FundingHistoryList = new List<FundingHistoryDto>()
                   {
                       new FundingHistoryDto()
                       {
                           TenantCode = "tenant123",
                           ConsumerCode = "consumer123",
                           FundRuleNumber = 1
                       }
                   }
               });

            _walletClientMock
               .Setup(client => client.Post<PurseFundingResponseDto>(It.IsAny<string>(), It.IsAny<PurseFundingRequestDto>()))
               .ReturnsAsync(new PurseFundingResponseDto());

            _fisClientMock
               .Setup(client => client.Post<CreateFundingHistoryResponse>(It.IsAny<string>(), It.IsAny<FundingHistoryDto>()))
               .ReturnsAsync(new CreateFundingHistoryResponse()
               {
                   ErrorCode = null
               });

            _walletClientMock
               .Setup(client => client.Post<PostRedeemStartResponseDto>(It.IsAny<string>(), It.IsAny<Core.Domain.Dtos.PostRedeemStartRequestDto>()))
               .ReturnsAsync(new PostRedeemStartResponseDto());

            _fisClientMock
               .Setup(client => client.Post<LoadValueResponseDto>(It.IsAny<string>(), It.IsAny<LoadValueRequestDto>()))
               .ReturnsAsync(new LoadValueResponseDto()
               {
                   ErrorCode = StatusCodes.Status400BadRequest
               });

            _walletClientMock
              .Setup(client => client.Post<PostRedeemFailResponseDto>(It.IsAny<string>(), It.IsAny<PostRedeemFailRequestDto>()))
              .ReturnsAsync(new PostRedeemFailResponseDto());

            // Act
            var result =  _service.ProcessInitialFundingAsync(validRequest);

            // Assert
            // Assert
            Assert.NotNull(result);
            Assert.Null(result.ErrorCode);
            _walletClientMock.Verify(c => c.Post<PostRedeemFailResponseDto>(It.IsAny<string>(), It.IsAny<PostRedeemFailRequestDto>()), Times.Once);
        }

        [Fact]
        public async TaskAlias ProcessInitialFundingAsync_ShouldcallRedumptionFail_WhenValueLoadThrowsException()
        {
            // Arrange
            var validRequest = new InitialFundingRequestDto
            {
                TenantCode = "tenant123",
                ConsumerCode = "consumer123",
                SelectedPurses = new List<string> { "OTC" }
            };

            var tenantAccount = new GetTenantAccountResponseDto
            {
                TenantAccount = new TenantAccountRequestDto()
                {
                    FundingConfigJson = JsonConvert.SerializeObject(new FundingConfigJson
                    {
                        FundingRules = new List<FundingRule>() {
                        new FundingRule()
                        {
                            Amount = 100,
                            ConsumerWalletType = "wat-4b364fg722f04034cv732b355d84f479",
                            MasterWalletType = "wat-4b364fg722f04034cv732b355d84f479",
                            Enabled = true,
                            RuleNumber = 3,
                            RuleDescription = "Upon onboarding, deposit $100 into the FOD Wallet",
                            RecurrenceType = "ONBOARDING",
                             EffectiveEndDate = DateTime.UtcNow.AddDays(10).ToString(),
                            EffectiveStartDate = DateTime.UtcNow.AddDays(-10).ToString(),
                        }
                    }
                    }),
                    TenantConfigJson = "{\"purseConfig\":{\"purses\":[{\"purseLabel\":\"OTC\",\"walletType\":\"wat-4b364fg722f04034cv732b355d84f479\",\"purseNumber\":12401,\"periodConfig\":{\"fundDate\":19,\"interval\":\"MONTH\",\"applyDateConfig\":{\"applyDate\":1,\"applyDateType\":\"NEXT_MONTH\"}},\"purseWalletType\":\"wat-4b364ed612f04034bf732b355d84f368\",\"masterWalletType\":\"wat-4b364fg722f04034cv732b355d84f479\",\"masterRedemptionWalletType\":\"wat-bc8f4f7c028d479f900f0af794e385c8\"},{\"purseLabel\":\"FOD\",\"walletType\":\"wat-35d4rt62d77y6119a6t730c9b6447457\",\"purseNumber\":22401,\"periodConfig\":{\"fundDate\":19,\"interval\":\"MONTH\",\"applyDateConfig\":{\"applyDate\":1,\"applyDateType\":\"NEXT_MONTH\"}},\"purseWalletType\":\"wat-35b3ac62d74b4119a5f630c9b6446035\",\"masterWalletType\":\"wat-35d4rt62d77y6119a6t730c9b6447457\",\"masterRedemptionWalletType\":\"wat-01b4a568c1814449b64168bb434127b7\"},{\"purseLabel\":\"REW\",\"walletType\":\"wat-2d62dcaf2aa4424b9ff6c2ddb5895077\",\"purseNumber\":32401,\"periodConfig\":{\"fundDate\":19,\"interval\":\"MONTH\",\"applyDateConfig\":{\"applyDate\":1,\"applyDateType\":\"NEXT_MONTH\"}},\"purseWalletType\":\"wat-55f05107b41642c39e7a3b459223cbd8\",\"masterWalletType\":\"wat-2d62dcaf2aa4424b9ff6c2ddb5895077\",\"masterRedemptionWalletType\":\"wat-274bd71345804f09928cf451dc0f6239\"}]},\"fisProgramDetail\":{\"clientId\":\"1234500\",\"companyId\":\"1204185\",\"packageId\":\"721736\",\"subprogramId\":\"873343\"}}"
                }

            };

            var consumerAccount = new ConsumerAccountDto();

            _tenantAccountServiceMock
                .Setup(service => service.GetTenantAccount(It.IsAny<string>()))
                .ReturnsAsync(tenantAccount);

            _consumerAccountServiceMock
                .Setup(service => service.GetConsumerAccount(It.IsAny<GetConsumerAccountRequestDto>()))
                .ReturnsAsync(new GetConsumerAccountResponseDto
                {
                    ConsumerAccount = consumerAccount
                });

            _walletClientMock
                .Setup(client => client.Get<List<TenantWalletDetailDto>>(It.IsAny<string>(), It.IsAny<Dictionary<string, long>>()))
                .ReturnsAsync(new List<TenantWalletDetailDto>());

            _walletClientMock
                .Setup(client => client.Get<GetAllMasterWalletsResponseDto>(It.IsAny<string>(), It.IsAny<Dictionary<string, long>>()))
                .ReturnsAsync(new GetAllMasterWalletsResponseDto
                {
                    MasterWallets = new List<TenantWalletDetailDto>()
                    {
                        new TenantWalletDetailDto()
                        {
                            Wallet = new WalletDto()
                            {
                                WalletName = "TENANT_MASTER_REDEMPTION:SUSPENSE_WALLET_OTC"

                            },
                            WalletType = new WalletTypeDto()
                            {
                                WalletTypeCode = "wat-bc8f4f7c028d479f900f0af794e385c8"
                            }
                        }
                    }
                });

            _fisClientMock
               .Setup(client => client.Get<FundingHistoryResponseDto>(It.IsAny<string>(), It.IsAny<Dictionary<string, long>>()))
               .ReturnsAsync(new FundingHistoryResponseDto()
               {
                   FundingHistoryList = new List<FundingHistoryDto>()
                   {
                       new FundingHistoryDto()
                       {
                           TenantCode = "tenant123",
                           ConsumerCode = "consumer123",
                           FundRuleNumber = 1
                       }
                   }
               });

            _walletClientMock
               .Setup(client => client.Post<PurseFundingResponseDto>(It.IsAny<string>(), It.IsAny<PurseFundingRequestDto>()))
               .ReturnsAsync(new PurseFundingResponseDto());

            _fisClientMock
               .Setup(client => client.Post<CreateFundingHistoryResponse>(It.IsAny<string>(), It.IsAny<FundingHistoryDto>()))
               .ReturnsAsync(new CreateFundingHistoryResponse()
               {
                   ErrorCode = null
               });

            _walletClientMock
               .Setup(client => client.Post<PostRedeemStartResponseDto>(It.IsAny<string>(), It.IsAny<Core.Domain.Dtos.PostRedeemStartRequestDto>()))
               .ReturnsAsync(new PostRedeemStartResponseDto());

            _fisClientMock
               .Setup(client => client.Post<LoadValueResponseDto>(It.IsAny<string>(), It.IsAny<LoadValueRequestDto>()))
               .ThrowsAsync(new Exception("Testing"));

            // Act
            var result =  _service.ProcessInitialFundingAsync(validRequest);

            Assert.NotNull(result);
            Assert.Null(result.ErrorCode);
            _walletClientMock.Verify(c => c.Post<PostRedeemFailResponseDto>(It.IsAny<string>(), It.IsAny<PostRedeemFailRequestDto>()), Times.Once);
        }

        //happy path
        [Fact]
        public async TaskAlias ProcessInitialFundingAsync_ShouldCallRedumptionComplete_WhenAllSuccess()
        {
            // Arrange
            var validRequest = new InitialFundingRequestDto
            {
                TenantCode = "tenant123",
                ConsumerCode = "consumer123",
                SelectedPurses = new List<string> { "OTC" }
            };

            var tenantAccount = new GetTenantAccountResponseDto
            {
                TenantAccount = new TenantAccountRequestDto()
                {
                    FundingConfigJson = JsonConvert.SerializeObject(new FundingConfigJson
                    {
                        FundingRules = new List<FundingRule>() {
                        new FundingRule()
                        {
                            Amount = 100,
                            ConsumerWalletType = "wat-4b364fg722f04034cv732b355d84f479",
                            MasterWalletType = "wat-4b364fg722f04034cv732b355d84f479",
                            Enabled = true,
                            RuleNumber = 3,
                            RuleDescription = "Upon onboarding, deposit $100 into the FOD Wallet",
                            RecurrenceType = "ONBOARDING",
                             EffectiveEndDate = DateTime.UtcNow.AddDays(10).ToString(),
                            EffectiveStartDate = DateTime.UtcNow.AddDays(-10).ToString(),
                        }
                    }
                    }),
                    TenantConfigJson = "{\"purseConfig\":{\"purses\":[{\"purseLabel\":\"OTC\",\"walletType\":\"wat-4b364fg722f04034cv732b355d84f479\",\"purseNumber\":12401,\"periodConfig\":{\"fundDate\":19,\"interval\":\"MONTH\",\"applyDateConfig\":{\"applyDate\":1,\"applyDateType\":\"NEXT_MONTH\"}},\"purseWalletType\":\"wat-4b364ed612f04034bf732b355d84f368\",\"masterWalletType\":\"wat-4b364fg722f04034cv732b355d84f479\",\"masterRedemptionWalletType\":\"wat-bc8f4f7c028d479f900f0af794e385c8\"},{\"purseLabel\":\"FOD\",\"walletType\":\"wat-35d4rt62d77y6119a6t730c9b6447457\",\"purseNumber\":22401,\"periodConfig\":{\"fundDate\":19,\"interval\":\"MONTH\",\"applyDateConfig\":{\"applyDate\":1,\"applyDateType\":\"NEXT_MONTH\"}},\"purseWalletType\":\"wat-35b3ac62d74b4119a5f630c9b6446035\",\"masterWalletType\":\"wat-35d4rt62d77y6119a6t730c9b6447457\",\"masterRedemptionWalletType\":\"wat-01b4a568c1814449b64168bb434127b7\"},{\"purseLabel\":\"REW\",\"walletType\":\"wat-2d62dcaf2aa4424b9ff6c2ddb5895077\",\"purseNumber\":32401,\"periodConfig\":{\"fundDate\":19,\"interval\":\"MONTH\",\"applyDateConfig\":{\"applyDate\":1,\"applyDateType\":\"NEXT_MONTH\"}},\"purseWalletType\":\"wat-55f05107b41642c39e7a3b459223cbd8\",\"masterWalletType\":\"wat-2d62dcaf2aa4424b9ff6c2ddb5895077\",\"masterRedemptionWalletType\":\"wat-274bd71345804f09928cf451dc0f6239\"}]},\"fisProgramDetail\":{\"clientId\":\"1234500\",\"companyId\":\"1204185\",\"packageId\":\"721736\",\"subprogramId\":\"873343\"}}"
                }

            };

            var consumerAccount = new ConsumerAccountDto();

            _tenantAccountServiceMock
                .Setup(service => service.GetTenantAccount(It.IsAny<string>()))
                .ReturnsAsync(tenantAccount);

            _consumerAccountServiceMock
                .Setup(service => service.GetConsumerAccount(It.IsAny<GetConsumerAccountRequestDto>()))
                .ReturnsAsync(new GetConsumerAccountResponseDto
                {
                    ConsumerAccount = consumerAccount
                });

            _walletClientMock
                .Setup(client => client.Get<List<TenantWalletDetailDto>>(It.IsAny<string>(), It.IsAny<Dictionary<string, long>>()))
                .ReturnsAsync(new List<TenantWalletDetailDto>());

            _walletClientMock
                .Setup(client => client.Get<GetAllMasterWalletsResponseDto>(It.IsAny<string>(), It.IsAny<Dictionary<string, long>>()))
                .ReturnsAsync(new GetAllMasterWalletsResponseDto
                {
                    MasterWallets = new List<TenantWalletDetailDto>()
                    {
                        new TenantWalletDetailDto()
                        {
                            Wallet = new WalletDto()
                            {
                                WalletName = "TENANT_MASTER_REDEMPTION:SUSPENSE_WALLET_OTC"

                            },
                            WalletType = new WalletTypeDto()
                            {
                                WalletTypeCode = "wat-bc8f4f7c028d479f900f0af794e385c8"
                            }
                        }
                    }
                });

            _fisClientMock
               .Setup(client => client.Get<FundingHistoryResponseDto>(It.IsAny<string>(), It.IsAny<Dictionary<string, long>>()))
               .ReturnsAsync(new FundingHistoryResponseDto()
               {
                   FundingHistoryList = new List<FundingHistoryDto>()
                   {
                       new FundingHistoryDto()
                       {
                           TenantCode = "tenant123",
                           ConsumerCode = "consumer123",
                           FundRuleNumber = 1
                       }
                   }
               });

            _walletClientMock
               .Setup(client => client.Post<PurseFundingResponseDto>(It.IsAny<string>(), It.IsAny<PurseFundingRequestDto>()))
               .ReturnsAsync(new PurseFundingResponseDto());

            _fisClientMock
               .Setup(client => client.Post<CreateFundingHistoryResponse>(It.IsAny<string>(), It.IsAny<FundingHistoryDto>()))
               .ReturnsAsync(new CreateFundingHistoryResponse()
               {
                   ErrorCode = null
               });

            _walletClientMock
               .Setup(client => client.Post<PostRedeemStartResponseDto>(It.IsAny<string>(), It.IsAny<Core.Domain.Dtos.PostRedeemStartRequestDto>()))
               .ReturnsAsync(new PostRedeemStartResponseDto());

            _fisClientMock
               .Setup(client => client.Post<LoadValueResponseDto>(It.IsAny<string>(), It.IsAny<LoadValueRequestDto>()))
               .ReturnsAsync(new LoadValueResponseDto());

            _walletClientMock
               .Setup(client => client.Post<PostRedeemCompleteResponseDto>(It.IsAny<string>(), It.IsAny<PostRedeemCompleteResponseDto>()))
               .ReturnsAsync(new PostRedeemCompleteResponseDto());

            // Act
            var result =  _service.ProcessInitialFundingAsync(validRequest);

            // Assert
            Assert.NotNull(result);
            Assert.Null(result.ErrorCode);
            _walletClientMock.Verify(c => c.Post<PostRedeemFailResponseDto>(It.IsAny<string>(), It.IsAny<PostRedeemFailRequestDto>()), Times.Never());

            _walletClientMock.Verify(c => c.Post<PostRedeemCompleteResponseDto>(It.IsAny<string>(), It.IsAny<PostRedeemCompleteRequestDto>()), Times.Once());

        }

        [Fact]
        public async TaskAlias ProcessInitialFundingAsync_ShouldReturnError_WhenUnexpectedErrorOccurs()
        {
            // Arrange
            var validRequest = new InitialFundingRequestDto
            {
                TenantCode = "tenant123",
                ConsumerCode = "consumer123",
                SelectedPurses = new List<string> { "OTC" }
            };

            _tenantAccountServiceMock
                .Setup(service => service.GetTenantAccount(It.IsAny<string>()))
                .ThrowsAsync(new Exception("Unexpected error"));

            // Act
            var result =  _service.ProcessInitialFundingAsync(validRequest);

            // Assert
            Assert.Equal(StatusCodes.Status500InternalServerError, result.ErrorCode);
            Assert.Contains("An unexpected error occurred", result.ErrorMessage);
        }
    }
}
