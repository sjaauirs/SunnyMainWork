using SunnyRewards.Helios.Common.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Task.Core.Domain.Dtos
{
    public class TaskRewardTypeResponseDto : BaseResponseDto
    {
        public TaskRewardTypeDto? TaskRewardType { get; set; }
    }
}
