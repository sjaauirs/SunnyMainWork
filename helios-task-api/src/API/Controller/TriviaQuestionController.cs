using Microsoft.AspNetCore.Mvc;
using SunnyRewards.Helios.Task.Core.Domain.Dtos;
using SunnyRewards.Helios.Task.Infrastructure.Services.Interface;

namespace SunnyRewards.Helios.Task.Api.Controller
{
    [ApiController]
    [Route("api/v1/trivia-questions")]
    public class TriviaQuestionController : ControllerBase
    {
        private readonly ITriviaQuestionService _triviaQuestionService;
        private readonly ILogger<TriviaQuestionController> _logger;
        private const string className = nameof(TriviaQuestionController);

        public TriviaQuestionController(ITriviaQuestionService triviaQuestionService, ILogger<TriviaQuestionController> logger)
        {
            _triviaQuestionService = triviaQuestionService;
            _logger = logger;
        }

        /// <summary>
        /// Gets all trivia questions.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult<TriviaQuestionResponseDto>> GetAllTriviaQuestions([FromQuery] string? languageCode)
        {
            const string methodName = nameof(GetAllTriviaQuestions);

            try
            {
                _logger.LogInformation("{ClassName}.{MethodName}: Fetching all trivia questions.", className, methodName);

                var response = await _triviaQuestionService.GetAllTriviaQuestions(languageCode);
                _logger.LogInformation("{ClassName}.{MethodName}: Successfully fetched all trivia questions.", className, methodName);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName}: An error occurred while fetching trivia questions. Error Message: {ErrorMessage}",
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
                _logger.LogInformation("{ClassName}.{MethodName}: Update request received for TriviaQuestionCode: {TriviaQuestionCode}", className, methodName, triviaQuestionCode);
                var response = await _triviaQuestionService.UpdateTriviaQuestion(triviaQuestionCode, updateRequest);
                if (response.ErrorCode != null)
                {
                    _logger.LogError("{ClassName}.{MethodName}: Failed to update TriviaQuestion. Code: {TriviaQuestionCode}, Error: {ErrorMessage}",
                        className, methodName, triviaQuestionCode, response.ErrorMessage);
                    return StatusCode((int)response.ErrorCode, response.ErrorMessage);
                }

                _logger.LogInformation("{ClassName}.{MethodName}: TriviaQuestion successfully updated. Code: {TriviaQuestionCode}", className, methodName, triviaQuestionCode);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName}: Unexpected error while updating TriviaQuestion. Code: {TriviaQuestionCode}, Error: {ErrorMessage}",
                    className, methodName, triviaQuestionCode, ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new TriviaQuestionUpdateResponseDto { ErrorMessage = "An unexpected error occurred." });
            }
        }

    }

}
