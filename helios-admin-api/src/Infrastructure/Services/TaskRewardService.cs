using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SunnyRewards.Helios.Admin.Core.Domain.Constants;
using SunnyRewards.Helios.Admin.Infrastructure.HttpClients.Interfaces;
using SunnyRewards.Helios.Admin.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.Task.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Admin.Infrastructure.Services
{
    public class TaskRewardService : ITaskRewardService
    {
        public readonly ILogger<TaskRewardService> _logger;
        private readonly IMapper _mapper;
        public readonly ITaskClient _taskClient;
        public const string className = nameof(TaskRewardService);

        public TaskRewardService(ILogger<TaskRewardService> logger, IMapper mapper, ITaskClient taskClient)
        {
            _logger = logger;
            _mapper = mapper;
            _taskClient = taskClient;
        }

        public async Task<BaseResponseDto> CreateTaskReward(CreateTaskRewardRequestDto createTaskRewardRequestDto)
        {
            const string methodName = nameof(CreateTaskReward);
            try
            {
                _logger.LogInformation("{ClassName}.{MethodName}: Create Task Reward process started for TenantCode: {TenantCode}", className, methodName, createTaskRewardRequestDto.TaskReward.TenantCode);

                var taskRewardResponse = await _taskClient.Post<BaseResponseDto>(Constant.CreateTaskRewardUrl, createTaskRewardRequestDto);
                if (taskRewardResponse.ErrorCode != null)
                {
                    _logger.LogWarning("{ClassName}.{MethodName}: Error occurred while creating Task Reward, TenantCode: {TenantCode}, ErrorCode: {ErrorCode}", className, methodName, createTaskRewardRequestDto.TaskReward.TenantCode, taskRewardResponse.ErrorCode);
                    return taskRewardResponse;
                }

                _logger.LogInformation("{ClassName}.{MethodName}: Task Rewards created successfully, TenantCode: {TenantCode}", className, methodName, createTaskRewardRequestDto.TaskReward.TenantCode);
                return new BaseResponseDto();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName}: Exception occurred while creating Task Rewards. ErrorMessage: {ErrorMessage}, StackTrace: {StackTrace}", className, methodName, ex.Message, ex.StackTrace);
                throw;
            }
        }

        /// <summary>
        /// UpdateTaskRewardAsync
        /// </summary>
        /// <param name="taskRewardId"></param>
        /// <param name="updateTaskRewardRequestDto"></param>
        /// <returns></returns>
        public async Task<TaskRewardResponseDto> UpdateTaskRewardAsync(long taskRewardId, TaskRewardRequestDto taskRewardRequestDto)
        {
            const string methodName = nameof(UpdateTaskRewardAsync);
            try
            {
                _logger.LogInformation("{ClassName}:{MethodName}: Started processing for TaskRewardId:{TaskRewardId}", className, methodName, taskRewardId);

                var taskRewardResponse = await _taskClient.Put<TaskRewardResponseDto>($"{Constant.TaskRewardApiUrl}/{taskRewardId}", taskRewardRequestDto);

                if (taskRewardResponse.ErrorCode != null)
                {
                    _logger.LogWarning("{ClassName}.{MethodName}: Error occurred while updating task reward, taskRewardId: {TaskRewardId}, ErrorCode: {ErrorCode}", className, methodName, taskRewardId, taskRewardResponse.ErrorCode);
                    return taskRewardResponse;
                }

                return taskRewardResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}:{MethodName}: Error processing task reward for TaskRewardId: {TaskRewardId}", className, methodName, taskRewardId);
                return new TaskRewardResponseDto() { TaskReward = _mapper.Map<TaskRewardDto>(taskRewardRequestDto), ErrorCode = StatusCodes.Status500InternalServerError, ErrorMessage = ex.Message };
            }
        }

        /// <summary>
        /// Retrieves tasks and taskrewards with tenant code
        /// </summary>
        /// <param name="getTaskRewardsRequestDto">The gettaskrewards request dto </param>
        /// <returns>Returns the list of tasks and taskrewards </returns>
        public async Task<GetTasksAndTaskRewardsResponseDto> GetTasksAndTaskRewards(GetTasksAndTaskRewardsRequestDto getTaskRewardsRequestDto)
        {
            const string methodName = nameof(GetTasksAndTaskRewards);
            try
            {
                _logger.LogInformation("{ClassName}.{MethodName}: Get Task and TaskReward process started for TenantCode: {TenantCode}", className, methodName, getTaskRewardsRequestDto.TenantCode);

                var taskRewardResponse = await _taskClient.Post<GetTasksAndTaskRewardsResponseDto>(Constant.GetTasksAndTaskRewardsAPIUrl, getTaskRewardsRequestDto);
                if (taskRewardResponse.ErrorCode != null)
                {
                    _logger.LogWarning("{ClassName}.{MethodName}: Error occurred while retrieving Task and Task Reward, TenantCode: {TenantCode}, ErrorCode: {ErrorCode}", className, methodName, getTaskRewardsRequestDto.TenantCode, taskRewardResponse.ErrorCode);
                    return taskRewardResponse;
                }

                _logger.LogInformation("{ClassName}.{MethodName}: Task and Task Rewards retrieved  successfully, TenantCode: {TenantCode}", className, methodName, getTaskRewardsRequestDto.TenantCode);
                return taskRewardResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName}: Exception occurred while retrieving Task and Task Rewards. ErrorMessage: {ErrorMessage}, StackTrace: {StackTrace}", className, methodName, ex.Message, ex.StackTrace);
                throw;
            }
        }

        /// <summary>
        /// Retrieves the list of task reward details for a given tenant code.
        /// </summary>
        /// <param name="tenantCode"></param>
        /// <returns></returns>
        public async Task<TaskRewardDetailsResponseDto> GetTaskRewardDetails(string tenantCode, string? languageCode)
        {
            const string methodName = nameof(GetTaskRewardDetails);

            try
            {
                _logger.LogInformation("{ClassName}.{MethodName}: Started processing task rewards with TenantCode: {TenantCode}", className, methodName, tenantCode);
                var url = $"{Constant.TaskRewardDetailsApiUrl}?tenantCode={tenantCode}&languageCode={languageCode}";
                return await _taskClient.Get<TaskRewardDetailsResponseDto>(url, new Dictionary<string, long>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName}: Error occurred while processing task rewards for TenantCode: {TenantCode}. Error Code: {ErrorCode}, Message: {Msg}",
                    className, methodName, tenantCode, StatusCodes.Status500InternalServerError, ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Retrieves the list of task health rewards for a given tenant code.
        /// </summary>
        /// <param name="tenantCode"></param>
        /// <returns></returns>
        public async Task<TaskRewardsResponseDto> GetHealthTaskRewards(string tenantCode)
        {
            const string methodName = nameof(GetHealthTaskRewards);

            try
            {
                _logger.LogInformation("{ClassName}.{MethodName}: Started processing task rewards with TenantCode: {TenantCode}", className, methodName, tenantCode);

                var url = $"{Constant.HealthTaskRewardsApiUrl}/{tenantCode}";
                return await _taskClient.Get<TaskRewardsResponseDto>(url, new Dictionary<string, long>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName}: Error occurred while processing task rewards for TenantCode: {TenantCode}. Error Code: {ErrorCode}, Message: {Msg}",
                    className, methodName, tenantCode, StatusCodes.Status500InternalServerError, ex.Message);
                throw;
            }
        }
    }
}
