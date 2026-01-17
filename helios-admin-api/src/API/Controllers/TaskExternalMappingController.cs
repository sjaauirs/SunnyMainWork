using Microsoft.AspNetCore.Mvc;
using SunnyRewards.Helios.Admin.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.Common.Core.Extensions;
using SunnyRewards.Helios.Task.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Admin.Api.Controllers
{
    [Route("api/v1/task-external-mapping")]
    [ApiController]
    public class TaskExternalMappingController : ControllerBase
    {
        private readonly ILogger<TaskExternalMappingController> _taskLogger;
        private readonly ITaskExternalMappingService _taskService;
        private const string className = nameof(TaskExternalMappingController);

        public TaskExternalMappingController(ILogger<TaskExternalMappingController> taskLogger, ITaskExternalMappingService taskService)
        {
            _taskLogger = taskLogger;
            _taskService = taskService;
        }
        [HttpPost]
        public async Task<IActionResult> CreateTaskExternalMapping(TaskExternalMappingRequestDto RequestDto)
        {
            const string methodName = nameof(CreateTaskExternalMapping);
            try
            {
                _taskLogger.LogInformation("{ClassName}.{MethodName}: Request started with TaskExternalMappingRequest: {TaskCode}", className, methodName, RequestDto.TaskExternalCode);
                var response = await _taskService.CreateTaskExternalMapping(RequestDto);

                if (response.ErrorCode != null)
                {
                    _taskLogger.LogError("{ClassName}.{MethodName}: Error occurred while creating TaskExternalMapping. Request: {RequestData}, Response: {ResponseData}, ErrorCode: {ErrorCode}", className, methodName, RequestDto.TaskExternalCode, response.ToJson(), response.ErrorCode);
                    return StatusCode((int)response.ErrorCode, response);
                }

                _taskLogger.LogInformation("{ClassName}.{MethodName}: Task created Successful, with TaskExternalMappingRequest: {TaskCode}", className, methodName, RequestDto.TaskExternalCode);

                return Ok(response);
            }
            catch (Exception ex)
            {
                _taskLogger.LogError(ex, "{ClassName}.{MethodName}: An error occurred while create TaskExternalMappingRequest. Error Message: {ErrorMessage}, ErrorCode: {ErrorCode}", className, methodName, ex.Message, StatusCodes.Status500InternalServerError);
                return StatusCode(StatusCodes.Status500InternalServerError, new BaseResponseDto() { ErrorCode = StatusCodes.Status500InternalServerError });
            }
        }
    }
}
