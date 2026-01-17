using AutoMapper;
using FluentNHibernate.Conventions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.Common.Core.Extensions;
using SunnyRewards.Helios.Common.Core.Services;
using SunnyRewards.Helios.Task.Core.Domain.Constants;
using SunnyRewards.Helios.Task.Core.Domain.Dtos;
using SunnyRewards.Helios.Task.Core.Domain.Models;
using SunnyRewards.Helios.Task.Infrastructure.Repositories.Interfaces;
using SunnyRewards.Helios.Task.Infrastructure.Services.Interface;
using static SunnyRewards.Helios.Task.Core.Domain.Dtos.TaskRewardDto;

namespace SunnyRewards.Helios.Task.Infrastructure.Services
{
    public class TaskService : BaseService, ITaskService
    {
        private readonly ILogger<TaskService> _taskLogger;
        private readonly IMapper _mapper;
        private readonly ITaskRepo _taskRepo;
        private readonly ITaskRewardRepo _taskRewardRepo;
        private readonly ITaskDetailRepo _taskDetailRepo;
        private readonly ITermsOfServiceRepo _termsOfServiceRepo;
        private readonly ITenantTaskCategoryRepo _tenantTaskCategoryRepo;
        private readonly ITaskTypeRepo _taskTypeRepo;
        private readonly ITaskCategoryRepo _taskCategoryRepo;
        private readonly ITaskRewardTypeRepo _taskRewardTypeRepo;
        private readonly IConsumerTaskRepo _consumerTaskRepo;
        private readonly ISubTaskRepo _subTaskRepo;
        private readonly ITaskExternalMappingRepo _taskExternalMappingRepo;
        private readonly ITriviaRepo _triviaRepo;
        private readonly ITriviaQuestionGroupRepo _triviaQuestionGroupRepo;
        private readonly ITriviaQuestionRepo _triviaQuestionRepo;
        private readonly IQuestionnaireRepo _questionnaireRepo;
        private readonly IQuestionnaireQuestionGroupRepo _questionnaireQuestionGroupRepo;
        private readonly IQuestionnaireQuestionRepo _questionnaireQuestionRepo;
        const string className = nameof(TaskService);

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

        public TaskService(
            ILogger<TaskService> taskLogger,
            IMapper mapper,
            ITaskRepo taskRepo,
            ITaskRewardRepo taskRewardRepo,
            ITaskDetailRepo taskDetailRepo,
            ITermsOfServiceRepo termsOfServiceRepo,
            ITenantTaskCategoryRepo tenantTaskCategoryRepo,
            ITaskTypeRepo taskTypeRepo,
            ITaskRewardTypeRepo taskRewardTypeRepo,
            IConsumerTaskRepo consumerTaskRepo,
            ISubTaskRepo subTaskRepo,
            ITaskExternalMappingRepo taskExternalMappingRepo,
            ITriviaRepo triviaRepo,
            ITriviaQuestionGroupRepo triviaQuestionGroupRepo,
            ITriviaQuestionRepo triviaQuestionRepo,
            ITaskCategoryRepo taskCategoryRepo,
            IQuestionnaireRepo questionnaireRepo,
            IQuestionnaireQuestionGroupRepo questionnaireQuestionGroupRepo,
            IQuestionnaireQuestionRepo questionnaireQuestionRepo)

        {
            _taskLogger = taskLogger;
            _mapper = mapper;
            _taskRepo = taskRepo;
            _taskRewardRepo = taskRewardRepo;
            _taskDetailRepo = taskDetailRepo;
            _termsOfServiceRepo = termsOfServiceRepo;
            _tenantTaskCategoryRepo = tenantTaskCategoryRepo;
            _taskTypeRepo = taskTypeRepo;
            _taskRewardTypeRepo = taskRewardTypeRepo;
            _consumerTaskRepo = consumerTaskRepo;
            _subTaskRepo = subTaskRepo;
            _taskExternalMappingRepo = taskExternalMappingRepo;
            _triviaRepo = triviaRepo;
            _triviaQuestionGroupRepo = triviaQuestionGroupRepo;
            _triviaQuestionRepo = triviaQuestionRepo;
            _taskCategoryRepo = taskCategoryRepo;
            _questionnaireRepo = questionnaireRepo;
            _questionnaireQuestionGroupRepo = questionnaireQuestionGroupRepo;
            _questionnaireQuestionRepo = questionnaireQuestionRepo;
        }

