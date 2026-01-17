using AutoMapper;
using Azure;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using NHibernate.Mapping.ByCode.Impl;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.Common.Core.Extensions;
using SunnyRewards.Helios.Task.Core.Domain.Dtos;
using SunnyRewards.Helios.Task.Core.Domain.Models;
using SunnyRewards.Helios.Task.Infrastructure.Repositories.Interfaces;
using SunnyRewards.Helios.Task.Infrastructure.Services.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Text.RegularExpressions;
using SunnyRewards.Helios.Task.Core.Domain.Constants;
using System.Threading.Tasks;
using Constant = SunnyRewards.Helios.Task.Core.Domain.Constants.Constant;
using SunnyRewards.Helios.Task.Infrastructure.Repositories;
using static Google.Apis.Requests.BatchRequest;
using System.Diagnostics.CodeAnalysis;


namespace SunnyRewards.Helios.Task.Infrastructure.Services
{
    [ExcludeFromCodeCoverage]
    public class ImportTaskService : IImportTaskService
    {
        private readonly ILogger<ImportTaskService> _importLogger;
        private readonly IMapper _mapper;
        private readonly ITaskRepo _taskRepo;
        private readonly ITaskCategoryRepo _taskCategoryRepo;
        private readonly ITaskRewardRepo _taskRewardRepo;
        private readonly ITaskDetailRepo _taskDetailRepo;
        private readonly ITaskService _taskservice;
        private readonly ITaskSubtaskMappingImportService _taskSubtaskMappingImportService;
        private readonly ITaskRewardService _taskRewardService;
        private readonly ITaskDetailsService _taskDetailsService;
        private readonly ITaskTypeRepo _taskTypeRepo;
        private readonly ITaskRewardTypeRepo _taskRewardTypeRepo;
        private readonly Dictionary<long, string> _taskRewardsLookup;
        private readonly Dictionary<string, long> _termsOfServiceLookup;
        private readonly ITermsOfServiceRepo _termsOfServiceRepo;


        const string className = nameof(ImportTaskService);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="taskLogger"></param>
        /// <param name="mapper"></param>
        /// <param name="taskRepo"></param>
        /// <param name="taskRewardRepo"></param>
        /// <param name="taskDetailRepo"></param>
        /// <param name="termsOfServiceRepo"></param>
        /// <param name="tenantTaskCategoryRepo"></param>
        /// <param name="taskTypeRepo"></param>
        /// <param name="taskRewardTypeRepo"></param>
        /// <param name="consumerTaskRepo"></param>

        public ImportTaskService(
            ILogger<ImportTaskService> importLogger,
            IMapper mapper,
            ITaskRepo taskRepo,
            ITaskRewardRepo taskRewardRepo, ITaskDetailsService taskDetailsService,
            ITaskDetailRepo taskDetailRepo, ITaskService taskService, ITaskRewardService taskRewardService,
               ITaskTypeRepo taskTypeRepo, ITaskRewardTypeRepo taskRewardTypeRepo, ITaskCategoryRepo taskCategoryRepo,
               ITaskSubtaskMappingImportService taskSubtaskMappingImportService, ITermsOfServiceRepo termsOfServiceRepo
           )

