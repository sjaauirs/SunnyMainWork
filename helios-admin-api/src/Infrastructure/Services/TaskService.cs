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
    public class TaskService : ITaskService
    {
        public readonly ILogger<TaskService> _logger;
        private readonly IMapper _mapper;
        public readonly ITaskClient _taskClient;
        public const string className = nameof(TaskService);

        public TaskService(ILogger<TaskService> logger, IMapper mapper, ITaskClient taskClient)
        {
            _logger = logger;
            _mapper = mapper;
            _taskClient = taskClient;
        }

        /// <summary>
        /// Retrieves a list of tasks from the tasks API and returns them in a standardized response format.
        /// </summary>
        /// <returns></returns>
        public async Task<TasksResponseDto> GetTasksAsync()
        {
            const string methodName = nameof(GetTasksAsync);
            try
            {
                return await _taskClient.Get<TasksResponseDto>(Constant.TasksApiUrl, new Dictionary<string, long>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName}: Error Message: {Message}, Error Code: {ErrorCode}", className, methodName, ex.Message, StatusCodes.Status500InternalServerError);

                return new TasksResponseDto
                {
                    ErrorCode = StatusCodes.Status500InternalServerError,
                    ErrorMessage = ex.Message
                };
            }
        }

        /// <summary>
        /// Creates a new task
        /// </summary>
        /// <param name="createTaskRequestDto">The request data for creating task</param>
        /// <returns></returns>
        public async Task<BaseResponseDto> CreateTask(CreateTaskRequestDto createTaskRequestDto)
        {
            const string methodName = nameof(CreateTask);
            try
            {
                _logger.LogInformation("{ClassName}.{MethodName}: Create Task process started for TaskCode: {TaskCode}", className, methodName, createTaskRequestDto.TaskCode);

                var taskResponse = await _taskClient.Post<BaseResponseDto>(Constant.CreateTaskAPIUrl, createTaskRequestDto);
                if (taskResponse.ErrorCode != null)
                {
                    _logger.LogWarning("{ClassName}.{MethodName}: Error occurred while creating tenant, TenantCode: {TenantCode}, ErrorCode: {ErrorCode}", className, methodName, createTaskRequestDto.TaskCode, taskResponse.ErrorCode);
                    return taskResponse;
                }
                _logger.LogInformation("{ClassName}.{MethodName}: Task created successfully, TaskCode: {TaskCode}", className, methodName, createTaskRequestDto.TaskCode);
                return new BaseResponseDto();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName}: Exception occurred while creating Task. ErrorMessage: {ErrorMessage}, StackTrace: {StackTrace}", className, methodName, ex.Message, ex.StackTrace);
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
                _logger.LogInformation("{ClassName}:{MethodName}: Started processing...taskId:{taskId}", className, methodName, taskId);

                var taskResponse = await _taskClient.Put<TaskResponseDto>($"{Constant.TaskApiUrl}/{taskId}", taskRequestDto);

                if (taskResponse.ErrorCode != null)
                {
                    _logger.LogError("{ClassName}.{MethodName}: Error processing for taskId: {taskId}, Error Code: {ErrorCode}, Error Message: {ErroMessage}", className, methodName, taskId, taskResponse.ErrorCode, taskResponse.ErrorMessage);
                    return taskResponse;
                }

                _logger.LogInformation("{ClassName}:{MethodName}: Ended processing...taskId:{taskId}", className, methodName, taskId);

                return taskResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}:{MethodName}: Error processing.", className, methodName);
                return new TaskResponseDto() { Task = _mapper.Map<TaskDto>(taskRequestDto), ErrorCode = StatusCodes.Status500InternalServerError, ErrorMessage = ex.Message };
            }
        }

        /// <summary>
        /// Method to call Task API remove-consumer-task for soft delete task 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public BaseResponseDto SoftDeleteTask(dynamic request)
        {
            const string methodName = nameof(SoftDeleteTask);
            try
            {
                var deleteConsumerTaskRequestDto = CreateDeleteConsumerTaskRequestDto(request);
                if (deleteConsumerTaskRequestDto == null)
                {
                    _logger.LogWarning("{ClassName}.{MethodName}: Invalid data for adding Soft Deleting a Task ErrorCode: {ErrorCode}", className, methodName, StatusCodes.Status400BadRequest);
                    return new BaseResponseDto() { ErrorCode = StatusCodes.Status400BadRequest, ErrorMessage = " Invalid data for adding Consumer to Cohort" };
                }
                _logger.LogInformation("{ClassName}.{MethodName}: API Call Soft Delete Task started", className, methodName);

                BaseResponseDto cohortResponse = _taskClient.Post<BaseResponseDto>("remove-consumer-task", deleteConsumerTaskRequestDto).GetAwaiter().GetResult();
                if (cohortResponse.ErrorCode != null)
                {
                    _logger.LogWarning("{ClassName}.{MethodName}: Invalid data for Soft Deleting a Task ErrorCode: {ErrorCode}", className, methodName, cohortResponse.ErrorCode);
                    return cohortResponse;
                }

                _logger.LogInformation("{ClassName}.{MethodName}: Task Soft Deleted successfully", className, methodName);
                return cohortResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName}: Exception occurred while Soft Deleteing Task ErrorMessage: {ErrorMessage}, StackTrace: {StackTrace}", className, methodName, ex.Message, ex.StackTrace);
                return new BaseResponseDto() { ErrorCode = StatusCodes.Status500InternalServerError, ErrorMessage = ex.Message };
            }
        }

        private DeleteConsumerTaskRequestDto? CreateDeleteConsumerTaskRequestDto(dynamic request)
        {
            const string methodName = nameof(CreateDeleteConsumerTaskRequestDto);
            try
            {
                // Attempt to extract properties safely
                string taskExternalCode = request.TaskExternalCode?.ToString();
                string consumerCode = request.ConsumerCode?.ToString();
                string tenantCode = request.TenantCode?.ToString();

                // Validate the extracted values
                if (string.IsNullOrEmpty(taskExternalCode))
                {
                    throw new ArgumentException("TaskExternalCode is required and cannot be null or empty.");
                }

                if (string.IsNullOrEmpty(consumerCode))
                {
                    throw new ArgumentException("ConsumerCode is required and cannot be null or empty.");
                }

                if (string.IsNullOrEmpty(tenantCode))
                {
                    throw new ArgumentException("TenantCode is required and cannot be null or empty.");
                }

                // Create and return the DTO if all fields are valid
                return new DeleteConsumerTaskRequestDto
                {
                    TaskExternalCode = taskExternalCode!,
                    ConsumerCode = consumerCode!,
                    TenantCode = tenantCode!
                };
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName}: Argument Exception occurred. ErrorMessage: {ErrorMessage}", className, methodName, ex.Message);
                return null; // Return null or handle as appropriate
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName}: Exception occurred while creating DeleteConsumerTaskRequestDto. ErrorMessage: {ErrorMessage}, StackTrace: {StackTrace}", className, methodName, ex.Message, ex.StackTrace);
                return null; // Return null or handle as appropriate
            }
        }
        /// <summary>
        /// Fetches a task based on the task name provided in the request DTO by making a call to an external task API.
        /// </summary>
        /// <param name="getTaskByTaskNameRequestDto">
        /// The DTO containing the task name for which the task details are requested.
        /// </param>
        /// <returns>
        /// A <see cref="GetTaskByTaskNameResponseDto"/> containing the task details if the task is found:
        /// - If an error occurs during the API call, returns a response with the appropriate error code.
        /// - If successful, logs the task retrieval and returns an empty <see cref="GetTaskByTaskNameResponseDto"/> if no specific response data is needed.
        /// </returns>
        /// <exception cref="Exception">
        /// Thrown when an unhandled exception occurs during the task fetching process.
        /// </exception>

        public async Task<GetTaskByTaskNameResponseDto> GetTaskByTaskName(GetTaskByTaskNameRequestDto getTaskByTaskNameRequestDto)
        {
            const string methodName = nameof(CreateTask);
            try
            {
                _logger.LogInformation("{ClassName}.{MethodName}: Get Task process started for TaskName: {Name}", className, methodName, getTaskByTaskNameRequestDto.TaskName);

                var taskResponse = await _taskClient.Post<GetTaskByTaskNameResponseDto>(Constant.GetTaskAPIUrl, getTaskByTaskNameRequestDto);
                if (taskResponse.ErrorCode != null)
                {
                    _logger.LogError("{ClassName}.{MethodName}: Error occurred while fetching task,ErrorCode: {ErrorCode}", className, methodName, taskResponse.ErrorCode);
                    return taskResponse;
                }
                _logger.LogInformation("{ClassName}.{MethodName}: Task fetched successfully, TaskName: {Name}", className, methodName, getTaskByTaskNameRequestDto.TaskName);
                return taskResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName}: Exception occurred while fetching Task. ErrorMessage: {ErrorMessage}, StackTrace: {StackTrace}", className, methodName, ex.Message, ex.StackTrace);
                throw;
            }
        }

        /// <summary>
        /// Imports a list of task types into the system by making a POST request to the tasks API.
        /// </summary>
        /// <param name="taskTypes"></param>
        /// <returns></returns>
        public async Task<ImportTaskTypeResponseDto> ImportTaskTypes(List<TaskTypeDto> taskTypes)
        {
            const string methodName = nameof(ImportTaskTypes);
            try
            {
                return await _taskClient.Post<ImportTaskTypeResponseDto>(Constant.ImportTaskTypes, new ImportTaskTypeRequestDto { TaskTypes = taskTypes });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName}: Error Message: {Message}, Error Code: {ErrorCode}", className, methodName, ex.Message, StatusCodes.Status500InternalServerError);

                return new ImportTaskTypeResponseDto
                {
                    ErrorCode = StatusCodes.Status500InternalServerError,
                    ErrorMessage = ex.Message
                };
            }
        }

        /// <summary>
        /// Imports a list of task categories into the system by making a POST request to the tasks API.
        /// </summary>
        /// <param name="taskCategories"></param>
        /// <returns></returns>
        public async Task<ImportTaskCategoryResponseDto> ImportTaskCategories(List<TaskCategoryDto> taskCategories)
        {
            const string methodName = nameof(ImportTaskCategories);
            try
            {
                return await _taskClient.Post<ImportTaskCategoryResponseDto>(Constant.ImportTaskCategories, new ImportTaskCategoryRequestDto { TaskCategories = taskCategories });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName}: Error Message: {Message}, Error Code: {ErrorCode}", className, methodName, ex.Message, StatusCodes.Status500InternalServerError);

                return new ImportTaskCategoryResponseDto
                {
                    ErrorCode = StatusCodes.Status500InternalServerError,
                    ErrorMessage = ex.Message
                };
            }
        }

        /// <summary>
        /// Imports a list of task reward types into the system by making a POST request to the tasks API.
        /// </summary>
        /// <param name="taskRewardTypes"></param>
        /// <returns></returns>
        public async Task<ImportRewardTypeResponseDto> ImportTaskRewardTypes(List<TaskRewardTypeDto> taskRewardTypes)
        {
            const string methodName = nameof(ImportTaskRewardTypes);
            try
            {
                return await _taskClient.Post<ImportRewardTypeResponseDto>(Constant.ImportRewardTypes, new ImportRewardTypeRequestDto { RewardTypes = taskRewardTypes });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName}: Error Message: {Message}, Error Code: {ErrorCode}", className, methodName, ex.Message, StatusCodes.Status500InternalServerError);

                return new ImportRewardTypeResponseDto
                {
                    ErrorCode = StatusCodes.Status500InternalServerError,
                    ErrorMessage = ex.Message
                };
            }
        }
    }
}
