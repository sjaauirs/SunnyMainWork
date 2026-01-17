using SunnyRewards.Helios.Common.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Task.Core.Domain.Dtos
{
    public class GetTasksAndTaskRewardsResponseDto : BaseResponseDto
    {
        public IList<TaskAndTaskRewardDto> taskAndTaskRewardDtos { get; set; } = new List<TaskAndTaskRewardDto>();

    }
}