        {
            _importLogger = importLogger;
            _mapper = mapper;
            _taskRepo = taskRepo;
            _taskRewardRepo = taskRewardRepo;
            _taskDetailRepo = taskDetailRepo;
            _taskservice = taskService;
            _taskRewardService = taskRewardService;
            _taskDetailsService = taskDetailsService;
            _taskTypeRepo = taskTypeRepo;
            _taskRewardTypeRepo = taskRewardTypeRepo;
            _taskCategoryRepo = taskCategoryRepo;
            _taskSubtaskMappingImportService = taskSubtaskMappingImportService;
            _taskRewardsLookup = new Dictionary<long, string>();
            _termsOfServiceRepo = termsOfServiceRepo;
            _termsOfServiceLookup = new Dictionary<string, long>();
        }
        public async Task<ImportTaskResponseDto> ImportTask(ImportTaskRewardDetailsRequestDto createTaskDetailsRequestDto)
        {
            const string methodName = nameof(ImportTask);
            int count = 0;
            var taskRewardList = new List<ImportTaskRewardDto>();
            if (createTaskDetailsRequestDto.TaskRewardDetails == null || createTaskDetailsRequestDto.TaskRewardDetails.Count <= 0 || string.IsNullOrEmpty(createTaskDetailsRequestDto.TenantCode))
            {
                _importLogger.LogError("{className}.{methodName}: invalid createTaskDetailsRequestDto {createTaskDetailsRequestDto}", className, methodName, createTaskDetailsRequestDto?.ToJson());
                return new ImportTaskResponseDto { ErrorCode = StatusCodes.Status204NoContent, ErrorDescription = "Invalid Request" };
            }
            if (createTaskDetailsRequestDto.TermsOfServices.Count > 0)
            {
                await ImportTermsOfService(createTaskDetailsRequestDto.TermsOfServices);
            }
            foreach (var taskRewardDetail in createTaskDetailsRequestDto.TaskRewardDetails)
            {
                try
                {
                    if (taskRewardDetail?.Task == null && taskRewardDetail?.Task.Task == null)
                    {
                        _importLogger.LogInformation("{className}.{methodName}: Request doesn't contain task data for import {TaskRequestDto}", className, methodName, taskRewardDetail?.Task?.ToJson());
                    }
                    else if (string.IsNullOrEmpty(taskRewardDetail?.Task.Task?.TaskName))
                    {
                        _importLogger.LogError("{className}.{methodName}: Task name cannot be empty or null for task import {TaskName}", className, methodName, taskRewardDetail?.Task.Task?.TaskName?.ToJson());
                    }
                    else
                    {
                        var task = await GetTask(taskRewardDetail.Task.Task.TaskName);
                        var tasktype = await _taskTypeRepo.FindOneAsync(x => x.TaskTypeCode == taskRewardDetail.Task.TaskTypeCode);
                        long? taskCategoryId = null;
                        if (!string.IsNullOrEmpty(taskRewardDetail.Task.TaskCategoryCode))
                        {
                            var taskCategory = await _taskCategoryRepo.FindOneAsync(x => x.TaskCategoryCode == taskRewardDetail.Task.TaskCategoryCode);
                            if (taskCategory == null)
                            {
                                _importLogger.LogError("{className}.{methodName}: task Category not found for task import {TaskName}", className, methodName, taskRewardDetail?.Task.Task?.TaskName?.ToJson());
                                continue;
                            }
                            taskCategoryId = taskCategory.TaskCategoryId;

                        }
                        if (tasktype == null)
                        {
                            _importLogger.LogError("{className}.{methodName}: Task Type not found for task import {TaskName}", className, methodName, taskRewardDetail?.Task.Task?.TaskName?.ToJson());
                            continue;
                        }
                        if (task == null)
                        {
                            _importLogger.LogInformation("{className}.{methodName}: sending request to create a new task {Task}", className, methodName, taskRewardDetail?.Task?.ToJson());

                            CreateTaskRequestDto createTask = _mapper.Map<CreateTaskRequestDto>(taskRewardDetail?.Task.Task);
                            createTask.CreateUser = Constant.ImportUser;
                            createTask.TaskTypeId = tasktype.TaskTypeId;
                            createTask.TaskCategoryId = taskCategoryId;
                            createTask.TaskCode = "tsk-" + Guid.NewGuid().ToString("N");
                            var response = await _taskservice.CreateTask(createTask);
                            if (response != null && response.ErrorCode == null)
                            {
                                task = await GetTask(taskRewardDetail.Task.Task.TaskName);
                                if (task != null && task.TaskCode != null)
                                {
                                    if (taskRewardDetail?.TaskDetail != null && taskRewardDetail?.TaskReward?.TaskReward != null)
                                    {
                                        response = await InsertTaskRewardAndDetails(task.TaskCode, task.TaskId, taskRewardDetail, createTaskDetailsRequestDto.TenantCode, taskRewardList,
                                            createTaskDetailsRequestDto.TermsOfServices);
                                        if (response.ErrorCode != null)
                                        {
                                            _importLogger.LogError("{className}.{methodName}: Error occurred while creating TaskRewardAndDetails  {Task}", className, methodName, taskRewardDetail?.Task?.ToJson());
                                        }
                                    }
                                }
                                else
                                {
                                    _importLogger.LogError("{className}.{methodName}: newly created task was not found {Task}", className, methodName, taskRewardDetail?.Task?.ToJson());

                                }

                            }

                        }
                        else
                        {
                            _importLogger.LogInformation("{className}.{methodName}: sending request to update a new task {Task}", className, methodName, taskRewardDetail?.Task?.ToJson());

                            TaskRequestDto taskRequestDto = _mapper.Map<TaskRequestDto>(taskRewardDetail?.Task.Task);
                            taskRequestDto.TaskTypeId = tasktype.TaskTypeId;
                            taskRequestDto.TaskCode = task.TaskCode;
                            taskRequestDto.TaskCategoryId = taskCategoryId;
                            BaseResponseDto baseResponseDto = new BaseResponseDto();
                            var taskresponse = await _taskservice.UpdateImportTaskAsync(task.TaskId, taskRequestDto);
                            if (taskresponse != null && taskresponse.ErrorCode == null && !string.IsNullOrEmpty(taskresponse?.Task?.TaskCode))
                            {
                                TaskRewardModel reward = new TaskRewardModel();
                                if (taskRewardDetail?.TaskReward != null && !string.IsNullOrEmpty(taskRewardDetail.TaskReward.TaskReward?.TaskExternalCode))
                                {
                                    reward = await CheckTaskRewardExists(taskRewardDetail.TaskReward.TaskReward.TaskExternalCode, createTaskDetailsRequestDto.TenantCode);
                                }
                                if (reward?.TaskExternalCode != null && taskRewardDetail != null)
                                {
                                    baseResponseDto = await UpdateTaskRewardAndDetails(taskresponse.Task.TaskId, taskresponse.Task.TaskCode, taskRewardDetail, reward,
                                        createTaskDetailsRequestDto.TenantCode, taskRewardList, createTaskDetailsRequestDto.TermsOfServices);
                                    if (baseResponseDto.ErrorCode != null)
                                    {
                                        _importLogger.LogError("{className}.{methodName}: Error occurred while Updating TaskRewardAndDetails  {Task}", className, methodName, taskRewardDetail?.Task?.ToJson());
                                    }
                                }
                                else
                                {
                                    if (taskRewardDetail?.TaskDetail != null && taskRewardDetail?.TaskReward?.TaskReward != null)
                                    {
                                        _importLogger.LogInformation("{className}.{methodName}:sending request to create task reward for existing task {Task}", className, methodName, taskRewardDetail?.Task?.ToJson());
                                        baseResponseDto = await InsertTaskRewardAndDetails(taskresponse.Task.TaskCode, taskresponse.Task.TaskId, taskRewardDetail,
                                            createTaskDetailsRequestDto.TenantCode, taskRewardList, createTaskDetailsRequestDto.TermsOfServices);
                                        if (baseResponseDto.ErrorCode != null)
                                        {
                                            _importLogger.LogError("{className}.{methodName}: Error occurred while Inserting TaskRewardAndDetails  {Task}, with response {response}", className, methodName, taskRewardDetail?.Task?.ToJson(), baseResponseDto.ToJson());
                                        }
                                    }
                                    else
                                    {
                                        _importLogger.LogError("{className}.{methodName}: Error occurred while Inserting TaskRewardAndDetails as record not exists {taskRewardDetail}", className, methodName, taskRewardDetail?.ToJson());

                                    }

                                }

                            }
                        }

                    }
                }

                catch (Exception ex)
                {
                    ++count;
                    _importLogger.LogError(ex, "{ClassName}:{MethodName}: Error while importing task details for request : {requestDto}", className, methodName, createTaskDetailsRequestDto.ToJson());

                }
            }
            if (createTaskDetailsRequestDto.SubTasks?.Count > 0 && _taskRewardsLookup != null)
            {
                var subtaskResponse = await _taskSubtaskMappingImportService.ImportSubtask(createTaskDetailsRequestDto, _taskRewardsLookup);
                if (subtaskResponse.ErrorCode != null)
                {
                    _importLogger.LogError("{className}.{methodName}: Error occurred while importing Subtask for tenant code {tenant_Code}", className, methodName, createTaskDetailsRequestDto.TenantCode);
                }
            }
            if (createTaskDetailsRequestDto.TaskExternalMappings?.Count > 0)
            {
                var taskExternalMappingResponse = await _taskSubtaskMappingImportService.ImportTaskExternalMapping(createTaskDetailsRequestDto);
                if (taskExternalMappingResponse.ErrorCode != null)
                {
                    _importLogger.LogError("{className}.{methodName}: Error occurred while importing task external mapping for tenant code {tenantcode}", className, methodName, createTaskDetailsRequestDto.TenantCode);
                }
            }
            if (createTaskDetailsRequestDto.TenantTaskCategory?.Count > 0)
            {
                var tenantTaskCategoryResponse = await _taskSubtaskMappingImportService.ImportTenantTaskCategoryMapping(createTaskDetailsRequestDto);
                if (tenantTaskCategoryResponse.ErrorCode != null)
                {
                    _importLogger.LogError("{className}.{methodName}: Error occurred while importing Tenant task category mapping for tenant code {tenantCode}", className, methodName, createTaskDetailsRequestDto.TenantCode);
                }
            }
            return new ImportTaskResponseDto
            {
                ErrorCode = count > 0 ? StatusCodes.Status206PartialContent : null,
                ErrorMessage = count > 0 ? "Some Records encountered Error" : null,
                TaskRewardList = taskRewardList
            };

        }

