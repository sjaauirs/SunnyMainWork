using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Sunny.Benefits.Bff.Api.Controllers;
using Sunny.Benefits.Bff.Core.Constants;
using Sunny.Benefits.Bff.Core.Domain.Constants;
using Sunny.Benefits.Bff.Core.Domain.Dtos;
using Sunny.Benefits.Bff.Infrastructure.HttpClients.Interfaces;
using Sunny.Benefits.Bff.Infrastructure.Repositories.Interfaces;
using Sunny.Benefits.Bff.Infrastructure.Services;
using Sunny.Benefits.Bff.Infrastructure.Services.Interfaces;
using Sunny.Benefits.Bff.UnitTest.Fixtures.MockDtos;
using Sunny.Benefits.Bff.UnitTest.Fixtures.MockModels;
using Sunny.Benefits.Bff.UnitTest.HttpClients;
using SunnyBenefits.Fis.Core.Domain.Dtos;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.Common.Core.Helpers.Interfaces;
using SunnyRewards.Helios.Tenant.Core.Domain.Dtos;
using SunnyRewards.Helios.User.Core.Domain.Dtos;
using SunnyRewards.Helios.Wallet.Core.Domain.Dtos;
using SunnyRewards.Helios.Wallet.Core.Domain.Models;
using System.Reflection;
using Xunit;

namespace Sunny.Benefits.Bff.UnitTest.Controllers
{
    public class WalletControllerUnitTest
    {
        private readonly Mock<ILogger<WalletController>> _walletontrollerLogger;
        private readonly Mock<ILogger<WalletService>> _walletServiceLogger;
        private readonly IWalletService _walletService;
        private readonly Mock<ITaskClient> _taskClient;
        private readonly Mock<IFisClient> _fisClient;
        private readonly Mock<IUserClient> _userClient;
        private readonly WalletController _walletController;
        private readonly Mock<IWalletClient> _walletClientMock;
        private readonly Mock<ITenantService> _tenantServiceMock;
        private readonly Mock<IVault> _vault;

        public WalletControllerUnitTest()
        {
            _walletontrollerLogger = new Mock<ILogger<WalletController>>();
            _walletServiceLogger = new Mock<ILogger<WalletService>>();
            _walletClientMock = new WalletClientMock();
            _taskClient = new TaskClientMock();
            _fisClient = new FisClientMock();
            _userClient = new UserClientMock();
            _vault = new Mock<IVault>();
            _tenantServiceMock = new Mock<ITenantService>();
            _walletService = new WalletService(_walletServiceLogger.Object, _walletClientMock.Object,
                _taskClient.Object, _tenantServiceMock.Object, _fisClient.Object, _userClient.Object, _vault.Object);
            _walletController = new WalletController(_walletontrollerLogger.Object, _walletService);
        }

        [Fact]
        public async Task Should_GetWallets_Controller()
        {
            var findConsumerWalletRequestMockDto = new FindConsumerWalletRequestMockDto();
            var response = await _walletController.GetWallets(findConsumerWalletRequestMockDto);
            var result = (int)((ObjectResult)response).StatusCode;
            Assert.True(result == 200);
        }

        [Fact]
        public async Task Should_GetWallets_NotFound_Controller()
        {
            var findConsumerWalletRequestMockDto = new FindConsumerWalletRequestMockDto();
            var walletServiceMock = new Mock<IWalletService>();
            walletServiceMock.Setup(x => x.GetWallets(findConsumerWalletRequestMockDto, null))
          .ReturnsAsync(new WalletResponseMockDto()
          {
              GrandTotal = 0,
              walletDetailDto = null,
              ErrorCode = 404
          });
            var walletController = new WalletController(_walletontrollerLogger.Object, walletServiceMock.Object);
            var response = await walletController.GetWallets(findConsumerWalletRequestMockDto);
            var notFoundResult = (NotFoundResult)response;
            int statusCode = notFoundResult.StatusCode;
            Assert.True(statusCode == 404);
        }

        [Fact]
        public async Task Should_GetWallets_Catch_Exception_Controller()
        {
            var findConsumerWalletRequestMockDto = new FindConsumerWalletRequestMockDto();
            var walletServiceMock = new Mock<IWalletService>();
            walletServiceMock.Setup(x => x.GetWallets(findConsumerWalletRequestMockDto, null))
          .ThrowsAsync(new InvalidOperationException("Simulated exception"));
            var walletController = new WalletController(_walletontrollerLogger.Object, walletServiceMock.Object);
            await Assert.ThrowsAsync<InvalidOperationException>(async () => await walletController.GetWallets(findConsumerWalletRequestMockDto));
        }

