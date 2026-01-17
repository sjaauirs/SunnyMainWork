using SunnyRewards.Helios.Common.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Task.Core.Domain.Dtos
{
    public class GetTriviaResponseDto : BaseResponseDto
    {
        public TriviaDto Trivia { get; set; } = new TriviaDto();
        public TriviaQuestionDto[] Questions { get; set; } = new TriviaQuestionDto[0];
        
    }
}
