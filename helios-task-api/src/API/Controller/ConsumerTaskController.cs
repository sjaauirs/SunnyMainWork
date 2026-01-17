using Microsoft.AspNetCore.Mvc;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.Common.Core.Extensions;
using SunnyRewards.Helios.Task.Core.Domain.Dtos;
using SunnyRewards.Helios.Task.Infrastructure.Services;
using SunnyRewards.Helios.Task.Infrastructure.Services.Interface;

namespace SunnyRewards.Helios.Task.Api.Controller
{
    [Route("api/v1/")]
    [ApiController]
    public class ConsumerTaskController : ControllerBase
    {
        private readonly ILogger<ConsumerTaskController> _consumerTaskLogger;
        private readonly IConsumerTaskService _consumerTaskService;
        private readonly ISubtaskService _subTaskService;
        const string className = nameof(ConsumerTaskController);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="consumerTaskLogger"></param>
        /// <param name="consumerTaskService"></param>
        /// <param name="subTaskService"></param>
        public ConsumerTaskController(ILogger<ConsumerTaskController> consumerTaskLogger, IConsumerTaskService consumerTaskService, ISubtaskService subTaskService)
        {
            _consumerTaskLogger = consumerTaskLogger;
            _consumerTaskService = consumerTaskService;
            _subTaskService = subTaskService;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="findConsumerTasksByIdRequestDto"></param>
        /// <returns></returns>
        [HttpPost("get-consumer-task-by-task-id")]
        public async Task<ActionResult<FindConsumerTasksByIdResponseDto>> GetConsumerTaskById(FindConsumerTasksByIdRequestDto findConsumerTasksByIdRequestDto)
        {
            const string methodName = nameof(GetConsumerTaskById);
            try
            {
                _consumerTaskLogger.LogInformation("{className}.{methodName}: API - Enter with {TaskStatus}", className, methodName, findConsumerTasksByIdRequestDto.TaskStatus);

                var response = await _consumerTaskService.GetConsumerTask(findConsumerTasksByIdRequestDto);
                return response != null && response.ConsumerTask != null ? Ok(response) : NotFound();
            }
            catch (Exception ex)
            {
                _consumerTaskLogger.LogError(ex, "{className}.{methodName}: API -  ERROR:{msg}, Error Code:{errorCode}", className, methodName, ex.Message, StatusCodes.Status500InternalServerError);
                return new FindConsumerTasksByIdResponseDto();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="findConsumerTaskRequestDto"></param>
        /// <returns></returns>
        [HttpPost("find-consumer-tasks")]
        public async Task<ActionResult?> FindConsumerTasks(FindConsumerTaskRequestDto findConsumerTaskRequestDto)
        {
            const string methodName = nameof(FindConsumerTasks);
            try
            {
                _consumerTaskLogger.LogInformation("{className}.{methodName}: API - Enter For Consumer Code:{consumerCode} with {TaskStatus}", className, methodName, findConsumerTaskRequestDto.ConsumerCode, findConsumerTaskRequestDto.TaskStatus);
                var response = await _consumerTaskService.GetConsumerTasks(findConsumerTaskRequestDto);
                return response != null ? Ok(response) : NotFound();
            }
            catch (Exception ex)
            {
                _consumerTaskLogger.LogError(ex, "{className}.{methodName}: API -  ERROR:{msg}, Error Code:{errorCode}", className, methodName, ex.Message, StatusCodes.Status500InternalServerError);
                return null;
            }

        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="consumerTaskDto"></param>
        /// <returns></returns>
        [HttpPost("consumer-task")]
        public async Task<ActionResult<ConsumerTaskResponseUpdateDto>?> PostConsumerTasks([FromBody] ConsumerTaskDto consumerTaskDto)
        {
            const string methodName = nameof(PostConsumerTasks);
            try
            {
                _consumerTaskLogger.LogInformation("{className}.{methodName}: API - Enter ConsumerCode:{consumer} with {TaskStartTs}", className, methodName,consumerTaskDto.ConsumerCode, consumerTaskDto.TaskStartTs);
                var response = await _consumerTaskService.CreateConsumerTasks(consumerTaskDto);
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
                _consumerTaskLogger.LogError(ex, "{className}.{methodName}: API -  ERROR:{msg}, Error Code:{errorCode}", className, methodName, ex.Message, StatusCodes.Status500InternalServerError);
                return null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="consumerTaskDto"></param>
        /// <returns></returns>
        [HttpPut("update-consumer-task")]
        public async Task<ActionResult<ConsumerTaskDto>> UpdateConsumerTask([FromForm] UpdateConsumerTaskDto consumerTaskDto)
        {
            const string methodName = nameof(UpdateConsumerTask);
            try
            {
                _consumerTaskLogger.LogInformation("{className}.{methodName}: API - Enter with {ConsumerTaskId}", className, methodName, consumerTaskDto.ConsumerTaskId);
                var result = await _consumerTaskService.UpdateConsumerTask(consumerTaskDto);
                return result.ConsumerTaskId > 0 ? Ok(result) : BadRequest(result);
            }
            catch (Exception ex)
            {
                _consumerTaskLogger.LogError(ex, "{className}.{methodName}: API -  ERROR:{msg}, Error Code:{errorCode}", className, methodName, ex.Message, StatusCodes.Status500InternalServerError);
                return new ConsumerTaskDto();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="consumerTaskDto"></param>
        /// <returns></returns>
        [HttpPut("update-consumer-task-details")]
        public async Task<ActionResult<ConsumerTaskDto>> UpdateConsumerTaskDetails(ConsumerTaskDto consumerTaskDto)
        {
            const string methodName = nameof(UpdateConsumerTaskDetails);
            try
            {
                _consumerTaskLogger.LogInformation("{className}.{methodName}: API - Enter with {ConsumerTaskId}", className, methodName, consumerTaskDto.ConsumerTaskId);
                var response = await _consumerTaskService.UpdateConsumerTaskDetails(consumerTaskDto);

                if (response.ErrorCode != null)
                {
                    _consumerTaskLogger.LogError("{ClassName}.{MethodName}: Error occurred while updating consumer task. Response: {ResponseData}, ErrorCode: {ErrorCode}",
                        className, methodName, response.ToJson(), response.ErrorCode);
                    return StatusCode((int)response.ErrorCode, response);
                }
                _consumerTaskLogger.LogInformation("{ClassName}.{MethodName}: Successfully updated consumer task.", className, methodName);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _consumerTaskLogger.LogError(ex, "{className}.{methodName}: API -  ERROR:{msg}, Error Code:{errorCode}", className, methodName, ex.Message, StatusCodes.Status500InternalServerError);
                return StatusCode(StatusCodes.Status500InternalServerError,
                              new BaseResponseDto
                              {
                                  ErrorCode = StatusCodes.Status500InternalServerError,
                                  ErrorMessage = "An unexpected error occurred while updating consumer task. Please try again later."
                              });
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="consumerTaskRequestDto"></param>
        /// <returns></returns>
        [HttpPost("get-all-consumer-tasks")]
        public async Task<ActionResult<ConsumerTaskResponseDto>> GetAllConsumerTask(ConsumerTaskRequestDto consumerTaskRequestDto)
        {
            const string methodName = nameof(GetAllConsumerTask);
            try
            {
                _consumerTaskLogger.LogInformation("{className}.{methodName}: API - Enter with {ConsumerCode}", className, methodName, consumerTaskRequestDto.ConsumerCode);
                var response = await _consumerTaskService.GetAllConsumerTask(consumerTaskRequestDto);
                return response != null ? Ok(response) : NotFound();
            }
            catch (Exception ex)
            {
                _consumerTaskLogger.LogError(ex, "{className}.{methodName}: API -  ERROR:{msg}, Error Code:{errorCode}", className, methodName, ex.Message, StatusCodes.Status500InternalServerError);
                return new ConsumerTaskResponseDto();
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="getConsumerSubtasksRequestDto"></param>
        /// <returns></returns>

        [HttpPost("get-consumer-subtasks")]
        public async Task<ActionResult<GetConsumerSubTaskResponseDto>> GetConsumerSubtask([FromBody] GetConsumerSubtasksRequestDto getConsumerSubtasksRequestDto)
        {
            const string methodName = nameof(GetConsumerSubtask);
            try
            {
                _consumerTaskLogger.LogInformation("{className}.{methodName}: API - Entered with ConsumerCode: {TaskRewardConsumerCodeCodes}", className, methodName, getConsumerSubtasksRequestDto.ConsumerCode);
                var response = await _subTaskService.GetConsumerSubtask(getConsumerSubtasksRequestDto);
                return response.ConsumerTaskDto.Length != 0 ? Ok(response) : NotFound();
            }
            catch (Exception ex)
            {
                _consumerTaskLogger.LogError(ex, "{className}.{methodName}: API -  ERROR:{msg}, Error Code:{errorCode}", className, methodName, ex.Message, StatusCodes.Status500InternalServerError);
                return new GetConsumerSubTaskResponseDto();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="consumerTaskDto"></param>
        /// <returns></returns>
        [HttpPut("complete-subtask")]
        public async Task<ActionResult<UpdateSubtaskResponseDto>> CompleteSubtask([FromBody] UpdateSubtaskRequestDto updateSubtaskRequestDto)
        {
            const string methodName = nameof(CompleteSubtask);
            try
            {
                _consumerTaskLogger.LogInformation("{className}.{methodName}: API - Enter with {ConsumerTaskId}", className, methodName, updateSubtaskRequestDto.ConsumerTaskId);
                var result = await _subTaskService.UpdateConsumerSubtask(updateSubtaskRequestDto);
                return result != null ? Ok(result) : BadRequest(result);
            }
            catch (Exception ex)
            {
                _consumerTaskLogger.LogError(ex, "{className}.{methodName}: API -  ERROR:{msg}, Error Code:{errorCode}", className, methodName, ex.Message, StatusCodes.Status500InternalServerError);
                return new UpdateSubtaskResponseDto();
            }
        }

        [HttpPost("revert-all-consumer-tasks")]
        public async Task<ActionResult<BaseResponseDto>> RevertAllConsumerTasks(RevertAllConsumerTasksRequestDto revertAllConsumerTasksRequestDto)
        {
            const string methodName = nameof(RevertAllConsumerTasks);
            try
            {
                _consumerTaskLogger.LogInformation("{className}.{methodName}: API - Started with ConsumerCode : {ConsumerCode}", className, methodName, revertAllConsumerTasksRequestDto.ConsumerCode);
                var response = await _consumerTaskService.RevertAllConsumerTasks(revertAllConsumerTasksRequestDto);

                return response.ErrorCode switch
                {
                    400 => BadRequest(response),
                    _ => Ok(response)
                };
            }
            catch (Exception ex)
            {
                _consumerTaskLogger.LogError(ex, "{className}.{methodName}: ERROR - Message : {Message}, Error Code:{errorCode}", className, methodName, ex.Message, StatusCodes.Status500InternalServerError);
                return StatusCode(StatusCodes.Status500InternalServerError, new BaseResponseDto()
                {
                    ErrorCode = StatusCodes.Status500InternalServerError,
                    ErrorMessage = "Internal Server Error"
                });
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rewardTypeConsumerTaskRequestDto"></param>
        /// <returns></returns>
        [HttpPost("get-available-tasks-by-reward-type")]
        public async Task<ActionResult<ConsumerTaskResponseDto>> GetAvailableTaskRewardType(GetRewardTypeConsumerTaskRequestDto rewardTypeConsumerTaskRequestDto)
        {
            const string methodName = nameof(GetAvailableTaskRewardType);
            try
            {
                _consumerTaskLogger.LogInformation("{className}.{methodName}: API - Started with ConsumerCode : {consumerCode} ,tenantCode :" +
                    "{TenantCode},rewardTypeCode :{RewardTypeCode}", className, methodName,  rewardTypeConsumerTaskRequestDto.ConsumerCode,rewardTypeConsumerTaskRequestDto.
                    TenantCode,rewardTypeConsumerTaskRequestDto.RewardTypeCode);

                var response = await _consumerTaskService.GetAvailableTaskRewardType(rewardTypeConsumerTaskRequestDto);
                return response != null ? Ok(response) : NotFound();
            }
            catch (Exception ex)
            {
                _consumerTaskLogger.LogError(ex, "{className}.{methodName}: API -  ERROR:{msg}, Error Code:{errorCode}", className, methodName, ex.Message, StatusCodes.Status500InternalServerError);
                throw;
            }

        }
        [HttpPost("subtask")]
        public async Task<IActionResult> CreateSubtask([FromBody] SubtaskRequestDto requestDto)
        {
            const string methodName = nameof(CreateSubtask);
            try
            {
                _consumerTaskLogger.LogInformation("{ClassName}.{MethodName}: Request started with Request: {RequestDto}", className, methodName, requestDto?.ToJson());

                var response = await _subTaskService.CreateSubTask(requestDto);

                if (response.ErrorCode != null)
                {
                    _consumerTaskLogger.LogError("{ClassName}.{MethodName}: Error occurred during Saving subtask. Request: {RequestData}, Response: {ResponseData}, ErrorCode: {ErrorCode}", className, methodName, requestDto?.ToJson(), response.ToJson(), response.ErrorCode);
                    return StatusCode((int)response.ErrorCode, response);
                }

                _consumerTaskLogger.LogInformation("{ClassName}.{MethodName}: successfully saved for subtask: {requestDto}", className, methodName, requestDto?.ToJson());

                return Ok(response);
            }
            catch (Exception ex)
            {
                _consumerTaskLogger.LogError(ex, "{ClassName}.{MethodName}: An error occurred during task export. Error Message: {ErrorMessage}, ErrorCode: {ErrorCode}", className, methodName, ex.Message, StatusCodes.Status500InternalServerError);
                return StatusCode(StatusCodes.Status500InternalServerError, new ExportTaskResponseDto() { ErrorCode = StatusCodes.Status500InternalServerError });
            }
        }

        /// <summary>
        /// Removes a specific consumer task based on the provided request details.
        /// </summary>
        /// <param name="deleteConsumerTaskRequestDto">The request DTO containing consumer and task details for the task to be removed.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a <see cref="BaseResponseDto"/> 
        /// indicating the success or failure of the removal operation.</returns>
        [HttpPost("remove-consumer-task")]
        public async Task<ActionResult<BaseResponseDto>> RemoveConsumerTask(DeleteConsumerTaskRequestDto deleteConsumerTaskRequestDto)
        {
            const string methodName = nameof(RemoveConsumerTask);
            try
            {
                _consumerTaskLogger.LogInformation("{className}.{methodName}: API - Started with ConsumerCode : {consumerCode} ,tenantCode :" +
                    "{TenantCode},TaskExternalCode :{TaskExternalCode}", className, methodName, deleteConsumerTaskRequestDto.ConsumerCode, deleteConsumerTaskRequestDto.TenantCode,
                    deleteConsumerTaskRequestDto.TaskExternalCode);

                var response = await _consumerTaskService.RemoveConsumerTask(deleteConsumerTaskRequestDto);
                if (response.ErrorCode != null)
                {
                    _consumerTaskLogger.LogError("{ClassName}.{MethodName}: Error occurred during soft delete consumer task. Request: {RequestData}, Response: {ResponseData}, ErrorCode: {ErrorCode}", className, methodName, deleteConsumerTaskRequestDto.ToJson(), response.ToJson(), response.ErrorCode);
                    return StatusCode((int)response.ErrorCode, response);
                }
                _consumerTaskLogger.LogInformation("{ClassName}.{MethodName}: soft delete consumer task successful for ConsumerCode: {ConsumerCode}", className, methodName, deleteConsumerTaskRequestDto.ConsumerCode);

                return Ok(response);
            }
            catch (Exception ex)
            {
                _consumerTaskLogger.LogError(ex, "{className}.{methodName}: API -  ERROR:{msg}, Error Code:{errorCode}", className, methodName, ex.Message, StatusCodes.Status500InternalServerError);
                return StatusCode(StatusCodes.Status500InternalServerError, new BaseResponseDto() { ErrorCode = StatusCodes.Status500InternalServerError , ErrorMessage = ex.Message});
            }

        }

        /// <summary>
        /// Retrieves all consumers who have completed a specific task within the provided date range.
        /// </summary>
        /// <param name="getConsumerTaskByTaskId">The request object containing TaskId, TenantCode, StartDate, and EndDate criteria.</param>
        /// <returns>A response containing the list of completed consumer tasks or an error message.</returns>

        [HttpPost("consumers-completing-taskId-in-range")]
        public async Task<ActionResult<BaseResponseDto>> GetConsumersByTaskId([FromBody] GetConsumerTaskByTaskId getConsumerTaskByTaskId)
        {
            const string methodName = nameof(GetConsumersByTaskId);
            try
            {
                _consumerTaskLogger.LogInformation(
                    "{className}.{methodName}: API - Started with TaskId: {TaskId}, TenantCode: {TenantCode}, From: {StartDate}, To: {EndDate}",
                    className, methodName, getConsumerTaskByTaskId.TaskId, getConsumerTaskByTaskId.TenantCode,
                    getConsumerTaskByTaskId.StartDate, getConsumerTaskByTaskId.EndDate);

                var response = await _consumerTaskService.GetConsumersByTaskId(getConsumerTaskByTaskId);

                _consumerTaskLogger.LogInformation(
                    "{className}.{methodName}: API - Completed for TaskId: {TaskId}, Result Count: {Count}",
                    className, methodName, getConsumerTaskByTaskId.TaskId, response?.CompletedTasks?.Count ?? 0);

                return Ok(response);
            }
            catch (Exception ex)
            {
                _consumerTaskLogger.LogError(ex,
                    "{className}.{methodName}: API - ERROR: {msg}, Error Code: {errorCode}",
                    className, methodName, ex.Message, StatusCodes.Status500InternalServerError);

                return StatusCode(StatusCodes.Status500InternalServerError,
                    new BaseResponseDto
                    {
                        ErrorCode = StatusCodes.Status500InternalServerError,
                        ErrorMessage = ex.Message
                    });
            }
        }

        [HttpPost("update-health-task-progress")]
        public async Task<IActionResult> UpdateHealthTaskProgress([FromBody] UpdateHealthTaskProgressRequestDto request)
        {
            const string methodName = nameof(UpdateHealthTaskProgress);

            try
            {
                _consumerTaskLogger.LogInformation(
                    "{ClassName}.{MethodName}: Request started with TenantCode: {TenantCode}, ConsumerCode: {ConsumerCode}, TaskId: {TaskId}",
                    className, methodName, request.TenantCode, request.ConsumerCode, request.TaskId);

                var response = await _consumerTaskService.UpdateHealthTaskProgress(request);

                if (response.ErrorCode != null)
                {
                    _consumerTaskLogger.LogError(
                        "{ClassName}.{MethodName}: Failed to update task progress. Request: {RequestData}, Response: {ResponseData}, ErrorCode: {ErrorCode}",
                        className, methodName, request.ToJson(), response.ToJson(), response.ErrorCode);

                    return StatusCode((int)response.ErrorCode, response);
                }

                _consumerTaskLogger.LogInformation(
                    "{ClassName}.{MethodName}: Task progress update successful. TaskId: {TaskId}, ConsumerCode: {ConsumerCode}",
                    className, methodName, request.TaskId, request.ConsumerCode);

                return Ok(response);
            }
            catch (Exception ex)
            {
                _consumerTaskLogger.LogError(ex,
                    "{ClassName}.{MethodName}: Exception occurred while updating health task. ErrorMessage: {Message}",
                    className, methodName, ex.Message);

                return StatusCode(StatusCodes.Status500InternalServerError, new ConsumerHealthTaskResponseUpdateDto
                {
                    ErrorCode = StatusCodes.Status500InternalServerError
                });
            }
        }
    }
}

