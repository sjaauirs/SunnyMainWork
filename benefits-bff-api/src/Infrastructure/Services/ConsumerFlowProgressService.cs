using Amazon.CloudWatch;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NHibernate.Util;
using Sunny.Benefits.Bff.Core.Constants;
using Sunny.Benefits.Bff.Core.Domain.Dtos;
using Sunny.Benefits.Bff.Infrastructure.Repositories.Interfaces;
using Sunny.Benefits.Bff.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.Tenant.Core.Domain.Dtos;
using SunnyRewards.Helios.Tenant.Core.Domain.Dtos.Json;
using SunnyRewards.Helios.User.Core.Domain.Constant;
using SunnyRewards.Helios.User.Core.Domain.Dtos;


namespace Sunny.Benefits.Bff.Infrastructure.Services
{
    public class ConsumerFlowProgressService : IConsumerFlowProgressService
    {
        private readonly ILogger<ConsumerFlowProgressService> _logger;
        private readonly IUserClient _userClient;
        private readonly ICohortConsumerService _cohortConsumerService;
        private readonly IFlowStepService _flowStepService;
        private const string _className = nameof(ConsumerFlowProgressService);

        public ConsumerFlowProgressService(ILogger<ConsumerFlowProgressService> logger, IUserClient userClient, 
            ICohortConsumerService cohortConsumerService, IFlowStepService flowStepService)
        {
            _logger = logger;
            _userClient = userClient;
            _cohortConsumerService = cohortConsumerService;
            _flowStepService = flowStepService;
        }
        /// <summary>
        /// Retrieves the consumer’s flow progress by fetching cohorts and invoking the user client.
        /// </summary>
        /// <param name="consumerFlowProgressRequest">The request containing tenant and consumer details.</param>
        /// <returns>A <see cref="ConsumerFlowProgressResponseDto"/> with the current flow progress.</returns>

