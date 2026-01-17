using Microsoft.AspNetCore.Mvc;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.Common.Core.Extensions;
using SunnyRewards.Helios.Task.Core.Domain.Constants;
using SunnyRewards.Helios.Task.Core.Domain.Dtos;
using SunnyRewards.Helios.Task.Infrastructure.Services;
using SunnyRewards.Helios.Task.Infrastructure.Services.Interface;
using System.Diagnostics;

namespace SunnyRewards.Helios.Task.Api.Controller
{
    [Route("api/v1/")]
    [ApiController]
    public class TaskRewardController : ControllerBase
    {
        private readonly ILogger<TaskRewardController> _taskRewardLogger;
        private readonly ITaskRewardService _taskRewardService;
        const string className = nameof(TaskRewardController);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="taskRewardLogger"></param>
        /// <param name="taskRewardService"></param>
        public TaskRewardController(ILogger<TaskRewardController> taskRewardLogger, ITaskRewardService taskRewardService)
        {
            _taskRewardLogger = taskRewardLogger;
            _taskRewardService = taskRewardService;
        }

        /// <summary>
        /// Retrieves a list of task rewards from the repository and returns them in a standardized response format.
        /// </summary>
        /// <returns></returns>
        [HttpGet("task-rewards")]
        public async Task<IActionResult> GetTaskRewardsAsync()
        {
            const string methodName = nameof(GetTaskRewardsAsync);
            try
            {
                _taskRewardLogger.LogInformation("{ClassName}.{MethodName}: Started processing...", className, methodName);

                var response = await _taskRewardService.GetTaskRewardsAsync();

                return response.ErrorCode switch
                {
                    409 => Conflict(response),
                    500 => StatusCode(StatusCodes.Status500InternalServerError, response),
                    _ => Ok(response)
                };
            }
            catch (Exception ex)
            {
                _taskRewardLogger.LogError(ex, "{ClassName}.{MethodName}: Error processing.", className, methodName);
                return StatusCode(StatusCodes.Status500InternalServerError, new GetTasksAndTaskRewardsResponseDto
                {
                    ErrorCode = StatusCodes.Status500InternalServerError,
                    ErrorMessage = ex.Message
                });
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
                response = await _taskRewardService.UpdateTaskRewardAsync(taskRewardId, taskRewardRequestDto, true);
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
        /// 
        /// </summary>
        /// <param name="taskRewardRequestDto"></param>
        /// <returns></returns>
        [HttpPost("find-task-rewards")]
        public async Task<ActionResult<FindTaskRewardResponseDto>?> FindTaskRewards([FromBody] FindTaskRewardRequestDto taskRewardRequestDto)
        {
            const string methodName = nameof(FindTaskRewards);
            try
            {
                _taskRewardLogger.LogInformation("{className}.{methodName}: API - Entered with TenantCode:{TenantCode}", className, methodName, taskRewardRequestDto.TenantCode);
                var response = await _taskRewardService.GetTaskRewardDetails(taskRewardRequestDto);
                return response.TaskRewardDetails.Count > 0 ? Ok(response) : NotFound();
            }
            catch (Exception ex)
            {
                _taskRewardLogger.LogError(ex, "{className}.{methodName}: API - ERROR:{msg},Error Code:{errorCode}", className, methodName, ex.Message, StatusCodes.Status500InternalServerError);
                return null;
            }

        }

        /// <summary>
        /// Returns one Task Reward with all details of the Task matching the given TaskRewardCode
        /// </summary>
        /// <param name="taskRewardRequestDto"></param>
        /// <returns></returns>
        [HttpPost("get-task-reward-by-code")]
        public async Task<ActionResult<GetTaskRewardByCodeResponseDto>?> GetTaskRewardByCode([FromBody] GetTaskRewardByCodeRequestDto taskRewardRequestDto)
        {
            const string methodName = nameof(GetTaskRewardByCode);
            try
            {
                _taskRewardLogger.LogInformation("{className}.{methodName}: API- Entered with TaskRewardCode: {TaskRewardCode}", className, methodName, taskRewardRequestDto.TaskRewardCode);
                var response = await _taskRewardService.GetTaskRewardByCode(taskRewardRequestDto);

                return response != null ? Ok(response) : NotFound();
            }
            catch (Exception ex)
            {
                _taskRewardLogger.LogError(ex, "{className}.{methodName} API - ERROR:{msg},Error Code:{errorCode}", className, methodName, ex.Message, StatusCodes.Status500InternalServerError);
                return null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rewardTypeRequestDto"></param>
        /// <returns></returns>
        [HttpPost("reward-type")]
        public async Task<ActionResult<RewardTypeResponseDto>> RewardType([FromBody] RewardTypeRequestDto rewardTypeRequestDto)
        {
            const string methodName = nameof(RewardType);
            try
            {
                _taskRewardLogger.LogInformation("{className}.{methodName}: API- Entered with TaskId : {TaskId}", className, methodName, rewardTypeRequestDto.TaskId);
                var response = await _taskRewardService.RewardType(rewardTypeRequestDto);

                return response.RewardTypeDto != null ? Ok(response) : NotFound();
            }
            catch (Exception ex)
            {
                _taskRewardLogger.LogError(ex, "{className}.{methodName} API - ERROR:{msg},Error Code:{errorCode}", className, methodName, ex.Message, StatusCodes.Status500InternalServerError);
                return null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="taskRewardType"></param>
        /// <returns></returns>
        [HttpPost("reward-type-code")]
        public async Task<ActionResult<RewardTypeResponseDto>> RewardTypeCode([FromBody] RewardTypeCodeRequestDto rewardTypeCodeRequestDto)
        {
            const string methodName = nameof(RewardTypeCode);
            try
            {
                _taskRewardLogger.LogInformation("{className}.{methodName}: API- Entered with RewardTypeCode : {RewardTypeCode}", className, methodName, rewardTypeCodeRequestDto.RewardTypeCode);
                var response = await _taskRewardService.RewardTypeCode(rewardTypeCodeRequestDto);

                return response.ErrorCode switch
                {
                    400 => BadRequest(response),
                    409 => Conflict(response),
                    404 => NotFound(response),
                    _ => Ok(response)
                };
            }
            catch (Exception ex)
            {
                _taskRewardLogger.LogError(ex, "{className}.{methodName} API - ERROR:{msg},Error Code:{errorCode}", className, methodName, ex.Message, StatusCodes.Status500InternalServerError);
                return null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="getTaskByTenantCodeRequestDto"></param>
        /// <returns></returns>
        [HttpPost("get-all-task-by-tenantcode")]
        public async Task<ActionResult<GetTaskByTenantCodeResponseDto>> GetAllTaskByTenantCode([FromBody] GetTaskByTenantCodeRequestDto getTaskByTenantCodeRequestDto)
        {
            const string methodName = nameof(GetAllTaskByTenantCode);
            try
            {
                _taskRewardLogger.LogInformation("{ClassName}.{MethodName}: Started processing  with tenantCode : {TenantCode}", className, methodName, getTaskByTenantCodeRequestDto.TenantCode);
                var response = await _taskRewardService.GetAllTaskByTenantCode(getTaskByTenantCodeRequestDto);

                if (response.ErrorCode != null)
                {
                    return StatusCode((int)response.ErrorCode, response);
                }
                _taskRewardLogger.LogInformation("{ClassName}.{MethodName}: Ended processing successfully.", className, methodName);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _taskRewardLogger.LogError(ex, "{ClassName}.{MethodName} Error processing- ERROR:{Msg},Error Code:{ErrorCode}", className, methodName, ex.Message, StatusCodes.Status500InternalServerError);
                return StatusCode(StatusCodes.Status500InternalServerError, new GetTaskByTenantCodeResponseDto
                {
                    ErrorCode = StatusCodes.Status500InternalServerError,
                    ErrorMessage = ex.Message
                });
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="taskRewardId"></param>
        /// <returns></returns>
        [HttpGet("current-period-descriptor/{taskRewardId}")]
        public async Task<ActionResult<GetTriviaResponseDto>> CurrentPeriodDescriptor(long taskRewardId)
        {
            const string methodName = nameof(CurrentPeriodDescriptor);
            try
            {
                _taskRewardLogger.LogInformation("{className}.{methodName}: API - Enter with {taskRewardId}", className, methodName, taskRewardId);

                var response = await _taskRewardService.CurrentPeriodDescriptor(taskRewardId);
                return response.ErrorCode switch
                {
                    404 => NotFound(response),
                    _ => Ok(response)
                };

            }
            catch (Exception ex)
            {
                _taskRewardLogger.LogError(ex, "{className}.{methodName} API - ERROR:{msg},Error Code:{errorCode}", className, methodName, ex.Message, StatusCodes.Status500InternalServerError);
                return new GetTriviaResponseDto() { ErrorMessage = ex.Message };

            }
        }

        [HttpPost("task-reward")]
        public async Task<IActionResult> CreateTaskReward([FromBody] CreateTaskRewardRequestDto requestDto)
        {
            const string methodName = nameof(CreateTaskReward);
            try
            {
                _taskRewardLogger.LogInformation("{ClassName}.{MethodName}: Request started with, TaskCode:{TaskCode}, Tenant Code: {TenantCode}", className, methodName, requestDto.TaskCode, requestDto.TaskReward.TenantCode);
                var response = await _taskRewardService.CreateTaskReward(requestDto);
                if (response.ErrorCode != null)
                {
                    _taskRewardLogger.LogError("{ClassName}.{MethodName}: Error occurred while creating task Reward,  Request: {RequestData}, Response: {ResponseData}, ErrorCode: {ErrorCode}", className, methodName, requestDto.ToJson(), response.ToJson(), response.ErrorCode);
                    return StatusCode((int)response.ErrorCode, response);
                }
                _taskRewardLogger.LogInformation("{ClassName}.{MethodName}: Task Reward Create successful for Tenant Code: {TenantCode}", className, methodName, requestDto.TaskReward.TenantCode);

                return Ok(response);
            }
            catch (Exception ex)
            {
                _taskRewardLogger.LogError(ex, "{ClassName}.{MethodName}: An error occurred during Task Reward Create. Error Message: {ErrorMessage}, ErrorCode: {ErrorCode}", className, methodName, ex.Message, StatusCodes.Status500InternalServerError);
                return StatusCode(StatusCodes.Status500InternalServerError, new BaseResponseDto() { ErrorCode = StatusCodes.Status500InternalServerError });
            }
        }

        /// <summary>
        /// Retrieves tasks and taskrewards with tenant code
        /// </summary>
        /// <param name="getTaskRewardsRequestDto">The gettaskrewards request dto </param>
        /// <returns></returns>
        [HttpPost("get-task-rewards")]
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
        public async Task<IActionResult> GetTaskRewardDetails([FromQuery] string tenantCode, [FromQuery] string? languageCode,[FromQuery] string? taskExternalCode)
        {
            const string methodName = nameof(GetTaskRewardDetails); // Method name for logging
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
                languageCode = string.IsNullOrEmpty(languageCode) ? Constant.LanguageCode : languageCode;

                // Call the service to fetch task reward details for the given tenant code
                var response = await _taskRewardService.GetTaskRewardDetails(tenantCode, taskExternalCode, languageCode);

                // Log the successful completion of processing
                _taskRewardLogger.LogInformation("{ClassName}.{MethodName}: Successfully ended processing with Tenant Code: {TenantCode}", className, methodName, tenantCode);

                // Return the successful response
                return Ok(response);
            }
            catch (Exception ex)
            {
                // Log the exception and return a 500 Internal Server Error response
                _taskRewardLogger.LogError(ex, "{ClassName}.{MethodName}: Error processing.", className, methodName);
                return StatusCode(StatusCodes.Status500InternalServerError, new TaskRewardDetailsResponseDto
                {
                    ErrorCode = StatusCodes.Status500InternalServerError,
                    ErrorMessage = "An unexpected error occurred while processing the request."
                });
            }
        }
        /// <summary>
        /// Retrieves a collection of task rewards for a given tenant code.
        /// </summary>
        /// <param name="taskRewardCollectionRequestDto">The request DTO containing the tenant code and other parameters to fetch task rewards.</param>
        /// <returns>
        /// Returns an <see cref="IActionResult"/> containing the task reward collection response.
        /// On success, returns an HTTP 200 OK response with the task reward details.
        /// On failure, returns an HTTP 500 Internal Server Error with an appropriate error message.
        /// </returns>
        [HttpPost("task-reward-collection")]
        public async Task<IActionResult> GetTaskRewardCollection([FromBody] TaskRewardCollectionRequestDto taskRewardCollectionRequestDto)
        {
            const string methodName = nameof(GetTaskRewardCollection); // Method name for logging
            try
            {
                _taskRewardLogger.LogInformation("{ClassName}.{MethodName}: Started processing with Tenant Code: {TenantCode} and TaskRewardCode:{TaskRewardCode}",
                   className, methodName, taskRewardCollectionRequestDto.TenantCode, taskRewardCollectionRequestDto.TaskRewardCode);

                // Call the service to fetch task reward details for the given tenant code
                var response = await _taskRewardService.GetTaskRewardCollection(taskRewardCollectionRequestDto);

                if (response.ErrorCode != null)
                {
                    _taskRewardLogger.LogError("{ClassName}.{MethodName}: Error occurred while retrieving taskreward collection Request: {Request}, Response:{Response},Error Code:{ErrorCode}", className, methodName, 
                        taskRewardCollectionRequestDto.ToJson(), response.ToJson(), response.ErrorCode);
                    return StatusCode((int)response.ErrorCode, response);
                }

                // Log the successful completion of processing
                _taskRewardLogger.LogInformation("{ClassName}.{MethodName}: Successfully ended processing with Tenant Code: {TenantCode}", className, methodName, taskRewardCollectionRequestDto.TenantCode);

                // Return the successful response
                return Ok(response);
            }
            catch (Exception ex)
            {
                // Log the exception and return a 500 Internal Server Error response
                _taskRewardLogger.LogError(ex, "{ClassName}.{MethodName}: Error processing with Tenant Code: {TenantCode} and TaskRewardCode:{TaskRewardCode}.",
                    className, methodName,taskRewardCollectionRequestDto.TenantCode, taskRewardCollectionRequestDto.TaskRewardCode);
                return StatusCode(StatusCodes.Status500InternalServerError, new TaskRewardCollectionResponseDto
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

                _taskRewardLogger.LogInformation("{ClassName}.{MethodName}: Successfully ended processing with Tenant Code: {TenantCode}", className, methodName, tenantCode);

                return Ok(new TaskRewardsResponseDto { TaskRewards = response });
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
        /// <summary>
        /// 
        /// </summary>
        /// <param name="adventureTaskCollectionRequestDto"></param>
        /// <returns></returns>
        [HttpPost("get-adventures-task-collections")]
        public async Task<IActionResult> GetAdventuresAndTaskCollections([FromBody] AdventureTaskCollectionRequestDto adventureTaskCollectionRequestDto)
        {
            const string methodName = nameof(GetAdventuresAndTaskCollections);
            try
            {
                var time = Stopwatch.StartNew();
                _taskRewardLogger.LogInformation("{className}.{methodName}: API - Started processing with Tenant Code: {TenantCode}", className, methodName, adventureTaskCollectionRequestDto.TenantCode);

                var response = await _taskRewardService.GetAdventuresAndTaskCollections(adventureTaskCollectionRequestDto);

                if (response.ErrorCode != null)
                {
                    _taskRewardLogger.LogError("{className}.{methodName}: API - Error occurred while processing. Error Msg: {response} Error Code:{errorCode}", className, methodName, response.ErrorMessage, response.ErrorCode);
                    return StatusCode((int)response.ErrorCode, response);
                }
                _taskRewardLogger.LogInformation("{className}.{methodName}: API - Successfully fetched adventure task collections,TimeTaken:{Time}", className, methodName,time.ElapsedMilliseconds);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _taskRewardLogger.LogError(ex, "{className}.{methodName}: API - Error occurred with Tenant Code: {TenantCode} ERROR Msg:{msg}", 
                    className, methodName, adventureTaskCollectionRequestDto.TenantCode, ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError,new AdventureTaskCollectionResponseDto()
                {
                    ErrorCode = StatusCodes.Status500InternalServerError,
                    ErrorMessage = "An unexpected error occurred while processing the request."
                });
            }
        }

    }
}
