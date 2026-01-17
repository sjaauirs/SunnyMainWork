using SunnyRewards.Helios.Common.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Task.Core.Domain.Dtos
{
    public class SubtaskResponseDto : BaseResponseDto
    {
        public SubTaskDto? Subtask { get; set; }
    }
}
