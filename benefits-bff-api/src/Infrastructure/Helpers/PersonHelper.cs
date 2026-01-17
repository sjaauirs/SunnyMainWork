using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Sunny.Benefits.Bff.Core.Constants;
using Sunny.Benefits.Bff.Core.Domain.Constants;
using Sunny.Benefits.Bff.Core.Domain.Dtos.Json;
using Sunny.Benefits.Bff.Infrastructure.Helpers.Interface;
using Sunny.Benefits.Bff.Infrastructure.HttpClients.Interfaces;
using Sunny.Benefits.Bff.Infrastructure.Repositories.Interfaces;
using SunnyRewards.Helios.Admin.Core.Domain.Dtos;
using SunnyRewards.Helios.Task.Core.Domain.Dtos;
using SunnyRewards.Helios.Tenant.Core.Domain.Dtos;
using SunnyRewards.Helios.User.Core.Domain.Dtos;
using SunnyRewards.Helios.User.Core.Domain.enums;

namespace Sunny.Benefits.Bff.Infrastructure.Helpers
{
    public class PersonHelper : IPersonHelper
    {
        private readonly ILogger<PersonHelper> _personHelperLogger;
        private readonly IUserClient _userClient;
        private readonly ITenantClient _tenantClient;
        private readonly IAdminClient _adminClient;
        private readonly ITaskClient _taskClient;
        private const string className = nameof(PersonHelper);
        public PersonHelper(ILogger<PersonHelper> PersonHelperLogger, IUserClient userClient, ITenantClient tenantClient, IAdminClient adminClient, ITaskClient taskClient)

        {
            _personHelperLogger = PersonHelperLogger;
            _userClient = userClient;
            _tenantClient = tenantClient;
            _adminClient = adminClient;
            _taskClient = taskClient;
        }
        public async Task<bool> UpdateOnBoardingState(UpdateOnboardingStateDto updateOnboardingStateDto)
        {

            _personHelperLogger.LogInformation("Updating Onboarding status to {State} for  ConsumerCode :{ConsumerCode}",
                                            updateOnboardingStateDto.OnboardingState.ToString(), updateOnboardingStateDto.ConsumerCode);
            var consumerResponse = await _userClient.Patch<ConsumerResponseDto>("consumer", updateOnboardingStateDto);
            if (consumerResponse.Consumer != null && consumerResponse.Consumer.PersonId > 0 && consumerResponse.Consumer.OnBoardingState == updateOnboardingStateDto.OnboardingState.ToString())
            {
                _personHelperLogger.LogInformation("Updated Onboarding status  to  OnBoardingState : {OnBoardingState} for  consumerCode : {consumerCode}", consumerResponse.Consumer.OnBoardingState, consumerResponse.Consumer.ConsumerCode);
                return consumerResponse.Consumer.PersonId > 0;
            }
            return false;
        }

        public async Task<GetPersonAndConsumerResponseDto?> GetPersonDetails(GetConsumerRequestDto getConsumerRequestDto)
        {
            _personHelperLogger.LogInformation("Get Person Details for consumer: {ConsumerCode}", getConsumerRequestDto.ConsumerCode);
            return await _userClient.Post<GetPersonAndConsumerResponseDto>(CardOperationConstants.GetPersonAndConsumerDetails, getConsumerRequestDto);

        }
        public async Task<bool> ValidatePersonIsVerified(GetConsumerRequestDto consumerCode)
        {
            _personHelperLogger.LogInformation("Validating Onboarding status for ConsumerCode: {ConsumerCode}", consumerCode.ConsumerCode);

            var consumer = await _userClient.Post<GetPersonAndConsumerResponseDto>(CardOperationConstants.GetPersonAndConsumerDetails, consumerCode);
            var consumerState = consumer?.Consumer?.OnBoardingState;

            if (consumer == null || consumer.Consumer == null)
                return false;

            var tenantCode = consumer.Consumer.TenantCode ?? string.Empty;
            var tenantData = await GetTenantByTenantCode(tenantCode);
            var tenantOption = tenantData?.TenantOption != null
                ? JsonConvert.DeserializeObject<TenantOption>(tenantData.TenantOption)
                : null;

            return IsPersonVerified(consumerState, tenantOption);
        }

