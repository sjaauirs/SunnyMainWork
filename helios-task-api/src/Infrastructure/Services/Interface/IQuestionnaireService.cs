using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.Task.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Task.Infrastructure.Services.Interface
{
    public interface IQuestionnaireService
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="taskRewardId"></param>
        /// <param name="consumerCode"></param>
        /// <param name="languageCode"></param>
        /// <returns></returns>
        Task<GetQuestionnaireResponseDto> GetQuestionnaire(long taskRewardId, string consumerCode, string? languageCode);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="questionnaireJson"></param>
        /// <param name="language"></param>
        /// <returns></returns>

        /// <summary>
        /// Update the questionnaire.
        /// </summary>
        /// <param name="requestDto"></param>
        /// <returns></returns>
        Task<BaseResponseDto> UpdateQuestionnaire(QuestionnaireRequestDto requestDto);
        /// <summary>
        /// Create the questionnaire.
        /// </summary>
        /// <param name="triviaDto"></param>
        /// <returns></returns>
        Task<BaseResponseDto> CreateQuestionnaire(QuestionnaireRequestDto requestDto);

        Task<BaseResponseDto> CreateQuestionnaireQuestionGroup(QuestionnaireQuestionGroupRequestDto requestDto);
    }
}
