using SunnyRewards.Helios.Common.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Task.Core.Domain.Dtos
{
    public class GetQuestionnaireResponseDto : BaseResponseDto
    {
        public QuestionnaireDto Questionnaire { get; set; } = new QuestionnaireDto();
        public QuestionnaireQuestionDto[] Questions { get; set; } = new QuestionnaireQuestionDto[0];
    }
}
