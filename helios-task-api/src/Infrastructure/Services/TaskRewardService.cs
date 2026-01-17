using AutoMapper;
using FluentNHibernate.Testing.Values;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NHibernate.Util;
using SunnyRewards.Helios.Common.Core.Domain;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.Common.Core.Services;
using SunnyRewards.Helios.Task.Core.Domain.Constants;
using SunnyRewards.Helios.Task.Core.Domain.Dtos;
using SunnyRewards.Helios.Task.Core.Domain.Json;
using SunnyRewards.Helios.Task.Core.Domain.Models;
using SunnyRewards.Helios.Task.Infrastructure.Helpers;
using SunnyRewards.Helios.Task.Infrastructure.Helpers.Interface;
using SunnyRewards.Helios.Task.Infrastructure.Repositories.Interfaces;
using SunnyRewards.Helios.Task.Infrastructure.Services.Interface;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SunnyRewards.Helios.Task.Infrastructure.Services
{
    public class TaskRewardService : BaseService, ITaskRewardService
    {
        private readonly ILogger<TaskRewardService> _taskRewardLogger;
        private readonly IMapper _mapper;
        private readonly ITaskRewardRepo _taskRewardRepo;
        private readonly ITaskRepo _taskRepo;
        private readonly ITaskDetailRepo _taskDetailRepo;
        private readonly ITermsOfServiceRepo _termsOfServiceRepo;
        private readonly IConsumerTaskRepo _consumerTaskRepo;
        private readonly ITenantTaskCategoryRepo _tenantCategoryRepo;
        private readonly ITaskTypeRepo _taskTypeRepo;
        private readonly ITaskRewardTypeRepo _taskRewardTypeRepo;
        private readonly ITaskRewardCollectionRepo _taskRewardCollectionRepo;
        private readonly ICommonTaskRewardService _commonTaskRewardService;
        private readonly IAdventureRepo _adventureRepo;
        const string className = nameof(TaskRewardService);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="taskRewardLogger"></param>
        /// <param name="mapper"></param>
        /// <param name="taskRewardRepo"></param>
        /// <param name="taskRepo"></param>
        /// <param name="taskDetailRepo"></param>
        /// <param name="termsOfService"></param>
        /// <param name="consumerTaskRepo"></param>
        /// <param name="tenantCategoryRepo"></param>
        /// <param name="taskTypeRepo"></param>
        /// <param name="taskRewardTypeRepo"></param>
        public TaskRewardService(
            ILogger<TaskRewardService> taskRewardLogger,
            IMapper mapper,
            ITaskRewardRepo taskRewardRepo,
            ITaskRepo taskRepo,
            ITaskDetailRepo taskDetailRepo,
            ITermsOfServiceRepo termsOfService,
            IConsumerTaskRepo consumerTaskRepo,
            ITenantTaskCategoryRepo tenantCategoryRepo,
            ITaskTypeRepo taskTypeRepo,
            ITaskRewardTypeRepo taskRewardTypeRepo,
            ITaskRewardCollectionRepo taskRewardCollectionRepo,
            ICommonTaskRewardService commonTaskRewardService,
            IAdventureRepo adventureRepo)
        {
            _taskRewardLogger = taskRewardLogger;
            _mapper = mapper;
            _taskRewardRepo = taskRewardRepo;
            _taskRepo = taskRepo;
            _taskDetailRepo = taskDetailRepo;
            _termsOfServiceRepo = termsOfService;
            _consumerTaskRepo = consumerTaskRepo;
            _tenantCategoryRepo = tenantCategoryRepo;
            _taskTypeRepo = taskTypeRepo;
            _taskRewardTypeRepo = taskRewardTypeRepo;
            _taskRewardCollectionRepo = taskRewardCollectionRepo;
            _commonTaskRewardService = commonTaskRewardService;
            _adventureRepo = adventureRepo;
        }

        /// <summary>
        /// Retrieves a list of task rewards from the repository and returns them in a standardized response format.
        /// </summary>
        /// <returns></returns>
        public async Task<TaskRewardsResponseDto> GetTaskRewardsAsync()
        {
            const string methodName = nameof(GetTaskRewardsAsync);
            try
            {
                var result = await _taskRewardRepo.FindAsync(x => x.DeleteNbr == 0);

                if (result == null || result.Count == 0)
                {
                    _taskRewardLogger.LogError("{ClassName}.{MethodName}: No task reward was found. Error Code: {ErrorCode}", className, methodName, StatusCodes.Status404NotFound);
                    return new TaskRewardsResponseDto
                    {
                        ErrorMessage = "No task reward was found."
                    };
                }

                return new TaskRewardsResponseDto
                {
                    TaskRewards = _mapper.Map<IList<TaskRewardDto>>(result)
                };
            }
            catch (Exception ex)
            {
                _taskRewardLogger.LogError(ex, "{ClassName}.{MethodName}: Error Message: {Message}, Error Code: {ErrorCode}", className, methodName, ex.Message, StatusCodes.Status500InternalServerError);

                return new TaskRewardsResponseDto
                {
                    ErrorCode = StatusCodes.Status500InternalServerError,
                    ErrorMessage = ex.Message
                };
            }
        }

        /// <summary>
        /// UpdateTaskRewardAsync
        /// </summary>
        /// <param name="taskRewardId"></param>
        /// <param name="updateTaskRewardRequestDto"></param>
        /// <returns></returns>
        public async Task<TaskRewardResponseDto> UpdateTaskRewardAsync(long taskRewardId, TaskRewardRequestDto taskRewardRequestDto, bool isImport = false)
        {
            const string methodName = nameof(UpdateTaskRewardAsync);
            try
            {
                _taskRewardLogger.LogInformation("{ClassName}.{MethodName}: Started processing for TaskRewardId:{TaskRewardId}", className, methodName, taskRewardId);
                var taskRewardModel = await _taskRewardRepo.FindOneAsync(x => x.TaskRewardId == taskRewardId && x.DeleteNbr == 0);

                if (taskRewardModel == null)
                {
                    return new TaskRewardResponseDto() { TaskReward = _mapper.Map<TaskRewardDto>(taskRewardRequestDto), ErrorCode = StatusCodes.Status404NotFound, ErrorMessage = $"No task found for given TaskRewardId: {taskRewardId}" };
                }

                taskRewardModel.TaskId = taskRewardRequestDto.TaskId;
                taskRewardModel.RewardTypeId = taskRewardRequestDto.RewardTypeId;
                taskRewardModel.TenantCode = taskRewardRequestDto.TenantCode;
                if (isImport)
                    taskRewardModel.TaskRewardCode = taskRewardRequestDto.TaskRewardCode;
                taskRewardModel.TaskActionUrl = taskRewardRequestDto.TaskActionUrl;
                taskRewardModel.Reward = taskRewardRequestDto.Reward;
                taskRewardModel.Priority = taskRewardRequestDto.Priority;
                taskRewardModel.Expiry = taskRewardRequestDto.Expiry;
                taskRewardModel.MinTaskDuration = taskRewardRequestDto.MinTaskDuration;
                taskRewardModel.MaxTaskDuration = taskRewardRequestDto.MaxTaskDuration;
                taskRewardModel.TaskExternalCode = taskRewardRequestDto.TaskExternalCode;
                taskRewardModel.ValidStartTs = taskRewardRequestDto.ValidStartTs;
                taskRewardModel.IsRecurring = taskRewardRequestDto.IsRecurring;
                taskRewardModel.RecurrenceDefinitionJson = taskRewardRequestDto.RecurrenceDefinitionJson;
                taskRewardModel.SelfReport = taskRewardRequestDto.SelfReport;
                taskRewardModel.TaskCompletionCriteriaJson = taskRewardRequestDto.TaskCompletionCriteriaJson;
                taskRewardModel.ConfirmReport = taskRewardRequestDto.ConfirmReport;
                taskRewardModel.UpdateUser = taskRewardRequestDto.UpdateUser ?? Constant.SystemUser;
                taskRewardModel.UpdateTs = DateTime.UtcNow;
                await _taskRewardRepo.UpdateAsync(taskRewardModel);

                _taskRewardLogger.LogInformation("{ClassName}.{MethodName}: Ended Successfully.", className, methodName);

                return new TaskRewardResponseDto() { TaskReward = _mapper.Map<TaskRewardDto>(taskRewardModel) };
            }
            catch (Exception ex)
            {
                _taskRewardLogger.LogError(ex, "{ClassName}.{MethodName}: Error Message: {Message}, Error Code: {ErrorCode}", className, methodName, ex.Message, StatusCodes.Status500InternalServerError);
                return new TaskRewardResponseDto() { TaskReward = _mapper.Map<TaskRewardDto>(taskRewardRequestDto), ErrorCode = StatusCodes.Status409Conflict, ErrorMessage = ex.Message };
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="taskRewardRequestDto"></param>
        /// <returns></returns>
        public async Task<FindTaskRewardResponseDto> GetTaskRewardDetails(FindTaskRewardRequestDto taskRewardRequestDto)
        {
            const string methodName = nameof(GetTaskRewardDetails);
            try
            {
                FindTaskRewardResponseDto taskRewardResponseDto = await GetTaskRewards(taskRewardRequestDto);
                if (taskRewardResponseDto.TaskRewardDetails == null || taskRewardResponseDto.TaskRewardDetails.Count <= 0)
                {
                    _taskRewardLogger.LogError("{className}.{methodName}: Task Details is Null or less than zero. Tenant code:{Tenant}, Error Code:{errorCode}", className, methodName, taskRewardRequestDto.TenantCode, StatusCodes.Status404NotFound);
                    return new FindTaskRewardResponseDto();
                }
                _taskRewardLogger.LogInformation("{className}.{methodName}: Successfully Retrieved TaskReward for TenantCode: {tenantCode}", className, methodName, taskRewardRequestDto.TenantCode);
                taskRewardResponseDto.TaskRewardDetails = FilterValidTaskRewards(taskRewardResponseDto.TaskRewardDetails);
                var availableTaskIds = taskRewardResponseDto.TaskRewardDetails.Select(x => x.Task?.TaskId).ToList();
                var consumerTasks = await _consumerTaskRepo.FindAsync(x => x.ConsumerCode == taskRewardRequestDto.ConsumerCode && availableTaskIds.Contains(x.TaskId) && x.DeleteNbr == 0);
                if (consumerTasks != null && consumerTasks.Count > 0)
                {
                    taskRewardResponseDto.TaskRewardDetails = await TaskHelper.FilterAvailableTasksAsync(taskRewardResponseDto.TaskRewardDetails, consumerTasks, _consumerTaskRepo, _taskRewardLogger);
                }

                _taskRewardLogger.LogInformation("{className}.{methodName}: successfully retrieved data from FindTaskRewards API for TenantCode: {TenantCode}", className, methodName, taskRewardRequestDto.TenantCode);
                return taskRewardResponseDto;
            }
            catch (Exception ex)
            {
                _taskRewardLogger.LogError(ex, "{className}.{methodName}: ERROR Msg:{msg}, Error Code:{errorCode}", className, methodName, ex.Message, StatusCodes.Status500InternalServerError);
                return new FindTaskRewardResponseDto();
            }
        }

        public async Task<FindTaskRewardResponseDto> GetTaskRewards(FindTaskRewardRequestDto taskRewardRequestDto, bool includeSubtask = false)
        {
            const string methodName = nameof(GetTaskRewards);
            // Fetch task rewards for the given tenant code
            var taskRewardList = await _taskRewardRepo.FindAsync(x => x.TenantCode == taskRewardRequestDto.TenantCode && x.DeleteNbr == 0);
            if (taskRewardList == null || taskRewardList.Count <= 0)
            {
                _taskRewardLogger.LogError("{className}.{methodName}: taskRewardList not found for given TenantCode: {tenantCode},Error Code:{errorCode}", className, methodName, taskRewardRequestDto.TenantCode, StatusCodes.Status404NotFound);
                return new FindTaskRewardResponseDto();
            }
            var taskRewardDtoList = _mapper.Map<List<TaskRewardDto>>(taskRewardList).OrderByDescending(x => x.Priority);
            var taskRewardResponseDto = new FindTaskRewardResponseDto();
            var taskIds = taskRewardDtoList.Select(item => item.TaskId).ToList();
            var taskRewardTypeIds = taskRewardDtoList.Select(tr => tr.RewardTypeId).Distinct().ToList();

            // Fetch task details, task types, terms of service, tenant task categories, and task reward types in bulk
            var tasks = includeSubtask ? (await _taskRepo.FindAsync(x => taskIds.Contains(x.TaskId) && x.DeleteNbr == 0))
                : (await _taskRepo.FindAsync(x => taskIds.Contains(x.TaskId) && !x.IsSubtask && x.DeleteNbr == 0));
            var taskCategoryIds = tasks.Select(t => t.TaskCategoryId).Distinct().ToList();
            var taskTypeIds = tasks.Select(t => t.TaskTypeId).Distinct().ToList();
            var requestedLanguageCode = string.IsNullOrWhiteSpace(taskRewardRequestDto?.LanguageCode) ? Constant.LanguageCode.ToLower() : taskRewardRequestDto?.LanguageCode?.ToLower();
            var taskDetails = await _taskDetailRepo.FindAsync(x => taskIds.Contains(x.TaskId) && x.TenantCode == taskRewardRequestDto.TenantCode && x.LanguageCode != null && x.LanguageCode.ToLower() == requestedLanguageCode && x.DeleteNbr == 0);
            if (taskDetails.Count == 0 && !string.Equals(requestedLanguageCode, Constant.LanguageCode, StringComparison.OrdinalIgnoreCase))
            {
                taskDetails = await _taskDetailRepo.FindAsync(x => taskIds.Contains(x.TaskId) && x.TenantCode == taskRewardRequestDto.TenantCode && x.LanguageCode != null && x.LanguageCode.ToLower() == Constant.LanguageCode.ToLower() && x.DeleteNbr == 0);
            }
            var taskDetailTermsOfServiceIds = taskDetails?.Select(td => td.TermsOfServiceId).Distinct().ToList();
            var termsOfServices = await _termsOfServiceRepo.FindAsync(x => taskDetailTermsOfServiceIds.Contains(x.TermsOfServiceId) && x.DeleteNbr == 0);

            var tenantTaskCategories = await _tenantCategoryRepo.FindAsync(x => taskCategoryIds.Contains(x.TaskCategoryId) && x.TenantCode == taskRewardRequestDto.TenantCode && x.DeleteNbr == 0);
            var taskTypes = await _taskTypeRepo.FindAsync(x => taskTypeIds.Contains(x.TaskTypeId) && x.DeleteNbr == 0);
            var taskRewardTypes = await _taskRewardTypeRepo.FindAsync(x => taskRewardTypeIds.Contains(x.RewardTypeId) && x.DeleteNbr == 0);

            taskRewardResponseDto = CreateTaskRewardResponseDto(taskRewardDtoList, tasks, taskDetails, termsOfServices, tenantTaskCategories, taskTypes, taskRewardTypes);
            _taskRewardLogger.LogInformation("{className}.{methodName}: successfully retrieved data from Task Rewards for TenantCode: {TenantCode}", className, methodName, taskRewardRequestDto?.TenantCode);
            return taskRewardResponseDto;
        }

        private FindTaskRewardResponseDto CreateTaskRewardResponseDto(IOrderedEnumerable<TaskRewardDto> taskRewardDtoList, IList<TaskModel> tasks, IList<TaskDetailModel> taskDetails, IList<TermsOfServiceModel> termsOfServices, IList<TenantTaskCategoryModel> tenantTaskCategories, IList<TaskTypeModel> taskTypes, IList<TaskRewardTypeModel> taskRewardTypes)
        {
            var taskRewardResponseDto = new FindTaskRewardResponseDto();
            foreach (var item in taskRewardDtoList)
            {
                var task = tasks.FirstOrDefault(t => t.TaskId == item.TaskId);
                if (task == null)
                {
                    continue;
                }

                var taskDetail = taskDetails?.FirstOrDefault(td => td.TaskId == item.TaskId);
                if (taskDetail == null)
                {
                    continue;
                }
                var termsOfService = termsOfServices.FirstOrDefault(tos => tos.TermsOfServiceId == taskDetail?.TermsOfServiceId);
                var tenantTaskCategory = tenantTaskCategories.FirstOrDefault(ttc => ttc.TaskCategoryId == task.TaskCategoryId);
                var taskType = taskTypes.FirstOrDefault(tt => tt.TaskTypeId == task.TaskTypeId);
                var taskRewardType = taskRewardTypes.FirstOrDefault(trt => trt.RewardTypeId == item.RewardTypeId);

                var taskRewardDetailDto = new TaskRewardDetailDto()
                {
                    TaskReward = item,
                    Task = _mapper.Map<TaskDto>(task),
                    TaskDetail = _mapper.Map<TaskDetailDto>(taskDetail),
                    TermsOfService = _mapper.Map<TermsOfServiceDto>(termsOfService),
                    TenantTaskCategory = _mapper.Map<TenantTaskCategoryDto>(tenantTaskCategory),
                    TaskType = _mapper.Map<TaskTypeDto>(taskType),
                    RewardTypeName = taskRewardType?.RewardTypeName
                };

                taskRewardResponseDto.TaskRewardDetails.Add(taskRewardDetailDto);
            }

            return taskRewardResponseDto;
        }

        private List<TaskRewardDetailDto> FilterValidTaskRewards(List<TaskRewardDetailDto> taskRewardDtoList)
        {
            var validTaskRewards = new List<TaskRewardDetailDto>();
            DateTime tsNow = DateTime.UtcNow;
            foreach (var item in taskRewardDtoList)
            {
                if (tsNow < item?.TaskReward?.ValidStartTs || tsNow > item?.TaskReward?.Expiry)
                {
                    continue;
                }
                validTaskRewards.Add(item);
            }
            return validTaskRewards;
        }

        /// <summary>
        /// Returns one Task Reward with all details of the Task matching the given TaskRewardCode
        /// </summary>
        /// <param name="taskRewardRequestDto"></param>
        /// <returns></returns>
        public async Task<GetTaskRewardByCodeResponseDto> GetTaskRewardByCode(GetTaskRewardByCodeRequestDto taskRewardRequestDto)
        {
            const string methodName = nameof(GetTaskRewardByCode);
            var taskRewardResponseDto = new GetTaskRewardByCodeResponseDto();
            try
            {
                //populating TaskReward table data
                var taskRewardModel = await _taskRewardRepo.FindOneAsync(x => x.TaskRewardCode == taskRewardRequestDto.TaskRewardCode && x.DeleteNbr == 0);
                if (taskRewardModel == null)
                {
                    _taskRewardLogger.LogError("{className}.{methodName}: Task not found for given for TaskRewardCode: {TaskRewardCode},Error Code:{errorCode}", className, methodName, taskRewardRequestDto.TaskRewardCode, StatusCodes.Status404NotFound);

                    return new GetTaskRewardByCodeResponseDto();
                }
                _taskRewardLogger.LogInformation("{className}.{methodName}: Successfully Retrieved  TaskReward Table  Data From GetTaskRewardByCode API For  TaskRewardCode: {TaskRewardCode}", className, methodName, taskRewardRequestDto.TaskRewardCode);

                var taskRewardDto = _mapper.Map<TaskRewardDto>(taskRewardModel);

                var taskData = await _taskRepo.FindOneAsync(x => x.TaskId == taskRewardDto.TaskId && x.DeleteNbr == 0);
                var taskDto = _mapper.Map<TaskDto>(taskData);
                var requestedLanguageCode = string.IsNullOrWhiteSpace(taskRewardRequestDto?.LanguageCode) ? Constant.LanguageCode.ToLower() : taskRewardRequestDto?.LanguageCode?.ToLower();
                var taskDetail = await _taskDetailRepo.FindOneAsync(x => x.TaskId == taskDto.TaskId && x.TenantCode == taskRewardModel.TenantCode &&
                    x.LanguageCode != null && x.LanguageCode.ToLower() == requestedLanguageCode && x.DeleteNbr == 0);
                if (taskDetail == null && requestedLanguageCode != Constant.LanguageCode.ToLower())
                {
                    taskDetail = await _taskDetailRepo.FindOneAsync(x => x.TaskId == taskDto.TaskId && x.TenantCode == taskRewardModel.TenantCode &&
                    x.LanguageCode != null && x.LanguageCode.ToLower() == Constant.LanguageCode.ToLower() && x.DeleteNbr == 0);
                }
                var taskDetailDto = _mapper.Map<TaskDetailDto>(taskDetail);
                var termsOfServiceId = taskDetailDto?.TermsOfServiceId ?? 0;
                var termsOfService = await _termsOfServiceRepo.FindOneAsync(x => x.TermsOfServiceId == termsOfServiceId && x.DeleteNbr == 0);
                var termsOfServiceDto = _mapper.Map<TermsOfServiceDto>(termsOfService);

                var tenantTaskCategory = await _tenantCategoryRepo.FindOneAsync(x => x.TenantTaskCategoryId == taskDto.TaskCategoryId
                        && x.TenantCode == taskRewardDto.TenantCode && x.DeleteNbr == 0);
                var tenantTaskCategoryDto = _mapper.Map<TenantTaskCategoryDto>(tenantTaskCategory);

                var taskRewardType = await _taskRewardTypeRepo.FindOneAsync(x => x.RewardTypeId == taskRewardDto.RewardTypeId && x.DeleteNbr == 0);

                var taskRewardDetailDto = new TaskRewardDetailDto()
                {
                    TaskReward = taskRewardDto,
                    Task = taskDto,
                    TaskDetail = taskDetailDto,
                    TermsOfService = termsOfServiceDto,
                    TenantTaskCategory = tenantTaskCategoryDto,
                    RewardTypeName = taskRewardType?.RewardTypeName
                };
                taskRewardResponseDto.TaskRewardDetail = taskRewardDetailDto;

                _taskRewardLogger.LogInformation("{className}.{methodName}: successfully retrieved data from  GetTaskRewardByCode API for TaskRewardCode: {TaskRewardCode}", className, methodName, taskRewardRequestDto.TaskRewardCode);
                return taskRewardResponseDto;
            }

            catch (Exception ex)
            {
                _taskRewardLogger.LogError(ex, "{className}.{methodName}: ERROR Msg:{msg}, Error Code:{errorCode}", className, methodName, ex.Message, StatusCodes.Status500InternalServerError);
                return new GetTaskRewardByCodeResponseDto();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rewardTypeRequestDto"></param>
        /// <returns></returns>
        public async Task<RewardTypeResponseDto> RewardType(RewardTypeRequestDto rewardTypeRequestDto)
        {
            const string methodName = nameof(RewardType);
            try
            {
                var taskId = rewardTypeRequestDto.TaskId;
                if (taskId <= 0)
                {
                    if (rewardTypeRequestDto.TaskCode != null)
                    {
                        var taskModel = await _taskRepo.FindOneAsync(x => x.TaskCode == rewardTypeRequestDto.TaskCode && x.DeleteNbr == 0);
                        if (taskModel == null)
                        {
                            _taskRewardLogger.LogError("{className}.{methodName}: Task not found for given for TaskCode: {TaskCode}, Error Code:{errorCode}", className, methodName, rewardTypeRequestDto.TaskCode, StatusCodes.Status404NotFound);
                            return new RewardTypeResponseDto();
                        }
                        taskId = taskModel.TaskId;
                    }
                    else
                    {
                        _taskRewardLogger.LogError("{className}.{methodName}: Needs either valid TaskId or TaskCode, TenantCode: {TenantCode},Error Code:{errorCode}", className, methodName, rewardTypeRequestDto.TenantCode, StatusCodes.Status404NotFound);
                        return new RewardTypeResponseDto();
                    }
                }

                // have valid taskId at this point

                var taskRewardModel = await _taskRewardRepo.FindOneAsync(x => x.TaskId == taskId &&
                    x.TenantCode == rewardTypeRequestDto.TenantCode && x.DeleteNbr == 0);

                if (taskRewardModel == null)
                {
                    _taskRewardLogger.LogError("{className}.{methodName}: RewardType not found for given for TaskId, TenantCode: {TaskId} {TenantCode},Error Code:{errorCode}", className, methodName, taskId, rewardTypeRequestDto.TenantCode, StatusCodes.Status404NotFound);
                    return new RewardTypeResponseDto();
                }


                var taskRewardTypeModel = await _taskRewardTypeRepo.FindOneAsync(x => x.RewardTypeId == taskRewardModel.RewardTypeId && x.DeleteNbr == 0);

                var response = new RewardTypeResponseDto()
                {
                    RewardTypeDto = new TaskRewardTypeDto()
                    {
                        RewardTypeId = taskRewardTypeModel.RewardTypeId,
                        RewardTypeName = taskRewardTypeModel.RewardTypeName,
                        RewardTypeDescription = taskRewardTypeModel.RewardTypeDescription,
                    }
                };
                _taskRewardLogger.LogInformation("{className}.{methodName}: successfully retrieved data from  RewardType API for TaskId, TenantCode: {TaskId} {TenantCode}", className, methodName, rewardTypeRequestDto.TaskId, rewardTypeRequestDto.TenantCode);
                return response;
            }
            catch (Exception ex)
            {
                _taskRewardLogger.LogError(ex, "{className}.{methodName}: ERROR Msg:{msg}, Error Code:{errorCode}", className, methodName, ex.Message, StatusCodes.Status500InternalServerError);
                return new RewardTypeResponseDto();
            }
        }

        public async Task<RewardTypeResponseDto> RewardTypeCode(RewardTypeCodeRequestDto rewardTypeCodeRequestDto)
        {
            const string methodName = nameof(RewardTypeCode);
            try
            {
                var rewardTypeModel = await _taskRewardTypeRepo.FindOneAsync(x => x.RewardTypeCode == rewardTypeCodeRequestDto.RewardTypeCode && x.DeleteNbr == 0);
                if (rewardTypeModel == null || rewardTypeModel.RewardTypeCode != rewardTypeCodeRequestDto.RewardTypeCode)
                {
                    _taskRewardLogger.LogInformation("{className}.{methodName}: No data found for RewardTypeCode: {RewardTypeCode},Error Code:{errorCode}", className, methodName, rewardTypeCodeRequestDto.RewardTypeCode, StatusCodes.Status404NotFound);
                    return new RewardTypeResponseDto { ErrorCode = StatusCodes.Status404NotFound };
                }
                var rewardTypeCode = _mapper.Map<TaskRewardTypeModel>(rewardTypeModel);
                var response = new RewardTypeResponseDto()
                {
                    RewardTypeDto = new TaskRewardTypeDto()
                    {
                        RewardTypeId = rewardTypeCode.RewardTypeId,
                        RewardTypeName = rewardTypeCode.RewardTypeName,
                        RewardTypeDescription = rewardTypeCode.RewardTypeDescription,
                        RewardTypeCode = rewardTypeCode.RewardTypeCode,
                    }
                };
                _taskRewardLogger.LogInformation("{className}.{methodName}: successfully retrieved data from  RewardTypeCode API for, RewardTypeCode: {RewardTypeCode}", className, methodName, rewardTypeCodeRequestDto.RewardTypeCode);
                return response;
            }
            catch (Exception ex)
            {

                _taskRewardLogger.LogError(ex, "{className}.{methodName}: ERROR Msg:{msg}, Error Code:{errorCode}", className, methodName, ex.Message, StatusCodes.Status500InternalServerError);
                return new RewardTypeResponseDto();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="getTaskByTenantCodeRequestDto"></param>
        /// <returns></returns>
        public async Task<GetTaskByTenantCodeResponseDto> GetAllTaskByTenantCode(GetTaskByTenantCodeRequestDto getTaskByTenantCodeRequestDto)
        {
            const string methodName = nameof(GetAllTaskByTenantCode);
            try
            {
                // Step 1: Fetch consumer task list
                List<GetConsumerTaskResponseDto> consumerTasksListDto = await ConsumerTaskList(getTaskByTenantCodeRequestDto);

                // Step 2: Fetch TaskReward records
                var taskRewardList = await _taskRewardRepo.FindAsync(x => x.TenantCode == getTaskByTenantCodeRequestDto.TenantCode && x.DeleteNbr == 0);

                if (taskRewardList == null || taskRewardList.Count == 0)
                {
                    _taskRewardLogger.LogError("{ClassName}.{MethodName}: TaskRewardList not found for given TenantCode: {TenantCode}, Error Code:{ErrorCode}", className, methodName, getTaskByTenantCodeRequestDto.TenantCode, StatusCodes.Status404NotFound);
                    return new GetTaskByTenantCodeResponseDto()
                    {
                        ErrorCode = StatusCodes.Status404NotFound,
                        ErrorMessage = $"TaskRewardList not found for given TenantCode: {getTaskByTenantCodeRequestDto.TenantCode}"
                    };
                }

                _taskRewardLogger.LogInformation("{ClassName}.{MethodName}: Successfully Retrieved TaskReward for TenantCode: {TenantCode}", className, methodName, getTaskByTenantCodeRequestDto.TenantCode);
                var taskRewardResponseDto = new GetTaskByTenantCodeResponseDto();

                taskRewardResponseDto.AvailableTasks = _taskRewardRepo.GetTaskRewardDetailsList(getTaskByTenantCodeRequestDto.TenantCode ?? string.Empty, getTaskByTenantCodeRequestDto.LanguageCode ?? Constant.LanguageCode);
                taskRewardResponseDto.ConsumerTaskList.AddRange(consumerTasksListDto);

                _taskRewardLogger.LogInformation("{ClassName}.{MethodName}: Successfully retrieved data from GetAllTaskByTenantCode API for TenantCode: {TenantCode}", className, methodName, getTaskByTenantCodeRequestDto.TenantCode);

                return taskRewardResponseDto;
            }
            catch (Exception ex)
            {
                _taskRewardLogger.LogError(ex, "{ClassName}.{MethodName}: ERROR:{Msg}, Error Code:{ErrorCode}", className, methodName, ex.Message, StatusCodes.Status500InternalServerError);
                throw;
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="getTaskByTenantCodeRequestDto"></param>
        /// <returns></returns>
        private async Task<List<GetConsumerTaskResponseDto>> ConsumerTaskList(GetTaskByTenantCodeRequestDto getTaskByTenantCodeRequestDto)
        {
            var consumerTasks = await _consumerTaskRepo.FindAsync(x => x.ConsumerCode == getTaskByTenantCodeRequestDto.ConsumerCode && x.DeleteNbr == 0);

            var consumerTasksListDto = consumerTasks.Select(x => new GetConsumerTaskResponseDto
            {
                ConsumerCode = x.ConsumerCode,
                ConsumerTaskId = x.ConsumerTaskId,
                TenantCode = x.TenantCode,
                ParentConsumerTaskId = x.ParentConsumerTaskId,
                Notes = x.Notes,
                TaskId = x.TaskId,
                TaskStatus = x.TaskStatus,
                Progress = x.Progress,
                ProgressDetail = x.ProgressDetail,
                AutoEnrolled = x.AutoEnrolled,
                TaskCompleteTs = x.TaskCompleteTs,
                TaskStartTs = x.TaskStartTs,

            }).ToList();
            return consumerTasksListDto;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="taskRewardId"></param>
        /// <returns></returns>
        public async Task<PeriodDescriptorResponseDto> CurrentPeriodDescriptor(long taskRewardId)
        {
            const string methodName = nameof(CurrentPeriodDescriptor);
            try
            {
                var response = new PeriodDescriptorResponseDto();
                var periodDescriptor = new PeriodDescriptorDto();

                var taskReward = await _taskRewardRepo.FindOneAsync(x => x.TaskRewardId == taskRewardId && x.DeleteNbr == 0);
                if (taskReward == null)
                {
                    _taskRewardLogger.LogInformation("{className}.{methodName}: No data found for taskRewardId: {taskRewardId}, Error Code:{errorCode}", className, methodName, taskRewardId, StatusCodes.Status404NotFound);
                    return new PeriodDescriptorResponseDto() { ErrorCode = StatusCodes.Status404NotFound, ErrorMessage = "Data Not found" };
                }
                var taskRewardDto = _mapper.Map<TaskRewardDto>(taskReward);

                if (string.IsNullOrEmpty(taskReward.RecurrenceDefinitionJson) || taskReward.RecurrenceDefinitionJson == "{}" || !taskReward.IsRecurring)
                {
                    periodDescriptor.PeriodDescriptor = Constant.PeriodDescriptor;
                    response.PeriodDescriptorDtO.PeriodDescriptor = periodDescriptor.PeriodDescriptor;
                    return response;
                }
                var recurrenceDetails = JsonConvert.DeserializeObject<RecurringDto>(taskRewardDto?.RecurrenceDefinitionJson ?? string.Empty);
                if (recurrenceDetails == null || recurrenceDetails.periodic == null || recurrenceDetails.periodic.period == null)
                {
                    periodDescriptor.PeriodDescriptor = Constant.PeriodDescriptor;
                    response.PeriodDescriptorDtO.PeriodDescriptor = periodDescriptor.PeriodDescriptor;
                    return response;
                }

                DateTime currentDate = DateTime.UtcNow;
                if (recurrenceDetails != null && recurrenceDetails.periodic?.period == Constant.Month)
                {
                    var periodRestartDate = (int)recurrenceDetails.periodic.periodRestartDate;
                    var currentMonth = TaskHelper.GetCurrentMonthUtc(periodRestartDate);
                    response.PeriodDescriptorDtO.PeriodDescriptor = Constant.Month + "-" + currentMonth.ToString();
                }
                else if (recurrenceDetails != null && recurrenceDetails.periodic?.period == Constant.QuarterlyPeriod)
                {
                    var periodRestartDate = (int)recurrenceDetails.periodic.periodRestartDate;
                    var currentQuarter = TaskHelper.GetQuarter(currentDate, periodRestartDate);
                    response.PeriodDescriptorDtO.PeriodDescriptor = Constant.QuarterlyPeriod + "-" + currentQuarter.ToString();
                }
                _taskRewardLogger.LogInformation("{className}.{methodName}: successfully retrieved data from  Current Period Descriptor for Task Reward Id: {RewardId}", className, methodName, taskReward);
                return response;
            }
            catch (Exception ex)
            {
                _taskRewardLogger.LogError(ex, "{className}.{methodName}: ERROR Msg:{msg}", className, methodName, ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Creates task Reward
        /// </summary>
        /// <param name="createTaskRewardRequestDto"></param>
        /// <returns></returns>
        public async Task<BaseResponseDto> CreateTaskReward(CreateTaskRewardRequestDto createTaskRewardRequestDto)
        {
            const string methodName = nameof(CreateTaskReward);
            try
            {
                _taskRewardLogger.LogInformation("{ClassName}:{MethodName}: Fetching Task Reward started for TaskCode:{TaskCode}, Tenant Code: {TenantCode}",
                 className, methodName, createTaskRewardRequestDto.TaskCode, createTaskRewardRequestDto.TaskReward.TenantCode);
                var task = await _taskRepo.FindOneAsync(x => x.TaskCode == createTaskRewardRequestDto.TaskCode && x.DeleteNbr == 0);
                if (task == null)
                {
                    return new BaseResponseDto() { ErrorCode = StatusCodes.Status404NotFound, ErrorMessage = $"Task not found with Task Code: {createTaskRewardRequestDto.TaskCode}" };
                }
                var taskReward = await _taskRewardRepo.FindOneAsync(x => x.TenantCode == createTaskRewardRequestDto.TaskReward.TenantCode && x.TaskId == task.TaskId && x.DeleteNbr == 0);
                if (taskReward != null)
                {
                    return new BaseResponseDto() { ErrorCode = StatusCodes.Status409Conflict, ErrorMessage = $"Task Reward are already Existed with Tenant Code: {createTaskRewardRequestDto.TaskReward.TenantCode} and Task Code:{createTaskRewardRequestDto.TaskCode}" };
                }
                var taskRewardModel = _mapper.Map<TaskRewardModel>(createTaskRewardRequestDto.TaskReward);
                taskRewardModel.CreateTs = DateTime.UtcNow;
                taskRewardModel.DeleteNbr = 0;
                taskRewardModel.TaskId = task.TaskId;
                taskRewardModel.TaskRewardId = 0;
                await _taskRewardRepo.CreateAsync(taskRewardModel);
                _taskRewardLogger.LogInformation("{ClassName}:{MethodName}: Task Reward are created successfully, for Tenant Code: {TenantCode}", className, methodName, createTaskRewardRequestDto.TaskReward.TenantCode);
                return new BaseResponseDto();

            }
            catch (Exception ex)
            {
                _taskRewardLogger.LogError(ex, "{ClassName}:{MethodName}: Error Creating TaskReward for Tenant Code: {TenantCode}", className, methodName, createTaskRewardRequestDto.TaskReward.TenantCode);
                throw;
            }
        }

        /// <summary>
        /// Retrives the tasks and taskrewards with tenant code
        /// </summary>
        /// <param name="getTaskRewardRequestDto"></param>
        /// <returns>Returns List of taskrewards matching with input tenantcode and List of tasks matching taskid in taskrewards</returns>
        public async Task<GetTasksAndTaskRewardsResponseDto> GetTasksAndTaskRewards(GetTasksAndTaskRewardsRequestDto getTasksAndTaskRewardsRequestDto)
        {
            const string methodName = nameof(GetTasksAndTaskRewards);
            try
            {
                _taskRewardLogger.LogInformation("{ClassName}.{MethodName}: Started processing gettasks and taskrewards with TenantCode: {TenantCode}", className, methodName, getTasksAndTaskRewardsRequestDto.TenantCode);
                // Retrieving taskrewards
                var responseModels = await _taskRewardRepo.GetTasksAndTaskRewards(getTasksAndTaskRewardsRequestDto);
                if (responseModels == null || responseModels.Count <= 0)
                {
                    _taskRewardLogger.LogError("{ClassName}.{MethodName}: Task and Task rewards not found for given TenantCode: {TenantCode}, Error Code:{ErrorCode}", className, methodName, getTasksAndTaskRewardsRequestDto.TenantCode, StatusCodes.Status404NotFound);
                    return new GetTasksAndTaskRewardsResponseDto() { ErrorCode = StatusCodes.Status404NotFound };
                }

                var responseDtos = _mapper.Map<List<TaskAndTaskRewardDto>>(responseModels);

                _taskRewardLogger.LogInformation("{ClassName}.{MethodName}: Sucessfully retrieved tasks and taskrewards with TenantCode: {TenantCode}", className, methodName, getTasksAndTaskRewardsRequestDto.TenantCode);

                return new GetTasksAndTaskRewardsResponseDto() { taskAndTaskRewardDtos = responseDtos };
            }
            catch (Exception ex)
            {
                _taskRewardLogger.LogError(ex, "{ClassName}.{MethodName}: Error occured while processing gettasks and taskrewards with TenantCode: {TenantCode}, Error Code:{ErrorCode},ERROR:{Msg}", className, methodName, getTasksAndTaskRewardsRequestDto?.TenantCode, StatusCodes.Status500InternalServerError, ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Retrieves the list of task reward details for a given tenant code.
        /// </summary>
        /// <param name="tenantCode"></param>
        /// <returns></returns>
        public async Task<TaskRewardDetailsResponseDto> GetTaskRewardDetails(string tenantCode, string? taskExternalCode, string languageCode)
        {
            const string methodName = nameof(GetTaskRewardDetails); // Method name for logging

            try
            {
                // Log the start of processing
                _taskRewardLogger.LogInformation("{ClassName}.{MethodName}: Started processing task rewards with TenantCode: {TenantCode}", className, methodName, tenantCode);

                // Retrieve task reward details from the repository
                var result = await _taskRewardRepo.GetTaskRewardDetails(tenantCode, taskExternalCode, languageCode);

                // Check if the result is null or empty
                if (result == null || result.Count == 0)
                {
                    // Log and return an error if no data is found
                    _taskRewardLogger.LogError("{ClassName}.{MethodName}: No task reward details found for TenantCode: {TenantCode}. Error Code: {ErrorCode}",
                        className, methodName, tenantCode, StatusCodes.Status404NotFound);

                    return new TaskRewardDetailsResponseDto
                    {
                        ErrorCode = StatusCodes.Status404NotFound,
                        ErrorMessage = "No task reward details found for the provided tenant code."
                    };
                }

                // Map the result to a response DTO
                var taskRewardDetails = _mapper.Map<IList<TaskRewardDetailsDto>>(result);

                // Log the successful retrieval of task reward details
                _taskRewardLogger.LogInformation("{ClassName}.{MethodName}: Successfully retrieved task reward details for TenantCode: {TenantCode}",
                    className, methodName, tenantCode);

                // Return the mapped response
                return new TaskRewardDetailsResponseDto { TaskRewardDetails = taskRewardDetails };
            }
            catch (Exception ex)
            {
                // Log the exception and rethrow for further handling
                _taskRewardLogger.LogError(ex, "{ClassName}.{MethodName}: Error occurred while processing task rewards for TenantCode: {TenantCode}. Error Code: {ErrorCode}, Message: {Msg}",
                    className, methodName, tenantCode, StatusCodes.Status500InternalServerError, ex.Message);
                throw;
            }
        }
        /// <summary>
        /// Retrieves the task reward collection details based on the provided request.
        /// </summary>
        /// <param name="taskRewardCollectionRequestDto">Request DTO containing TenantCode and TaskRewardCode.</param>
        /// <returns>Returns TaskRewardCollectionResponseDto containing the task reward details.</returns>
        public async Task<TaskRewardCollectionResponseDto> GetTaskRewardCollection(TaskRewardCollectionRequestDto taskRewardCollectionRequestDto)
        {
            const string methodName = nameof(GetTaskRewardCollection);
            try
            {
                // Logging method entry with key identifiers
                _taskRewardLogger.LogInformation("{ClassName}.{MethodName}: Started processing with Tenant Code: {TenantCode} and TaskRewardCode:{TaskRewardCode}",
                    className, methodName, taskRewardCollectionRequestDto.TenantCode, taskRewardCollectionRequestDto.TaskRewardCode);

                string errorMessage = null;
                var taskRewardDetailDtos = new List<TaskRewardDetailDto>();
                var tenantCode = taskRewardCollectionRequestDto.TenantCode;
                // Fetching task-reward details from repository
                TaskRewardDto taskRewardDto = await GetTaskReward(taskRewardCollectionRequestDto);

                if (taskRewardDto == null)
                {
                    errorMessage = $"Task reward not found with TenantCode:{taskRewardCollectionRequestDto.TenantCode} and TaskRewardCode:{taskRewardCollectionRequestDto.TaskRewardCode}";
                    return LogAndReturnError(methodName, StatusCodes.Status404NotFound, errorMessage);
                }

                var rewardConfig = JsonConvert.DeserializeObject<TaskRewardConfigJson>(taskRewardDto.TaskRewardConfigJson ?? string.Empty);

                if (rewardConfig?.CollectionConfig == null)
                {
                    errorMessage = $"Invalid reward configuration for TenantCode:{taskRewardCollectionRequestDto.TenantCode} and TaskRewardCode:{taskRewardCollectionRequestDto.TaskRewardCode}";
                    return LogAndReturnError(methodName, StatusCodes.Status422UnprocessableEntity, errorMessage);
                }

                // Handling non-flattened tasks (returning parent task collection)
                if (!rewardConfig.CollectionConfig.FlattenTasks)
                {
                    TaskRewardDetailDto taskRewardDetailDto = _taskRewardRepo.GetTaskRewardDetailsList(
                    taskRewardCollectionRequestDto.TenantCode, taskRewardCollectionRequestDto.LanguageCode ?? Constant.LanguageCode, new List<long>() { taskRewardDto.TaskRewardId }).FirstOrDefault() ?? new TaskRewardDetailDto();
                    taskRewardDetailDtos.Add(taskRewardDetailDto);
                    var availableTaskRewardList = await _commonTaskRewardService.GetAvailableTasksAsync(taskRewardDetailDtos, taskRewardCollectionRequestDto);
                    return new TaskRewardCollectionResponseDto() { TaskRewards = availableTaskRewardList };
                }
                else
                {
                    // Retrieving collection of child task rewards
                    var taskRewardCollections = await _taskRewardCollectionRepo.FindAsync(x => x.ParentTaskRewardId == taskRewardDto.TaskRewardId && x.DeleteNbr == 0);
                    if (taskRewardCollections == null || taskRewardCollections.Count == 0)
                    {
                        errorMessage = $"Task reward collection not found for TaskRewardCode:{taskRewardCollectionRequestDto.TaskRewardCode} and ParentTaskRewardId:{taskRewardDto.TaskRewardId}";
                        return LogAndReturnError(methodName, StatusCodes.Status404NotFound, errorMessage);
                    }

                    taskRewardDetailDtos = _taskRewardRepo.GetTaskRewardDetailsList(
                        taskRewardCollectionRequestDto.TenantCode, taskRewardCollectionRequestDto.LanguageCode ?? Constant.LanguageCode, taskRewardCollections.Select(x => x.ChildTaskRewardId).ToList());
                }
                foreach (var taskRewardDetailDto in taskRewardDetailDtos)
                {
                    await SetTaskRewardTimingAndRecurrence(taskRewardDetailDto);
                }

                var availableTaskList = await _commonTaskRewardService.GetAvailableTasksAsync(taskRewardDetailDtos, taskRewardCollectionRequestDto);

                _taskRewardLogger.LogInformation("{ClassName}.{MethodName}: Successfully retrieved task reward details for TenantCode: {TenantCode}",
                    className, methodName, taskRewardCollectionRequestDto.TenantCode);
                return new TaskRewardCollectionResponseDto() { TaskRewards = availableTaskList };
            }
            catch (Exception ex)
            {
                _taskRewardLogger.LogError(ex, "{ClassName}.{MethodName}: Unexpected error occurred with Tenant Code: {TenantCode} and TaskRewardCode:{TaskRewardCode}",
                    className, methodName, taskRewardCollectionRequestDto.TenantCode, taskRewardCollectionRequestDto.TaskRewardCode);
                throw;
            }
        }

        private async System.Threading.Tasks.Task SetTaskRewardTimingAndRecurrence(TaskRewardDetailDto taskRewardDetailDto)
        {
            if (taskRewardDetailDto.TaskReward != null && taskRewardDetailDto.TaskReward.IsRecurring)
            {
                await _commonTaskRewardService.RecurrenceTaskProcess(taskRewardDetailDto);
            }
            if (taskRewardDetailDto!.TaskReward != null && !taskRewardDetailDto.TaskReward.IsRecurring)
            {
                taskRewardDetailDto.MinAllowedTaskCompleteTs = taskRewardDetailDto.TaskReward.ValidStartTs;
                taskRewardDetailDto.TaskReward.MaxAllowedTaskCompletionTs = DateTime.UtcNow;
                taskRewardDetailDto.ComputedTaskExpiryTs = taskRewardDetailDto.TaskReward.Expiry;
            }
        }

        /// <summary>
        /// Retrieves the list of task health rewards for a given tenant code.
        /// </summary>
        /// <param name="tenantCode"></param>
        /// <returns></returns>
        public async Task<IList<TaskRewardDto>> GetHealthTaskRewards(string tenantCode)
        {
            const string methodName = nameof(GetHealthTaskRewards);
            try
            {
                _taskRewardLogger.LogInformation("{ClassName}.{MethodName}: Started processing task rewards with TenantCode: {TenantCode}", className, methodName, tenantCode);

                var healthTask = new GetSelfReportTaskReward()
                {
                    TenantCode = tenantCode,
                    selfReport = false
                };

                var result = (await _taskRewardRepo.GetSelfReportTaskRewards(healthTask))
                             .Where(x => x.TaskCompletionCriteria!.CompletionCriteriaType == Constant.HealthCriteriaType && x.TaskCompletionCriteria.CompletionPeriodType == Constant.MonthlyPeriodType)
                             .ToList();

                _taskRewardLogger.LogInformation("{ClassName}.{MethodName}: Successfully retrieved task reward details for TenantCode: {TenantCode}", className, methodName, tenantCode);

                return _mapper.Map<IList<TaskRewardDto>>(result);
            }
            catch (Exception ex)
            {
                _taskRewardLogger.LogError(ex, "{ClassName}.{MethodName}: Error occurred while processing task rewards for TenantCode: {TenantCode}. Error Code: {ErrorCode}, Message: {Msg}",
                    className, methodName, tenantCode, StatusCodes.Status500InternalServerError, ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Logs an error and returns a standardized error response.
        /// </summary>
        private TaskRewardCollectionResponseDto LogAndReturnError(string methodName, int errorCode, string errorMessage)
        {
            _taskRewardLogger.LogError("{ClassName}.{MethodName}: ErrorCode:{ErrorCode}, Error:{ErrorMessage}", className, methodName, errorCode, errorMessage);
            return new TaskRewardCollectionResponseDto()
            {
                ErrorCode = errorCode,
                ErrorMessage = errorMessage
            };
        }


        private async Task<TaskRewardDto> GetTaskReward(TaskRewardCollectionRequestDto taskRewardCollectionRequestDto)
        {
            var taskReward = await _taskRewardRepo.FindOneAsync(x => x.TenantCode == taskRewardCollectionRequestDto.TenantCode
                                                                && x.TaskRewardCode == taskRewardCollectionRequestDto.TaskRewardCode && x.IsCollection && x.DeleteNbr == 0);
            var taskRewardDto = _mapper.Map<TaskRewardDto>(taskReward);
            return taskRewardDto;
        }


        /// <summary>
        /// Retrieves a collection of adventures and their corresponding task rewards 
        /// based on the provided request DTO.
        /// </summary>
        /// <param name="requestDto">The request containing tenant code and cohort-task mapping.</param>
        /// <returns>Returns a response DTO containing the collection of adventures and their task rewards.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the request DTO or required properties are null or empty.</exception>
        /// <exception cref="Exception">Thrown when an unexpected error occurs while retrieving data.</exception>
        /// <summary>
        /// Retrieves adventure and task collections based on tenant and cohort mappings.
        /// </summary>
        /// <param name="requestDto">The request containing tenant code, language, and cohort-task mappings.</param>
        /// <returns>AdventureTaskCollectionResponseDto with task rewards grouped by adventure.</returns>
        public async Task<AdventureTaskCollectionResponseDto> GetAdventuresAndTaskCollections(AdventureTaskCollectionRequestDto requestDto)
        {
            const string methodName = nameof(GetAdventuresAndTaskCollections);

            try
            {
                _taskRewardLogger.LogInformation("{ClassName}.{MethodName}: Start processing for tenant: {TenantCode}", className, methodName, requestDto.TenantCode);

                var taskRewardDetailList = _taskRewardRepo.GetTaskRewardDetailsList(requestDto.TenantCode, requestDto.LanguageCode ?? Constant.LanguageCode);

                if (taskRewardDetailList == null || taskRewardDetailList.Count == 0)
                {
                    _taskRewardLogger.LogWarning("{ClassName}.{MethodName}: No task rewards found for tenant: {TenantCode}", className, methodName, requestDto.TenantCode);
                    return new AdventureTaskCollectionResponseDto
                    {
                        ErrorCode = StatusCodes.Status404NotFound,
                        ErrorMessage = $"Task not found with TenantCode: {requestDto.TenantCode}"
                    };
                }

                // Create dictionary for faster lookup
                var taskRewardDetailDict = taskRewardDetailList
                    .Where(x => x.TaskReward != null)
                    .ToDictionary(x => x.TaskReward.TaskRewardCode, StringComparer.OrdinalIgnoreCase);

                var adventures = await _adventureRepo.GetAllAdventures(requestDto.TenantCode);
                var adventuresWithLanguageCode = _mapper.Map<List<AdventureDto>>(adventures);
                adventuresWithLanguageCode.ForEach(adventure =>
                {
                    if (adventure.CmsComponentCode != null &&
                        requestDto.LanguageComponentCodes.ContainsKey(adventure.CmsComponentCode))
                    {
                        adventure.LanguageCode = requestDto.LanguageComponentCodes[adventure.CmsComponentCode];
                    }
                });

                var adventureTaskCollections = new List<AdventureTaskCollectionDto>();

                foreach (var cohort in requestDto.CohortTaskMap)
                {
                    _taskRewardLogger.LogInformation("{ClassName}.{MethodName}: Processing cohort: {CohortKey}", className, methodName, cohort.Key);

                    var adventure = GetAdventureDto(adventuresWithLanguageCode, cohort.Key, requestDto.LanguageCode!);
                    if (adventure == null)
                    {
                        _taskRewardLogger.LogWarning("{ClassName}.{MethodName}: No adventure found for cohort key: {CohortKey}", className, methodName, cohort.Key);
                        continue;
                    }

                    foreach (var taskRewardCode in cohort.Value)
                    {
                        if (string.IsNullOrWhiteSpace(taskRewardCode))
                        {
                            _taskRewardLogger.LogWarning("{ClassName}.{MethodName}: Empty task reward code for cohort: {CohortKey}", className, methodName, cohort.Key);
                            continue;
                        }

                        if (!taskRewardDetailDict.TryGetValue(taskRewardCode, out var taskRewardDetailDto) || taskRewardDetailDto.TaskReward == null)
                        {
                            _taskRewardLogger.LogWarning("{ClassName}.{MethodName}: Task reward not found for code: {TaskRewardCode}", className, methodName, taskRewardCode);
                            continue;
                        }

                        var rewardConfig = JsonConvert.DeserializeObject<TaskRewardConfigJson>(taskRewardDetailDto.TaskReward.TaskRewardConfigJson ?? string.Empty);
                        if (rewardConfig?.CollectionConfig == null)
                        {
                            _taskRewardLogger.LogWarning("{ClassName}.{MethodName}: Invalid reward config for TaskRewardCode: {TaskRewardCode}", className, methodName, taskRewardCode);
                            continue;
                        }

                        var taskRewardDetailDtos = new List<TaskRewardDetailDto>();

                        if (!rewardConfig.CollectionConfig.FlattenTasks)
                        {
                            taskRewardDetailDtos.Add(taskRewardDetailDto);
                        }
                        else
                        {
                            var taskRewardCollections = await _taskRewardCollectionRepo.FindAsync(x =>
                                x.ParentTaskRewardId == taskRewardDetailDto.TaskReward.TaskRewardId && x.DeleteNbr == 0);

                            if (taskRewardCollections == null || !taskRewardCollections.Any())
                            {
                                _taskRewardLogger.LogWarning("{ClassName}.{MethodName}: No child task rewards for TaskRewardCode: {TaskRewardCode}", className, methodName, taskRewardCode);
                                continue;
                            }

                            var taskRewardIds = taskRewardCollections.Select(x => x.ChildTaskRewardId).ToHashSet();

                            taskRewardDetailDtos = taskRewardDetailList
                                .Where(x => x.TaskReward != null && taskRewardIds.Contains(x.TaskReward.TaskRewardId))
                                .ToList();
                        }
                        foreach (var dto in taskRewardDetailDtos)
                        {
                            await SetTaskRewardTimingAndRecurrence(dto);
                        }
                        var collectionRequestDto = new TaskRewardCollectionRequestDto
                        {
                            ConsumerCode = requestDto.ConsumerCode,
                            LanguageCode = requestDto.LanguageCode,
                            TenantCode = requestDto.TenantCode,
                            TaskRewardCode = taskRewardCode
                        };

                        var availableTaskList = await _commonTaskRewardService.GetAvailableTasksAsync(taskRewardDetailDtos, collectionRequestDto);

                        adventureTaskCollections.Add(new AdventureTaskCollectionDto
                        {
                            Adventure = adventure,
                            TaskRewards = availableTaskList
                        });
                    }
                }

                _taskRewardLogger.LogInformation("{ClassName}.{MethodName}: Successfully built {Count} adventure-task collections.", className, methodName, adventureTaskCollections.Count);

                return new AdventureTaskCollectionResponseDto
                {
                    AdventureTaskRewards = adventureTaskCollections
                };
            }
            catch (Exception ex)
            {
                _taskRewardLogger.LogError(ex, "{ClassName}.{MethodName}: Error processing task collections for tenant: {TenantCode}. Message: {Message}",
                    className, methodName, requestDto.TenantCode, ex.Message);
                throw;
            }
        }



        private AdventureDto? GetAdventureDto(IList<AdventureDto> tenantAdventures, string cohortName, string languageCode)
        {
            AdventureDto? adventureDto = GetMatchingAdventure(tenantAdventures, cohortName, languageCode);
            if (adventureDto == null)
            {
                adventureDto = GetMatchingAdventure(tenantAdventures, cohortName, Constant.LanguageCode);
            }

            return adventureDto;

            AdventureDto? GetMatchingAdventure(IList<AdventureDto> tenantAdventures, string cohortName, string languageCode)
            {
                return tenantAdventures
                                .Select(adventure => new { Adventure = adventure, AdventureConfig = JsonConvert.DeserializeObject<AdventureConfig>(adventure.AdventureConfigJson) })
                                .Where(x => x.AdventureConfig != null
                                            && x.AdventureConfig.Cohorts.Contains(cohortName)
                                            && (string.IsNullOrEmpty(x.Adventure.LanguageCode) || 
                                                string.Equals(x.Adventure.LanguageCode, languageCode, StringComparison.OrdinalIgnoreCase)))
                                .Select(x => x.Adventure)
                                .FirstOrDefault();
            }
        }
    }

}