        [Fact]
        public async Task Should_GetWallets_Service()
        {
            var findConsumerWalletRequestMockDto = new FindConsumerWalletRequestMockDto();
            var liveBalance = new ExternalSyncWalletResponseDtoMock();
            _userClient.Setup(client => client.Post<GetConsumerResponseDto>("consumer/get-consumer", It.IsAny<BaseRequestDto>()))
                          .ReturnsAsync(new GetConsumerResponseMockDto());
            _fisClient.Setup(client => client.Post<ExternalSyncWalletResponseDto>("fis/get-purse-balances", It.IsAny<ExternalSyncWalletRequestDto>()))
                          .ReturnsAsync(liveBalance);
            FisGetFundingDescriptionResponseDto walletResponseDto = GetFundingDescriptionResponse();

            _fisClient.Setup(client => client.Post<FisGetFundingDescriptionResponseDto>("get-funding-description", It.IsAny<FisGetFundingDescriptionRequestDto>()))
                .ReturnsAsync(walletResponseDto);

            var response = await _walletService.GetWallets(findConsumerWalletRequestMockDto);
            Assert.True(response.walletDetailDto != null);
        }
        [Fact]
        public async Task Should_GetWallets_Service_when_PickAPurseEnabled()
        {
            // Arrange
            var requestDto = new FindConsumerWalletRequestMockDto();
            WalletResponseMockDto mockdto = new WalletResponseMockDto();
            var mockWalletDetailDto = mockdto.walletDetailDto;
            string ConsumerCode = "cmr-c457c5257c59451d8a93ea941a9f2e0a";
            var liveBalance = new ExternalSyncWalletResponseDtoMock();
            var consumerRequest = new BaseRequestDto { consumerCode = ConsumerCode };

            _walletClientMock.Setup(client => client.Post<WalletResponseDto>("wallet/get-wallets", It.IsAny<FindConsumerWalletRequestDto>()))
            .ReturnsAsync(mockdto);
            _userClient.Setup(client => client.Post<GetConsumerResponseDto>("consumer/get-consumer", It.IsAny<BaseRequestDto>()))
                      .ReturnsAsync(new GetConsumerResponseMockDto());
            _fisClient.Setup(client => client.Post<ExternalSyncWalletResponseDto>("fis/get-purse-balances", It.IsAny<ExternalSyncWalletRequestDto>()))
                          .ReturnsAsync(liveBalance);


            _tenantServiceMock.Setup(x => x.GetTenantByConsumerCode(It.IsAny<string>())).ReturnsAsync(new GetTenantResponseDto
            {
                Tenant = new TenantDto
                {
                    TenantId = 1,
                    SponsorId = 1,
                    TenantCode = "ten-ecada21e57154928a2bb959e8365b8b4",
                    PlanYear = 2023,
                    PeriodStartTs = DateTime.UtcNow,
                    PartnerCode = "par-7e92b06aa4fe405198d27d2427bf3de4",
                    RecommendedTask = false,
                    TenantAttribute = "{\"pickAPurseOnboardingEnabled\":true}",
                    SelfReport = true,
                    EnableServerLogin = false,
                    DirectLogin = false,
                    TenantName = "SunnyTenant",
                    ApiKey = "19afb4b6-dvtu-5869-ndvr-025d2a617b78",
                    CreateTs = DateTime.Now,
                    UpdateTs = DateTime.Now,
                    CreateUser = "sunny",
                    UpdateUser = "sunny rewards",
                    DeleteNbr = 0
                }
            });
            FisGetFundingDescriptionResponseDto walletResponseDto = GetFundingDescriptionResponse();
            _walletClientMock.Setup(client => client.Post<BaseResponseDto>("wallet/update-wallet-balance", It.IsAny<List<WalletModel>>()))
                .ReturnsAsync(new BaseResponseDto());

            _fisClient.Setup(client => client.Post<FisGetFundingDescriptionResponseDto>("get-funding-description", It.IsAny<FisGetFundingDescriptionRequestDto>()))
                          .ReturnsAsync(walletResponseDto);
            _fisClient.Setup(client => client.Post<ExportTenantAccountResponseDto>(WalletConstants.GetTenantAccount, It.IsAny<ExportTenantAccountRequestDto>()))
                          .ReturnsAsync(new ExportTenantAccountResponseDto
                          {
                              TenantAccount = new GetTenantAccountDto
                              {
                                  TenantCode = "ten-ecada21e57154928a2bb959e8365b8b4",
                                  TenantConfigJson = "{\"purseConfig\":{\"purses\":[{\"purseLabel\":\"FOD\",\"purseWalletType\":\"wat-55f05107b41642c39e7a3b459223cbd8\"},{\"purseLabel\":\"MASTER_FUND_NO\",\"purseWalletType\":\"test\"}]}}",
                              }
                          });
            _fisClient.Setup(client => client.Post<GetConsumerAccountResponseDto>(WalletConstants.GetConsumerAccount, It.IsAny<GetConsumerAccountRequestDto>()))
                          .ReturnsAsync(new GetConsumerAccountResponseDto
                          {
                              ConsumerAccount = new ConsumerAccountDto
                              {
                                  TenantCode = "ten-ecada21e57154928a2bb959e8365b8b4",
                                  ConsumerCode = "cmr-f9c419da974c4bbb99eab99fd3b490e0",
                                  ConsumerAccountConfigJson = "{\"purseConfig\":{\"purses\":[{\"purseLabel\":\"FOD\",\"enabled\":false},{\"purseLabel\":\"MASTER_FUND_NO\",\"enabled\": true}]}}",
                              }
                          });

            var walletService = new WalletService(_walletServiceLogger.Object, _walletClientMock.Object, _taskClient.Object,
                _tenantServiceMock.Object, _fisClient.Object, _userClient.Object, _vault.Object);

            // Act
            var result = await walletService.GetWallets(requestDto);

            // Assert
            Assert.True(result.walletDetailDto != null);
        }
        [Fact]
        public async Task Should_GetWallets_Service_when_PickAPurseEnabled_No_walletFound()
        {
            // Arrange // Arrange
            var requestDto = new FindConsumerWalletRequestMockDto();
            WalletResponseMockDto mockdto = new WalletResponseMockDto();
            var mockWalletDetailDto = mockdto.walletDetailDto;
            _walletClientMock.Setup(client => client.Post<WalletResponseDto>("wallet/get-wallets", It.IsAny<FindConsumerWalletRequestDto>()))
            .ReturnsAsync(mockdto);
            _userClient.Setup(client => client.Post<GetConsumerResponseDto>("consumer/get-consumer", It.IsAny<BaseRequestDto>()))
                      .ReturnsAsync(new GetConsumerResponseMockDto());
            _fisClient.Setup(client => client.Post<ExternalSyncWalletResponseDto>("fis/get-purse-balances", It.IsAny<ExternalSyncWalletRequestDto>()))
                          .ReturnsAsync(new ExternalSyncWalletResponseDtoMock());
            _tenantServiceMock.Setup(x => x.GetTenantByConsumerCode(It.IsAny<string>())).ReturnsAsync(new GetTenantResponseDto
            {
                Tenant = new TenantDto
                {
                    TenantId = 1,
                    SponsorId = 1,
                    TenantCode = "ten-ecada21e57154928a2bb959e8365b8b4",
                    PlanYear = 2023,
                    PeriodStartTs = DateTime.UtcNow,
                    PartnerCode = "par-7e92b06aa4fe405198d27d2427bf3de4",
                    RecommendedTask = false,
                    TenantAttribute = "{\"pickAPurseOnboardingEnabled\":true}",
                    SelfReport = true,
                    EnableServerLogin = false,
                    DirectLogin = false,
                    TenantName = "SunnyTenant",
                    ApiKey = "19afb4b6-dvtu-5869-ndvr-025d2a617b78",
                    CreateTs = DateTime.Now,
                    UpdateTs = DateTime.Now,
                    CreateUser = "sunny",
                    UpdateUser = "sunny rewards",
                    DeleteNbr = 0
                }
            });

            var walletResponseDto = new FisGetFundingDescriptionResponseDto()
            {
                FundingDescriptions = new Dictionary<string, FundingDescriptionDto>
            {
                { "wat-1234",  new FundingDescriptionDto { FundAmount = 202.50,FundDate = DateTime.UtcNow } },
                { "wat-5678",  new FundingDescriptionDto { FundAmount = 202.50,FundDate = DateTime.UtcNow } },
            }
            };

            _fisClient.Setup(client => client.Post<FisGetFundingDescriptionResponseDto>("get-funding-description", It.IsAny<FisGetFundingDescriptionRequestDto>()))
                          .ReturnsAsync(walletResponseDto);
            _fisClient.Setup(client => client.Post<ExportTenantAccountResponseDto>(WalletConstants.GetTenantAccount, It.IsAny<ExportTenantAccountRequestDto>()))
                          .ReturnsAsync(new ExportTenantAccountResponseDto
                          {
                              TenantAccount = new GetTenantAccountDto
                              {
                                  TenantCode = "ten-ecada21e57154928a2bb959e8365b8b4",
                                  TenantConfigJson = "{\"purseConfig\":{\"purses\":[{\"purseLabel\":\"FOD\",\"purseWalletType\":\"wat-55f05107b41642c39e7a3b459223cbd8\"},{\"purseLabel\":\"MASTER_FUND_NO\",\"purseWalletType\":\"wat-55f05107b41642c39e7a3b45wrbd8\"}]}}",
                              }
                          });
            _fisClient.Setup(client => client.Post<GetConsumerAccountResponseDto>(WalletConstants.GetConsumerAccount, It.IsAny<GetConsumerAccountRequestDto>()))
                          .ReturnsAsync(new GetConsumerAccountResponseDto
                          {
                              ConsumerAccount = new ConsumerAccountDto
                              {
                                  TenantCode = "ten-ecada21e57154928a2bb959e8365b8b4",
                                  ConsumerCode = "cmr-f9c419da974c4bbb99eab99fd3b490e0",
                                  ConsumerAccountConfigJson = "{\"purseConfig\":{\"purses\":[{\"purseLabel\":\"FOD\",\"enabled\":false},{\"purseLabel\":\"MASTER_FUND_NO\",\"enabled\": true}]}}",
                              }
                          });

            var walletService = new WalletService(_walletServiceLogger.Object, _walletClientMock.Object, _taskClient.Object,
                _tenantServiceMock.Object, _fisClient.Object, _userClient.Object, _vault.Object);

            // Act
            var result = await walletService.GetWallets(requestDto);

            // Assert
            Assert.True(result.walletDetailDto != null);
        }
        [Fact]
        public async Task Should_GetWallets_NotFound_Service()
        {
            var findConsumerWalletRequestMockDto = new FindConsumerWalletRequestMockDto();
            var walletClientMock = new Mock<IWalletClient>();
            walletClientMock.Setup(client => client.Post<WalletResponseDto>("wallet/get-wallets", It.IsAny<FindConsumerWalletRequestDto>()))
          .ReturnsAsync(new WalletResponseMockDto()
          {
              ErrorCode = 404
          });
            var walletService = new WalletService(_walletServiceLogger.Object, walletClientMock.Object, _taskClient.Object,
                _tenantServiceMock.Object, _fisClient.Object, _userClient.Object, _vault.Object);
            var response = await walletService.GetWallets(findConsumerWalletRequestMockDto);
            var result = response.walletDetailDto == null;
            Assert.True(response.ErrorCode == 404);
        }

