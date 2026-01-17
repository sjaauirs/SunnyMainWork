using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SunnyBenefits.Fis.Core.Domain.Dtos;
using SunnyBenefits.Fis.Core.Domain.Enums;
using SunnyRewards.Helios.Admin.Core.Domain.Constants;
using SunnyRewards.Helios.Admin.Core.Domain.Dtos;
using SunnyRewards.Helios.Admin.Infrastructure.HttpClients.Interfaces;
using SunnyRewards.Helios.Admin.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.Cohort.Core.Domain.Dtos;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.Tenant.Core.Domain.Dtos;
using SunnyRewards.Helios.Tenant.Core.Domain.Dtos.Json;
using SunnyRewards.Helios.User.Core.Domain.Dtos;
using System.Text.Json;
using SunnyRewards.Helios.User.Core.Domain.enums;

namespace SunnyRewards.Helios.Admin.Infrastructure.Services
{
    public class AgreementsVerifiedEventService : IAgreementsVerifiedEventService
    {
        private readonly ILogger<AgreementsVerifiedEventService> _logger;
        private readonly IUserClient _userClient;
        private readonly ITenantClient _tenantClient;
        private readonly IFisClient _fisClient;
        private readonly ICohortClient _cohortClient;

        private const string ClassName = nameof(AgreementsVerifiedEventService);

        public AgreementsVerifiedEventService(
            ILogger<AgreementsVerifiedEventService> logger, 
            IUserClient userClient, 
            ITenantClient tenantClient, 
            IFisClient fisClient, 
            ICohortClient cohortClient)
        {
            _logger = logger;
            _userClient = userClient;
            _tenantClient = tenantClient;
            _fisClient = fisClient;
            _cohortClient = cohortClient;
        }

