using SunnyRewards.Helios.Common.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Task.Core.Domain.Dtos
{
    public class TasksResponseDto : BaseResponseDto
    {
        public IList<TaskDto>? Tasks { get; set; }
    }
}