        [Fact]
        public async Task Should_GetWallets_Catch_Exception_Service()
        {
            var findConsumerWalletRequestMockDto = new FindConsumerWalletRequestMockDto();
            var walletClientMock = new Mock<IWalletClient>();
            walletClientMock.Setup(client => client.Post<WalletResponseDto>("wallet/get-wallets", It.IsAny<FindConsumerWalletRequestDto>()))
            .ThrowsAsync(new Exception("Simulated exception"));
            var walletService = new WalletService(_walletServiceLogger.Object, walletClientMock.Object, _taskClient.Object, _tenantServiceMock.Object,
                _fisClient.Object, _userClient.Object, _vault.Object);
            var response = await walletService.GetWallets(findConsumerWalletRequestMockDto);
            Assert.NotEqual(0, response.ErrorCode);
            Assert.True(response.ErrorMessage == null);
        }

        [Fact]
        public async Task Should_GetTransactions_Controller()
        {
            var postGetTransactionsRequestMockDto = new PostGetTransactionsRequestMockDto();
            var response = await _walletController.GetTransactions(postGetTransactionsRequestMockDto);
            var result = (int)((ObjectResult)response).StatusCode;
            Assert.True(result == 200);
        }
        [Fact]
        public async Task Should_GetTransactions_NotFound_Controller()
        {
            var postGetTransactionsRequestMockDto = new PostGetTransactionsRequestMockDto();
            var walletServiceMock = new Mock<IWalletService>();
            walletServiceMock.Setup(x => x.GetTransactions(postGetTransactionsRequestMockDto))
          .ReturnsAsync(new TransactionBySectionResponseMockDto()
          {
              Transaction = null,
              ErrorCode = 404
          });
            var walletController = new WalletController(_walletontrollerLogger.Object, walletServiceMock.Object);
            var response = await walletController.GetTransactions(postGetTransactionsRequestMockDto);
            var notFoundResult = (NotFoundResult)response;
            int statusCode = notFoundResult.StatusCode;
            Assert.True(statusCode == 404);
        }

        [Fact]
        public async Task Should_GetTransactions_Catch_Exception_Controller()
        {
            var postGetTransactionsRequestMockDto = new PostGetTransactionsRequestMockDto();
            var walletServiceMock = new Mock<IWalletService>();
            walletServiceMock.Setup(x => x.GetTransactions(postGetTransactionsRequestMockDto))
          .ThrowsAsync(new InvalidOperationException("Simulated exception"));
            var walletController = new WalletController(_walletontrollerLogger.Object, walletServiceMock.Object);
            await Assert.ThrowsAsync<InvalidOperationException>(async () => await walletController.GetTransactions(postGetTransactionsRequestMockDto));
        }

        [Fact]
        public async Task Should_GetTransactions_Service()
        {
            var postGetTransactionsRequestMockDto = new PostGetTransactionsRequestMockDto();
            var response = await _walletService.GetTransactions(postGetTransactionsRequestMockDto);
            Assert.True(response.Transaction != null);
        }

        [Fact]
        public async Task Should_GetTransactions_NotFound_Service()
        {
            //Arrange
            var postGetTransactionsRequestMockDto = new PostGetTransactionsRequestMockDto();
            var walletClientMock = new Mock<IWalletClient>();
            walletClientMock.Setup(client => client.Post<WalletResponseDto>("wallet/get-wallets", It.IsAny<FindConsumerWalletRequestDto>()))
             .ReturnsAsync(new WalletResponseMockDto());
            walletClientMock.Setup(client => client.Post<PostGetTransactionsResponseDto>("transaction/get-transactions",
              It.IsAny<PostGetTransactionsRequestMockDto>())).ReturnsAsync(new PostGetTransactionsResponseMockDto()
              {
                  Transactions = new List<TransactionEntryDto>(),
                  ErrorCode = 404
              });
            _userClient.Setup(client => client.Post<GetConsumerResponseDto>("consumer/get-consumer", It.IsAny<BaseRequestDto>()))
                      .ReturnsAsync(new GetConsumerResponseMockDto());
            FisGetFundingDescriptionResponseDto walletResponseDto = GetFundingDescriptionResponse();

            _fisClient.Setup(client => client.Post<FisGetFundingDescriptionResponseDto>("get-funding-description", It.IsAny<FisGetFundingDescriptionRequestDto>()))
                          .ReturnsAsync(walletResponseDto);
            _fisClient.Setup(client => client.Post<ExternalSyncWalletResponseDto>("fis/get-purse-balances", It.IsAny<ExternalSyncWalletRequestDto>()))
                          .ReturnsAsync(new ExternalSyncWalletResponseDtoMock());
            var walletService = new WalletService(_walletServiceLogger.Object, walletClientMock.Object, _taskClient.Object, _tenantServiceMock.Object,
                _fisClient.Object, _userClient.Object, _vault.Object);
            // Act
            var response = await walletService.GetTransactions(postGetTransactionsRequestMockDto);
            var result = response.Transaction != null;
            Assert.True(response.ErrorCode == 404);
        }

