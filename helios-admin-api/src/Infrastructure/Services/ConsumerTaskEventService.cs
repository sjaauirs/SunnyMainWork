using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SunnyBenefits.Fis.Core.Domain.Dtos;
using SunnyBenefits.Fis.Core.Domain.Enums;
using SunnyRewards.Helios.Admin.Core.Domain.Constants;
using SunnyRewards.Helios.Admin.Core.Domain.Dtos;
using SunnyRewards.Helios.Admin.Core.Domain.Dtos.Enums;
using SunnyRewards.Helios.Admin.Infrastructure.HttpClients;
using SunnyRewards.Helios.Admin.Infrastructure.HttpClients.Interfaces;
using SunnyRewards.Helios.Admin.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.Cohort.Core.Domain.Dtos;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.Task.Core.Domain.Dtos;
using SunnyRewards.Helios.Tenant.Core.Domain.Dtos;
using SunnyRewards.Helios.Tenant.Core.Domain.Dtos.Json;
using SunnyRewards.Helios.User.Core.Domain.Dtos;
using System.Text.Json;

namespace SunnyRewards.Helios.Admin.Infrastructure.Services
{
    public class ConsumerTaskEventService : IConsumerTaskEventService
    {
        private readonly ILogger<ConsumerTaskEventService> _logger;
        private readonly ITaskClient _taskClient;
        private readonly IUserClient _userClient;
        private readonly ITenantClient _tenantClient;
        private readonly IFisClient _fisClient;
        private readonly ICohortClient _cohortClient;

        private const string ClassName = nameof(ConsumerTaskEventService);

        public ConsumerTaskEventService(ILogger<ConsumerTaskEventService> logger, ITaskClient taskClient, IUserClient userClient, ITenantClient tenantClient, IFisClient fisClient, ICohortClient cohortClient)
        {
            _logger = logger;
            _taskClient = taskClient;
            _userClient = userClient;
            _tenantClient = tenantClient;
            _fisClient = fisClient;
            _cohortClient = cohortClient;
        }

        /// <summary>
        /// Enrolls the consumer task.
        /// </summary>
        /// <param name="consumerTaskEventRequestDto">The consumerTaskEventRequestDto.</param>
        /// <returns></returns>
        public BaseResponseDto ConsumerTaskEventProcess(ConsumerTaskEventRequestDto consumerTaskEventRequestDto)
        {
            const string MethodName = nameof(ConsumerTaskEventProcess);

            // Input validation
            if (string.IsNullOrWhiteSpace(consumerTaskEventRequestDto.TenantCode) || string.IsNullOrWhiteSpace(consumerTaskEventRequestDto.ConsumerCode))
            {
                var errorMessage = "One or more required parameters are missing.";
                var errorCode = StatusCodes.Status400BadRequest;
                _logger.LogError("{ClassName}.{MethodName}: {ErrorMessage} ErrorCode: {ErrorCode}, TenantCode: {TenantCode}, ConsumerCode: {ConsumerCode}",
                    ClassName, MethodName, errorMessage, errorCode, consumerTaskEventRequestDto.TenantCode, consumerTaskEventRequestDto.ConsumerCode);

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
                    ConsumerCode = consumerTaskEventRequestDto.ConsumerCode
                }).GetAwaiter().GetResult();

                // Handle consumer not found
                if (consumerResp == null || consumerResp.Consumer == null || consumerResp.Consumer.ConsumerCode == null)
                {
                    var errorMessage = "Consumer not found or invalid consumer code.";
                    var errorCode = StatusCodes.Status404NotFound;
                    _logger.LogError("{ClassName}.{MethodName}: {ErrorMessage} ErrorCode: {ErrorCode}, ConsumerCode: {ConsumerCode}",
                        ClassName, MethodName, errorMessage, errorCode, consumerTaskEventRequestDto.ConsumerCode);

                    return new BaseResponseDto
                    {
                        ErrorCode = errorCode,
                        ErrorMessage = errorMessage
                    };
                }

                // Handle mismatched tenant code
                if (consumerResp.Consumer.TenantCode != consumerTaskEventRequestDto.TenantCode)
                {
                    var errorMessage = "Tenant code does not match the consumer's tenant.";
                    var errorCode = StatusCodes.Status404NotFound;
                    _logger.LogError("{ClassName}.{MethodName}: {ErrorMessage} ErrorCode: {ErrorCode}, TenantCode: {TenantCode}, ConsumerCode: {ConsumerCode}",
                        ClassName, MethodName, errorMessage, errorCode, consumerTaskEventRequestDto.TenantCode, consumerTaskEventRequestDto.ConsumerCode);

                    return new BaseResponseDto
                    {
                        ErrorCode = errorCode,
                        ErrorMessage = errorMessage
                    };
                }

