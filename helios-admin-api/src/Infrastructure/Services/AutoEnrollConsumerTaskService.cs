using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SunnyRewards.Helios.Admin.Core.Domain.Constants;
using SunnyRewards.Helios.Admin.Core.Domain.Dtos;
using SunnyRewards.Helios.Admin.Infrastructure.HttpClients.Interfaces;
using SunnyRewards.Helios.Admin.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.Task.Core.Domain.Dtos;
using SunnyRewards.Helios.User.Core.Domain.Dtos;
using System.Net;

namespace SunnyRewards.Helios.Admin.Infrastructure.Services
{
    public class AutoEnrollConsumerTaskService : IAutoEnrollConsumerTaskService
    {
        private readonly ILogger<AutoEnrollConsumerTaskService> _logger;
        private readonly ITaskClient _taskClient;
        private readonly IUserClient _userClient;

        private const string ClassName = nameof(AutoEnrollConsumerTaskService);

        public AutoEnrollConsumerTaskService(ILogger<AutoEnrollConsumerTaskService> logger, ITaskClient taskClient, IUserClient userClient)
        {
            _logger = logger;
            _taskClient = taskClient;
            _userClient = userClient;
        }

        /// <summary>
        /// Enrolls the consumer task.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns></returns>
        public BaseResponseDto EnrollConsumerTask(AutoEnrollConsumerTaskRequestDto request)
        {
            const string MethodName = nameof(EnrollConsumerTask);

            // Input validation
            if (string.IsNullOrWhiteSpace(request.TenantCode) || string.IsNullOrWhiteSpace(request.ConsumerCode) || string.IsNullOrWhiteSpace(request.TaskExternalCode))
            {
                var errorMessage = "One or more required parameters are missing.";
                var errorCode = StatusCodes.Status400BadRequest;
                _logger.LogError("{ClassName}.{MethodName}: {ErrorMessage} ErrorCode: {ErrorCode}, TenantCode: {TenantCode}, ConsumerCode: {ConsumerCode}, TaskExternalCode: {TaskExternalCode}",
                    ClassName, MethodName, errorMessage, errorCode, request.TenantCode, request.ConsumerCode, request.TaskExternalCode);

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
                    ConsumerCode = request.ConsumerCode
                }).GetAwaiter().GetResult();

                // Handle consumer not found
                if (consumerResp == null || consumerResp.Consumer == null || consumerResp.Consumer.ConsumerCode == null)
                {
                    var errorMessage = "Consumer not found or invalid consumer code.";
                    var errorCode = StatusCodes.Status404NotFound;
                    _logger.LogError("{ClassName}.{MethodName}: {ErrorMessage} ErrorCode: {ErrorCode}, ConsumerCode: {ConsumerCode}",
                        ClassName, MethodName, errorMessage, errorCode, request.ConsumerCode);

                    return new BaseResponseDto
                    {
                        ErrorCode = errorCode,
                        ErrorMessage = errorMessage
                    };
                }

                // Handle mismatched tenant code
                if (consumerResp.Consumer.TenantCode != request.TenantCode)
                {
                    var errorMessage = "Tenant code does not match the consumer's tenant.";
                    var errorCode = StatusCodes.Status404NotFound;
                    _logger.LogError("{ClassName}.{MethodName}: {ErrorMessage} ErrorCode: {ErrorCode}, TenantCode: {TenantCode}, ConsumerCode: {ConsumerCode}",
                        ClassName, MethodName, errorMessage, errorCode, request.TenantCode, request.ConsumerCode);

                    return new BaseResponseDto
                    {
                        ErrorCode = errorCode,
                        ErrorMessage = errorMessage
                    };
                }

                // Fetch task reward details
                var getTaskRewardDetailsAPIUrl = $"{Constant.TaskRewardDetailsAPIUrl}?tenantCode={request.TenantCode}&taskExternalCode={request.TaskExternalCode}";
                var taskRewardDetailsResponse = _taskClient.Get<TaskRewardDetailsResponseDto>(getTaskRewardDetailsAPIUrl, null).GetAwaiter().GetResult(); ;

