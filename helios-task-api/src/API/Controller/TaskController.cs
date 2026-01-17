using Microsoft.AspNetCore.Mvc;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.Common.Core.Extensions;
using SunnyRewards.Helios.Task.Core.Domain.Dtos;
using SunnyRewards.Helios.Task.Infrastructure.Services.Interface;

namespace SunnyRewards.Helios.Task.Api.Controller
{
    [Route("api/v1/")]
    [ApiController]
    public class TaskController : ControllerBase
    {
        private readonly ILogger<TaskController> _taskLogger;
        private readonly ITaskService _taskService;
        private const string className = nameof(TaskController);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="taskLogger"></param>
        /// <param name="taskService"></param>
        public TaskController(ILogger<TaskController> taskLogger, ITaskService taskService)
        {
            _taskLogger = taskLogger;
            _taskService = taskService;
        }

        /// <summary>
        /// Retrieves a list of tasks from the repository and returns them in a standardized response format.
        /// </summary>
        /// <returns></returns>
        [HttpGet("tasks")]
        public async Task<IActionResult> GetTasksAsync()
        {
            const string methodName = nameof(GetTasksAsync);
            try
            {
                _taskLogger.LogInformation("{ClassName}.{MethodName}: Started processing...", className, methodName);

                var response = await _taskService.GetTasksAsync();

                return response.ErrorCode switch
                {
                    409 => Conflict(response),
                    500 => StatusCode(StatusCodes.Status500InternalServerError, response),
                    _ => Ok(response)
                };
            }
            catch (Exception ex)
            {
                _taskLogger.LogError(ex, "{ClassName}.{MethodName}: Error processing.", className, methodName);
                return StatusCode(StatusCodes.Status500InternalServerError, new GetTasksAndTaskRewardsResponseDto
                {
                    ErrorCode = StatusCodes.Status500InternalServerError,
                    ErrorMessage = ex.Message
                });
            }
        }
        /// <summary>
        /// Creates the Task
        /// </summary>
        /// <param name="taskRequestDto"></param>
        /// <returns></returns>
        [HttpPost("task")]
        public async Task<IActionResult> CreateTask([FromBody] CreateTaskRequestDto taskRequestDto)
        {
            const string methodName = nameof(CreateTask);
            try
            {
                _taskLogger.LogInformation("{ClassName}.{MethodName}: Request started with Task Code: {TaskCode}", className, methodName, taskRequestDto.TaskCode);
                var response = await _taskService.CreateTask(taskRequestDto);
                if (response.ErrorCode != null)
                {
                    _taskLogger.LogError("{ClassName}.{MethodName}: Error occurred during task Create. Request: {RequestData}, Response: {ResponseData}, ErrorCode: {ErrorCode}", className, methodName, taskRequestDto.ToJson(), response.ToJson(), response.ErrorCode);
                    return StatusCode((int)response.ErrorCode, response);
                }

                _taskLogger.LogInformation("{ClassName}.{MethodName}: Task Create successful for Task Code: {TaskCode}", className, methodName, taskRequestDto.TaskCode);

                return Ok(response);
            }
            catch (Exception ex)
            {
                _taskLogger.LogError(ex, "{ClassName}.{MethodName}: An error occurred during task import. Error Message: {ErrorMessage}, ErrorCode: {ErrorCode}", className, methodName, ex.Message, StatusCodes.Status500InternalServerError);
                return StatusCode(StatusCodes.Status500InternalServerError, new BaseResponseDto() { ErrorCode = StatusCodes.Status500InternalServerError });
            }

        }

