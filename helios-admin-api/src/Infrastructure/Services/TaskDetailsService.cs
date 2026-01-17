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
    public class TaskDetailsService : ITaskDetailsService
    {
        public readonly ILogger<TaskDetailsService> _logger;
        private readonly IMapper _mapper;
        public readonly ITaskClient _taskClient;
        public const string className = nameof(TaskService);

        public TaskDetailsService(ILogger<TaskDetailsService> logger, IMapper mapper, ITaskClient taskClient)
        {
            _logger = logger;
            _mapper = mapper;
            _taskClient = taskClient;
        }

        public async Task<BaseResponseDto> CreateTaskDetails(CreateTaskDetailsRequestDto createTaskDetailsRequestDto)
        {
            const string methodName = nameof(CreateTaskDetails);
            try
            {
                _logger.LogInformation("{ClassName}.{MethodName}: Create Task Details process started for TenantCode: {TenantCode}", className, methodName, createTaskDetailsRequestDto.TaskDetail.TenantCode);

                var taskDetailResponse = await _taskClient.Post<BaseResponseDto>(Constant.CreateTaskDetailsUrl, createTaskDetailsRequestDto);
                if (taskDetailResponse.ErrorCode != null)
                {
                    _logger.LogWarning("{ClassName}.{MethodName}: Error occurred while creating Task Details, TenantCode: {TenantCode}, ErrorCode: {ErrorCode}", className, methodName, createTaskDetailsRequestDto.TaskDetail.TenantCode, taskDetailResponse.ErrorCode);
                    return taskDetailResponse;
                }

                _logger.LogInformation("{ClassName}.{MethodName}: Task Details created successfully, TenantCode: {TenantCode}", className, methodName, createTaskDetailsRequestDto.TaskDetail.TenantCode);
                return new BaseResponseDto();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName}: Exception occurred while creating Task Details. ErrorMessage: {ErrorMessage}, StackTrace: {StackTrace}", className, methodName, ex.Message, ex.StackTrace);
                throw;
            }

        }

        /// <summary>
        /// UpdateTaskDetailAsync
        /// </summary>
        /// <param name="taskDetailId"></param>
        /// <param name="taskDetailRequestDto"></param>
        /// <returns></returns>
        public async Task<TaskDetailResponseDto> UpdateTaskDetailAsync(long taskDetailId, TaskDetailRequestDto taskDetailRequestDto)
        {
            const string methodName = nameof(UpdateTaskDetailAsync);
            try
            {
                _logger.LogInformation("{ClassName}:{MethodName}: Started processing...TaskDetailId:{TaskDetailId}", className, methodName, taskDetailId);

                var taskDetailResponse = await _taskClient.Put<TaskDetailResponseDto>($"{Constant.TaskDetailApiUrl}/{taskDetailId}", taskDetailRequestDto);

                if (taskDetailResponse.ErrorCode != null)
                {
                    _logger.LogError("{ClassName}.{MethodName}: Error processing for TaskDetailId: {TaskDetailId}, Error Code: {ErrorCode}, Error Message: {ErroMessage}", className, methodName, taskDetailId, taskDetailResponse.ErrorCode, taskDetailResponse.ErrorMessage);
                    return taskDetailResponse;
                }

                _logger.LogInformation("{ClassName}:{MethodName}: Ended processing...TaskDetailId:{TaskDetailId}", className, methodName, taskDetailId);

                return taskDetailResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}:{MethodName}: Error processing for TaskDetailId: {TaskDetailId}", className, methodName, taskDetailId);
                return new TaskDetailResponseDto() { TaskDetail = _mapper.Map<TaskDetailDto>(taskDetailRequestDto), ErrorCode = StatusCodes.Status409Conflict, ErrorMessage = ex.Message };
            }
        }
    }
}