        private static FisGetFundingDescriptionResponseDto GetFundingDescriptionResponse()
        {
            return new FisGetFundingDescriptionResponseDto()
            {
                FundingDescriptions = new Dictionary<string, FundingDescriptionDto>
            {
                { "wat-1234",  new FundingDescriptionDto { FundAmount = 202.50,FundDate = DateTime.UtcNow } },
                { "wat-5678",  new FundingDescriptionDto { FundAmount = 202.50,FundDate = DateTime.UtcNow } },
            }
            };
        }

        [Fact]
        public async Task Should_GetTransactions_Catch_Exception_Service()
        {
            var postGetTransactionsRequestMockDto = new PostGetTransactionsRequestMockDto();
            var walletResponse = new WalletResponseMockDto();
            walletResponse.walletDetailDto[0].Wallet.WalletId = postGetTransactionsRequestMockDto.WalletId.Value;
            var walletClientMock = new Mock<IWalletClient>();
            walletClientMock.Setup(client => client.Post<WalletResponseDto>("wallet/get-wallets", It.IsAny<FindConsumerWalletRequestDto>()))
                .ReturnsAsync(walletResponse);
            walletClientMock.Setup(client => client.Post<PostGetTransactionsResponseDto>("transaction/get-transactions", It.IsAny<PostGetTransactionsRequestMockDto>()))
                .ThrowsAsync(new Exception("Simulated exception"));
            _userClient.Setup(client => client.Post<GetConsumerResponseDto>("consumer/get-consumer", It.IsAny<BaseRequestDto>()))
                      .ReturnsAsync(new GetConsumerResponseMockDto());
            FisGetFundingDescriptionResponseDto walletResponseDto = GetFundingDescriptionResponse();

            _fisClient.Setup(client => client.Post<FisGetFundingDescriptionResponseDto>("get-funding-description", It.IsAny<FisGetFundingDescriptionRequestDto>()))
                          .ReturnsAsync(walletResponseDto);
            _fisClient.Setup(client => client.Post<ExternalSyncWalletResponseDto>("fis/get-purse-balances", It.IsAny<ExternalSyncWalletRequestDto>()))
                          .ReturnsAsync(new ExternalSyncWalletResponseDtoMock());

            var walletService = new WalletService(_walletServiceLogger.Object, walletClientMock.Object, _taskClient.Object, _tenantServiceMock.Object,
                _fisClient.Object, _userClient.Object, _vault.Object);
            _tenantServiceMock.Setup(x=>x.GetTenantByConsumerCode(It.IsAny<string>())).ReturnsAsync(new GetTenantResponseMockDto());

            await Assert.ThrowsAsync<InvalidOperationException>(async () => await walletService.GetTransactions(postGetTransactionsRequestMockDto));
        }
        [Fact]
        public async Task GetUpdatedWalletBalance_Successful()
        {
            // Arrange
            var findConsumerWalletRequestMockDto = new FindConsumerWalletRequestMockDto();
            var walletClientMock = new Mock<IWalletClient>();
            var userClientMock = new Mock<IUserClient>();
            var fisClientMock = new Mock<IFisClient>();

            WalletResponseMockDto mockdto = new WalletResponseMockDto();
            var filterWallets = mockdto.walletDetailDto;
            walletClientMock.Setup(client => client.Post<WalletResponseDto>("wallet/get-wallets", It.IsAny<FindConsumerWalletRequestDto>()))
       .ReturnsAsync(mockdto);
            var consumer = new GetConsumerResponseMockDto();
            var liveBalance = new ExternalSyncWalletResponseDto()
            {
                Wallets = new List<ExternalSyncWalletDto>() { new ExternalSyncWalletDto(){
                PurseNumber = 12111,
                PurseWalletType = "test",
                Wallet = new WalletDto()
                {
                    WalletId = 1,
                    WalletTypeId = 1,
                    CustomerCode = "customer123",
                    SponsorCode = "sponsor456",
                    TenantCode = "tenant789",
                    WalletCode = "wallet001",
                    MasterWallet = true,
                    WalletName = "Main Wallet",
                    Active = true,
                    ActiveStartTs = DateTime.UtcNow,
                    ActiveEndTs = DateTime.UtcNow.AddYears(1),
                    Balance = 1000.00,
                    EarnMaximum = 500.00,
                    TotalEarned = 200.00,
                    PendingTasksTotalRewardAmount = 100.00,
                    LeftToEarn = 300.00,
                    CreateTs = DateTime.UtcNow,
                    CreateUser = "admin"
                } }
            }
            };
            string consumerCode = "cmr-c457c5257c59451d8a93ea941a9f2e0a";
            var consumerRequest = new BaseRequestDto { consumerCode = consumerCode };


            userClientMock.Setup(client => client.Post<GetConsumerResponseDto>("consumer/get-consumer", It.IsAny<BaseRequestDto>()))
                      .ReturnsAsync(new GetConsumerResponseMockDto());
            fisClientMock.Setup(client => client.Post<ExternalSyncWalletResponseDto>("fis/get-purse-balances", It.IsAny<ExternalSyncWalletRequestDto>()))
                          .ReturnsAsync(liveBalance);
            walletClientMock.Setup(client => client.Post<BaseResponseDto>("wallet/update-wallet-balance", It.IsAny<List<WalletModel>>()))
     .ReturnsAsync(new BaseResponseDto());


            var walletService = new WalletService(_walletServiceLogger.Object, walletClientMock.Object, _taskClient.Object, _tenantServiceMock.Object,
                fisClientMock.Object, userClientMock.Object, _vault.Object);

            // Act
            var result = await walletService.GetWallets(findConsumerWalletRequestMockDto);

            // Assert
            Assert.True(result.walletDetailDto != null);
        }
        [Fact]
        public async Task GetUpdatedWalletBalance_NotFound()
        {
            // Arrange

            var findConsumerWalletRequestMockDto = new FindConsumerWalletRequestMockDto();
            var walletClientMock = new Mock<IWalletClient>();
            var userClientMock = new Mock<IUserClient>();
            var fisClientMock = new Mock<IFisClient>();
            WalletResponseMockDto mockdto = new WalletResponseMockDto();

            walletClientMock.Setup(client => client.Post<WalletResponseDto>("wallet/get-wallets", It.IsAny<FindConsumerWalletRequestDto>()))
       .ReturnsAsync(new WalletResponseMockDto()
       {
           ErrorCode = 404
       });

            var liveBalance = new ExternalSyncWalletResponseDtoMock();
            string consumerCode = "cmr-c457c5257c59451d8a93ea941a9f2e0a";
            var consumerRequest = new BaseRequestDto { consumerCode = consumerCode };

            userClientMock.Setup(client => client.Post<GetConsumerResponseDto>("consumer/get-consumer", consumerRequest))
                          .ReturnsAsync(new GetConsumerResponseDto()
                          {
                              ErrorCode = 404
                          });

            fisClientMock.Setup(client => client.Post<ExternalSyncWalletResponseDto>("fis/get-purse-balances", It.IsAny<ExternalSyncWalletRequestDto>()))
                          .ReturnsAsync(liveBalance);
            walletClientMock.Setup(client => client.Post<BaseResponseDto>("wallet/update-wallet-balance", It.IsAny<List<WalletModel>>()))
     .ReturnsAsync(new BaseResponseDto() { ErrorCode = 404 });

            var walletService = new WalletService(_walletServiceLogger.Object, walletClientMock.Object, _taskClient.Object, _tenantServiceMock.Object,
                fisClientMock.Object, userClientMock.Object, _vault.Object);

            // Act
            var result = await walletService.GetWallets(findConsumerWalletRequestMockDto);

            // Assert
            Assert.True(result.ErrorCode == 404);

        }
        [Fact]
        public async Task GetTransactions_ReturnsCardTransaction()
        {
            // Arrange
            var requestDto = new PostGetTransactionsRequestMockDto();
            WalletResponseMockDto mockdto = new WalletResponseMockDto();
            mockdto.walletDetailDto[0].Wallet.WalletId = requestDto.WalletId.Value;
            var mockWalletDetailDto = mockdto.walletDetailDto;

            string ConsumerCode = "cmr-c457c5257c59451d8a93ea941a9f2e0a";
            var mockTransactionsResponseDto = new PostGetTransactionsResponseMockDto();

            _walletClientMock.Setup(client => client.Post<WalletResponseDto>("wallet/get-wallets", It.IsAny<FindConsumerWalletRequestDto>()))
            .ReturnsAsync(mockdto);
            _walletClientMock.Setup(client => client.Post<PostGetTransactionsResponseDto>("transaction/get-transactions", It.IsAny<PostGetTransactionsRequestDto>()))
            .ReturnsAsync(mockTransactionsResponseDto);
            var liveBalance = new ExternalSyncWalletResponseDtoMock();

            var consumerRequest = new BaseRequestDto { consumerCode = ConsumerCode };

            FisGetFundingDescriptionResponseDto walletResponseDto = GetFundingDescriptionResponse();

            _fisClient.Setup(client => client.Post<FisGetFundingDescriptionResponseDto>("get-funding-description", It.IsAny<FisGetFundingDescriptionRequestDto>()))
                          .ReturnsAsync(walletResponseDto);
            _userClient.Setup(client => client.Post<GetConsumerResponseDto>("consumer/get-consumer", It.IsAny<BaseRequestDto>()))
                      .ReturnsAsync(new GetConsumerResponseMockDto());
            _fisClient.Setup(client => client.Post<ExternalSyncWalletResponseDto>("fis/get-purse-balances", It.IsAny<ExternalSyncWalletRequestDto>()))
                          .ReturnsAsync(liveBalance);

            _tenantServiceMock.Setup(x => x.GetTenantByConsumerCode(It.IsAny<string>())).ReturnsAsync(new GetTenantResponseMockDto());
            var CardTransactionsDto = new CardTransactionsResponseMockDto();
            _fisClient.Setup(m => m.Post<CardTransactionsResponseDto>("fis/card-transactions", It.IsAny<CardTransactionsRequestDto>()))
                .ReturnsAsync(CardTransactionsDto);

            // Act
            var result = await _walletService.GetTransactions(requestDto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Transaction.ContainsKey("Today"));

        }
        [Fact]
        public async Task GetTransactions_Returnsnull()
        {
            // Arrange
            var requestDto = new PostGetTransactionsRequestMockDto();
            WalletResponseMockDto mockdto = new WalletResponseMockDto();
            var mockWalletDetailDto = mockdto.walletDetailDto;

            string ConsumerCode = "cmr-c457c5257c59451d8a93ea941a9f2e0a";
            var mockTransactionsResponseDto = new PostGetTransactionsResponseMockDto();

            _walletClientMock.Setup(client => client.Post<WalletResponseDto>("wallet/get-wallets", It.IsAny<FindConsumerWalletRequestDto>()))
            .ReturnsAsync(mockdto);
            _walletClientMock.Setup(client => client.Post<PostGetTransactionsResponseDto>("transaction/get-transactions", It.IsAny<PostGetTransactionsRequestDto>()))
            .ReturnsAsync(mockTransactionsResponseDto);
            var liveBalance = new ExternalSyncWalletResponseDtoMock();

            var consumerRequest = new BaseRequestDto { consumerCode = ConsumerCode };


            _userClient.Setup(client => client.Post<GetConsumerResponseDto>("consumer/get-consumer", It.IsAny<BaseRequestDto>()))
                      .ReturnsAsync(new GetConsumerResponseMockDto());
            _fisClient.Setup(client => client.Post<ExternalSyncWalletResponseDto>("fis/get-purse-balances", It.IsAny<ExternalSyncWalletRequestDto>()))
                          .ReturnsAsync(liveBalance);


            _tenantServiceMock.Setup(x => x.GetTenantByConsumerCode(It.IsAny<string>())).ReturnsAsync(new GetTenantResponseDto
            {
                Tenant = new TenantDto
                {
                    TenantId = 1,
                    SponsorId = 1,
                    TenantCode = "ten-ecada21e57154928a2bb959e8365b8b4",
                    PlanYear = 2023,
                    PeriodStartTs = DateTime.UtcNow,
                    PartnerCode = "par-7e92b06aa4fe405198d27d2427bf3de4",
                    RecommendedTask = false,
                    TenantAttribute = "success",
                    SelfReport = true,
                    EnableServerLogin = false,
                    DirectLogin = false,
                    TenantName = "SunnyTenant",
                    ApiKey = "19afb4b6-dvtu-5869-ndvr-025d2a617b78",
                    CreateTs = DateTime.Now,
                    UpdateTs = DateTime.Now,
                    CreateUser = "sunny",
                    UpdateUser = "sunny rewards",
                    DeleteNbr = 0
                }
            }
);
            var CardTransactionsDto = new CardTransactionsResponseMockDto();
            _fisClient.Setup(m => m.Post<CardTransactionsResponseDto>("fis/card-transactions", It.IsAny<CardTransactionsRequestDto>()))
                .ReturnsAsync(CardTransactionsDto);
            var tenantAccount = new TenantAccountMockDto();
            _fisClient.Setup(m => m.Post<TenantAccountDto>("TenantAccount/get-tenant-account", It.IsAny<TenantAccountCreateRequestDto>()));

            // Act
            var result = await _walletService.GetTransactions(requestDto);

            // Assert
            Assert.NotNull(result);

        }
        [Fact]
        public void GetMappingValue_WithValidKey_ShouldReturnMappedValue()
        {
            // Arrange
            string key = "ValueLoad";
            string expectedMapping = "FUND";

            // Act
            string actualMapping = InvokeGetMappingValue(key);

            // Assert
            Assert.Equal(expectedMapping, actualMapping);
        }

