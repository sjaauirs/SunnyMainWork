using Microsoft.AspNetCore.Mvc;
using SunnyRewards.Helios.Task.Core.Domain.Dtos;
using SunnyRewards.Helios.Task.Infrastructure.Services.Interface;

namespace SunnyRewards.Helios.Task.Api.Controller
{
    [Route("api/v1/questionnaire/")]
    [ApiController]
    public class QuestionnaireController : ControllerBase
    {
        private readonly ILogger<QuestionnaireController> _logger;
        private readonly IQuestionnaireService _questionnaireService;
        const string className = nameof(QuestionnaireController);

        public QuestionnaireController(ILogger<QuestionnaireController> logger, IQuestionnaireService questionnaireService)
        {
            _logger = logger;
            _questionnaireService = questionnaireService;
        }
        /// <summary>
        ///     
        /// </summary>
        /// <param name="taskRewardId"></param>
        /// <param name="consumerCode"></param>
        /// <param name="languageCode"></param>
        /// <returns></returns>

        [HttpGet("{taskRewardId}/{languageCode?}")]
        public async Task<ActionResult<GetQuestionnaireResponseDto>> GetQuestionnaire(long taskRewardId, string consumerCode, string? languageCode)
        {
            const string methodName = nameof(GetQuestionnaire);
            try
            {
                _logger.LogInformation("{className}.{methodName}: API - Enter with {taskRewardId}", className, methodName, taskRewardId);

                var response = await _questionnaireService.GetQuestionnaire(taskRewardId, consumerCode, languageCode);
                return response != null ? Ok(response) : NotFound();

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{className}.{methodName}: API -  ERROR:{msg}, Error Code:{errorCode}", className, methodName, ex.Message, StatusCodes.Status500InternalServerError);
                return new GetQuestionnaireResponseDto() { ErrorMessage = ex.Message };

            }
        }
    }
}
