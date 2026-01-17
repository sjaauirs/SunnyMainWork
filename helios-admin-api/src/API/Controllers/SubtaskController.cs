using Microsoft.AspNetCore.Mvc;
using SunnyRewards.Helios.Admin.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.Common.Core.Extensions;
using SunnyRewards.Helios.Task.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Admin.Api.Controllers
{
    [Route("api/v1/sub-task")]
    [ApiController]
    public class SubtaskController : ControllerBase
    {
        private readonly ILogger<SubtaskController> _taskLogger;
        private readonly ISubtaskService _taskService;
        private const string className = nameof(SubtaskController);

        public SubtaskController(ILogger<SubtaskController> taskLogger, ISubtaskService taskService)
        {
            _taskLogger = taskLogger;
            _taskService = taskService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateSubTask(SubtaskRequestDto createsubTaskRequestDto)
        {
            const string methodName = nameof(CreateSubTask);
            try
            {
                _taskLogger.LogInformation("{ClassName}.{MethodName}: Request started with TaskCode: {TaskCode}", className, methodName, createsubTaskRequestDto.ParentTaskRewardCode);
                var response = await _taskService.CreateSubTask(createsubTaskRequestDto);

                if (response.ErrorCode != null)
                {
                    _taskLogger.LogError("{ClassName}.{MethodName}: Error occurred while creating Task. Request: {RequestData}, Response: {ResponseData}, ErrorCode: {ErrorCode}", className, methodName, createsubTaskRequestDto.ToJson(), response.ToJson(), response.ErrorCode);
                    return StatusCode((int)response.ErrorCode, response);
                }

                _taskLogger.LogInformation("{ClassName}.{MethodName}: Task created Successful, with TaskCode: {TaskCode}", className, methodName, createsubTaskRequestDto.ParentTaskRewardCode);

                return Ok(response);
            }
            catch (Exception ex)
            {
                _taskLogger.LogError(ex, "{ClassName}.{MethodName}: An error occurred while create Task. Error Message: {ErrorMessage}, ErrorCode: {ErrorCode}", className, methodName, ex.Message, StatusCodes.Status500InternalServerError);
                return StatusCode(StatusCodes.Status500InternalServerError, new BaseResponseDto() { ErrorCode = StatusCodes.Status500InternalServerError });
            }
        }
    }
}