        /// <summary>
        /// Process AgreementsVerified event - checks consumer and cohort agreement status
        /// and sets card issue status to ELIGIBLE_TO_ORDER if both are AGREEMENTS_VERIFIED
        /// </summary>
        /// <param name="agreementsVerifiedEventRequestDto">dto for agreements verified event.</param>
        /// <returns></returns>
        public BaseResponseDto AgreementsVerifiedEventProcess(AgreementsVerifiedEventRequestDto agreementsVerifiedEventRequestDto)
        {
            const string MethodName = nameof(AgreementsVerifiedEventProcess);

            // Input validation
            if (string.IsNullOrWhiteSpace(agreementsVerifiedEventRequestDto.TenantCode) || 
                string.IsNullOrWhiteSpace(agreementsVerifiedEventRequestDto.ConsumerCode))
            {
                var errorMessage = "One or more required parameters are missing.";
                var errorCode = StatusCodes.Status400BadRequest;
                _logger.LogError("{ClassName}.{MethodName}: {ErrorMessage} ErrorCode: {ErrorCode}, TenantCode: {TenantCode}, ConsumerCode: {ConsumerCode}",
                    ClassName, MethodName, errorMessage, errorCode, agreementsVerifiedEventRequestDto.TenantCode, agreementsVerifiedEventRequestDto.ConsumerCode);

                return new BaseResponseDto
                {
                    ErrorCode = errorCode,
                    ErrorMessage = errorMessage
                };
            }

            try
            {
                // Call user API to get consumer details
                var consumerResp = _userClient.Post<GetConsumerResponseDto>(Constant.GetConsumerAPIUrl, new GetConsumerRequestDto
                {
                    ConsumerCode = agreementsVerifiedEventRequestDto.ConsumerCode
                }).GetAwaiter().GetResult();

                // Handle consumer not found
                if (consumerResp == null || consumerResp.Consumer == null || consumerResp.Consumer.ConsumerCode == null)
                {
                    var errorMessage = "Consumer not found or invalid consumer code.";
                    var errorCode = StatusCodes.Status404NotFound;
                    _logger.LogError("{ClassName}.{MethodName}: {ErrorMessage} ErrorCode: {ErrorCode}, ConsumerCode: {ConsumerCode}",
                        ClassName, MethodName, errorMessage, errorCode, agreementsVerifiedEventRequestDto.ConsumerCode);

                    return new BaseResponseDto
                    {
                        ErrorCode = errorCode,
                        ErrorMessage = errorMessage
                    };
                }

                // Check consumer agreement status
                if (consumerResp.Consumer.AgreementStatus != nameof(OnboardingState.VERIFIED))
                {
                    return new BaseResponseDto
                    {
                        ErrorCode = StatusCodes.Status400BadRequest,
                        ErrorMessage = $"Consumer agreement status is not VERIFIED. Current status: {consumerResp.Consumer.AgreementStatus}"
                    };
                }

                //Get Tenant options

                var tenant = _tenantClient.Post<TenantDto>( Constant.GetTenantByTenantCode, new GetTenantCodeRequestDto()
                { TenantCode = agreementsVerifiedEventRequestDto.TenantCode }).GetAwaiter().GetResult();

                TenantOptions? tenantOptions = GetTenantOptions(tenant);
                if(tenantOptions == null || tenantOptions.BenefitsOptions == null)
                {
                    return new BaseResponseDto()
                    {
                        ErrorCode = StatusCodes.Status500InternalServerError,
                        ErrorMessage = $"Tenant Options not defined for TenantCode :{agreementsVerifiedEventRequestDto.TenantCode}"
                    };
                }
                Core.Domain.Dtos.BenefitsOptions benefitsOptions = tenantOptions.BenefitsOptions;

                if (!benefitsOptions.DisableOnboardingFlow)
                {
                    return new BaseResponseDto()
                    {
                        ErrorCode = StatusCodes.Status500InternalServerError,
                        ErrorMessage = $"DisableOnboardingFlow is set to false for TenantCode :{agreementsVerifiedEventRequestDto.TenantCode}"
                    };
                }

                var requestDto = new ConsumerCohortsRequestDto() { ConsumerCode = agreementsVerifiedEventRequestDto.ConsumerCode, TenantCode = agreementsVerifiedEventRequestDto.TenantCode };

                var consumerCohorts = _cohortClient.Post<CohortsResponseDto>("consumer-cohorts", requestDto).GetAwaiter().GetResult();

                // Collect cohort codes for the consumer
                var consumerCohortCodes = consumerCohorts?.Cohorts?
                    .Select(c => c.CohortCode)
                    .Where(c => !string.IsNullOrWhiteSpace(c))
                    .ToList() ?? new List<string>();

                string? selectedFlowType = null;

                // Match consumer cohorts to configured flow types
                foreach (var flow in benefitsOptions.CardIssueFlowType)
                {
                    if (flow.CohortCode.Any(code =>
                        consumerCohortCodes.Contains(code, StringComparer.OrdinalIgnoreCase)))
                    {
                        selectedFlowType = flow.FlowType;
                        break;
                    }
                }

                // Fallback: if only one flowType exists, use that
                if (selectedFlowType == null && benefitsOptions.CardIssueFlowType.Count == 1 && benefitsOptions.CardIssueFlowType[0].CohortCode.Count == 0)
                {
                    selectedFlowType = benefitsOptions.CardIssueFlowType[0].FlowType;
                }

                if (string.IsNullOrEmpty(selectedFlowType))
                {
                    return new BaseResponseDto()
                    {
                        ErrorCode = StatusCodes.Status500InternalServerError,
                        ErrorMessage = $"Unable to determine card issue flow type for consumer :{agreementsVerifiedEventRequestDto.ConsumerCode}"
                    };
                }
                _logger.LogInformation($"{ClassName}.{MethodName}: Selected Card Issue Flow Type: {selectedFlowType} for ConsumerCode: {agreementsVerifiedEventRequestDto.ConsumerCode}");

                // Only proceed if the selected flow type is AGREEMENTS_VERIFIED
                if (selectedFlowType != nameof(Core.Domain.Dtos.Enums.CardIssueFlowType.AGREEMENTS_VERIFIED))
                {
                    _logger.LogInformation("{ClassName}.{MethodName}: Selected flow type is not AGREEMENTS_VERIFIED. Flow type: {FlowType}. No action required for ConsumerCode: {ConsumerCode}",
                        ClassName, MethodName, selectedFlowType, agreementsVerifiedEventRequestDto.ConsumerCode);
                    
                    return new BaseResponseDto();
                }

                // Check current status of Consumer Account
                var consumerAccountResponse = GetConsumerAccount(agreementsVerifiedEventRequestDto);
                if (consumerAccountResponse.ConsumerAccount == null)
                {
                    return new BaseResponseDto()
                    {
                        ErrorCode = StatusCodes.Status400BadRequest,
                        ErrorMessage = $"Consumer Account not found for ConsumerCode: {agreementsVerifiedEventRequestDto.ConsumerCode} and Tenantcode : {agreementsVerifiedEventRequestDto.TenantCode}"
                    };
                }

                // Only update status when current status is NOT_ELIGIBLE else conflict
                if (consumerAccountResponse.ConsumerAccount.CardIssueStatus != nameof(CardIssueStatus.NOT_ELIGIBLE))
                {
                    return new BaseResponseDto()
                    {
                        ErrorCode = StatusCodes.Status409Conflict,
                        ErrorMessage = $"Consumer Account status is already {consumerAccountResponse.ConsumerAccount.CardIssueStatus} for ConsumerCode: {agreementsVerifiedEventRequestDto.ConsumerCode}, TenantCode: {agreementsVerifiedEventRequestDto.TenantCode}"
                    };
                }

                // Consumer has AGREEMENTS_VERIFIED status and selected flow type is AGREEMENTS_VERIFIED, update card issue status
                _logger.LogInformation("{ClassName}.{MethodName}: Consumer has AGREEMENTS_VERIFIED status and selected flow type is AGREEMENTS_VERIFIED. Updating card issue status to ELIGIBLE_TO_ORDER for ConsumerCode: {ConsumerCode}",
                    ClassName, MethodName, agreementsVerifiedEventRequestDto.ConsumerCode);

                return UpdateConsumerAccountCardIssueStatus(agreementsVerifiedEventRequestDto);
            }
            catch (Exception ex)
            {
                var errorCode = StatusCodes.Status500InternalServerError;
                _logger.LogError(ex, "{ClassName}.{MethodName}: Exception occurred during AgreementsVerified event processing. ConsumerCode: {ConsumerCode}, TenantCode: {TenantCode}. ErrorMessage: {ErrorMessage}, StackTrace: {StackTrace}, ErrorCode: {ErrorCode}",
                    ClassName, MethodName, agreementsVerifiedEventRequestDto.ConsumerCode, agreementsVerifiedEventRequestDto.TenantCode, ex.Message, ex.StackTrace, errorCode);

                return new BaseResponseDto
                {
                    ErrorCode = errorCode,
                    ErrorMessage = $"An unexpected error occurred during AgreementsVerified event processing. ErrorMessage: {ex.Message}"
                };
            }
        }

