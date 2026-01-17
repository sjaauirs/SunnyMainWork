using SunnyRewards.Helios.Common.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Task.Core.Domain.Dtos
{
    public class TriviaQuestionResponseDto : BaseResponseDto
    {
        public List<TriviaQuestionData>? TriviaQuestions { get; set; }
    }
}
