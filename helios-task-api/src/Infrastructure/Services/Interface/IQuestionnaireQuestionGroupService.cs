using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.Task.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Task.Infrastructure.Services.Interface
{
    public interface IQuestionnaireQuestionGroupService
    {
        /// <summary>
        /// Get Questionnaire Question Groups By Questionnaire Id
        /// </summary>
        /// <param name="questionnaireId"></param>
        /// <returns></returns>
        Task<QuestionnaireQuestionGroupResponseDto> GetQuestionnaireQuestionGroupsByQuestionnaireId(long questionnaireId);


        /// <summary>
        /// Update Questionnaire Question Group
        /// </summary>
        /// <param name="questionnaireQuestionGroupId"></param>
        /// <param name="updateRequest"></param>
        /// <returns></returns>
        Task<QuestionnaireQuestionGroupUpdateResponseDto> UpdateQuestionnaireQuestionGroup(long questionnaireQuestionGroupId, QuestionnaireQuestionGroupDto updateRequest);

        /// <summary>
        /// Delete Questionnaire Question Group
        /// </summary>
        /// <param name="questionnaireQuestionGroupId"></param>
        /// <returns></returns>
        Task<BaseResponseDto> DeleteQuestionnaireQuestionGroup(long questionnaireQuestionGroupId);
    }
}
