using Microsoft.AspNetCore.Mvc;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.Common.Core.Extensions;
using SunnyRewards.Helios.Task.Core.Domain.Dtos;
using SunnyRewards.Helios.Task.Infrastructure.Services.Interface;

namespace SunnyRewards.Helios.Task.Api.Controller
{
    [Route("api/v1")]
    [ApiController]
    public class ImportTaskRewardCollectionController : ControllerBase
    {
        private readonly ILogger<ImportTaskRewardCollectionController> _taskLogger;
        private readonly IImportTaskRewardCollectionService _importTaskRewardCollectionService;
        private const string className = nameof(ImportTaskController);

        public ImportTaskRewardCollectionController(ILogger<ImportTaskRewardCollectionController> taskLogger, IImportTaskRewardCollectionService importTaskRewardCollectionService)
        {
            _taskLogger = taskLogger;
            _importTaskRewardCollectionService = importTaskRewardCollectionService;
        }

        /// <summary>
        /// Imports a task reward collection by processing the provided request DTO.
        /// Logs the start and completion of the process, and handles any exceptions that occur.
        /// </summary>
        /// <param name="requestDto">The request data containing details of the task reward collection to import.</param>
        /// <returns>
        /// Returns an HTTP 200 OK response with the import result if successful, or an HTTP 500 Internal Server Error 
        /// with error details in case of failure.
        /// </returns>
        [HttpPost("import-task-reward-collection")]
        public async Task<IActionResult> ImportTaskRewardCollection([FromBody] ImportTaskRewardCollectionRequestDto requestDto)
        {
            const string methodName = nameof(ImportTaskRewardCollection);
            try
            {
                _taskLogger.LogInformation("{ClassName}.{MethodName}: Request started with Import Task Reward Collection : {dto}", className, methodName, requestDto.ToJson());
                var response = await _importTaskRewardCollectionService.ImportTaskRewardCollection(requestDto);

                _taskLogger.LogInformation("{ClassName}.{MethodName}: Import Task Reward Create successful for Import Task Reward Collection : {dyo}", className, methodName, requestDto.ToJson());

                return Ok(response);
            }
            catch (Exception ex)
            {
                _taskLogger.LogError(ex, "{ClassName}.{MethodName}: An error occurred during taskRewardCollection import. Error Message: {ErrorMessage}, ErrorCode: {ErrorCode}", className, methodName, ex.Message, StatusCodes.Status500InternalServerError);
                return StatusCode(StatusCodes.Status500InternalServerError, new BaseResponseDto() { ErrorCode = StatusCodes.Status500InternalServerError });
            }

        }
    }
}
