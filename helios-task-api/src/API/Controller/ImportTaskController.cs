using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.Common.Core.Extensions;
using SunnyRewards.Helios.Task.Core.Domain.Dtos;
using SunnyRewards.Helios.Task.Infrastructure.Services.Interface;

namespace SunnyRewards.Helios.Task.Api.Controller
{
    [Route("api/v1/")]
    [ApiController]
    public class ImportTaskController : ControllerBase
    {
        private readonly ILogger<ImportTaskController> _taskLogger;
        private readonly IImportTaskService _importTaskService;
        private readonly IImportTriviaService _importTriviaService;
        private readonly IImportQuestionnaireService _importQuestionnaireService;
        private const string className = nameof(ImportTaskController);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="taskLogger"></param>
        /// <param name="taskService"></param>
        public ImportTaskController(ILogger<ImportTaskController> taskLogger, IImportTaskService importTaskService, IImportTriviaService importTriviaService, IImportQuestionnaireService importQuestionnaireService)
        {
            _taskLogger = taskLogger;
            _importTaskService = importTaskService;
            _importTriviaService = importTriviaService;
            _importQuestionnaireService = importQuestionnaireService;
        }
        [HttpPost("import-task")]
        public async Task<IActionResult> ImportTask([FromBody] ImportTaskRewardDetailsRequestDto taskRequestDto)
        {
            const string methodName = nameof(ImportTask);
            try
            {
                _taskLogger.LogInformation("{ClassName}.{MethodName}: Request started with Import Task Reward Detail : {dto}", className, methodName, taskRequestDto.ToJson());
                var response = await _importTaskService.ImportTask(taskRequestDto);
                if (response.ErrorCode != null)
                {
                    _taskLogger.LogError("{ClassName}.{MethodName}: Error occurred during Import Task Reward Create. Request: {RequestData}, Response: {ResponseData}, ErrorCode: {ErrorCode}", className, methodName, taskRequestDto.ToJson(), response.ToJson(), response.ErrorCode);
                    return StatusCode((int)response.ErrorCode, response);
                }

                _taskLogger.LogInformation("{ClassName}.{MethodName}: Import Task Reward Create successful for Import Task Reward : {dyo}", className, methodName, taskRequestDto.ToJson());

                return Ok(response);
            }
            catch (Exception ex)
            {
                _taskLogger.LogError(ex, "{ClassName}.{MethodName}: An error occurred during task import. Error Message: {ErrorMessage}, ErrorCode: {ErrorCode}", className, methodName, ex.Message, StatusCodes.Status500InternalServerError);
                return StatusCode(StatusCodes.Status500InternalServerError, new BaseResponseDto() { ErrorCode = StatusCodes.Status500InternalServerError });
            }

        }
        /// <summary>
        /// Import Trivia
        /// </summary>
        /// <param name="triviaRequestDto"></param>
        /// <returns></returns>
        [HttpPost("import-trivia")]
        public async Task<IActionResult> ImportTrivia([FromBody] ImportTriviaRequestDto triviaRequestDto)
        {
            const string methodName = nameof(ImportTrivia);
            try
            {
                _taskLogger.LogInformation("{ClassName}.{MethodName}: Request started with Import trivia Detail : {dto}", className, methodName, triviaRequestDto.ToJson());
                var triviaResponse = await _importTriviaService.ImportTrivia(triviaRequestDto);
                if (triviaResponse.ErrorCode != null)
                {
                    _taskLogger.LogError("{ClassName}.{MethodName}: Error occurred during Import trivia Create. Request: {RequestData}, Response: {ResponseData}", className, methodName, triviaRequestDto.ToJson(), triviaResponse.ToJson());
                    return StatusCode((int)triviaResponse.ErrorCode, triviaResponse);
                }


                return Ok(triviaResponse);
            }
            catch (Exception ex)
            {
                _taskLogger.LogError(ex, "{ClassName}.{MethodName}: An error occurred during task import. Error Message: {ErrorMessage}, triviaErrorCode: {triviaErrorCode}", className, methodName, ex.Message, StatusCodes.Status500InternalServerError);
                return StatusCode(StatusCodes.Status500InternalServerError, new BaseResponseDto() { ErrorCode = StatusCodes.Status500InternalServerError , ErrorMessage="An Exception Occured"});
            }

        }

        [HttpPost("import-questionnaire")]
        public async Task<IActionResult> ImportQuestionnaire([FromBody] ImportQuestionnaireRequestDto questionnaireRequestDto)
        {
            const string methodName = nameof(ImportQuestionnaire);
            try
            {
                _taskLogger.LogInformation("{ClassName}.{MethodName}: Request started with Import trivia Detail : {dto}", className, methodName, questionnaireRequestDto.ToJson());
                var questionnaireResponse = await _importQuestionnaireService.ImportQuestionnaire(questionnaireRequestDto);
                if (questionnaireResponse.ErrorCode != null)
                {
                    _taskLogger.LogError("{ClassName}.{MethodName}: Error occurred during Import questionnaire Create. Request: {RequestData}, Response: {ResponseData}", className, methodName, questionnaireRequestDto.ToJson(), questionnaireResponse.ToJson());
                    return StatusCode((int)questionnaireResponse.ErrorCode, questionnaireResponse);
                }


                return Ok(questionnaireResponse);
            }
            catch (Exception ex)
            {
                _taskLogger.LogError(ex, "{ClassName}.{MethodName}: An error occurred during task import. Error Message: {ErrorMessage}, questionnaireErrorCode: {questionnaireErrorCode}", className, methodName, ex.Message, StatusCodes.Status500InternalServerError);
                return StatusCode(StatusCodes.Status500InternalServerError, new BaseResponseDto() { ErrorCode = StatusCodes.Status500InternalServerError, ErrorMessage = "An Exception Occured" });
            }

        }
    }
}
