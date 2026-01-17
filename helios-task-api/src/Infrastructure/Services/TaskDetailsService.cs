using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.Task.Core.Domain.Constants;
using SunnyRewards.Helios.Task.Core.Domain.Dtos;
using SunnyRewards.Helios.Task.Core.Domain.Models;
using SunnyRewards.Helios.Task.Infrastructure.Repositories.Interfaces;
using SunnyRewards.Helios.Task.Infrastructure.Services.Interface;

namespace SunnyRewards.Helios.Task.Infrastructure.Services
{
    public class TaskDetailsService : ITaskDetailsService
    {
        private readonly ITaskDetailRepo _taskDetailRepo;
        private readonly ILogger<TaskDetailsService> _taskDetailsLogger;
        private readonly IMapper _mapper;
        private readonly ITaskRepo _taskRepo;
        public const string className = nameof(TaskDetailsService);

        public TaskDetailsService(ITaskDetailRepo taskDetailRepo, ILogger<TaskDetailsService> taskDetailsLogger, IMapper mapper, ITaskRepo taskRepo)
        {
            _taskDetailRepo = taskDetailRepo;
            _taskDetailsLogger = taskDetailsLogger;
            _mapper = mapper;
            _taskRepo = taskRepo;
        }
        /// <summary>
        /// Creates Task Details
        /// </summary>
        /// <param name="createTaskDetailsRequestDto">request which is contain Task Details to create</param>
        /// <returns></returns>
        public async Task<BaseResponseDto> CreateTaskDetails(CreateTaskDetailsRequestDto createTaskDetailsRequestDto)
        {
            const string methodName = nameof(CreateTaskDetails);
            try
            {
                _taskDetailsLogger.LogInformation("{ClassName}:{MethodName}: Fetching task Details started for TaskCode:{TaskCode}, Tenant Code: {TenantCode}",
                 className, methodName, createTaskDetailsRequestDto.TaskCode, createTaskDetailsRequestDto.TaskDetail.TenantCode);
                var task = await _taskRepo.FindOneAsync(x => x.TaskCode == createTaskDetailsRequestDto.TaskCode && x.DeleteNbr == 0);
                if (task == null)
                {
                    return new BaseResponseDto() { ErrorCode = StatusCodes.Status404NotFound, ErrorMessage = $"Task not found with Task Code: {createTaskDetailsRequestDto.TaskCode}" };
                }
                var taskDetails = await _taskDetailRepo.FindOneAsync(x => x.TenantCode == createTaskDetailsRequestDto.TaskDetail.TenantCode && x.TaskId == task.TaskId && x.LanguageCode== createTaskDetailsRequestDto.TaskDetail.LanguageCode && x.DeleteNbr == 0);
                if (taskDetails != null)
                {
                    return new BaseResponseDto() { ErrorCode = StatusCodes.Status409Conflict, ErrorMessage = $"Task Details are already Existed with Tenant Code: {createTaskDetailsRequestDto.TaskDetail.TenantCode} and Task Code:{createTaskDetailsRequestDto.TaskCode}" };
                }
                var taskDetailsModel = _mapper.Map<TaskDetailModel>(createTaskDetailsRequestDto.TaskDetail);
                taskDetailsModel.TaskId = task.TaskId;
                taskDetailsModel.CreateTs = DateTime.UtcNow;
                taskDetailsModel.DeleteNbr = 0;
                taskDetailsModel.TaskDetailId = 0;
                await _taskDetailRepo.CreateAsync(taskDetailsModel);
                _taskDetailsLogger.LogInformation("{ClassName}:{MethodName}: Task Details are created successfully, for Tenant Code: {TenantCode}",
                 className, methodName, createTaskDetailsRequestDto.TaskDetail.TenantCode);
                return new BaseResponseDto();

            }
            catch (Exception ex)
            {
                _taskDetailsLogger.LogError(ex, "{ClassName}:{MethodName}: Error Creating taskDetails for Tenant Code: {TenantCode}", className, methodName, createTaskDetailsRequestDto.TaskDetail.TenantCode);
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
                _taskDetailsLogger.LogInformation("{ClassName}:{MethodName}: Started processing...TaskDetailId:{TaskDetailId}", className, methodName, taskDetailId);

                var taskDetailModel = await _taskDetailRepo.FindOneAsync(x => x.TaskDetailId == taskDetailId && x.DeleteNbr == 0);

                if (taskDetailModel == null)
                {
                    return new TaskDetailResponseDto() { TaskDetail = _mapper.Map<TaskDetailDto>(taskDetailRequestDto), ErrorCode = StatusCodes.Status404NotFound, ErrorMessage = $"No task details found for given TaskDetailId: {taskDetailId}" };
                }

                taskDetailModel.TaskId = taskDetailRequestDto.TaskId;
                taskDetailModel.TermsOfServiceId = taskDetailRequestDto.TermsOfServiceId;
                taskDetailModel.TaskHeader = taskDetailRequestDto.TaskHeader;
                taskDetailModel.TaskDescription = taskDetailRequestDto.TaskDescription;
                taskDetailModel.LanguageCode = taskDetailRequestDto.LanguageCode;
                taskDetailModel.TenantCode = taskDetailRequestDto.TenantCode;
                taskDetailModel.TaskCtaButtonText = taskDetailRequestDto.TaskCtaButtonText;
                taskDetailModel.UpdateTs = DateTime.UtcNow;
                taskDetailModel.UpdateUser = taskDetailRequestDto.UpdateUser??Constant.SystemUser;
                await _taskDetailRepo.UpdateAsync(taskDetailModel);

                var taskDetail = _mapper.Map<TaskDetailDto>(taskDetailModel);
                _taskDetailsLogger.LogInformation("{ClassName}.{MethodName}: Ended Successfully.", className, methodName);

                return new TaskDetailResponseDto() { TaskDetail = taskDetail};
            }
            catch (Exception ex)
            {
                _taskDetailsLogger.LogError(ex, "{ClassName}:{MethodName}: Error processing for TaskDetailId: {TaskDetailId}", className, methodName, taskDetailId);
                return new TaskDetailResponseDto() { TaskDetail = _mapper.Map<TaskDetailDto>(taskDetailRequestDto), ErrorCode = StatusCodes.Status409Conflict, ErrorMessage = ex.Message };
            }
        }
    }
}
