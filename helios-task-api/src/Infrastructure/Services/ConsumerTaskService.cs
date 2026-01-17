using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using SunnyRewards.Helios.Common.Core.Domain;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.Common.Core.Extensions;
using SunnyRewards.Helios.Common.Core.Helpers.Interfaces;
using SunnyRewards.Helios.Common.Core.Services;
using SunnyRewards.Helios.Task.Core.Domain.Constants;
using SunnyRewards.Helios.Task.Core.Domain.Dtos;
using SunnyRewards.Helios.Task.Core.Domain.Dtos.Enums;
using SunnyRewards.Helios.Task.Core.Domain.Models;
using SunnyRewards.Helios.Task.Infrastructure.Helpers;
using SunnyRewards.Helios.Task.Infrastructure.Helpers.Interface;
using SunnyRewards.Helios.Task.Infrastructure.Repositories.Interfaces;
using SunnyRewards.Helios.Task.Infrastructure.Services.Interface;
using System.Net;
using ISession = NHibernate.ISession;

namespace SunnyRewards.Helios.Task.Infrastructure.Services
{
    public class ConsumerTaskService : BaseService, IConsumerTaskService
    {
        private const string CONSUMER_TASK_EVIDENCE_FOLDER = "consumer_task_evidence";

        private readonly ILogger<ConsumerTaskService> _consumerTaskServiceLogger;
        private readonly IMapper _mapper;
        private readonly ISession _session;
        private readonly IConsumerTaskRepo _consumerTaskRepo;
        private readonly ITaskRepo _taskRepo;
        private readonly ITaskDetailRepo _taskDetailRepo;
        private readonly ITaskRewardRepo _taskRewardRepo;
        private readonly ITermsOfServiceRepo _termsOfServiceRepo;
        private readonly ITaskRewardService _taskRewardService;
        private readonly ITenantTaskCategoryRepo _tenantTaskCategoryRepo;
        private readonly ITaskTypeRepo _taskTypeRepo;
        private readonly ISubtaskService _subTaskService;
        private readonly ITaskRewardTypeRepo _taskRewardTypeRepo;
        private readonly IConfiguration _configuration;
        private readonly IVault _vault;
        private readonly IFileHelper _fileHelper;
        private readonly ITaskCommonHelper _taskCommonHelper;
        private readonly IHeliosEventPublisher<ConsumerTaskEventDto> _heliosEventPublisher;
        private readonly ICommonTaskRewardService _commonTaskRewardService;

        private readonly JsonSerializerSettings _settings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            Formatting = Formatting.Indented
        };

