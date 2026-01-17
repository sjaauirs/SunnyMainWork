using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Sunny.Benefits.Bff.Core.Domain.Constants;
using Sunny.Benefits.Bff.Core.Domain.Dtos;
using Sunny.Benefits.Bff.Core.Domain.Dtos.Json;
using Sunny.Benefits.Bff.Infrastructure.Helpers.Interface;
using Sunny.Benefits.Bff.Infrastructure.HttpClients;
using Sunny.Benefits.Bff.Infrastructure.HttpClients.Interfaces;
using Sunny.Benefits.Bff.Infrastructure.Services.Interfaces;
using SunnyBenefits.Fis.Core.Domain.Dtos;
using SunnyBenefits.Fis.Core.Domain.Dtos.Json;
using SunnyBenefits.Fis.Core.Domain.Models;
using SunnyRewards.Helios.Admin.Core.Domain.Dtos;
using SunnyRewards.Helios.Admin.Core.Domain.Dtos.Enums;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.Common.Core.Extensions;
using SunnyRewards.Helios.Tenant.Core.Domain.Dtos;
using SunnyRewards.Helios.User.Core.Domain.Dtos;
using SunnyRewards.Helios.User.Core.Domain.enums;
using SunnyRewards.Helios.Wallet.Core.Domain.Dtos;
using FundTransferToPurseRequestDto = Sunny.Benefits.Bff.Core.Domain.Dtos.FundTransferToPurseRequestDto;
namespace Sunny.Benefits.Bff.Infrastructure.Services
{
    public class CardOperationService : ICardOperationService
    {
        private readonly ILogger<CardOperationService> _logger;
        private readonly IFisClient _fisClient;
        private readonly ICardOperationsHelper _extractGetCardStatusHelper;
        private readonly IPersonHelper _personHelper;
        private readonly IAdminClient _adminClient;
        private readonly IConfiguration _configuration;
        private readonly INotificationHelper _notificationHelper;
        private readonly ITenantAccountService _tenantAccountService;

        private const string className = nameof(CardOperationService);

