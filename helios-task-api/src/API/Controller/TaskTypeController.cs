using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using SunnyRewards.Helios.Common.Core.Extensions;
using SunnyRewards.Helios.Task.Core.Domain.Dtos;
using SunnyRewards.Helios.Task.Infrastructure.Services.Interface;

namespace SunnyRewards.Helios.Task.Api.Controller
{
    [Route("api/v1/")]
    [ApiController]
    public class TaskTypeController : ControllerBase
    {

        private readonly ILogger<TaskTypeController> _taskTypeLogger;
        private readonly ITaskTypeService _taskTypeService;
        const string className = nameof(TaskTypeController);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="taskTypeLogger"></param>
        /// <param name="taskTypeService"></param>
        public TaskTypeController(ILogger<TaskTypeController> taskTypeLogger, ITaskTypeService taskTypeService)
        {
            _taskTypeLogger = taskTypeLogger;   
            _taskTypeService = taskTypeService;
        }

       /// <summary>
       /// 
       /// </summary>
       /// <param name="taskTypeId"></param>
       /// <returns></returns>

        [HttpGet("get-by-taskTypeId")]
        public async Task<ActionResult<TaskTypeResponseDto>?> GetTaskTypeById(long taskTypeId)
        {
            const string methodName = nameof(GetTaskTypeById);
            try
            {
                _taskTypeLogger.LogInformation("{className}.{methodName}: API - Entered with taskTypeId: {taskTypeId}", className, methodName, taskTypeId);
                var response = await _taskTypeService.GetTaskTypeById(taskTypeId);
                return response.ErrorCode switch
                {
                    400 => BadRequest(response),
                    404 => NotFound(response),
                    500 => StatusCode(StatusCodes.Status500InternalServerError, response),
                    _ => Ok(response)
                };
            }
            catch (Exception ex)
            {
                _taskTypeLogger.LogError(ex, "{className}.{methodName}: ERROR Msg:{msg}, Error Code:{errorCode}", className, methodName, ex.Message, StatusCodes.Status500InternalServerError);
                return null;
            }
        }

        [HttpGet("get-by-taskTypeCode")]
        public async Task<ActionResult<TaskTypeResponseDto>?> GetTaskTypeByCode(string taskTypeCode)
        {
            const string methodName = nameof(GetTaskTypeByCode);
            try
            {
                _taskTypeLogger.LogInformation("{className}.{methodName}: Entered with taskTypeCode: {taskTypeCode}", className, methodName, taskTypeCode);
                var response = await _taskTypeService.GetTaskTypeByTypeCode(taskTypeCode);
                return response.ErrorCode switch
                {
                    400 => BadRequest(response),
                    404 => NotFound(response),
                    500 => StatusCode(StatusCodes.Status500InternalServerError, response),
                    _ => Ok(response)
                };
            }
            catch (Exception ex)
            {
                _taskTypeLogger.LogError(ex, "{className}.{methodName}: ERROR Msg:{msg}, Error Code:{errorCode}", className, methodName, ex.Message, StatusCodes.Status500InternalServerError);
                return null;
            }
        }

        [HttpGet("task-types")]
        public async Task<IActionResult> GetTaskTypesAsync()
        {
            const string methodName = nameof(GetTaskTypesAsync);
            try
            {
                _taskTypeLogger.LogInformation("{ClassName}.{MethodName}: Started processing...", className, methodName);

                var response = await _taskTypeService.GetTaskTypesAsync();

                return response.ErrorCode switch
                {
                    409 => Conflict(response),
                    500 => StatusCode(StatusCodes.Status500InternalServerError, response),
                    _ => Ok(response)
                };
            }
            catch (Exception ex)
            {
                _taskTypeLogger.LogError(ex, "{ClassName}.{MethodName}: Error processing.", className, methodName);
                return StatusCode(StatusCodes.Status500InternalServerError, new GetTasksAndTaskRewardsResponseDto
                {
                    ErrorCode = StatusCodes.Status500InternalServerError,
                    ErrorMessage = ex.Message
                });
            }
        }
        /// <summary>
        /// Imports task types by calling the service layer and returns appropriate HTTP response.
        /// </summary>
        /// <param name="taskTypeRequestDto">DTO containing task types to import.</param>
        /// <returns>ActionResult with import status and details.</returns>
        [HttpPost("import-task-types")]
        public async Task<IActionResult> ImportTaskTypesAsync([FromBody] ImportTaskTypeRequestDto taskTypeRequestDto)
        {
            const string methodName = nameof(ImportTaskTypesAsync);
            _taskTypeLogger.LogInformation("{ClassName}.{MethodName} - Import request received with {Count} task types.", className,methodName, taskTypeRequestDto.TaskTypes.Count);
            try
            {
                var response = await _taskTypeService.ImportTaskTypesAsync(taskTypeRequestDto);

                if (response.ErrorCode != null)
                {
                    _taskTypeLogger.LogWarning("{ClassName}.{MethodName} - Import completed with partial errors. Response: {Response}", 
                        className, methodName, response.ToJson());
                    return StatusCode((int)response.ErrorCode, response);
                }

                _taskTypeLogger.LogInformation("{ClassName}.{MethodName} - Import completed successfully with all task types.", className,methodName);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _taskTypeLogger.LogError(ex, "{ClassName}.{MethodName} - Import failed due to an unexpected error.",className, methodName);
                return StatusCode(StatusCodes.Status500InternalServerError, new ImportTaskTypeResponseDto
                {
                    ErrorCode = StatusCodes.Status500InternalServerError,
                    ErrorMessage = ex.Message
                });
            }
        }
    }
}
