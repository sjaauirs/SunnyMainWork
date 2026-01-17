using Microsoft.AspNetCore.Mvc;
using SunnyRewards.Helios.Admin.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.Common.Core.Extensions;
using SunnyRewards.Helios.Task.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Admin.Api.Controllers
{
    [Route("api/v1/")]
    [ApiController]
    public class TaskRewardController : ControllerBase
    {
        private readonly ILogger<TaskRewardController> _taskRewardLogger;
        private readonly ITaskRewardService _taskRewardService;
        private const string className = nameof(TaskRewardController);

        public TaskRewardController(ILogger<TaskRewardController> taskRewardLogger, ITaskRewardService taskRewardService)
        {
            _taskRewardLogger = taskRewardLogger;
            _taskRewardService = taskRewardService;
        }
        [HttpPost("task-reward")]
        public async Task<IActionResult> CreateTaskReward(CreateTaskRewardRequestDto requestDto)
        {
            const string methodName = nameof(CreateTaskReward);
            try
            {
                _taskRewardLogger.LogInformation("{ClassName}.{MethodName}: Request started with TaskCode: {TaskCode}, Tenant Code:{TenantCode}", className, methodName, requestDto.TaskCode, requestDto.TaskReward.TenantCode);
                var response = await _taskRewardService.CreateTaskReward(requestDto);

                if (response.ErrorCode != null)
                {
                    _taskRewardLogger.LogError("{ClassName}.{MethodName}: Error occurred while creating Task Reward. Request: {RequestData}, Response: {ResponseData}, ErrorCode: {ErrorCode}", className, methodName, requestDto.ToJson(), response.ToJson(), response.ErrorCode);
                    return StatusCode((int)response.ErrorCode, response);
                }

                _taskRewardLogger.LogInformation("{ClassName}.{MethodName}: Task Reward created Successful, with TenantCode: {TenantCode}", className, methodName, requestDto.TaskReward.TenantCode);

                return Ok(response);
            }
            catch (Exception ex)
            {
                _taskRewardLogger.LogError(ex, "{ClassName}.{MethodName}: An error occurred while create Task Detail. Error Message: {ErrorMessage}, ErrorCode: {ErrorCode}", className, methodName, ex.Message, StatusCodes.Status500InternalServerError);
                return StatusCode(StatusCodes.Status500InternalServerError, new BaseResponseDto() { ErrorCode = StatusCodes.Status500InternalServerError });
            }
        }

        /// <summary>
        /// UpdateTaskRewardAsync
        /// </summary>
        /// <param name="taskRewardId"></param>
        /// <param name="taskRewardRequestDto"></param>
        /// <returns></returns>
        [HttpPut("task-reward/{taskRewardId}")]
        public async Task<IActionResult> UpdateTaskRewardAsync(long taskRewardId, [FromBody] TaskRewardRequestDto taskRewardRequestDto)
        {
            const string methodName = nameof(UpdateTaskRewardAsync);
            TaskRewardResponseDto? response = null;
            try
            {
                _taskRewardLogger.LogInformation("{ClassName}.{MethodName}: Started processing...TaskRewardId: {TaskRewardId}", className, methodName, taskRewardId);
                response = await _taskRewardService.UpdateTaskRewardAsync(taskRewardId, taskRewardRequestDto);
                if (response.ErrorCode != null)
                {
                    _taskRewardLogger.LogError("{ClassName}.{MethodName}: Error occurred processing. Request: {RequestData}, Response: {ResponseData}, ErrorCode: {ErrorCode}", className, methodName, taskRewardRequestDto.ToJson(), response.ToJson(), response.ErrorCode);
                    return Conflict(response);
                }

                _taskRewardLogger.LogInformation("{ClassName}.{MethodName}: Ended processing successfully.", className, methodName);

                return Ok(response);
            }
            catch (Exception ex)
            {
                _taskRewardLogger.LogError(ex, "{ClassName}.{MethodName}: Error occurred processing. Error Message: {ErrorMessage}, ErrorCode: {ErrorCode}", className, methodName, ex.Message, StatusCodes.Status500InternalServerError);
                return Conflict(response);
            }
        }

        /// <summary>
        /// Retrieves tasks and taskrewards with tenant code
        /// </summary>
        /// <param name="getTaskRewardsRequestDto">The gettaskrewards request dto </param>
        /// <returns></returns>
        [HttpPost("task-reward/get-task-rewards")]
        public async Task<IActionResult> GetTasksAndTaskRewards(GetTasksAndTaskRewardsRequestDto getTaskRewardsRequestDto)
        {
            const string methodName = nameof(GetTasksAndTaskRewards);
            try
            {
                _taskRewardLogger.LogInformation("{ClassName}.{MethodName}: Started processing gettasks and taskrewards with TenantCode: {TenantCode}", className, methodName, getTaskRewardsRequestDto.TenantCode);

                var response = await _taskRewardService.GetTasksAndTaskRewards(getTaskRewardsRequestDto);
                if (response.ErrorCode != null)
                {
                    _taskRewardLogger.LogError("{ClassName}.{MethodName}: Error occured while retrieving tasks and taskrewards Request: {Request}, Response:{Response},Error Code:{ErrorCode}", className, methodName, getTaskRewardsRequestDto.ToJson(), response.ToJson(), StatusCodes.Status404NotFound);
                    return StatusCode((int)response.ErrorCode, response);
                }
                _taskRewardLogger.LogInformation("{ClassName}.{MethodName}: Sucessfully retrieved tasks and taskrewards with TenantCode: {TenantCode}", className, methodName, getTaskRewardsRequestDto.TenantCode);

                return Ok(response);
            }
            catch (Exception ex)
            {
                _taskRewardLogger.LogError(ex, "{ClassName}.{MethodName}: Error occured while processing gettasks and taskrewards with TenantCode: {TenantCode}, Error Code:{ErrorCode},ERROR:{Msg}", className, methodName, getTaskRewardsRequestDto?.TenantCode, StatusCodes.Status500InternalServerError, ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new GetTasksAndTaskRewardsResponseDto() { ErrorCode = StatusCodes.Status500InternalServerError });
            }
        }

        /// <summary>
        /// Retrieves tasks and task rewards associated with a given tenant code.
        /// </summary>
        /// <param name="tenantCode">The tenant code for which tasks and rewards are to be retrieved.</param>
        /// <returns>An <see cref="IActionResult"/> containing the response data or an error message if the operation fails.</returns>
        [HttpGet("task-reward-details")]
        public async Task<IActionResult> GetTaskRewardDetails(string tenantCode, string? languageCode)
        {
            const string methodName = nameof(GetTaskRewardDetails);
            try
            {
                _taskRewardLogger.LogInformation("{ClassName}.{MethodName}: Started processing with Tenant Code: {TenantCode}", className, methodName, tenantCode);

                if (string.IsNullOrWhiteSpace(tenantCode))
                {
                    _taskRewardLogger.LogError("{ClassName}.{MethodName}: Tenant code is null or empty.", className, methodName);
                    return BadRequest(new TaskRewardDetailsResponseDto()
                    {
                        ErrorCode = StatusCodes.Status400BadRequest,
                        ErrorMessage = "Tenant code cannot be null or empty."
                    });
                }

                var response = await _taskRewardService.GetTaskRewardDetails(tenantCode,languageCode);

                _taskRewardLogger.LogInformation("{ClassName}.{MethodName}: Successfully ended processing with Tenant Code: {TenantCode}", className, methodName, tenantCode);

                return Ok(response);
            }
            catch (Exception ex)
            {
                _taskRewardLogger.LogError(ex, "{ClassName}.{MethodName}: Error processing.", className, methodName);
                return StatusCode(StatusCodes.Status500InternalServerError, new TaskRewardDetailsResponseDto
                {
                    ErrorCode = StatusCodes.Status500InternalServerError,
                    ErrorMessage = "An unexpected error occurred while processing the request."
                });
            }
        }

        /// <summary>
        /// Retrieves tasks and health task rewards associated with a given tenant code.
        /// </summary>
        /// <param name="tenantCode">The tenant code for which tasks and rewards are to be retrieved.</param>
        /// <returns>An <see cref="IActionResult"/> containing the response data or an error message if the operation fails.</returns>
        [HttpGet("health-task-rewards/{tenantCode}")]
        public async Task<IActionResult> GetHealthTaskRewards(string tenantCode)
        {
            const string methodName = nameof(GetHealthTaskRewards);
            try
            {
                _taskRewardLogger.LogInformation("{ClassName}.{MethodName}: Started processing with Tenant Code: {TenantCode}", className, methodName, tenantCode);

                if (string.IsNullOrWhiteSpace(tenantCode))
                {
                    _taskRewardLogger.LogError("{ClassName}.{MethodName}: Tenant code is null or empty.", className, methodName);
                    return BadRequest(new TaskRewardsResponseDto()
                    {
                        ErrorCode = StatusCodes.Status400BadRequest,
                        ErrorMessage = "Tenant code cannot be null or empty."
                    });
                }

                var response = await _taskRewardService.GetHealthTaskRewards(tenantCode);
                
                if (response.ErrorCode != null)
                {
                    _taskRewardLogger.LogError("{className}.{methodName}: API - ERROR Msg:{msg}, Error Code:{errorCode}, Request Data: {request}, Response Data:{response}", className, methodName, response.ErrorMessage, response.ErrorCode, tenantCode, response.ToJson());
                    return StatusCode(Convert.ToInt32(response.ErrorCode), response);
                }

                _taskRewardLogger.LogInformation("{ClassName}.{MethodName}: Successfully ended processing with Tenant Code: {TenantCode}", className, methodName, tenantCode);

                return Ok(response);
            }
            catch (Exception ex)
            {
                _taskRewardLogger.LogError(ex, "{ClassName}.{MethodName}: Error processing.", className, methodName);
                return StatusCode(StatusCodes.Status500InternalServerError, new TaskRewardsResponseDto
                {
                    ErrorCode = StatusCodes.Status500InternalServerError,
                    ErrorMessage = "An unexpected error occurred while processing the request."
                });
            }
        }
    }

}