        public CardOperationService(ILogger<CardOperationService> logger, IFisClient fisClient, ICardOperationsHelper extractGetCardStatusHelper, IPersonHelper personHelper, IAdminClient adminClient, IConfiguration configuration, INotificationHelper notificationHelper, ITenantAccountService tenantAccountService)
        {
            _logger = logger;
            _fisClient = fisClient;
            _personHelper = personHelper;
            _extractGetCardStatusHelper = extractGetCardStatusHelper;
            _adminClient = adminClient;
            _configuration = configuration;
            _notificationHelper = notificationHelper;
            _tenantAccountService = tenantAccountService;
        }
        public async Task<ExecuteCardOperationResponseDto> ExecuteCardOperation(ExecuteCardOperationRequestDto requestDto)
        {
            bool cardActivateRequest = false;
            const string methodName = nameof(ExecuteCardOperation);
            try
            {
                if (string.IsNullOrEmpty(requestDto.CardOperation) || !CardOperationConstants.CardOperationMappings.ContainsKey(requestDto.CardOperation.ToUpper().Trim()))
                {
                    _logger.LogError("{ClassName}.{MethodName} - Null response received from TenantCode:{TenantCode}, ConsumerCode :{ConsumerCode},ErrorCode:{ErrorCode},",
                        className, methodName, requestDto.TenantCode, requestDto.ConsumerCode, StatusCodes.Status400BadRequest);
                    return new ExecuteCardOperationResponseDto { ErrorCode = StatusCodes.Status400BadRequest };
                }

                var cardOperationRequest = new CardOperationRequestDto()
                {
                    ConsumerCode = requestDto.ConsumerCode,
                    TenantCode = requestDto.TenantCode,
                    CardOperation = CardOperationConstants.CardOperationMappings[requestDto.CardOperation.ToUpper().Trim()]
                };
                var consumer = new GetConsumerRequestDto
                {
                    ConsumerCode = requestDto.ConsumerCode
                };

                if (String.Equals(cardOperationRequest.CardOperation, CardOperationConstants.CardOperationMappings["ACTIVATE"]))
                {
                    cardActivateRequest = true;
                    var consumerDetails = await _personHelper.GetPersonDetails(consumer);
                    if (consumerDetails?.ErrorCode != null)
                    {
                        _logger.LogError("{ClassName}.{MethodName} - Cannot find Person Details with TenantCode:{TenantCode},ConsumerCode :{Consumer}, ErrorCode:{ErrorCode},",
                            className, methodName, requestDto.TenantCode, consumer.ConsumerCode, consumerDetails.ErrorCode);
                        return new ExecuteCardOperationResponseDto { ErrorCode = consumerDetails.ErrorCode };
                    }
                    if (consumerDetails == null||consumerDetails.Consumer == null || !CardLast4Verified(consumerDetails.Consumer))
                    {
                        _logger.LogError("{ClassName}.{MethodName} - Conflict Error, Onboarding State is not valid, TenantCode:{TenantCode}, ConsumerCode :{Consumer}, ErrorCode:{ErrorCode},",
                            className, methodName, requestDto.TenantCode, consumer, StatusCodes.Status409Conflict);
                        return new ExecuteCardOperationResponseDto { ErrorCode = StatusCodes.Status409Conflict };
                    }
                }
                var response = await _fisClient.Post<CardOperationResponseDto>(CardOperationConstants.FisCardOperationApiUrl, cardOperationRequest);

                _logger.LogInformation("{ClassName}.{MethodName} -  Executed card operation Successfully for TenantCode:{TenantCode}, ConsumerCode :{ConsumerCode}",
                    className, methodName, requestDto.TenantCode, requestDto.ConsumerCode);
                if (response.ErrorCode != null)
                {
                    _logger.LogError("{ClassName}.{MethodName} - Invalid response from FisAPI, ErrorCode:{ErrorCode}, ERROR:{Msg}", className, methodName, response.ErrorCode, response.ErrorMessage);
                    return new ExecuteCardOperationResponseDto { ErrorCode = response.ErrorCode };
                }
                if (string.IsNullOrEmpty(response.FisResponse) || !IsValidCardOperationResponse(response.FisResponse, cardOperationRequest.CardOperation))
                {
                    _logger.LogError("{ClassName}.{MethodName} - Invalid response from FisAPI, Request Data: {RequestData}", className, methodName, requestDto.ToJson());
                    throw new InvalidOperationException("Invalid response from FisAPI");
                }
                if(cardActivateRequest)
                {
                    //card opration is successful
                     await FundTransferToHealthyLiving(requestDto);
                }

                // Triggering notification event if card is frozen
                if (String.Equals(cardOperationRequest.CardOperation, CardOperationConstants.CardOperationMappings["FREEZE"]))
                {
                    _logger.LogInformation("{ClassName}.{MethodName}: Triggering Card Freezed Notification Event for ConsumerCode: {ConsumerCode}, TenantCode: {TenantCode}", className, methodName, requestDto.ConsumerCode, requestDto.TenantCode);
                    await _notificationHelper.ProcessNotification(requestDto.ConsumerCode, requestDto.TenantCode, CardOperationConstants.CardFreezeNotificationEventName);
                }

                return new ExecuteCardOperationResponseDto()
                {
                    Success = true
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName} -  Error Occurred While performing card operation, TenantCode:{TenantCode}, ConsumerCode :{ConsumerCode}, ErrorCode:{ErrorCode},  ERROR: {Message}",
                    className, methodName, requestDto.TenantCode, requestDto.ConsumerCode, StatusCodes.Status500InternalServerError, ex.Message);
                throw new InvalidOperationException("ExecuteCardOperation :Error ", ex);
            }
        }

       
        public async Task<GetCardStatusResponseDto> GetCardStatus(GetCardStatusRequestDto requestDto)
        {
            const string methodName = nameof(GetCardStatus);
            try
            {
                var response = await _fisClient.Post<CardOperationResponseDto>(CardOperationConstants.FisCardStatusApiUrl, requestDto);
                _logger.LogInformation("{ClassName}.{MethodName} - Executed card operation Successfully for TenantCode:{TenantCode}, ConsuemrCode :{ConsumerCode}",
                    className, methodName, requestDto.TenantCode, requestDto.ConsumerCode);
                if (response.ErrorCode != null)
                {
                    return new GetCardStatusResponseDto { ErrorCode = response.ErrorCode };
                }
                var cardStatus = _extractGetCardStatusHelper.ExtractCardStatusFromFisResponse(response.FisResponse);
                if (string.IsNullOrEmpty(cardStatus))
                {
                    _logger.LogError("{ClassName}.{MethodName} - Error response from FisAPI for TenantCode:{TenantCode}, ConsumerCode :{ConsumerCode}, Response Data: {ResponseData}",
                        className, methodName, requestDto.TenantCode, requestDto.ConsumerCode, response.FisResponse);
                    throw new InvalidOperationException("Error response from FisAPI");
                }
                if (!CardOperationConstants.ReverseCardOperationMappings.ContainsKey(cardStatus.ToUpper().Trim()))
                {
                    return new GetCardStatusResponseDto() { CardStatus = CardOperationConstants.UnknownStatus };
                }

                return new GetCardStatusResponseDto() { CardStatus = CardOperationConstants.ReverseCardOperationMappings[cardStatus.ToUpper().Trim()] };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName} - Error Occurred While fetching card status for TenantCode:{TenantCode}, ConsumerCode :{ConsumerCode}, ErrorCode:{ErrorCode}, ERROR: {Message}",
                    className, methodName, requestDto.TenantCode, requestDto.ConsumerCode, StatusCodes.Status500InternalServerError, ex.Message);
                throw new InvalidOperationException("GetCardStatus :Error ", ex);
            }
        }

        private bool IsValidCardOperationResponse(string fisResponse, string cardOperation)
        {
            const string methodName = nameof(IsValidCardOperationResponse);
            if (fisResponse == $"1 {cardOperation}^")
            {
                return true;
            }
            else
            {
                _logger.LogError("{ClassName}.{MethodName} - IsValidResponse: Invalid response from FisAPI, Response: {FisResponse}", className, methodName, fisResponse);
                return false;
            }
        }

        private bool CardLast4Verified(ConsumerDto consumer)
        {
            var presentState = Enum.Parse<OnboardingState>(consumer.OnBoardingState!);
            return presentState >= OnboardingState.CARD_LAST_4_VERIFIED;
        }

        private async Task<bool> FundTransferToHealthyLiving(ExecuteCardOperationRequestDto requestDto)
        {
            try
            {
                var transferFlag = await CheckSupportLiveTransferToRewardsPurseflag(requestDto.TenantCode!);

                if (!transferFlag)
                {
                    _logger.LogInformation("Fund transfer not configured at tenant : {TenanatCode}", requestDto.TenantCode);
                    return false;
                }

                var tenantAccout = await _tenantAccountService.GetTenantAccount(new TenantAccountCreateRequestDto
                {
                    TenantCode = requestDto.TenantCode
                });

                var tenantConfig = tenantAccout.TenantConfigJson != null
                    ? JsonConvert.DeserializeObject<TenantConfig>(tenantAccout.TenantConfigJson)
                    : new TenantConfig();

                var activePurses = tenantConfig?
                    .PurseConfig?
                    .Purses?
                    .Where(p =>
                        p.ActiveStartTs.HasValue &&
                        p.RedeemEndTs.HasValue &&
                        p.ActiveStartTs.Value < DateTime.UtcNow &&
                        p.RedeemEndTs.Value >= DateTime.UtcNow
                    )
                    .ToList() ?? new List<Purse>();

                bool anyTransferDone = false;

                if (activePurses.Any())
                {
                    var consumerWallets = await GetConsumerWallets(requestDto);

                    var activeRewardWallets = consumerWallets.ConsumerWalletDetails
                        .Where(w =>
                            w.WalletType?.WalletTypeCode == GetRewardWalletTypeCode() &&
                            w.Wallet.ActiveStartTs < DateTime.UtcNow &&
                            w.Wallet.RedeemEndTs >= DateTime.UtcNow
                        )
                        .ToList();

                    var rewardWalletToPurseMap = activeRewardWallets
                        .Select(w => new
                        {
                            RewardWallet = w,
                            Purse = activePurses
                                .Where(p =>
                                    w.Wallet?.ActiveStartTs <= p.RedeemEndTs.Value &&
                                    w.Wallet.RedeemEndTs >= p.ActiveStartTs.Value
                                )
                                .OrderBy(p => Math.Abs(
                                    (p.RedeemEndTs.Value - w.Wallet.RedeemEndTs).TotalSeconds))
                                .FirstOrDefault()
                        })
                        .Where(x => x.Purse != null)
                        .ToList();

                    foreach (var mapping in rewardWalletToPurseMap)
                    {
                        var rewardWallet = mapping.RewardWallet;
                        if (rewardWallet?.Wallet?.Balance > 0)
                        {
                            var fundTransferRequest = new FundTransferToPurseRequestDto
                            {
                                ConsumerCode = requestDto.ConsumerCode,
                                TenantCode = requestDto.TenantCode,
                                ConsumerWalletTypeCode = string.Empty,
                                RedemptionVendorCode = CardOperationConstants.HealthyLivingRedumtionVendorCode,
                                RedemptionAmount = rewardWallet.Wallet.Balance,
                                PurseWalletType = mapping.Purse!.PurseWalletType,
                                WalletId = rewardWallet.Wallet.WalletId
                            };

                            var response = await _adminClient.Post<BaseResponseDto>("fund-transfer", fundTransferRequest);

                            if (response.ErrorCode == null)
                            {
                                _logger.LogInformation("Fund transfer completed for ConsumerCode={ConsumerCode} with amount={Amount}",
                                    fundTransferRequest.ConsumerCode, fundTransferRequest.RedemptionAmount);

                                anyTransferDone = true;
                            }
                            _logger.LogError("failed to transfer fund-  ErrorCode: {ErrorCode} Message: {Message}", response.ErrorCode, response.ErrorMessage);
                        }
                        else
                        {
                            _logger.LogInformation("No balance to transfer for ConsumerCode={ConsumerCode}", requestDto.ConsumerCode);
                        }
                    }
                }
                else
                {
                    _logger.LogInformation(
                        "No active purse wallets for tenant {Tenantcode}",
                        requestDto.TenantCode
                    );
                }
                
                return anyTransferDone;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in LiveFundTransferToPurse for ConsumerCode={ConsumerCode}, TenantCode={TenantCode}",
                    requestDto.ConsumerCode, requestDto.TenantCode);
                return false;
            }
        }

        private async Task<ConsumerWalletResponseDto> GetConsumerWallets(ExecuteCardOperationRequestDto requestDto)
        {
            var getConsumerWalletRequestDto = new FindConsumerWalletRequestDto()
            {
                ConsumerCode = requestDto.ConsumerCode,
                IncludeRedeemOnlyWallets = true
            };
            return await _adminClient.Post<ConsumerWalletResponseDto>(AdminConstants.GetAllConsumerRedeemableWallets, getConsumerWalletRequestDto);
        }

        private string? GetRewardWalletTypeCode()
        {
            return _configuration.GetSection("Reward_Wallet_Type_Code").Value;
        }

        private string? GetHealthyLivingPurseWalletTypeCode()
        {
            return _configuration.GetSection("Healthy_Living_Wallet_Type_Code").Value;
        }

        private async Task<TenantDto?> GetTenantDetails(string tenantCode)
        {
            var methodName = nameof(GetTenantDetails);
            IDictionary<string, long> parameters = new Dictionary<string, long>();
            var response = await _adminClient.Get<TenantResponseDto>($"{AdminConstants.GetTenant}?tenantCode={tenantCode}", parameters);
            if (response?.ErrorCode != null)
            {
                _logger.LogError("{ClassName}.{MethodName}: API - Error occurred while fetching tenant details, ErrorCode: {ErrorCode}, ErrorMessage: {ErrorMessage}", className, methodName, response.ErrorCode, response.ErrorMessage);
                return null;
            }
            return response?.Tenant?.TenantId > 0 ? response.Tenant : null;
        }

        private async Task<bool> CheckSupportLiveTransferToRewardsPurseflag(string TenantCode)
        {
            var tenant = await GetTenantDetails(TenantCode);

            if (tenant != null)
            {
                var tenantOption = !string.IsNullOrEmpty(tenant.TenantOption)
                    ? JsonConvert.DeserializeObject<TenantOption>(tenant.TenantOption)
                    : new TenantOption();

                if (tenantOption?.Apps?.Any(x => string.Equals(x, AdminConstants.Apps.Benefits, StringComparison.OrdinalIgnoreCase)) == true)
                {
                    var tenantAttributes = !string.IsNullOrEmpty(tenant.TenantAttribute)
                        ? JsonConvert.DeserializeObject<TenantAttribute>(tenant.TenantAttribute)
                        : new TenantAttribute();
                    return tenantAttributes?.SupportLiveTransferToRewardsPurse ?? false;
                }
            }

            return false;
        }

    }
}
