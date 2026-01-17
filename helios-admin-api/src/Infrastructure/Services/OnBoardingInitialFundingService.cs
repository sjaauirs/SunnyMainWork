using Grpc.Net.Client.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SunnyBenefits.Fis.Core.Domain.Dtos;
using SunnyBenefits.Fis.Core.Domain.Dtos.Json;
using SunnyRewards.Helios.Admin.Core.Domain.Dtos;
using SunnyRewards.Helios.Admin.Infrastructure.Exceptions;
using SunnyRewards.Helios.Admin.Infrastructure.Helpers.Interface;
using SunnyRewards.Helios.Admin.Infrastructure.HttpClients.Interfaces;
using SunnyRewards.Helios.Admin.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.Cohort.Core.Domain.Dtos;
using SunnyRewards.Helios.Tenant.Core.Domain.Dtos.Json;
using SunnyRewards.Helios.Tenant.Core.Domain.Models;
using SunnyRewards.Helios.User.Core.Domain.Dtos;
using SunnyRewards.Helios.Wallet.Core.Domain.Constants;
using SunnyRewards.Helios.Wallet.Core.Domain.Dtos;
using System.Security.Cryptography;
using Constant = SunnyRewards.Helios.Admin.Core.Domain.Constants.Constant;
using FundingConfigJson = SunnyBenefits.Fis.Core.Domain.Dtos.Json.FundingConfigJson;
namespace SunnyRewards.Helios.Admin.Infrastructure.Services
{
    public class OnBoardingInitialFundingService : IOnBoardingInitialFundingService
    {
        public readonly ILogger<OnBoardingInitialFundingService> _logger;
        private readonly ITenantAccountService _tenantAccountService;
        private readonly IConsumerAccountService _consumerAccountService;
        private readonly IWalletClient _walletClient;
        private readonly IFisClient _fisClient;
        private readonly IConfiguration _configuration;
        private readonly IConsumerCohortHelper _consumerCohortHelper;

        public const string className = nameof(OnBoardingInitialFundingService);
        public OnBoardingInitialFundingService(ILogger<OnBoardingInitialFundingService> logger,
            ITenantAccountService tenantAccountService, IConsumerAccountService consumerAccountService
            , IWalletClient walletClient, IFisClient fisClient, IConfiguration configuration, IConsumerCohortHelper consumerCohortHelper)
        {
            _logger = logger;
            _tenantAccountService = tenantAccountService;
            _consumerAccountService = consumerAccountService;
            _walletClient = walletClient;
            _fisClient = fisClient;
            _configuration = configuration;
            _consumerCohortHelper = consumerCohortHelper;
        }

