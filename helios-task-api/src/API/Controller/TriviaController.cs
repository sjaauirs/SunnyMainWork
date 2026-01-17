using Microsoft.AspNetCore.Mvc;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.Common.Core.Extensions;
using SunnyRewards.Helios.Task.Core.Domain.Dtos;
using SunnyRewards.Helios.Task.Infrastructure.Services;
using SunnyRewards.Helios.Task.Infrastructure.Services.Interface;

namespace SunnyRewards.Helios.Task.Api.Controller
{
    [Route("api/v1/trivia/")]
    [ApiController]
    public class TriviaController : ControllerBase
    {

        private readonly ILogger<TriviaController> _triviaLogger;
        private readonly ITriviaService _triviaService;
        const string className = nameof(TriviaController);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="triviaLogger"></param>
        /// <param name="triviaService"></param>
        public TriviaController(ILogger<TriviaController> triviaLogger, ITriviaService triviaService)
        {
            _triviaLogger = triviaLogger;
            _triviaService = triviaService;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="taskRewardId"></param>
        /// <param name="consumerCode"></param>
        /// <returns></returns>
        [HttpGet("{taskRewardId}/{languageCode?}")]
        public async Task<ActionResult<GetTriviaResponseDto>> GetTrivia(long taskRewardId, string consumerCode, string? languageCode)
        {
            const string methodName = nameof(GetTrivia);
            try
            {
                _triviaLogger.LogInformation("{className}.{methodName}: API - Enter with {taskRewardId}", className, methodName, taskRewardId);

                var response = await _triviaService.GetTrivia(taskRewardId, consumerCode, languageCode);
                return response != null ? Ok(response) : NotFound();

            }
            catch (Exception ex)
            {
                _triviaLogger.LogError(ex, "{className}.{methodName}: API -  ERROR:{msg}, Error Code:{errorCode}", className, methodName, ex.Message, StatusCodes.Status500InternalServerError);
                return new GetTriviaResponseDto() { ErrorMessage = ex.Message };

            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="postTaskProgressUpdateRequestDto"></param>
        /// <returns></returns>
        [HttpPost("task-progress-update")]
        public async Task<ActionResult<PostTaskProgressUpdateResponseDto>> TaskProgressUpdate([FromBody] PostTaskProgressUpdateRequestDto postTaskProgressUpdateRequestDto)
        {
            const string methodName = nameof(TaskProgressUpdate);
            try
            {
                _triviaLogger.LogInformation("{className}.{methodName}: API - Enter with {consumerCode}", className, methodName, postTaskProgressUpdateRequestDto.ConsumerCode);

                var response = await _triviaService.TaskProgressUpdate(postTaskProgressUpdateRequestDto);
                return response != null ? Ok(response) : NotFound();

            }
            catch (Exception ex)
            {
                _triviaLogger.LogError(ex, "{className}.{methodName}: API -  ERROR:{msg}, Error Code:{errorCode}", className, methodName, ex.Message, StatusCodes.Status500InternalServerError);
                return new PostTaskProgressUpdateResponseDto() { ErrorMessage = ex.Message };

            }
        }
        [HttpPost("trivia")]
        public async Task<ActionResult<BaseResponseDto>> CreateTrivia([FromBody] TriviaRequestDto triviaDto)
        {
            const string methodName = nameof(CreateTrivia);
            try
            {
                _triviaLogger.LogInformation("{className}.{methodName}: API - Enter with trivia code {code}", className, methodName, triviaDto.trivia.TriviaCode);

                var response = await _triviaService.CreateTrivia(triviaDto);
                return response != null ? Ok(response) : NotFound();

            }
            catch (Exception ex)
            {
                _triviaLogger.LogError(ex, "{className}.{methodName}: API -  ERROR:{msg}, Error Code:{errorCode}", className, methodName, ex.Message, StatusCodes.Status500InternalServerError);
                return StatusCode(StatusCodes.Status500InternalServerError, new BaseResponseDto() { ErrorCode = StatusCodes.Status500InternalServerError });

            }
        }
        [HttpPost("trivia-question")]
        public async Task<ActionResult<BaseResponseDto>> CreateTriviaQuestion([FromBody] TriviaQuestionRequestDto triviaQuestionDto)
        {
            const string methodName = nameof(CreateTriviaQuestion);
            try
            {
                _triviaLogger.LogInformation("{className}.{methodName}: API - Enter with trivia code {code}", className, methodName, triviaQuestionDto.TriviaQuestionCode);

                var response = await _triviaService.CreateTriviaQuestion(triviaQuestionDto);
                return response != null ? Ok(response) : NotFound();

            }
            catch (Exception ex)
            {
                _triviaLogger.LogError(ex, "{className}.{methodName}: API -  ERROR:{msg}, Error Code:{errorCode}", className, methodName, ex.Message, StatusCodes.Status500InternalServerError);
                return StatusCode(StatusCodes.Status500InternalServerError, new BaseResponseDto() { ErrorCode = StatusCodes.Status500InternalServerError });

            }
        }
        [HttpPost("trivia-question-group")]
        public async Task<ActionResult<BaseResponseDto>> CreateTriviaQuestionGroup([FromBody] TriviaQuestionGroupRequestDto triviaQuestionGroupDto)
        {
            const string methodName = nameof(CreateTriviaQuestionGroup);
            try
            {
                _triviaLogger.LogInformation("{className}.{methodName}: API - Enter with trivia code {code}", className, methodName, triviaQuestionGroupDto.TriviaQuestionCode);

                var response = await _triviaService.CreateTriviaQuestionGroup(triviaQuestionGroupDto);
                return response != null ? Ok(response) : NotFound();

            }
            catch (Exception ex)
            {
                _triviaLogger.LogError(ex, "{className}.{methodName}: API -  ERROR:{msg}, Error Code:{errorCode}", className, methodName, ex.Message, StatusCodes.Status500InternalServerError);
                return StatusCode(StatusCodes.Status500InternalServerError, new BaseResponseDto() { ErrorCode = StatusCodes.Status500InternalServerError });

            }
        }

        /// <summary>
        /// Retrieves all trivia data from the system.
        /// </summary>
        /// <returns>
        /// An <see cref="ActionResult{T}"/> containing a <see cref="TriviaResponseDto"/> object with the trivia data.
        /// On success, returns HTTP 200 (OK) with the trivia data.
        /// On failure, returns appropriate HTTP status codes (e.g., 500 for internal server errors).
        /// </returns>
        /// <remarks>
        /// Logs the operation's start, success, and error scenarios.
        /// Handles errors gracefully by logging and returning a meaningful error message in the response.
        /// </remarks>
        /// <exception cref="Exception">
        /// Captures and logs any unexpected errors that occur during the process.
        /// </exception>
        [HttpGet]
        public async Task<ActionResult<TriviaResponseDto>> GetAllTrivia()
        {
            const string methodName = nameof(GetAllTrivia);
            try
            {
                _triviaLogger.LogInformation("{ClassName}.{MethodName}: Fetching all trivia data.", className, methodName);
                var response = await _triviaService.GetAllTrivia();
                _triviaLogger.LogInformation("{ClassName}.{MethodName}: Successfully fetched all trivia data.", className, methodName);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _triviaLogger.LogError(ex, "{ClassName}.{MethodName}: An unexpected error occurred while fetching trivia data. Error Message: {ErrorMessage}",
                    className, methodName, ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new TriviaResponseDto
                    {
                        ErrorCode = StatusCodes.Status500InternalServerError,
                        ErrorMessage = "An unexpected error occurred while retrieving trivia data. Please try again later."
                    });
            }
        }
    }
}