        /// <summary>
        /// Retrieves a list of tasks from the repository and returns them in a standardized response format.
        /// </summary>
        /// <returns></returns>
        public async Task<TasksResponseDto> GetTasksAsync()
        {
            const string methodName = nameof(GetTasksAsync);
            try
            {
                var result = await _taskRepo.FindAsync(x => x.DeleteNbr == 0);

                if (result == null || result.Count == 0)
                {
                    _taskLogger.LogError("{ClassName}.{MethodName}: No task was found. Error Code: {ErrorCode}", className, methodName, StatusCodes.Status404NotFound);
                    return new TasksResponseDto
                    {
                        ErrorMessage = "No task was found."
                    };
                }

                return new TasksResponseDto
                {
                    Tasks = _mapper.Map<IList<TaskDto>>(result)
                };
            }
            catch (Exception ex)
            {
                _taskLogger.LogError(ex, "{ClassName}.{MethodName}: Error Message: {Message}, Error Code: {ErrorCode}", className, methodName, ex.Message, StatusCodes.Status500InternalServerError);

                return new TasksResponseDto
                {
                    ErrorCode = StatusCodes.Status500InternalServerError,
                    ErrorMessage = ex.Message
                };
            }
        }

        /// <summary>
        /// Create task
        /// </summary>
        /// <param name="createTaskRequest">The create task request.</param>
        /// <returns></returns>
        public async Task<BaseResponseDto> CreateTask(CreateTaskRequestDto taskRequestDto)
        {
            const string methodName = nameof(CreateTask);
            try
            {
                _taskLogger.LogInformation("{ClassName}:{MethodName}: Fetching tasks started for Task Code: {TaskCode}",
                    className, methodName, taskRequestDto.TaskCode);
                var tasks = await _taskRepo.FindAsync(x => x.TaskName == taskRequestDto.TaskName && x.DeleteNbr == 0);

                var taskWithSameTaskCode = tasks?.FirstOrDefault(x => x.TaskCode == taskRequestDto.TaskCode);
                var taskWithOtherCode = tasks?.FirstOrDefault(x => x.TaskCode != taskRequestDto.TaskCode);

                if (taskWithSameTaskCode != null)
                {
                    return new BaseResponseDto() { ErrorCode = StatusCodes.Status409Conflict, ErrorMessage = $"Task is already Existed with Task Code: {taskRequestDto.TaskCode}" };
                }
                if (taskWithOtherCode != null)
                {
                    return new BaseResponseDto() { ErrorCode = StatusCodes.Status422UnprocessableEntity, ErrorMessage = $"Task Name is Existed but Task Code is different: {taskRequestDto.TaskCode} and " };
                }
                var taskModel = _mapper.Map<TaskModel>(taskRequestDto);
                taskModel.CreateTs = DateTime.UtcNow;
                taskModel.DeleteNbr = 0;
                taskModel.TaskId = 0;
                await _taskRepo.CreateAsync(taskModel);
                _taskLogger.LogInformation("{ClassName}.{MethodName}: Task created Successfully. Task Code:{TaskCode}", className, methodName, taskRequestDto.TaskCode);
                return new BaseResponseDto();
            }
            catch (Exception ex)
            {
                _taskLogger.LogError(ex, "{ClassName}:{MethodName}: Error Creating tasks for Task Code: {TaskCode}", className, methodName, taskRequestDto.TaskCode);
                throw;
            }
        }