        /// <summary>
        /// Processes the initial funding asynchronous.
        /// </summary>
        /// <param name="initialFundingRequest">The initial funding request.</param>
        /// <returns></returns>
        public InitialFundingResponseDto ProcessInitialFundingAsync(InitialFundingRequestDto initialFundingRequest)
        {
            try
            {
                ValidateRequest(initialFundingRequest);

                var tenantAccount =  GetTenantAccountAsync(initialFundingRequest.TenantCode!).GetAwaiter().GetResult(); ;
                var consumerAccount =  GetConsumerAccountAsync(initialFundingRequest.TenantCode!, initialFundingRequest.ConsumerCode!).GetAwaiter().GetResult(); ;

                var fundingConfig = ParseFundingConfig(tenantAccount);
               

                var masterWallets = GetMasterWalletsAsync(initialFundingRequest.TenantCode!).GetAwaiter().GetResult(); ;

                ProcessPursesAsync(initialFundingRequest, fundingConfig, masterWallets, tenantAccount).GetAwaiter().GetResult(); ;
                return new InitialFundingResponseDto();
            }
            catch (ServiceValidationException ex)
            {
                _logger.LogError(ex, "Validation error in {MethodName}: {Message}", nameof(ProcessInitialFundingAsync), ex.Message);
                return new InitialFundingResponseDto
                {
                    ErrorCode = ex.StatusCode,
                    ErrorMessage = ex.Message
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in {MethodName}: {Message}", nameof(ProcessInitialFundingAsync), ex.Message);
                return new InitialFundingResponseDto
                {
                    ErrorCode = StatusCodes.Status500InternalServerError,
                    ErrorMessage = "An unexpected error occurred. Please try again later."
                };
            }
        }

        private async Task<(bool isValid, string error)>  ValidateFundingRules(FundingRule fundingRule, InitialFundingRequestDto initialFundingRequestDto)
        {
            bool validDate = false;
            bool validCohort = true; // true if cohort not found
            string error = string.Empty;

            // validate Date
            var Today = DateTime.UtcNow.Date;
            if (DateTime.TryParse(fundingRule.EffectiveStartDate, out var startDate) &&
                         DateTime.TryParse(fundingRule.EffectiveEndDate, out var endDate))
            {
                validDate= startDate <= Today && endDate >= Today;
            }

            if (!validDate)
            {
                _logger.LogInformation("Funding rule {RuleNumber} is not within the effective date range for consumerCode: {ConsumerCode}.", fundingRule.RuleNumber, initialFundingRequestDto.ConsumerCode);
                error = $"Funding rule {fundingRule.RuleNumber} is not within the effective date range for consumerCode: {initialFundingRequestDto.ConsumerCode}.";
                return (validDate, error);
            }


            // validate Cohort if fundng rule has cohorts
            if (fundingRule.CohortCodes.Count > 0)
            {

                var consumerCohorts = await _consumerCohortHelper.GetConsumerCohorts(new ConsumerCohortsRequestDto
                {
                    TenantCode = initialFundingRequestDto.TenantCode!,
                    ConsumerCode = initialFundingRequestDto.ConsumerCode!
                });


                if (consumerCohorts != null && consumerCohorts.Cohorts != null && consumerCohorts.Cohorts.Any())
                {
                    var consumerEnrolledCohorts = consumerCohorts.Cohorts.Select(c => c.CohortCode).ToList();
                    validCohort = fundingRule.CohortCodes.All(cohort => consumerEnrolledCohorts.Contains(cohort, StringComparer.OrdinalIgnoreCase));

                    if (!validCohort)
                    {
                        _logger.LogInformation("Consumer {ConsumerCode} is not enrolled in all required cohorts for funding rule {RuleNumber}.", initialFundingRequestDto.ConsumerCode, fundingRule.RuleNumber);
                        error = $"Consumer {initialFundingRequestDto.ConsumerCode} is not enrolled in all required cohorts for funding rule {fundingRule.RuleNumber}.";
                        return (validCohort, error);
                    }
                }
            }
          


                return (validDate && validCohort, error);
        }

        /// <summary>
        /// Validates the request.
        /// </summary>
        /// <param name="initialFundingRequest">The initial funding request.</param>
        /// <exception cref="SunnyRewards.Helios.Admin.Infrastructure.Exceptions.ServiceValidationException">
        /// Tenant code or consumer code is invalid.
        /// or
        /// Selected purses are not available.
        /// </exception>
        private void ValidateRequest(InitialFundingRequestDto initialFundingRequest)
        {
            if (string.IsNullOrWhiteSpace(initialFundingRequest?.TenantCode) || string.IsNullOrWhiteSpace(initialFundingRequest?.ConsumerCode))
            {
                throw new ServiceValidationException("Tenant code or consumer code is invalid.", StatusCodes.Status400BadRequest);
            }

            if (initialFundingRequest.SelectedPurses == null || !initialFundingRequest.SelectedPurses.Any())
            {
                throw new ServiceValidationException("Selected purses are not available.", StatusCodes.Status400BadRequest);
            }
        }

        /// <summary>
        /// Gets the tenant account asynchronous.
        /// </summary>
        /// <param name="tenantCode">The tenant code.</param>
        /// <returns></returns>
        /// <exception cref="SunnyRewards.Helios.Admin.Infrastructure.Exceptions.ServiceValidationException">
        /// Tenant account not found for TenantCode: {tenantCode}
        /// </exception>
        private async Task<TenantAccountRequestDto> GetTenantAccountAsync(string tenantCode)
        {
            var response = await _tenantAccountService.GetTenantAccount(tenantCode);
            if (response?.ErrorCode != null)
            {
                throw new ServiceValidationException(response.ErrorMessage ?? string.Empty, response.ErrorCode);
            }

            if (response?.TenantAccount == null)
            {
                throw new ServiceValidationException($"Tenant account not found for TenantCode: {tenantCode}", StatusCodes.Status404NotFound);
            }

            return response.TenantAccount;
        }

        /// <summary>
        /// Gets the consumer account asynchronous.
        /// </summary>
        /// <param name="tenantCode">The tenant code.</param>
        /// <param name="consumerCode">The consumer code.</param>
        /// <returns></returns>
        /// <exception cref="SunnyRewards.Helios.Admin.Infrastructure.Exceptions.ServiceValidationException">
        /// Consumer account not found for ConsumerCode: {consumerCode}
        /// </exception>
        private async Task<ConsumerAccountDto> GetConsumerAccountAsync(string tenantCode, string consumerCode)
        {
            var request = new GetConsumerAccountRequestDto { TenantCode = tenantCode, ConsumerCode = consumerCode };
            var response = await _consumerAccountService.GetConsumerAccount(request);
            if (response?.ErrorCode != null)
            {
                throw new ServiceValidationException(response.ErrorMessage ?? string.Empty, response.ErrorCode);
            }

            if (response?.ConsumerAccount == null)
            {
                throw new ServiceValidationException($"Consumer account not found for ConsumerCode: {consumerCode}", StatusCodes.Status404NotFound);
            }

            return response.ConsumerAccount;
        }

        /// <summary>
        /// Parses the funding configuration.
        /// </summary>
        /// <param name="tenantAccount">The tenant account.</param>
        /// <returns></returns>
        /// <exception cref="SunnyRewards.Helios.Admin.Infrastructure.Exceptions.ServiceValidationException">
        /// Funding rules not configured for the tenant.
        /// or
        /// No funding rules configured for the tenant.
        /// or
        /// No OnBoarding funding rules configured for the tenant.
        /// </exception>
        private FundingConfigJson ParseFundingConfig(TenantAccountRequestDto tenantAccount)
        {
            if (string.IsNullOrEmpty(tenantAccount.FundingConfigJson))
            {
                throw new ServiceValidationException("Funding rules not configured for the tenant.", StatusCodes.Status404NotFound);
            }

            var fundingConfig = JsonConvert.DeserializeObject<FundingConfigJson>(tenantAccount.FundingConfigJson);
            if (fundingConfig == null || fundingConfig.FundingRules == null || !fundingConfig.FundingRules.Any())
            {
                throw new ServiceValidationException("No funding rules configured for the tenant.", StatusCodes.Status404NotFound);
            }

            fundingConfig.FundingRules = fundingConfig.FundingRules
                .Where(x => x.RecurrenceType != null && x.RecurrenceType.Equals(Constant.FISOnBoardingRecurrenceType, StringComparison.OrdinalIgnoreCase) && x.Enabled)
                .ToList();

            if (!fundingConfig.FundingRules.Any())
            {
                throw new ServiceValidationException("No OnBoarding funding rules configured for the tenant.", StatusCodes.Status404NotFound);
            }

            return fundingConfig;
        }

        /// <summary>
        /// Processes the purses asynchronous.
        /// </summary>
        /// <param name="initialFundingRequest">The initial funding request.</param>
        /// <param name="fundingConfig">The funding configuration.</param>
        /// <param name="masterWallets">The master wallets.</param>
        /// <param name="tenantAccount">The tenant account.</param>
        private async System.Threading.Tasks.Task ProcessPursesAsync(
        InitialFundingRequestDto initialFundingRequest,
        FundingConfigJson fundingConfig,
        List<TenantWalletDetailDto> masterWallets,
        TenantAccountRequestDto tenantAccount)
        {
            var tenantConfig = string.IsNullOrEmpty(tenantAccount.TenantConfigJson)
                ? new TenantConfigDto()
                : JsonConvert.DeserializeObject<TenantConfigDto>(tenantAccount.TenantConfigJson);

            var purseErrors = new List<string>();
            foreach (var selectedPurseLabel in initialFundingRequest.SelectedPurses!)
            {
                try
                {
                    await ProcessSinglePurseAsync(initialFundingRequest, fundingConfig, masterWallets, tenantConfig!, selectedPurseLabel);
                }
                catch (Exception ex)
                {
                    var msg = string.Format("Error processing purse {0} for Consumer {1}", selectedPurseLabel, initialFundingRequest.ConsumerCode);
                    _logger.LogError(ex, msg);
                    purseErrors.Add(ex.Message);
                }
            }

            if (purseErrors.Count > 0)
            {
                var allErrors = string.Join(", ", purseErrors);
                throw new ServiceValidationException(allErrors);
            }
        }

        /// <summary>
        /// Processes the single purse asynchronous.
        /// </summary>
        /// <param name="initialFundingRequest">The initial funding request.</param>
        /// <param name="fundingConfig">The funding configuration.</param>
        /// <param name="masterWallets">The master wallets.</param>
        /// <param name="tenantConfig">The tenant configuration.</param>
        /// <param name="selectedPurseLabel">The selected purse label.</param>
        private async System.Threading.Tasks.Task ProcessSinglePurseAsync(InitialFundingRequestDto initialFundingRequest, 
            FundingConfigJson fundingConfig,
            List<TenantWalletDetailDto> masterWallets,
            TenantConfigDto tenantConfig,
            string selectedPurseLabel)
        {
            var purse = tenantConfig.PurseConfig?.Purses?.FirstOrDefault(p => p.PurseLabel == selectedPurseLabel);
            if (purse == null)
            {
                _logger.LogError("Purse configuration not found for label {PurseLabel}.", selectedPurseLabel);
                throw new ServiceValidationException($"Purse configuration not found for label {selectedPurseLabel}.", 400);
            }

            var fundingRule = fundingConfig.FundingRules.FirstOrDefault(rule => rule.ConsumerWalletType == purse.WalletType );
            if (fundingRule == null)
            {
                _logger.LogError("No funding rule found for wallet type {WalletType}.", purse.WalletType);
                throw new ServiceValidationException($"No funding rule found for wallet type {purse.WalletType}.", 400);
            }

            var (isValid, error) =await ValidateFundingRules(fundingRule, initialFundingRequest);

            if (!isValid)
            {
                _logger.LogError("Funding rules validation failed for TenantCode: {TenantCode}", initialFundingRequest.TenantCode);
                throw new ServiceValidationException(error!, StatusCodes.Status400BadRequest);
            }

            var redemptionVendorCode = GetRedemptionVendorCode(masterWallets, purse.MasterRedemptionWalletType);
            if (string.IsNullOrEmpty(redemptionVendorCode))
            {
                _logger.LogError("Redemption vendor code not found for wallet type {WalletType}.", purse.MasterRedemptionWalletType);
                throw new ServiceValidationException($"Redemption vendor code not found for wallet type {purse.MasterRedemptionWalletType}.", 400);
            }

            if (await HasFundingHistoryAsync(initialFundingRequest, fundingRule))
            {
                var msg = string.Format("Funding already exists for TenantCode: {0}, ConsumerCode: {1}, RuleNumber: {2}.",
                    initialFundingRequest.TenantCode, initialFundingRequest.ConsumerCode, fundingRule.RuleNumber);
                _logger.LogInformation(msg);
                throw new ServiceValidationException(msg, 400);
            }

            await FundPurseAsync(initialFundingRequest, fundingRule, redemptionVendorCode, purse);
        }

        /// <summary>
        /// Determines whether [has funding history asynchronous] [the specified initial funding request].
        /// </summary>
        /// <param name="initialFundingRequest">The initial funding request.</param>
        /// <param name="fundingRule">The funding rule.</param>
        /// <returns>
        ///   <c>true</c> if [has funding history asynchronous] [the specified initial funding request]; otherwise, <c>false</c>.
        /// </returns>
        private async Task<bool> HasFundingHistoryAsync(InitialFundingRequestDto initialFundingRequest, FundingRule fundingRule)
        {
            var parameters = new Dictionary<string, long>();
            var fundingHistoryResponse = await _fisClient.Get<FundingHistoryResponseDto>(
                $"{Constant.FundingHistoryAPIUrl}/{initialFundingRequest.TenantCode}/{initialFundingRequest.ConsumerCode}", parameters);

            if (fundingHistoryResponse.ErrorCode != null)
            {
                _logger.LogError(
                    "Error fetching funding history for TenantCode: {TenantCode}, ConsumerCode: {ConsumerCode}.",
                    initialFundingRequest.TenantCode, initialFundingRequest.ConsumerCode);
                return false;
            }

            return fundingHistoryResponse.FundingHistoryList?.Any(
                history => history.TenantCode == initialFundingRequest.TenantCode
                           && history.ConsumerCode == initialFundingRequest.ConsumerCode
                           && history.FundRuleNumber == fundingRule.RuleNumber) == true;
        }

        /// <summary>
        /// Funds the purse asynchronous.
        /// </summary>
        /// <param name="initialFundingRequest">The initial funding request.</param>
        /// <param name="fundingRule">The funding rule.</param>
        /// <param name="redemptionVendorCode">The redemption vendor code.</param>
        /// <param name="purse">The purse.</param>
        private async System.Threading.Tasks.Task FundPurseAsync(
            InitialFundingRequestDto initialFundingRequest,
            FundingRule fundingRule,
            string redemptionVendorCode,
            PurseDto purse)
        {
            var purseFundingRequest = new PurseFundingRequestDto
            {
                TenantCode = initialFundingRequest.TenantCode!,
                ConsumerCode = initialFundingRequest.ConsumerCode!,
                ConsumerWalletType = fundingRule.ConsumerWalletType,
                MasterWalletType = fundingRule.MasterWalletType,
                Amount = (long)fundingRule.Amount,
                RuleNumber = fundingRule.RuleNumber,
                TransactionDetailType = Constant.BenefitTransactionDetailType,
                RewardDescription = fundingRule.RuleDescription
            };

            var fundingResponse = await _walletClient.Post<PurseFundingResponseDto>(Constant.PurseFundingAPIUrl, purseFundingRequest);
            if (fundingResponse.ErrorCode != null)
            {
                _logger.LogError("Funding failed for RuleNumber: {RuleNumber}, Error: {ErrorMessage}", fundingRule.RuleNumber, fundingResponse.ErrorMessage);
                throw new ServiceValidationException($"Funding failed for RuleNumber: {fundingRule.RuleNumber}, Error: {fundingResponse.ErrorMessage}", 400);
            }

            await SaveFundingHistoryAsync(initialFundingRequest, fundingRule);

            var fundTransferRequest = new FundTransferRequestDto
            {
                TenantCode = initialFundingRequest.TenantCode,
                ConsumerCode = initialFundingRequest.ConsumerCode,
                ConsumerWalletTypeCode = purse.WalletType,
                RedemptionWalletTypeCode = purse.MasterRedemptionWalletType,
                RedemptionVendorCode = redemptionVendorCode,
                RedemptionAmount = (double)fundingRule.Amount,
                RedemptionRef = Guid.NewGuid().ToString("N"),
                RedemptionItemDescription = $"Transferred to {purse.PurseLabel} purse",
                PurseWalletType = purse.PurseWalletType
            };

            await FundTransferToPurse(fundTransferRequest);
        }

        /// <summary>
        /// Saves the funding history asynchronous.
        /// </summary>
        /// <param name="initialFundingRequest">The initial funding request.</param>
        /// <param name="fundingRule">The funding rule.</param>
        private async System.Threading.Tasks.Task SaveFundingHistoryAsync(InitialFundingRequestDto initialFundingRequest, FundingRule fundingRule)
        {
            var fundingHistoryDto = new FundingHistoryDto
            {
                TenantCode = initialFundingRequest.TenantCode,
                ConsumerCode = initialFundingRequest.ConsumerCode,
                FundRuleNumber = fundingRule.RuleNumber,
                FundTs = DateTime.UtcNow,
                FundingHistoryId = 0
            };

            var response = await _fisClient.Post<CreateFundingHistoryResponse>(Constant.FundingHistoryAPIUrl, fundingHistoryDto);
            if (response.ErrorCode != null)
            {
                _logger.LogError("Error saving funding history for RuleNumber: {RuleNumber}, Error: {ErrorMessage}", fundingRule.RuleNumber, response.ErrorMessage);
                throw new ServiceValidationException($"Error saving funding history for RuleNumber: {fundingRule.RuleNumber}, Error: {response.ErrorMessage}");
            }
        }

        /// <summary>
        /// Gets the master wallets asynchronous.
        /// </summary>
        /// <param name="tenantCode">The tenant code.</param>
        /// <returns></returns>
        /// <exception cref="SunnyRewards.Helios.Admin.Infrastructure.Exceptions.ServiceValidationException">
        /// Master wallets not found for TenantCode {tenantCode}.
        /// </exception>
        private async Task<List<TenantWalletDetailDto>> GetMasterWalletsAsync(string tenantCode)
        {
            var parameters = new Dictionary<string, long>();
            var response = await _walletClient.Get<GetAllMasterWalletsResponseDto>($"{Constant.MasterWallet}/{tenantCode}", parameters);
            if (response.ErrorCode != null)
            {
                throw new ServiceValidationException(response.ErrorMessage ?? string.Empty, response.ErrorCode);
            }

            if (response.MasterWallets == null || !response.MasterWallets.Any())
            {
                throw new ServiceValidationException($"Master wallets not found for TenantCode {tenantCode}.", StatusCodes.Status404NotFound);
            }

            return response.MasterWallets;
        }

        /// <summary>
        /// Funds the transfer to purse.
        /// </summary>
        /// <param name="fundTransferRequestDto">The fund transfer request dto.</param>
        private async System.Threading.Tasks.Task FundTransferToPurse(FundTransferRequestDto fundTransferRequestDto)
        {
            try
            {

                var postRedeemStartRequestDto = new Core.Domain.Dtos.PostRedeemStartRequestDto
                {
                    ConsumerWalletTypeCode = fundTransferRequestDto.ConsumerWalletTypeCode,
                    RedemptionWalletTypeCode = fundTransferRequestDto.RedemptionWalletTypeCode,
                    TenantCode = fundTransferRequestDto.TenantCode,
                    ConsumerCode = fundTransferRequestDto.ConsumerCode,
                    RedemptionVendorCode = fundTransferRequestDto.RedemptionVendorCode,
                    RedemptionAmount = fundTransferRequestDto.RedemptionAmount,
                    RedemptionRef = fundTransferRequestDto.RedemptionRef,
                    RedemptionItemDescription = fundTransferRequestDto.RedemptionItemDescription
                };
                var redeemStartResponse = await _walletClient.Post<PostRedeemStartResponseDto>(SunnyRewards.Helios.Admin.Core.Domain.Dtos.Constants.Constant.WalletRedeemStartAPIUrl, postRedeemStartRequestDto);
                if (redeemStartResponse.ErrorCode != null)
                {
                    var msg = string.Format("{0}.UpdateTask.TransferTaskRewardAmountToRewardPurse: An error occurred while redeeming wallet balance of Consumer: {1}, Error: {2}, Error Code:{3}", className,
                        postRedeemStartRequestDto.ConsumerCode, redeemStartResponse.ErrorMessage, redeemStartResponse.ErrorCode);
                    _logger.LogInformation(msg, redeemStartResponse.ErrorCode);
                    throw new ServiceValidationException(msg, redeemStartResponse.ErrorCode);
                }
                var loadValueRequestDto = new LoadValueRequestDto
                {
                    TenantCode = fundTransferRequestDto.TenantCode,
                    ConsumerCode = fundTransferRequestDto.ConsumerCode,
                    PurseWalletType = fundTransferRequestDto.PurseWalletType,
                    Amount = fundTransferRequestDto.RedemptionAmount ?? 0,
                    Currency = SunnyRewards.Helios.Admin.Core.Domain.Dtos.Constants.Constant.Currency_USD,
                    MerchantName = Constant.MerchantNameForInitialValueLoad
                };

                var loadValueResponse = await PerformLoadValueWithRetries(loadValueRequestDto, fundTransferRequestDto.ConsumerCode);

                if (loadValueResponse == null || loadValueResponse.ErrorCode != null)
                {
                    var postRedeemFailRequestDto = new PostRedeemFailRequestDto()
                    {
                        TenantCode = fundTransferRequestDto.TenantCode,
                        ConsumerCode = fundTransferRequestDto.ConsumerCode,
                        RedemptionVendorCode = fundTransferRequestDto.RedemptionVendorCode,
                        RedemptionAmount = fundTransferRequestDto.RedemptionAmount ?? 0,
                        RedemptionRef = fundTransferRequestDto.RedemptionRef,
                        Notes = fundTransferRequestDto.Notes
                    };
                    var redeemFailResponse = await _walletClient.Post<PostRedeemFailResponseDto>(SunnyRewards.Helios.Admin.Core.Domain.Dtos.Constants.Constant.WalletRedeemFailAPIUrl, postRedeemFailRequestDto);

                    var msg = String.Format("Perform Load Value Fails,- return Errorcode: {0}, RedeemFail called", loadValueResponse?.ErrorCode ?? 404);
                    throw new ServiceValidationException(msg, 500);
                }
                if (loadValueResponse != null && (loadValueResponse?.ErrorCode == null || loadValueResponse?.ErrorCode == StatusCodes.Status200OK))
                {
                    var postRedeemCompleteRequestDto = new PostRedeemCompleteRequestDto()
                    {
                        ConsumerCode = postRedeemStartRequestDto.ConsumerCode,
                        RedemptionVendorCode = postRedeemStartRequestDto.RedemptionVendorCode,
                        RedemptionRef = postRedeemStartRequestDto.RedemptionRef
                    };

                    var redeemSuccessResponse = await _walletClient.Post<PostRedeemCompleteResponseDto>(SunnyRewards.Helios.Admin.Core.Domain.Dtos.Constants.Constant.WalletRedeemCompleteAPIUrl, postRedeemCompleteRequestDto);
                    _logger.LogInformation("{className}.UpdateTask: Successfully redeem completed for Consumer: {ConsumerCode}", className, postRedeemStartRequestDto.ConsumerCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{className}.UpdateTask: Error occurred while transferring reward amount to reward purse, ErrorMessage - {errorMessage}", className, ex.Message);
                throw;
            }

        }

        /// <summary>
        /// Performs the load value with retries.
        /// </summary>
        /// <param name="loadValueRequestDto">The load value request dto.</param>
        /// <param name="consumerCode">The consumer code.</param>
        /// <returns></returns>
        private async Task<LoadValueResponseDto?> PerformLoadValueWithRetries(LoadValueRequestDto loadValueRequestDto, string? consumerCode)
        {
            var maxTries = WalletConstants.MaxTries;
            LoadValueResponseDto? loadValueResponse = null;
            while (maxTries > 0)
            {
                try
                {
                    loadValueResponse = await _fisClient.Post<LoadValueResponseDto>(SunnyRewards.Helios.Admin.Core.Domain.Dtos.Constants.Constant.FISValueLoadAPIUrl, loadValueRequestDto);
                    if (loadValueResponse.ErrorCode == null)
                    {
                        break;
                    }

                    _logger.LogError("{className}.PerformLoadValueWithRetries: Response ErrorCode: {errCode} in Load value retrying count left={maxTries}, ConsumerCode: {consumerCode}", className, loadValueResponse.ErrorCode, maxTries,
                        consumerCode);
                    maxTries--;
                    await System.Threading.Tasks.Task.Delay(GetSecureRandomNumber(WalletConstants.RetryMinWaitMS, WalletConstants.RetryMaxWaitMS));

                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "{className}.PerformLoadValueWithRetries: Error occurred while Load value, retrying count left={maxTries}", className, maxTries);
                    maxTries--;
                    await System.Threading.Tasks.Task.Delay(GetSecureRandomNumber(WalletConstants.RetryMinWaitMS, WalletConstants.RetryMaxWaitMS));
                }
            }

            return loadValueResponse;
        }

        /// <summary>
        /// Gets the secure random number.
        /// </summary>
        /// <param name="minValue">The minimum value.</param>
        /// <param name="maxValue">The maximum value.</param>
        /// <returns></returns>
        private int GetSecureRandomNumber(int minValue, int maxValue)
        {
            var buffer = new byte[4];
            RandomNumberGenerator.Fill(buffer);
            int result = BitConverter.ToInt32(buffer, 0) & int.MaxValue;
            return minValue + (result % (maxValue - minValue));
        }

        /// <summary>
        /// Gets the redemption vendor code.
        /// </summary>
        /// <param name="masterWallets">The master wallets.</param>
        /// <param name="redemptionWalletTypeCode">The redemption wallet type code.</param>
        /// <returns></returns>
        private string GetRedemptionVendorCode(List<TenantWalletDetailDto>? masterWallets, string? redemptionWalletTypeCode)
        {
            string redemptionVendorCode = string.Empty;
            var rewardRemdeptionWalletTypeCode = GetRewardRedemptionWalletTypeCode();
            
            var masterWallet = masterWallets?.FirstOrDefault(x => x.WalletType?.WalletTypeCode == redemptionWalletTypeCode);
            if (redemptionWalletTypeCode == rewardRemdeptionWalletTypeCode)
            {
                masterWallet = masterWallets?.FirstOrDefault(x => x.WalletType?.WalletTypeCode == redemptionWalletTypeCode
                && x.Wallet?.WalletName == Constant.RewardSuspenseWalletName);
            }
            if (masterWallet == null)
            {
                return redemptionVendorCode;
            }
            var masterwalletName = masterWallet?.Wallet?.WalletName;
            redemptionVendorCode = masterwalletName?.Split(':').ElementAtOrDefault(1) ?? string.Empty;
            return redemptionVendorCode;
        }

        private string? GetRewardRedemptionWalletTypeCode()
        {
            return _configuration.GetSection("Health_Actions_Redemption_Wallet_Type_Code").Value;
        }
    }
}
