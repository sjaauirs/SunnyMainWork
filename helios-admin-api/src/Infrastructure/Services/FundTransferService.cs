using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SunnyBenefits.Fis.Core.Domain.Dtos;
using SunnyRewards.Helios.Admin.Core.Domain.Constants;
using SunnyRewards.Helios.Admin.Core.Domain.Dtos;
using SunnyRewards.Helios.Admin.Infrastructure.HttpClients.Interfaces;
using SunnyRewards.Helios.Admin.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.User.Core.Domain.Dtos;
using SunnyRewards.Helios.Wallet.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Admin.Infrastructure.Services
{
    public class FundTransferService : IFundTransferService
    {
        private static int RETRY_MIN_WAIT_MS = 5; // min amount of milliseconds to wait before retrying
        private static int RETRY_MAX_WAIT_MS = 50; // max amount of milliseconds to wait before retrying

        private readonly IUserClient _userClient;
        private readonly IWalletClient _walletClient;
        private readonly IFisClient _fisClient;
        private readonly ILogger<FundTransferService> _logger;
        private readonly IConfiguration _config;
        private readonly Random _random = new Random();
        private readonly ITenantAccountService _tenantAccountService;
        const string className = nameof(FundTransferService);
        public FundTransferService(
            ILogger<FundTransferService> logger,
            IUserClient userClient,
            IWalletClient walletClient,
            IFisClient fisClient,
            IConfiguration config,
            ITenantAccountService tenantAccountService)
        {
            _logger = logger;
            _userClient = userClient;
            _walletClient = walletClient;
            _fisClient = fisClient;
            _config = config;
            _tenantAccountService = tenantAccountService;
        }

        /// <summary>
        /// Transfers the funds asynchronous.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns></returns>
        public async Task<BaseResponseDto> TransferFundsAsync(FundTransferToPurseRequestDto request)
        {
            var response = new BaseResponseDto();
            const string methodName = nameof(TransferFundsAsync);
            try
            {
                if (request.RedemptionAmount <= 0)
                {
                    _logger.LogError("{ClassName}.{MethodName}: Invalid amount specified. Amount must be greater than zero. ConsumerCode: {ConsumerCode}, Amount: {Amount}", className, methodName, request.ConsumerCode, request.RedemptionAmount);
                    response.ErrorCode = StatusCodes.Status400BadRequest;
                    response.ErrorMessage = "Amount must be greater than zero.";
                    return response;
                }


                var getConsumerRequestDto = new GetConsumerRequestDto()
                {
                    ConsumerCode = request.ConsumerCode
                };
                var consumerResponse = await GetConsumer(getConsumerRequestDto);
                if (consumerResponse.Consumer == null || consumerResponse.Consumer.ConsumerId == 0)
                {
                    _logger.LogError("{ClassName}.{MethodName}: Consumer not found for ConsumerCode: {ConsumerCode}", className, methodName, request.ConsumerCode);
                    response.ErrorCode = StatusCodes.Status404NotFound;
                    return response;
                }

                if (consumerResponse.Consumer.TenantCode != request.TenantCode)
                {
                    _logger.LogError("{ClassName}.{MethodName}:TenantCode mismatch for ConsumerCode: {ConsumerCode}", className, methodName, request.ConsumerCode);
                    response.ErrorCode = StatusCodes.Status400BadRequest;
                    return response;
                }

                var getConsumerWalletRequestDto = new FindConsumerWalletRequestDto()
                {
                    ConsumerCode = request.ConsumerCode,
                    IncludeRedeemOnlyWallets = true
                };
                var consumerWallets = await _walletClient.Post<ConsumerWalletResponseDto>(Constant.GetAllConsumerRedeemableWallets, getConsumerWalletRequestDto);
                if (consumerWallets.ErrorCode != null)
                {
                    _logger.LogError("{ClassName}.{MethodName}: TenantCode mismatch for ConsumerCode: {ConsumerCode}", className, methodName, request.ConsumerCode);
                    response.ErrorCode = consumerWallets.ErrorCode;
                    response.ErrorMessage = consumerWallets.ErrorMessage;
                    return response;
                }

                if (!CheckWaleltType(consumerWallets, request.PurseWalletType))
                {
                    _logger.LogError("{ClassName}.{MethodName}: Consumer does not have the required SourceWalletType or TargetWalletType for ConsumerCode: {ConsumerCode}", className, methodName, request.ConsumerCode);
                    response.ErrorCode = StatusCodes.Status400BadRequest;
                    response.ErrorMessage = "Consumer does not have the required wallets.";
                    return response;
                }

                var tenantAccountResponse = await _tenantAccountService.GetTenantAccount(request.TenantCode!);
                if (tenantAccountResponse.ErrorCode != null)
                {
                    _logger.LogError("{ClassName}.{MethodName}: Tenant account not found for TenantCode: {TenantCode}", className, methodName, request.TenantCode);
                    response.ErrorCode = consumerWallets.ErrorCode;
                    response.ErrorMessage = consumerWallets.ErrorMessage;
                    return response;
                }
                var tenantConfig = string.IsNullOrEmpty(tenantAccountResponse.TenantAccount?.TenantConfigJson)
                    ? new TenantConfigDto()
                    : JsonConvert.DeserializeObject<TenantConfigDto>(tenantAccountResponse.TenantAccount.TenantConfigJson);
                var purse = tenantConfig?.PurseConfig?.Purses?.FirstOrDefault(p => p.PurseWalletType == request.PurseWalletType && p.RedemptionTarget);

                if (purse == null)
                {
                    var errorMessage = $"No purse configuration found for Wallet Type: {request.PurseWalletType} with Redemption Target set to True.";
                    _logger.LogError("{ClassName}.{MethodName}: {ErrorMessage}", className, methodName, errorMessage);
                    response.ErrorCode = StatusCodes.Status400BadRequest;
                    response.ErrorMessage = errorMessage;
                    return response;
                }
                var fundTransferResponse = await TransferAmountToPurse(request, purse);
                if (fundTransferResponse.ErrorCode != null)
                {
                    response.ErrorCode = fundTransferResponse.ErrorCode;
                    return response;
                }
                _logger.LogInformation("{ClassName}.{MethodName}: Fund transfer completed successfully for ConsumerCode: {ConsumerCode}", className, methodName, request.ConsumerCode);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An exception occurred during fund transfer for ConsumerCode: {ConsumerCode}", request.ConsumerCode);
                response.ErrorCode = StatusCodes.Status500InternalServerError;
                response.ErrorMessage = "An error occurred during the fund transfer.";
            }

            return response;
        }

        /// <summary>
        /// Gets the consumer.
        /// </summary>
        /// <param name="consumerSummaryRequestDto">The consumer summary request dto.</param>
        /// <returns></returns>
        private async Task<GetConsumerResponseDto> GetConsumer(GetConsumerRequestDto consumerSummaryRequestDto)
        {
            const string methodName = nameof(GetConsumer);
            _logger.LogInformation("{ClassName}.{MethodName} - Consumer get started with Consumer code: {ConsumerCode}", className, methodName, consumerSummaryRequestDto.ConsumerCode);
            var consumer = await _userClient.Post<GetConsumerResponseDto>(Constant.GetConsumerAPIUrl, consumerSummaryRequestDto);
            if (consumer?.Consumer == null)
            {
                _logger.LogError("{ClassName}.{MethodName} - Invalid Consumer code: {ConsumerCode},ErrorCode:{Code}", className, methodName, consumerSummaryRequestDto.ConsumerCode, StatusCodes.Status404NotFound);
                return new GetConsumerResponseDto();
            }
            _logger.LogInformation("{ClassName}.{MethodName} - Ending to GetConsumer, ConsumerCode : {ConsumerCode}", className, methodName, consumerSummaryRequestDto.ConsumerCode);
            return consumer;
        }

        private bool CheckWaleltType(ConsumerWalletResponseDto cerWalletResponseDto, string? walletType)
        {
            var cosnumerWallet = cerWalletResponseDto?.ConsumerWalletDetails?.FirstOrDefault(x => x.WalletType?.WalletTypeCode == walletType);

            return cosnumerWallet != null;
        }

        /// <summary>
        /// Transfers the amount to purse.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns></returns>
        private async Task<BaseResponseDto> TransferAmountToPurse(FundTransferToPurseRequestDto request, PurseDto purse)
        {
            var response = new BaseResponseDto();
            try
            {
                var redemptionRef = Guid.NewGuid().ToString("N");
                var postRedeemStartRequestDto = new Core.Domain.Dtos.PostRedeemStartRequestDto
                {
                    ConsumerWalletTypeCode = request.ConsumerWalletTypeCode,
                    RedemptionWalletTypeCode = purse.MasterRedemptionWalletType,
                    TenantCode = request.TenantCode,
                    ConsumerCode = request.ConsumerCode,
                    RedemptionVendorCode = request.RedemptionVendorCode,
                    RedemptionAmount = request.RedemptionAmount,
                    RedemptionRef = redemptionRef,
                    RedemptionItemDescription = Core.Domain.Dtos.Constants.Constant.RedemptionItemDescription_ValueLoad_HealthyLiving,
                    WalletId = request.WalletId
                };
                var redeemStartResponse = await _walletClient.Post<PostRedeemStartResponseDto>(Constant.WalletRedeemStartAPIUrl, postRedeemStartRequestDto);
                if (redeemStartResponse.ErrorCode != null)
                {
                    _logger.LogError("{className}.TransferFundsAsync.TransferAmountToPurse: An error occurred while redeeming wallet balance of Consumer: {ConsumerCode}, Error: {Message}, Error Code:{errorCode}", className,
                        postRedeemStartRequestDto.ConsumerCode, redeemStartResponse.ErrorMessage, redeemStartResponse.ErrorCode);
                    response.ErrorCode = StatusCodes.Status500InternalServerError;
                    return response;
                }
                var loadValueRequestDto = new LoadValueRequestDto
                {
                    TenantCode = request.TenantCode,
                    ConsumerCode = request.ConsumerCode,
                    PurseWalletType = request.PurseWalletType,
                    Amount = request.RedemptionAmount!.Value,
                    Currency = Constant.Currency_USD,
                    MerchantName = Constant.MerchantNameForFundTransfer
                };

                var loadValueResponse = await PerformLoadValueWithRetries(loadValueRequestDto, request.ConsumerCode);

                if (loadValueResponse == null || loadValueResponse.ErrorCode != null)
                {
                    response.ErrorCode = StatusCodes.Status500InternalServerError;
                    var postRedeemFailRequestDto = new PostRedeemFailRequestDto()
                    {
                        TenantCode = request.TenantCode,
                        ConsumerCode = request.ConsumerCode,
                        RedemptionVendorCode = request.RedemptionVendorCode,
                        RedemptionAmount = request.RedemptionAmount.Value,
                        RedemptionRef = redemptionRef,
                        Notes = request.Notes
                    };
                    var redeemFailResponse = await _walletClient.Post<PostRedeemFailResponseDto>(Constant.WalletRedeemFailAPIUrl, postRedeemFailRequestDto);
                    if (redeemFailResponse.ErrorCode != null)
                    {
                        _logger.LogError("{className}.TransferFundsAsync.TransferAmountToPurse: An error occurred while reverting redeem transaction for Consumer: {ConsumerCode}, Error: {Message}, Error Code:{errorCode}", className,
                            postRedeemStartRequestDto.ConsumerCode, redeemFailResponse.ErrorMessage, redeemFailResponse.ErrorCode);
                    }
                    return response;
                }
                if (loadValueResponse != null && (loadValueResponse?.ErrorCode == null || loadValueResponse?.ErrorCode == StatusCodes.Status200OK))
                {
                    var postRedeemCompleteRequestDto = new PostRedeemCompleteRequestDto()
                    {
                        ConsumerCode = postRedeemStartRequestDto.ConsumerCode,
                        RedemptionVendorCode = postRedeemStartRequestDto.RedemptionVendorCode,
                        RedemptionRef = postRedeemStartRequestDto.RedemptionRef
                    };

                    var redeemSuccessResponse = await _walletClient.Post<PostRedeemCompleteResponseDto>(Constant.WalletRedeemCompleteAPIUrl, postRedeemCompleteRequestDto);
                    if (redeemSuccessResponse.ErrorCode != null)
                    {
                        _logger.LogError("{className}.TransferFundsAsync.TransferAmountToPurse: An error occurred while executing redeem complete transaction for Consumer: {ConsumerCode}, Error: {Message}, Error Code:{errorCode}", className,
                            postRedeemStartRequestDto.ConsumerCode, redeemSuccessResponse.ErrorMessage, redeemSuccessResponse.ErrorCode);
                    }
                    _logger.LogInformation("{className}.TransferFundsAsync: Successfully redeem completed for Consumer: {ConsumerCode}", className, postRedeemStartRequestDto.ConsumerCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{className}.TransferFundsAsync: Error occurred while transferring reward amount to FIS purse, ErrorMessage - {errorMessage}", className, ex.Message);
                response.ErrorCode = StatusCodes.Status500InternalServerError;
            }
            return response;
        }

        /// <summary>
        /// Performs the load value with retries.
        /// </summary>
        /// <param name="loadValueRequestDto">The load value request dto.</param>
        /// <param name="consumerCode">The consumer code.</param>
        /// <returns></returns>
        private async Task<LoadValueResponseDto?> PerformLoadValueWithRetries(LoadValueRequestDto loadValueRequestDto, string? consumerCode)
        {
            var maxTries = Constant.MaxTries_Count;
            LoadValueResponseDto? loadValueResponse = null;
            while (maxTries > 0)
            {
                try
                {
                    loadValueResponse = await _fisClient.Post<LoadValueResponseDto>(Constant.FISValueLoadAPIUrl, loadValueRequestDto);
                    if (loadValueResponse.ErrorCode == null)
                    {
                        break;
                    }

                    _logger.LogError("{className}.PerformLoadValueWithRetries: Response ErrorCode: {errCode} in Load value retrying count left={maxTries}, ConsumerCode: {consumerCode}", className, loadValueResponse.ErrorCode, maxTries,
                        consumerCode);
                    maxTries--;
                    Thread.Sleep(_random.Next(RETRY_MIN_WAIT_MS, RETRY_MAX_WAIT_MS));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "{className}.PerformLoadValueWithRetries: Error occurred while Load value, retrying count left={maxTries}", className, maxTries);
                    maxTries--;
                    Thread.Sleep(_random.Next(RETRY_MIN_WAIT_MS, RETRY_MAX_WAIT_MS));
                }
            }

            return loadValueResponse;
        }

        /// <summary>
        /// Gets the reward redemption wallet type code.
        /// </summary>
        /// <returns></returns>
        private string? GetRewardRedemptionWalletTypeCode()
        {
            return _config.GetSection("Health_Actions_Redemption_Wallet_Type_Code").Value;
        }
    }
}
