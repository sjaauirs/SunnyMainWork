using Microsoft.AspNetCore.Mvc;
using SunnyRewards.Helios.Admin.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.Common.Core.Extensions;
using SunnyRewards.Helios.Task.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Admin.Api.Controllers
{
    [Route("api/v1/task")]
    [ApiController]
    public class TaskController : ControllerBase
    {
        private readonly ILogger<TaskController> _taskLogger;
        private readonly ITaskService _taskService;
        private const string className = nameof(TaskController);

        public TaskController(ILogger<TaskController> taskLogger, ITaskService taskService)
        {
            _taskLogger = taskLogger;
            _taskService = taskService;
        }

        /// <summary>
        /// Retrieves a list of tasks from the tasks API and returns them in a standardized response format.
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
        ///  Create Task
        /// </summary>
        /// <param name="createTaskRequestDto">request for create Task</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> CreateTask(CreateTaskRequestDto createTaskRequestDto)
        {
            const string methodName = nameof(CreateTask);
            try
            {
                _taskLogger.LogInformation("{ClassName}.{MethodName}: Request started with TaskCode: {TaskCode}", className, methodName, createTaskRequestDto.TaskCode);
                var response = await _taskService.CreateTask(createTaskRequestDto);

                if (response.ErrorCode != null)
                {
                    _taskLogger.LogError("{ClassName}.{MethodName}: Error occurred while creating Task. Request: {RequestData}, Response: {ResponseData}, ErrorCode: {ErrorCode}", className, methodName, createTaskRequestDto.ToJson(), response.ToJson(), response.ErrorCode);
                    return StatusCode((int)response.ErrorCode, response);
                }

                _taskLogger.LogInformation("{ClassName}.{MethodName}: Task created Successful, with TaskCode: {TaskCode}", className, methodName, createTaskRequestDto.TaskCode);

                return Ok(response);
            }
            catch (Exception ex)
            {
                _taskLogger.LogError(ex, "{ClassName}.{MethodName}: An error occurred while create Task. Error Message: {ErrorMessage}, ErrorCode: {ErrorCode}", className, methodName, ex.Message, StatusCodes.Status500InternalServerError);
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
                if (response?.ErrorCode != null)
                {
                    _taskLogger.LogError("{ClassName}.{MethodName}: Error occurred during task update. Request: {RequestData}, Response: {ResponseData}, ErrorCode: {ErrorCode}", className, methodName, taskRequestDto.ToJson(), response.ToJson(), response.ErrorCode);
                    return StatusCode((int)response.ErrorCode, response);
                }
                return Ok(response);
            }
            catch (Exception ex)
            {
                _taskLogger.LogError(ex, "{ClassName}.{MethodName}: An error occurred during task update. Error Message: {ErrorMessage}, ErrorCode: {ErrorCode}", className, methodName, ex.Message, StatusCodes.Status500InternalServerError);
                return StatusCode(StatusCodes.Status500InternalServerError, response);
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
        [HttpPost("get-task-by-task-name")]
        public async Task<IActionResult> GetTaskByTaskName([FromBody] GetTaskByTaskNameRequestDto getTaskByTaskNameRequestDto)
        {
            const string methodName = nameof(GetTaskByTaskName);
            try
            {
                _taskLogger.LogInformation("{ClassName}.{MethodName}: Request started with Task Name: {Name}", className, methodName, getTaskByTaskNameRequestDto.TaskName);

                var response = await _taskService.GetTaskByTaskName(getTaskByTaskNameRequestDto);
                if (response.ErrorCode != null)
                {
                    _taskLogger.LogError("{ClassName}.{MethodName}: Error occurred during fetching task. Request: {RequestData}, Response: {ResponseData}, ErrorCode: {ErrorCode}", className, methodName, getTaskByTaskNameRequestDto.ToJson(), response.ToJson(), response.ErrorCode);
                    return StatusCode((int)response.ErrorCode, response);
                }

                _taskLogger.LogInformation("{ClassName}.{MethodName}: Task fetched successful for TaskName: {Name}", className, methodName, getTaskByTaskNameRequestDto.TaskName);

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
