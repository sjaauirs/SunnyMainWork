using Microsoft.AspNetCore.Mvc;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.Common.Core.Extensions;
using SunnyRewards.Helios.Task.Core.Domain.Dtos;
using SunnyRewards.Helios.Task.Infrastructure.Services.Interface;

namespace SunnyRewards.Helios.Task.Api.Controller
{
    [Route("api/v1/")]
    [ApiController]
    public class TaskDetailsController : ControllerBase
    {
        private readonly ILogger<TaskDetailsController> _taskDetailsLogger;
        private readonly ITaskDetailsService _taskDetailsService;
        const string className = nameof(TaskDetailsController);

        public TaskDetailsController(ILogger<TaskDetailsController> taskDetailsLogger, ITaskDetailsService taskDetailsService)
        {
            _taskDetailsLogger = taskDetailsLogger;
            _taskDetailsService = taskDetailsService;
        }
        /// <summary>
        /// CreateTaskDetails
        /// </summary>
        /// <param name="requestDto"></param>
        /// <returns></returns>

        [HttpPost("task-detail")]
        public async Task<IActionResult> CreateTaskDetails([FromBody] CreateTaskDetailsRequestDto requestDto)
        {
            const string methodName = nameof(CreateTaskDetails);
            try
            {
                _taskDetailsLogger.LogInformation("{ClassName}.{MethodName}: Request started with, TaskCode:{TaskCode}, Tenant Code: {TenantCode}", className, methodName,requestDto.TaskCode, requestDto.TaskDetail.TenantCode);
                var response = await _taskDetailsService.CreateTaskDetails(requestDto);
                if (response.ErrorCode != null)
                {
                    _taskDetailsLogger.LogError("{ClassName}.{MethodName}: Error occurred while creating task details,  Request: {RequestData}, Response: {ResponseData}, ErrorCode: {ErrorCode}", className, methodName, requestDto.ToJson(), response.ToJson(), response.ErrorCode);
                    return StatusCode((int)response.ErrorCode, response);
                }
                _taskDetailsLogger.LogInformation("{ClassName}.{MethodName}: Task Details Create successful for Tenant Code: {TenantCode}", className, methodName, requestDto.TaskDetail.TenantCode);

                return Ok(response);
            }
            catch (Exception ex)
            {
                _taskDetailsLogger.LogError(ex, "{ClassName}.{MethodName}: An error occurred during Task Details Create. Error Message: {ErrorMessage}, ErrorCode: {ErrorCode}", className, methodName, ex.Message, StatusCodes.Status500InternalServerError);
                return StatusCode(StatusCodes.Status500InternalServerError, new BaseResponseDto() { ErrorCode = StatusCodes.Status500InternalServerError });
            }
        }

        /// <summary>
        /// UpdateTaskDetailAsync
        /// </summary>
        /// <param name="updateTaskDetailRequestDto"></param>
        /// <returns></returns>
        [HttpPut("task-detail/{taskDetailId}")]
        public async Task<IActionResult> UpdateTaskDetailAsync(long taskDetailId, [FromBody] TaskDetailRequestDto taskDetailRequestDto)
        {
            const string methodName = nameof(UpdateTaskDetailAsync);
            try
            {
                _taskDetailsLogger.LogInformation("{ClassName}.{MethodName}: Started processing...TaskRewardId: {TaskRewardId}", className, methodName, taskDetailId);
                var response = await _taskDetailsService.UpdateTaskDetailAsync(taskDetailId, taskDetailRequestDto);
                if (response.ErrorCode != null)
                {
                    _taskDetailsLogger.LogError("{ClassName}.{MethodName}: Error occurred processing. Request: {RequestData}, Response: {ResponseData}, ErrorCode: {ErrorCode}", className, methodName, taskDetailRequestDto.ToJson(), response.ToJson(), response.ErrorCode);
                    return Conflict(response);
                }

                _taskDetailsLogger.LogInformation("{ClassName}.{MethodName}: Ended processing successfully.", className, methodName);

                return Ok(response);
            }
            catch (Exception ex)
            {
                _taskDetailsLogger.LogError(ex, "{ClassName}.{MethodName}: Error occurred processing. Error Message: {ErrorMessage}, ErrorCode: {ErrorCode}", className, methodName, ex.Message, StatusCodes.Status500InternalServerError);
                return StatusCode(StatusCodes.Status500InternalServerError, new BaseResponseDto() { ErrorCode = StatusCodes.Status500InternalServerError });
            }
        }
    }
}
