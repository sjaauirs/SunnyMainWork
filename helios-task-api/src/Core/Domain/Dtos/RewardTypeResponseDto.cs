using SunnyRewards.Helios.Common.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Task.Core.Domain.Dtos
{
    public class RewardTypeResponseDto : BaseResponseDto
    {
        public TaskRewardTypeDto? RewardTypeDto { get; set; }
    }
}