        /// <summary>
        /// Imports or updates the Terms of Service records based on the provided input.
        /// If a matching TermsOfServiceCode and LanguageCode exists, it updates the record; otherwise, it creates a new one.
        /// </summary>
        /// <param name="createTaskDetailsRequestDto">DTO containing the list of Terms of Service to import.</param>
        /// <param name="termsOfServiceIds">A list to which the method will add the IDs of the imported/updated Terms of Service.</param>
        /// <returns>A Task representing the asynchronous operation.</returns>
        private async System.Threading.Tasks.Task ImportTermsOfService(List<TermsOfServiceDto> termsOfServices)
        {
            foreach (var termsOfService in termsOfServices)
            {
                try
                {
                    _importLogger.LogInformation("Processing TermsOfServiceCode: {Code}, LanguageCode: {Lang}",
                        termsOfService.TermsOfServiceCode, termsOfService.LanguageCode);

                    var termsOfServiceModel = await _termsOfServiceRepo.FindOneAsync(x =>
                        x.TermsOfServiceCode == termsOfService.TermsOfServiceCode &&
                        x.LanguageCode == termsOfService.LanguageCode &&
                        x.DeleteNbr == 0);

                    if (termsOfServiceModel != null && termsOfServiceModel.TermsOfServiceId > 0)
                    {
                        _importLogger.LogInformation("Existing Terms of Service found. Updating TermsOfServiceCode: {Code}",
                            termsOfServiceModel.TermsOfServiceCode);
                        termsOfServiceModel.TermsOfServiceText = termsOfService.TermsOfServiceText;
                        termsOfServiceModel.UpdateTs = DateTime.UtcNow;
                        termsOfServiceModel.UpdateUser = Constant.ImportUser;

                        await _termsOfServiceRepo.UpdateAsync(termsOfServiceModel);
                        _termsOfServiceLookup[termsOfService.TermsOfServiceCode] = termsOfServiceModel.TermsOfServiceId;
                        _importLogger.LogInformation("Updated Terms of Service with TermsOfServiceCode: {Code}",
                            termsOfServiceModel.TermsOfServiceCode);
                    }
                    else
                    {
                        _importLogger.LogInformation("No matching Terms of Service found. Creating new record for TermsOfServiceCode: {Code}",
                            termsOfService.TermsOfServiceCode);

                        termsOfServiceModel = _mapper.Map<TermsOfServiceModel>(termsOfService);
                        termsOfServiceModel.TermsOfServiceId = 0;
                        termsOfServiceModel.CreateTs = DateTime.UtcNow;
                        termsOfServiceModel.CreateUser = Constant.ImportUser;
                        termsOfServiceModel.UpdateUser = null;
                        termsOfServiceModel.UpdateTs = null;

                        await _termsOfServiceRepo.CreateAsync(termsOfServiceModel);
                        _termsOfServiceLookup[termsOfService.TermsOfServiceCode] = termsOfServiceModel.TermsOfServiceId;
                        _importLogger.LogInformation("Created new Terms of Service with TermsOfServiceId: {Code}",
                            termsOfServiceModel.TermsOfServiceCode);
                    }
                }
                catch (Exception ex)
                {
                    _importLogger.LogError(ex, "Error occurred while importing Terms of Service. Code: {Code}, Language: {Lang}",
                        termsOfService.TermsOfServiceCode, termsOfService.LanguageCode);
                    continue;
                }
            }
        }


