using Microsoft.AspNetCore.Mvc;
using SunnyRewards.Helios.Admin.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.Task.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Admin.Api.Controllers
{
    [Route("api/v1/")]
    [ApiController]
    public class TaskTypeController : ControllerBase
    {
        private readonly ILogger<TaskTypeController> _logger;
        private readonly ITaskTypeService _taskTypeService;
        private const string className = nameof(TaskTypeController);

        public TaskTypeController(ILogger<TaskTypeController> logger, ITaskTypeService taskTypeService)
        {
            _logger = logger;
            _taskTypeService = taskTypeService;
        }
        
        /// <summary>
        /// Retrieves a list of task types from the TaskType API and returns them in a standardized response format.
        /// </summary>
        /// <returns></returns>
        [HttpGet("task-types")]
        public async Task<IActionResult> GetTaskTypesAsync()
        {
            const string methodName = nameof(GetTaskTypesAsync);
            try
            {
                _logger.LogInformation("{ClassName}.{MethodName}: Started processing...", className, methodName);

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
                _logger.LogError(ex, "{ClassName}.{MethodName}: Error processing.", className, methodName);
                return StatusCode(StatusCodes.Status500InternalServerError, new TaskTypesResponseDto
                {
                    ErrorCode = StatusCodes.Status500InternalServerError,
                    ErrorMessage = ex.Message
                });
            }
        }
    }

}
