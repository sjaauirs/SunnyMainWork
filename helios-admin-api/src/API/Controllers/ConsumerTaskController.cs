using Microsoft.AspNetCore.Mvc;
using SunnyBenefits.Fis.Core.Domain.Dtos;
using SunnyRewards.Helios.Common.Core.Extensions;
using SunnyRewards.Helios.Task.Core.Domain.Dtos;
using SunnyRewards.Helios.Admin.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.Admin.Core.Domain.Dtos;


namespace SunnyRewards.Helios.ConsumerTask.Api.Controllers
{
    [Route("api/v1/")]
    [ApiController]
    public class ConsumerTaskController : ControllerBase
    {
        private readonly ILogger<ConsumerTaskController> _ConsumerTaskLogger;
        private readonly IConsumerTaskService _consumerTaskService;

        const string className = nameof(ConsumerTaskController);
        public ConsumerTaskController(ILogger<ConsumerTaskController> ConsumerTaskLogger,
            IConsumerTaskService consumerTaskService)
        {
            _ConsumerTaskLogger = ConsumerTaskLogger;
            _consumerTaskService = consumerTaskService;
        }

        [HttpPost("get-available-recurring-tasks")]
        public async Task<IActionResult> GetAvailableRecurringTask([FromBody] AvailableRecurringTasksRequestDto availableRecurringTasksRequestDto)
        {
            const string methodName = nameof(GetAvailableRecurringTask);
            try
            {
                _ConsumerTaskLogger.LogInformation("{ClassName}.{MethodName}: Request started with TenantCode: {TenantCode}, Consumer Code : {ConsumerCode}", className, methodName, availableRecurringTasksRequestDto.TenantCode, availableRecurringTasksRequestDto.ConsumerCode);

                var response = await _consumerTaskService.GetAvailableRecurringTask(availableRecurringTasksRequestDto);
                if (response.ErrorCode != null)
                {
                    _ConsumerTaskLogger.LogError("{ClassName}.{MethodName}: Error occurred during fetching Available Recurring task. Request: {RequestData}, Response: {ResponseData}, ErrorCode: {ErrorCode}", className, methodName, availableRecurringTasksRequestDto.ToJson(), response.ToJson(), response.ErrorCode);
                    return StatusCode((int)response.ErrorCode, response);
                }

                _ConsumerTaskLogger.LogInformation("{ClassName}.{MethodName}: Task fetched successful for TenantCode: {TenantCode}, ConsumerCode  : {ConsumerCode}", className, methodName, availableRecurringTasksRequestDto.TenantCode, availableRecurringTasksRequestDto.ConsumerCode);

                return Ok(response);
            }
            catch (Exception ex)
            {
                _ConsumerTaskLogger.LogError(ex, "{ClassName}.{MethodName}: An error occurred while fetching task. Error Message: {ErrorMessage}, ErrorCode: {ErrorCode}", className, methodName, ex.Message, StatusCodes.Status500InternalServerError);
                return StatusCode(StatusCodes.Status500InternalServerError, new GetTaskByTaskNameResponseDto() { ErrorCode = StatusCodes.Status500InternalServerError });
            }
        }


        [HttpPost("consumers-completing-task-in-range")]
        public async Task<IActionResult> GetConsumersByCompletedTask([FromBody] GetConsumerTaskByTaskId getConsumersByTaskIdResuestDto)
        {
            const string methodName = nameof(GetConsumersByCompletedTask);
            try
            {
                _ConsumerTaskLogger.LogInformation(
                    "{className}.{methodName}: API - Started with TaskId: {TaskId}, TenantCode: {TenantCode}, From: {StartDate}, To: {EndDate}",
                    className, methodName, getConsumersByTaskIdResuestDto.TaskId, getConsumersByTaskIdResuestDto.TenantCode,
                    getConsumersByTaskIdResuestDto.StartDate, getConsumersByTaskIdResuestDto.EndDate);

                var response = await _consumerTaskService.GetConsumersByCompletedTask(getConsumersByTaskIdResuestDto);

                if (response.ErrorCode != null)
                {
                    _ConsumerTaskLogger.LogError(
                        "{ClassName}.{MethodName}: Error occurred during fetching consumers. Request: {RequestData}, Response: {ResponseData}, ErrorCode: {ErrorCode}",
                        className, methodName, getConsumersByTaskIdResuestDto.ToJson(), response.ToJson(), response.ErrorCode);

                    return StatusCode((int)response.ErrorCode, response);
                }

                _ConsumerTaskLogger.LogInformation(
                    "{ClassName}.{MethodName}: Task fetch successful for TenantCode: {TenantCode}",
                    className, methodName, getConsumersByTaskIdResuestDto.TenantCode);

                return Ok(response);
            }
            catch (Exception ex)
            {
                _ConsumerTaskLogger.LogError(
                    ex,
                    "{ClassName}.{MethodName}: An error occurred while fetching task. Error Message: {ErrorMessage}, ErrorCode: {ErrorCode}",
                    className, methodName, ex.Message, StatusCodes.Status500InternalServerError);

                return StatusCode(StatusCodes.Status500InternalServerError, new GetTaskByTaskNameResponseDto
                {
                    ErrorCode = StatusCodes.Status500InternalServerError
                });
            }
        }
    }
}