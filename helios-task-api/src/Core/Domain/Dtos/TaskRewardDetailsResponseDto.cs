using SunnyRewards.Helios.Common.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Task.Core.Domain.Dtos
{
    public class TaskRewardDetailsResponseDto : BaseResponseDto
    {
        public IList<TaskRewardDetailsDto>? TaskRewardDetails { get; set; }
    }
}