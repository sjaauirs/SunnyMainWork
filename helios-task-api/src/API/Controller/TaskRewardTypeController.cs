using Microsoft.AspNetCore.Mvc;
using SunnyRewards.Helios.Common.Core.Extensions;
using SunnyRewards.Helios.Task.Core.Domain.Dtos;
using SunnyRewards.Helios.Task.Infrastructure.Services.Interface;

namespace SunnyRewards.Helios.Task.Api.Controller
{
    [Route("api/v1/")]
    [ApiController]
    public class TaskRewardTypeController : ControllerBase
    {
        private readonly ILogger<TaskRewardTypeController> _logger;
        private readonly ITaskRewardTypeService _taskRewardTypeService;
        const string className = nameof(TaskRewardTypeController);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="taskTaskRewardTypeLogger"></param>
        /// <param name="taskTaskRewardTypeService"></param>
        public TaskRewardTypeController(ILogger<TaskRewardTypeController> logger, ITaskRewardTypeService taskRewardTypeService)
        {
            _logger = logger;
            _taskRewardTypeService = taskRewardTypeService;
        }

        [HttpGet("task-reward-types")]
        public async Task<IActionResult> GetTaskRewardTypesAsync()
        {
            const string methodName = nameof(GetTaskRewardTypesAsync);
            try
            {
                _logger.LogInformation("{ClassName}.{MethodName}: Started processing...", className, methodName);

                var response = await _taskRewardTypeService.GetTaskRewardTypesAsync();

                return response.ErrorCode switch
                {
                    409 => Conflict(response),
                    500 => StatusCode(StatusCodes.Status500InternalServerError, response),
                    _ => Ok(response)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName}: Error processing.", className, methodName);
                return StatusCode(StatusCodes.Status500InternalServerError, new TaskRewardTypesResponseDto
                {
                    ErrorCode = StatusCodes.Status500InternalServerError,
                    ErrorMessage = ex.Message
                });
            }
        }

        /// <summary>
        /// UpdateTaskRewardTypeAsync
        /// </summary>
        /// <param name="rewardTypeId"></param>
        /// <param name="taskRewardTypeRequestDto"></param>
        /// <returns></returns>
        [HttpPut("task-reward-type/{rewardTypeId}")]
        public async Task<IActionResult> UpdateTaskRewardTypeAsync(long rewardTypeId, [FromBody] TaskRewardTypeRequestDto taskRewardTypeRequestDto)
        {
            const string methodName = nameof(UpdateTaskRewardTypeAsync);
            var response = new TaskRewardTypeResponseDto();
            try
            {
                _logger.LogInformation("{ClassName}.{MethodName}: Started processing...RewardTypeId: {RewardTypeId}", className, methodName, rewardTypeId);
                response = await _taskRewardTypeService.UpdateTaskRewardTypeAsync(rewardTypeId, taskRewardTypeRequestDto);
                if (response?.ErrorCode != null)
                {
                    _logger.LogError("{ClassName}.{MethodName}: Error occurred processing. Request: {RequestData}, Response: {ResponseData}, ErrorCode: {ErrorCode}", className, methodName, taskRewardTypeRequestDto.ToJson(), response.ToJson(), response.ErrorCode);
                    return Conflict(response);
                }

                _logger.LogInformation("{ClassName}.{MethodName}: Ended processing successfully.", className, methodName);

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName}: Error occurred processing. Error Message: {ErrorMessage}, ErrorCode: {ErrorCode}", className, methodName, ex.Message, StatusCodes.Status500InternalServerError);
                return Conflict(response);
            }
        }
        /// <summary>
        /// Imports reward types into the system.
        /// </summary>
        /// <param name="requestDto">Request DTO containing list of reward types.</param>
        /// <returns>
        /// Returns 200 OK if all types are imported,
        /// 206 Partial Content if some failed,
        /// or 500 Internal Server Error for unexpected errors.
        /// </returns>
        [HttpPost("import-reward-types")]
        public async Task<IActionResult> ImportRewardTypesAsync([FromBody] ImportRewardTypeRequestDto requestDto)
        {
            const string methodName = nameof(ImportRewardTypesAsync);
            _logger.LogInformation("{ClassName}.{MethodName} - Import reward types request received with {Count} reward types.", 
                className, methodName,requestDto.RewardTypes.Count);
            try
            {
                var response = await _taskRewardTypeService.ImportRewardTypesAsync(requestDto);

                if (response.ErrorCode != null)
                {
                    _logger.LogWarning("{ClassName}.{MethodName} - Import completed with errors. Response:{Response}", 
                        className, methodName, response.ToJson());
                    return StatusCode((int)response.ErrorCode, response);
                }

                _logger.LogInformation("{ClassName}.{MethodName} - Import completed successfully.", className, methodName);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName} - Unexpected error during import.", className, methodName);
                return StatusCode(StatusCodes.Status500InternalServerError, new ImportRewardTypeResponseDto
                {
                    ErrorCode = StatusCodes.Status500InternalServerError,
                    ErrorMessage = ex.Message
                });
            }
        }
    }
}