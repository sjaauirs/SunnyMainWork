using Microsoft.AspNetCore.Mvc;
using SunnyRewards.Helios.Admin.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.Task.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Admin.Api.Controllers
{
    [Route("api/v1/")]
    [ApiController]
    public class TaskRewardTypeController : ControllerBase
    {
        private readonly ILogger<TaskRewardTypeController> _logger;
        private readonly ITaskRewardTypeService _taskRewardTypeService;
        private const string className = nameof(TaskRewardTypeController);

        public TaskRewardTypeController(ILogger<TaskRewardTypeController> logger, ITaskRewardTypeService taskRewardTypeService)
        {
            _logger = logger;
            _taskRewardTypeService = taskRewardTypeService;
        }

        /// <summary>
        /// Retrieves a list of reward types from TaskRewardType API and returns them in a standardized response format.
        /// </summary>
        /// <returns></returns>
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
    }

}