        [Fact]
        public void GetMappingValue_WithInvalidKey_ShouldReturnNull()
        {
            // Arrange
            string key = "InvalidType";

            // Act
            string actualMapping = InvokeGetMappingValue(key);

            // Assert
            Assert.Null(actualMapping);
        }

        private string InvokeGetMappingValue(string key)
        {
            // Since GetMappingValue is private static, we use reflection to invoke it for testing purposes
            MethodInfo methodInfo = typeof(WalletService).GetMethod("GetMappingValue", BindingFlags.Static | BindingFlags.NonPublic);
            if (methodInfo != null)
            {
                return (string)methodInfo.Invoke(null, new object[] { key });
            }
            throw new InvalidOperationException("GetMappingValue method not found.");
        }
        [Fact]
        public async Task GetFundDescription_Successful()
        {
            // Arrange
            var requestDto = new FindConsumerWalletRequestMockDto();
            WalletResponseMockDto mockdto = new WalletResponseMockDto();
            var mockWalletDetailDto = mockdto.walletDetailDto;

            string ConsumerCode = "cmr-c457c5257c59451d8a93ea941a9f2e0a";
            var mockTransactionsResponseDto = new PostGetTransactionsResponseMockDto();

            _walletClientMock.Setup(client => client.Post<WalletResponseDto>("wallet/get-wallets", It.IsAny<FindConsumerWalletRequestDto>()))
            .ReturnsAsync(mockdto);
            _walletClientMock.Setup(client => client.Post<PostGetTransactionsResponseDto>("transaction/get-transactions", It.IsAny<PostGetTransactionsRequestDto>()))
            .ReturnsAsync(mockTransactionsResponseDto);
            var liveBalance = new ExternalSyncWalletResponseDtoMock();

            var consumerRequest = new BaseRequestDto { consumerCode = ConsumerCode };


            _userClient.Setup(client => client.Post<GetConsumerResponseDto>("consumer/get-consumer", It.IsAny<BaseRequestDto>()))
                      .ReturnsAsync(new GetConsumerResponseMockDto());
            _fisClient.Setup(client => client.Post<ExternalSyncWalletResponseDto>("fis/get-purse-balances", It.IsAny<ExternalSyncWalletRequestDto>()))
                          .ReturnsAsync(liveBalance);


            _tenantServiceMock.Setup(x => x.GetTenantByConsumerCode(It.IsAny<string>())).ReturnsAsync(new GetTenantResponseDto
            {
                Tenant = new TenantDto
                {
                    TenantId = 1,
                    SponsorId = 1,
                    TenantCode = "ten-ecada21e57154928a2bb959e8365b8b4",
                    PlanYear = 2023,
                    PeriodStartTs = DateTime.UtcNow,
                    PartnerCode = "par-7e92b06aa4fe405198d27d2427bf3de4",
                    RecommendedTask = false,
                    TenantAttribute = "{\"pickAPurseOnboardingEnabled\":false}",
                    SelfReport = true,
                    EnableServerLogin = false,
                    DirectLogin = false,
                    TenantName = "SunnyTenant",
                    ApiKey = "19afb4b6-dvtu-5869-ndvr-025d2a617b78",
                    CreateTs = DateTime.Now,
                    UpdateTs = DateTime.Now,
                    CreateUser = "sunny",
                    UpdateUser = "sunny rewards",
                    DeleteNbr = 0
                }
            }
);
            var CardTransactionsDto = new CardTransactionsResponseMockDto();
            _fisClient.Setup(m => m.Post<CardTransactionsResponseDto>("fis/card-transactions", It.IsAny<CardTransactionsRequestDto>()))
                .ReturnsAsync(CardTransactionsDto);

            FisGetFundingDescriptionResponseDto walletResponseDto = GetFundingDescriptionResponse();
            _walletClientMock.Setup(client => client.Post<BaseResponseDto>("wallet/update-wallet-balance", It.IsAny<List<WalletModel>>()))
   .ReturnsAsync(new BaseResponseDto());

            _fisClient.Setup(client => client.Post<FisGetFundingDescriptionResponseDto>("get-funding-description", It.IsAny<FisGetFundingDescriptionRequestDto>()))
                          .ReturnsAsync(walletResponseDto);

            var walletService = new WalletService(_walletServiceLogger.Object, _walletClientMock.Object, _taskClient.Object,
                _tenantServiceMock.Object, _fisClient.Object, _userClient.Object, _vault.Object);

            // Act
            var result = await walletService.GetWallets(requestDto);

            // Assert
            Assert.True(result.walletDetailDto != null);
        }
        [Fact]
        public async Task GetFundDescription_NotFound()
        {
            // Arrange // Arrange
            var requestDto = new FindConsumerWalletRequestMockDto();
            WalletResponseMockDto mockdto = new WalletResponseMockDto();
            var mockWalletDetailDto = mockdto.walletDetailDto;

            string ConsumerCode = "cmr-c457c5257c59451d8a93ea941a9f2e0a";
            var mockTransactionsResponseDto = new PostGetTransactionsResponseMockDto();

            _walletClientMock.Setup(client => client.Post<WalletResponseDto>("wallet/get-wallets", It.IsAny<FindConsumerWalletRequestDto>()))
            .ReturnsAsync(mockdto);
            _walletClientMock.Setup(client => client.Post<PostGetTransactionsResponseDto>("transaction/get-transactions", It.IsAny<PostGetTransactionsRequestDto>()))
            .ReturnsAsync(mockTransactionsResponseDto);
            var liveBalance = new ExternalSyncWalletResponseDtoMock();

            var consumerRequest = new BaseRequestDto { consumerCode = ConsumerCode };


            _userClient.Setup(client => client.Post<GetConsumerResponseDto>("consumer/get-consumer", It.IsAny<BaseRequestDto>()))
                      .ReturnsAsync(new GetConsumerResponseMockDto());
            _fisClient.Setup(client => client.Post<ExternalSyncWalletResponseDto>("fis/get-purse-balances", It.IsAny<ExternalSyncWalletRequestDto>()))
                          .ReturnsAsync(liveBalance);


            _tenantServiceMock.Setup(x => x.GetTenantByConsumerCode(It.IsAny<string>())).ReturnsAsync(new GetTenantResponseDto
            {
                Tenant = new TenantDto
                {
                    TenantId = 1,
                    SponsorId = 1,
                    TenantCode = "ten-ecada21e57154928a2bb959e8365b8b4",
                    PlanYear = 2023,
                    PeriodStartTs = DateTime.UtcNow,
                    PartnerCode = "par-7e92b06aa4fe405198d27d2427bf3de4",
                    RecommendedTask = false,
                    TenantAttribute = "{\"pickAPurseOnboardingEnabled\":false}",
                    SelfReport = true,
                    EnableServerLogin = false,
                    DirectLogin = false,
                    TenantName = "SunnyTenant",
                    ApiKey = "19afb4b6-dvtu-5869-ndvr-025d2a617b78",
                    CreateTs = DateTime.Now,
                    UpdateTs = DateTime.Now,
                    CreateUser = "sunny",
                    UpdateUser = "sunny rewards",
                    DeleteNbr = 0
                }
            }
);
            var CardTransactionsDto = new CardTransactionsResponseMockDto();
            _fisClient.Setup(m => m.Post<CardTransactionsResponseDto>("fis/card-transactions", It.IsAny<CardTransactionsRequestDto>()))
                .ReturnsAsync(CardTransactionsDto);

            FisGetFundingDescriptionResponseDto walletResponseDto = GetFundingDescriptionResponse();
            _walletClientMock.Setup(client => client.Post<BaseResponseDto>("wallet/update-wallet-balance", It.IsAny<List<WalletModel>>()))
   .ReturnsAsync(new BaseResponseDto());

            _fisClient.Setup(client => client.Post<FisGetFundingDescriptionResponseDto>("/get-funding-description", It.IsAny<FisGetFundingDescriptionRequestDto>()))
                          .ReturnsAsync(walletResponseDto);

            var walletService = new WalletService(_walletServiceLogger.Object, _walletClientMock.Object, _taskClient.Object, _tenantServiceMock.Object,
                _fisClient.Object, _userClient.Object, _vault.Object);

            // Act
            var result = await walletService.GetWallets(requestDto);

            // Assert
            Assert.True(result.walletDetailDto != null);
        }

