using SunnyRewards.Helios.Common.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Task.Core.Domain.Dtos
{
    public class GetTaskByTaskNameResponseDto : BaseResponseDto
    {
        public TaskDto? TaskDto { get; set; }
    }
}