        public async Task<OnboardingFlowStepsResponseDto> GetConsumerFlowProgressAsync(GetConsumerFlowRequestDto ConsumerFlowRequestDto, FlowResponseDto? flowSteps = null)
        {
            const string methodName = nameof(GetConsumerFlowProgressAsync);
            _logger.LogInformation("{_className}.{MethodName}: Fetching consumer cohorts for TenantCode: {TenantCode}, " +
                "ConsumerCode: {ConsumerCode}",_className, methodName, ConsumerFlowRequestDto.TenantCode, ConsumerFlowRequestDto.ConsumerCode);
            try
            {
                var result = new OnboardingFlowStepsResponseDto();

                var consumerCohorts = await _cohortConsumerService.GetConsumerAllCohorts(ConsumerFlowRequestDto.TenantCode, ConsumerFlowRequestDto.ConsumerCode);

                var consumerFlowProgressRequest = new ConsumerFlowProgressRequestDto() { 
                ConsumerCode = ConsumerFlowRequestDto.ConsumerCode,
                TenantCode  = ConsumerFlowRequestDto.TenantCode,
                };
                if (consumerCohorts == null || consumerCohorts.Cohorts.Count == 0)
                {
                    _logger.LogWarning("{_className}.{MethodName}: No cohorts mapped for TenantCode: {TenantCode}, ConsumerCode: {ConsumerCode}",
                        _className, methodName, ConsumerFlowRequestDto.TenantCode, ConsumerFlowRequestDto.ConsumerCode);

                    consumerFlowProgressRequest.CohortCodes = new List<string>();
                }
                else
                {
                    consumerFlowProgressRequest.CohortCodes = consumerCohorts.Cohorts
                        .Where(x => !string.IsNullOrEmpty(x.CohortCode))
                        .Select(x => x.CohortCode!)
                        .ToList();

                    _logger.LogInformation("{_className}.{MethodName}: Retrieved {CohortCount} cohorts for ConsumerCode: {ConsumerCode}",
                        _className, methodName, consumerFlowProgressRequest.CohortCodes.Count, consumerFlowProgressRequest.ConsumerCode);
                }

                _logger.LogInformation("{_className}.{MethodName}: Calling API: {ApiUrl} with ConsumerCode: {ConsumerCode}",
                    _className, methodName, CommonConstants.GetCurrentFlowStatusAPIUrl, consumerFlowProgressRequest.ConsumerCode);

                var response = await _userClient.Post<ConsumerFlowProgressResponseDto>(CommonConstants.GetCurrentFlowStatusAPIUrl, consumerFlowProgressRequest);

                if (response == null || response.ErrorCode != null)
                {
                    _logger.LogInformation("{_className}.{MethodName}: API call completed. Response ErrorCode: {ErrorCode}",
                                        _className, methodName, response?.ErrorCode);
                    result.ErrorCode = response?.ErrorCode ?? 404;
                    return result;
                }

                if (flowSteps == null)
                {
                    flowSteps = await GetFlowAndFlowSteps(consumerFlowProgressRequest.TenantCode, consumerFlowProgressRequest.ConsumerCode, response!.ConsumerFlowProgress.FlowFk, methodName);
                }

                result.ConsumerCode = consumerFlowProgressRequest.ConsumerCode;
                result.TenantCode = consumerFlowProgressRequest.TenantCode;

                FlowStepDto? currentStep = null;
                FlowStepDto? successStep = null;
                FlowStepDto? failedStep = null;
                bool skipSteps = false;

                if (response.ConsumerFlowProgress.Status == "NOT_STARTED")
                {
                    (currentStep, successStep, failedStep, skipSteps) = ResolveSteps(flowSteps, flowSteps.Steps.FirstOrDefault()?.StepId);
                }
                else
                {
                    (currentStep, successStep, failedStep, skipSteps) = ResolveSteps(flowSteps, response.ConsumerFlowProgress.FlowStepPk);
                }

                result.OnboardingFlowStatus = response.ConsumerFlowProgress.Status!;
                result.FlowId = flowSteps.FlowId;
                result.CurrentStepName = currentStep?.ComponentName?? "";
                result.CurrentStepId = currentStep?.StepId ?? 0;
                result.FailedStepName = failedStep?.ComponentName??"";
                result.SuccessStepName = successStep?.ComponentName??"";
                result.canSkipStep = skipSteps;

                return result;

            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private static (FlowStepDto? current, FlowStepDto? success, FlowStepDto? failed , bool skipStep)  ResolveSteps(
            FlowResponseDto flow, long? currentStepId)
        {
            bool skip = false;
            var currentStep = flow.Steps.FirstOrDefault(s => s.StepId == currentStepId);

            if (currentStep != null && !string.IsNullOrEmpty(currentStep.StepConfigJson))
            {
                var stepConfig = System.Text.Json.JsonSerializer.Deserialize<StepConfigDto>(currentStep.StepConfigJson);
                skip = stepConfig?.SkipSteps ?? false;
            }

            var successStep = currentStep?.OnSuccessStepId != null
                ? flow.Steps.FirstOrDefault(s => s.StepId == currentStep.OnSuccessStepId)
                : null;

            var failedStep = currentStep?.OnFailureStepId != null
                ? flow.Steps.FirstOrDefault(s => s.StepId == currentStep.OnFailureStepId)
                : null;

            return (currentStep, successStep, failedStep, skip);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="consumerFlowStatusRequestDto"></param>
        /// <returns></returns>
        public async Task<OnboardingFlowStepsResponseDto> UpdateConsumerFlowStatusAsync(UpdateConsumerFlowRequestDto consumerFlowStatusRequestDto)
        {
            const string methodName = nameof(UpdateConsumerFlowStatusAsync);

            _logger.LogInformation(
                "{Class}.{Method}: Starting flow status update. Status:{Status}, ConsumerCode:{ConsumerCode}, TenantCode:{TenantCode}",
                _className, methodName, consumerFlowStatusRequestDto.Status,
                consumerFlowStatusRequestDto.ConsumerCode, consumerFlowStatusRequestDto.TenantCode);

            try
            {
                OnboardingFlowStepsResponseDto response = new OnboardingFlowStepsResponseDto();
                ConsumerFlowProgressResponseDto consumerFlowProgressResponseDto;

                FlowResponseDto flowSteps = await GetFlowAndFlowSteps(consumerFlowStatusRequestDto.TenantCode, consumerFlowStatusRequestDto.ConsumerCode, consumerFlowStatusRequestDto.FlowId, methodName);

                // Handle Skip flow with custom logic
                if (IsSkipStatus(consumerFlowStatusRequestDto.Status))
                {
                    _logger.LogInformation("{Class}.{Method}: Handling Skip flow for ConsumerCode:{ConsumerCode}, TenantCode:{TenantCode}",
                        _className, methodName, consumerFlowStatusRequestDto.ConsumerCode, consumerFlowStatusRequestDto.TenantCode);

                    consumerFlowProgressResponseDto = await HandleSkipFlowAsync(consumerFlowStatusRequestDto, flowSteps , methodName);
                }
                else
                {
                    // Default handling: just post the flow status
                    consumerFlowProgressResponseDto = await PostFlowStatusAsync(consumerFlowStatusRequestDto, flowSteps);
                }

                // Validate response
                if (consumerFlowProgressResponseDto.ErrorCode != null)
                {
                    _logger.LogError(
                        "{Class}.{Method}: Error while updating flow status. ConsumerCode:{ConsumerCode}, TenantCode:{TenantCode}, " +
                        "ErrorCode:{ErrorCode}, ErrorMessage:{ErrorMessage}",_className, methodName,
                        consumerFlowStatusRequestDto.ConsumerCode,consumerFlowStatusRequestDto.TenantCode, consumerFlowProgressResponseDto.ErrorCode, consumerFlowProgressResponseDto.ErrorMessage);

                    response.ErrorCode = consumerFlowProgressResponseDto.ErrorCode;
                    return response;
                }

                _logger.LogInformation(
                    "{Class}.{Method}: Successfully updated flow status. ConsumerCode:{ConsumerCode}, TenantCode:{TenantCode}, FinalStatus:{Status}",
                    _className, methodName,consumerFlowStatusRequestDto.ConsumerCode,consumerFlowStatusRequestDto.TenantCode,consumerFlowStatusRequestDto.Status);


                //next Step
                var request = new GetConsumerFlowRequestDto()
                {
                    ConsumerCode = consumerFlowStatusRequestDto.ConsumerCode,
                    TenantCode = consumerFlowStatusRequestDto.TenantCode,
                };
                response = await GetConsumerFlowProgressAsync(request , flowSteps);
                return response;


            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "{Class}.{Method}: Exception occurred while updating flow status. ConsumerCode:{ConsumerCode}, TenantCode:{TenantCode}, FlowStep:{FlowStep}, Status:{Status}, ERROR:{Message}",
                    _className, methodName,consumerFlowStatusRequestDto.ConsumerCode,consumerFlowStatusRequestDto.TenantCode,consumerFlowStatusRequestDto.CurrentStepId,
                    consumerFlowStatusRequestDto.Status,ex.Message);

                throw;
            }
        }

        #region Helper Methods

        /// <summary>
        /// Checks whether the given status is "Skip"
        /// </summary>
        private static bool IsSkipStatus(string status) =>
            status.Equals(CommonConstants.Skip, StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// Handles Skip flow logic by fetching flow steps, identifying current step, and applying skip rules.
        /// </summary>
        private async Task<ConsumerFlowProgressResponseDto> HandleSkipFlowAsync(UpdateConsumerFlowRequestDto consumerFlowStatusRequestDto, FlowResponseDto flowSteps, string methodName)
        {
            // Identify the current step using FromFlowStepId
            var currentStep = IdentifyCurrentStep(flowSteps, consumerFlowStatusRequestDto);
            

            if (currentStep == null || currentStep.StepConfigJson == null)
            {
                _logger.LogWarning("{Class}.{Method}: No current step found or StepConfig is empty. Defaulting to PostFlowStatus.",
                    _className, methodName);
                return await PostFlowStatusAsync(consumerFlowStatusRequestDto, flowSteps);
            }
            var stepconfig = System.Text.Json.JsonSerializer.Deserialize<StepConfigDto>(currentStep.StepConfigJson);

            _logger.LogInformation("{Class}.{Method}: Processing Skip logic for StepId:{StepId}, ConsumerCode:{ConsumerCode}",
                _className, methodName, currentStep.StepId, consumerFlowStatusRequestDto.ConsumerCode);

            return await ProcessSkipLogicAsync(stepconfig, flowSteps, consumerFlowStatusRequestDto);
        }

        private async Task<FlowResponseDto> GetFlowAndFlowSteps( string tenantCode,string consumerCode, long flowId,  string methodName)
        {
            // Build initial request DTO
            var requestDto = new FlowRequestDto
            {
                TenantCode = tenantCode,
                FlowId = flowId,
                FlowName = "onboarding_flow"
            };

            // Add cohort details
            requestDto = await BuildFlowRequestAsync(tenantCode, consumerCode, methodName, requestDto);

            // Fetch all flow steps
            var flowSteps = await _flowStepService.GetFlowSteps(requestDto);
            return flowSteps;
        }

        /// <summary>
        /// Adds cohort codes to the request DTO.
        /// </summary>
        private async Task<FlowRequestDto> BuildFlowRequestAsync(string tenantCode, string consumerCode, string methodName, FlowRequestDto requestDto)
        {
            var consumerCohorts = await _cohortConsumerService.GetConsumerAllCohorts(
                tenantCode, consumerCode);

            if (consumerCohorts == null || consumerCohorts.Cohorts.Count == 0)
            {
                _logger.LogWarning("{Class}.{Method}: No cohorts mapped. TenantCode:{TenantCode}, ConsumerCode:{ConsumerCode}",
                    _className, methodName, tenantCode, consumerCode);

                requestDto.CohortCodes = new List<string>();
            }
            else
            {
                requestDto.CohortCodes = consumerCohorts.Cohorts
                    .Where(x => !string.IsNullOrEmpty(x.CohortCode))
                    .Select(x => x.CohortCode!)
                    .ToList();

                _logger.LogInformation("{Class}.{Method}: Retrieved {CohortCount} cohorts for ConsumerCode:{ConsumerCode}",
                    _className, methodName, requestDto.CohortCodes.Count, consumerCode);
            }

            return requestDto;
        }

        /// <summary>
        /// Finds the current step in flow steps using FromFlowStepId.
        /// </summary>
        private static FlowStepDto? IdentifyCurrentStep(FlowResponseDto flowSteps, UpdateConsumerFlowRequestDto consumerFlowStatusRequestDto) =>
            flowSteps.Steps.FirstOrDefault(x => x.StepId == consumerFlowStatusRequestDto.CurrentStepId);

        /// <summary>
        /// Processes skip logic across connected components, updating status as skipped, in-progress, or completed.
        /// </summary>
        private async Task<ConsumerFlowProgressResponseDto> ProcessSkipLogicAsync(StepConfigDto? stepConfig, FlowResponseDto flowSteps, UpdateConsumerFlowRequestDto consumerFlowStatusRequestDto)
        {
            // Skipping parent record
            consumerFlowStatusRequestDto.Status = CommonConstants.Skip.ToLower();
            await PostFlowStatusAsync(consumerFlowStatusRequestDto, flowSteps);

            for (int i = 0; i < stepConfig?.ConnectedComponent?.Count; i++)
            {
                var componentId = stepConfig.ConnectedComponent[i];

                // Intermediate component - mark as skipped and move forward
                consumerFlowStatusRequestDto.Status = CommonConstants.Skip.ToLower();
                consumerFlowStatusRequestDto.CurrentStepId = componentId;

                _logger.LogInformation("{Class}.{Method}: Skipping component. FromStepId:{FromStep}, Status:{Status}",
                    _className, nameof(ProcessSkipLogicAsync), consumerFlowStatusRequestDto.CurrentStepId,
                     consumerFlowStatusRequestDto.Status);

                await PostFlowStatusAsync(consumerFlowStatusRequestDto,  flowSteps);
            }

            return new ConsumerFlowProgressResponseDto();
        }

        /// <summary>
        /// Posts flow status update to API.
        /// </summary>
        private async Task<ConsumerFlowProgressResponseDto> PostFlowStatusAsync(UpdateConsumerFlowRequestDto requestDto , FlowResponseDto flowSteps)
        {
            _logger.LogDebug("{Class}.{Method}: Posting flow status update. Status:{Status}, FromStep:{FromStep}",
                _className, nameof(PostFlowStatusAsync), requestDto.Status, requestDto.CurrentStepId);


            var lastStepId = flowSteps.Steps.MaxBy(s => s.StepIdx)?.StepId;

            var status = requestDto.Status;
            if (requestDto.CurrentStepId == lastStepId)
            {
                status = CommonConstants.Completed;
            }

            var toStepId = flowSteps.Steps.FirstOrDefault(s => s.StepId == requestDto.CurrentStepId)?.OnSuccessStepId;


            var updateRequest = new UpdateFlowStatusRequestDto()
            {
                TenantCode = requestDto.TenantCode,
                ConsumerCode = requestDto.ConsumerCode,
                FromFlowStepId = requestDto.CurrentStepId,
                FlowId = requestDto.FlowId,
                Status = status,
                ToFlowStepId = toStepId,
                VersionId = flowSteps.VersionNumber
            };


            return await _userClient.Post<ConsumerFlowProgressResponseDto>(
                $"{CommonConstants.UpdateOnboardingFlowStatusAPIUrl}", updateRequest);
        }

        #endregion

    }
}
