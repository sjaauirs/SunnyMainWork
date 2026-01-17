using Microsoft.AspNetCore.Mvc;
using SunnyRewards.Helios.Common.Core.Extensions;
using SunnyRewards.Helios.Task.Core.Domain.Dtos;
using SunnyRewards.Helios.Task.Infrastructure.Services.Interface;

namespace SunnyRewards.Helios.Task.Api.Controller
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="taskCategoryService"></param>
    [Route("api/v1/")]
    [ApiController]
    public class TaskCategoryController : ControllerBase
    {
        private readonly ILogger<TaskCategoryController> _logger;
        private readonly ITaskCategoryService _taskCategoryService ;
        private const string className = nameof(TaskCategoryController);

        public TaskCategoryController(ILogger<TaskCategoryController> logger, ITaskCategoryService taskCategoryService)
        {
            _logger = logger;
            _taskCategoryService = taskCategoryService;
        }

        /// <summary>
        /// Retrieves a list of task categories from the repository and returns them in a standardized response format.
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
        /// <summary>
        /// Imports task categories using the provided request DTO.
        /// </summary>
        /// <param name="taskCategoryRequestDto">The DTO containing a list of task categories to be imported.</param>
        /// <returns>
        /// Returns an <see cref="IActionResult"/> containing:
        /// - 200 OK if all categories are imported successfully,
        /// - 206 Partial Content if some categories failed to import,
        /// - 500 Internal Server Error in case of unexpected exceptions.
        /// </returns>
        [HttpPost("import-task-categories")]
        public async Task<IActionResult> ImportTaskCategoriesAsync(ImportTaskCategoryRequestDto taskCategoryRequestDto)
        {
            const string methodName = nameof(ImportTaskCategoriesAsync);
            _logger.LogInformation("{ClassName}.{MethodName} - Import task categories request received with {count} task categories.",className, methodName,
                taskCategoryRequestDto.TaskCategories.Count);
            try
            {
                var response = await _taskCategoryService.ImportTaskCategoriesAsync(taskCategoryRequestDto);

                if (response.ErrorCode != null)
                {
                    _logger.LogWarning("{ClassName}.{MethodName} - Import completed with errors. Response: {Response}",
                        className, methodName,response.ToJson());
                    return StatusCode((int)response.ErrorCode, response);
                }

                _logger.LogInformation("{ClassName}.{MethodName} - Import completed successfully.",className, methodName);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName} - Unexpected error occurred during import.",className, methodName);
                return StatusCode(StatusCodes.Status500InternalServerError, new ImportTaskCategoryResponseDto
                {
                    ErrorCode = StatusCodes.Status500InternalServerError,
                    ErrorMessage = ex.Message
                });
            }
        }
    }
}