                // Handle task fetch failure
                if (taskRewardDetailsResponse?.ErrorCode != null)
                {
                    var errorMessage = $"Error occurred while fetching task reward details. ErrorCode: {taskRewardDetailsResponse.ErrorCode}, TaskExternalCode: {request.TaskExternalCode}";
                    var errorCode = taskRewardDetailsResponse.ErrorCode ?? StatusCodes.Status500InternalServerError;
                    _logger.LogError("{ClassName}.{MethodName}: {ErrorMessage} ErrorCode: {ErrorCode}, TenantCode: {TenantCode}, TaskExternalCode: {TaskExternalCode}",
                        ClassName, MethodName, errorMessage, errorCode, request.TenantCode, request.TaskExternalCode);

                    return new BaseResponseDto
                    {
                        ErrorCode = errorCode,
                        ErrorMessage = taskRewardDetailsResponse.ErrorMessage
                    };
                }

                // Create consumer task
                var consumerTaskDto = new CreateConsumerTaskDto
                {
                    TenantCode = request.TenantCode,
                    ConsumerCode = request.ConsumerCode,
                    TaskId = taskRewardDetailsResponse.TaskRewardDetails[0].Task.TaskId,
                    TaskStatus = Constant.TaskStatus.InProgress,
                    AutoEnrolled = true
                };

                var taskResponse = _taskClient.Post<ConsumerTaskResponseUpdateDto>(Constant.ConsumerTaskAPIUrl, consumerTaskDto).GetAwaiter().GetResult(); 

                // Handle task creation failure
                if (taskResponse?.ErrorCode != null)
                {
                    var errorMessage = $"Error occurred while creating consumer task. ErrorCode: {taskResponse.ErrorCode}, ConsumerCode: {request.ConsumerCode}, TaskId: {consumerTaskDto.TaskId}, TaskStatus: {consumerTaskDto.TaskStatus}";
                    var errorCode = taskResponse.ErrorCode ?? StatusCodes.Status500InternalServerError;
                    _logger.LogError("{ClassName}.{MethodName}: {ErrorMessage} ErrorCode: {ErrorCode}, ConsumerCode: {ConsumerCode}, TaskId: {TaskId}, TaskStatus: {TaskStatus}",
                        ClassName, MethodName, errorMessage, errorCode, request.ConsumerCode, consumerTaskDto.TaskId, consumerTaskDto.TaskStatus);

                    return new BaseResponseDto
                    {
                        ErrorCode = errorCode,
                        ErrorMessage = taskResponse.ErrorMessage
                    };
                }

                _logger.LogInformation("{ClassName}.{MethodName}: Consumer task enrollment completed successfully for ConsumerCode: {ConsumerCode}, TenantCode: {TenantCode}, TaskId: {TaskId}",
                    ClassName, MethodName, request.ConsumerCode, request.TenantCode, consumerTaskDto.TaskId);

                return new BaseResponseDto();
            }
            catch (Exception ex)
            {
                var errorCode = StatusCodes.Status500InternalServerError;
                _logger.LogError(ex, "{ClassName}.{MethodName}: Exception occurred during auto-enrollment. ConsumerCode: {ConsumerCode}, TenantCode: {TenantCode}. ErrorMessage: {ErrorMessage}, StackTrace: {StackTrace}, ErrorCode: {ErrorCode}",
                    ClassName, MethodName, request.ConsumerCode, request.TenantCode, ex.Message, ex.StackTrace, errorCode);

                return new BaseResponseDto
                {
                    ErrorCode = errorCode,
                    ErrorMessage = $"An unexpected error occurred during auto-enrollment. ErrorMessage: {ex.Message}"
                };
            }
        }
    }
}