        private bool IsPersonVerified(string? consumerState, TenantOption? tenantOption)
        {
            if (tenantOption?.BenefitsOptions?.DisableOnboardingFlow == true)
            {
                return consumerState == OnboardingState.EMAIL_VERIFIED.ToString()
                    || consumerState == OnboardingState.VERIFIED.ToString();
            }

            return consumerState == OnboardingState.VERIFIED.ToString();
        }

        private ConsumerRequestDto CreateConsumerRequest(ConsumerDto dto)
        {
            return new ConsumerRequestDto
            {
                PersonId = dto.PersonId,
                TenantCode = dto.TenantCode,
                ConsumerCode = dto.ConsumerCode,
                RegistrationTs = dto.RegistrationTs,
                EligibleStartTs = dto.EligibleStartTs,
                EligibleEndTs = dto.EligibleEndTs,
                Registered = dto.Registered,
                Eligible = dto.Eligible,
                MemberNbr = dto.MemberNbr,
                SubscriberMemberNbr = dto.SubscriberMemberNbr,
                ConsumerAttribute = dto.ConsumerAttribute,
                AnonymousCode = dto.AnonymousCode,
                SubscriberOnly = dto.SubscriberOnly,
                IsSsoAuthenticated = dto.IsSsoAuthenticated,
                EnrollmentStatus = dto.EnrollmentStatus,
                EnrollmentStatusSource = dto.EnrollmentStatusSource,
                MemberId= dto.MemberId,
                
            };
        }

        public async Task<TenantDto> GetTenantByTenantCode(string tenantCode)
        {
            const string methodName = nameof(GetTenantByTenantCode);
            var getTenantCodeRequestDto = new GetTenantCodeRequestDto()
            {
                TenantCode = tenantCode,
            };
            var tenantResponse = await _tenantClient.Post<TenantDto>(CommonConstants.GetTenantByCodeAPIUrl, getTenantCodeRequestDto);
            if (tenantResponse == null || tenantResponse.TenantCode == null)
            {
                _personHelperLogger.LogError("{ClassName}.{MethodName} - TenantDetails Not Found for TenantCode : {TenantCode}", className, methodName, getTenantCodeRequestDto.TenantCode);
                return new TenantDto();
            }
            _personHelperLogger.LogInformation("Retrieved Tenant Successfully for TenantCode : {TenantCode}", getTenantCodeRequestDto.TenantCode);

            return tenantResponse;
        }

        public async Task<ConsumerResponseDto> UpdateConsumer(long consumerId, ConsumerDto consumerDto, string auth0UserName)
        {
            const string methodName = nameof(UpdateConsumer);
            ConsumerRequestDto consumerRequestDto = CreateConsumerRequest(consumerDto);
            consumerRequestDto.Auth0UserName = auth0UserName;
            try
            {
                _personHelperLogger.LogInformation("{ClassName}.{MethodName} - Updating consumer with ConsumerId: {ConsumerId}, TenantCode: {TenantCode}",
                    className, methodName, consumerId, consumerRequestDto.TenantCode);

                var consumerResponse = await _userClient.Put<ConsumerResponseDto>(
                    $"{UserConstants.ConsumerAPIUrl}/{consumerId}", consumerRequestDto);

                if (consumerResponse.ErrorCode != null)
                {
                    _personHelperLogger.LogError("{ClassName}.{MethodName} - Failed to update consumer. ConsumerId: {ConsumerId}, TenantCode: {TenantCode}, ErrorCode: {ErrorCode}, ErrorMessage: {ErrorMessage}",
                        className, methodName, consumerId, consumerRequestDto.TenantCode, consumerResponse.ErrorCode, consumerResponse.ErrorMessage);
                    return new ConsumerResponseDto();
                }

                _personHelperLogger.LogInformation("{ClassName}.{MethodName} - Successfully updated consumer with ConsumerId: {ConsumerId}, TenantCode: {TenantCode}",
                    className, methodName, consumerId, consumerRequestDto.TenantCode);

                return consumerResponse;
            }
            catch (Exception ex)
            {
                _personHelperLogger.LogError(ex, "{ClassName}.{MethodName} - Exception occurred while updating consumer. ConsumerId: {ConsumerId}, TenantCode: {TenantCode}, Error: {Error}",
                    className, methodName, consumerId, consumerRequestDto.TenantCode, ex.Message);
                throw;
            }
        }

