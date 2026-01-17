using Microsoft.AspNetCore.Mvc;
using SunnyRewards.Helios.Admin.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.Task.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Admin.Api.Controllers
{
    [Route("api/v1/")]
    [ApiController]
    public class TaskCategoryController : ControllerBase
    {
        private readonly ILogger<TaskCategoryController> _logger;
        private readonly ITaskCategoryService _taskCategoryService;
        private const string className = nameof(TaskCategoryController);

        public TaskCategoryController(ILogger<TaskCategoryController> logger, ITaskCategoryService taskCategoryService)
        {
            _logger = logger;
            _taskCategoryService = taskCategoryService;
        }
        
        /// <summary>
        /// Retrieves a list of task categories from the TaskCategory API and returns them in a standardized response format.
        /// </summary>
        /// <returns></returns>
        [HttpGet("task-categories")]
        public async Task<IActionResult> GetTaskCategoriesAsync()
        {
            const string methodName = nameof(GetTaskCategoriesAsync);
            try
            {
                _logger.LogInformation("{ClassName}.{MethodName}: Started processing...", className, methodName);

                var response = await _taskCategoryService.GetTaskCategoriesAsync();

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
                return StatusCode(StatusCodes.Status500InternalServerError, new TaskCategoriesResponseDto
                {
                    ErrorCode = StatusCodes.Status500InternalServerError,
                    ErrorMessage = ex.Message
                });
            }
        }
    }

}
