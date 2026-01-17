using SunnyRewards.Helios.Common.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Task.Core.Domain.Dtos
{
    public class TriviaQuestionGroupResponseDto: BaseResponseDto
    {
        public List<TriviaQuestionGroupDto>? TriviaQuestionGroupList { get; set; }
    }
}