        const string className = nameof(ConsumerTaskService);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="consumerTaskServiceLogger"></param>
        /// <param name="mapper"></param>
        /// <param name="session"></param>
        /// <param name="consumerTaskRepo"></param>
        /// <param name="taskRepo"></param>
        /// <param name="taskDetailRepo"></param>
        /// <param name="taskRewardRepo"></param>
        /// <param name="termsOfServiceRepo"></param>
        /// <param name="taskRewardService"></param>
        /// <param name="tenantCategoryRepo"></param>
        /// <param name="taskTypeRepo"></param>
        /// <param name="subTaskService"></param>
        /// <param name="taskRewardTypeRepo"></param>
        /// <param name="configuration"></param>
        /// <param name="vault"></param>
        public ConsumerTaskService(
            ILogger<ConsumerTaskService> consumerTaskServiceLogger,
            IMapper mapper,
            ISession session,
            IConsumerTaskRepo consumerTaskRepo,
            ITaskRepo taskRepo,
            ITaskDetailRepo taskDetailRepo,
            ITaskRewardRepo taskRewardRepo,
            ITermsOfServiceRepo termsOfServiceRepo,
            ITaskRewardService taskRewardService,
            ITenantTaskCategoryRepo tenantCategoryRepo,
            ITaskTypeRepo taskTypeRepo,
            ISubtaskService subTaskService,
            ITaskRewardTypeRepo taskRewardTypeRepo,
             IFileHelper fileHelper,
            IConfiguration configuration,
            IVault vault, ITaskCommonHelper taskCommonHelper,
            ICommonTaskRewardService commonTaskRewardService,
          IHeliosEventPublisher<ConsumerTaskEventDto> heliosEventPublisher
            )
        {
            _consumerTaskServiceLogger = consumerTaskServiceLogger;
            _mapper = mapper;
            _session = session;
            _consumerTaskRepo = consumerTaskRepo;
            _taskRepo = taskRepo;
            _taskDetailRepo = taskDetailRepo;
            _taskRewardRepo = taskRewardRepo;
            _termsOfServiceRepo = termsOfServiceRepo;
            _taskRewardService = taskRewardService;
            _tenantTaskCategoryRepo = tenantCategoryRepo;
            _taskTypeRepo = taskTypeRepo;
            _subTaskService = subTaskService;
            _taskRewardTypeRepo = taskRewardTypeRepo;
            _configuration = configuration;
            _vault = vault;
            _fileHelper = fileHelper;
            _taskCommonHelper = taskCommonHelper;
            _commonTaskRewardService = commonTaskRewardService;
            _heliosEventPublisher = heliosEventPublisher;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="findConsumerTasksByIdRequestDto"></param>
        /// <returns></returns>
        public async Task<FindConsumerTasksByIdResponseDto> GetConsumerTask(FindConsumerTasksByIdRequestDto findConsumerTasksByIdRequestDto)
        {
            const string methodName = nameof(GetAllConsumerTask);
            DateTime tsNow = DateTime.UtcNow;
            TaskModel? taskData = null;
            if (findConsumerTasksByIdRequestDto.TaskId > 0)
            {
                taskData = await _taskRepo.FindOneAsync(x => x.TaskId == findConsumerTasksByIdRequestDto.TaskId && x.DeleteNbr == 0);
                if (taskData == null)
                {
                    _consumerTaskServiceLogger.LogError("{className}.{methodName}: Task Data is Null. For Task Id: {taskId}, Error Code:{errorCode}", className, methodName, findConsumerTasksByIdRequestDto.TaskId, StatusCodes.Status404NotFound);
                    return new FindConsumerTasksByIdResponseDto();
                }
                findConsumerTasksByIdRequestDto.TaskId = taskData.TaskId;
            }
            else if (!string.IsNullOrEmpty(findConsumerTasksByIdRequestDto.TaskCode))
            {
                taskData = await _taskRepo.FindOneAsync(x => x.TaskCode == findConsumerTasksByIdRequestDto.TaskCode && x.DeleteNbr == 0);
                if (taskData == null)
                {
                    _consumerTaskServiceLogger.LogError("{className}.{methodName}: Task Data is Null. For Task Code: {taskCode}, Error Code:{errorCode}", className, methodName, findConsumerTasksByIdRequestDto.TaskCode, StatusCodes.Status404NotFound);
                    return new FindConsumerTasksByIdResponseDto();
                }
                findConsumerTasksByIdRequestDto.TaskId = taskData.TaskId;
            }
            else if (!string.IsNullOrEmpty(findConsumerTasksByIdRequestDto.TaskExternalCode) && !string.IsNullOrEmpty(findConsumerTasksByIdRequestDto.TenantCode))
            {
                var taskRewardModel = await _taskRewardRepo.FindOneAsync(x => x.TaskExternalCode == findConsumerTasksByIdRequestDto.TaskExternalCode
                && x.DeleteNbr == 0 && x.TenantCode == findConsumerTasksByIdRequestDto.TenantCode);

                if (taskRewardModel == null)
                {
                    _consumerTaskServiceLogger.LogError("{className}.{methodName}: Task Reward is Null. For Tenant Code: {tenant}, Error Code:{errorCode}", className, methodName, findConsumerTasksByIdRequestDto.TenantCode, StatusCodes.Status404NotFound);
                    return new FindConsumerTasksByIdResponseDto();
                }
                if (tsNow < taskRewardModel?.ValidStartTs || tsNow > taskRewardModel?.Expiry)
                {
                    taskData = await _taskRepo.FindOneAsync(x => x.TaskId == taskRewardModel.TaskId && x.DeleteNbr == 0);
                    if (taskData == null)
                    {
                        _consumerTaskServiceLogger.LogError("{className}.{methodName}: Task Data is Null. For Task Id: {taskId}, Error Code:{errorCode}", className, methodName, taskRewardModel.TaskId, StatusCodes.Status404NotFound);
                        return new FindConsumerTasksByIdResponseDto();
                    }
                    findConsumerTasksByIdRequestDto.TaskId = taskData.TaskId;

                }
            }
            ConsumerTaskModel? consumerTask = null;
            if (string.IsNullOrEmpty(findConsumerTasksByIdRequestDto.TaskStatus))
            {
                var consumerTasks = await _consumerTaskRepo.FindAsync(x => x.ConsumerCode == findConsumerTasksByIdRequestDto.ConsumerCode &&
                    x.TaskId == findConsumerTasksByIdRequestDto.TaskId &&
                    x.DeleteNbr == 0);
                consumerTask = consumerTasks.OrderByDescending(x => x.ConsumerTaskId).FirstOrDefault();
            }
            else
            {
                var consumerTasks = await _consumerTaskRepo.FindAsync(x => x.ConsumerCode == findConsumerTasksByIdRequestDto.ConsumerCode &&
                    x.TaskId == findConsumerTasksByIdRequestDto.TaskId &&
                    x.TaskStatus.ToLower() == findConsumerTasksByIdRequestDto.TaskStatus.ToLower() &&
                    x.DeleteNbr == 0);
                consumerTask = consumerTasks.OrderByDescending(x => x.ConsumerTaskId).FirstOrDefault();
            }

            if (consumerTask == null)
            {
                _consumerTaskServiceLogger.LogError("{className}.{methodName}: Consumer Task is Null For Consumer Code:{consumerCode}, Error Code:{errorCode}", className, methodName, findConsumerTasksByIdRequestDto.ConsumerCode, StatusCodes.Status404NotFound);
                return new FindConsumerTasksByIdResponseDto();
            }

            var consumerTaskDto = _mapper.Map<ConsumerTaskDto>(consumerTask);

            var taskDto = _mapper.Map<TaskDto>(taskData);
            var requestedLanguageCode = string.IsNullOrWhiteSpace(findConsumerTasksByIdRequestDto?.LanguageCode) ? Constant.LanguageCode.ToLower() : findConsumerTasksByIdRequestDto?.LanguageCode?.ToLower();

            var taskDetail = await _taskDetailRepo.FindOneAsync(x => x.TaskId == taskDto.TaskId &&
                x.TenantCode == consumerTask.TenantCode &&
                x.LanguageCode != null && x.LanguageCode.ToLower() == requestedLanguageCode && x.DeleteNbr == 0);

            if (taskDetail == null && requestedLanguageCode != Constant.LanguageCode.ToLower())
            {
                taskDetail = await _taskDetailRepo.FindOneAsync(x => x.TaskId == taskDto.TaskId &&
                x.TenantCode == consumerTask.TenantCode &&
                x.LanguageCode != null && x.LanguageCode.ToLower() == Constant.LanguageCode.ToLower() && x.DeleteNbr == 0);
            }

            var taskDetailDto = _mapper.Map<TaskDetailDto>(taskDetail);

            var taskReward = await _taskRewardRepo.FindOneAsync(x => x.TaskId == taskDto.TaskId && x.TenantCode == consumerTask.TenantCode && x.DeleteNbr == 0);

            var taskRewardDto = _mapper.Map<TaskRewardDto>(taskReward);
            var termsOfServiceId = taskDetailDto?.TermsOfServiceId ?? 0;
            var termsOfService = await _termsOfServiceRepo.FindOneAsync(x => x.TermsOfServiceId == termsOfServiceId && x.DeleteNbr == 0);
            var termsOfServiceDto = _mapper.Map<TermsOfServiceDto>(termsOfService);

            var tenantTaskCategory = await _tenantTaskCategoryRepo.FindOneAsync(x => x.TenantTaskCategoryId == taskDto.TaskCategoryId
                        && x.TenantCode == taskRewardDto.TenantCode && x.DeleteNbr == 0);
            var tenantTaskCategoryDto = _mapper.Map<TenantTaskCategoryDto>(tenantTaskCategory);

            var tasktype = await _taskTypeRepo.FindOneAsync(x => x.TaskTypeId == taskDto.TaskTypeId && x.DeleteNbr == 0);
            var taskTypeDto = _mapper.Map<TaskTypeDto>(tasktype);

            var taskRewardType = await _taskRewardTypeRepo.FindOneAsync(x => x.RewardTypeId == taskRewardDto.RewardTypeId && x.DeleteNbr == 0);

            var taskRewardDetailDto = new TaskRewardDetailDto()
            {
                Task = taskDto,
                TaskDetail = taskDetailDto,
                TaskReward = taskRewardDto,
                TermsOfService = termsOfServiceDto,
                TenantTaskCategory = tenantTaskCategoryDto,
                TaskType = taskTypeDto,
                RewardTypeName = taskRewardType?.RewardTypeName,
                ConsumerTaskDto = consumerTaskDto
            };

            var findConsumerTasksByIdResponseDto = new FindConsumerTasksByIdResponseDto()
            {
                TaskRewardDetail = taskRewardDetailDto,
                ConsumerTask = consumerTaskDto,
            };

            _consumerTaskServiceLogger.LogInformation("{className}.{methodName}: successfully retrieved data from  GetConsumerTaskByTaskId API for TaskStatus: {TaskStatus}", className, methodName, findConsumerTasksByIdRequestDto.TaskStatus);
            return findConsumerTasksByIdResponseDto;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="consumerTaskRequestDto"></param>
        /// <returns></returns>
        public async Task<FindConsumerTaskResponseDto> GetConsumerTasks(FindConsumerTaskRequestDto consumerTaskRequestDto)
        {
            const string methodName = nameof(GetConsumerTasks);
            DateTime tsNow = DateTime.UtcNow;
            try
            {
                var consumerTaskList = new List<ConsumerTaskDto>();
                var taskRewardDetails = new List<TaskRewardDetailDto>();

                if (consumerTaskRequestDto.TaskStatus?.ToLower() == Constants.Completed.ToLower())
                {
                    var consumerTaskCompletedList = await _consumerTaskRepo.FindAsync(x => x.ConsumerCode == consumerTaskRequestDto.ConsumerCode
                        && x.TaskStatus != null && x.TaskStatus.ToLower() == Constants.Completed.ToLower() && x.DeleteNbr == 0);
                    consumerTaskList = _mapper.Map<IList<ConsumerTaskDto>>(consumerTaskCompletedList).ToList();
                }
                else if (consumerTaskRequestDto.TaskStatus?.ToLower() != Constants.Completed.ToLower())
                {
                    var consumerTaskPendingList = await _consumerTaskRepo.FindAsync(x => x.ConsumerCode == consumerTaskRequestDto.ConsumerCode
                        && x.TaskStatus != null && x.TaskStatus.ToLower() != Constants.Completed.ToLower() && x.DeleteNbr == 0);
                    consumerTaskList = _mapper.Map<IList<ConsumerTaskDto>>(consumerTaskPendingList).ToList();
                }

                string? tenantCode = consumerTaskList.Count > 0 ? consumerTaskList[0].TenantCode : null;

                foreach (var item in consumerTaskList)
                {
                    var taskData = await _taskRepo.FindOneAsync(x => x.TaskId == item.TaskId && x.IsSubtask != true && x.DeleteNbr == 0);

                    if (taskData == null)
                    {
                        continue;
                    }
                    var taskDto = _mapper.Map<TaskDto>(taskData);
                    var requestedLanguageCode = string.IsNullOrWhiteSpace(consumerTaskRequestDto?.LanguageCode) ? Constant.LanguageCode.ToLower() : consumerTaskRequestDto?.LanguageCode?.ToLower();
                    var taskDetail = await _taskDetailRepo.FindOneAsync(x => x.TaskId == taskDto.TaskId && x.TenantCode == tenantCode &&
                         x.LanguageCode != null && x.LanguageCode.ToLower() == requestedLanguageCode && x.DeleteNbr == 0);
                    if (taskDetail == null && requestedLanguageCode != Constant.LanguageCode.ToLower())
                    {
                        taskDetail = await _taskDetailRepo.FindOneAsync(x => x.TaskId == taskDto.TaskId && x.TenantCode == tenantCode &&
                            x.LanguageCode != null && x.LanguageCode.ToLower() == Constant.LanguageCode.ToLower() && x.DeleteNbr == 0);
                    }

                    var taskDetailDto = _mapper.Map<TaskDetailDto>(taskDetail);

                    TaskRewardModel? taskReward = null;

                    taskReward = await _taskRewardRepo.FindOneAsync(x => x.TaskId == taskDto.TaskId && x.TenantCode == tenantCode && x.DeleteNbr == 0);

                    if (taskReward == null || taskDetail == null)
                    {
                        continue;
                    }

                    if (consumerTaskRequestDto.TaskStatus?.ToLower() == Constants.InProgress.ToLower())
                    {
                        if (tsNow < taskReward?.ValidStartTs || tsNow > taskReward?.Expiry)
                        {
                            continue;
                        }
                    }
                    var taskRewardDto = _mapper.Map<TaskRewardDto>(taskReward);

                    var termsOfService = await _termsOfServiceRepo.FindOneAsync(x => x.TermsOfServiceId == taskDetailDto.TermsOfServiceId && x.DeleteNbr == 0);
                    var termsOfServiceDto = _mapper.Map<TermsOfServiceDto>(termsOfService);

                    var tenantTaskCategory = await _tenantTaskCategoryRepo.FindOneAsync(x => x.TaskCategoryId == taskDto.TaskCategoryId
                        && x.TenantCode == taskRewardDto.TenantCode && x.DeleteNbr == 0);
                    var tenantTaskCategoryDto = _mapper.Map<TenantTaskCategoryDto>(tenantTaskCategory);

                    var tasktype = await _taskTypeRepo.FindOneAsync(x => x.TaskTypeId == taskDto.TaskTypeId && x.DeleteNbr == 0);
                    var taskTypeDto = _mapper.Map<TaskTypeDto>(tasktype);

                    var taskRewardType = await _taskRewardTypeRepo.FindOneAsync(x => x.RewardTypeId == taskRewardDto.RewardTypeId && x.DeleteNbr == 0);

                    var taskRewardDetailDto = new TaskRewardDetailDto()
                    {
                        Task = taskDto,
                        TaskDetail = taskDetailDto,
                        TaskReward = taskRewardDto,
                        TermsOfService = termsOfServiceDto,
                        TenantTaskCategory = tenantTaskCategoryDto,
                        TaskType = taskTypeDto,
                        RewardTypeName = taskRewardType?.RewardTypeName
                    };

                    if (taskRewardDetailDto!.TaskReward != null && taskRewardDetailDto.TaskReward.IsRecurring)
                    {
                        await _commonTaskRewardService.RecurrenceTaskProcess(taskRewardDetailDto);

                    }
                    if (taskRewardDetailDto!.TaskReward != null && !taskRewardDetailDto.TaskReward.IsRecurring)
                    {
                        taskRewardDetailDto.MinAllowedTaskCompleteTs = taskRewardDetailDto.TaskReward.ValidStartTs;
                        taskRewardDetailDto.TaskReward.MaxAllowedTaskCompletionTs = DateTime.UtcNow;
                        taskRewardDetailDto.ComputedTaskExpiryTs = taskRewardDetailDto.TaskReward.Expiry;
                    }
                    taskRewardDetails.Add(taskRewardDetailDto);
                }

                var consumerTaskResponseDto = new FindConsumerTaskResponseDto()
                {
                    ConsumerTask = consumerTaskList,
                    TaskRewardDetail = taskRewardDetails.OrderByDescending(x => x.TaskDetail?.UpdateTs).ToList()
                };

                _consumerTaskServiceLogger.LogInformation("{className}.{methodName}: successfully retrieved data from  FindConsumerTasks for Consumer Code:{consumerCode} with TaskStatus: {TaskStatus}", className, methodName, consumerTaskRequestDto.ConsumerCode, consumerTaskRequestDto.TaskStatus);

                return consumerTaskResponseDto;
            }

            catch (Exception ex)
            {
                _consumerTaskServiceLogger.LogError(ex, "{className}.{methodName}: - ERROR Msg:{msg}", className, methodName, ex.Message);
                throw;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="consumerTaskDto"></param>
        /// <returns></returns>
        public async Task<ConsumerTaskResponseUpdateDto> CreateConsumerTasks(ConsumerTaskDto consumerTaskDto)
        {
            const string methodName = nameof(CreateConsumerTasks);
            ConsumerTaskResponseUpdateDto response = new ConsumerTaskResponseUpdateDto();
            consumerTaskDto.TaskStartTs = consumerTaskDto.CreateTs = DateTime.UtcNow;
            var existingConsumerTask = await _consumerTaskRepo.GetConsumerTasksWithRewards(consumerTaskDto.TenantCode, consumerTaskDto.ConsumerCode, consumerTaskDto.TaskId);
            var existingtaskRewardDto = new TaskRewardDto();
            if (existingConsumerTask != null)
            {
                existingtaskRewardDto = _mapper.Map<TaskRewardDto>(existingConsumerTask.TaskReward);

            }
            // check if task is valid
            var taskRewardDetails = await _taskRewardRepo.FindAsync(x => x.TaskId == consumerTaskDto.TaskId && x.TenantCode == consumerTaskDto.TenantCode && x.DeleteNbr == 0);
            if (taskRewardDetails == null || taskRewardDetails.Count == 0)
            {
                return CreateErrorResponse(HttpStatusCode.Conflict, $"Task reward not found");
            }
            var taskRewardDtoList = taskRewardDetails!
    .Select(x => new TaskRewardDetailDto { TaskReward = _mapper.Map<TaskRewardDto>(x) })
    .ToList();

            var validTaskRewards = taskRewardDtoList;
            if (!consumerTaskDto.SkipValidation)
            {
                validTaskRewards = FilterValidTaskRewards(taskRewardDtoList);
            }


            if (validTaskRewards.Count == 0)
            {
                return CreateErrorResponse(HttpStatusCode.Conflict, $"Task dates are not valid");
            }



            var consumerTasks = await _consumerTaskRepo.FindAsync(x => x.TenantCode == consumerTaskDto.TenantCode && x.ConsumerCode == consumerTaskDto.ConsumerCode && x.TaskId == consumerTaskDto.TaskId && x.DeleteNbr == 0);
            var recurrenceDetails = JsonConvert.DeserializeObject<RecurringDto>(existingConsumerTask?.TaskReward?.RecurrenceDefinitionJson ?? string.Empty);

            // Check if there is an existing consumer task and if its task status is "Completed"
            // Also, verify if the task is recurring and exceeds the allowed number of occurrences
            if (existingConsumerTask != null && existingConsumerTask.ConsumerTask != null && existingConsumerTask.ConsumerTask.TaskStatus.Equals(Constants.Completed, StringComparison.CurrentCultureIgnoreCase) &&
                (existingConsumerTask.TaskReward.IsRecurring && existingConsumerTask.ConsumerTask.ParentConsumerTaskId == null && !TaskHelper.VerifyTaskValidOccurrences((int)consumerTaskDto.TaskId, consumerTasks, recurrenceDetails)))
            {
                _consumerTaskServiceLogger.LogError("{className}.{methodName}: Consumer task not in right state, Consumer Code:{consumerCode}, TaskId: {consumerTaTaskIdskId}, curr-state: {currState}, Error Code:{errorCode}", className, methodName, consumerTaskDto.ConsumerCode,
                                 consumerTaskDto.TaskId, consumerTaskDto.TaskStatus, HttpStatusCode.Conflict);
                return CreateErrorResponse(HttpStatusCode.Conflict, $"The maximum number of allowed occurrences ({recurrenceDetails?.periodic?.MaxOccurrences}) for this task within the {recurrenceDetails?.periodic?.period} recurrence period has been reached. Further completions will be available starting from the next period restart date.");
            }

            // Check if there are consumer tasks available and if the existing consumer task has a parent task
            // If so, fetch the parent task and verify its eligibility for completion
            else if (existingConsumerTask != null && existingConsumerTask.ConsumerTask != null && existingConsumerTask.ConsumerTask.TaskStatus.Equals(Constants.Completed, StringComparison.CurrentCultureIgnoreCase) &&
                    consumerTasks != null && !string.IsNullOrEmpty(existingConsumerTask.ConsumerTask.TenantCode) && existingConsumerTask.ConsumerTask.ParentConsumerTaskId != null)
            {
                // Fetch the parent task and its reward details
                var parentTaskAndReward = await _consumerTaskRepo.GetConsumerTaskWithReward(existingConsumerTask.ConsumerTask.TenantCode, (long)existingConsumerTask.ConsumerTask.ParentConsumerTaskId, Constants.Completed);

                if (!TaskHelper.ValidateParentTaskEligibility(parentTaskAndReward, consumerTasks, _consumerTaskServiceLogger))
                {
                    _consumerTaskServiceLogger.LogError("{className}.{methodName}: Consumer task not in right state, Consumer Code:{consumerCode}, TaskId: {consumerTaTaskIdskId}, curr-state: {currState}, Error Code:{errorCode}", className, methodName, consumerTaskDto.ConsumerCode,
                                 consumerTaskDto.TaskId, consumerTaskDto.TaskStatus, HttpStatusCode.Conflict);
                    return CreateErrorResponse(HttpStatusCode.Conflict, $"The maximum number of allowed occurrences ({recurrenceDetails?.periodic?.MaxOccurrences}) for this task within the {recurrenceDetails?.periodic?.period} recurrence period has been reached. Further completions will be available starting from the next period restart date.");
                }
            }

            else if (existingConsumerTask != null && existingConsumerTask.ConsumerTask != null &&
               (existingConsumerTask.ConsumerTask.TaskStatus == Constants.InProgress ||
               (existingConsumerTask.ConsumerTask.TaskStatus == Constants.Completed && !existingConsumerTask.TaskReward.IsRecurring)
               || (existingConsumerTask.TaskReward.IsRecurring && !TaskHelper.VerifyTaskValidOccurrences((int)consumerTaskDto.TaskId, consumerTasks, recurrenceDetails) && !TaskHelper.IsValidRecurring(existingConsumerTask.TaskReward, existingConsumerTask.ConsumerTask)
               && !TaskHelper.IsValidScheduleRecurring(recurrenceDetails, existingtaskRewardDto.IsRecurring, existingConsumerTask.ConsumerTask))))
            {
                // Task is already in progress, throw custom exception
                _consumerTaskServiceLogger.LogError("{className}.{methodName}: Consumer task not in right state, Consumer Code:{consumerCode}, TaskId: {consumerTaTaskIdskId}, curr-state: {currState}, Error Code:{errorCode}", className, methodName, consumerTaskDto.ConsumerCode,
                             consumerTaskDto.TaskId, consumerTaskDto.TaskStatus, HttpStatusCode.Conflict);
                return CreateErrorResponse(HttpStatusCode.Conflict, "Incorrect consumer task state");
            }

            using var transaction = _session.BeginTransaction();
            var consumerModel = _mapper.Map<ConsumerTaskModel>(consumerTaskDto);

            try
            {
                var result = await _consumerTaskRepo.CreateAsync(consumerModel);
                if (result.ConsumerTaskId > 0)
                {
                    _consumerTaskServiceLogger.LogInformation("{className}.{methodName}: successfully retrieved data from  ConsumerTask for Consumer Code:{consumerCode}, TaskStartTs: {TaskStartTs}", className, methodName, consumerTaskDto.ConsumerCode, consumerTaskDto.TaskStartTs);

                    response.ConsumerTask = _mapper.Map<ConsumerTaskDto>(consumerModel);
                    var taskRewardCode = existingConsumerTask?.TaskReward?.TaskRewardCode ?? validTaskRewards.FirstOrDefault()?.TaskReward?.TaskRewardCode;
                    if (string.IsNullOrEmpty(taskRewardCode))
                    {
                        _consumerTaskServiceLogger.LogError(
                        "{className}.{methodName}: Failed to publish message as task reward code not found",
                        className, methodName);
                    }
                    else
                    {
                        var publishResultDto = await GenerateConsumerTaskUpdateEvent(response.ConsumerTask, taskRewardCode);
                    }
                }
                return response;
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                _consumerTaskServiceLogger.LogError(ex, "{className}.{methodName}: PostConsumerTasks - ERROR Msg:{msg}", className, methodName, ex.Message);
                throw;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="errorCode"></param>
        /// <param name="errorMessage"></param>
        /// <returns></returns>
        private static ConsumerTaskResponseUpdateDto CreateErrorResponse(HttpStatusCode errorCode, string errorMessage)
        {
            return new ConsumerTaskResponseUpdateDto()
            {
                ErrorCode = (int)errorCode,
                ErrorMessage = errorMessage
            };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="consumerTaskDto"></param>
        /// <returns></returns>
        public async Task<ConsumerTaskDto> UpdateConsumerTask(UpdateConsumerTaskDto consumerTaskDto)
        {
            var consumerTaskModel = await _consumerTaskRepo.FindOneAsync(x => x.ConsumerTaskId == consumerTaskDto.ConsumerTaskId);
            const string methodName = nameof(UpdateConsumerTask);
            try
            {
                PublishResultDto publishResultDto = new PublishResultDto();
                var now = DateTime.UtcNow;
                var taskStatus = consumerTaskDto.TaskStatus ?? Constants.Completed;
                string taskRewardCode = string.Empty;
                if (consumerTaskModel != null && consumerTaskModel.ConsumerTaskId > 0)
                {
                    consumerTaskModel.Notes = consumerTaskDto.Notes ?? string.Empty;
                    consumerTaskModel.UpdateTs = now;
                    // consumerTaskModel.UpdateUser = consumerTaskDto.UpdateUser;
                    consumerTaskModel.TaskStatus = taskStatus;

                    var taskReward = await _taskRewardRepo.FindOneAsync(x => x.TaskId == consumerTaskModel.TaskId && x.TenantCode == consumerTaskModel.TenantCode && x.DeleteNbr == 0);
                    if (!consumerTaskDto.SkipValidation)
                    {
                        CheckTaskCompleteDate(consumerTaskDto, consumerTaskModel, taskStatus, taskReward);
                    }
                    taskRewardCode = taskReward?.TaskRewardCode ?? string.Empty;
                    if (taskStatus.Equals(Constants.Completed, StringComparison.CurrentCultureIgnoreCase) && consumerTaskDto?.TaskCompleteTs == null)
                    {
                        consumerTaskModel.TaskCompleteTs = now;
                    }
                    else
                    {
                        consumerTaskModel.TaskCompleteTs = consumerTaskDto.TaskCompleteTs.Value;
                    }

                    consumerTaskModel.Progress = consumerTaskDto.Progress;
                    consumerTaskModel.ProgressDetail = consumerTaskDto.ProgressDetail;

                    var result = await _consumerTaskRepo.UpdateAsync(consumerTaskModel);
                    _consumerTaskServiceLogger.LogInformation("{className}.{methodName}: Successfully retrieved data from  UpdateConsumer for ConsumerTaskId: {ConsumerTaskId}", className, methodName, consumerTaskDto.ConsumerTaskId);
                    if (consumerTaskDto.SpinWheelTaskEnabled)
                    {
                        await _subTaskService.CreateConsumerSubtask(consumerTaskDto);
                        _consumerTaskServiceLogger.LogInformation("{className}.{methodName} - CreateSubTaskConsumer: Successfully from  UpdateConsumer API for ConsumerTaskId: {ConsumerTaskId}", className, methodName, consumerTaskDto.ConsumerTaskId);
                    }
                    // upload file in S3
                    if (consumerTaskDto.TaskCompletionEvidenceDocument != null && consumerTaskDto.TaskCompletionEvidenceDocument.Length > 0)
                    {
                        await UploadTaskCompletionFile(consumerTaskDto.TaskCompletionEvidenceDocument, consumerTaskModel);
                    }

                    var response = _mapper.Map<ConsumerTaskDto>(result);
                    if (string.IsNullOrEmpty(taskRewardCode))
                    {
                        _consumerTaskServiceLogger.LogError(
                        "{className}.{methodName}: Failed to publish message as task reward code not found",
                        className, methodName);
                        return response;
                    }

                    publishResultDto = await GenerateConsumerTaskUpdateEvent(consumerTaskDto, taskRewardCode);

                    return response;
                }
                else
                {
                    // auto-enroll
                    bool requiresTaskRewardCheck = true;
                    long taskId;
                    if (consumerTaskDto.TaskId <= 0)
                    {
                        if (!string.IsNullOrEmpty(consumerTaskDto.TaskCode))
                        {
                            // look up task from taskCode
                            var task = await _taskRepo.FindOneAsync(x => x.TaskCode == consumerTaskDto.TaskCode && x.DeleteNbr == 0);
                            if (task == null)
                            {
                                _consumerTaskServiceLogger.LogError("{className}.{methodName}: Cannot find task, taskCode: {taskCode} Error Code:{errorCode}", className, methodName, consumerTaskDto.TaskCode, StatusCodes.Status404NotFound);
                                return new ConsumerTaskDto();
                            }
                            taskId = task.TaskId;
                        }
                        else if (!string.IsNullOrEmpty(consumerTaskDto.TaskExternalCode) && !string.IsNullOrEmpty(consumerTaskDto.TenantCode))
                        {
                            // look up TaskReward by TaskExternalCode
                            var taskReward = await _taskRewardRepo.FindOneAsync(x => x.TenantCode == consumerTaskDto.TenantCode &&
                                x.TaskExternalCode == consumerTaskDto.TaskExternalCode && x.DeleteNbr == 0);
                            if (taskReward == null)
                            {
                                _consumerTaskServiceLogger.LogError("{className}.{methodName}: Cannot find task, taskExternalCode: {taskExternalCode}, " +
                                    "tenantCode: {tenantCode} Error Code:{errorCode}", className, methodName, consumerTaskDto.TaskExternalCode, consumerTaskDto.TenantCode, StatusCodes.Status404NotFound);
                                return new ConsumerTaskDto();
                            }
                            taskRewardCode = taskReward.TaskRewardCode;
                            taskId = taskReward.TaskId;
                            requiresTaskRewardCheck = false;
                        }
                        else
                        {
                            _consumerTaskServiceLogger.LogError("{className}.{methodName}: TaskId, TaskCode and TaskExternalCode cannot all be null/empty/invalid, consumer: {consumer} Error code:{errorCode}", className, methodName,
                                consumerTaskDto.ConsumerCode, StatusCodes.Status404NotFound);
                            return new ConsumerTaskDto();
                        }
                    }
                    else
                    {
                        taskId = consumerTaskDto.TaskId;
                    }

                    // need tenantCode and consumerCode
                    if (string.IsNullOrEmpty(consumerTaskDto.TenantCode) || string.IsNullOrEmpty(consumerTaskDto.ConsumerCode))
                    {
                        _consumerTaskServiceLogger.LogError("{className}.{methodName}: Auto-enroll: TenantCode or ConsumerCode cannot be null, taskId: {taskId}, taskCode: {taskCode}, Error Code:{errorCode}", className, methodName,
                            consumerTaskDto.TaskId, consumerTaskDto.TaskCode, StatusCodes.Status404NotFound);
                        return new ConsumerTaskDto();
                    }

                    if (requiresTaskRewardCheck)
                    {
                        // before auto-enrolling, make sure task_reward exists
                        var taskReward = await _taskRewardRepo.FindOneAsync(x => x.TaskId == taskId && x.TenantCode == consumerTaskDto.TenantCode && x.DeleteNbr == 0);
                        if (taskReward == null)
                        {
                            _consumerTaskServiceLogger.LogError("{className}.{methodName}: Auto-enroll: Given taskId: {taskId}, is not valid for consumer: {consumer}, tenant: {tenant}, Error Code:{errorCode}", className, methodName,
                                consumerTaskDto.TaskId, consumerTaskDto.ConsumerCode, consumerTaskDto.TenantCode, StatusCodes.Status404NotFound);
                            return new ConsumerTaskDto();
                        }
                        taskRewardCode = taskReward?.TaskRewardCode ?? string.Empty;
                    }

                    consumerTaskModel = new ConsumerTaskModel
                    {
                        TaskId = taskId,
                        TenantCode = consumerTaskDto.TenantCode,
                        Notes = consumerTaskDto.Notes ?? string.Empty,
                        CreateTs = now,
                        CreateUser = Constants.CreateUser,
                        UpdateTs = default,
                        UpdateUser = null,
                        TaskStartTs = now,
                        TaskStatus = consumerTaskDto.TaskStatus ?? Constants.Completed,
                        TaskCompleteTs = now,
                        ConsumerCode = consumerTaskDto.ConsumerCode,
                        Progress = consumerTaskDto.Progress,
                        DeleteNbr = 0,
                        AutoEnrolled = true
                    };

                    var result = await _consumerTaskRepo.CreateAsync(consumerTaskModel);

                    _consumerTaskServiceLogger.LogInformation("{className}.{methodName}: Successfully added record in ConsumerTask table for ConsumerTaskId: {ConsumerTaskId}", className, methodName, consumerTaskDto.ConsumerTaskId);
                    if (consumerTaskDto.SpinWheelTaskEnabled)
                    {

                        consumerTaskDto.ConsumerTaskId = result.ConsumerTaskId;
                        await _subTaskService.CreateConsumerSubtask(consumerTaskDto);
                        _consumerTaskServiceLogger.LogInformation("{className}.{methodName}: Successfully from  UpdateConsumer API for ConsumerTaskId: {ConsumerTaskId}", className, methodName, consumerTaskDto.ConsumerTaskId);

                    }
                    var response = _mapper.Map<ConsumerTaskDto>(result);
                    if (string.IsNullOrEmpty(taskRewardCode))
                    {
                        _consumerTaskServiceLogger.LogError(
                        "{className}.{methodName}: Failed to publish message as task reward code not found",
                        className, methodName);
                        return response;
                    }

                    publishResultDto = await GenerateConsumerTaskUpdateEvent(consumerTaskDto, taskRewardCode);


                    return response;
                }
            }
            catch (Exception ex)
            {
                _consumerTaskServiceLogger.LogError(ex, "{className}.{methodName}: ERROR - {msg}, Error Code:{errorCode}", className, methodName, ex.Message, StatusCodes.Status500InternalServerError);
                return new ConsumerTaskDto();
            }
        }

        /// <summary>
        /// Update consumer task data
        /// </summary>
        /// <param name="consumerTaskDto"></param>
        /// <returns></returns>
        public async Task<BaseResponseDto> UpdateConsumerTaskDetails(ConsumerTaskDto consumerTaskDto)
        {
            const string methodName = nameof(UpdateConsumerTaskDetails);
            try
            {
                var consumerTask = await _consumerTaskRepo.FindOneAsync(consumerTaskDto.ConsumerTaskId);
                if (consumerTask == null)
                {
                    _consumerTaskServiceLogger.LogError("{className}.{methodName}: Consumer task not found with consumerTaskId: {consumerTaskId}",
                        className, methodName, consumerTaskDto.ConsumerTaskId);
                    return new BaseResponseDto
                    {
                        ErrorCode = StatusCodes.Status404NotFound,
                        ErrorMessage = $"Consumer task not found with consumerTaskId: {consumerTaskDto.ConsumerTaskId}"
                    };
                }
                consumerTask.WalletTransactionCode = consumerTaskDto.WalletTransactionCode;
                consumerTask.RewardInfoJson = consumerTaskDto.RewardInfoJson;
                consumerTask.UpdateTs = DateTime.UtcNow;
                consumerTask.UpdateUser = Constant.SystemUser;
                var result = await _consumerTaskRepo.UpdateAsync(consumerTask);

                _consumerTaskServiceLogger.LogInformation("{ClassName}.{MethodName}: Updated consumer task Successfully.", className, methodName);

                return new BaseResponseDto();
            }
            catch (Exception ex)
            {
                _consumerTaskServiceLogger.LogError(ex, "{className}.{methodName}: ERROR - {msg}, Error Code:{errorCode}",
                    className, methodName, ex.Message, StatusCodes.Status500InternalServerError);
                throw;
            }
        }

        private async void CheckTaskCompleteDate(UpdateConsumerTaskDto consumerTaskDto, ConsumerTaskModel? consumerTaskModel, string taskStatus, TaskRewardModel taskReward)
        {
            var now = DateTime.UtcNow;
            var recurrenceDetails = JsonConvert.DeserializeObject<RecurringDto>(taskReward?.RecurrenceDefinitionJson ?? string.Empty);
            var startDateOfRecurrence = await GetStartDateOfRecurrence(recurrenceDetails) ?? consumerTaskModel?.TaskStartTs.Date;
            bool IsFutureDate(DateTime? date) => date.HasValue && date.Value.Date > now.Date;
            bool IsBeforeStart(DateTime? completeTs, DateTime? startTs) => completeTs.HasValue && startTs.HasValue && completeTs.Value.Date < startTs.Value.Date;
            bool IsCompletedStatus(string status) => status.Equals(Constants.Completed, StringComparison.CurrentCultureIgnoreCase);

            if (IsFutureDate(consumerTaskDto.TaskCompleteTs))
            {
                LogError("TaskCompleteTs should not be less than Current Time.", consumerTaskDto, consumerTaskModel?.TaskStartTs, StatusCodes.Status400BadRequest);
                throw new InvalidOperationException("Task completion time stamp cannot be set to a future date.");
            }

            if (IsCompletedStatus(taskStatus))
            {
                if (IsBeforeStart(consumerTaskDto.TaskCompleteTs, startDateOfRecurrence) && taskReward != null && taskReward.IsRecurring)
                {
                    LogError("TaskCompleteTs should not be less than task Recurrence start date.", consumerTaskDto, startDateOfRecurrence, StatusCodes.Status400BadRequest);
                    throw new InvalidOperationException("TaskCompleteTs should not be less than task recurrent start date.");
                }

                if (IsBeforeStart(consumerTaskDto.TaskCompleteTs, taskReward?.ValidStartTs) && taskReward != null && !taskReward.IsRecurring)
                {
                    LogError("TaskCompleteTs should not be less than task valid start date.", consumerTaskDto, taskReward.ValidStartTs, StatusCodes.Status400BadRequest);
                    throw new InvalidOperationException("TaskCompleteTs should not be less than task valid start date.");
                }
            }
        }

        private async Task<DateTime?> GetStartDateOfRecurrence(RecurringDto? recurrenceDetails)
        {
            if (recurrenceDetails == null)
            {
                return null;
            }

            // Process periodic recurrence types
            if (recurrenceDetails.recurrenceType == Constant.Periodic && recurrenceDetails.periodic?.period != null)
            {
                // based on the period restart date and recurrence type (e.g., monthly, quarterly).
                var (periodStartDate, _) = await _taskCommonHelper.GetPeriodStartAndEndDatesAsync(recurrenceDetails.periodic.periodRestartDate, recurrenceDetails.periodic.period);
                return periodStartDate;
            }
            else if (recurrenceDetails.Schedules != null && recurrenceDetails.recurrenceType == Constant.Schedule)
            {
                var (scheduleStartDate, _) = await _taskCommonHelper.FindMatchingScheduleStartDateAndExpiryDateAsync(recurrenceDetails.Schedules);
                return scheduleStartDate;
            }

            return null;
        }

        private void LogError(string message, UpdateConsumerTaskDto taskDto, DateTime? comparisonDate, int errorCode)
        {
            _consumerTaskServiceLogger.LogError("{className}.{methodName}: {message} TaskId: {taskId}, ConsumerTaskId: {consumerTaskId}, TaskCompleteTs: {taskCompleteTs}, ComparisonDate: {comparisonDate}, ErrorCode: {errorCode}",
               className, nameof(CheckTaskCompleteDate), message, taskDto.TaskId, taskDto.TaskCode, taskDto.TaskCompleteTs?.Date, comparisonDate?.Date, errorCode);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="consumerTaskRequestDto"></param>
        /// <returns></returns>
        public async Task<ConsumerTaskResponseDto> GetAllConsumerTask(ConsumerTaskRequestDto consumerTaskRequestDto)
        {
            const string methodName = nameof(GetAllConsumerTask);
            try
            {
                var taskRewardRequestDto = new FindTaskRewardRequestDto()
                {
                    ConsumerCode = consumerTaskRequestDto.ConsumerCode,
                    TenantCode = consumerTaskRequestDto.TenantCode,
                    LanguageCode = consumerTaskRequestDto.LanguageCode
                };
                var allTaskList = await _taskRewardService.GetTaskRewards(taskRewardRequestDto, true);

                if (allTaskList == null || allTaskList.TaskRewardDetails == null || allTaskList.TaskRewardDetails.Count <= 0)
                {
                    _consumerTaskServiceLogger.LogError("{className}.{methodName}: taskRewardList not found for given TenantCode: {tenantCode},Error Code:{errorCode}", className, methodName, taskRewardRequestDto.TenantCode, StatusCodes.Status404NotFound);
                    return new ConsumerTaskResponseDto();
                }

                var consumerTasks = await _consumerTaskRepo.FindAsync(x => x.ConsumerCode == taskRewardRequestDto.ConsumerCode && x.DeleteNbr == 0);

                var availableRewardDetails = allTaskList.TaskRewardDetails;
                if (consumerTaskRequestDto.FilterTaskReward)
                {
                    availableRewardDetails = FilterValidTaskRewards(allTaskList.TaskRewardDetails);
                }
                var availableTaskList = await FilterAvailableTasks(consumerTasks, availableRewardDetails);
                var pendingTaskList = await FilterTasksByStatus(availableRewardDetails, consumerTasks, Constants.InProgress);
                var completedTaskList = await FilterTasksByStatus(allTaskList.TaskRewardDetails, consumerTasks, Constants.Completed);

                var consumerTaskResponse = new ConsumerTaskResponseDto()
                {
                    AvailableTasks = availableTaskList,
                    PendingTasks = pendingTaskList,
                    CompletedTasks = completedTaskList
                };
                _consumerTaskServiceLogger.LogInformation("{className}.{methodName}: successfully retrieved data from  GetAllConsumerTasks API for ConsumerCode: {ConsumerCode}", className, methodName, consumerTaskRequestDto.ConsumerCode);


                return consumerTaskResponse;
            }
            catch (Exception ex)
            {
                _consumerTaskServiceLogger.LogError(ex, "{className}.{methodName}: ERROR - {msg}, Error Code:{errorCode}", className, methodName, ex.Message, StatusCodes.Status500InternalServerError);
                return new ConsumerTaskResponseDto();
            }
        }
        /// <summary>
        /// This will add the computedTaskExpiryTs for all the available Tasks
        /// </summary>
        /// <param name="consumerTasks"></param>
        /// <param name="availableRewardDetails"></param>
        /// <returns>It will return the list of available Tasks</returns>
        private async Task<List<TaskRewardDetailDto>> FilterAvailableTasks(IList<ConsumerTaskModel> consumerTasks, List<TaskRewardDetailDto> availableRewardDetails)
        {
            var availableTaskList = await TaskHelper.FilterAvailableTasksAsync(availableRewardDetails, consumerTasks, _consumerTaskRepo, _consumerTaskServiceLogger);
            foreach (var availableTask in availableTaskList)
            {
                if (availableTask!.TaskReward != null && availableTask.TaskReward.IsRecurring)
                {
                    await _commonTaskRewardService.RecurrenceTaskProcess(availableTask);

                }
                if (availableTask!.TaskReward != null && !availableTask.TaskReward.IsRecurring)
                {
                    availableTask.MinAllowedTaskCompleteTs = availableTask.TaskReward.ValidStartTs;
                    availableTask.TaskReward.MaxAllowedTaskCompletionTs = DateTime.UtcNow;
                    availableTask.ComputedTaskExpiryTs = availableTask.TaskReward.Expiry;
                }
            }

            return availableTaskList;
        }

        /// <summary>
        /// Revert all consumer tasks for given consumer
        /// </summary>
        /// <param name="revertAllConsumerTasksRequestDto"></param>
        /// <returns></returns>
        public async Task<BaseResponseDto> RevertAllConsumerTasks(RevertAllConsumerTasksRequestDto revertAllConsumerTasksRequestDto)
        {
            const string methodName = nameof(RevertAllConsumerTasks);
            var tenantCode = revertAllConsumerTasksRequestDto.TenantCode;
            var consumerCode = revertAllConsumerTasksRequestDto.ConsumerCode;
            if (string.IsNullOrEmpty(tenantCode) || string.IsNullOrEmpty(consumerCode))
            {
                _consumerTaskServiceLogger.LogError("{className}.{methodName}: Tenant Code or Consumer Code IsNullOrEmpty, Consumer:{consumer}, Error Code:{errorCode}", className, methodName, revertAllConsumerTasksRequestDto.ConsumerCode, StatusCodes.Status400BadRequest);
                return new BaseResponseDto() { ErrorCode = Microsoft.AspNetCore.Http.StatusCodes.Status400BadRequest, ErrorMessage = "Invalid input" };
            }

            using var transaction = _session.BeginTransaction();
            try
            {
                var consumerTasks = await _consumerTaskRepo.FindAsync(x => x.ConsumerCode == consumerCode && x.TenantCode == tenantCode && x.DeleteNbr == 0);
                foreach (var consumerTask in consumerTasks)
                {
                    consumerTask.DeleteNbr = consumerTask.ConsumerTaskId;
                    consumerTask.UpdateTs = DateTime.UtcNow;
                    await _session.UpdateAsync(consumerTask);
                }
                await transaction.CommitAsync();
                _consumerTaskServiceLogger.LogInformation("{className}.{methodName}: Successfully all consumer tasks for consumer: {ConsumerCode}", className, methodName, consumerCode);

                return new BaseResponseDto();
            }
            catch (Exception ex)
            {
                _consumerTaskServiceLogger.LogError(ex, "{className}.{methodName}: ERROR - Message : {Message}", className, methodName, ex.Message);
                await transaction.RollbackAsync();
                throw;
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rewardTypeConsumerTaskRequestDto"></param>
        /// <returns></returns>
        public async Task<ConsumerTaskResponseDto> GetAvailableTaskRewardType(GetRewardTypeConsumerTaskRequestDto rewardTypeConsumerTaskRequestDto)
        {
            const string methodName = nameof(GetAvailableTaskRewardType);
            var response = new ConsumerTaskResponseDto();
            try
            {
                var taskRewardType = await _taskRewardTypeRepo.FindOneAsync(x => x.RewardTypeCode == rewardTypeConsumerTaskRequestDto.RewardTypeCode);

                var availableTask = new ConsumerTaskRequestDto()
                {
                    ConsumerCode = rewardTypeConsumerTaskRequestDto.ConsumerCode,
                    TenantCode = rewardTypeConsumerTaskRequestDto.TenantCode,
                    LanguageCode = rewardTypeConsumerTaskRequestDto.LanguageCode
                };

                var availableTaskList = await GetAllConsumerTask(availableTask);

                var filteredAvailableTasks = availableTaskList?.AvailableTasks?.Where(x => x.TaskReward?.RewardTypeId == taskRewardType.RewardTypeId)
                    .ToList();

                var filteredPendingTasks = availableTaskList?.PendingTasks?.Where(x => x.TaskReward?.RewardTypeId == taskRewardType.RewardTypeId)
                    .ToList();

                response.AvailableTasks = filteredAvailableTasks;
                response.PendingTasks = filteredPendingTasks;
                response.CompletedTasks = availableTaskList?.CompletedTasks?.ToList();

                _consumerTaskServiceLogger.LogInformation("{className}.{methodName}: successfully retrieved data from  GetAvailableTaskRewardType API for ConsumerCode : {consumerCode} ," +
                    "TenantCode " + ":{TenantCode}," + "rewardTypeCode :{RewardTypeCode}", className, methodName, rewardTypeConsumerTaskRequestDto.ConsumerCode,
                    rewardTypeConsumerTaskRequestDto.TenantCode, rewardTypeConsumerTaskRequestDto.RewardTypeCode);

                return response;
            }
            catch (Exception ex)
            {
                _consumerTaskServiceLogger.LogError(ex, "{className}.{methodName}: ERROR - Message : {Message}", className, methodName, ex.Message);
                throw;
            }
        }

        /// <summary>
        /// get jey , secret and Bucket and upload to S3
        /// </summary>
        /// <param name="consumerTaskfile"></param>
        /// <param name="consumerTaskModel"></param>
        /// <returns></returns>
        public async System.Threading.Tasks.Task UploadTaskCompletionFile(Microsoft.AspNetCore.Http.IFormFile consumerTaskfile, ConsumerTaskModel consumerTaskModel)
        {
            const string methodName = nameof(UploadTaskCompletionFile);
            string awsAccessKey = await _vault.GetSecret(_configuration.GetSection("AWS:AWS_ACCESS_KEY_NAME").Value?.ToString() ?? "");
            string awsSecretKey = await _vault.GetSecret(_configuration.GetSection("AWS:AWS_SECRET_KEY_NAME").Value?.ToString() ?? "");
            string bucketName = _configuration.GetSection("AWS:AWS_BUCKET_NAME").Value?.ToString() ?? "";
            var extention = Path.GetExtension(consumerTaskfile.FileName);
            var fileName = $"{consumerTaskModel.ConsumerTaskId}_task_complete_evidence.{extention}";
            using (MemoryStream memoryStream = new MemoryStream())
            {
                await consumerTaskfile.CopyToAsync(memoryStream);
                memoryStream.Position = 0;
                Stream inputStream = memoryStream;
                using (var s3Client = new AmazonS3Client(awsAccessKey, awsSecretKey, RegionEndpoint.USEast2))
                {
                    try
                    {
                        _consumerTaskServiceLogger.LogInformation("{className}.{methodName}: File uploaded to Bucket:{name}", className, methodName, bucketName);
                        await _fileHelper.UploadFile(s3Client, bucketName, CONSUMER_TASK_EVIDENCE_FOLDER, fileName, inputStream);
                    }
                    catch (Exception ex)
                    {

                        _consumerTaskServiceLogger.LogError(ex, $"{className}.{methodName} - UploadFile: ERROR uploading file to S3 bucket: {ex.Message}");
                    }

                }
            }
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

        private async Task<List<TaskRewardDetailDto>> FilterTasksByStatus(List<TaskRewardDetailDto> taskRewardDetails, IList<ConsumerTaskModel> consumerTasks, string taskStatus)
        {
            DateTime currentMonthTS = DateTime.UtcNow;
            DateTime tsNow = DateTime.UtcNow;
            var taskRewardDetailsList = new List<TaskRewardDetailDto>();
            var consumerTaskList = new List<ConsumerTaskDto>();
            try
            {
                if (taskStatus.ToLower() == Constants.Completed.ToLower())
                {
                    var consumerTaskCompletedList = consumerTasks.Where(x =>
                    x.TaskStatus != null && x.TaskStatus.ToLower() == Constants.Completed.ToLower() && x.DeleteNbr == 0).ToList();
                    consumerTaskList = _mapper.Map<IList<ConsumerTaskDto>>(consumerTaskCompletedList).ToList();
                }
                else if (taskStatus.ToLower() != Constants.Completed.ToLower())
                {
                    var consumerTaskPendingList = consumerTasks.Where(x =>
                    x.TaskStatus != null && x.TaskStatus.ToLower() != Constants.Completed.ToLower() && x.DeleteNbr == 0).ToList();
                    consumerTaskList = _mapper.Map<IList<ConsumerTaskDto>>(consumerTaskPendingList).ToList();
                }

                consumerTasks = consumerTasks?.OrderByDescending(x => x.ConsumerTaskId).ToList();
                if (consumerTasks?.Count <= 0)
                    return taskRewardDetailsList;

                foreach (var consumerTask in consumerTaskList)
                {
                    var taskReward = taskRewardDetails.FirstOrDefault(x => x.TaskReward.TaskId == consumerTask.TaskId);
                    if (taskReward == null)
                    {
                        continue;
                    }
                    var taskRewardDto = _mapper.Map<TaskRewardDto>(taskReward.TaskReward);


                    var taskRewardDetailDto = new TaskRewardDetailDto
                    {
                        Task = taskReward.Task,
                        TaskReward = taskRewardDto,
                        TaskDetail = taskReward.TaskDetail,
                        TermsOfService = taskReward.TermsOfService,
                        TenantTaskCategory = taskReward.TenantTaskCategory,
                        TaskType = taskReward.TaskType,
                        RewardTypeName = taskReward.RewardTypeName,
                        ConsumerTask = new ConsumerTaskStatTSDto
                        {
                            TaskStartTs = consumerTask?.TaskStartTs
                        },
                        ConsumerTaskDto = consumerTask
                    };

                    if (taskRewardDetailDto!.TaskReward != null && taskRewardDetailDto.TaskReward.IsRecurring)
                    {
                        await _commonTaskRewardService.RecurrenceTaskProcess(taskRewardDetailDto);

                    }
                    if (taskRewardDetailDto!.TaskReward != null && !taskRewardDetailDto.TaskReward.IsRecurring)
                    {
                        taskRewardDetailDto.MinAllowedTaskCompleteTs = taskRewardDetailDto.TaskReward.ValidStartTs;
                        taskRewardDetailDto.TaskReward.MaxAllowedTaskCompletionTs = DateTime.UtcNow;
                        taskRewardDetailDto.ComputedTaskExpiryTs = taskRewardDetailDto.TaskReward.Expiry;
                    }
                    taskRewardDetailsList.Add(taskRewardDetailDto);

                }
            }
            catch (Exception ex)
            {
                _consumerTaskServiceLogger.LogError(ex, "{className}.FilterTasksByStatus - ERROR Msg:{msg}", className, ex.Message);
                throw;
            }

            return taskRewardDetailsList;
        }

        /// <summary>
        /// Removes a specific consumer task based on the provided request details.
        /// </summary>
        /// <param name="deleteConsumerTaskRequestDto">The request DTO containing consumer and task details for the task to be removed.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a <see cref="BaseResponseDto"/> 
        /// indicating the success or failure of the removal operation.</returns>
        public async Task<BaseResponseDto> RemoveConsumerTask(DeleteConsumerTaskRequestDto deleteConsumerTaskRequestDto)
        {
            const string methodName = nameof(RemoveConsumerTask);
            try
            {
                // Check for valid TaskExternalCode and TenantCode
                if (!string.IsNullOrEmpty(deleteConsumerTaskRequestDto.TaskExternalCode) && !string.IsNullOrEmpty(deleteConsumerTaskRequestDto.TenantCode))
                {
                    // Look up TaskReward by TaskExternalCode
                    var taskReward = await _taskRewardRepo.FindOneAsync(x => x.TenantCode == deleteConsumerTaskRequestDto.TenantCode &&
                        x.TaskExternalCode == deleteConsumerTaskRequestDto.TaskExternalCode && x.DeleteNbr == 0);

                    if (taskReward == null)
                    {
                        _consumerTaskServiceLogger.LogError("{className}.{methodName}: Cannot find consumer task, taskExternalCode: {taskExternalCode}, " +
                            "tenantCode: {tenantCode} Error Code:{errorCode}", className, methodName, deleteConsumerTaskRequestDto.TaskExternalCode, deleteConsumerTaskRequestDto.TenantCode, StatusCodes.Status404NotFound);
                        return new BaseResponseDto() { ErrorCode = StatusCodes.Status404NotFound, ErrorMessage = $"Task Reward not found for taskExternalCode: {deleteConsumerTaskRequestDto.TaskExternalCode}" };
                    }

                    var taskId = taskReward.TaskId;

                    var consumerTask = await _consumerTaskRepo.FindOneAsync(x => x.ConsumerCode == deleteConsumerTaskRequestDto.ConsumerCode &&
                        x.TaskId == taskId && x.TenantCode == deleteConsumerTaskRequestDto.TenantCode &&
                        x.DeleteNbr == 0);

                    if (consumerTask == null)
                    {
                        _consumerTaskServiceLogger.LogError("{className}.{methodName} - Consumer Task Not found with ConsumerCode:{Code},TenantCode:{TenantCode}, TaskId :{TaskId}", className, methodName,
                            deleteConsumerTaskRequestDto.ConsumerCode, deleteConsumerTaskRequestDto.TenantCode, taskId);
                        return new BaseResponseDto() { ErrorCode = StatusCodes.Status404NotFound, ErrorMessage = $"Consumer Task not found for  TaskId:{taskId}" };
                    }

                    // Delete the consumer task
                    consumerTask.UpdateUser = Constant.SystemUser;
                    consumerTask.DeleteNbr = consumerTask.ConsumerTaskId;
                    consumerTask.UpdateTs = DateTime.UtcNow;

                    var deleteResult = await _consumerTaskRepo.UpdateAsync(consumerTask);
                    if (deleteResult == null)
                    {
                        _consumerTaskServiceLogger.LogError("{className}.{methodName} - Consumer Task Not Deleted TaskId :{TaskId}", className, methodName, taskId);
                        return new BaseResponseDto() { ErrorCode = StatusCodes.Status500InternalServerError, ErrorMessage = $"Consumer Task not Deleted - TaskId :{taskId}" };
                    }
                    return new BaseResponseDto();
                }

                // Return an appropriate response if TaskExternalCode or TenantCode is missing
                return new BaseResponseDto() { ErrorCode = StatusCodes.Status400BadRequest, ErrorMessage = "Invalid input: TaskExternalCode and TenantCode are required." };
            }
            catch (Exception ex)
            {
                _consumerTaskServiceLogger.LogError(ex, "{className}.{methodName} - Consumer Task Not Deleted: {Message}", className, methodName, ex.Message);
                throw new InvalidDataException(ex.Message);
            }
        }

        private async Task<PublishResultDto> GenerateConsumerTaskUpdateEvent(ConsumerTaskDto consumerTaskDto, string taskRewardCode)
        {
            const string methodName = nameof(GenerateConsumerTaskUpdateEvent);
            try
            {
                _consumerTaskServiceLogger.LogInformation(
                    "{className}.{methodName}: Starting event publish for consumer task update. ConsumerTaskId: {ConsumerTaskId}",
                    className, methodName, consumerTaskDto.ConsumerTaskId);

                var eventDto = new EventDto<ConsumerTaskEventDto>
                {
                    Header = new EventHeaderDto
                    {
                        EventId = $"evt-{Guid.NewGuid():N}",
                        ConsumerCode = consumerTaskDto.ConsumerCode,
                        TenantCode = consumerTaskDto.TenantCode,
                        EventSubtype = Constant.ConsumerTaskEventSubType,
                        EventType = Constant.ConsumerTaskEventType,
                        PublishTs = DateTime.UtcNow,
                        SourceModule = Constant.TaskApiSource
                    },
                    Data = new ConsumerTaskEventDto
                    {
                        ProgressDetail = consumerTaskDto.ProgressDetail,
                        Status = consumerTaskDto.TaskStatus ?? Constant.EnrolledTaskStatus,
                        TaskRewardCode = taskRewardCode
                    }
                };

                _consumerTaskServiceLogger.LogInformation(
                    "{className}.{methodName}: Created EventDto for consumer task update. EventDto: {eventDto}",
                    className, methodName, eventDto.ToJson());

                var publishResultDto = await _heliosEventPublisher.PublishMessage(eventDto.Header, eventDto.Data);

                if (!string.IsNullOrEmpty(publishResultDto.ErrorCode))
                {
                    _consumerTaskServiceLogger.LogError(
                        "{className}.{methodName}: Consumer task update event publish failed. Request: {eventDto}, Response: {responseDto}",
                        className, methodName, eventDto.ToJson(), publishResultDto.ToJson());
                }

                return publishResultDto;
            }
            catch (Exception ex)
            {
                _consumerTaskServiceLogger.LogError(ex,
                    "{className}.{methodName}: Consumer task update event publish failed. Error: {Message}",
                    className, methodName, ex.Message);

                return new PublishResultDto
                {
                    ErrorCode = StatusCodes.Status500InternalServerError.ToString(),
                    ErrorMessage = ex.Message
                };
            }
        }

        public async Task<PageinatedCompletedConsumerTaskResponseDto> GetConsumersByTaskId(GetConsumerTaskByTaskId getConsumerTaskByTaskId)
        {
            const string methodName = nameof(GetConsumersByTaskId);

            _consumerTaskServiceLogger.LogInformation("{ClassName}.{MethodName}: Started processing for TaskId: {TaskId}, TenantCode: {TenantCode}, StartDate: {StartDate}, EndDate: {EndDate}",
                className, methodName, getConsumerTaskByTaskId.TaskId, getConsumerTaskByTaskId.TenantCode, getConsumerTaskByTaskId.StartDate, getConsumerTaskByTaskId.EndDate);

            try
            {

                var pageinatedResponse = await _consumerTaskRepo.GetPaginatedConsumerTask(getConsumerTaskByTaskId);

                _consumerTaskServiceLogger.LogInformation("{ClassName}.{MethodName}: Found {Count} completed tasks.", className, methodName, pageinatedResponse.TotalRecords);

                return new PageinatedCompletedConsumerTaskResponseDto
                {
                    CompletedTasks = _mapper.Map<List<ConsumerTaskDto>>(pageinatedResponse.CompletedTasks),
                    TotalRecords = pageinatedResponse.TotalRecords
                };
            }
            catch (Exception ex)
            {
                _consumerTaskServiceLogger.LogError(ex, "{ClassName}.{MethodName}: Error occurred - {Message}", className, methodName, ex.Message);
                throw;
            }
        }

        public async Task<ConsumerHealthTaskResponseUpdateDto> UpdateHealthTaskProgress(UpdateHealthTaskProgressRequestDto request)
        {
            var methodName = nameof(UpdateHealthTaskProgress);
            var isTaskCompleted = false;

            if (!Enum.IsDefined(typeof(HealthTaskType), request.HealthTaskType))
            {
                return new ConsumerHealthTaskResponseUpdateDto
                {
                    ErrorCode = StatusCodes.Status400BadRequest,
                    ErrorMessage = $"Invalid HealthTaskType: {request.HealthTaskType}"
                };
            }
            var taskRewards = await _taskRewardRepo.FindAsync(x =>
                x.TaskId == request.TaskId &&
                x.TenantCode == request.TenantCode &&
                x.DeleteNbr == 0);
            var taskReward = taskRewards?.OrderByDescending(x => x.TaskRewardId).FirstOrDefault();
            if (taskReward == null)
            {
                _consumerTaskServiceLogger.LogError("{ClassName}.{MethodName}: Task reward not found for TaskId: {TaskId}, TenantCode: {TenantCode}",
                    className, methodName, request.TaskId, request.TenantCode);

                return NotFoundResponse("Task reward not found");
            }
            if (!taskReward.SelfReport)
            {
                _consumerTaskServiceLogger.LogError(
                    "{ClassName}.{MethodName}: Task reward is not self-reportable for TaskId: {TaskId}, TenantCode: {TenantCode}",
                    className, methodName, request.TaskId, request.TenantCode);

                return NotFoundResponse("Task reward not found");
            }

            var criteria = taskReward?.TaskCompletionCriteria;
            if (criteria?.CompletionCriteriaType != Constant.HealthCriteriaType)
            {
                return NotFoundResponse("CompletionCriteriaType not found");
            }

            var consumerTasks = await _consumerTaskRepo.FindAsync(x =>
                x.ConsumerCode == request.ConsumerCode &&
                x.TaskId == request.TaskId &&
                x.TenantCode == request.TenantCode &&
                x.TaskStatus == Constants.InProgress &&
                x.DeleteNbr == 0);

            var consumerTask = consumerTasks?.OrderByDescending(x => x.ParentConsumerTaskId).FirstOrDefault();

            if (consumerTask == null)
            {
                _consumerTaskServiceLogger.LogError("{ClassName}.{MethodName}: Consumer task not found for ConsumerCode: {ConsumerCode}, TaskId: {TaskId}, TenantCode: {TenantCode}",
                    className, methodName, request.ConsumerCode, request.TaskId, request.TenantCode);

                return NotFoundResponse("Consumer task not found");
            }

            var healthTaskType = request.HealthTaskType.Trim().ToUpper();

            consumerTask.ProgressDetail = UpdateProgressDetail(healthTaskType, request, consumerTask.ProgressDetail, criteria, ref consumerTask);
            if (consumerTask.TaskStatus == Constants.Completed)
            {
                isTaskCompleted = true;
                consumerTask.TaskStatus = Constants.InProgress;
            }
            consumerTask.UpdateTs = DateTime.UtcNow;
            consumerTask.UpdateUser = Constant.SystemUser;

            await _consumerTaskRepo.UpdateAsync(consumerTask);

            _consumerTaskServiceLogger.LogInformation("{ClassName}.{MethodName}: Successfully updated health task progress for ConsumerCode: {ConsumerCode}, TaskId: {TaskId}, TenantCode: {TenantCode}",
                className, methodName, request.ConsumerCode, request.TaskId, request.TenantCode);

            return new ConsumerHealthTaskResponseUpdateDto
            {
                ConsumerTask = _mapper.Map<ConsumerTaskDto>(consumerTask),
                IsTaskCompleted = isTaskCompleted
            };
        }

        private string UpdateProgressDetail(string healthTaskType, UpdateHealthTaskProgressRequestDto request, string? existingDetail, TaskCompletionCriteriaJson? criteria, ref ConsumerTaskModel consumerTask)
        {
            TrackingDto newActivity = new()
            {
                TimeStamp = request.DateTimeAddedFor != null ? request.DateTimeAddedFor.Value : DateTime.UtcNow,
                Source = "Manual"
            };

            switch (healthTaskType)
            {
                case nameof(HealthTaskType.SLEEP):
                    var sleep = string.IsNullOrWhiteSpace(existingDetail)
                        ? new HealthProgressDetails<SleepRollupDataDto>
                        {
                            DetailType = nameof(HealthTaskType.SLEEP),
                            HealthProgress = new SleepRollupDataDto
                            {
                                SleepTracking = new SleepTrackingDto
                                {
                                    NumDaysAtOrAboveMinDuration = 0
                                }
                            }
                        }
                        : JsonConvert.DeserializeObject<HealthProgressDetails<SleepRollupDataDto>>(existingDetail, _settings);

                    if(sleep == null)
                    {
                        sleep = new HealthProgressDetails<SleepRollupDataDto>
                        {
                            DetailType = nameof(HealthTaskType.SLEEP),
                            HealthProgress = new SleepRollupDataDto
                            {
                                SleepTracking = new SleepTrackingDto
                                {
                                    NumDaysAtOrAboveMinDuration = 0
                                }
                            }
                        };
                    }

                    var unitsAdded = request.NumberOfDays ?? 0;
                    var dateTimeAddedForSleep = request.DateTimeAddedFor ?? DateTime.UtcNow;

                    if (sleep.HealthProgress != null && sleep.HealthProgress.ActivityLog.Any(a => a.TimeStamp.ToString() == dateTimeAddedForSleep.ToString()))
                    {
                        var existingActivity = sleep.HealthProgress.ActivityLog.First(a => a.TimeStamp.ToString() == dateTimeAddedForSleep.ToString());

                        sleep.HealthProgress.SleepTracking.NumDaysAtOrAboveMinDuration -= existingActivity.UnitsAdded;
                        sleep.HealthProgress.ActivityLog = sleep.HealthProgress.ActivityLog
                            .Where(a => a.TimeStamp.ToString() != dateTimeAddedForSleep.ToString())
                            .ToArray();
                        newActivity.TimeStamp = DateTime.UtcNow;
                    }

                    // Add the new activity
                    newActivity.UnitsAdded = unitsAdded;
                    sleep.HealthProgress.ActivityLog = sleep.HealthProgress.ActivityLog.Append(newActivity).ToArray();
                    
                    sleep.HealthProgress.SleepTracking.NumDaysAtOrAboveMinDuration += unitsAdded;

                    if (criteria?.HealthCriteria?.HealthTaskType == nameof(HealthTaskType.SLEEP) &&
                        criteria.HealthCriteria.RequiredSleep?.NumDaysAtOrAboveMinDuration <= sleep.HealthProgress.SleepTracking.NumDaysAtOrAboveMinDuration)
                    {
                        consumerTask.TaskStatus = Constants.Completed;
                        consumerTask.TaskCompleteTs = DateTime.UtcNow;
                    }

                    return JsonConvert.SerializeObject(sleep, _settings);

                case nameof(HealthTaskType.STEPS):
                    var steps = string.IsNullOrWhiteSpace(existingDetail)
                        ? new HealthProgressDetails<StepsRollupDataDto>
                        {
                            DetailType = nameof(HealthTaskType.STEPS),
                            HealthProgress = new StepsRollupDataDto
                            {
                                TotalSteps = 0,
                                ActivityLog = []
                            }
                        }
                        : JsonConvert.DeserializeObject<HealthProgressDetails<StepsRollupDataDto>>(existingDetail, _settings);

                    if(steps == null)
                    {
                        steps = new HealthProgressDetails<StepsRollupDataDto>
                        {
                            DetailType = nameof(HealthTaskType.STEPS),
                            HealthProgress = new StepsRollupDataDto
                            {
                                TotalSteps = 0,
                                ActivityLog = []
                            }
                        };
                    }

                    if (steps.HealthProgress == null)
                    {
                        throw new Exception($"HealthProgress for Steps is recorded as null earlier for taskId {request.TaskId} consumer code {request.ConsumerCode}");
                    }
                    var unitsofStepsAdded = request.Steps ?? 0;

                    var dateTimeAddedFor = request.DateTimeAddedFor ?? DateTime.UtcNow;
                    // Check if an activity with the same timestamp already exists
                    if (steps.HealthProgress.ActivityLog.Any(a => a.TimeStamp.ToString() == dateTimeAddedFor.ToString()))
                    {
                        var existingActivity = steps.HealthProgress.ActivityLog.First(a => a.TimeStamp.ToString() == dateTimeAddedFor.ToString());

                        steps.HealthProgress.TotalSteps -= existingActivity.UnitsAdded;
                        // Remove the matched activity from ActivityLog
                        steps.HealthProgress.ActivityLog = steps.HealthProgress.ActivityLog
                            .Where(a => a.TimeStamp.ToString() != dateTimeAddedFor.ToString())
                            .ToArray();
                        newActivity.TimeStamp = DateTime.UtcNow;
                    }

                    // Add the new activity
                    newActivity.UnitsAdded = unitsofStepsAdded;
                    steps.HealthProgress.ActivityLog = steps.HealthProgress.ActivityLog.Append(newActivity).ToArray();

                    steps.HealthProgress.TotalSteps += unitsofStepsAdded;

                    if (criteria?.HealthCriteria?.HealthTaskType == nameof(HealthTaskType.STEPS) &&
                        criteria.HealthCriteria.RequiredSteps <= steps.HealthProgress.TotalSteps)
                    {
                        consumerTask.TaskStatus = Constants.Completed;
                    }

                    return JsonConvert.SerializeObject(steps, _settings);

                case nameof(HealthTaskType.HYDRATION):
                    return UpdateHydrationConsumerTask(existingDetail, healthTaskType, request, newActivity, criteria, ref consumerTask);
                case nameof(HealthTaskType.OTHER):
                   return UpdateOtherConsumerTask(existingDetail, healthTaskType, request, newActivity, criteria, ref consumerTask);
                default:
                    return existingDetail ?? string.Empty;
            }
        }

        private string UpdateOtherConsumerTask(string? existingDetail, string healthTaskType, UpdateHealthTaskProgressRequestDto request, TrackingDto newActivity, TaskCompletionCriteriaJson? criteria, ref ConsumerTaskModel consumerTask)
        {
            var otherTask = string.IsNullOrWhiteSpace(existingDetail)
                        ? new HealthProgressDetails<OtherHealthTaksRollupDataDto>
                        {
                            DetailType = nameof(HealthTaskType.OTHER),
                            HealthProgress = new OtherHealthTaksRollupDataDto
                            {
                                TotalUnits = 0,
                            }
                        }
                        : JsonConvert.DeserializeObject<HealthProgressDetails<OtherHealthTaksRollupDataDto>>(existingDetail, _settings);

            if (otherTask == null)
            {
                otherTask = new HealthProgressDetails<OtherHealthTaksRollupDataDto>
                {
                    DetailType = nameof(HealthTaskType.OTHER),
                    HealthProgress = new OtherHealthTaksRollupDataDto
                    {
                        TotalUnits = 0,
                        ActivityLog = [],
                        HealthReport = new List<HealthTrackingDto>()
                    }
                };
            }

            if (otherTask.HealthProgress == null)
            {
                throw new Exception($"HealthProgress for {healthTaskType} is recorded as null earlier for taskId {request.TaskId} consumer code {request.ConsumerCode}");
            }

            var unitsAdded = request.NumberOfUnits ?? 0;
            var dateTimeAddedFor = request.DateTimeAddedFor ?? DateTime.UtcNow;
            
            // Check if an activity with the same timestamp already exists
            if (otherTask.HealthProgress.ActivityLog.Any(a => a.TimeStamp.ToString() == dateTimeAddedFor.ToString()))
            {
                var existingActivity = otherTask.HealthProgress.ActivityLog.First(a => a.TimeStamp.ToString() == dateTimeAddedFor.ToString());

                otherTask.HealthProgress.TotalUnits -= existingActivity.UnitsAdded;
                // Remove the matched activity from ActivityLog
                otherTask.HealthProgress.ActivityLog = otherTask.HealthProgress.ActivityLog
                    .Where(a => a.TimeStamp.ToString() != dateTimeAddedFor.ToString())
                    .ToArray();
                newActivity.TimeStamp = DateTime.UtcNow;
            }

            newActivity.UnitsAdded = unitsAdded;
            otherTask.HealthProgress.ActivityLog = otherTask.HealthProgress.ActivityLog.Append(newActivity).ToArray();
            otherTask.HealthProgress.TotalUnits += unitsAdded;

            if (criteria?.HealthCriteria?.HealthTaskType == nameof(HealthTaskType.OTHER))
            {
                if (criteria.HealthCriteria.RequiredUnits <= otherTask.HealthProgress.TotalUnits)
                {
                    consumerTask.TaskStatus = Constants.Completed;
                }

                return JsonConvert.SerializeObject(otherTask, _settings);
            }
            else if (criteria?.SelfReportType?.ToLower() == Constant.UIComponentReportType.ToLower()
                && criteria?.HealthCriteria?.UiComponent != null)
            {
                if (request.HealthReport != null)
                {
                    otherTask.HealthProgress.HealthReport?.AddRange(request.HealthReport);
                    if (ValidateUIComponent(criteria.HealthCriteria.UiComponent, request.HealthReport))
                    {
                        if (criteria.HealthCriteria.IsDialerRequired && criteria.HealthCriteria.RequiredUnits <= otherTask.HealthProgress.TotalUnits)
                        { consumerTask.TaskStatus = Constants.Completed; }
                        else if (!criteria.HealthCriteria.IsDialerRequired)
                        {
                            consumerTask.TaskStatus = Constants.Completed;

                        }
                    }
                }
                return JsonConvert.SerializeObject(otherTask, _settings);
            }

            return JsonConvert.SerializeObject(otherTask, _settings);
        }
        private string UpdateHydrationConsumerTask(string? existingDetail, string healthTaskType, UpdateHealthTaskProgressRequestDto request, TrackingDto newActivity, TaskCompletionCriteriaJson? criteria, ref ConsumerTaskModel consumerTask)
        {
            var hydration = string.IsNullOrWhiteSpace(existingDetail)
                ? new HealthProgressDetails<HydrationRollupDataDto>
                {
                    DetailType = nameof(HealthTaskType.HYDRATION),
                    HealthProgress = new HydrationRollupDataDto
                    {
                        TotalDays = 0
                    }
                }
                : JsonConvert.DeserializeObject<HealthProgressDetails<HydrationRollupDataDto>>(existingDetail, _settings);

            if (hydration == null)
            {
                hydration = new HealthProgressDetails<HydrationRollupDataDto>
                {
                    DetailType = nameof(HealthTaskType.HYDRATION),
                    HealthProgress = new HydrationRollupDataDto
                    {
                        TotalDays = 0
                    }
                };
            }

            hydration.HealthProgress.TotalDays += request.NumberOfDays ?? 0;
            newActivity.UnitsAdded = request.NumberOfDays ?? 0;

            var unitsAdded = request.NumberOfUnits ?? 0;
            var dateTimeAddedFor = request.DateTimeAddedFor ?? DateTime.UtcNow;

            // Check if an activity with the same timestamp already exists
            if (hydration.HealthProgress.ActivityLog.Any(a => a.TimeStamp.ToString() == dateTimeAddedFor.ToString()))
            {
                var existingActivity = hydration.HealthProgress.ActivityLog.First(a => a.TimeStamp.ToString() == dateTimeAddedFor.ToString());

                hydration.HealthProgress.TotalDays -= existingActivity.UnitsAdded;
                // Remove the matched activity from ActivityLog
                hydration.HealthProgress.ActivityLog = hydration.HealthProgress.ActivityLog
                    .Where(a => a.TimeStamp.ToString() != dateTimeAddedFor.ToString())
                    .ToArray();
                newActivity.TimeStamp = DateTime.UtcNow;
            }
            hydration.HealthProgress.ActivityLog = hydration.HealthProgress.ActivityLog.Append(newActivity).ToArray();

            if (criteria?.HealthCriteria?.HealthTaskType == nameof(HealthTaskType.HYDRATION) &&
                criteria.HealthCriteria.RequiredDays <= hydration.HealthProgress.TotalDays)
            {
                consumerTask.TaskStatus = Constants.Completed;
            }

            return JsonConvert.SerializeObject(hydration, _settings);
        }

        private bool ValidateUIComponent(List<UiComponent> components, List<HealthTrackingDto> healthReport)
        {
            var methodName = nameof(UpdateHealthTaskProgress);
            bool isValid = true;
            foreach (var component in components)
            {
                var healthReportDetails = healthReport?
      .SelectMany(x => x.HealthReportData ?? new List<HealthTrackingDetailDto>());

                // Get the matching HealthReportValue by HealthReportType
                var healthReportValue = healthReportDetails?
                    .FirstOrDefault(hr => hr.HealthReportType == component.EnUSReportLabel)
                    ?.HealthReportValue;
                if (component.IsRequiredField && string.IsNullOrWhiteSpace(healthReportValue))
                {
                    _consumerTaskServiceLogger.LogError("{className}.{methodName}: Task Ui Component validation failed.  Error Code:{errorCode}", className, methodName, StatusCodes.Status404NotFound);
                    isValid = false;
                }
            }

            return isValid;
        }

        private ConsumerHealthTaskResponseUpdateDto NotFoundResponse(string message)
        {
            return new ConsumerHealthTaskResponseUpdateDto
            {
                ErrorCode = StatusCodes.Status404NotFound,
                ErrorMessage = message
            };
        }

    }
}
