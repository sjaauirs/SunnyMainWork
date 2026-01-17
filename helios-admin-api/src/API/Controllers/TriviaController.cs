using Microsoft.AspNetCore.Mvc;
using SunnyRewards.Helios.Admin.Infrastructure.Services;
using SunnyRewards.Helios.Admin.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.Common.Core.Extensions;
using SunnyRewards.Helios.Task.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Admin.Api.Controllers
{
    [Route("api/v1/trivia")]
    [ApiController]

    public class TriviaController : ControllerBase
    {
        private readonly ILogger<TriviaController> _triviaLogger;
        private readonly ITriviaService _triviaService;
        private const string className = nameof(TriviaController);

        public TriviaController(ILogger<TriviaController> triviaLogger, ITriviaService triviaService)
        {
            _triviaLogger = triviaLogger;
            _triviaService = triviaService;
        }
        [HttpPost]
        public async Task<IActionResult> CreateTrivia(TriviaRequestDto triviaDto)
        {
            const string methodName = nameof(CreateTrivia);
            try
            {
                _triviaLogger.LogInformation("{ClassName}.{MethodName}: Request started with triviaCode: {TaskCode}", className, methodName, triviaDto.trivia.TriviaCode);
                var response = await _triviaService.CreateTrivia(triviaDto);

                if (response.ErrorCode != null)
                {
                    _triviaLogger.LogError("{ClassName}.{MethodName}: Error occurred while creating trivia. Request: {RequestData}, Response: {ResponseData}, ErrorCode: {ErrorCode}", className, methodName, triviaDto.ToJson(), response.ToJson(), response.ErrorCode);
                    return StatusCode((int)response.ErrorCode, response);
                }

                _triviaLogger.LogInformation("{ClassName}.{MethodName}: trivia created Successful, with triviaCode: {TaskCode}", className, methodName, triviaDto.trivia.TriviaCode);

                return Ok(response);
            }
            catch (Exception ex)
            {
                _triviaLogger.LogError(ex, "{ClassName}.{MethodName}: An error occurred while create trivia. Error Message: {ErrorMessage}, ErrorCode: {ErrorCode}", className, methodName, ex.Message, StatusCodes.Status500InternalServerError);
                return StatusCode(StatusCodes.Status500InternalServerError, new BaseResponseDto() { ErrorCode = StatusCodes.Status500InternalServerError });
            }
        }

        /// <summary>
        /// Gets all trivia.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult<TriviaResponseDto>> GetAllTrivia()
        {
            const string methodName = nameof(GetAllTrivia);
            try
            {
                _triviaLogger.LogInformation("{ClassName}.{MethodName}: Fetching all trivia data.", className, methodName);
                var response = await _triviaService.GetAllTrivia();
                if (response.ErrorCode != null)
                {
                    _triviaLogger.LogError("{ClassName}.{MethodName}: Error occurred while fetching all trivia data. Response: {ResponseData}, ErrorCode: {ErrorCode}", className, methodName,  response.ToJson(), response.ErrorCode);
                    return StatusCode((int)response.ErrorCode, response);
                }
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