        private static string CleanTaskName(string taskName)
        {
            if (string.IsNullOrEmpty(taskName))
            {
                return string.Empty;
            }
            // Convert to lowercase, remove whitespace, and remove all non-alphanumeric symbols
            string cleanedTaskName = taskName.ToLower().Replace(" ", "")
            .Replace("!", "")
            .Replace("@", "")
            .Replace("#", "")
            .Replace("$", "")
            .Replace("%", "")
            .Replace("^", "")
            .Replace("&", "")
            .Replace("*", "")
            .Replace("(", "")
            .Replace(")", "")
            .Replace("[", "")
            .Replace("]", "")
            .Replace("{", "")
            .Replace("}", "")
            .Replace(":", "")
            .Replace(";", "")
            .Replace("'", "")
            .Replace("<", "")
            .Replace(">", "")
            .Replace(",", "")
            .Replace(".", "")
            .Replace("?", "")
            .Replace("/", "")
            .Replace(@"\", "")
            .Replace(@"|", "")
            .Replace(@"`", "")
            .Replace(@"~", "")
            .Replace(@"+", "")
            .Replace(@"-", "")
            .Replace(@"=", "")
            .Replace(@"_", "");   // Convert to lowercase
            cleanedTaskName = Regex.Replace(cleanedTaskName, @"\s+", "");  // Remove whitespace
            cleanedTaskName = Regex.Replace(cleanedTaskName, @"\W", ""); // Remove non-alphanumeric characters

            return cleanedTaskName;
        }
        private async Task<TaskModel?> GetTask(string taskName)
        {
            var cleanedTaskName = CleanTaskName(taskName);

            var task = await _taskRepo.FindAsync(x => x.TaskName != null && x.TaskName.ToLower().Replace(" ", "")
            .Replace("!", "")
            .Replace("@", "")
            .Replace("#", "")
            .Replace("$", "")
            .Replace("%", "")
            .Replace("^", "")
            .Replace("&", "")
            .Replace("*", "")
            .Replace("(", "")
            .Replace(")", "")
            .Replace("[", "")
            .Replace("]", "")
            .Replace("{", "")
            .Replace("}", "")
            .Replace(":", "")
            .Replace(";", "")
            .Replace("'", "")
            .Replace("<", "")
            .Replace(">", "")
            .Replace(",", "")
            .Replace(".", "")
            .Replace("?", "")
            .Replace("/", "")
            .Replace(@"\", "")
            .Replace(@"|", "")
            .Replace(@"`", "")
            .Replace(@"~", "")
            .Replace(@"+", "")
            .Replace(@"-", "")
            .Replace(@"=", "")
            .Replace(@"_", "")

            == cleanedTaskName);


            if (task == null)
            {
                return null;
            }
            return task.OrderByDescending(x => x.TaskId).FirstOrDefault();
        }
        private async Task<TaskRewardModel?> CheckTaskRewardExists(string? taskExternalCode, string tenantcode)
        {
            var taskReward = await _taskRewardRepo.FindAsync(x => x.TaskExternalCode == taskExternalCode && x.TenantCode == tenantcode);
            if (taskReward == null)
            {
                return null;
            }
            return taskReward.OrderByDescending(x => x.TaskRewardId).FirstOrDefault();
        }

        public async Task<BaseResponseDto> InsertTaskRewardAndDetails(string taskCode, long taskId, ImportTaskRewardDetailDto taskRewardDetailDto,
            string tenantCode, List<ImportTaskRewardDto> taskRewardsList, List<TermsOfServiceDto> termsOfServices)
        {
            const string methodName = nameof(InsertTaskRewardAndDetails);

            BaseResponseDto response = new BaseResponseDto();
            if (taskRewardDetailDto?.TaskReward?.TaskReward != null && !String.IsNullOrEmpty(taskRewardDetailDto?.TaskReward?.TaskRewardTypeCode))
            {
                var taskRewardtype = await _taskRewardTypeRepo.FindOneAsync(x => x.RewardTypeCode == taskRewardDetailDto.TaskReward.TaskRewardTypeCode);
                if (taskRewardtype == null)
                {
                    _importLogger.LogError("{className}.{methodName}: Task Reward Type not found for task Reward import {TaskReward}", className, methodName, taskRewardDetailDto?.TaskReward?.TaskReward.ToJson());
                    return new BaseResponseDto { ErrorCode = StatusCodes.Status404NotFound, ErrorMessage = "Task Reward Type not found" };
                }
                CreateTaskRewardRequestDto createTaskRewardRequestDto = new CreateTaskRewardRequestDto
                {
                    TaskCode = taskCode,
                    TaskReward = taskRewardDetailDto?.TaskReward.TaskReward
                };
                var oldTaskRewardCode = taskRewardDetailDto?.TaskReward.TaskReward.TaskRewardCode;
                createTaskRewardRequestDto.TaskReward.TenantCode = tenantCode;
                createTaskRewardRequestDto.TaskReward.RewardTypeId = taskRewardtype.RewardTypeId;
                createTaskRewardRequestDto.TaskReward.CreateUser = Constant.ImportUser;
                createTaskRewardRequestDto.TaskReward.TaskRewardCode = "trw-" + Guid.NewGuid().ToString("N");
                _taskRewardsLookup[taskRewardDetailDto.TaskReward.TaskReward.TaskRewardId] = createTaskRewardRequestDto.TaskReward.TaskRewardCode;


                response = await _taskRewardService.CreateTaskReward(createTaskRewardRequestDto);
                if (response != null && response.ErrorCode != null)
                {
                    _importLogger.LogError("{className}.{methodName}: some error occurred while creating task reward {TaskReward}, response: {response}", className, methodName,
                        taskRewardDetailDto?.TaskReward?.ToJson(), response.ToJson());
                    return response;
                }
                var importTaskRewardDto = _mapper.Map<ImportTaskRewardDto>(createTaskRewardRequestDto.TaskReward);
                importTaskRewardDto.NewRewardCode = createTaskRewardRequestDto.TaskReward.TaskRewardCode;
                importTaskRewardDto.TaskRewardCode = oldTaskRewardCode;
                taskRewardsList.Add(importTaskRewardDto);
            }
            else
            {
                _importLogger.LogError("{className}.{methodName}: task does not contain any associated task reward {taskreward}", className, methodName, taskRewardDetailDto?.ToJson());

            }
            response = await UpsertTaskDetails(taskId, taskRewardDetailDto, taskCode, tenantCode, termsOfServices);


            return response;
        }

        private async Task<BaseResponseDto> UpdateTaskRewardAndDetails(long taskId, string taskCode, ImportTaskRewardDetailDto taskRewardDetailDto,
            TaskRewardModel taskRewardModel, string tenantCode, List<ImportTaskRewardDto> taskRewardsList, List<TermsOfServiceDto> termsOfServices)
        {
            const string methodName = nameof(UpdateTaskRewardAndDetails);

            BaseResponseDto response = new BaseResponseDto();
            if (taskRewardModel != null && taskRewardDetailDto.TaskReward?.TaskReward != null && !String.IsNullOrEmpty(taskRewardDetailDto?.TaskReward?.TaskRewardTypeCode))
            {
                var taskRewardtype = await _taskRewardTypeRepo.FindOneAsync(x => x.RewardTypeCode == taskRewardDetailDto.TaskReward.TaskRewardTypeCode);
                if (taskRewardtype == null)
                {
                    _importLogger.LogError("{className}.{methodName}: Task Reward Type not found for task Reward import {TaskReward}", className, methodName, taskRewardDetailDto?.TaskReward?.TaskReward.ToJson());
                    return new BaseResponseDto { ErrorCode = StatusCodes.Status404NotFound, ErrorMessage = "Task Reward Type not found" };
                }
                _taskRewardsLookup[taskRewardDetailDto.TaskReward.TaskReward.TaskRewardId] = taskRewardModel.TaskRewardCode;
                TaskRewardRequestDto UpdateTaskRewardRequestDto = _mapper.Map<TaskRewardRequestDto>(taskRewardDetailDto.TaskReward.TaskReward);
                UpdateTaskRewardRequestDto.TenantCode = tenantCode;
                UpdateTaskRewardRequestDto.TaskId = taskId;
                UpdateTaskRewardRequestDto.TaskRewardCode = taskRewardModel.TaskRewardCode;
                UpdateTaskRewardRequestDto.RewardTypeId = taskRewardtype.RewardTypeId;
                UpdateTaskRewardRequestDto.UpdateUser = Constant.ImportUser;

                var rewardResponse = await _taskRewardService.UpdateTaskRewardAsync(taskRewardModel.TaskRewardId, UpdateTaskRewardRequestDto);

                if (rewardResponse != null && rewardResponse.ErrorCode != null)
                {
                    _importLogger.LogError("{className}.{methodName}: some error occurred while updating task reward {TaskReward}, response: {response}", className, methodName,
                        taskRewardDetailDto.TaskReward.ToJson(), response.ToJson());
                    return rewardResponse;
                }
                var importTaskRewardDto = _mapper.Map<ImportTaskRewardDto>(rewardResponse?.TaskReward);
                importTaskRewardDto.NewRewardCode = UpdateTaskRewardRequestDto.TaskRewardCode;
                importTaskRewardDto.TaskRewardCode = taskRewardDetailDto?.TaskReward.TaskReward.TaskRewardCode;
                taskRewardsList.Add(importTaskRewardDto);
            }
            response = await UpsertTaskDetails(taskId, taskRewardDetailDto, taskCode, tenantCode, termsOfServices);

            return response;
        }
        private async Task<BaseResponseDto> UpsertTaskDetails(long Taskid, ImportTaskRewardDetailDto taskRewardDetailDto, string taskCode, string tenantCode, List<TermsOfServiceDto> termsOfServices)
        {
            const string methodName = nameof(UpsertTaskDetails);

            BaseResponseDto response = new BaseResponseDto();

            if (taskRewardDetailDto?.TaskDetail != null)
            {
                foreach (var TaskDetail in taskRewardDetailDto.TaskDetail)
                {
                    try
                    {
                        TaskDetail.LanguageCode = string.IsNullOrEmpty(TaskDetail.LanguageCode) ? Constant.LanguageCode : TaskDetail.LanguageCode;
                        if (TaskDetail != null && TaskDetail.TaskId > 0)
                        {
                            (response, var existingTermsOfServiceId) = await GetTermsOfServiceId(TaskDetail, termsOfServices);
                            if (response.ErrorCode != null)
                            {
                                return response;
                            }

                            var taskdetail = await _taskDetailRepo.FindOneAsync(x => x.TaskId == Taskid && x.TenantCode == tenantCode
                            && x.LanguageCode!.ToLower() == TaskDetail.LanguageCode!.ToLower() && x.DeleteNbr == 0);
                            if (taskdetail != null)
                            {
                                TaskDetailRequestDto updateTaskDetailRequestDto = _mapper.Map<TaskDetailRequestDto>(TaskDetail);
                                updateTaskDetailRequestDto.TenantCode = tenantCode;
                                updateTaskDetailRequestDto.TaskId = Taskid;
                                updateTaskDetailRequestDto.UpdateUser = Constant.ImportUser;
                                updateTaskDetailRequestDto.TermsOfServiceId = existingTermsOfServiceId;


                                response = await _taskDetailsService.UpdateTaskDetailAsync(taskdetail.TaskDetailId, updateTaskDetailRequestDto);
                                if (response != null && response.ErrorCode != null)
                                {
                                    _importLogger.LogError("{className}.{methodName}: some error occurred while updating task details {TaskDetails}, response: {response}", className, methodName,
                                        taskRewardDetailDto.TaskDetail.ToJson(), response.ToJson());
                                    response.ErrorMessage?.Concat(response.ErrorMessage);
                                    response.ErrorCode = StatusCodes.Status412PreconditionFailed;
                                }
                            }
                            else
                            {

                                PostTaskDetailsDto taskDetailRequestDto = _mapper.Map<PostTaskDetailsDto>(TaskDetail);
                                CreateTaskDetailsRequestDto createTaskDetailsRequestDto = new CreateTaskDetailsRequestDto { TaskCode = taskCode, TaskDetail = taskDetailRequestDto };

                                createTaskDetailsRequestDto.TaskDetail.TenantCode = tenantCode;
                                createTaskDetailsRequestDto.TaskDetail.CreateUser = Constant.ImportUser;
                                createTaskDetailsRequestDto.TaskDetail.TermsOfServiceId = existingTermsOfServiceId;


                                response = await _taskDetailsService.CreateTaskDetails(createTaskDetailsRequestDto);
                                if (response != null && response.ErrorCode != null)
                                {
                                    _importLogger.LogError("{className}.{methodName}: some error occurred while creating task details {taskdetail}, response: {response}", className, methodName,
                                        TaskDetail.ToJson(), response.ToJson());
                                    response.ErrorMessage?.Concat(response.ErrorMessage);
                                    response.ErrorCode = StatusCodes.Status412PreconditionFailed;

                                }

                            }
                        }
                        else
                        {
                            _importLogger.LogError("{className}.{methodName}: task does not contain any associated task details {taskdetail}", className, methodName, taskRewardDetailDto?.ToJson());

                        }

                    }
                    catch (Exception ex)
                    {
                        _importLogger.LogError(ex, "{ClassName}:{MethodName}: Error Creating taskDetails for Tenant Code: {TenantCode}", className, methodName, TaskDetail.TenantCode);
                        response = new BaseResponseDto
                        {
                            ErrorCode = StatusCodes.Status500InternalServerError,
                            ErrorMessage = "Error Creating taskDetails for Tenant"
                        };
                    }
                }
            }
            return response ?? new BaseResponseDto();
        }

        private async Task<(BaseResponseDto, long)> GetTermsOfServiceId(TaskDetailDto taskDetail, List<TermsOfServiceDto> termsOfServices)
        {
            const string methodName = nameof(GetTermsOfServiceId);
            var termsOfServiceId = taskDetail?.TermsOfServiceId ?? 0;
            var termsOfService = termsOfServices.FirstOrDefault(x => x.TermsOfServiceId == termsOfServiceId);

            if (termsOfService == null)
            {
                var error = $"Terms of Service not found for TermsOfServiceId: {termsOfServiceId}. Skipping record.";
                _importLogger.LogError("{className}.{methodName}: {Error}", className, methodName, error);
                return (new BaseResponseDto
                {
                    ErrorCode = StatusCodes.Status422UnprocessableEntity,
                    ErrorMessage = error
                }, 0);
            }

            if (!_termsOfServiceLookup.TryGetValue(termsOfService.TermsOfServiceCode, out var existingTermsOfServiceId)
                || existingTermsOfServiceId <= 0)
            {
                var termsOfServiceModel = await _termsOfServiceRepo.FindOneAsync(x =>
                    x.TermsOfServiceCode == termsOfService.TermsOfServiceCode && x.DeleteNbr == 0);

                if (termsOfServiceModel == null)
                {
                    var error = $"Terms of Service not found in DB for TermsOfServiceCode: {termsOfService.TermsOfServiceCode}. Skipping record.";
                    _importLogger.LogError("{className}.{methodName}: {Error}", className, methodName, error);
                    return (new BaseResponseDto
                    {
                        ErrorCode = StatusCodes.Status422UnprocessableEntity,
                        ErrorMessage = error
                    }, 0);
                }
            }
            return (new BaseResponseDto(), existingTermsOfServiceId);
        }
    }
}
