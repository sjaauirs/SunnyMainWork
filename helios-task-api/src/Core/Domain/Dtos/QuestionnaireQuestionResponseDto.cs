using SunnyRewards.Helios.Common.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Task.Core.Domain.Dtos
{
    public class QuestionnaireQuestionResponseDto : BaseResponseDto
    {
        public List<QuestionnaireQuestionData>? QuestionnaireQuestions { get; set; }
    }
}
