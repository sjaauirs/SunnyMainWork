using SunnyRewards.Helios.Common.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Task.Core.Domain.Dtos
{
    public class TaskRewardCollectionResponseDto:BaseResponseDto
    {
        public IList<TaskRewardDetailDto> TaskRewards { get; set; } = null!;
    }
}