                //Get Tenant options

                var tenant = _tenantClient.Post<TenantDto>( Constant.GetTenantByTenantCode, new GetTenantCodeRequestDto()
                { TenantCode = consumerTaskEventRequestDto.TenantCode }).GetAwaiter().GetResult();

                TenantOptions? tenantOptions = getTenantOptions(tenant);
                if(tenantOptions == null || tenantOptions.BenefitsOptions == null)
                {
                    return new BaseResponseDto()
                    {
                        ErrorCode = StatusCodes.Status500InternalServerError,
                        ErrorMessage = $"Tenant Options not defined for TenantCode :{consumerTaskEventRequestDto.TenantCode}"
                    };
                }
                Core.Domain.Dtos.BenefitsOptions benefitsOptions = tenantOptions.BenefitsOptions;

                if (!benefitsOptions.DisableOnboardingFlow)
                {
                    return new BaseResponseDto()
                    {
                        ErrorCode = StatusCodes.Status500InternalServerError,
                        ErrorMessage = $"DisableOnboardingFlow is set to false for TenantCode :{consumerTaskEventRequestDto.TenantCode}"
                    };
                }

                var requestDto = new ConsumerCohortsRequestDto() { ConsumerCode = consumerTaskEventRequestDto.ConsumerCode, TenantCode = consumerTaskEventRequestDto.TenantCode };

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
                        ErrorMessage = $"Unable to determine card issue flow type for consumer :{consumerTaskEventRequestDto.ConsumerCode}"
                    };
                }
                _logger.LogInformation($"{ClassName}.{MethodName}: Selected Card Issue Flow Type: {selectedFlowType} for ConsumerCode: {consumerTaskEventRequestDto.ConsumerCode}");

                // for card Issue flow = immidiate - NO CHECK
                if (selectedFlowType == nameof(Core.Domain.Dtos.Enums.CardIssueFlowType.IMMIDIATE) || selectedFlowType == nameof(Core.Domain.Dtos.Enums.CardIssueFlowType.AGREEMENTS_VERIFIED))
                {
                    // no check for Immidiate
                    return new BaseResponseDto();
                }
                //check type of Card Issue Flow =TASK_COMPLETION_CHECK - Match TaskCompletionCheckCode
                else if (selectedFlowType == nameof(Core.Domain.Dtos.Enums.CardIssueFlowType.TASK_COMPLETION_CHECK))
                {
                    //get all consumer task for this consumer
                    var consumerTasksDto = new ConsumerTaskRequestDto()
                    {
                        ConsumerCode = consumerTaskEventRequestDto.ConsumerCode,
                        TenantCode = consumerTaskEventRequestDto.TenantCode
                    };

                    var consumerTaskResponse = _taskClient.Post<ConsumerTaskResponseDto>(Constant.GetAllConsumerTasks, consumerTasksDto).GetAwaiter().GetResult();

                    if (consumerTaskResponse == null || consumerTaskResponse.CompletedTasks == null)
                    {
                        var errorMessage = $"Consumer has not completed any task has been Completed for ConsumerCode {consumerTasksDto.ConsumerCode}";
                        var errorCode = StatusCodes.Status404NotFound;
                        _logger.LogError("{ClassName}.{MethodName}: {ErrorMessage} ErrorCode: {ErrorCode}, ConsumerCode: {ConsumerCode}",
                            ClassName, MethodName, errorMessage, errorCode, consumerTaskEventRequestDto.ConsumerCode);

                        return new BaseResponseDto
                        {
                            ErrorCode = errorCode,
                            ErrorMessage = errorMessage
                        };
                    }

                    //get taskCompletionCheckCode

                    if (benefitsOptions?.TaskCompletionCheckCode == null || benefitsOptions.TaskCompletionCheckCode?.Count == 0)
                    {
                        var errorMessage = "Task Completion Check Code not defined in Tenant options";
                        var errorCode = StatusCodes.Status500InternalServerError;
                        _logger.LogError("{ClassName}.{MethodName}: {ErrorMessage} ErrorCode: {ErrorCode}, ConsumerCode: {ConsumerCode}",
                            ClassName, MethodName, errorMessage, errorCode, consumerTaskEventRequestDto.ConsumerCode);

                        return new BaseResponseDto
                        {
                            ErrorCode = errorCode,
                            ErrorMessage = errorMessage
                        };
                    }

                    // check current status of Consumer Account
                    var consumerAccountResponse = GetConsumerAccount(consumerTaskEventRequestDto);
                    if (consumerAccountResponse.ConsumerAccount == null)
                    {
                        return new BaseResponseDto()
                        {
                            ErrorCode = StatusCodes.Status400BadRequest,
                            ErrorMessage = $"Consumer Account not found for ConsumerCode: {consumerTaskEventRequestDto.ConsumerCode} and Tenantcode : {consumerTaskEventRequestDto.TenantCode}"
                        };
                    }

                    // only update status when current status is NOT_ELIGIBLE else conflict
                    // conflict events are not moved to Dead Letter queue
                    if (consumerAccountResponse.ConsumerAccount.CardIssueStatus != nameof(CardIssueStatus.NOT_ELIGIBLE))
                    {
                        return new BaseResponseDto()
                        {
                            ErrorCode = StatusCodes.Status409Conflict,
                            ErrorMessage = $"Consumer Account status is already {consumerAccountResponse.ConsumerAccount.CardIssueStatus} for  ConsumerCode: {consumerTaskEventRequestDto.ConsumerCode}, TenantCode: {consumerTaskEventRequestDto.TenantCode}"
                        };
                    }
                    // check if any task completed with reward amount > 0
                    //HAP

                    if (benefitsOptions.TaskCompletionCheckCode != null &&  benefitsOptions.TaskCompletionCheckCode.FirstOrDefault() == "ANY")
                    {
                        //Newlogic
                        if (consumerTaskResponse.CompletedTasks.Any(
                            t => t.TaskReward != null && TaskRewardDto.GetRewardDetails(t.TaskReward.Reward ?? string.Empty).RewardAmount > 0))
                        {
                        
                            return updateConsumerAccountCardIssueStatus(consumerTaskEventRequestDto);
                        }
                         else
                        {
                            return new BaseResponseDto
                            {
                                ErrorCode = StatusCodes.Status400BadRequest,
                                ErrorMessage = "Consumer completed tasks but none have reward amount greater than 0"
                            };
                        }
                    }
                    //KP - check if any task completed with reward code in the list of TaskCompletionCheckCode
                    else
                    {
                        var acceptedTaskRewards = benefitsOptions.TaskCompletionCheckCode!;
                        var anyTaskCompletedFromList = consumerTaskResponse.CompletedTasks
                            .Any(x => x.TaskReward != null && acceptedTaskRewards.Contains(x.TaskReward.TaskRewardCode!)&& TaskRewardDto.GetRewardDetails(x.TaskReward.Reward ?? string.Empty).RewardAmount > 0);

                        if (anyTaskCompletedFromList)
                        {
                            return updateConsumerAccountCardIssueStatus(consumerTaskEventRequestDto);
                        }
                        else
                        {
                            return new BaseResponseDto()
                            {
                                ErrorCode = StatusCodes.Status400BadRequest,
                                ErrorMessage = "Task matched TaskCompletionCheckCode but reward amount is 0, Consumer Account Card Issue Status not updated",
                            };
                        }
                    }//kp

                }


                return new BaseResponseDto();
            }
            catch (Exception ex)
            {
                var errorCode = StatusCodes.Status500InternalServerError;
                _logger.LogError(ex, "{ClassName}.{MethodName}: Exception occurred during consumer Task Update Event processing. ConsumerCode: {ConsumerCode}, TenantCode: {TenantCode}. ErrorMessage: {ErrorMessage}, StackTrace: {StackTrace}, ErrorCode: {ErrorCode}",
                    ClassName, MethodName, consumerTaskEventRequestDto.ConsumerCode, consumerTaskEventRequestDto.TenantCode, ex.Message, ex.StackTrace, errorCode);

                return new BaseResponseDto
                {
                    ErrorCode = errorCode,
                    ErrorMessage = $"An unexpected error occurred during  consumer Task Update Event processing. ErrorMessage: {ex.Message}"
                };
            }
        }

        private TenantOptions? getTenantOptions(TenantDto tenant)
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



        private BaseResponseDto updateConsumerAccountCardIssueStatus(ConsumerTaskEventRequestDto requestDto)
        {
            var consumerAccount = _fisClient.Put<ConsumerAccountResponseDto>(Constant.UpdateCardIssueStatus, new UpdateCardIssueRequestDto()
            {
                ConsumerCode = requestDto.ConsumerCode,
                TenantCode = requestDto.TenantCode,
                TargetCardIssueStatus = nameof(CardIssueStatus.ELIGIBLE_TO_ORDER)
            }).GetAwaiter().GetResult();

            if(consumerAccount.ErrorCode != null) {
                return new BaseResponseDto()
                {
                    ErrorCode = consumerAccount.ErrorCode,
                    ErrorMessage = consumerAccount.ErrorMessage
                };
            }

            return new BaseResponseDto();

        }

        private GetConsumerAccountResponseDto GetConsumerAccount(ConsumerTaskEventRequestDto requestDto)
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
    }
}