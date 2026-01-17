using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using Sunny.Benefits.Bff.Api.Controllers;
using Sunny.Benefits.Bff.Core.Domain.Constants;
using Sunny.Benefits.Bff.Core.Domain.Dtos;
using Sunny.Benefits.Bff.Infrastructure.Helpers;
using Sunny.Benefits.Bff.Infrastructure.Helpers.Interface;
using Sunny.Benefits.Bff.Infrastructure.HttpClients.Interfaces;
using Sunny.Benefits.Bff.Infrastructure.Repositories.Interfaces;
using Sunny.Benefits.Bff.Infrastructure.Services;
using Sunny.Benefits.Bff.Infrastructure.Services.Interfaces;
using Sunny.Benefits.Bff.UnitTest.Fixtures.MockDtos;
using Sunny.Benefits.Bff.UnitTest.HttpClients;
using SunnyBenefits.Fis.Core.Domain.Dtos;
using SunnyBenefits.Fis.Core.Domain.Dtos.Json;
using SunnyRewards.Helios.Admin.Core.Domain.Dtos;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.Common.Core.Extensions;
using SunnyRewards.Helios.Tenant.Core.Domain.Dtos;
using SunnyRewards.Helios.Tenant.Core.Domain.Dtos.Json;
using SunnyRewards.Helios.User.Core.Domain.Dtos;
using SunnyRewards.Helios.User.Core.Domain.enums;
using SunnyRewards.Helios.Wallet.Core.Domain.Dtos;
using Xunit;
using FundTransferToPurseRequestDto = Sunny.Benefits.Bff.Core.Domain.Dtos.FundTransferToPurseRequestDto;

namespace Sunny.Benefits.Bff.UnitTest.Controllers
{
    public class CardOperationControllerTests
    {
        private readonly Mock<ILogger<CardOperationController>> _cardOperationControllerLogger;
        private readonly Mock<ILogger<CardOperationService>> _cardOperationServiceLogger;
        private readonly Mock<ILogger<CardReissueService>> _cardReissueServiceLogger;
        private readonly ICardOperationService _cardOperationService;
        private readonly CardOperationController _cardOperationController;
        private readonly Mock<IFisClient> _fisClient;
        private readonly Mock<ILogger<ReplaceCardService>> _replaceCardServiceLogger;
        private readonly IReplaceCardService _replaceCardService;
        private readonly ICardOperationsHelper _cardOperationsHelper;
        private readonly Mock<IPersonHelper> _personHelper;
        private readonly Mock<IAdminClient> _adminClient;
        private readonly Mock<IConfiguration> _configuration;
        private readonly Mock<INotificationHelper> _notificationHelper;
        private readonly Mock<ITenantAccountService> _tenantAccountService;

        private readonly ICardReissueService _cardReissueService;
        public CardOperationControllerTests()
        {
            _cardOperationControllerLogger = new Mock<ILogger<CardOperationController>>();
            _cardOperationServiceLogger = new Mock<ILogger<CardOperationService>>();
            _cardReissueServiceLogger=new Mock<ILogger<CardReissueService>>();
            _personHelper = new Mock<IPersonHelper>();
            _fisClient = new FisClientMock();
            _adminClient = new Mock<IAdminClient>();
            _configuration = new Mock<IConfiguration>();
            _cardOperationsHelper = new CardOperationsHelper();
            _notificationHelper = new Mock<INotificationHelper>();
            _tenantAccountService = new Mock<ITenantAccountService>();
            _cardOperationService = new CardOperationService(_cardOperationServiceLogger.Object, _fisClient.Object,_cardOperationsHelper , _personHelper.Object, _adminClient.Object , _configuration.Object, _notificationHelper.Object, _tenantAccountService.Object);
            _replaceCardServiceLogger = new Mock<ILogger<ReplaceCardService>>();

            
            _replaceCardService = new ReplaceCardService(_replaceCardServiceLogger.Object, _fisClient.Object, _cardOperationsHelper,_personHelper.Object);
            _cardReissueService=new CardReissueService(_cardReissueServiceLogger.Object, _fisClient.Object, _cardOperationsHelper);
            _cardOperationController = new CardOperationController(_cardOperationService, _cardOperationControllerLogger.Object,_cardReissueService, _replaceCardService);
            _configuration.Setup(c => c.GetSection("Reward_Wallet_Type_Code").Value).Returns("456");
            _configuration.Setup(c => c.GetSection("Healthy_Living_Wallet_Type_Code").Value).Returns("956");
        }