        /// <summary>
        /// UpdateTaskAsync
        /// </summary>
        /// <param name="taskId"></param>
        /// <param name="taskRequestDto"></param>
        /// <returns></returns>
        public async Task<TaskResponseDto> UpdateTaskAsync(long taskId, TaskRequestDto taskRequestDto)
        {
            const string methodName = nameof(UpdateTaskAsync);
            try
            {
                _taskLogger.LogInformation("{ClassName}:{MethodName}: Started processing for TaskId: {TaskId}", className, methodName, taskId);

                var taskModel = await _taskRepo.FindOneAsync(x => x.TaskId == taskId && x.DeleteNbr == 0);

                if (taskModel == null)
                {
                    return new TaskResponseDto() { Task = _mapper.Map<TaskDto>(taskRequestDto), ErrorCode = StatusCodes.Status404NotFound, ErrorMessage = $"No task found for given TaskId: {taskId}" };
                }

                taskModel.TaskTypeId = taskRequestDto.TaskTypeId;
                taskModel.TaskCode = taskRequestDto.TaskCode;
                taskModel.TaskName = taskRequestDto.TaskName;
                taskModel.SelfReport = taskRequestDto.SelfReport;
                taskModel.ConfirmReport = taskRequestDto.ConfirmReport;
                taskModel.TaskCategoryId = taskRequestDto.TaskCategoryId;
                taskModel.IsSubtask = taskRequestDto.IsSubtask;
                taskModel.UpdateUser = taskRequestDto.UpdateUser ?? Constant.SystemUser;
                taskModel.UpdateTs = DateTime.UtcNow;
                await _taskRepo.UpdateAsync(taskModel);

                _taskLogger.LogInformation("{ClassName}.{MethodName}: Ended Successfully.", className, methodName);

                return new TaskResponseDto() { Task = _mapper.Map<TaskDto>(taskModel) };
            }
            catch (Exception ex)
            {
                _taskLogger.LogError(ex, "{ClassName}.{MethodName}: Error Message: {Message}, Error Code: {ErrorCode}", className, methodName, ex.Message, StatusCodes.Status500InternalServerError);
                return new TaskResponseDto() { Task = _mapper.Map<TaskDto>(taskRequestDto), ErrorCode = StatusCodes.Status409Conflict, ErrorMessage = ex.Message };
            }
        }
        public async Task<TaskResponseDto> UpdateImportTaskAsync(long taskId, TaskRequestDto taskRequestDto)
        {
            const string methodName = nameof(UpdateImportTaskAsync);
            try
            {
                _taskLogger.LogInformation("{ClassName}:{MethodName}: Started processing for TaskId: {TaskId}", className, methodName, taskId);

                var taskModelvalue = await _taskRepo.FindOneAsync(x => x.TaskId == taskId && x.DeleteNbr == 0);

                if (taskModelvalue == null)
                {
                    return new TaskResponseDto() { Task = _mapper.Map<TaskDto>(taskRequestDto), ErrorCode = StatusCodes.Status404NotFound, ErrorMessage = $"No task found for given TaskId: {taskId}" };
                }

                taskModelvalue.TaskTypeId = taskRequestDto.TaskTypeId;
                taskModelvalue.TaskName = taskRequestDto.TaskName;
                taskModelvalue.SelfReport = taskRequestDto.SelfReport;
                taskModelvalue.ConfirmReport = taskRequestDto.ConfirmReport;
                taskModelvalue.TaskCategoryId = taskRequestDto.TaskCategoryId;
                taskModelvalue.IsSubtask = taskRequestDto.IsSubtask;

                taskModelvalue.UpdateTs = DateTime.UtcNow;
                taskModelvalue.UpdateUser = Constant.ImportUser;
                await _taskRepo.UpdateAsync(taskModelvalue);

                _taskLogger.LogInformation("{ClassName}.{MethodName}: Ended Successfully.", className, methodName);

                return new TaskResponseDto() { Task = _mapper.Map<TaskDto>(taskModelvalue) };
            }
            catch (Exception ex)
            {
                _taskLogger.LogError(ex, "{ClassName}.{MethodName}: Error Message: {Message}, Error Code: {ErrorCode}", className, methodName, ex.Message, StatusCodes.Status500InternalServerError);
                return new TaskResponseDto() { Task = _mapper.Map<TaskDto>(taskRequestDto), ErrorCode = StatusCodes.Status409Conflict, ErrorMessage = ex.Message };
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="taskId"></param>
        /// <returns></returns>
        public async Task<TaskDto> GetTaskData(long taskId)
        {
            const string methodName = nameof(GetTaskData);
            try
            {
                var task = await _taskRepo.FindOneAsync(x => x.TaskId == taskId && x.DeleteNbr == 0);
                if (task != null)
                {
                    var response = _mapper.Map<TaskDto>(task);
                    var taskDetail = await _taskDetailRepo.FindOneAsync(x => x.TaskId == task.TaskId && x.DeleteNbr == 0);
                    var termsOfService = await _termsOfServiceRepo.FindOneAsync(x => x.TermsOfServiceId == taskDetail.TermsOfServiceId && x.DeleteNbr == 0);
                    _taskLogger.LogInformation("{className}.{methodName}: successfully retrieved data from  TaskId API for TaskId: {taskId}", className, methodName, taskId);
                    return response;
                }
                return new TaskDto();
            }
            catch (Exception ex)
            {
                _taskLogger.LogError(ex, "{className}.{methodName}: ERROR Msg:{msg}, Error Code:{errorCode}", className, methodName, ex.Message, StatusCodes.Status500InternalServerError);
                return new TaskDto();
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="taskRewardRequestDto"></param>
        /// <returns></returns>
        public async Task<GetTaskRewardResponseDto> GetTasksByTaskRewardCode(GetTaskRewardRequestDto taskRewardRequestDto)
        {
            const string methodName = nameof(GetTasksByTaskRewardCode);
            var taskRewardResponseDto = new GetTaskRewardResponseDto();
            DateTime tsNow = DateTime.UtcNow;
            try
            {
                foreach (var taskRewardCode in taskRewardRequestDto.TaskRewardCodes)
                {
                    var taskRewards = await _taskRewardRepo.FindOneAsync(x => x.TaskRewardCode == taskRewardCode && x.DeleteNbr == 0);

                    if (tsNow < taskRewards.ValidStartTs || tsNow > taskRewards.Expiry)
                    {
                        continue;
                    }
                    var taskRewardDto = _mapper.Map<TaskRewardDto>(taskRewards);

                    var taskData = await _taskRepo.FindOneAsync(x => x.TaskId == taskRewards.TaskId && x.IsSubtask != true && x.DeleteNbr == 0);

                    if (taskData == null)
                    {
                        continue;
                    }
                    var taskDto = _mapper.Map<TaskDto>(taskData);
                    var requestedLanguageCode = string.IsNullOrWhiteSpace(taskRewardRequestDto?.LanguageCode) ? Constant.LanguageCode.ToLower() : taskRewardRequestDto?.LanguageCode.ToLower();
                    var taskDetail = await _taskDetailRepo.FindOneAsync(x => x.TaskId == taskDto.TaskId &&
                        x.TenantCode == taskRewardDto.TenantCode && x.LanguageCode != null && x.LanguageCode.ToLower() == requestedLanguageCode &&
                        x.DeleteNbr == 0);
                    if (taskDetail == null && requestedLanguageCode != Constant.LanguageCode.ToLower())
                    {
                        taskDetail = await _taskDetailRepo.FindOneAsync(x => x.TaskId == taskDto.TaskId &&
                        x.TenantCode == taskRewardDto.TenantCode && x.LanguageCode != null && x.LanguageCode.ToLower() == Constant.LanguageCode.ToLower() &&
                        x.DeleteNbr == 0);
                    }

                    var taskDetailDto = _mapper.Map<TaskDetailDto>(taskDetail);
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
                        TaskReward = taskRewardDto,
                        Task = taskDto,
                        TaskDetail = taskDetailDto,
                        TermsOfService = termsOfServiceDto,
                        TenantTaskCategory = tenantTaskCategoryDto,
                        TaskType = taskTypeDto,
                        RewardTypeName = taskRewardType?.RewardTypeName,
                    };
                    taskRewardResponseDto.TaskRewardDetails?.Add(taskRewardDetailDto);
                }

                _taskLogger.LogInformation("{className}.{methodName}: successfully retrieved data from  GetTasksByTaskRewardCode API for TaskRewardCodes: {TaskRewardCodes}", className, methodName, taskRewardRequestDto.TaskRewardCodes);
                return taskRewardResponseDto;
            }
            catch (Exception ex)
            {
                _taskLogger.LogError(ex, "{className}.{methodName}: ERROR Msg:{msg}, Error Code:{errorCode}", className, methodName, ex.Message, StatusCodes.Status500InternalServerError);
                return new GetTaskRewardResponseDto();
            }
        }

        public async Task<ExportTaskResponseDto> GetTaskExport(ExportTaskRequestDto exportTaskRequestDto)
        {
            const string methodName = nameof(GetTaskExport);
            try
            {
                _taskLogger.LogInformation("{ClassName}:{MethodName}: Fetching tasks started for TenantCode: {TenantCode}",
                    className, methodName, exportTaskRequestDto.TenantCode);

                var tenantTaskCategories = await _tenantTaskCategoryRepo.FindAsync(x => x.TenantCode == exportTaskRequestDto.TenantCode && x.DeleteNbr == 0);
                var tenantTaskCategoryIds = tenantTaskCategories.Select(x => x.TaskCategoryId).Distinct();
                var tenantCategoryTypesTask = await _taskCategoryRepo.FindAsync(x => tenantTaskCategoryIds.Contains(x.TaskCategoryId) && x.DeleteNbr == 0);
                List<ExportTenantTaskCategoryDto> tenantTaskCategoryDtos = new List<ExportTenantTaskCategoryDto>();
                if (tenantCategoryTypesTask != null && tenantCategoryTypesTask.Count > 0)
                {
                    tenantTaskCategoryDtos = tenantTaskCategories
                      .Select(tenantCategory => new ExportTenantTaskCategoryDto
                      {
                          TenantTaskCategory = _mapper.Map<TenantTaskCategoryDto>(tenantCategory),
                          TaskCategoryCode = tenantCategoryTypesTask.Where(x => x.TaskCategoryId == tenantCategory.TaskCategoryId).FirstOrDefault()?.TaskCategoryCode ?? null,

                      })
                         .OrderBy(x => x.TenantTaskCategory?.TenantTaskCategoryId) // Sorting before materializing the list
                          .ToList();
                }

                var taskDetails = await _taskDetailRepo.FindAsync(x => x.TenantCode == exportTaskRequestDto.TenantCode && x.DeleteNbr == 0);
                var taskIds = taskDetails.Select(x => x.TaskId).Distinct();
                var tasks = await _taskRepo.FindAsync(x => taskIds.Contains(x.TaskId) && x.DeleteNbr == 0);

                List<ExportTaskDto> exportTaskDto = new List<ExportTaskDto>();

                var taskRewards = await _taskRewardRepo.FindAsync(x => x.TenantCode == exportTaskRequestDto.TenantCode && x.DeleteNbr == 0);
                //  Filter tasks that have at least one task reward
                var validTaskIdsWithRewards = taskRewards?.Select(tr => tr.TaskId).Distinct().ToHashSet() ?? new HashSet<long>();

                if (tasks != null)
                {
                    // Only include tasks that have task rewards
                    var filteredTasks = tasks.Where(t => validTaskIdsWithRewards.Contains(t.TaskId)).ToList();

                    var taskTypeIds = filteredTasks.Select(x => x.TaskTypeId).Distinct();
                    var taskCategoryIds = filteredTasks.Select(x => x.TaskCategoryId).Distinct();

                    var taskTypesTask = await _taskTypeRepo.FindAsync(x => taskTypeIds.Contains(x.TaskTypeId) && x.DeleteNbr == 0);
                    var categoryTypesTask = await _taskCategoryRepo.FindAsync(x => taskCategoryIds.Contains(x.TaskCategoryId) && x.DeleteNbr == 0);

                    var taskTypes = taskTypesTask.ToDictionary(x => x.TaskTypeId);
                    var categoryTypes = categoryTypesTask.ToDictionary(x => x.TaskCategoryId);
                    if (taskTypes != null && taskTypes.Count > 0)
                    {
                        exportTaskDto = filteredTasks
                       .Select(task => new ExportTaskDto
                       {
                           Task = _mapper.Map<TaskDto>(task),
                           TaskTypeCode = taskTypes.TryGetValue(task.TaskTypeId, out var taskType) ? taskType.TaskTypeCode : null,
                           TaskCategoryCode = task.TaskCategoryId.HasValue && categoryTypes.TryGetValue(task.TaskCategoryId.Value, out var categoryType)
                               ? categoryType.TaskCategoryCode
                               : null // Includes tasks with null TaskCategoryId
                       })
                       .OrderBy(x => x.Task?.TaskName?.ToLower()) // Sorting before materializing the list
                       .ToList();
                    }
                }   

                List<ExportTaskRewardDto> exportTaskRewardDto = new List<ExportTaskRewardDto>();
                if (taskRewards != null)
                {
                    var taskRewardTypeIds = taskRewards.Select(x => x.RewardTypeId).Distinct();

                    var taskRewardTypes = await _taskRewardTypeRepo.FindAsync(x => taskRewardTypeIds.Contains(x.RewardTypeId) && x.DeleteNbr == 0);
                    if (taskRewardTypes != null)
                    {
                        exportTaskRewardDto = taskRewards.Join(taskRewardTypes,
                      taskReward => taskReward.RewardTypeId,
                      taskRewardType => taskRewardType.RewardTypeId,
                      (taskReward, taskRewardType) => new ExportTaskRewardDto
                      {
                          TaskReward = _mapper.Map<TaskRewardDto>(taskReward),
                          TaskRewardTypeCode = taskRewardType.RewardTypeCode,
                      }).OrderBy(x => x.TaskReward?.TaskRewardId).ToList();
                    }
                }
                var taskExternalMappings = await _taskExternalMappingRepo.FindAsync(x => x.TenantCode == exportTaskRequestDto.TenantCode && x.DeleteNbr == 0);

                var tosIds = taskDetails.Select(td => td.TermsOfServiceId).Distinct();
                var taskRewardIds = taskRewards?.Select(x => x.TaskRewardId).Distinct();
                var termsOfService = await _termsOfServiceRepo.FindAsync(x => tosIds.Contains(x.TermsOfServiceId) && x.DeleteNbr == 0);
                var subtasks = await _subTaskRepo.FindAsync(x => taskRewardIds.Contains(x.ParentTaskRewardId) && x.DeleteNbr == 0);

                var trivia = await _triviaRepo.FindAsync(x => taskRewardIds.Contains(x.TaskRewardId));
                var triviaIds = trivia?.Select(t => t.TriviaId).Distinct();
                var triviaQuestionGroups = await _triviaQuestionGroupRepo.FindAsync(x => triviaIds!.Contains(x.TriviaId) && x.DeleteNbr == 0);

                var triviaQuestionIds = triviaQuestionGroups.Select(x => x.TriviaQuestionId).Distinct();
                var triviaQuestions = await _triviaQuestionRepo.FindAsync(x => triviaQuestionIds.Contains(x.TriviaQuestionId) && x.DeleteNbr == 0);
                _taskLogger.LogInformation("{ClassName}:{MethodName}: Successfully fetched cohorts and task rewards for TenantCode: {TenantCode}",
                    className, methodName, exportTaskRequestDto.TenantCode);
                var taskRewardDto = _mapper.Map<List<TaskRewardDto>>(taskRewards?.OrderBy(x => x.TaskRewardId));

                // Including Questionnaire Data into the export
                var questionnaires = await _questionnaireRepo.FindAsync(x => taskRewardIds.Contains(x.TaskRewardId) && x.DeleteNbr == 0);
                var questionnaireIds = questionnaires?.Select(q => q.QuestionnaireId).Distinct();
                var questionnaireQuestionGroups = await _questionnaireQuestionGroupRepo.FindAsync(x => questionnaireIds!.Contains(x.QuestionnaireId) && x.DeleteNbr == 0);
                var questionnaireQuestionIds = questionnaireQuestionGroups.Select(x => x.QuestionnaireQuestionId).Distinct();
                var questionnaireQuestions = await _questionnaireQuestionRepo.FindAsync(x => questionnaireQuestionIds.Contains(x.QuestionnaireQuestionId) && x.DeleteNbr == 0);
               

                List<ExportTriviaDto> exportTriviaDto = new List<ExportTriviaDto>();
                if (trivia != null && taskRewards != null)
                {
                    exportTriviaDto = trivia.Join(taskRewards,
                  tri => tri.TaskRewardId,
                  reward => reward.TaskRewardId,
                  (tri, reward) => new ExportTriviaDto
                  {
                      Trivia = _mapper.Map<TriviaDto>(tri),
                      TaskExternalCode = reward.TaskExternalCode,
                  }).OrderBy(x => x.Trivia?.TriviaId).ToList();
                }

                // Mapping Questionnaire Data
                List<ExportQuestionnaireDto> exportQuestionnaireDto = new List<ExportQuestionnaireDto>();
                if (questionnaires != null && taskRewards != null)
                {
                    exportQuestionnaireDto = questionnaires.Join(taskRewards,
                  ques => ques.TaskRewardId,
                  reward => reward.TaskRewardId,
                  (ques, reward) => new ExportQuestionnaireDto
                  {
                      Questionnaire = _mapper.Map<QuestionnaireDto>(ques),
                      TaskExternalCode = reward.TaskExternalCode,
                  }).OrderBy(x => x.Questionnaire?.QuestionnaireId).ToList();
                }

                return new ExportTaskResponseDto()
                {
                    Task = exportTaskDto,
                    TenantTaskCategory = tenantTaskCategoryDtos,
                    TaskDetail = _mapper.Map<List<TaskDetailDto>>(taskDetails?.OrderBy(x => x.TaskDetailId)),
                    TaskReward = exportTaskRewardDto,
                    TaskExternalMapping = _mapper.Map<List<TaskExternalMappingDto>>(taskExternalMappings?.OrderBy(x => x.TaskExternalMappingId)),
                    TermsOfService = _mapper.Map<List<TermsOfServiceDto>>(termsOfService?.OrderBy(x => x.TermsOfServiceId)),
                    SubTask = _mapper.Map<List<SubTaskDto>>(subtasks?.OrderBy(x => x.SubTaskId)),
                    Trivia = exportTriviaDto,
                    TriviaQuestionGroup = _mapper.Map<List<TriviaQuestionGroupDto>>(triviaQuestionGroups?.OrderBy(x => x.TriviaQuestionGroupId)),
                    TriviaQuestion = _mapper.Map<List<TriviaQuestionDto>>(triviaQuestions?.OrderBy(x => x.TriviaQuestionId)),
                    Questionnaire = exportQuestionnaireDto,
                    QuestionnaireQuestionGroup = _mapper.Map<List<QuestionnaireQuestionGroupDto>>(questionnaireQuestionGroups?.OrderBy(x => x.QuestionnaireQuestionGroupId)),
                    QuestionnaireQuestion = _mapper.Map<List<QuestionnaireQuestionDto>>(questionnaireQuestions?.OrderBy(x => x.QuestionnaireQuestionId))
                };
            }
            catch (Exception ex)
            {
                _taskLogger.LogError(ex, "{ClassName}:{MethodName}: Error occurred while fetching cohort export. TenantCode: {TenantCode}, ErrorMessage: {ErrorMessage}",
                    className, methodName, exportTaskRequestDto.TenantCode, ex.Message);
                throw;
            }
        }

        public async Task<BaseResponseDto> CreateTaskExternalMapping(TaskExternalMappingRequestDto requestDto)
        {
            const string methodName = nameof(CreateTaskExternalMapping);
            try
            {
                if (requestDto == null)
                {
                    _taskLogger.LogError("{className}.{methodName}: Failed to Saved data for  Task External Mapping API for request: {requestDto}", className, methodName, requestDto?.ToJson());
                    return new BaseResponseDto { ErrorCode = StatusCodes.Status404NotFound, ErrorMessage = "Task External Mapping Not Found" };
                }


                var existingTaskExternalMappings = await _taskExternalMappingRepo.FindOneAsync(x => x.TenantCode == requestDto.TenantCode && x.TaskThirdPartyCode == requestDto.TaskThirdPartyCode && x.DeleteNbr == 0);
                if (existingTaskExternalMappings != null)
                {
                    _taskLogger.LogInformation("{className}.{methodName}: record exists for request: {requestDto}", className, methodName, requestDto.ToJson());
                    return new BaseResponseDto { ErrorCode = StatusCodes.Status409Conflict, ErrorMessage = "Task External Mapping already exists" };
                }

                TaskExternalMappingModel TaskExternalMapping = new TaskExternalMappingModel();
                TaskExternalMapping = _mapper.Map<TaskExternalMappingModel>(requestDto);
                TaskExternalMapping.CreateTs = DateTime.Now;
                TaskExternalMapping.CreateUser = requestDto.CreateUser ?? Constant.SystemUser;
                _taskLogger.LogInformation("{className}.{methodName}: Successfully Saved data for Task External Mapping  for request: {requestDto}", className, methodName, requestDto.ToJson());
                TaskExternalMapping = await _taskExternalMappingRepo.CreateAsync(TaskExternalMapping);
                if (TaskExternalMapping.TaskExternalMappingId > 0)
                {
                    _taskLogger.LogInformation("{className}.{methodName}: Successfully Saved data for  Task External Mapping API for request: {requestDto}", className, methodName, requestDto.ToJson());
                    return new BaseResponseDto();
                }
                else
                {
                    _taskLogger.LogError("{className}.{methodName}: Failed to Saved data for  Task External Mapping API for request: {requestDto}", className, methodName, requestDto.ToJson());
                    return new BaseResponseDto { ErrorCode = StatusCodes.Status404NotFound, ErrorMessage = "Task External Mapping Not Created" };
                }

            }
            catch (Exception ex)
            {
                _taskLogger.LogError(ex, "{className}.{methodName}: ERROR - msg : {msg}, for requestDto: {RequestDto}", className, methodName, ex.Message, requestDto.ToJson());
                return new BaseResponseDto { ErrorCode = StatusCodes.Status500InternalServerError, ErrorMessage = "Task External Mapping Not Created" };

            }

        }


        /// <summary>
        /// Retrieves a task by its task name from the repository.
        /// </summary>
        /// <param name="getTaskByTaskNameRequestDto">The DTO containing the task name used to fetch the task.</param>
        /// <returns>
        /// A <see cref="GetTaskByTaskNameResponseDto"/> containing the <see cref="TaskDto"/> if the task is found; 
        /// otherwise, returns a <see cref="GetTaskByTaskNameResponseDto"/> with an error code and message indicating "Not Found."
        /// </returns>
        public async Task<GetTaskByTaskNameResponseDto> GetTaskByTaskName(GetTaskByTaskNameRequestDto getTaskByTaskNameRequestDto)
        {
            const string methodName = nameof(GetTaskByTaskName);
            try
            {
                _taskLogger.LogError("{ClassName}.{MethodName}: Started processing get task with taskname:{name}", className, methodName, getTaskByTaskNameRequestDto.TaskName);
                var taskName = getTaskByTaskNameRequestDto.TaskName?.Trim().ToLower();
                var tasksList = await _taskRepo.FindAsync(x => x.TaskName != null && x.TaskName.Trim().ToLower() == taskName && x.DeleteNbr == 0);
                if (tasksList == null)
                {
                    _taskLogger.LogError("{ClassName}.{MethodName}: Task not found with taskname:{name},ErrorCode:{Code}", className, methodName, getTaskByTaskNameRequestDto.TaskName, StatusCodes.Status404NotFound);
                    return new GetTaskByTaskNameResponseDto() { ErrorCode = StatusCodes.Status404NotFound, ErrorMessage = $"Task not found with taskname:{getTaskByTaskNameRequestDto.TaskName}" };
                }
                var task = tasksList.OrderBy(x => x.TaskId).FirstOrDefault();
                var taskDto = _mapper.Map<TaskDto>(task);
                return new GetTaskByTaskNameResponseDto() { TaskDto = taskDto };
            }
            catch (Exception ex)
            {
                _taskLogger.LogError(ex, "{ClassName}.{MethodName}: Error occured while fetching task with taskName:{name}, ERROR - msg : {msg}", className, methodName, getTaskByTaskNameRequestDto.TaskName, ex.Message);
                throw;
            }
        }
    }
}
