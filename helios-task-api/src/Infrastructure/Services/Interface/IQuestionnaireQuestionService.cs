using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.Task.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Task.Infrastructure.Services.Interface
{
    public interface IQuestionnaireQuestionService
    {
        /// <summary>
        /// Gets all questionnaire questions.
        /// </summary>
        /// <returns></returns>
        Task<QuestionnaireQuestionResponseDto> GetAllQuestionnaireQuestions(string? languageCode);
        /// <summary>
        /// Updates the questionnaire question.
        /// </summary>
        /// <param name="questionnaireQuestionCode">The questionnaire question code.</param>
        /// <param name="updateRequest">The update request.</param>
        /// <returns></returns>
        Task<QuestionnaireQuestionUpdateResponseDto> UpdateQuestionnaireQuestion(string questionnaireQuestionCode, QuestionnaireQuestionData updateRequest);

        /// <summary>
        /// Creates the questionnaire question.
        /// </summary>
        /// <param name="questionnaireQuestionDto"></param>
        /// <returns></returns>
        Task<BaseResponseDto> CreateQuestionnaireQuestion(QuestionnaireQuestionRequestDto questionnaireQuestionDto);
    }
}