        /// <summary>
        /// UpdateTaskAsync
        /// </summary>
        /// <param name="taskId"></param>
        /// <param name="taskRequestDto"></param>
        /// <returns></returns>
        [HttpPut("task/{taskId}")]
        public async Task<IActionResult> UpdateTaskAsync(long taskId, [FromBody] TaskRequestDto taskRequestDto)
        {
            const string methodName = nameof(UpdateTaskAsync);
            TaskResponseDto? response = null;
            try
            {
                _taskLogger.LogInformation("{ClassName}.{MethodName}: Request started with Task Code: {TaskCode}", className, methodName, taskRequestDto.TaskCode);
                response = await _taskService.UpdateTaskAsync(taskId, taskRequestDto);
                if (response.ErrorCode != null)
                {
                    _taskLogger.LogError("{ClassName}.{MethodName}: Error occurred during task Create. Request: {RequestData}, Response: {ResponseData}, ErrorCode: {ErrorCode}", className, methodName, taskRequestDto.ToJson(), response.ToJson(), response.ErrorCode);
                    return Conflict(response);
                }

                _taskLogger.LogInformation("{ClassName}.{MethodName}: Task Create successful for Task Code: {TaskCode}", className, methodName, taskRequestDto.TaskCode);

                return Ok(response);
            }
            catch (Exception ex)
            {
                _taskLogger.LogError(ex, "{ClassName}.{MethodName}: An error occurred during task import. Error Message: {ErrorMessage}, ErrorCode: {ErrorCode}", className, methodName, ex.Message, StatusCodes.Status500InternalServerError);
                return Conflict(response);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="taskRewardRequestDto"></param>
        /// <returns></returns>
        [HttpPost("task/get-by-task-reward-code")]
        public async Task<ActionResult<TaskTypeDto>?> GetTasksByTaskRewardCode([FromBody] GetTaskRewardRequestDto taskRewardRequestDto)
        {
            const string methodName = nameof(GetTasksByTaskRewardCode);
            try
            {
                _taskLogger.LogInformation("{className}.{methodName}: API - Entered with TaskRewardCodes: {TaskRewardCodes}", className, methodName, taskRewardRequestDto.TaskRewardCodes);
                var response = await _taskService.GetTasksByTaskRewardCode(taskRewardRequestDto);
                return response != null ? Ok(response) : NotFound();
            }
            catch (Exception ex)
            {
                _taskLogger.LogError(ex, "{className}.{methodName} API: ERROR:{msg}, Error Code:{errorCode}", className, methodName, ex.Message, StatusCodes.Status500InternalServerError);
                return null;
            }
        }

        [HttpPost("task/task-export")]
        public async Task<IActionResult> GetTaskExport([FromBody] ExportTaskRequestDto exportTaskRequestDto)
        {
            const string methodName = nameof(GetTaskExport);
            try
            {
                _taskLogger.LogInformation("{ClassName}.{MethodName}: Request started with TenantCode: {TenantCode}", className, methodName, exportTaskRequestDto.TenantCode);

                var response = await _taskService.GetTaskExport(exportTaskRequestDto);

                if (response.ErrorCode != null)
                {
                    _taskLogger.LogError("{ClassName}.{MethodName}: Error occurred during task export. Request: {RequestData}, Response: {ResponseData}, ErrorCode: {ErrorCode}", className, methodName, exportTaskRequestDto.ToJson(), response.ToJson(), response.ErrorCode);
                    return StatusCode((int)response.ErrorCode, response);
                }

                _taskLogger.LogInformation("{ClassName}.{MethodName}: Task export successful for TenantCode: {TenantCode}", className, methodName, exportTaskRequestDto.TenantCode);

                return Ok(response);
            }
            catch (Exception ex)
            {
                _taskLogger.LogError(ex, "{ClassName}.{MethodName}: An error occurred during task export. Error Message: {ErrorMessage}, ErrorCode: {ErrorCode}", className, methodName, ex.Message, StatusCodes.Status500InternalServerError);
                return StatusCode(StatusCodes.Status500InternalServerError, new ExportTaskResponseDto() { ErrorCode = StatusCodes.Status500InternalServerError });
            }
        }

        [HttpPost("task/task-external-mapping")]
        public async Task<IActionResult> CreateTaskExternalMapping([FromBody] TaskExternalMappingRequestDto requestDto)
        {
            const string methodName = nameof(CreateTaskExternalMapping);
            try
            {
                _taskLogger.LogInformation("{ClassName}.{MethodName}: Request started with Request: {RequestDto}", className, methodName, requestDto.ToJson());

                var response = await _taskService.CreateTaskExternalMapping(requestDto);

                if (response.ErrorCode != null)
                {
                    _taskLogger.LogError("{ClassName}.{MethodName}: Error occurred during Saving Task External Mapping. Request: {RequestData}, Response: {ResponseData}, ErrorCode: {ErrorCode}", className, methodName, requestDto.ToJson(), response.ToJson(), response.ErrorCode);
                    return StatusCode((int)response.ErrorCode, response);
                }

                _taskLogger.LogInformation("{ClassName}.{MethodName}: Cohort export successful for Task External Mapping: {requestDto}", className, methodName, requestDto.ToJson());

                return Ok(response);
            }
            catch (Exception ex)
            {
                _taskLogger.LogError(ex, "{ClassName}.{MethodName}: An error occurred during task export. Error Message: {ErrorMessage}, ErrorCode: {ErrorCode}", className, methodName, ex.Message, StatusCodes.Status500InternalServerError);
                return StatusCode(StatusCodes.Status500InternalServerError, new BaseResponseDto() { ErrorCode = StatusCodes.Status500InternalServerError });
            }
        }
        /// <summary>
        /// Fetches a task based on the task name provided in the request.
        /// </summary>
        /// <param name="getTaskByTaskNameRequestDto">The DTO containing the task name for which the task details are requested.</param>
        /// <returns>
        /// An <see cref="IActionResult"/> representing the result of the task fetch operation:
        /// - If successful, returns an HTTP 200 (OK) response with a <see cref="GetTaskByTaskNameResponseDto"/> containing the task details.
        /// - If the task is not found or an error occurs, returns an appropriate HTTP status code with a response indicating the error.
        /// </returns>
        [HttpPost("task/get-task-by-task-name")]
        public async Task<IActionResult> GetTaskByTaskName([FromBody] GetTaskByTaskNameRequestDto getTaskByTaskNameRequestDto)
        {
            const string methodName = nameof(GetTaskByTaskName);
            try
            {
                _taskLogger.LogInformation("{ClassName}.{MethodName}: Request started with Task Name: {TaskCode}", className, methodName, getTaskByTaskNameRequestDto.TaskName);

                var response = await _taskService.GetTaskByTaskName(getTaskByTaskNameRequestDto);
                if (response.ErrorCode != null)
                {
                    _taskLogger.LogError("{ClassName}.{MethodName}: Error occurred during fetching task. Request: {RequestData}, Response: {ResponseData}, ErrorCode: {ErrorCode}", className, methodName, getTaskByTaskNameRequestDto.ToJson(), response.ToJson(), response.ErrorCode);
                    return StatusCode((int)response.ErrorCode, response);
                }

                _taskLogger.LogInformation("{ClassName}.{MethodName}: Task fetched successful for Task Code: {TaskCode}", className, methodName, getTaskByTaskNameRequestDto.TaskName);

                return Ok(response);
            }
            catch (Exception ex)
            {
                _taskLogger.LogError(ex, "{ClassName}.{MethodName}: An error occurred while fetching task. Error Message: {ErrorMessage}, ErrorCode: {ErrorCode}", className, methodName, ex.Message, StatusCodes.Status500InternalServerError);
                return StatusCode(StatusCodes.Status500InternalServerError, new GetTaskByTaskNameResponseDto() { ErrorCode = StatusCodes.Status500InternalServerError });
            }
        }
    }
}