        [Fact]
        public async Task GetConsumerBenefitsWalletTypes_ReturnsOkResult_WhenResponseIsSuccessful()
        {
            // Arrange
            var request = new ConsumerBenefitsWalletTypesRequestDto
            {
                TenantCode = "tenant1",
                ConsumerCode = "consumer1"
            };

            var mockResponse = new ConsumerBenefitsWalletTypesResponseDto
            {
                BenefitsWalletTypes = new List<ConsumerBenefitWalletTypeDto>
            {
                new ConsumerBenefitWalletTypeDto
                {
                    PurseLabel = "Purse1",
                    WalletType = "WalletType1",
                    RedemptionTarget = true,
                    IsFilteredSpend = true
                }
            }
            };

            _fisClient
                .Setup(client => client.Post<ConsumerBenefitsWalletTypesResponseDto>(It.IsAny<string>(), It.IsAny<object>()))
                .ReturnsAsync(mockResponse);

            // Act
            var result = await _walletService.GetConsumerBenefitsWalletTypesAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.BenefitsWalletTypes);
            Assert.Equal("Purse1", result.BenefitsWalletTypes[0].PurseLabel);
        }

        [Fact]
        public async Task GetConsumerBenefitsWalletTypes_ReturnsInternalServerError_WhenExceptionIsThrown()
        {
            // Arrange
            var request = new ConsumerBenefitsWalletTypesRequestDto { TenantCode = "Tenant1", ConsumerCode = "Consumer1" };
            _fisClient.Setup(client => client.Post<ConsumerBenefitsWalletTypesResponseDto>("consumer-benefits-wallet-types", It.IsAny<ConsumerBenefitsWalletTypesRequestDto>()))
                          .ThrowsAsync(new System.Exception("Test exception"));
            // Act
            var result = await _walletController.GetConsumerBenefitsWalletTypes(request);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, statusCodeResult.StatusCode);
            var response = Assert.IsType<ConsumerBenefitsWalletTypesResponseDto>(statusCodeResult.Value);
            Assert.Equal(500, response.ErrorCode);
        }

        [Fact]
        public async Task GetConsumerBenefitsWalletTypesAsync_ReturnsError_WhenConsumerConfigWalletTypesIsNull()
        {
            // Arrange
            var request = new ConsumerBenefitsWalletTypesRequestDto
            {
                TenantCode = "tenant1",
                ConsumerCode = "consumer1"
            };

            _fisClient
                .Setup(client => client.Post<ConsumerBenefitsWalletTypesResponseDto>(It.IsAny<string>(), It.IsAny<object>()))
                .ReturnsAsync((ConsumerBenefitsWalletTypesResponseDto)null);

            // Act
            var result = await _walletService.GetConsumerBenefitsWalletTypesAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result.BenefitsWalletTypes);
            Assert.Equal(StatusCodes.Status500InternalServerError, result.ErrorCode);
        }

        [Fact]
        public async Task GetConsumerBenefitsWalletTypesAsync_ReturnsError_WhenTenantAccountIsNull()
        {
            // Arrange
            var request = new ConsumerBenefitsWalletTypesRequestDto
            {
                TenantCode = "tenant1",
                ConsumerCode = "consumer1"
            };

            var mockResponse = new ConsumerBenefitsWalletTypesResponseDto
            {
                BenefitsWalletTypes = new List<ConsumerBenefitWalletTypeDto>()
            };

            _fisClient
                .Setup(client => client.Post<ConsumerBenefitsWalletTypesResponseDto>(It.IsAny<string>(), It.IsAny<object>()))
                .ReturnsAsync(mockResponse);

            _fisClient
                .Setup(client => client.Post<TenantAccountDto>(It.IsAny<string>(), It.IsAny<object>()))
                .ReturnsAsync((TenantAccountDto)null);

            // Act
            var result = await _walletService.GetConsumerBenefitsWalletTypesAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, result.ErrorCode);
        }

        [Fact]
        public async Task GetConsumerBenefitsWalletTypesAsync_ReturnsError_WhenTenantConfigJsonIsNull()
        {
            // Arrange
            var request = new ConsumerBenefitsWalletTypesRequestDto
            {
                TenantCode = "tenant1",
                ConsumerCode = "consumer1"
            };

            var tenantAccount = new TenantAccountDto
            {
                TenantConfigJson = null
            };

            _fisClient
                .Setup(client => client.Post<ConsumerBenefitsWalletTypesResponseDto>(It.IsAny<string>(), It.IsAny<object>()))
                .ReturnsAsync((ConsumerBenefitsWalletTypesResponseDto)null);

            _fisClient
                .Setup(client => client.Post<TenantAccountDto>(It.IsAny<string>(), It.IsAny<object>()))
                .ReturnsAsync(tenantAccount);

            // Act
            var result = await _walletService.GetConsumerBenefitsWalletTypesAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, result.ErrorCode);
        }

        [Fact]
        public async Task GetConsumerBenefitsWalletTypesAsync_LogsError_WhenExceptionOccurs()
        {
            // Arrange
            var request = new ConsumerBenefitsWalletTypesRequestDto
            {
                TenantCode = "tenant1",
                ConsumerCode = "consumer1"
            };

            _fisClient
                .Setup(client => client.Post<ConsumerBenefitsWalletTypesResponseDto>(It.IsAny<string>(), It.IsAny<object>()))
                .ThrowsAsync(new Exception("Test exception"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _walletService.GetConsumerBenefitsWalletTypesAsync(request));
            Assert.Equal("Test exception", exception.Message);
        }

        [Fact]
        public async Task GetConsumerBenefitsWalletTypesAsync_ShouldReturnFilteredWalletTypes_WhenValidDataExists()
        {
            // Arrange
            var request = new ConsumerBenefitsWalletTypesRequestDto { TenantCode = "Tenant1", ConsumerCode = "Consumer1" };
            var tenantAccount = new TenantAccountDto
            {
                TenantConfigJson = "{\"PurseConfig\": {\"Purses\": [{\"PurseLabel\": \"Purse1\", \"PurseWalletType\": \"Type1\", \"PickAPurseStatus\": \"DISABLED\", \"RedemptionTarget\": true, \"IsFilteredSpend\": true}, {\"PurseLabel\": \"Purse2\", \"PurseWalletType\": \"Type2\", \"PickAPurseStatus\": \"Disabled\", \"RedemptionTarget\": true, \"IsFilteredSpend\": false}]}}"
            };

            var expectedResponse = new ConsumerBenefitsWalletTypesResponseDto
            {
                BenefitsWalletTypes = new List<ConsumerBenefitWalletTypeDto>
        {
            new ConsumerBenefitWalletTypeDto
            {
                PurseLabel = "Purse1",
                WalletType = "Type1",
                RedemptionTarget = true,
                IsFilteredSpend = true
            }
        }
            };

            _fisClient.Setup(x => x.Post<ConsumerBenefitsWalletTypesResponseDto>(
                WalletConstants.GetConsumerBenefitWalletTypesAPIUrl, request))
                .ReturnsAsync((ConsumerBenefitsWalletTypesResponseDto)null);

            _fisClient.Setup(x => x.Post<TenantAccountDto>(
                WalletConstants.GetTenantAccountByTenantCode, It.IsAny<TenantAccountCreateRequestDto>()))
                .ReturnsAsync(tenantAccount);

            // Act
            var result = await _walletService.GetConsumerBenefitsWalletTypesAsync(request);

            // Assert

            Assert.NotNull(result);
            Assert.Single(result.BenefitsWalletTypes);
            Assert.Equal("Purse2", result.BenefitsWalletTypes[0].PurseLabel);
            Assert.Equal("Type2", result.BenefitsWalletTypes[0].WalletType);
            Assert.Equal(true, result.BenefitsWalletTypes[0].RedemptionTarget);
            Assert.False(result.BenefitsWalletTypes[0].IsFilteredSpend);
        }

        [Fact]
        public async Task GetConsumerBenefitsWalletTypesAsync_ShouldReturnEmpty_WhenPursesIsNull()
        {
            // Arrange
            var request = new ConsumerBenefitsWalletTypesRequestDto { TenantCode = "Tenant1", ConsumerCode = "Consumer1" };
            var tenantAccount = new TenantAccountDto
            {
                TenantConfigJson = "{\"PurseConfig\": {\"Purses\": null}}"
            };

            var expectedResponse = new ConsumerBenefitsWalletTypesResponseDto
            {
                BenefitsWalletTypes = new List<ConsumerBenefitWalletTypeDto>()
            };

            _fisClient.Setup(x => x.Post<ConsumerBenefitsWalletTypesResponseDto>(
                WalletConstants.GetConsumerBenefitWalletTypesAPIUrl, request))
                .ReturnsAsync((ConsumerBenefitsWalletTypesResponseDto)null);

            _fisClient.Setup(x => x.Post<TenantAccountDto>(
                WalletConstants.GetTenantAccountByTenantCode, It.IsAny<TenantAccountCreateRequestDto>()))
                .ReturnsAsync(tenantAccount);

            // Act
            var result = await _walletService.GetConsumerBenefitsWalletTypesAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result.BenefitsWalletTypes);
        }

        [Fact]
        public async Task GetConsumerBenefitsWalletTypesAsync_ShouldReturnEmpty_WhenPurseConfigIsNull()
        {
            // Arrange
            var request = new ConsumerBenefitsWalletTypesRequestDto { TenantCode = "Tenant1", ConsumerCode = "Consumer1" };
            var tenantAccount = new TenantAccountDto
            {
                TenantConfigJson = "{\"PurseConfig\": null}"
            };

            var expectedResponse = new ConsumerBenefitsWalletTypesResponseDto
            {
                BenefitsWalletTypes = new List<ConsumerBenefitWalletTypeDto>()
            };

            _fisClient.Setup(x => x.Post<ConsumerBenefitsWalletTypesResponseDto>(
                WalletConstants.GetConsumerBenefitWalletTypesAPIUrl, request))
                .ReturnsAsync((ConsumerBenefitsWalletTypesResponseDto)null);

            _fisClient.Setup(x => x.Post<TenantAccountDto>(
                WalletConstants.GetTenantAccountByTenantCode, It.IsAny<TenantAccountCreateRequestDto>()))
                .ReturnsAsync(tenantAccount);

            // Act
            var result = await _walletService.GetConsumerBenefitsWalletTypesAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result.BenefitsWalletTypes);
        }

    }
}
