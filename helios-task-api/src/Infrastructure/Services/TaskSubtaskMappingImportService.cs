using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.Common.Core.Extensions;
using SunnyRewards.Helios.Task.Core.Domain.Constants;
using SunnyRewards.Helios.Task.Core.Domain.Dtos;
using SunnyRewards.Helios.Task.Infrastructure.Repositories;
using SunnyRewards.Helios.Task.Infrastructure.Repositories.Interfaces;
using SunnyRewards.Helios.Task.Infrastructure.Services.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunnyRewards.Helios.Task.Infrastructure.Services
{
    public class TaskSubtaskMappingImportService : ITaskSubtaskMappingImportService
    {
        private readonly ILogger<TaskSubtaskMappingImportService> _importLogger;
        private readonly IMapper _mapper;
        private readonly ITaskRewardRepo _taskRewardRepo;
        private readonly ISubTaskRepo _subTaskRepo;
        private readonly ISubtaskService _subTaskService;
        private readonly ITaskService _taskService;
        private readonly ITaskExternalMappingRepo _taskExternalMappingRepo;
        private readonly ITenantTaskCategoryRepo _tenantTaskCategoryRepo;
        private readonly ITenantTaskCategoryService _tenantTaskCategoryService;
        private readonly ITaskCategoryRepo _taskCategoryRepo;

        const string className = nameof(TaskSubtaskMappingImportService);


        public TaskSubtaskMappingImportService(
            ILogger<TaskSubtaskMappingImportService> importLogger,
            IMapper mapper,
            ITaskRewardRepo taskRewardRepo,
            ISubTaskRepo subTaskRepo,
            ISubtaskService subTaskService,
            ITaskExternalMappingRepo taskExternalMappingRepo,
            ITenantTaskCategoryRepo tenantTaskCategoryRepo,
            ITaskCategoryRepo taskCategoryRepo,
            ITenantTaskCategoryService tenantTaskCategoryService,
            ITaskService taskService)
        {
            _importLogger = importLogger;
            _mapper = mapper;
            _taskRewardRepo = taskRewardRepo;
            _subTaskRepo = subTaskRepo;
            _subTaskService = subTaskService;
            _taskExternalMappingRepo = taskExternalMappingRepo;
            _tenantTaskCategoryRepo = tenantTaskCategoryRepo;
            _taskCategoryRepo = taskCategoryRepo;
            _tenantTaskCategoryService = tenantTaskCategoryService;
            _taskService = taskService;
        }

        public async Task<BaseResponseDto> ImportSubtask(ImportTaskRewardDetailsRequestDto subTasks, Dictionary<long, string> taskRwardLookUp)
        {
            const string methodName = nameof(ImportSubtask);
            StringBuilder sb = new StringBuilder();
            if (subTasks?.SubTasks != null && taskRwardLookUp.Count>0)
            {
                foreach (var subTask in subTasks.SubTasks)
                {
                    try
                    {
                        if (subTask != null)
                        {
                            if (subTask.ParentTaskRewardId > 0 
                                && taskRwardLookUp.ContainsKey(subTask.ParentTaskRewardId) && subTask.ChildTaskRewardId > 0 
                                && taskRwardLookUp.ContainsKey(subTask.ChildTaskRewardId) && taskRwardLookUp[subTask.ParentTaskRewardId] != null 
                                && taskRwardLookUp[subTask.ChildTaskRewardId]!=null)
                            {
                                var parentTaskData = await _taskRewardRepo.FindOneAsync(x =>x.TaskRewardCode !=null && x.TaskRewardCode == taskRwardLookUp[subTask.ParentTaskRewardId] && x.DeleteNbr == 0);
                                var childTaskData = await _taskRewardRepo.FindOneAsync(x => x.TaskRewardCode != null && x.TaskRewardCode == taskRwardLookUp[subTask.ChildTaskRewardId] && x.DeleteNbr == 0);
                                if (parentTaskData != null && childTaskData != null)
                                {
                                    var subtaskExist = await _subTaskRepo.FindOneAsync(x => x.ParentTaskRewardId == parentTaskData.TaskRewardId && x.ChildTaskRewardId == childTaskData.TaskRewardId && x.DeleteNbr == 0);
                                    if (subtaskExist != null)
                                    {

                                        SubTaskUpdateRequestDto subtaskRequestDto = _mapper.Map<SubTaskUpdateRequestDto>(subtaskExist);
                                        subtaskRequestDto.ConfigJson = subTask.ConfigJson;
                                        var updateResponse = await _subTaskService.UpdateSubtask(subtaskRequestDto);
                                        if (updateResponse.ErrorCode != null)
                                        {
                                            _importLogger.LogError("{className}.{methodName}: Subtask import failed  for subtask : {subtask}, error message: {errorMessage}", className, methodName, subTask.ToJson(), updateResponse.ErrorMessage);

                                        }
                                    }
                                    else
                                    {
                                        SubtaskRequestDto subtaskRequestDto = new SubtaskRequestDto
                                        {
                                            ChildTaskRewardCode = childTaskData.TaskRewardCode,
                                            ParentTaskRewardCode = parentTaskData.TaskRewardCode,
                                            Subtask = new PostSubTaskDto
                                            {
                                                CreateUser = Constant.ImportUser,
                                                ChildTaskRewardId = childTaskData.TaskRewardId,
                                                ParentTaskRewardId = parentTaskData.TaskRewardId,
                                                ConfigJson = subTask.ConfigJson,
                                                
                                            }
                                        };
                                        var createResponse = await _subTaskService.CreateSubTask(subtaskRequestDto);
                                        if (createResponse.ErrorCode != null)
                                        {
                                            _importLogger.LogError("{className}.{methodName}: Subtask import failed  for subtask : {subtask}, error message: {errorMessage}", className, methodName, subTask.ToJson(), createResponse.ErrorMessage);

                                        }
                                    }


                                }

                            }
                            else
                        {
                            _importLogger.LogError("{className}.{methodName}: Subtask import failed  for subtask as parent and child task reward record not found: {subtask}", className, methodName, subTask?.ToJson());

                        }
                        }
                        
                    }
                    catch (Exception ex)
                    {
                        _importLogger.LogError(ex, "{ClassName}:{MethodName}: Error while importing subtask details for request : {requestDto}", className, methodName, subTask?.ToJson());
                        sb.AppendLine("Task Error: " + ex.Message);

                    }
                }

            }
            return new BaseResponseDto { ErrorCode = sb.Length > 0 ? StatusCodes.Status206PartialContent : null, ErrorMessage = sb.Length > 0 ? sb.ToString() : null };

        }
        public async Task<BaseResponseDto> ImportTenantTaskCategoryMapping(ImportTaskRewardDetailsRequestDto tenantTaskCategorys)
        {
            const string methodName = nameof(ImportSubtask);
            StringBuilder sb = new StringBuilder();
            if (tenantTaskCategorys?.TenantTaskCategory != null)
            {
                foreach (var tenantTaskCategory in tenantTaskCategorys.TenantTaskCategory)
                {
                    try
                    {
                        if (tenantTaskCategory.TenantTaskCategory != null)
                        {
                            if (tenantTaskCategory.TaskCategoryCode != null)
                            {
                                var taskCategory = await _taskCategoryRepo.FindOneAsync(x => x.TaskCategoryCode == tenantTaskCategory.TaskCategoryCode);
                                if (taskCategory == null)
                                {
                                    _importLogger.LogError("{className}.{methodName}: task Category not found for tenantTaskCategory import {tenantTaskCategory}", className, methodName, tenantTaskCategory.TaskCategoryCode?.ToJson());
                                    continue;
                                }
                                var tenantTaskCategoryData = await _tenantTaskCategoryRepo.FindOneAsync(x => x.TaskCategoryId == taskCategory.TaskCategoryId && x.TenantCode == tenantTaskCategorys.TenantCode && x.DeleteNbr == 0);
                                if (tenantTaskCategoryData == null)
                                {
                                    TenantTaskCategoryRequestDto tenantTaskCategoryRequestDto = _mapper.Map<TenantTaskCategoryRequestDto>(tenantTaskCategory.TenantTaskCategory);
                                    tenantTaskCategoryRequestDto.TenantCode = tenantTaskCategorys.TenantCode;
                                    tenantTaskCategoryRequestDto.TaskCategoryId = taskCategory.TaskCategoryId;
                                    tenantTaskCategoryRequestDto.CreateUser = Constant.ImportUser;
                                    var createResponse = await _tenantTaskCategoryService.CreateTenantTaskCategory(tenantTaskCategoryRequestDto);
                                    if (createResponse.ErrorCode != null)
                                    {
                                        _importLogger.LogError("{className}.{methodName}: Subtask import failed  for subtask : {subtask}, error message: {errorMessage}", className, methodName, tenantTaskCategory.TenantTaskCategory.ToJson(), createResponse.ErrorMessage);

                                    }
                                }
                                else
                                {
                                    tenantTaskCategory.TenantTaskCategory.TenantCode = tenantTaskCategorys.TenantCode;
                                    tenantTaskCategory.TenantTaskCategory.TaskCategoryId = taskCategory.TaskCategoryId;
                                    tenantTaskCategory.TenantTaskCategory.TenantTaskCategoryId = tenantTaskCategoryData.TenantTaskCategoryId;
                                    var updateResponse = await _tenantTaskCategoryService.UpdateTenantTaskCategory(tenantTaskCategory.TenantTaskCategory);
                                    if (updateResponse.ErrorCode != null)
                                    {
                                        _importLogger.LogError("{className}.{methodName}: Subtask import failed  for subtask : {subtask}, error message: {errorMessage}", className, methodName, tenantTaskCategory.TaskCategoryCode.ToJson(), updateResponse.ErrorMessage);

                                    }
                                }


                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _importLogger.LogError(ex, "{ClassName}:{MethodName}: Error while importing tenant Task category details for request : {requestDto}", className, methodName, tenantTaskCategory?.TenantTaskCategory?.ToJson());
                        sb.AppendLine("Task Error: " + ex.Message);

                    }
                }

            }
            return new BaseResponseDto { ErrorCode = sb.Length > 0 ? StatusCodes.Status206PartialContent : null, ErrorMessage = sb.Length > 0 ? sb.ToString() : null };

        }

        public async Task<BaseResponseDto> ImportTaskExternalMapping(ImportTaskRewardDetailsRequestDto taskExternalMappingDto)
        {
            const string methodName = nameof(ImportSubtask);
            StringBuilder sb = new StringBuilder();
            if (taskExternalMappingDto != null && taskExternalMappingDto.TaskExternalMappings!=null)
            {
                foreach (var taskExternalMapping in taskExternalMappingDto.TaskExternalMappings)
                {
                    try
                    {
                        if (taskExternalMapping != null)
                        {
                            if (taskExternalMapping.TaskExternalCode != null && taskExternalMapping.TaskThirdPartyCode != null)
                            {
                                var TaskRewardDataExists = await _taskRewardRepo.FindOneAsync(x => x.TaskExternalCode == taskExternalMapping.TaskExternalCode &&  x.TenantCode == taskExternalMappingDto.TenantCode && x.DeleteNbr == 0);
                                if (TaskRewardDataExists == null)
                                {
                                    _importLogger.LogError(@"{className}.{methodName}: Task external mapping import failed  for as task reward for tenant not exist task external code: {extCode}, tenant code: {TenantCode}", className, methodName, taskExternalMapping.TaskExternalCode, taskExternalMappingDto.TenantCode);

                                    continue;
                                }

                                var taskexternalMapping = await _taskExternalMappingRepo.FindOneAsync(x => x.TaskThirdPartyCode == taskExternalMapping.TaskThirdPartyCode && x.TaskExternalCode == taskExternalMapping.TaskExternalCode
                                && x.TenantCode == taskExternalMappingDto.TenantCode);
                                if (taskexternalMapping != null)
                                {
                                    _importLogger.LogInformation("{className}.{methodName}: task external Mapping exists {taskExternalMapping}", className, methodName, taskexternalMapping.ToJson());
                                    continue;
                                }
                                else
                                {
                                    TaskExternalMappingRequestDto taskExternalMappingRequestDto = _mapper.Map<TaskExternalMappingRequestDto>(taskExternalMapping);
                                    taskExternalMappingRequestDto.TenantCode = taskExternalMappingDto.TenantCode;
                                    taskExternalMappingRequestDto.CreateUser = Constant.ImportUser;

                                    var createResponse = await _taskService.CreateTaskExternalMapping(taskExternalMappingRequestDto);
                                    if (createResponse.ErrorCode != null)
                                    {
                                        _importLogger.LogError("{className}.{methodName}: Task external mapping import failed  for mapping : {subtask}, error message: {errorMessage}", className, methodName, taskExternalMapping.ToJson(), createResponse.ErrorMessage);

                                    }
                                }



                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _importLogger.LogError(ex, "{ClassName}:{MethodName}: Error while importing tenant Task category details for request : {requestDto}", className, methodName, taskExternalMapping?.ToJson());
                        sb.AppendLine("Task Error: " + ex.Message);

                    }
                }

            }
            return new BaseResponseDto { ErrorCode = sb.Length > 0 ? StatusCodes.Status206PartialContent : null, ErrorMessage = sb.Length > 0 ? sb.ToString() : null };

        }
    }
}
