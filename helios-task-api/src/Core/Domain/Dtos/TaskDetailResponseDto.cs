using SunnyRewards.Helios.Common.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Task.Core.Domain.Dtos
{
    public class TaskDetailResponseDto : BaseResponseDto
    {
        public TaskDetailDto? TaskDetail { get; set; }
    }
}