        [Fact]
        public async Task ExecuteCardOperation_Should_Return_Success_Result()
        {

            // Arrange
            var requestDto = new ExecuteCardOperationRequestMockDto();
            var responseDto = new CardOperationResponseDto()
            {
                FisResponse = "1 SUSPEND^"
            };
            _fisClient.Setup(client => client.Post<CardOperationResponseDto>(CardOperationConstants.FisCardOperationApiUrl, It.IsAny<CardOperationRequestDto>()))
                .ReturnsAsync(responseDto);
            _notificationHelper.Setup(helper => helper.ProcessNotification(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _cardOperationController.ExecuteCardOperation(requestDto) as ObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<ExecuteCardOperationResponseDto>(result.Value);
            var response = (ExecuteCardOperationResponseDto)result.Value;
            Assert.True(response.Success);
            Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
        }

        [Fact]
        public async Task ExecuteCardOperation_Should_call_Update_OnBoarding_When_Activate_CardLast4_Digits_Not_Verified()
        {

            // Arrange
            var requestDto = new ExecuteCardOperationRequestMockDto();
            requestDto.CardOperation = "ACTIVATE";

            var responseDto = new CardOperationResponseDto()
            {
                FisResponse = "1 ACTIVATE^"
            };

            var getPersonAndConsumerResponseDto = new GetPersonAndConsumerResponseDto()
            {
                Consumer = new ConsumerDto() { OnBoardingState = OnboardingState.EMAIL_VERIFIED.ToString() },
                Person = new PersonDto()
            };
            _fisClient.Setup(client => client.Post<CardOperationResponseDto>("fis/card-operation", It.IsAny<CardOperationRequestDto>()))
                .ReturnsAsync(responseDto);

            _personHelper.Setup(x => x.UpdateOnBoardingState(It.IsAny<UpdateOnboardingStateDto>())).ReturnsAsync(true);
            _personHelper.Setup(x => x.GetPersonDetails(It.IsAny<GetConsumerRequestDto>())).ReturnsAsync(getPersonAndConsumerResponseDto);
            // Act
            var result = await _cardOperationController.ExecuteCardOperation(requestDto) as ObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<ExecuteCardOperationResponseDto>(result.Value);
            var response = (ExecuteCardOperationResponseDto)result.Value;
            Assert.False(response.Success);
            Assert.Equal(StatusCodes.Status409Conflict, result.StatusCode);
        }
        [Fact]
        public async Task ExecuteCardOperation_Should_call_Update_OnBoarding_When_Activate()
        {

            // Arrange
            var requestDto = new ExecuteCardOperationRequestMockDto();
            requestDto.CardOperation = "ACTIVATE";

            var responseDto = new CardOperationResponseDto()
            {
                FisResponse = "1 ACTIVATE^"
            };
            var getPersonAndConsumerResponseDto = new GetPersonAndConsumerResponseDto()
            {
                Consumer = new ConsumerDto() { OnBoardingState = OnboardingState.CARD_LAST_4_VERIFIED.ToString() },
                Person = new PersonDto()
                {
                    
                }
            };
            _fisClient.Setup(client => client.Post<CardOperationResponseDto>("fis/card-operation", It.IsAny<CardOperationRequestDto>()))
                .ReturnsAsync(responseDto);

            _personHelper.Setup(x => x.UpdateOnBoardingState(It.IsAny<UpdateOnboardingStateDto>())).ReturnsAsync(true);
            _personHelper.Setup(x => x.GetPersonDetails(It.IsAny<GetConsumerRequestDto>())).ReturnsAsync(getPersonAndConsumerResponseDto);

            // Act
            var result = await _cardOperationController.ExecuteCardOperation(requestDto) as ObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<ExecuteCardOperationResponseDto>(result.Value);
            var response = (ExecuteCardOperationResponseDto)result.Value;
            Assert.True(response.Success);
            Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
        }

        [Fact]
        public async Task ExecuteCardOperation_Should_call_Update_OnBoarding_When_Activate_FundTranster()
        {

            // Arrange
            var requestDto = new ExecuteCardOperationRequestMockDto();
            requestDto.CardOperation = "ACTIVATE";

            var responseDto = new CardOperationResponseDto()
            {
                FisResponse = "1 ACTIVATE^"
            };
            var getPersonAndConsumerResponseDto = new GetPersonAndConsumerResponseDto()
            {
                Consumer = new ConsumerDto() { OnBoardingState = OnboardingState.CARD_LAST_4_VERIFIED.ToString() },
                Person = new PersonDto()
                {

                }
            };
            _fisClient.Setup(client => client.Post<CardOperationResponseDto>("fis/card-operation", It.IsAny<CardOperationRequestDto>()))
                .ReturnsAsync(responseDto);

            IDictionary<string, long> parameters = new Dictionary<string, long>();
            var tenantAttribute = new TenantAttribute() { SupportLiveTransferToRewardsPurse = true };
            var tenantOption = new TenantOption() { Apps = new List<string>() { "BENEFITS" } };

            var now = DateTime.UtcNow;

            var tenantConfig = new TenantConfig
            {
                PurseConfig = new PurseConfig
                {
                    Purses = new List<Purse>
                    {
                        new Purse
                        {
                            PurseWalletType = "HL_PURSE",
                            ActiveStartTs = now.AddDays(-1),
                            RedeemEndTs = now.AddDays(1)
                        },
                        new Purse
                        {
                            PurseWalletType = "EXPIRED_PURSE",
                            ActiveStartTs = now.AddDays(-10),
                            RedeemEndTs = now.AddDays(-1)
                        }
                    }
                }
            };

            var tenantConfigJson = JsonConvert.SerializeObject(tenantConfig);

            _tenantAccountService
                .Setup(x => x.GetTenantAccount(
                    It.Is<TenantAccountCreateRequestDto>(r =>
                        r.TenantCode == requestDto.TenantCode)))
                .ReturnsAsync(new TenantAccountDto
                {
                    TenantConfigJson = tenantConfigJson
                });

            _adminClient
            .Setup(client => client.Get<TenantResponseDto>(It.IsAny<String>(), parameters))
            .ReturnsAsync(new TenantResponseDto() { Tenant = new TenantDto() { TenantId = 100 , TenantAttribute = tenantAttribute.ToJson()  , TenantOption = tenantOption.ToJson()}  });

            _adminClient
                .Setup(client => client.Post<ConsumerWalletResponseDto>(AdminConstants.GetAllConsumerRedeemableWallets, It.IsAny<FindConsumerWalletRequestDto>()))
                .ReturnsAsync(new ConsumerWalletResponseDto()
                {
                    ConsumerWalletDetails = new List<ConsumerWalletDetailDto>() {
                    new ConsumerWalletDetailDto() {
                        Wallet = new WalletDto() { WalletId = 1 , Balance = 100, ActiveStartTs = now.AddDays(-1), RedeemEndTs = now.AddDays(1) } ,
                        WalletType = new WalletTypeDto(){WalletTypeCode = "456" }
                    }  
                    }
                });

            _adminClient
                .Setup(client => client.Post<BaseResponseDto>("fund-transfer", It.IsAny<FundTransferToPurseRequestDto>()))
                .ReturnsAsync(new BaseResponseDto());

            _personHelper.Setup(x => x.UpdateOnBoardingState(It.IsAny<UpdateOnboardingStateDto>())).ReturnsAsync(true);
            _personHelper.Setup(x => x.GetPersonDetails(It.IsAny<GetConsumerRequestDto>())).ReturnsAsync(getPersonAndConsumerResponseDto);

            // Act
            var result = await _cardOperationController.ExecuteCardOperation(requestDto) as ObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<ExecuteCardOperationResponseDto>(result.Value);
            var response = (ExecuteCardOperationResponseDto)result.Value;
            Assert.True(response.Success);
            Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
        }


        [Fact]
        public async Task ExecuteCardOperation_Should_call_Update_OnBoarding_When_Activate_But_Card_Not_Activated()
        {

            // Arrange
            var requestDto = new ExecuteCardOperationRequestMockDto();
            requestDto.CardOperation = "ACTIVATE";

            var responseDto = new CardOperationResponseDto()
            {
                FisResponse = "1 ACTIVATE^"

            };
            var getPersonAndConsumerResponseDto = new GetPersonAndConsumerResponseDto()
            {
                Consumer = new ConsumerDto() { OnBoardingState = OnboardingState.EMAIL_VERIFIED.ToString() },
                Person = new PersonDto()
            };
            _fisClient.Setup(client => client.Post<CardOperationResponseDto>("fis/card-operation", It.IsAny<CardOperationRequestDto>()))
                .ReturnsAsync(responseDto);

            _personHelper.Setup(x => x.UpdateOnBoardingState(It.IsAny<UpdateOnboardingStateDto>())).ReturnsAsync(false);
            _personHelper.Setup(x => x.GetPersonDetails(It.IsAny<GetConsumerRequestDto>())).ReturnsAsync(getPersonAndConsumerResponseDto);
            // Act
            var result = await _cardOperationController.ExecuteCardOperation(requestDto) as ObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<ExecuteCardOperationResponseDto>(result.Value);
            var response = (ExecuteCardOperationResponseDto)result.Value;
            Assert.False(response.Success);
            Assert.Equal(StatusCodes.Status409Conflict, result.StatusCode);
        }

        [Fact]
        public async Task ExecuteCardOperation_Should_Return_Bad_Request_Result()
        {

            // Arrange
            var requestDto = new ExecuteCardOperationRequestMockDto();
            requestDto.CardOperation = "InvalidOperation";

            // Act
            var result = await _cardOperationController.ExecuteCardOperation(requestDto) as ObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<ExecuteCardOperationResponseDto>(result.Value);
            var response = (ExecuteCardOperationResponseDto)result.Value;
            Assert.False(response.Success);
            Assert.Equal(StatusCodes.Status400BadRequest, result.StatusCode);
        }

        [Fact]
        public async Task ExecuteCardOperation_Should_Return_Internal_Server_Error_Result()
        {

            // Arrange
            var requestDto = new ExecuteCardOperationRequestMockDto();
            var responseDto = new CardOperationResponseDto()
            {
                FisResponse = "1 INVALID^"
            };
            _fisClient.Setup(client => client.Post<CardOperationResponseDto>(CardOperationConstants.FisCardOperationApiUrl, It.IsAny<CardOperationRequestDto>()))
                .ReturnsAsync(responseDto);

            // Act
            var result = await _cardOperationController.ExecuteCardOperation(requestDto) as ObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<ExecuteCardOperationResponseDto>(result.Value);
            var response = (ExecuteCardOperationResponseDto)result.Value;
            Assert.False(response.Success);
            Assert.Equal(StatusCodes.Status500InternalServerError, result.StatusCode);
        }

        [Fact]
        public async Task ExecuteCardOperation_Should_Return_Not_Found_Result()
        {

            // Arrange
            var requestDto = new ExecuteCardOperationRequestMockDto();
            var responseDto = new CardOperationResponseDto()
            {
                ErrorCode = StatusCodes.Status404NotFound
            };
            _fisClient.Setup(client => client.Post<CardOperationResponseDto>(CardOperationConstants.FisCardOperationApiUrl, It.IsAny<CardOperationRequestDto>()))
                .ReturnsAsync(responseDto);

            // Act
            var result = await _cardOperationController.ExecuteCardOperation(requestDto) as ObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<ExecuteCardOperationResponseDto>(result.Value);
            var response = (ExecuteCardOperationResponseDto)result.Value;
            Assert.False(response.Success);
            Assert.Equal(StatusCodes.Status404NotFound, result.StatusCode);
        }

        [Fact]
        public async Task GetCardStatus_Should_Return_Success_Result()
        {

            // Arrange
            var requestDto = new GetCardStatusRequestMockDto();
            var responseDto = new CardOperationResponseDto()
            {
                FisResponse = "1 READY|09/29|<NULL>|Ready for activation^"
            };
            _fisClient.Setup(client => client.Post<CardOperationResponseDto>(CardOperationConstants.FisCardStatusApiUrl, It.IsAny<GetCardStatusRequestDto>()))
                .ReturnsAsync(responseDto);

            // Act
            var result = await _cardOperationController.GetCardStatus(requestDto) as ObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<GetCardStatusResponseDto>(result.Value);
            var response = (GetCardStatusResponseDto)result.Value;
            Assert.Equal("ReadyForActivation", response.CardStatus);
            Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
        }

        [Fact]
        public async Task GetCardStatus_Should_Return_Not_Found_Result()
        {

            // Arrange
            var requestDto = new GetCardStatusRequestMockDto();
            var responseDto = new CardOperationResponseDto()
            {
                ErrorCode = StatusCodes.Status404NotFound
            };
            _fisClient.Setup(client => client.Post<CardOperationResponseDto>(CardOperationConstants.FisCardStatusApiUrl, It.IsAny<GetCardStatusRequestDto>()))
                .ReturnsAsync(responseDto);

            // Act
            var result = await _cardOperationController.GetCardStatus(requestDto) as ObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<GetCardStatusResponseDto>(result.Value);
            var response = (GetCardStatusResponseDto)result.Value;
            Assert.Equal(StatusCodes.Status404NotFound, result.StatusCode);
        }

        [Fact]
        public async Task GetCardStatus_Should_Return_Internal_Server_Error_Result_When_FIS_Response_Is_Empty()
        {

            // Arrange
            var requestDto = new GetCardStatusRequestDto();
            var responseDto = new CardOperationResponseDto()
            {
                FisResponse = string.Empty
            };
            _fisClient.Setup(client => client.Post<CardOperationResponseDto>(CardOperationConstants.FisCardStatusApiUrl, It.IsAny<GetCardStatusRequestDto>()))
                .ReturnsAsync(responseDto);

            // Act
            var result = await _cardOperationController.GetCardStatus(requestDto) as ObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<GetCardStatusResponseDto>(result.Value);
            var response = (GetCardStatusResponseDto)result.Value;
            Assert.Equal(StatusCodes.Status500InternalServerError, result.StatusCode);
        }

        [Fact]
        public async Task GetCardStatus_Should_Return_Success_Result_When_Status_Is_Unknown()
        {

            // Arrange
            var requestDto = new GetCardStatusRequestMockDto();
            var responseDto = new CardOperationResponseDto()
            {
                FisResponse = "1 Unknown|09/29|<NULL>^"
            };
            _fisClient.Setup(client => client.Post<CardOperationResponseDto>(CardOperationConstants.FisCardStatusApiUrl, It.IsAny<GetCardStatusRequestDto>()))
                .ReturnsAsync(responseDto);

            // Act
            var result = await _cardOperationController.GetCardStatus(requestDto) as ObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<GetCardStatusResponseDto>(result.Value);
            var response = (GetCardStatusResponseDto)result.Value;
            Assert.Equal("Unknown", response.CardStatus);
            Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
        }
        [Fact]
        public async Task ExecuteCardReissue_Should_Return_Success_Result()
        {

            // Arrange
            var requestDto = new CardReissueRequestMockDto();
            var responseDto = new CardReissueResponseDto();
         

            _fisClient.Setup(client => client.Post<CardReissueResponseDto>(CardOperationConstants.FisCardReissueApiUrl, It.IsAny<CardReissueRequestDto>()))
                .ReturnsAsync(responseDto);
            // ActS
            var result = await _cardOperationController.ExecuteCardReissue(requestDto) as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<ExecuteCardReissueResponseDto>(result.Value);
            var response = (ExecuteCardReissueResponseDto)result.Value;
            Assert.True(response.Success);
            Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
        }

        [Fact]
        public async Task ExecuteCardReissue_Should_Return_NotFound_Result()
        {

            // Arrange
            var requestDto = new CardReissueRequestMockDto();
            var responseDto = new CardReissueResponseDto()
            {
                ErrorCode = StatusCodes.Status404NotFound
            };

            _fisClient.Setup(client => client.Post<CardReissueResponseDto>(CardOperationConstants.FisCardReissueApiUrl, It.IsAny<CardReissueRequestDto>()))
                .ReturnsAsync(responseDto);
            // Act
            var result = await _cardOperationController.ExecuteCardReissue(requestDto) as ObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<ExecuteCardReissueResponseDto>(result.Value);
            var response = (ExecuteCardReissueResponseDto)result.Value;
            Assert.False(response.Success);
            Assert.Equal(StatusCodes.Status404NotFound, result.StatusCode);
        }
        [Fact]
        public async Task ExecuteCardReissue_Should_Return_NotFound_Result_When_tenantcode_isInvalid()
        {

            // Arrange
            var requestDto = new CardReissueRequestMockDto();
            requestDto.TenantCode = "1239875";
            var responseDto = new CardReissueResponseDto()
            {
                ErrorCode = StatusCodes.Status404NotFound
            };

            _fisClient.Setup(client => client.Post<CardReissueResponseDto>(CardOperationConstants.FisCardReissueApiUrl, It.IsAny<CardReissueRequestDto>()))
                .ReturnsAsync(responseDto);
            // Act
            var result = await _cardOperationController.ExecuteCardReissue(requestDto) as ObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<ExecuteCardReissueResponseDto>(result.Value);
            var response = (ExecuteCardReissueResponseDto)result.Value;
            Assert.False(response.Success);
            Assert.Equal(StatusCodes.Status404NotFound, result.StatusCode);
        }

        [Fact]
        public async Task ExecuteCardReissue_Should_Return_NotFound_Result_When_consumercode_isInvalid()
        {

            // Arrange
            var requestDto = new CardReissueRequestMockDto();
            requestDto.ConsumerCode = "123567";
            var responseDto = new CardReissueResponseDto()
            {
                ErrorCode = StatusCodes.Status404NotFound
            };

            _fisClient.Setup(client => client.Post<CardReissueResponseDto>(CardOperationConstants.FisCardReissueApiUrl, It.IsAny<CardReissueRequestDto>()))
                .ReturnsAsync(responseDto);
            // Act
            var result = await _cardOperationController.ExecuteCardReissue(requestDto) as ObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<ExecuteCardReissueResponseDto>(result.Value);
            var response = (ExecuteCardReissueResponseDto)result.Value;
            Assert.False(response.Success);
            Assert.Equal(StatusCodes.Status404NotFound, result.StatusCode);
        }
        [Fact]
        public async Task ExecuteCardReissue_Should_Return_Internal_ServerError_When_Fis_throws_Exception()
        {

            // Arrange
            var requestDto = new CardReissueRequestMockDto();
           
            _fisClient.Setup(client => client.Post<CardReissueResponseDto>(CardOperationConstants.FisCardReissueApiUrl, It.IsAny<CardReissueRequestDto>()))
                .Throws(new Exception("Some thing went wrong"));
            // Act
            var result = await _cardOperationController.ExecuteCardReissue(requestDto) as ObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<ExecuteCardReissueResponseDto>(result.Value);
            var response = (ExecuteCardReissueResponseDto)result.Value;
            Assert.False(response.Success);
            Assert.Equal(StatusCodes.Status500InternalServerError, result.StatusCode);
        }
        [Fact]
        public async Task ExecutereplaceCard_Should_Return_Bad_Request_Result()
        {
            // Arrange
            var requestDto = new ReplaceCardRequestMockDto();
            var fisGetStatusResponse = new CardOperationResponseDto
            {
                FisResponse = "1 ACTIVE|09/29|<NULL>|Active^"
            };

            var responseDto = new CardOperationResponseDto
            {
                FisResponse = "1 LOSTSTOLEN"
            };

            var replaceCardResponse = new ReplaceCardResponseDto
            {
                ErrorCode = StatusCodes.Status400BadRequest,
            };

            _fisClient.Setup(client => client.Post<CardOperationResponseDto>(CardOperationConstants.FisCardOperationApiUrl, It.IsAny<CardOperationRequestDto>()))
                .ReturnsAsync(responseDto);
            _fisClient.Setup(client => client.Post<CardOperationResponseDto>(CardOperationConstants.FisCardStatusApiUrl, It.IsAny<ReplaceCardRequestDto>()))
                .ReturnsAsync(fisGetStatusResponse);

            _fisClient.Setup(client => client.Post<ReplaceCardResponseDto>(CardOperationConstants.FisReplaceCardApiUrl, It.IsAny<ReplaceCardRequestDto>()))
                .ReturnsAsync(replaceCardResponse);

            // Act
            var result = await _cardOperationController.ExecuteCardReplacement(requestDto) as ObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<ExecuteReplaceCardResponseDto>(result.Value);
            var response = (ExecuteReplaceCardResponseDto)result.Value;
            Assert.False(response.Success);
            Assert.Equal(StatusCodes.Status400BadRequest, result.StatusCode);
        }
        [Fact]
        public async Task ExecuteReplaceCard_Should_Return_Internal_Server_Error_Result()
        {

            // Arrange
            var requestDto = new ReplaceCardRequestMockDto();
            var responseDto = new ReplaceCardResponseDto()
            {
                ProxyNumber = string.Empty
            };
            _fisClient.Setup(client => client.Post<ReplaceCardResponseDto>(CardOperationConstants.FisReplaceCardApiUrl, It.IsAny<ReplaceCardRequestDto>()))
                .ReturnsAsync(responseDto);

            // Act
            var result = await _cardOperationController.ExecuteCardReplacement(requestDto) as ObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<ExecuteReplaceCardResponseDto>(result.Value);
            var response = (ExecuteReplaceCardResponseDto)result.Value;
            Assert.Equal(StatusCodes.Status500InternalServerError, result.StatusCode);
        }
        [Fact]
        public async Task ExecuteReplaceCard_Should_Return_Success_Result()
        {
            // Arrange
            var requestDto = new ReplaceCardRequestMockDto();
            var fisGetStatusResponse = new CardOperationResponseDto
            {
                FisResponse = "1 ACTIVE|09/29|<NULL>|Active^"
            };

            var responseDto = new CardOperationResponseDto
            {
               FisResponse = "1 LOSTSTOLEN"
            };

            var replaceCardResponse = new ReplaceCardResponseDto
            {
                ProxyNumber = "3850570779257"
            };
            var executeReplaceCard = new ExecuteCardOperationResponseDto
            {
                Success = true
            };

            _fisClient.Setup(client => client.Post<CardOperationResponseDto>(CardOperationConstants.FisCardOperationApiUrl, It.IsAny<CardOperationRequestDto>()))
                .ReturnsAsync(responseDto);
            _fisClient.Setup(client => client.Post<CardOperationResponseDto>(CardOperationConstants.FisCardStatusApiUrl, It.IsAny<ReplaceCardRequestDto>()))
                .ReturnsAsync(fisGetStatusResponse);

            _fisClient.Setup(client => client.Post<ReplaceCardResponseDto>(CardOperationConstants.FisReplaceCardApiUrl, It.IsAny<ReplaceCardRequestDto>()))
                .ReturnsAsync(replaceCardResponse);
            _fisClient.Setup(client => client.Post<ExecuteCardOperationResponseDto>("3850570779257", It.IsAny<ReplaceCardRequestDto>())).ReturnsAsync(executeReplaceCard);
            _personHelper.Setup(x => x.UpdateOnBoardingState(It.IsAny<UpdateOnboardingStateDto>())).ReturnsAsync(true);


            // Act
            var result = await _cardOperationController.ExecuteCardReplacement(requestDto) as ObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<ExecuteReplaceCardResponseDto>(result.Value);
            var response = (ExecuteReplaceCardResponseDto)result.Value;
            Assert.True(response.Success);
            Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
        }

        [Fact]
        public async Task ExecutereplaceCard_Should_Return_422_Unprocessable_Result()
        {
            // Arrange
            var requestDto = new ReplaceCardRequestMockDto();
            var fisGetStatusResponse = new CardOperationResponseDto
            {
                FisResponse = "1 ACTIVE|09/29|<NULL>|Active^"
            };

            var responseDto = new CardOperationResponseDto
            {
                FisResponse = "0 Authorization failed"
            };

            var replaceCardResponse = new ReplaceCardResponseDto
            {
                ErrorCode = StatusCodes.Status422UnprocessableEntity,
            };

            _fisClient.Setup(client => client.Post<CardOperationResponseDto>(CardOperationConstants.FisCardOperationApiUrl, It.IsAny<CardOperationRequestDto>()))
                .ReturnsAsync(responseDto);
            _fisClient.Setup(client => client.Post<CardOperationResponseDto>(CardOperationConstants.FisCardStatusApiUrl, It.IsAny<ReplaceCardRequestDto>()))
                .ReturnsAsync(fisGetStatusResponse);

            _fisClient.Setup(client => client.Post<ReplaceCardResponseDto>(CardOperationConstants.FisReplaceCardApiUrl, It.IsAny<ReplaceCardRequestDto>()))
                .ReturnsAsync(replaceCardResponse);

            // Act
            var result = await _cardOperationController.ExecuteCardReplacement(requestDto) as ObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<ExecuteReplaceCardResponseDto>(result.Value);
            var response = (ExecuteReplaceCardResponseDto)result.Value;
            Assert.False(response.Success);
            Assert.Equal(StatusCodes.Status422UnprocessableEntity, result.StatusCode);
        }
        [Fact]
        public async Task ExecutereplaceCard_Should_Return_Not_Found_Result()
        {
            // Arrange
            var requestDto = new ReplaceCardRequestMockDto();
            var fisGetStatusResponse = new CardOperationResponseDto
            {
                FisResponse = "1 SUSPENDED|09/29|<NULL>|Suspended^"
            };

            var responseDto = new CardOperationResponseDto
            {
                FisResponse = "1 Active"
            };
            var responseToLostStolen = new CardOperationResponseDto
            {
                FisResponse = "1 LOSTSTOLEN"
            };

            var replaceCardResponse = new ReplaceCardResponseDto
            {
                ErrorCode = StatusCodes.Status404NotFound,
            };

            _fisClient.Setup(client => client.Post<CardOperationResponseDto>(CardOperationConstants.FisCardOperationApiUrl, It.IsAny<CardOperationRequestDto>()))
                .ReturnsAsync(responseDto);
            _fisClient.Setup(client => client.Post<CardOperationResponseDto>(CardOperationConstants.FisCardOperationApiUrl, It.IsAny<CardOperationRequestDto>()))
                .ReturnsAsync(responseToLostStolen);
            _fisClient.Setup(client => client.Post<CardOperationResponseDto>(CardOperationConstants.FisCardStatusApiUrl, It.IsAny<ReplaceCardRequestDto>()))
                .ReturnsAsync(fisGetStatusResponse);

            _fisClient.Setup(client => client.Post<ReplaceCardResponseDto>(CardOperationConstants.FisReplaceCardApiUrl, It.IsAny<ReplaceCardRequestDto>()))
                .ReturnsAsync(replaceCardResponse);

            // Act
            var result = await _cardOperationController.ExecuteCardReplacement(requestDto) as ObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<ExecuteReplaceCardResponseDto>(result.Value);
            var response = (ExecuteReplaceCardResponseDto)result.Value;
            Assert.False(response.Success);
            Assert.Equal(StatusCodes.Status404NotFound, result.StatusCode);
        }
        [Fact]
        public async Task ExecuteReplaceCard_Should_Return_UnProcessable_Entity_Result()
        {

            // Arrange
            var requestDto = new ReplaceCardRequestMockDto();
            var fisGetStatusResponse = new CardOperationResponseDto
            {
                FisResponse = "1 READY|09/29|<NULL>|Ready for activation^"
            };

            var responseDto = new CardOperationResponseDto
            {
                FisResponse = "1 Active"
            };
            var responseToLostStolen = new CardOperationResponseDto
            {
                FisResponse = "1 LOSTSTOLEN"
            };

            var replaceCardResponse = new ReplaceCardResponseDto
            {
                ErrorCode = StatusCodes.Status422UnprocessableEntity
            };

            _fisClient.Setup(client => client.Post<CardOperationResponseDto>(CardOperationConstants.FisCardOperationApiUrl, It.IsAny<CardOperationRequestDto>()))
                .ReturnsAsync(responseDto);
            _fisClient.Setup(client => client.Post<CardOperationResponseDto>(CardOperationConstants.FisCardOperationApiUrl, It.IsAny<CardOperationRequestDto>()))
                .ReturnsAsync(responseToLostStolen);
            _fisClient.Setup(client => client.Post<CardOperationResponseDto>(CardOperationConstants.FisCardStatusApiUrl, It.IsAny<ReplaceCardRequestDto>()))
                .ReturnsAsync(fisGetStatusResponse);
            _fisClient.Setup(client => client.Post<ReplaceCardResponseDto>(CardOperationConstants.FisReplaceCardApiUrl, It.IsAny<ReplaceCardRequestDto>()))
                .ReturnsAsync(replaceCardResponse);

            // Act
            var result = await _cardOperationController.ExecuteCardReplacement(requestDto) as ObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<ExecuteReplaceCardResponseDto>(result.Value);
            var response = (ExecuteReplaceCardResponseDto)result.Value;
            Assert.Equal(StatusCodes.Status422UnprocessableEntity, result.StatusCode);
        }
        [Fact]
        public async Task ExecuteReplaceCard_Should_Return_Bad_Request_Result()
        {

            // Arrange
            var requestDto = new ReplaceCardRequestMockDto();
            var fisGetStatusResponse = new CardOperationResponseDto
            {
                ErrorCode = StatusCodes.Status400BadRequest
            };

            var responseDto = new CardOperationResponseDto
            {
                FisResponse = "1 Active"
            };
            var responseToLostStolen = new CardOperationResponseDto
            {
                FisResponse = "1 LOSTSTOLEN"
            };

        

            _fisClient.Setup(client => client.Post<CardOperationResponseDto>(CardOperationConstants.FisCardOperationApiUrl, It.IsAny<CardOperationRequestDto>()))
                .ReturnsAsync(responseDto);
            _fisClient.Setup(client => client.Post<CardOperationResponseDto>(CardOperationConstants.FisCardOperationApiUrl, It.IsAny<CardOperationRequestDto>()))
                .ReturnsAsync(responseToLostStolen);
            _fisClient.Setup(client => client.Post<CardOperationResponseDto>(CardOperationConstants.FisCardStatusApiUrl, It.IsAny<ReplaceCardRequestDto>()))
                .ReturnsAsync(fisGetStatusResponse);

            // Act
            var result = await _cardOperationController.ExecuteCardReplacement(requestDto) as ObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<ExecuteReplaceCardResponseDto>(result.Value);
            var response = (ExecuteReplaceCardResponseDto)result.Value;
            Assert.Equal(StatusCodes.Status400BadRequest, result.StatusCode);
        }
        [Fact]
        public async Task ExecuteReplaceCard_Should_Return_Invalid_Operation_Result()
        {

            // Arrange
            var requestDto = new ReplaceCardRequestMockDto();
            var fisGetStatusResponse = new CardOperationResponseDto
            {
                FisResponse = "1 READY|09/29|<NULL>|Ready for activation^"
            };

            var responseDto = new CardOperationResponseDto
            {
                FisResponse = "1 Active"
            };
            var responseToLostStolen = new CardOperationResponseDto
            {
                FisResponse = "1 LOSTSTOLEN"
            };

            var replaceCardResponse = new ReplaceCardResponseDto
            {
                ProxyNumber = string.Empty
            };

            _fisClient.Setup(client => client.Post<CardOperationResponseDto>(CardOperationConstants.FisCardOperationApiUrl, It.IsAny<CardOperationRequestDto>()))
                .ReturnsAsync(responseDto);
            _fisClient.Setup(client => client.Post<CardOperationResponseDto>(CardOperationConstants.FisCardOperationApiUrl, It.IsAny<CardOperationRequestDto>()))
                .ReturnsAsync(responseToLostStolen);
            _fisClient.Setup(client => client.Post<CardOperationResponseDto>(CardOperationConstants.FisCardStatusApiUrl, It.IsAny<ReplaceCardRequestDto>()))
                .ReturnsAsync(fisGetStatusResponse);
            _fisClient.Setup(client => client.Post<ReplaceCardResponseDto>(CardOperationConstants.FisReplaceCardApiUrl, It.IsAny<ReplaceCardRequestDto>()))
                .ReturnsAsync(replaceCardResponse);

            // Act
            var result = await _cardOperationController.ExecuteCardReplacement(requestDto) as ObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<ExecuteReplaceCardResponseDto>(result.Value);
            var response = (ExecuteReplaceCardResponseDto)result.Value;
            Assert.Equal(StatusCodes.Status500InternalServerError, result.StatusCode);
        }
        [Fact]
        public async Task ExecutereplaceCard_Update_Card_Status_Should_Return_Not_Found_Result()
        {
            // Arrange
            var requestDto = new ReplaceCardRequestMockDto();
            var fisGetStatusResponse = new CardOperationResponseDto
            {
                FisResponse = "1 SUSPENDED|09/29|<NULL>|Suspended^"
            };

            var responseDto = new CardOperationResponseDto
            {
                FisResponse = "1 Active"
            };
            var responseToLostStolen = new CardOperationResponseDto
            {
                ErrorCode = StatusCodes.Status500InternalServerError,
            };
            _fisClient.Setup(client => client.Post<CardOperationResponseDto>(CardOperationConstants.FisCardStatusApiUrl, It.IsAny<ReplaceCardRequestDto>()))
              .ReturnsAsync(fisGetStatusResponse);
            _fisClient.Setup(client => client.Post<CardOperationResponseDto>(CardOperationConstants.FisCardOperationApiUrl, It.IsAny<CardOperationRequestDto>()))
                .ReturnsAsync(responseDto);
            _fisClient.Setup(client => client.Post<CardOperationResponseDto>(CardOperationConstants.FisCardOperationApiUrl, It.IsAny<CardOperationRequestDto>()))
                .ReturnsAsync(responseToLostStolen);

            // Act
            var result = await _cardOperationController.ExecuteCardReplacement(requestDto) as ObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<ExecuteReplaceCardResponseDto>(result.Value);
            var response = (ExecuteReplaceCardResponseDto)result.Value;
            Assert.False(response.Success);
            Assert.Equal(StatusCodes.Status500InternalServerError, result.StatusCode);
        }
        

    }
}
