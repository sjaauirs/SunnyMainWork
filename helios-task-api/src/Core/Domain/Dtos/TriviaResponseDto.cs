using SunnyRewards.Helios.Common.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Task.Core.Domain.Dtos
{
    public class TriviaResponseDto : BaseResponseDto
    {
        public List<TriviaDataDto>? TriviaList { get; set; }
    }
}
