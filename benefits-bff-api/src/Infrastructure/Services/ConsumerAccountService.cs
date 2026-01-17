using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Sunny.Benefits.Bff.Core.Domain.Constants;
using Sunny.Benefits.Bff.Infrastructure.Helpers.Interface;
using Sunny.Benefits.Bff.Infrastructure.HttpClients.Interfaces;
using Sunny.Benefits.Bff.Infrastructure.Services.Interfaces;
using SunnyBenefits.Fis.Core.Domain.Dtos;
using SunnyBenefits.Fis.Core.Domain.Enums;

namespace Sunny.Benefits.Bff.Infrastructure.Services
{
    public class ConsumerAccountService : IConsumerAccountService
    {
        private readonly ILogger<ConsumerAccountService> _logger;
        private readonly IFisClient _fisClient;
        private readonly IEventService _eventService;
        private readonly INotificationHelper _notificationHelper;
        private const string className = nameof(ConsumerAccountService);

        public ConsumerAccountService(ILogger<ConsumerAccountService> logger, IFisClient fisClient, IEventService eventService, INotificationHelper notificationHelper)
        {
            _logger = logger;
            _fisClient = fisClient;
            _eventService = eventService;
            _notificationHelper = notificationHelper;
        }

        public async Task<ConsumerAccountUpdateResponseDto> UpdateConsumerAccountConfig(ConsumerAccountUpdateRequestDto consumerAccountUpdateRequest)
        {
            const string methodName = nameof(UpdateConsumerAccountConfig);
            try
            {
                _logger.LogInformation("{ClassName}.{MethodName}: Fetching ConsumerAccount Details for TenantCode:{Tenant}, Consumer:{Consumer}", className, methodName, consumerAccountUpdateRequest.TenantCode, consumerAccountUpdateRequest.ConsumerCode);
                var response = await _fisClient.Patch<ConsumerAccountUpdateResponseDto>("consumer-account", consumerAccountUpdateRequest);
                if (response.ErrorCode != null)
                {
                    _logger.LogError("{ClassName}.{MethodName} - Consumer Account Details Not Found for ConsumerCode : {Consumer}", className, methodName, consumerAccountUpdateRequest.ConsumerCode);
                    return new ConsumerAccountUpdateResponseDto { ErrorCode = response.ErrorCode, ErrorMessage = response.ErrorMessage };
                }
                _logger.LogInformation("{ClassName}.{MethodName}: Successfully Updated Consumer Account Config for ConsumerCode:{Consumer}", className, methodName, consumerAccountUpdateRequest.ConsumerCode);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName} - Error occured while Updating ConsumerAccount Details, ConsumerCode : {Consumer} - ErrorCode:{ErrorCode}, ERROR:{ErrorMessage}", className, methodName, consumerAccountUpdateRequest.ConsumerCode, StatusCodes.Status500InternalServerError, ex.Message);
                throw;
            }
        }

        public async Task<ConsumerAccountResponseDto> UpdateConsumerAccountCardIssue(UpdateCardIssueRequestDto updateCardIssueRequestDto)
        {
            const string methodName = nameof(UpdateConsumerAccountCardIssue);
            try
            {
                _logger.LogInformation("{ClassName}.{MethodName}: Updating consumer account Card Issue Status for ConsumerCode: {ConsumerCode}, TenantCode: {TenantCode}", className, methodName, updateCardIssueRequestDto.ConsumerCode, updateCardIssueRequestDto.TenantCode);
                var response = await _fisClient.Put<ConsumerAccountResponseDto>("update-card-issue-status", updateCardIssueRequestDto);
                if (response.ErrorCode != null)
                {
                    _logger.LogError("{ClassName}.{MethodName} - Error occurred while updating consumer account Card Issue Status for ConsumerCode: {ConsumerCode}, TenantCode: {TenantCode}", className, methodName, updateCardIssueRequestDto.ConsumerCode, updateCardIssueRequestDto.TenantCode);
                    return new ConsumerAccountResponseDto { ErrorCode = response.ErrorCode, ErrorMessage = response.ErrorMessage };
                }

                // Triggering notification event if card is ordered
                if (Enum.TryParse<CardIssueStatus>(updateCardIssueRequestDto.TargetCardIssueStatus, out var status) && status == CardIssueStatus.ELIGIBLE_FOR_FIS_BATCH_PROCESS)
                {
                    _logger.LogInformation("{ClassName}.{MethodName}: Triggering Card Ordered Notification Event for ConsumerCode: {ConsumerCode}, TenantCode: {TenantCode}", className, methodName, updateCardIssueRequestDto.ConsumerCode, updateCardIssueRequestDto.TenantCode);
                    await _notificationHelper.ProcessNotification(updateCardIssueRequestDto.ConsumerCode, updateCardIssueRequestDto.TenantCode, CardOperationConstants.CardOrderedNotificationEventName);
                }

                _logger.LogInformation("{ClassName}.{MethodName}: Successfully updated Card Issue Status for ConsumerCode: {ConsumerCode}, TenantCode: {TenantCode}", className, methodName, updateCardIssueRequestDto.ConsumerCode, updateCardIssueRequestDto.TenantCode);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName} - Error occured while Updating Card Issue Status for ConsumerCode: {ConsumerCode}, TenantCode: {TenantCode} - ErrorCode:{ErrorCode}, ERROR:{ErrorMessage}", className, methodName, updateCardIssueRequestDto.ConsumerCode, updateCardIssueRequestDto.TenantCode, StatusCodes.Status500InternalServerError, ex.Message);
                throw;
            }
        }
        public async Task<GetConsumerAccountResponseDto> GetConsumerAccount(GetConsumerAccountRequestDto getConsumerAccountRequestDto)
        {
            const string methodName = nameof(UpdateConsumerAccountCardIssue);
            try
            {
                _logger.LogInformation("{ClassName}.{MethodName}: Getting FIS consumer account for ConsumerCode: {ConsumerCode}, TenantCode: {TenantCode}", className, methodName, getConsumerAccountRequestDto.ConsumerCode, getConsumerAccountRequestDto.TenantCode);
                var response = await _fisClient.Post<GetConsumerAccountResponseDto>("get-consumer-account", getConsumerAccountRequestDto);
                if (response.ErrorCode != null)
                {
                    _logger.LogError("{ClassName}.{MethodName} - Error occurred while getting FIS consumer account for ConsumerCode: {ConsumerCode}, TenantCode: {TenantCode}", className, methodName, getConsumerAccountRequestDto.ConsumerCode, getConsumerAccountRequestDto.TenantCode);
                    return new GetConsumerAccountResponseDto { ErrorCode = response.ErrorCode, ErrorMessage = response.ErrorMessage };
                }
                _logger.LogInformation("{ClassName}.{MethodName}: Successfully retrieved FIS consumer account for ConsumerCode: {ConsumerCode}, TenantCode: {TenantCode}", className, methodName, getConsumerAccountRequestDto.ConsumerCode, getConsumerAccountRequestDto.TenantCode);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName} - Error occured while getting FIS consumer account for ConsumerCode: {ConsumerCode}, TenantCode: {TenantCode} - ErrorCode:{ErrorCode}, ERROR:{ErrorMessage}", className, methodName, getConsumerAccountRequestDto.ConsumerCode, getConsumerAccountRequestDto.TenantCode, StatusCodes.Status500InternalServerError, ex.Message);
                throw;
            }
        }
    }
}