        public async Task<bool> UpdateOnBoardingTask(ConsumerDto consumerDto)
        {
            try 
            { 
            var tenantCode = consumerDto.TenantCode!;
            _personHelperLogger.LogInformation("Starting onboarding task update for TenantCode: {TenantCode}, ConsumerCode: {ConsumerCode}", tenantCode, consumerDto.ConsumerCode);

            var tenant = await GetTenantByTenantCode(tenantCode);
            if (tenant == null)
            {
                _personHelperLogger.LogWarning("Tenant not found for TenantCode: {TenantCode}", tenantCode);
                return false;
            }

            TenantOption? tenantOption = null;
            try
            {
                tenantOption = !string.IsNullOrEmpty(tenant.TenantOption)
                    ? JsonConvert.DeserializeObject<TenantOption>(tenant.TenantOption)
                    : new TenantOption();
            }
            catch (Exception ex)
            {
                _personHelperLogger.LogError(ex, "Failed to deserialize TenantOption for TenantCode: {TenantCode}", tenantCode);
                return false;
            }

            if (tenantOption?.BenefitsOptions == null)
            {
                _personHelperLogger.LogWarning("BenefitsOptions not configured for TenantCode: {TenantCode}", tenantCode);
                return false;
            }

            if (!tenantOption.BenefitsOptions.AutoCompleteTaskOnLogin)
            {
                _personHelperLogger.LogInformation("AutoCompleteTaskOnLogin is disabled for TenantCode: {TenantCode}", tenantCode);
                return false;
            }

            var taskRewardCode = tenantOption.BenefitsOptions.TaskCompletionCheckCode?.FirstOrDefault();
            if (string.IsNullOrWhiteSpace(taskRewardCode))
            {
                _personHelperLogger.LogWarning("TaskCompletionCheckCode is missing or empty for TenantCode: {TenantCode}", tenantCode);
                return false;
            }

            var getTaskRequest = new GetTaskRewardRequestDto
            {
                TaskRewardCodes = new List<string> { taskRewardCode }
            };

            var getTaskResponse = await _taskClient.Post<GetTaskRewardResponseDto>("task/get-by-task-reward-code", getTaskRequest);

            if (getTaskResponse?.TaskRewardDetails == null || getTaskResponse.TaskRewardDetails.Count == 0)
            {
                _personHelperLogger.LogWarning("No task found for TaskRewardCode: {TaskRewardCode}, TenantCode: {TenantCode}", taskRewardCode, tenantCode);
                return false;
            }

            var taskId = getTaskResponse.TaskRewardDetails[0].Task.TaskId;
            var taskUpdateRequest = new TaskUpdateRequestDto
            {
                ConsumerCode = consumerDto.ConsumerCode,
                PartnerCode = tenant.PartnerCode,
                TaskStatus = CommonConstants.Completed,
                IsAutoEnrollEnabled = true,
                TaskId = taskId
            };


                var updateResponse = await _adminClient.PostFormData<ConsumerTaskUpdateResponseDto>("admin/consumer/task-update", taskUpdateRequest);
                if (updateResponse.ErrorCode != null)
                {
                    _personHelperLogger.LogError("Failed to update task. ErrorCode: {ErrorCode}, ConsumerCode: {ConsumerCode}, TenantCode: {TenantCode}", updateResponse.ErrorCode, consumerDto.ConsumerCode, tenantCode);
                    return false;
                }

                _personHelperLogger.LogInformation("Successfully completed onboarding task for ConsumerCode: {ConsumerCode}, TenantCode: {TenantCode} and ConsumerTaskId : {ConsumerTaskId}", consumerDto.ConsumerCode, tenantCode, updateResponse!.ConsumerTask!.ConsumerTaskId);
                return true;
            }
            catch(Exception ex)
            {
                _personHelperLogger.LogError(ex, "Error occured in completed onboarding task for ConsumerCode: {ConsumerCode}, TenantCode: {TenantCode}",consumerDto.ConsumerCode, consumerDto.TenantCode);
                return false;
            }
        }
    }

}
