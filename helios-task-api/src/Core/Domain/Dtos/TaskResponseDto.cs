using SunnyRewards.Helios.Common.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Task.Core.Domain.Dtos
{
    public class TaskResponseDto : BaseResponseDto
    {
        public TaskDto? Task { get; set; }
    }
}
