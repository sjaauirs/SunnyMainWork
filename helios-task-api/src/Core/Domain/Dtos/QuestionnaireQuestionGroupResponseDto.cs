using SunnyRewards.Helios.Common.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Task.Core.Domain.Dtos
{
    public class QuestionnaireQuestionGroupResponseDto : BaseResponseDto
    {
        public List<QuestionnaireQuestionGroupDto>? QuestionnaireQuestionGroupList { get; set; }
    }
}
