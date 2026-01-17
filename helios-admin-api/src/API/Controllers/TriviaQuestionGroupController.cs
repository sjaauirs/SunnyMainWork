using Microsoft.AspNetCore.Mvc;
using SunnyRewards.Helios.Admin.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.Common.Core.Extensions;
using SunnyRewards.Helios.Task.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Admin.Api.Controllers
{
    [Route("api/v1/trivia-question-group")]
    [ApiController]
    public class TriviaQuestionGroupController : ControllerBase
    {
        private readonly ILogger<TriviaQuestionGroupController> _logger;
        private readonly ITriviaQuestionGroupService _triviaQuestionGroupService;
        private const string className = nameof(TriviaQuestionGroupController);

        public TriviaQuestionGroupController(ILogger<TriviaQuestionGroupController> logger, ITriviaQuestionGroupService triviaQuestionGroupService)
        {
            _logger = logger;
            _triviaQuestionGroupService = triviaQuestionGroupService;
        }
        [HttpPost]
        public async Task<IActionResult> CreateTriviaQuestionGroup(TriviaQuestionGroupRequestDto requestDto)
        {
            const string methodName = nameof(CreateTriviaQuestionGroup);
            try
            {
                _logger.LogInformation("{ClassName}.{MethodName}: Request started with TriviaQuestionCode: {TaskCode}", className, methodName, requestDto.TriviaQuestionCode);
                var response = await _triviaQuestionGroupService.CreateTriviaQuestionGroup(requestDto);

                if (response.ErrorCode != null)
                {
                    _logger.LogError("{ClassName}.{MethodName}: Error occurred while creating requestDto.TriviaQuestionCode. Request: {RequestData}, Response: {ResponseData}, ErrorCode: {ErrorCode}", className, methodName, requestDto.ToJson(), response.ToJson(), response.ErrorCode);
                    return StatusCode((int)response.ErrorCode, response);
                }

                _logger.LogInformation("{ClassName}.{MethodName}: triviaQuestion created Successful, with TriviaQuestionCode: {TaskCode}", className, methodName, requestDto.TriviaQuestionCode);

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName}: An error occurred while create triviaQuestion. Error Message: {ErrorMessage}, ErrorCode: {ErrorCode}", className, methodName, ex.Message, StatusCodes.Status500InternalServerError);
                return StatusCode(StatusCodes.Status500InternalServerError, new BaseResponseDto() { ErrorCode = StatusCodes.Status500InternalServerError });
            }
        }

        /// <summary>
        /// Gets the trivia question groups by trivia id
        /// </summary>
        /// <param name="triviaId">The trivia identifier.</param>
        /// <returns></returns>
        [HttpGet("{triviaId}")]
        public async Task<ActionResult<TriviaQuestionGroupResponseDto>> GetTriviaQuestionGroupsByTriviaId(long triviaId)
        {
            const string methodName = nameof(GetTriviaQuestionGroupsByTriviaId);

            try
            {
                _logger.LogInformation("{className}.{methodName}: Fetching trivia question groups for TriviaId: {TriviaId}.", className, methodName, triviaId);
                var response = await _triviaQuestionGroupService.GetTriviaQuestionGroupsByTriviaId(triviaId);
                _logger.LogInformation("{className}.{methodName}: Successfully fetched trivia question groups for TriviaId: {TriviaId}.", className, methodName, triviaId);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{className}.{methodName}: An unexpected error occurred while fetching trivia question groups. TriviaId: {TriviaId}, Error: {ErrorMessage}.",
                    className, methodName, triviaId, ex.Message);

                return StatusCode(StatusCodes.Status500InternalServerError,
                    new TriviaQuestionGroupResponseDto
                    {
                        ErrorCode = StatusCodes.Status500InternalServerError,
                        ErrorMessage = "An unexpected error occurred while retrieving trivia question groups."
                    });
            }
        }

        /// <summary>
        /// Updates the trivia question group.
        /// </summary>
        /// <param name="triviaQuestionGroupId">The trivia question group identifier.</param>
        /// <param name="updateRequest">The update request.</param>
        /// <returns></returns>
        [HttpPut("{triviaQuestionGroupId}")]
        public async Task<ActionResult<TriviaQuestionGroupUpdateResponseDto>> UpdateTriviaQuestionGroup(long triviaQuestionGroupId, [FromBody] TriviaQuestionGroupDto updateRequest)
        {
            const string methodName = nameof(UpdateTriviaQuestionGroup);

            try
            {
                _logger.LogInformation("{className}.{methodName}: Updating trivia question group. Id: {TriviaQuestionGroupId}.", className, methodName, triviaQuestionGroupId);

                var response = await _triviaQuestionGroupService.UpdateTriviaQuestionGroup(triviaQuestionGroupId, updateRequest);
                if (response.ErrorCode != null)
                {
                    _logger.LogError("{className}.{methodName}: Failed to update trivia question group. Id: {TriviaQuestionGroupId}, Error: {ErrorMessage}.",
                        className, methodName, triviaQuestionGroupId, response.ErrorMessage);
                    return StatusCode((int)response.ErrorCode, response);
                }

                _logger.LogInformation("{className}.{methodName}: Successfully updated trivia question group. Id: {TriviaQuestionGroupId}.", className, methodName, triviaQuestionGroupId);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{className}.{methodName}: An unexpected error occurred while updating trivia question group. Id: {TriviaQuestionGroupId}, Error: {ErrorMessage}.",
                    className, methodName, triviaQuestionGroupId, ex.Message);

                return StatusCode(StatusCodes.Status500InternalServerError,
                    new TriviaQuestionGroupUpdateResponseDto
                    {
                        ErrorCode = StatusCodes.Status500InternalServerError,
                        ErrorMessage = "An unexpected error occurred while updating the trivia question group.",
                        IsSuccess = false
                    });
            }
        }


        /// <summary>
        /// Deletes the trivia question group.
        /// </summary>
        /// <param name="triviaQuestionGroupId">The trivia question group identifier.</param>
        /// <returns></returns>
        [HttpDelete("{triviaQuestionGroupId}")]
        public async Task<ActionResult<BaseResponseDto>> DeleteTriviaQuestionGroup(long triviaQuestionGroupId)
        {
            const string methodName = nameof(DeleteTriviaQuestionGroup);

            try
            {
                _logger.LogInformation("{ClassName}.{MethodName}: Attempting to delete trivia question group. Id: {TriviaQuestionGroupId}.", className, methodName, triviaQuestionGroupId);

                var response = await _triviaQuestionGroupService.DeleteTriviaQuestionGroup(triviaQuestionGroupId);

                if (response.ErrorCode != null)
                {
                    _logger.LogError("{ClassName}.{MethodName}: Failed to delete trivia question group. Id: {TriviaQuestionGroupId}, Error: {ErrorMessage}.",
                        className, methodName, triviaQuestionGroupId, response.ErrorMessage);
                    return StatusCode((int)response.ErrorCode, response);
                }

                _logger.LogInformation("{ClassName}.{MethodName}: Successfully deleted trivia question group. Id: {TriviaQuestionGroupId}.", className, methodName, triviaQuestionGroupId);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName}: An unexpected error occurred while deleting trivia question group. Id: {TriviaQuestionGroupId}, Error: {ErrorMessage}.",
                    className, methodName, triviaQuestionGroupId, ex.Message);

                return StatusCode(StatusCodes.Status500InternalServerError,
                    new BaseResponseDto
                    {
                        ErrorCode = StatusCodes.Status500InternalServerError,
                        ErrorMessage = "An unexpected error occurred while deleting the trivia question group."
                    });
            }
        }
    }
}
