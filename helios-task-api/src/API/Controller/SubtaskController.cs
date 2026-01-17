using Microsoft.AspNetCore.Mvc;
using SunnyRewards.Helios.Task.Core.Domain.Dtos;
using SunnyRewards.Helios.Task.Infrastructure.Services.Interface;

namespace SunnyRewards.Helios.Task.Api.Controller
{
    [Route("api/v1/")]
    [ApiController]
    public class SubtaskController : ControllerBase
    {
        private readonly ILogger<SubtaskController> _logger;
        private readonly ISubtaskService _subtaskService;
        public const string className = nameof(SubtaskController);

        public SubtaskController(ILogger<SubtaskController> logger, ISubtaskService SubtaskService)
        {
            _logger = logger;
            _subtaskService = SubtaskService;
        }
        
        /// <summary>
        /// Updates an existing Subtask based on the provided request data.
        /// </summary>
        /// <param name="requestDto">The request data containing the details to update.</param>
        /// <returns>A response DTO indicating success or failure.</returns>
        [HttpPut("subtask/{subtaskId}")]
        public async Task<IActionResult> UpdateSubtask(long subtaskId, [FromBody] SubTaskUpdateRequestDto requestDto)
        {
            const string methodName = nameof(UpdateSubtask);

            if (subtaskId != requestDto.SubTaskId)
            {
                _logger.LogInformation("{ClassName}.{MethodName}: Mismatch between SubtaskId in query parameter and object.", className, methodName);

                return StatusCode(StatusCodes.Status400BadRequest, "Mismatch between SubtaskId in query parameter and object.");
            }

            var response = await _subtaskService.UpdateSubtask(requestDto);
            if (response.ErrorCode != null)
            {
                return StatusCode((int)response.ErrorCode, response);
            }

            return Ok(response);
        }
    }
}
