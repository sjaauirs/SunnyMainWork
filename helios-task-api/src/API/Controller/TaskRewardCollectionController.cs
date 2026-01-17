using Microsoft.AspNetCore.Mvc;
using SunnyRewards.Helios.Task.Core.Domain.Dtos;
using SunnyRewards.Helios.Task.Infrastructure.Services.Interface;

namespace SunnyRewards.Helios.Task.Api.Controller
{
    [Route("api/v1")]
    [ApiController]
    public class TaskRewardCollectionController : ControllerBase
    {
        private readonly ILogger<TaskRewardCollectionController> _logger;
        private readonly ITaskRewardCollectionService _taskRewardCollectionService;
        private const string className = nameof(TaskRewardCollectionController);

        public TaskRewardCollectionController(ILogger<TaskRewardCollectionController> logger, ITaskRewardCollectionService taskRewardCollectionService)
        {
            _logger = logger;
            _taskRewardCollectionService = taskRewardCollectionService;
        }
        [HttpPost("export-taskreward-collection")]
        public async Task<IActionResult> ExportTaskRewardCollection(ExportTaskRewardCollectionRequestDto requestDto)
        {
            const string methodName = nameof(ExportTaskRewardCollection);
            try
            {
                _logger.LogInformation("{ClassName}.{MethodName}: Started processing with TenantCode:{TenantCode}",
                    className, methodName, requestDto.TenantCode);

                var response = await _taskRewardCollectionService.ExportTaskRewardCollection(requestDto);

                if (response.ErrorCode != null)
                {
                    _logger.LogWarning("{ClassName}.{MethodName}: Error occured with TenantCode:{TenantCode},Response:{Response}",
                    className, methodName, requestDto.TenantCode, response);
                    return StatusCode((int)response.ErrorCode, response);
                }
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName}: Error occured with TenantCode:{TenantCode},ErrorCode:{ErrorCode},ERROR:{Error}",
                    className, methodName, requestDto.TenantCode, StatusCodes.Status500InternalServerError, ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new ExportTaskRewardCollectionResponseDto() { ErrorCode = StatusCodes.Status500InternalServerError });
            }
        }
    }
}