        private BaseResponseDto UpdateConsumerAccountCardIssueStatus(AgreementsVerifiedEventRequestDto requestDto)
        {
            var consumerAccount = _fisClient.Put<ConsumerAccountResponseDto>(Constant.UpdateCardIssueStatus, new UpdateCardIssueRequestDto()
            {
                ConsumerCode = requestDto.ConsumerCode,
                TenantCode = requestDto.TenantCode,
                TargetCardIssueStatus = nameof(CardIssueStatus.ELIGIBLE_TO_ORDER)
            }).GetAwaiter().GetResult();

            if (consumerAccount.ErrorCode != null)
            {
                return new BaseResponseDto()
                {
                    ErrorCode = consumerAccount.ErrorCode,
                    ErrorMessage = consumerAccount.ErrorMessage
                };
            }

            return new BaseResponseDto();
        }

        private GetConsumerAccountResponseDto GetConsumerAccount(AgreementsVerifiedEventRequestDto requestDto)
        {
            var consumerAccount = _fisClient.Post<GetConsumerAccountResponseDto>(Constant.GetConsumerAccount, new GetConsumerAccountRequestDto()
            {
                ConsumerCode = requestDto.ConsumerCode,
                TenantCode = requestDto.TenantCode
            }).GetAwaiter().GetResult();

            if (consumerAccount.ErrorCode != null)
            {
                return new GetConsumerAccountResponseDto()
                {
                    ErrorCode = consumerAccount.ErrorCode,
                    ErrorMessage = consumerAccount.ErrorMessage,
                    ConsumerAccount = null
                };
            }

            return consumerAccount;
        }

        private TenantOptions? GetTenantOptions(TenantDto tenant)
        {
            if (string.IsNullOrWhiteSpace(tenant.TenantOption))
            {
                return null;
            }

            try
            {
                return JsonSerializer.Deserialize<TenantOptions>(tenant.TenantOption, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });
            }
            catch (JsonException ex)
            {
                _logger?.LogError(ex, "Failed to deserialize TenantOptions from TenantOption JSON: {TenantOption}", tenant.TenantOption);
                return null;
            }
        }
    }
}
