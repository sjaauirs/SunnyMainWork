using SunnyRewards.Helios.Common.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Task.Core.Domain.Dtos
{
    public class TaskRewardsResponseDto : BaseResponseDto
    {
        public IList<TaskRewardDto>? TaskRewards { get; set; }
    }
}
