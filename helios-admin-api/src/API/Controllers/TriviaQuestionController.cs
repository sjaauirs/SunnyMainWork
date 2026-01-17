using Microsoft.AspNetCore.Mvc;
using SunnyRewards.Helios.Admin.Infrastructure.Services;
using SunnyRewards.Helios.Admin.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.Common.Core.Extensions;
using SunnyRewards.Helios.Task.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Admin.Api.Controllers
{
    [Route("api/v1/trivia-question")]
    [ApiController]
    public class TriviaQuestionController : ControllerBase
    {
        private readonly ILogger<TriviaQuestionController> _triviaLogger;
        private readonly ITriviaQuestionService _triviaService;
        private const string className = nameof(TriviaQuestionController);

        public TriviaQuestionController(ILogger<TriviaQuestionController> triviaLogger, ITriviaQuestionService triviaService)
        {
            _triviaLogger = triviaLogger;
            _triviaService = triviaService;
        }
        [HttpPost]
        public async Task<IActionResult> CreateTriviaQuestion(TriviaQuestionRequestDto triviaQuestionDto)
        {
            const string methodName = nameof(CreateTriviaQuestion);
            try
            {
                _triviaLogger.LogInformation("{ClassName}.{MethodName}: Request started with triviaQuestionCode: {TaskCode}", className, methodName, triviaQuestionDto.TriviaQuestionCode);
                var response = await _triviaService.CreateTriviaQuestion(triviaQuestionDto);

                if (response.ErrorCode != null)
                {
                    _triviaLogger.LogError("{ClassName}.{MethodName}: Error occurred while creating triviaQuestion. Request: {RequestData}, Response: {ResponseData}, ErrorCode: {ErrorCode}", className, methodName, triviaQuestionDto.ToJson(), response.ToJson(), response.ErrorCode);
                    return StatusCode((int)response.ErrorCode, response);
                }

                _triviaLogger.LogInformation("{ClassName}.{MethodName}: triviaQuestion created Successful, with triviaQuestionCode: {TaskCode}", className, methodName, triviaQuestionDto.TriviaQuestionCode);

                return Ok(response);
            }
            catch (Exception ex)
            {
                _triviaLogger.LogError(ex, "{ClassName}.{MethodName}: An error occurred while create triviaQuestion. Error Message: {ErrorMessage}, ErrorCode: {ErrorCode}", className, methodName, ex.Message, StatusCodes.Status500InternalServerError);
                return StatusCode(StatusCodes.Status500InternalServerError, new BaseResponseDto() { ErrorCode = StatusCodes.Status500InternalServerError });
            }
        }

        /// <summary>
        /// Gets all trivia questions.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult<TriviaQuestionResponseDto>> GetAllTriviaQuestions([FromQuery]  string? languageCode)
        {
            const string methodName = nameof(GetAllTriviaQuestions);

            try
            {
                _triviaLogger.LogInformation("{ClassName}.{MethodName}: Fetching all trivia questions.", className, methodName);

                var response = await _triviaService.GetAllTriviaQuestions(languageCode);
                _triviaLogger.LogInformation("{ClassName}.{MethodName}: Successfully fetched all trivia questions.", className, methodName);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _triviaLogger.LogError(ex, "{ClassName}.{MethodName}: An error occurred while fetching trivia questions. Error Message: {ErrorMessage}",
                    className, methodName, ex.Message);

                return StatusCode(StatusCodes.Status500InternalServerError,
                    new TriviaQuestionResponseDto
                    {
                        TriviaQuestions = null
                    });
            }
        }

        /// <summary>
        /// Updates the trivia question.
        /// </summary>
        /// <param name="triviaQuestionCode">The trivia question code.</param>
        /// <param name="updateRequest">The update request.</param>
        /// <returns></returns>
        [HttpPut("{triviaQuestionCode}")]
        public async Task<ActionResult<TriviaQuestionUpdateResponseDto>> UpdateTriviaQuestion(string triviaQuestionCode, [FromBody] TriviaQuestionData updateRequest)
        {
            const string methodName = nameof(UpdateTriviaQuestion);
            try
            {
                _triviaLogger.LogInformation("{ClassName}.{MethodName}: Update request received for TriviaQuestionCode: {TriviaQuestionCode}", className, methodName, triviaQuestionCode);
                var response = await _triviaService.UpdateTriviaQuestion(triviaQuestionCode, updateRequest);
                if (response.ErrorCode != null)
                {
                    _triviaLogger.LogError("{ClassName}.{MethodName}: Failed to update TriviaQuestion. Code: {TriviaQuestionCode}, Error: {ErrorMessage}",
                        className, methodName, triviaQuestionCode, response.ErrorMessage);
                    return StatusCode((int)response.ErrorCode, response.ErrorMessage);
                }

                _triviaLogger.LogInformation("{ClassName}.{MethodName}: TriviaQuestion successfully updated. Code: {TriviaQuestionCode}", className, methodName, triviaQuestionCode);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _triviaLogger.LogError(ex, "{ClassName}.{MethodName}: Unexpected error while updating TriviaQuestion. Code: {TriviaQuestionCode}, Error: {ErrorMessage}",
                    className, methodName, triviaQuestionCode, ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new TriviaQuestionUpdateResponseDto { ErrorMessage = "An unexpected error occurred." });
            }
        }
    }
}
