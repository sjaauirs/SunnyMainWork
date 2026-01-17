using SunnyRewards.Helios.Common.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Task.Core.Domain.Dtos
{
    public class TaskRewardTypesResponseDto : BaseResponseDto
    {
        public IList<TaskRewardTypeDto>? TaskRewardTypes { get; set; }
    }
}
