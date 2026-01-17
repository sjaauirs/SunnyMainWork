using SunnyRewards.Helios.Common.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Task.Core.Domain.Dtos
{
    public class FindTaskRewardResponseDto: BaseResponseDto
    {
        public FindTaskRewardResponseDto()
        {
            TaskRewardDetails = new List<TaskRewardDetailDto>();
        }
        public List<TaskRewardDetailDto> TaskRewardDetails { get; set; }
    }
}
