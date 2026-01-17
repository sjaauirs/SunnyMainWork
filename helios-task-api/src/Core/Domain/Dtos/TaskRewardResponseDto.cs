using SunnyRewards.Helios.Common.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Task.Core.Domain.Dtos
{
    public class TaskRewardResponseDto : BaseResponseDto
    {
        public TaskRewardDto? TaskReward { get; set; }
    }
}
